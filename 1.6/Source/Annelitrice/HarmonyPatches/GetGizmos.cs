using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Annelitrice.HarmonyPatches
{
	[HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
	public static class Patch_Pawn_GetGizmos
	{
		public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
		{
			if (Find.Selector == null)
			{
				foreach (var gizmo in __result)
					yield return gizmo;
				yield break;
			}

			bool multiSelect = Find.Selector.NumSelected > 1;

			foreach (var gizmo in __result)
			{
				if (!multiSelect)
				{
					yield return gizmo;
					continue;
				}

				if (gizmo is Command_Ability abilityCmd &&
					abilityCmd.Ability != null &&
					abilityCmd.Ability.def == AnnelitriceDefOf.Anneli_Infest)
				{
					continue;
				}

				yield return gizmo;
			}
		}
	}
}
