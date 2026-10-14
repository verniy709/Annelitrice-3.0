using HarmonyLib;
using Verse;
using DubsBadHygiene;
using NareisLib;
using Verse.AI;

namespace Annelitrice_DBHHarmonyPatches
{
    // Prevents NareisLib pawn rendering in washing jobs
    [HarmonyPatch(typeof(PawnRenderNodeWorker_TextureLevels),
                  nameof(PawnRenderNodeWorker_TextureLevels.CanDrawNow))]
    public static class Patch_PawnRenderNodeWorker_TextureLevels_CanDrawNow
    {
        public static bool Prefix(PawnRenderNode node, PawnDrawParms parms, ref bool __result)
        {
            Pawn pawn = parms.pawn;
            if (pawn == null || pawn.def.defName != "Annelitrice")
                return true;

            Job curJob = pawn.CurJob;
            WashingJobDef wdef = ((curJob != null) ? curJob.def : null) as WashingJobDef;
            bool isWashing = wdef != null &&
                             pawn.health?.hediffSet?.HasHediff(DubDef.Washing, false) == true;

            if (!isWashing)
                return true;

            TextureLevelsToNode tlNode = node as TextureLevelsToNode;
            if (tlNode == null)
                return true;

            TextureLevels tex = tlNode.textureLevels;
            TextureRenderLayer layer = tex?.renderLayer ?? TextureRenderLayer.None;

            if (!HiddenLayer(layer))
                return true;

            __result = false;
            return false;
        }

        private static bool HiddenLayer(TextureRenderLayer layer)
        {
            switch (layer)
            {
                case TextureRenderLayer.Apparel:
                case TextureRenderLayer.BottomShell:
                case TextureRenderLayer.FrontShell:
                case TextureRenderLayer.Hat:
                case TextureRenderLayer.FaceMask:
                case TextureRenderLayer.HeadMask:
                    return true;

                default:
                    return false;
            }
        }
    }
}
