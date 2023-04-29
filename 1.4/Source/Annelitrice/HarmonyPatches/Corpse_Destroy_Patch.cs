using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Annelitrice.HarmonyPatches
{
	[HarmonyPatch(typeof(Corpse), "Destroy")]
	public static class Corpse_Destroy_Patch
	{
		[HarmonyPrefix]
		public static void Prefix(Corpse __instance)
		{
			if(__instance.InnerPawn != null && __instance.InnerPawn.def.defName == "Annelitrice" && __instance.Spawned)
			GenPlace.TryPlaceThing(CompEgg.MakeEgg(__instance.InnerPawn), __instance.PositionHeld, __instance.MapHeld, ThingPlaceMode.Near);
		}
	}
	[HarmonyPatch(typeof(Pawn), "Destroy")]
	public static class Pawn_Destroy_Patch
	{
		[HarmonyPrefix]
		public static void Prefix(Pawn __instance)
		{
			if (__instance.Corpse is null && __instance.def.defName == "Annelitrice" && __instance.Spawned)
			{
				GenPlace.TryPlaceThing(CompEgg.MakeEgg(__instance), __instance.PositionHeld, __instance.MapHeld, ThingPlaceMode.Near);
			}
		}
	}
}
