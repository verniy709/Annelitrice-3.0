using HarmonyLib;
using Verse;

namespace Annelitrice.HarmonyPatches
{
	[HarmonyPatch(typeof(Pawn_HealthTracker), "get_CanCrawl")]
	public static class Patch_Pawn_HealthTracker_CanCrawl
	{
		public static void Postfix(
			Pawn ___pawn,
			ref bool __result)
		{
			if (__result &&
				___pawn?.def == AnnelitriceDefOf.Annelitrice)
			{
				__result = false;
			}
		}
	}
}
