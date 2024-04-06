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
	class patch_DrawEquipment
	{
		[HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming")]
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


			public static void Prefix(PawnRenderer __instance, Pawn ___pawn, Thing eq, ref Vector3 drawLoc, float aimAngle)
			{
				// Check if the pawn is annelitrice
				if (___pawn.def != null && ___pawn.def is ThingDef_AlienRace annelitrice && annelitrice.defName.Equals("Annelitrice"))
				{
					// Check if the pawn's current job is null or neverShowWeapon
					if (___pawn.CurJob == null || ___pawn.CurJob.def.neverShowWeapon)
						return;

					if (___pawn.Rotation == Rot4.North)
					{
						drawLoc.z += northZOffset;
						drawLoc.x += northXOffset;
					}
					else if (___pawn.Rotation == Rot4.East)
					{
						drawLoc.z += eastZOffset;
						drawLoc.x += eastXOffset;
					}
					else if (___pawn.Rotation == Rot4.South)
					{
						drawLoc.z += southZOffset;
						drawLoc.x += southXOffset;
					}
					else if (___pawn.Rotation == Rot4.West)
					{
						drawLoc.z += westZOffset;
						drawLoc.x += westXOffset;
					}
					return;
				}
			}
		}
	}
}
