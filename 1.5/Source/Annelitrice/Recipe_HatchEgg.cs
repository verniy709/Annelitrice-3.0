using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace Annelitrice
{
	class Recipe_HatchEgg : RecipeWorker
	{
		public override void Notify_IterationCompleted(Pawn pawn, List<Thing> ingredients)
		{
			base.Notify_IterationCompleted(pawn, ingredients);

			Thing egg = ingredients.FirstOrDefault(thing =>
				thing.def.defName == "Anneli_ColonistEgg" ||
				thing.def.defName == "Anneli_OutsiderEgg");

			if (egg == null)
			{
				Log.Error("No egg found in ingredients.");
				return;
			}

			CompContainPawn eggContainer = egg.TryGetComp<CompContainPawn>();
			if (eggContainer == null)
			{
				Log.Message("No contained pawn found.");
				Messages.Message("NoContainedPawn_Annelitrice".Translate(), MessageTypeDefOf.NeutralEvent, true);
				return;
			}

			IntVec3 pos = pawn.Position;
			Map map = pawn.Map;
			Pawn larva = PawnGenerator.GeneratePawn(PawnKindDef.Named("AnnelitriceLarvaAsAnimal"), Faction.OfPlayer);
			CompContainPawn larvaContainer = larva.GetComp<CompContainPawn>();
			eggContainer.GetDirectlyHeldThings().TryTransferAllToContainer(larvaContainer.GetDirectlyHeldThings());
			GenSpawn.Spawn(larva, pos, map, WipeMode.VanishOrMoveAside);
		}
	}
}
