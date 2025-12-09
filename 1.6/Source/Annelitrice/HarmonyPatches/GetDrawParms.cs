using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Annelitrice.HarmonyPatches
{
	[HarmonyPatch(typeof(PawnRenderer), "GetDrawParms")]
	public static class Patch_GetDrawParms
	{
		public static void Postfix(ref PawnDrawParms __result, Pawn ___pawn)
		{
			if (___pawn == null)
				return;

			if (___pawn.def != AnnelitriceDefOf.Annelitrice)
				return;

			if (___pawn.CurrentBed() == null)
				return;

			if (__result.posture == PawnPosture.Standing)
				return;

			__result.bed = null;

			/*Move the pawn up a bit*/
			const float zOffset = 0.12f;
			var drawLoc = Matrix4x4.Translate(new Vector3(0f, 0f, zOffset));
			__result.matrix = drawLoc * __result.matrix;
		}
	}
}
