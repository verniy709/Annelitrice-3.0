using HarmonyLib;
using AlienRace;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Annelitrice.HarmonyPatches
{
	[HarmonyPatch(typeof(Corpse), "Destroy")]
	public static class Patch_Corpse_Destroy
	{
		[HarmonyPrefix]
		public static void Prefix(Corpse __instance)
		{
			if (__instance.InnerPawn != null && __instance.InnerPawn.def.defName == "Annelitrice" && __instance.Spawned && !__instance.InnerPawn.IsShambler && !(__instance is UnnaturalCorpse))
			{
				GenPlace.TryPlaceThing(CompEgg.MakeEgg(__instance.InnerPawn), __instance.PositionHeld, __instance.MapHeld, ThingPlaceMode.Near);
			}
			if (__instance.InnerPawn != null && __instance.InnerPawn.def.defName == "AnnelitriceLarvaAsAnimal" && __instance.Spawned)
			{
				GenPlace.TryPlaceThing(CompEgg.MakeEgg(__instance.InnerPawn.TryGetComp<CompContainPawn>().GetDirectlyHeldThings().First() as Pawn), __instance.PositionHeld, __instance.MapHeld, ThingPlaceMode.Near);
			}
		}
	}
	[HarmonyPatch(typeof(Pawn), "Destroy")]
	public static class Pawn_Destroy_Patch
	{
		[HarmonyPrefix]
		public static void Prefix(Pawn __instance)
		{
			if (__instance.Corpse is null && __instance.def.defName == "Annelitrice" && __instance.Spawned && !__instance.IsShambler && !__instance.IsAwokenCorpse)
			{
				GenPlace.TryPlaceThing(CompEgg.MakeEgg(__instance), __instance.PositionHeld, __instance.MapHeld, ThingPlaceMode.Near);
			}
			if (__instance.Corpse is null && __instance.def.defName == "AnnelitriceLarvaAsAnimal" && __instance.Spawned)
			{
				if (__instance.TryGetComp<CompContainPawn>().GetDirectlyHeldThings().Any)
				{
					GenPlace.TryPlaceThing(CompEgg.MakeEgg(__instance.TryGetComp<CompContainPawn>().GetDirectlyHeldThings().First() as Pawn), __instance.PositionHeld, __instance.MapHeld, ThingPlaceMode.Near);
				}
			}
		}
	}
}
