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
	internal class patch_DrawEquipment
	{
		[HarmonyPatch(typeof(PawnRenderUtility), "DrawEquipmentAndApparelExtras")]
		//[HarmonyBefore("com.yayo.yayoAni")]
		public static class DrawEquipmentAndApparelExtras_Patch
		{
			public static float northZOffset = 0.23f;
			public static float eastZOffset = 0.23f;
			public static float southZOffset = 0.23f;
			public static float westZOffset = 0.23f;
			public static float northXOffset = 0.0f;
			public static float eastXOffset = -0.1f;
			public static float southXOffset = 0.0f;
			public static float westXOffset = 0.1f;

			public static void Prefix(Pawn pawn, Vector3 drawPos)
			{
				// Check if the pawn is annelitrice
				if (pawn.def != null && pawn.def is ThingDef_AlienRace annelitrice && annelitrice.defName.Equals("Annelitrice"))
				{
					// Check if the pawn's current job is null or neverShowWeapon
					if (pawn.CurJob == null || pawn.CurJob.def.neverShowWeapon)
						return;

					if (pawn.Rotation == Rot4.North)
					{
						drawPos += new Vector3(northXOffset, 0f, northZOffset);
					}
					else if (pawn.Rotation == Rot4.East)
					{
						drawPos += new Vector3(eastXOffset, 0f, eastZOffset);
					}
					else if (pawn.Rotation == Rot4.South)
					{
						drawPos += new Vector3(southXOffset, 0f, southZOffset);
					}
					else if (pawn.Rotation == Rot4.West)
					{
						drawPos += new Vector3(westXOffset, 0f, westZOffset);
					}
					return;
				}
			}
		}
	}
}
