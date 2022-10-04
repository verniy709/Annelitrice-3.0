using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Annelitrice
{
 //   public class JobDriver_GainEPsFromWormIncubator : JobDriver
 //   {
	//	public const TargetIndex BuildingInd = TargetIndex.A;

	//	public const int WorkTimeTicks = 300;
	//	public override bool TryMakePreToilReservations(bool errorOnFailed)
	//	{
	//		LocalTargetInfo target = job.GetTarget(BuildingInd);
	//		if (!pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
	//		{
	//			return false;
	//		}
	//		Thing thing = target.Thing;
	//		if (thing != null && thing.def.hasInteractionCell && !pawn.ReserveSittableOrSpot(thing.InteractionCell, job, errorOnFailed))
	//		{
	//			return false;
	//		}
	//		return true;
	//	}

	//	protected override IEnumerable<Toil> MakeNewToils()
	//	{
	//		this.FailOnDespawnedOrNull(TargetIndex.A);
	//		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
	//		yield return Toils_General.Wait(WorkTimeTicks, TargetIndex.A).PlaySustainerOrSound(SoundDefOf.Interact_RecolorApparel).WithProgressBarToilDelay(TargetIndex.A);
	//		yield return Toils_General.Do(delegate
	//		{
	//			var incubator = job.targetA.Thing as WormIncubator;
	//			incubator.evolutionPoints -= job.count;
	//			var comp = pawn.GetComp<CompEvolution>();
	//			comp.evolutionPoints += job.count;
	//		});
	//	}
	//}
}
