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

    // Restore cached clothing requests when a pawn finishes washing.
    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.RemoveHediff))]
    public static class Patch_Pawn_HealthTracker_RemoveHediff_WashingDirty
    {
        public static void Postfix(Pawn_HealthTracker __instance, Hediff hediff)
        {
            if (hediff?.def != DubDef.Washing) return;

            Pawn pawn = __instance.hediffSet?.pawn;
            if (pawn?.def == AnnelitriceDefOf.Annelitrice)
                pawn.Drawer?.renderer?.SetAllGraphicsDirty();
        }
    }
}
