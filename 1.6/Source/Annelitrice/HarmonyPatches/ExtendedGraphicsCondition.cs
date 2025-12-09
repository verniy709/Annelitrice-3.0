using HarmonyLib;
using AlienRace;
using AlienRace.ExtendedGraphics;
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
	[HarmonyPatch(typeof(AlienRace.ExtendedGraphics.Condition))]
	[HarmonyPatch(MethodType.StaticConstructor)]
	static class Patch_ExtendedGraphicsCondition
	{
		static void Postfix()
		{
			var xmlNameParseKeys = Traverse.Create(typeof(AlienRace.ExtendedGraphics.Condition))
				.Field<Dictionary<string, string>>("XmlNameParseKeys").Value;
			xmlNameParseKeys.Add(AnnelitriceConditionIsDead.XmlNameParseKey, typeof(AnnelitriceConditionIsDead).FullName);
		}
	}
}
