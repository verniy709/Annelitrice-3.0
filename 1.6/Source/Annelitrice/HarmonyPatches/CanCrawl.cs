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
	[HarmonyPatch(typeof(Pawn_HealthTracker), "get_CanCrawl")]
	public static class ForbidCrawling_Patch
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
