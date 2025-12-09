using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Annelitrice.HarmonyPatches
{
	[HarmonyPatch(typeof(Effecter))]
	internal static class Patch_Effecter_Fishing_Annelitrice
	{
		private static readonly FieldInfo fiDef =
			AccessTools.Field(typeof(Effecter), "def");

		// Only guard this patch, not the vanilla Cleanup
		private static readonly HashSet<Effecter> cleanupGuard = new HashSet<Effecter>();

		[HarmonyTargetMethods]
		private static IEnumerable<MethodBase> TargetMethods()
		{
			foreach (var m in typeof(Effecter).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				if (m.Name != "EffectTick")
					continue;

				var p = m.GetParameters();
				if (p.Length != 2)
					continue;

				bool bothTargetInfo =
					p[0].ParameterType == typeof(TargetInfo) &&
					p[1].ParameterType == typeof(TargetInfo);

				bool bothLocalTargetInfo =
					p[0].ParameterType == typeof(LocalTargetInfo) &&
					p[1].ParameterType == typeof(LocalTargetInfo);

				if (bothTargetInfo || bothLocalTargetInfo)
					yield return m;
			}
		}

		private static bool Prefix(Effecter __instance, object __0, object __1)
		{
			if (cleanupGuard.Contains(__instance))
				return false;

			if (!(fiDef?.GetValue(__instance) is EffecterDef def) || def != EffecterDefOf.Fishing)
				return true;

			Pawn pawn = ExtractPawnFromTargetLike(__0) ?? ExtractPawnFromTargetLike(__1);
			if (pawn == null)
				return true;

			if (pawn.def == null || pawn.def != AnnelitriceDefOf.Annelitrice)
				return true;

			cleanupGuard.Add(__instance);
			__instance.Cleanup();

			return false;
		}

		private static Pawn ExtractPawnFromTargetLike(object maybeTarget)
		{
			if (maybeTarget == null) return null;

			if (maybeTarget is TargetInfo ti)
				return ti.Thing as Pawn;

			if (maybeTarget is LocalTargetInfo lti)
				return lti.Thing as Pawn;

			return null;
		}
	}
}
