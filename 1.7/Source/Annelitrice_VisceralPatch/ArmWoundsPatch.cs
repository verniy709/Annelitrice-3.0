using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Annelitrice;
using HarmonyLib;
using NareisLib;
using UnityEngine;
using Verse;
using VisceralAnatomy;

namespace Annelitrice.VisceralPatch
{
    [StaticConstructorOnStartup]
    internal static class Bootstrap
    {
        static Bootstrap()
        {
            new Harmony("verniy709.annelitrice.visceral.armwounds").PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    internal static class ArmWounds
    {
        private const int SilhouetteSize = 192;
        private static readonly MethodInfo Read = AccessTools.Method(
            typeof(VisceralChunks), "Read", new[] { typeof(Texture), typeof(int), typeof(int) });
        private static readonly Dictionary<long, Texture2D> Silhouettes = new Dictionary<long, Texture2D>();
        private static readonly Dictionary<ulong, Texture2D> Shapes = new Dictionary<ulong, Texture2D>();
        private sealed class PartState
        {
            internal readonly HashSet<BodyPartRecord> Missing = new HashSet<BodyPartRecord>();
        }
        private static readonly ConditionalWeakTable<Pawn, PartState> PartStates =
            new ConditionalWeakTable<Pawn, PartState>();

        internal static bool IsArm(PawnRenderNode node)
        {
            TextureLevelsToNode levelNode = node as TextureLevelsToNode;
            TextureLevels level = levelNode?.textureLevels;
            string name = level?.textureLevelsName;
            return level != null && levelNode?.tree?.pawn?.def == AnnelitriceDefOf.Annelitrice &&
                (level.isSleeve || name == "Anneli_Arm_Generic_Left" || name == "Anneli_Arm_Generic_Right");
        }

        private static bool IsRight(PawnRenderNode node)
        {
            TextureLevels level = (node as TextureLevelsToNode)?.textureLevels;
            return level != null && (level.isSleeve ? level.flipped :
                level.textureLevelsName == "Anneli_Arm_Generic_Right");
        }

        internal static bool IsSleeve(PawnRenderNode node)
        {
            return IsArm(node) && (node as TextureLevelsToNode).textureLevels.isSleeve;
        }

        // Visceral encodes the left and right shoulder/arm cuts in separate
        // nibbles. Both Nareis arm nodes use one source image, with the right
        // arm mirrored by its mesh. Move only this node's cuts to the source
        // image's side and let Visceral paint its normal progressive cut mask.
        private const string ArmSilhouettePrefix = "AnneliVisceralArmSilhouette_";

        internal static int MissingKeyForArm(int key, PawnRenderNode node, Texture2D body)
        {
            if (!IsArm(node) || !IsArmSilhouette(body))
                return key;

            int from = IsRight(node) ? 3 : 0;
            int shoulder = (key >> (from * 4)) & 15;
            int arm = (key >> ((from + 1) * 4)) & 15;
            return shoulder | (arm << 4);
        }

        private static bool IsArmSilhouette(Texture2D body)
        {
            return body != null && body.name != null &&
                body.name.StartsWith(ArmSilhouettePrefix, StringComparison.Ordinal);
        }

        // Visceral rejects queued and completed masks when their stored key no
        // longer matches the pawn's key. Arm requests store the mapped key, so
        // compare against the same side of the current key at both checkpoints.
        internal static int CurrentKeyForRequest(int key, Texture2D body)
        {
            if (!IsArmSilhouette(body))
                return key;

            bool right = body.name.StartsWith(ArmSilhouettePrefix + "R_", StringComparison.Ordinal);
            int from = right ? 3 : 0;
            return ((key >> (from * 4)) & 15) | (((key >> ((from + 1) * 4)) & 15) << 4);
        }

        // Visceral's completed materials need a fresh draw request, but
        // rebuilding the tree also recreates NareisLib's animated arm nodes.
        internal static void RefreshMaterials(PawnRenderer renderer)
        {
            if (renderer == null)
                return;
            PawnRenderTree tree = renderer.renderTree;
            Pawn pawn = tree?.pawn;
            if (pawn?.def != AnnelitriceDefOf.Annelitrice)
            {
                renderer.SetAllGraphicsDirty();
                return;
            }

            // A cut neck or lost eye can add a dynamic Visceral render node.
            // Those nodes need one tree rebuild when the set of missing parts
            // changes; wound severity alone only needs material recaching.
            PartState parts = PartStates.GetValue(pawn, _ => new PartState());
            HashSet<BodyPartRecord> missingNow = new HashSet<BodyPartRecord>();
            List<Hediff> hediffs = pawn.health?.hediffSet?.hediffs;
            if (hediffs != null)
                foreach (Hediff hediff in hediffs)
                    if (hediff is Hediff_MissingPart absent && absent.Part != null)
                        missingNow.Add(absent.Part);
            bool structureChanged;
            lock (parts)
            {
                structureChanged = !parts.Missing.SetEquals(missingNow);
                if (structureChanged)
                {
                    parts.Missing.Clear();
                    parts.Missing.UnionWith(missingNow);
                }
            }
            if (structureChanged)
            {
                renderer.SetAllGraphicsDirty();
                return;
            }

            if (tree.rootNode != null)
                tree.rootNode.requestRecache = true;
            RimWorld.PortraitsCache.SetDirty(pawn);
            GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
        }

        // Visceral's own wound painter works in a 192x192 silhouette. Annelitrice's
        // arm images share the body's canvas, so their alpha can be added to it.
        internal static Texture2D SelectSilhouette(PawnRenderNode node, Texture2D source,
            Texture2D body, PawnDrawParms parms)
        {
            if (!IsArm(node) || source == null || body == null || Read == null)
                return body;

            // RimWorld may prepare several render nodes on worker threads at once.
            // Both dictionaries and the first creation of a shared silhouette must
            // be protected as one operation.
            lock (Silhouettes)
            {
            // Each side needs a separate mask cache entry because the wound sides
            // in MarksForArm differ even when both nodes share the same image.
            long key = ((long)body.GetInstanceID() << 32) ^ (uint)source.GetInstanceID() ^
                ((long)parms.facing.AsInt << 48);
            if (IsRight(node))
                key ^= unchecked((long)0x9E3779B97F4A7C15UL);
            if (Silhouettes.TryGetValue(key, out Texture2D cached) && cached != null)
                return cached;

            try
            {
                Color32[] bodyPixels = (Color32[])Read.Invoke(null, new object[] { body, SilhouetteSize, SilhouetteSize });
                Color32[] armPixels = (Color32[])Read.Invoke(null, new object[] { source, SilhouetteSize, SilhouetteSize });
                if (bodyPixels == null || armPixels == null ||
                    bodyPixels.Length != SilhouetteSize * SilhouetteSize || armPixels.Length != bodyPixels.Length)
                    return source;

                Color32[] union = new Color32[bodyPixels.Length];
                for (int i = 0; i < union.Length; i++)
                {
                    byte alpha = Math.Max(bodyPixels[i].a, armPixels[i].a);
                    union[i] = new Color32(255, 255, 255, alpha);
                }

                // The shoulder mark sits just inside the torso outline. Extend
                // only the synthetic silhouette's outer shoulder rows; the
                // material itself remains clipped by the arm or sleeve art.
                bool south = parms.facing == Rot4.South;
                bool north = parms.facing == Rot4.North;
                if (south || north)
                {
                    for (int y = 132; y <= 145; y++)
                    {
                        int edge = south ? -1 : SilhouetteSize;
                        int row = y * SilhouetteSize;
                        for (int x = 0; x < SilhouetteSize; x++)
                        {
                            if (armPixels[row + x].a <= 100)
                                continue;
                            edge = south ? Math.Max(edge, x) : Math.Min(edge, x);
                        }
                        if (edge < 0 || edge >= SilhouetteSize)
                            continue;
                        for (int dx = 1; dx <= 8; dx++)
                        {
                            int x = edge + (south ? dx : -dx);
                            if (x >= 0 && x < SilhouetteSize)
                                union[row + x].a = 255;
                        }
                    }
                }

                ulong shapeHash = 14695981039346656037UL ^ (IsRight(node) ? 1UL : 0UL);
                for (int i = 0; i < union.Length; i++)
                    shapeHash = unchecked((shapeHash ^ union[i].a) * 1099511628211UL);

                // Action textures often share an outline. Reuse the same
                // silhouette object so Visceral's own silhouette cache can too.
                if (Shapes.TryGetValue(shapeHash, out Texture2D matching) && matching != null)
                {
                    Silhouettes.Add(key, matching);
                    return matching;
                }

                Texture2D silhouette = new Texture2D(SilhouetteSize, SilhouetteSize, TextureFormat.RGBA32, false);
                silhouette.name = ArmSilhouettePrefix + (IsRight(node) ? "R_" : "L_") + key;
                silhouette.SetPixels32(union);
                silhouette.Apply(false, true);
                Shapes.Add(shapeHash, silhouette);
                Silhouettes.Add(key, silhouette);
                return silhouette;
            }
            catch (Exception ex)
            {
                Log.ErrorOnce("[Annelitrice/Visceral] Could not prepare arm silhouette: " + ex,
                    41158031);
                return source;
            }
            }
        }

        // Nareis uses an arm image on the right of the canvas when facing south,
        // and on the left when facing north. The other arm mirrors that image.
        // Place limb marks on the image side before the mesh is mirrored.
        internal static VisceralChunks.WoundMark[] MarksForArm(VisceralChunks.WoundMark[] marks,
            PawnRenderNode node, PawnDrawParms parms)
        {
            if (marks == null)
                return marks;
            if (IsArm(node))
            {
                bool right = IsRight(node);
                List<VisceralChunks.WoundMark> selected = new List<VisceralChunks.WoundMark>(marks.Length);
                for (int i = 0; i < marks.Length; i++)
                {
                    VisceralChunks.WoundMark mark = marks[i];
                    bool limb = mark.region == VisceralChunks.WoundRegion.Shoulder ||
                        mark.region == VisceralChunks.WoundRegion.Arm ||
                        mark.region == VisceralChunks.WoundRegion.Hand;
                    if (limb && mark.side < 2 && mark.side != (right ? 1 : 0))
                        continue;

                    if (limb && parms.facing == Rot4.South)
                        mark.side = 0;
                    else if (limb && parms.facing == Rot4.North)
                        mark.side = 1;
                    else if (right && mark.side < 2)
                        mark.side = (byte)(1 - mark.side);
                    selected.Add(mark);
                }
                return selected.ToArray();
            }

            return marks;
        }
    }

    [HarmonyPatch(typeof(VisceralChunks), "IsBodyLayer")]
    internal static class RecognizeArmAsBodyLayer
    {
        private static void Postfix(PawnRenderNode __0, ref bool __result)
        {
            if (!__result && ArmWounds.IsArm(__0))
                __result = true;
        }
    }

    [HarmonyPatch(typeof(VisceralChunks), "Variant")]
    internal static class UseArmSilhouette
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);
            MethodInfo select = AccessTools.Method(typeof(ArmWounds), nameof(ArmWounds.SelectSilhouette));
            MethodInfo marks = AccessTools.Method(typeof(ArmWounds), nameof(ArmWounds.MarksForArm));
            MethodInfo missingKey = AccessTools.Method(typeof(ArmWounds), nameof(ArmWounds.MissingKeyForArm));
            int keyAt = -1;
            for (int i = 0; i < code.Count - 1; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && code[i + 1].opcode == OpCodes.Stloc_3 &&
                    ((FieldInfo)code[i].operand).Name == "key" &&
                    ((FieldInfo)code[i].operand).DeclaringType?.Name == "PawnChunks")
                {
                    keyAt = i + 1;
                    break;
                }
            }
            if (keyAt < 0)
                throw new InvalidOperationException("Visceral Variant missing-part key was not found");
            code.InsertRange(keyAt, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Call, missingKey)
            });

            // There are two checks of local 2 (the body silhouette texture).
            // Insert at the second one, where both the normal and fallback paths meet.
            int checks = 0;
            int silhouetteAt = -1;
            for (int i = 0; i < code.Count - 2; i++)
            {
                if (code[i].opcode == OpCodes.Ldloc_2 && code[i + 1].opcode == OpCodes.Ldnull &&
                    code[i + 2].opcode == OpCodes.Call &&
                    ((MethodInfo)code[i + 2].operand).Name == "op_Equality")
                {
                    if (++checks == 2)
                    {
                        silhouetteAt = i;
                        break;
                    }
                }
            }
            if (silhouetteAt < 0)
                throw new InvalidOperationException("Visceral Variant silhouette join was not found");

            CodeInstruction first = new CodeInstruction(OpCodes.Ldarg_1);
            first.labels.AddRange(code[silhouetteAt].labels);
            code[silhouetteAt].labels.Clear();
            first.blocks.AddRange(code[silhouetteAt].blocks);
            code[silhouetteAt].blocks.Clear();
            code.InsertRange(silhouetteAt, new[]
            {
                first,
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Call, select),
                new CodeInstruction(OpCodes.Stloc_2)
            });

            int marksAt = -1;
            for (int i = 0; i < code.Count - 1; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && code[i + 1].opcode == OpCodes.Stfld &&
                    ((FieldInfo)code[i + 1].operand).Name == "marks" &&
                    ((FieldInfo)code[i + 1].operand).DeclaringType?.Name == "Request")
                {
                    marksAt = i + 1;
                    break;
                }
            }
            if (marksAt < 0)
                throw new InvalidOperationException("Visceral Variant wound request was not found");

            code.InsertRange(marksAt, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Call, marks)
            });
            return code;
        }
    }

    [HarmonyPatch]
    internal static class KeepArmMaskRequestsCurrent
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(VisceralChunks), "ProcessNext");
            yield return AccessTools.Method(typeof(VisceralChunks), "Stale");
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            MethodBase original)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);
            MethodInfo map = AccessTools.Method(typeof(ArmWounds), nameof(ArmWounds.CurrentKeyForRequest));
            int at = -1;
            FieldInfo bodyField = null;
            for (int i = 0; i < code.Count - 2; i++)
            {
                if (code[i].opcode != OpCodes.Ldfld || code[i + 2].opcode != OpCodes.Ldfld ||
                    !(code[i].operand is FieldInfo current) ||
                    !(code[i + 2].operand is FieldInfo requested) ||
                    current.Name != "key" || current.DeclaringType?.Name != "PawnChunks" ||
                    requested.Name != "key" || requested.DeclaringType?.Name != "Request")
                    continue;
                at = i + 1;
                bodyField = AccessTools.Field(requested.DeclaringType, "body");
                break;
            }
            if (at < 0 || bodyField == null)
                throw new InvalidOperationException("Visceral " + original.Name +
                    " queued-mask state comparison was not found");

            CodeInstruction loadRequest = new CodeInstruction(code[at].opcode, code[at].operand);
            loadRequest.labels.AddRange(code[at].labels);
            code[at].labels.Clear();
            loadRequest.blocks.AddRange(code[at].blocks);
            code[at].blocks.Clear();
            code.InsertRange(at, new[]
            {
                loadRequest,
                new CodeInstruction(OpCodes.Ldfld, bodyField),
                new CodeInstruction(OpCodes.Call, map)
            });
            return code;
        }
    }

    [HarmonyPatch(typeof(VisceralChunks), "TornCode")]
    internal static class TreatSleeveAsCloth
    {
        private static void Postfix(PawnRenderNode __0, ref int __result)
        {
            if (ArmWounds.IsSleeve(__0) && __0.apparel == null)
                __result = VisceralChunks.TornOn ? 8189 : 8193;
        }
    }

    [HarmonyPatch]
    internal static class PreserveArmAnimation
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (string name in new[] { "Early", "FrameFull", "BreastOvalsFor" })
            {
                MethodInfo method = AccessTools.Method(typeof(VisceralChunks), name);
                if (method == null)
                    throw new MissingMethodException(typeof(VisceralChunks).FullName, name);
                yield return method;
            }
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            MethodBase original)
        {
            MethodInfo fullDirty = AccessTools.Method(typeof(PawnRenderer),
                nameof(PawnRenderer.SetAllGraphicsDirty));
            MethodInfo refresh = AccessTools.Method(typeof(ArmWounds),
                nameof(ArmWounds.RefreshMaterials));
            int replaced = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Call && Equals(instruction.operand, fullDirty))
                {
                    instruction.operand = refresh;
                    replaced++;
                }
                yield return instruction;
            }
            if (replaced != 1)
                throw new InvalidOperationException("Visceral " + original.Name +
                    " graphic refresh changed; expected one renderer reset, found " + replaced);
        }
    }
}
