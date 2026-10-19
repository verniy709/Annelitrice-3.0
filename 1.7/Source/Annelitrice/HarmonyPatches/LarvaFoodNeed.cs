using HarmonyLib;
using RimWorld;
using Verse;

namespace Annelitrice.HarmonyPatches
{
    [HarmonyPatch(typeof(Need_Food), nameof(Need_Food.NeedInterval))]
    internal static class Patch_LarvaFoodNeed
    {
        private static bool Prefix(Pawn ___pawn)
        {
            // Keep the current food level fixed while this larva's pupation is paused.
            if (___pawn == null || ___pawn.def != AnnelitriceDefOf.AnnelitriceLarvaAsAnimal)
            {
                return true;
            }

            return ___pawn.TryGetComp<CompLarva>()?.AllowPupation != false;
        }
    }
}
