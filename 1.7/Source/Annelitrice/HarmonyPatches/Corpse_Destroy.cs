using HarmonyLib;
using System;
using System.Linq;
using Verse;

namespace Annelitrice.HarmonyPatches
{
	[HarmonyPatch(typeof(RecipeWorker), "ConsumeIngredient")]
	public static class Patch_RecipeWorker_ConsumeIngredient
	{
		[ThreadStatic]
		private static int butcherCorpseDepth;

		public static bool IsButcheringCorpse => butcherCorpseDepth > 0;

		[HarmonyPrefix]
		public static void Prefix(Thing ingredient, RecipeDef recipe, out bool __state)
		{
			__state = ingredient is Corpse && recipe?.defName == "ButcherCorpseFlesh"
				&& ModsConfig.IsActive("Verniy709.Annelitrice3.0.Retheme.DarkSouls");
			if (__state)
			{
				butcherCorpseDepth++;
			}
		}

		[HarmonyFinalizer]
		public static Exception Finalizer(Exception __exception, bool __state)
		{
			if (__state)
			{
				butcherCorpseDepth--;
			}
			return __exception;
		}
	}

	[HarmonyPatch(typeof(Corpse), "Destroy")]
	public static class Patch_Corpse_Destroy
	{
		public static Pawn GetContainedPawn(Pawn larva)
		{
			ThingOwner heldThings = larva.TryGetComp<CompContainPawn>()?.GetDirectlyHeldThings();
			return heldThings == null ? null : heldThings.FirstOrDefault() as Pawn;
		}

		[HarmonyPrefix]
		public static void Prefix(Corpse __instance)
		{
			if (Patch_RecipeWorker_ConsumeIngredient.IsButcheringCorpse)
			{
				return;
			}

			if (__instance.InnerPawn != null && __instance.InnerPawn.def == AnnelitriceDefOf.Annelitrice && __instance.Spawned && !__instance.InnerPawn.IsShambler && !(__instance is UnnaturalCorpse))
			{
				GenPlace.TryPlaceThing(CompEgg.MakeEgg(__instance.InnerPawn), __instance.PositionHeld, __instance.MapHeld, ThingPlaceMode.Near);
			}
			if (__instance.InnerPawn != null && __instance.InnerPawn.def == AnnelitriceDefOf.AnnelitriceLarvaAsAnimal && __instance.Spawned)
			{
				Pawn containedPawn = GetContainedPawn(__instance.InnerPawn);
				if (containedPawn != null)
				{
					GenPlace.TryPlaceThing(CompEgg.MakeEgg(containedPawn), __instance.PositionHeld, __instance.MapHeld, ThingPlaceMode.Near);
				}
			}
		}
	}
	[HarmonyPatch(typeof(Pawn), "Destroy")]
	public static class Patch_Pawn_Destroy
	{
		[HarmonyPrefix]
		public static void Prefix(Pawn __instance)
		{
			if (Patch_RecipeWorker_ConsumeIngredient.IsButcheringCorpse)
			{
				return;
			}

			if (__instance.Corpse is null && __instance.def == AnnelitriceDefOf.Annelitrice && __instance.Spawned && !__instance.IsShambler && !__instance.IsAwokenCorpse)
			{
				GenPlace.TryPlaceThing(CompEgg.MakeEgg(__instance), __instance.PositionHeld, __instance.MapHeld, ThingPlaceMode.Near);
			}
			if (__instance.Corpse is null && __instance.def == AnnelitriceDefOf.AnnelitriceLarvaAsAnimal && __instance.Spawned)
			{
				Pawn containedPawn = Patch_Corpse_Destroy.GetContainedPawn(__instance);
				if (containedPawn != null)
				{
					GenPlace.TryPlaceThing(CompEgg.MakeEgg(containedPawn), __instance.PositionHeld, __instance.MapHeld, ThingPlaceMode.Near);
				}
			}
		}
	}
}
