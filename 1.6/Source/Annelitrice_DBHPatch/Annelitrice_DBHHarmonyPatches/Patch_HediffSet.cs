using HarmonyLib;
using Verse;
using DubsBadHygiene;
using Annelitrice;

namespace Annelitrice.DBHHarmonyPatches
{
    //Reset pawn rendering during wash jobs, for arm actions and body props
    [HarmonyPatch(typeof(HediffSet), nameof(HediffSet.AddDirect))]
    public static class Patch_HediffSet_AddDirect_WashingDirty
    {
        public static void Postfix(HediffSet __instance, Hediff hediff)
        {
            if (hediff?.def != DubDef.Washing) return;
            Pawn pawn = __instance.pawn;
            if (pawn != null && pawn?.def == AnnelitriceDefOf.Annelitrice)
                pawn.Drawer?.renderer?.SetAllGraphicsDirty();
        }
    }
}
