using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Annelitrice.HarmonyPatches
{
	[HarmonyPatch]
	internal static class Patch_Effecter_FishingForceHide
	{
		private static readonly FieldInfo fiChildren = AccessTools.Field(typeof(Effecter), "children");

		[HarmonyTargetMethods]
		static IEnumerable<MethodBase> TargetMethods()
		{
			var methods = new List<MethodBase>
			{
				AccessTools.Method(typeof(Effecter), nameof(Effecter.Trigger), new[] { typeof(TargetInfo), typeof(TargetInfo) }),
				AccessTools.Method(typeof(Effecter), nameof(Effecter.Trigger), new[] { typeof(LocalTargetInfo), typeof(LocalTargetInfo) }),
				AccessTools.Method(typeof(Effecter), nameof(Effecter.EffectTick), new[] { typeof(TargetInfo), typeof(TargetInfo) }),
				AccessTools.Method(typeof(Effecter), nameof(Effecter.EffectTick), new[] { typeof(LocalTargetInfo), typeof(LocalTargetInfo) })
			};

			return methods.Where(m => m != null);
		}

		[HarmonyPrefix]
		private static bool Prefix(Effecter __instance, object __0, object __1)
		{
			if (__instance.def == null || !__instance.def.defName.Contains("Fishing"))
				return true;

			Pawn pawn = ExtractPawn(__0) ?? ExtractPawn(__1);
			if (pawn?.def == null || pawn.def.defName != "Annelitrice")
				return true;

			__instance.Cleanup();

			if (fiChildren?.GetValue(__instance) is List<SubEffecter> children)
			{
				children.Clear();
			}

			return false;
		}

		private static Pawn ExtractPawn(object target)
		{
			if (target is TargetInfo ti) return ti.Thing as Pawn;
			if (target is LocalTargetInfo lti) return lti.Thing as Pawn;
			return null;
		}
	}
}