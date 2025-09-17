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

	[HarmonyPatch(typeof(PawnRenderUtility), "DrawEquipmentAiming")]
	[HarmonyBefore("com.yayo.yayoAni")]
	public static class DrawEquipmentAiming_Patch
	{
		public static float northZOffset = 0.23f;
		public static float eastZOffset = 0.23f;
		public static float southZOffset = 0.23f;
		public static float westZOffset = 0.23f;
		public static float northXOffset = 0.0f;
		public static float eastXOffset = -0.1f;
		public static float southXOffset = 0.0f;
		public static float westXOffset = 0.1f;

		public static Pawn GetPawnAsEquipmentUser(this Thing thing)
		{
			if (thing.ParentHolder is Pawn_EquipmentTracker pawn_EquipmentTracker) return pawn_EquipmentTracker.pawn;
			return null;
		}

		public static void Prefix(Thing eq, ref Vector3 drawLoc)
		{
			Pawn pawn = eq.GetPawnAsEquipmentUser();

			if (pawn.def != null && pawn.def.defName == AnnelitriceDefOf.Annelitrice.defName)
			{
				if (pawn.CurJob == null || pawn.CurJob.def.neverShowWeapon)
					return;

				if (pawn.Rotation == Rot4.North)
				{
					drawLoc += new Vector3(northXOffset, 0f, northZOffset);
				}
				else if (pawn.Rotation == Rot4.East)
				{
					drawLoc += new Vector3(eastXOffset, 0f, eastZOffset);
				}
				else if (pawn.Rotation == Rot4.South)
				{
					drawLoc += new Vector3(southXOffset, 0f, southZOffset);
				}
				else if (pawn.Rotation == Rot4.West)
				{
					drawLoc += new Vector3(westXOffset, 0f, westZOffset);
				}
				return;
			}
		}
	}
}

