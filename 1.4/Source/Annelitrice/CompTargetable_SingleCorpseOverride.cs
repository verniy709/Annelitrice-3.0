using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Annelitrice
{
	//Target corpse and egg for resurrection
    public class CompTargetable_SingleCorpseOverride : CompTargetable_SingleCorpse
    {
		protected override TargetingParameters GetTargetingParameters()
		{
			return new TargetingParameters
			{
				canTargetPawns = false,
				canTargetBuildings = false,
				canTargetItems = true,
				mapObjectTargetsMustBeAutoAttackable = false,
				validator = (TargetInfo x) => (x.Thing is Corpse || x.Thing is ThingWithComps egg) && BaseTargetValidator(x.Thing)
			};
		}
	}

	public class CompTargetable_SingleCorpseAnneliRace : CompTargetable_SingleCorpse
	{
		protected override TargetingParameters GetTargetingParameters()
		{
			return new TargetingParameters
			{
				canTargetPawns = false,
				canTargetBuildings = false,
				canTargetItems = true,
				mapObjectTargetsMustBeAutoAttackable = false,
				validator = (TargetInfo x) => (x.Thing is Corpse corpse && corpse.InnerPawn.def == AnnelitriceDefOf.Annelitrice || x.Thing is ThingWithComps egg) && BaseTargetValidator(x.Thing)
			};
		}
	}
}
