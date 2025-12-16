using HarmonyLib;
using System.Collections.Generic;

namespace Annelitrice.HarmonyPatches
{
	[HarmonyPatch(typeof(AlienRace.ExtendedGraphics.Condition))]
	[HarmonyPatch(MethodType.StaticConstructor)]
	static class Patch_ExtendedGraphics_Condition
	{
		static void Postfix()
		{
			var xmlNameParseKeys = Traverse.Create(typeof(AlienRace.ExtendedGraphics.Condition))
				.Field<Dictionary<string, string>>("XmlNameParseKeys").Value;
			xmlNameParseKeys.Add(AnnelitriceConditionIsDead.XmlNameParseKey, typeof(AnnelitriceConditionIsDead).FullName);
		}
	}
}
