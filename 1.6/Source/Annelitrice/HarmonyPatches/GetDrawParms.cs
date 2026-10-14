using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Annelitrice.HarmonyPatches
{
	[HarmonyPatch(typeof(PawnRenderer), "GetDrawParms")]
	public static class Patch_PawnRenderer_GetDrawParms
	{
		public static void Postfix(ref PawnDrawParms __result, Pawn ___pawn)
		{
			if (___pawn == null)
				return;

			if (___pawn.def != AnnelitriceDefOf.Annelitrice)
				return;

			Building_Bed bed = ___pawn.CurrentBed();
			if (bed == null)
				return;

			if (__result.posture == PawnPosture.Standing)
				return;

			__result.bed = null;

			float x = 0f;
			float z = 0f;

			if (bed.Rotation == Rot4.North)
			{
				z += 0.06f;
			}
			else if (bed.Rotation == Rot4.South)
			{
				z += 0.08f;
			}
			else if (bed.Rotation == Rot4.East)
			{
				x -= 0.04f;
				//z += 0.12f;
			}
			else if (bed.Rotation == Rot4.West)
			{
				x += 0.04f;
				//z += 0.12f;
			}

			Matrix4x4 translate = Matrix4x4.Translate(new Vector3(x, 0f, z));
			__result.matrix = translate * __result.matrix;
		}
	}
}
