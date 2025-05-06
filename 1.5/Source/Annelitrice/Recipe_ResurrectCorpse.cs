using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace Annelitrice
{
	class Recipe_ResurrectCorpse : RecipeWorker
	{
		public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
		{
			if (!(ingredient is Corpse))
			{
				base.ConsumeIngredient(ingredient, recipe, map);
			}
		}

		public override void Notify_IterationCompleted(Pawn pawn, List<Thing> ingredients)
		{
			Corpse corpse = ingredients.Find(t => t is Corpse) as Corpse;
			if (corpse != null && corpse.InnerPawn != null)
			{
				if (corpse.def.defName == "Corpse_Annelitrice")
				{
					ResurrectionUtility.TryResurrectWithSideEffects(corpse.InnerPawn);
				}
				else
				{
					Messages.Message("NotResurrectingPawn_Annelitrice".Translate(), MessageTypeDefOf.NeutralEvent, true);
					return;
				}
			}
		}
	}
}
