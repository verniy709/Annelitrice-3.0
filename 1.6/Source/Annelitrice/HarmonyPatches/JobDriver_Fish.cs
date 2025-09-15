using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Verse;

namespace Annelitrice.HarmonyPatches
{
	internal static class CleanupGuard
	{
		public sealed class ByteBox
		{
			public byte b;
		}

		public static readonly ConditionalWeakTable<Effecter, ByteBox> Cleaned = new ConditionalWeakTable<Effecter, ByteBox>();

		[ThreadStatic] public static bool AllowNextCleanup;
	}

	[HarmonyPatch(typeof(Effecter), nameof(Effecter.Cleanup))]
	internal static class Patch_Effecter_Cleanup_Idempotent
	{
		static bool Prefix(Effecter __instance)
		{
			if (CleanupGuard.AllowNextCleanup) return true; 

			CleanupGuard.ByteBox _;
			if (CleanupGuard.Cleaned.TryGetValue(__instance, out _))
				return false;

			return true;
		}

		static void Postfix(Effecter __instance)
		{
			CleanupGuard.Cleaned.GetValue(__instance, _ => new CleanupGuard.ByteBox { b = 1 });
		}
	}

	[HarmonyPatch(typeof(Effecter))]
	internal static class Patch_Effecter_EffectTick_FishingForRace
	{
		private static readonly FieldInfo fiDef = AccessTools.Field(typeof(Effecter), "def");
		private static readonly FieldInfo fiMaintainer = AccessTools.Field(typeof(Effecter), "maintainer");

		static IEnumerable<MethodBase> TargetMethods()
		{
			foreach (var m in typeof(Effecter).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				if (m.Name != "EffectTick") continue;
				var p = m.GetParameters();
				if (p.Length != 2) continue;

				if ((p[0].ParameterType == typeof(TargetInfo) && p[1].ParameterType == typeof(TargetInfo)) ||
					(p[0].ParameterType == typeof(LocalTargetInfo) && p[1].ParameterType == typeof(LocalTargetInfo)))
				{
					yield return m;
				}
			}
		}

		static bool Prefix(Effecter __instance, object __0, object __1)
		{
			var def = fiDef?.GetValue(__instance) as EffecterDef;
			if (def == null) return true;

			bool isFishing = (def == EffecterDefOf.Fishing);
			if (!isFishing) return true;

			var pawn = TryExtractPawnFromTargets(__0, __1) ?? TryExtractPawnFromMaintainer(__instance);
			if (pawn == null) return true;

			if (pawn.def == null || AnnelitriceDefOf.Annelitrice == null) return true;
			if (!ReferenceEquals(pawn.def, AnnelitriceDefOf.Annelitrice) &&
				!string.Equals(pawn.def.defName, AnnelitriceDefOf.Annelitrice.defName, StringComparison.Ordinal))
				return true;

			if (!AlreadyCleaned(__instance))
			{
				CleanupOnceGuarded(__instance);
			}

			return false; 
		}


		private static Pawn TryExtractPawnFromTargets(object a, object b)
		{
			Pawn pawn = ExtractPawnFromTargetLike(a);
			if (pawn != null) return pawn;
			return ExtractPawnFromTargetLike(b);
		}

		private static Pawn ExtractPawnFromTargetLike(object maybeTarget)
		{
			if (maybeTarget == null) return null;

			if (maybeTarget is TargetInfo ti) return ti.Thing as Pawn;

			if (maybeTarget is LocalTargetInfo lti) return lti.Thing as Pawn;

			return null;
		}

		private static Pawn TryExtractPawnFromMaintainer(Effecter eff)
		{
			var maintainer = fiMaintainer?.GetValue(eff);
			if (maintainer == null) return null;

			var mt = maintainer.GetType();
			var fThing = AccessTools.Field(mt, "thing");
			if (fThing?.GetValue(maintainer) is Pawn p1) return p1;

			var fMaintainer = AccessTools.Field(mt, "maintainer");
			if (fMaintainer?.GetValue(maintainer) is Pawn p2) return p2;

			return null;
		}

		private static bool AlreadyCleaned(Effecter eff)
		{
			CleanupGuard.ByteBox _;
			return CleanupGuard.Cleaned.TryGetValue(eff, out _);
		}

		private static void CleanupOnceGuarded(Effecter eff)
		{
			CleanupGuard.AllowNextCleanup = true;
			try
			{
				eff.Cleanup();
			}
			finally
			{
				CleanupGuard.AllowNextCleanup = false;
			}

			CleanupGuard.Cleaned.GetValue(eff, _ => new CleanupGuard.ByteBox { b = 1 });
		}
	}
}
