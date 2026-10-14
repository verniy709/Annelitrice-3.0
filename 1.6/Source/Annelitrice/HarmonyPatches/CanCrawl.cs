using HarmonyLib;
using Verse;

namespace Annelitrice.HarmonyPatches
{
	[HarmonyPatch(typeof(Pawn_HealthTracker), "get_CanCrawl")]
	public static class Patch_Pawn_HealthTracker_CanCrawl
	{
		public static void Postfix(Pawn_HealthTracker __instance, ref bool __result)
		{
			if (!__result) return;

			Pawn pawn = (Pawn)AccessTools.Field(typeof(Pawn_HealthTracker), "pawn").GetValue(__instance); 

			if (pawn.def.defName == AnnelitriceDefOf.Annelitrice.defName)
			{
				__result = false;
			}
		}
	}
}
