using HarmonyLib;
using RimWorld;
using System;
using System.Runtime.CompilerServices;
using Verse;

namespace Annelitrice.HarmonyPatches
{
	[StaticConstructorOnStartup]
	public static class Patch_Init
	{
		static Patch_Init()
		{
			var h = new Harmony("Annelitrice.DisableFishingEffecter");

			var miTI = AccessTools.Method(typeof(Effecter), "EffectTick",
				new Type[] { typeof(TargetInfo), typeof(TargetInfo) });

			if (miTI != null)
			{
				h.Patch(miTI,
					prefix: new HarmonyMethod(typeof(Patch_Effecter_EffectTick), nameof(Patch_Effecter_EffectTick.PrefixTI)));
			}
			else
			{
				Log.Warning("[Annelitrice] Effecter.EffectTick(TargetInfo, TargetInfo) not found.");
			}

			var miLTI = AccessTools.Method(typeof(Effecter), "EffectTick",
				new Type[] { typeof(LocalTargetInfo), typeof(LocalTargetInfo) });

			if (miLTI != null)
			{
				h.Patch(miLTI,
					prefix: new HarmonyMethod(typeof(Patch_Effecter_EffectTick), nameof(Patch_Effecter_EffectTick.PrefixLTI)));
			}
			else
			{
				Log.Message("[Annelitrice] Effecter.EffectTick(LocalTargetInfo, LocalTargetInfo) not found.");
			}
		}
	}

	public static class Patch_Effecter_EffectTick
	{
		private static readonly ConditionalWeakTable<Effecter, ByteBox> Cleaned =
			new ConditionalWeakTable<Effecter, ByteBox>();
		private sealed class ByteBox
		{
			public byte b;
		}

		public static bool PrefixTI(Effecter __instance, TargetInfo A, TargetInfo B)
		{
			return AllowEffect(__instance, A, B);
		}

		public static bool PrefixLTI(Effecter __instance, LocalTargetInfo A, LocalTargetInfo B)
		{
			return AllowEffect(__instance, A, B);
		}

		private static bool AllowEffect(Effecter effecter, TargetInfo A, TargetInfo B)
		{
			if (!ShouldSuppress(effecter, TryGetPawn(A) ?? TryGetPawn(B)))
				return true;

			CleanupOnce(effecter);
			return false;
		}

		private static bool AllowEffect(Effecter effecter, LocalTargetInfo A, LocalTargetInfo B)
		{
			if (!ShouldSuppress(effecter, TryGetPawn(A) ?? TryGetPawn(B)))
				return true;

			CleanupOnce(effecter);
			return false;
		}

		private static bool ShouldSuppress(Effecter effecter, Pawn pawn)
		{
			if (effecter == null || effecter.def != EffecterDefOf.Fishing) return false;
			if (pawn == null) return false;
			return pawn.def != null && pawn.def.defName == AnnelitriceDefOf.Annelitrice.defName;
		}

		private static void CleanupOnce(Effecter effecter)
		{
			if (effecter == null) return;

			ByteBox _;
			if (Cleaned.TryGetValue(effecter, out _))
				return;

			effecter.Cleanup();
			Cleaned.GetValue(effecter, k => new ByteBox { b = 1 }); 
		}

		private static Pawn TryGetPawn(TargetInfo target)
		{
			return target.HasThing && target.Thing is Pawn ? (Pawn)target.Thing : null;
		}

		private static Pawn TryGetPawn(LocalTargetInfo localTarget)
		{
			return localTarget.HasThing && localTarget.Thing is Pawn ? (Pawn)localTarget.Thing : null;
		}
	}
}
