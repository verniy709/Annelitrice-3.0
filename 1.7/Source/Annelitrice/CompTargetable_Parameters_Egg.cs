using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Annelitrice
{
	public class CompTargetable_Parameters_Egg : CompTargetable
	{
		protected override bool PlayerChoosesTarget
		{
			get
			{
				return true;
			}
		}
		public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
		{
			yield return targetChosenByPlayer;
			yield break;
		}
		protected override TargetingParameters GetTargetingParameters()
		{
			return new TargetingParameters
			{
				canTargetPawns = false,
				canTargetBuildings = false,
				canTargetItems = true,
				mapObjectTargetsMustBeAutoAttackable = false,
				validator = delegate (TargetInfo x)
				{
					return x.Thing != null &&
						(
							(x.Thing.TryGetComp<CompEgg>() != null && x.Thing.def.defName != "Anneli_Pupa")
							||
							(x.Thing is Corpse corpse &&
							 (corpse.def.defName == "Corpse_Annelitrice"|| corpse.def.defName == "Corpse_AnnelitriceLarvaAsAnimal"))
						);
				}
			};
		}
	}
}
