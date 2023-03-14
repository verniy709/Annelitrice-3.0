using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Annelitrice
{
	[StaticConstructorOnStartup]
	public static class HarmonyInit
	{
		//Pawn render patch
		public static Harmony harmonyInstance;
		static HarmonyInit()
		{
			harmonyInstance = new Harmony("Annelitrice.Mod");
			harmonyInstance.PatchAll();

			var drawHatWithHair2 = AccessTools.Method("HatDisplaySelection.Patch:DrawHatWithHair2");
			if (drawHatWithHair2 is null)
			{
				harmonyInstance.Patch(AccessTools.Method("PawnRenderer:RenderPawnInternal"),
					transpiler: new HarmonyMethod(AccessTools.Method(typeof(RenderPawnInternal_Patch), "RenderPawnInternal_Transpiler")));
			}
		}
		public static IEnumerable<CodeInstruction> EmptyTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
		{
			yield break;
		}
		public static IEnumerable<CodeInstruction> DrawHatWithHair2Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
		{
			var pawnField = AccessTools.Field(typeof(PawnRenderer), "pawn");
			var label = ilg.DefineLabel();
			var shouldOverride = AccessTools.Method(typeof(DrawHeadHair_Patch), "ShouldOverride");
			var changeVector = AccessTools.Method(typeof(RenderPawnInternal_Patch), "ChangeVector");
			var found = 0;

			var codes = instructions.ToList();
			for (var i = 0; i < codes.Count; i++)
			{
				var instr = codes[i];
				yield return instr;
				if (found < 2 && i > 3 && i < codes.Count - 1 && codes[i - 3].opcode == OpCodes.Ldc_R4 && codes[i - 3].OperandIs(0.02895753f)
					&& codes[i - 1].opcode == OpCodes.Stind_R4)
				{
					found++;
					if (found == 2)
					{
						codes[i + 1].labels.Add(label);
						yield return new CodeInstruction(OpCodes.Nop);

						yield return new CodeInstruction(OpCodes.Ldarg_0);
						yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
						yield return new CodeInstruction(OpCodes.Call, shouldOverride);
						yield return new CodeInstruction(OpCodes.Brfalse, label);

						yield return new CodeInstruction(OpCodes.Nop);

						yield return new CodeInstruction(OpCodes.Ldloc_2);
						yield return new CodeInstruction(OpCodes.Call, changeVector);
						yield return new CodeInstruction(OpCodes.Stloc_2);

						Log.Message("Anneli: Patched RenderPawnInternal");
					}
				}
			}
		}
	}

	public class RenderPawnInternal_Patch
	{
		[TweakValue("AN", -1f, 1f)] public static float test1 = 0.015f;
		public static IEnumerable<CodeInstruction> RenderPawnInternal_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
		{
			var pawnField = AccessTools.Field(typeof(PawnRenderer), "pawn");
			var label = ilg.DefineLabel();
			var shouldOverride = AccessTools.Method(typeof(DrawHeadHair_Patch), "ShouldOverride");
			var changeVector = AccessTools.Method(typeof(RenderPawnInternal_Patch), "ChangeVector");
			var found = 0;

			var codes = instructions.ToList();
			for (var i = 0; i < codes.Count; i++)
			{
				var instr = codes[i];
				yield return instr;
				if (found < 2 && i > 3 && i < codes.Count - 1 && codes[i - 3].opcode == OpCodes.Ldc_R4 && codes[i - 3].OperandIs(0.0231660213f) && codes[i - 1].opcode == OpCodes.Stind_R4)
				{
					found++;
					if (found == 2)
					{
						codes[i + 1].labels.Add(label);
						yield return new CodeInstruction(OpCodes.Nop);

						yield return new CodeInstruction(OpCodes.Ldarg_0);
						yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
						yield return new CodeInstruction(OpCodes.Call, shouldOverride);
						yield return new CodeInstruction(OpCodes.Brfalse, label);

						yield return new CodeInstruction(OpCodes.Nop);

						yield return new CodeInstruction(OpCodes.Ldloc_2);
						yield return new CodeInstruction(OpCodes.Call, changeVector);
						yield return new CodeInstruction(OpCodes.Stloc_2);

						Log.Message("Anneli: Patched RenderPawnInternal");
					}
				}
			}
		}

		public static Vector3 ChangeVector(Vector3 vector3)
		{
			vector3.y += test1;
			return vector3;
		}
	}

	[HarmonyPatch(typeof(PawnRenderer), "DrawHeadHair")]
	public class DrawHeadHair_Patch
	{
		[TweakValue("AN", -1f, 1f)] public static float test2 = 0.016f;
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
		{
			var pawnField = AccessTools.Field(typeof(PawnRenderer), "pawn");
			var label = ilg.DefineLabel();
			var shouldOverride = AccessTools.Method(typeof(DrawHeadHair_Patch), "ShouldOverride");
			var changeVector = AccessTools.Method(typeof(DrawHeadHair_Patch), "ChangeVector");
			var onHeadField = AccessTools.Field(typeof(PawnRenderer).GetNestedTypes(AccessTools.all)
				.First(c => c.Name.Contains("c__DisplayClass54_0")), "onHeadLoc");
			var found = false;
			var codes = instructions.ToList();

			for (var i = 0; i < codes.Count; i++)
			{
				var instr = codes[i];
				yield return instr;
				if (!found && i > 2 && i < codes.Count - 1 && codes[i - 2].opcode == OpCodes.Ldc_R4
					&& codes[i - 2].OperandIs(0.0289575271f) && codes[i].opcode == OpCodes.Stind_R4)
				{
					found = true;
					codes[i + 1].labels.Add(label);
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
					yield return new CodeInstruction(OpCodes.Call, shouldOverride);
					yield return new CodeInstruction(OpCodes.Brfalse, label);

					yield return new CodeInstruction(OpCodes.Nop);

					yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
					yield return new CodeInstruction(OpCodes.Ldloc_0);

					yield return new CodeInstruction(OpCodes.Ldfld, onHeadField);
					yield return new CodeInstruction(OpCodes.Call, changeVector);
					yield return new CodeInstruction(OpCodes.Stfld, onHeadField);
					Log.Message("Anneli: Patched DrawHeadHair");
				}
			}
		}

		public static bool ShouldOverride(Pawn pawn)
		{
			return pawn.def == AnnelitriceDefOf.Annelitrice;
		}
		public static Vector3 ChangeVector(Vector3 vector3)
		{
			vector3.y += test2;
			return vector3;
		}
	}

	[HarmonyPatch(typeof(PawnRenderer), "DrawPawnBody")]
	public class DrawPawnBody_Patch
	{
		[HarmonyPriority(Priority.Last)]
		private static void Postfix(Pawn ___pawn, PawnRenderer __instance, ref Vector3 rootLoc, ref float angle, ref Rot4 facing, ref RotDrawMode bodyDrawType, ref PawnRenderFlags flags, ref Mesh bodyMesh)
		{
			//if (bodyDrawType != RotDrawMode.Dessicated && ___pawn.RaceProps.Humanlike)
			//{
			//    Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
			//
			//    Vector3 vector = rootLoc;
			//    if (___pawn.TryGetCompEvolution(out var comp))
			//    {
			//        if (comp.CanShowLegs)
			//        {
			//            Material rightLeg = comp.RightLegGraphic.MatAt(___pawn.Rotation);
			//            GenDraw.DrawMeshNowOrLater(bodyMesh, vector, quat, rightLeg, flags.FlagSet(PawnRenderFlags.DrawNow));
			//
			//            Material leftLeg = comp.LeftLegGraphic.MatAt(___pawn.Rotation);
			//            GenDraw.DrawMeshNowOrLater(bodyMesh, vector, quat, leftLeg, flags.FlagSet(PawnRenderFlags.DrawNow));
			//        }
			//
			//        if (comp.CanShowFace)
			//        {
			//            Vector3 vector2 = rootLoc;
			//            if (facing != Rot4.North)
			//            {
			//                vector2.y += 0.0231660213f;
			//            }
			//            else
			//            {
			//                vector2.y += 3f / 148f;
			//            }
			//            var vector3 = quat * __instance.BaseHeadOffsetAt(facing);
			//            var headVector = vector2 + vector3;
			//            Material mouth = comp.MouthGraphic.MatAt(___pawn.Rotation);
			//            GenDraw.DrawMeshNowOrLater(MeshPool.humanlikeHeadSet.MeshAt(facing), headVector, quat, mouth, flags.FlagSet(PawnRenderFlags.DrawNow));
			//
			//            headVector.y += 0.005f;
			//            Material eyes = comp.EyesGraphic.MatAt(___pawn.Rotation);
			//            GenDraw.DrawMeshNowOrLater(MeshPool.humanlikeHeadSet.MeshAt(facing), headVector, quat, eyes, flags.FlagSet(PawnRenderFlags.DrawNow));
			//
			//            Material eyeBrows = comp.EyeBrowsGraphic.MatAt(___pawn.Rotation);
			//            GenDraw.DrawMeshNowOrLater(MeshPool.humanlikeHeadSet.MeshAt(facing), headVector, quat, eyeBrows, flags.FlagSet(PawnRenderFlags.DrawNow));
			//        }
			//    }
			//}
		}
	}

	//spawn eggs after body destruction
	[HarmonyPatch(typeof(Pawn), "Destroy")]
	public static class Pawn_Destroy_Patch
	{
		public static void Prefix(Pawn __instance)
		{
			if (__instance.Corpse is null)
			{
				__instance.TrySpawnegg();
			}
		}
	}
	[HarmonyPatch(typeof(Corpse), "Destroy")]
	public static class Corpse_Destroy_Patch
	{
		public static void Prefix(Corpse __instance)
		{
			__instance.InnerPawn.TrySpawnegg();
		}
	}

	[HarmonyPatch(typeof(ITab_Pawn_Character), "PawnToShowInfoAbout", MethodType.Getter)]
	public static class ITab_Pawn_Character_PawnToShowInfoAbout_Patch
	{
		public static bool Prefix(ref Pawn __result)
		{
			if (Find.Selector.SingleSelectedThing is egg egg)
			{
				__result = egg.savedPawn;
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(CompTargetEffect_Resurrect), "DoEffectOn")]
	public static class CompTargetEffect_Resurrect_DoEffectOn_Patch
	{
		public static bool Prefix(CompTargetEffect_Resurrect __instance, Pawn user, Thing target)
		{
			if (target is egg egg)
			{
				if (user.IsColonistPlayerControlled && user.CanReserveAndReach(target, PathEndMode.Touch, Danger.Deadly))
				{
					var job = JobMaker.MakeJob(AnnelitriceDefOf.Anneli_ResurrectUsingEgg, egg, __instance.parent);
					job.count = 1;
					user.jobs.TryTakeOrderedJob(job, JobTag.Misc);
					return false;
				}
			}
			return true;
		}
	}

	//[HarmonyPatch(typeof(CompRefuelable), "ConsumptionRatePerTick", MethodType.Getter)]
	//public static class CompRefuelable_ConsumptionRatePerTick_Patch
	//{
	//    public static bool Prefix(CompRefuelable __instance, ref float __result)
	//    {
	//        if (__instance.parent is eggIncubator incubator)
	//        {
	//            __result = incubator.GetFuelConsumptionRate();

	//            return false;
	//        }
	//        return true;
	//    }
	//}
	//[HarmonyPatch(typeof(CompRefuelable), "CompInspectStringExtra")]
	//public static class CompRefuelable_CompInspectStringExtra_Patch
	//{
	//    public static bool Prefix(CompRefuelable __instance, ref string __result)
	//    {
	//        if (__instance.parent is eggIncubator incubator)
	//        {
	//            __result = CompInspectStringExtra(incubator, __instance);
	//            return false;
	//        }
	//        return true;
	//    }
	//    public static string CompInspectStringExtra(eggIncubator incubator, CompRefuelable instance)
	//    {
	//        if (instance.Props.fuelIsMortarBarrel && Find.Storyteller.difficulty.classicMortars)
	//        {
	//            return string.Empty;
	//        }
	//        string text = instance.Props.FuelLabel + ": " + instance.Fuel.ToStringDecimalIfSmall() + " / " + instance.Props.fuelCapacity.ToStringDecimalIfSmall();
	//        if (!instance.Props.consumeFuelOnlyWhenUsed && instance.HasFuel)
	//        {
	//            int numTicks = (int)((instance.Fuel / incubator.GetFuelConsumptionRate()));// * 60000f);
	//            text = text + " (" + numTicks.ToStringTicksToPeriod() + ")";
	//        }
	//        if (!instance.HasFuel && !instance.Props.outOfFuelMessage.NullOrEmpty())
	//        {
	//            text += $"\n{instance.Props.outOfFuelMessage} ({instance.GetFuelCountToFullyRefuel()}x {instance.Props.fuelFilter.AnyAllowedDef.label})";
	//        }
	//        if (instance.Props.targetFuelLevelConfigurable)
	//        {
	//            text += "\n" + "ConfiguredTargetFuelLevel".Translate(instance.TargetFuelLevel.ToStringDecimalIfSmall());
	//        }
	//        return text;
	//    }
	//}

	[HarmonyPatch(typeof(Pawn), "GetGizmos")]
	public class Pawn_GetGizmos_Patch
	{
		private static int lastActiveFrame = -1;
		public static bool DoCastRadius(IntVec3 center)
		{
			if (lastActiveFrame != Time.frameCount)
			{
				lastActiveFrame = Time.frameCount;
				GenDraw.DrawRadiusRing(center, 51.9f);
			}
			return true;
		}
		//protected static TargetingParameters GetHumanCorpseTargetParameters(Pawn caster)
		//{
		//    return new TargetingParameters
		//    {
		//        canTargetAnimals = false,
		//        canTargetHumans = false,
		//        canTargetMechs = false,
		//        mapObjectTargetsMustBeAutoAttackable = false,
		//        canTargetPawns = false,
		//        canTargetSelf = false,
		//        canTargetBuildings = false,
		//        canTargetItems = true,
		//        validator = (TargetInfo targ) => DoCastRadius(caster.Position) && targ.Thing is Corpse corpse && corpse.InnerPawn.RaceProps.Humanlike && corpse.Position.DistanceTo(caster.Position) <= 2.9f
		//    };
		//}
		protected static TargetingParameters GetHumanoidTargetParameters(Pawn caster)
		{
			return new TargetingParameters
			{
				canTargetAnimals = false,
				canTargetHumans = true,
				canTargetPawns = true,
				canTargetSelf = false,
				validator = (TargetInfo targ) => DoCastRadius(caster.Position) && targ.Thing is Pawn pawn && pawn.RaceProps.Humanlike && pawn.Position.DistanceTo(caster.Position) <= 2.9f
			};
		}
		public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
		{
			foreach (var g in __result)
			{
				yield return g;
			}
			var pawn = __instance;
			if (pawn.Faction != Faction.OfPlayer || pawn.InMentalState || !pawn.TryGetCompEvolution(out var comp))
			{
				yield break;
			}

			//Command_Action assimilate = new Command_Action()
			//{
			//    defaultLabel = "Anneli.Assimilate".Translate(),
			//    defaultDesc = "Anneli.AssimilateDesc".Translate(),
			//    icon = ContentFinder<Texture2D>.Get("Anneli_Skill/Assimilate"),
			//    action = delegate
			//    {
			//        Find.Targeter.BeginTargeting(GetHumanCorpseTargetParameters(pawn), delegate (LocalTargetInfo t)
			//        {
			//            Job job = JobMaker.MakeJob(AnnelitriceDefOf.Anneli_AssimilateCorpse, t.Thing);
			//            pawn.jobs.TryTakeOrderedJob(job);
			//        }, pawn);
			//    },
			//    onHover = () => DoCastRadius(__instance.Position),
			//};
			//yield return assimilate;

			var infest = new Command_Action()
			{
				defaultLabel = "Anneli.Infest".Translate(),
				defaultDesc = "Anneli.InfestDesc".Translate(),
				icon = ContentFinder<Texture2D>.Get("Anneli_UI/Anneli_Infect"),
				action = delegate
				{
					Find.Targeter.BeginTargeting(GetHumanoidTargetParameters(pawn), delegate (LocalTargetInfo t)
					{
						var job = JobMaker.MakeJob(AnnelitriceDefOf.Anneli_InfestHuman, t.Thing);
						pawn.jobs.TryTakeOrderedJob(job);
					}, pawn);
				},
				onHover = () => DoCastRadius(__instance.Position),
			};
			//if (pawn.needs.food.CurLevelPercentage < 0.1f)
			//{
			//    infest.Disable("Anneli.CannotInfestTooLowFoodLevel".Translate());
			//}
			yield return infest;
		}
	}

	//[HarmonyPatch(typeof(BodyPartDef), "GetMaxHealth")]
	//public class GetMaxHealth_Patch
	//{
	//    [HarmonyPriority(Priority.Last)]
	//    private static void Postfix(BodyPartDef __instance, Pawn pawn, ref float __result)
	//    {
	//        if (pawn.TryGetCompEvolution(out var comp))
	//        {
	//            if (comp.blueBilePoints > 0)
	//            {
	//                __result *= 1 + (comp.blueBilePoints * 0.125f);
	//            }
	//        }
	//    }
	//}

	//[HarmonyPatch(typeof(StatExtension), nameof(StatExtension.GetStatValue))]
	//public static class GetStatValue_Patch
	//{
	//    [HarmonyPriority(Priority.Last)]
	//    private static void Postfix(Thing thing, StatDef stat, bool applyPostProcess, ref float __result)
	//    {
	//        if (thing is Pawn pawn && pawn.TryGetCompEvolution(out var comp))
	//        {
	//            if (stat == StatDefOf.MoveSpeed)
	//            {
	//                if (comp.greenBilePoints > 0)
	//                {
	//                    __result *= 1 + (comp.greenBilePoints * 0.05f);
	//                }
	//            }
	//            else if (stat == StatDefOf.PsychicSensitivity)
	//            {
	//                if (comp.blueBilePoints > 0)
	//                {
	//                    __result *= 1 + (comp.blueBilePoints * 0.05f);
	//                }
	//            }
	//        }
	//    }
	//}

	//[HarmonyPatch(typeof(PawnCapacityWorker_Consciousness), "CalculateCapacityLevel")]
	//public class CalculateCapacityLevel_Patch
	//{
	//    [HarmonyPriority(Priority.Last)]
	//    private static void Postfix(ref float __result, HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
	//    {
	//        if (diffSet.pawn.TryGetCompEvolution(out var comp))
	//        {
	//            if (comp.blueBilePoints > 0)
	//            {
	//                __result *= 1 + (comp.blueBilePoints * 0.035f);
	//            }
	//        }
	//    }
	//}

	//race specific flirt chance increase
	[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "RandomSelectionWeight")]
	public class RandomSelectionWeight_Patch
	{
		[HarmonyPriority(Priority.First)]
		private static bool Prefix(ref float __result, Pawn initiator, Pawn recipient)
		{
			if (initiator.def == AnnelitriceDefOf.Annelitrice)
			{
				__result = RandomSelectionWeight(initiator, recipient);
				return false;
			}
			return true;
		}

		public static float RandomSelectionWeight(Pawn initiator, Pawn recipient)
		{
			if (TutorSystem.TutorialMode)
			{
				return 0f;
			}
			if (LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient))
			{
				return 0f;
			}
			var num = initiator.relations.SecondaryRomanceChanceFactor(recipient);
			if (num < 0.15f)
			{
				return 0f;
			}
			var num2 = 5f;
			var num3 = initiator.relations.OpinionOf(recipient);
			if (num3 < num2)
			{
				return 0f;
			}
			if (recipient.relations.OpinionOf(initiator) < num2)
			{
				return 0f;
			}
			var num4 = 1f;
			if (!new HistoryEvent(initiator.GetHistoryEventForLoveRelationCountPlusOne(), initiator.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo())
			{
				var pawn = LovePartnerRelationUtility.ExistingMostLikedLovePartner(initiator, allowDead: false);
				if (pawn != null)
				{
					float value = initiator.relations.OpinionOf(pawn);
					num4 = Mathf.InverseLerp(50f, -50f, value);
				}
			}
			var num5 = 1f;
			var num6 = Mathf.InverseLerp(0.15f, 1f, num);
			var num7 = Mathf.InverseLerp(num2, 100f, num3);
			var num8 = (initiator.gender == recipient.gender) ? ((!initiator.story.traits.HasTrait(TraitDefOf.Gay) || !recipient.story.traits.HasTrait(TraitDefOf.Gay)) ? 0.15f : 1f) : ((initiator.story.traits.HasTrait(TraitDefOf.Gay) || recipient.story.traits.HasTrait(TraitDefOf.Gay)) ? 0.15f : 1f);
			return 1.15f * num5 * num6 * num7 * num4 * num8;
		}
	}

	//weapon show position z offset for the race
	[HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming")]
	public static class DrawEquipmentAiming_Patch
	{
		public static float zOffset = 0.195f;
		//public static float xOffset = 0.08f;
		//public static float angleNew = 340f;
		//public static float drawSizeFactor = 0.85f;
		public static void Prefix(PawnRenderer __instance, Pawn ___pawn, Thing eq, ref Vector3 drawLoc, float aimAngle)
		{
			if (___pawn.TryGetCompEvolution(out _))
			{
				drawLoc.z += zOffset;
			}
		}
	}

	//RenderAsPack patch for race apparel layers
	[HarmonyPatch(typeof(PawnRenderer), "RenderAsPack")]
	public static class RenderAsPack_Patch
	{
		public static HashSet<string> supportedLayers = new HashSet<string>
		{
			"Anneli_UpperShell_Tactical",
			"Anneli_Weapon"
		};
		public static void Postfix(PawnRenderer __instance, Pawn ___pawn, Apparel apparel, ref bool __result)
		{
			if (!__result)
			{
				if (apparel.def.apparel.wornGraphicData != null && apparel.def.apparel.wornGraphicData.renderUtilityAsPack)
				{
					__result = apparel.def.apparel.layers.Any(x => supportedLayers.Contains(x.defName));
				}
			}
		}
	}

	//Apparel textures switch patch
	[HarmonyPatch(typeof(PawnGraphicSet))]
	[HarmonyPatch("ResolveApparelGraphics")]
	public class Annelitrice_ApparelHarmonyPatch
	{
		[HarmonyAfter(new string[] { "rimworld.erdelf.alien_race.main" })]
		[HarmonyPriority(390)]
		private static void Postfix(PawnGraphicSet __instance)
		{
			var count = __instance.apparelGraphics.Count();
			for (var i = 0; i < count; i++)
			{
				var comp = __instance.apparelGraphics[i].sourceApparel.GetComp<CompApparelSecialTex>();
				if (comp == null)
				{
					continue;
				}
				var apparel = __instance.apparelGraphics[i].sourceApparel;
				var apparelDef = apparel.def;
				var list = comp.list;
				var data = list.FirstOrDefault(x => __instance.apparelGraphics.Exists(y => y.sourceApparel.def == x.apparelDef));
				if (data != null)
				{
					var shader = data.shader;
					if (shader == null)
					{
						shader = ShaderDatabase.Cutout;
						if (apparelDef.apparel.useWornGraphicMask)
						{
							shader = ShaderDatabase.CutoutComplex;
						}
					}
					var path = data.path + "_" + __instance.pawn.story.bodyType.defName;
					var graphic = GraphicDatabase.Get<Graphic_Multi>(path, shader, apparelDef.graphicData.drawSize, apparel.DrawColor);
					__instance.apparelGraphics[i] = new ApparelGraphicRecord(graphic, apparel);
				}
			}
		}
	}


	//底发渲染Patch
	[HarmonyPatch(typeof(PawnRenderer))]
	[HarmonyPatch("DrawHeadHair")]
	public class Annelitrice_HairPatch
	{
		private static bool Prefix(PawnRenderer __instance, Vector3 rootLoc, float angle, Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags, ref Pawn ___pawn)
		{
			var def = __instance.graphics.pawn.story.hairDef;
			var baseHair = DefDatabase<BaseHairDef>.AllDefsListForReading.FirstOrDefault(x => x.hairDef == def);
			if (baseHair == null || flags.FlagSet(PawnRenderFlags.HeadStump) || bodyDrawType == RotDrawMode.Dessicated)
			{
				return true;
			}

			var headFacing = bodyFacing;
			var headOffset = Vector3.zero;
			//画正常头发的底发
			if (__instance.graphics?.headGraphic != null)
			{
				var apparelGraphics = __instance.graphics.apparelGraphics;
				var hasSurFaceApparel = !baseHair.apparelList.NullOrEmpty() && apparelGraphics.Exists(x => baseHair.apparelList.Contains(x.sourceApparel.def));
				if (!hasSurFaceApparel && (!flags.FlagSet(PawnRenderFlags.Portrait) || !Prefs.HatsOnlyOnMap))
				{
					foreach (var apparel in apparelGraphics)
					{
						if (apparel.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead || apparel.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.EyeCover)
						{
							if (!apparel.sourceApparel.def.apparel.hatRenderedFrontOfFace && !apparel.sourceApparel.def.apparel.forceRenderUnderHair)
							{
								hasSurFaceApparel = true;
							}
						}
					}
				}
				if (!hasSurFaceApparel)
				{
					var quaternion = Quaternion.AngleAxis(angle, Vector3.up);
					headOffset = quaternion * __instance.BaseHeadOffsetAt(headFacing);
					var mesh3 = __instance.graphics.HairMeshSet.MeshAt(headFacing);

					var baseHairGraphic = GraphicDatabase.Get<Graphic_Multi>(baseHair.baseTexPath, ShaderDatabase.Transparent, Vector2.one, __instance.graphics.pawn.story.HairColor);
					var baseMat = baseHairGraphic.MatAt(headFacing, null);
					if (!flags.FlagSet(PawnRenderFlags.Portrait) && __instance.graphics.pawn.IsInvisible())
					{
						baseMat = InvisibilityMatPool.GetInvisibleMat(baseMat);
					}
					if (!flags.FlagSet(PawnRenderFlags.Cache))
					{
						baseMat = __instance.graphics.flasher.GetDamagedMat(baseMat);
					}

					var loc2 = rootLoc + headOffset;
					loc2.y += baseHair.yOffset;
					GenDraw.DrawMeshNowOrLater(mesh3, loc2, quaternion, baseMat, flags.FlagSet(PawnRenderFlags.DrawNow));

				}
			}
			return true;
		}
	}
}
