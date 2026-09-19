using HarmonyLib;
using RimWorld;
using Verse;
using Annelitrice;
using rjw;
using rjw.RenderNodeWorkers;

namespace Annelitrice.RJWHarmonyPatches
{
    [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_DrawNude), nameof(PawnRenderNodeWorker_Apparel_DrawNude.CanDrawNowSub))]
    public static class Patch_CompRJW
    {
        public static bool Prefix(PawnRenderNode node, PawnDrawParms parms, ref bool __result)
        {
            if (parms.pawn != null && parms.pawn?.def == AnnelitriceDefOf.Annelitrice)
            {
                __result = true;
                return false;      
            }

            return true;
        }
    }
}
