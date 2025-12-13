using HarmonyLib;
using Verse;
using DubsBadHygiene;

namespace Annelitrice_DBHHarmonyPatches
{
    //Reset pawn rendering during wash jobs, for arm actions and body props
    [HarmonyPatch(typeof(HediffSet), nameof(HediffSet.AddDirect))]
    public static class Patch_HediffSet_AddDirect_WashingDirty
    {
        public static void Postfix(HediffSet __instance, Hediff hediff)
        {
            if (hediff?.def != DubDef.Washing) return;
            Pawn pawn = __instance.pawn;
            if (pawn != null && pawn.def.defName == "Annelitrice")
                pawn.Drawer?.renderer?.SetAllGraphicsDirty();
        }
    }
}
