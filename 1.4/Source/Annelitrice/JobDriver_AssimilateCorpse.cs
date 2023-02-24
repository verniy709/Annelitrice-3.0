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
 //   public class JobDriver_AssimilateCorpse : JobDriver
 //   {
	//	public const TargetIndex CorpseInd = TargetIndex.A;
	//	public const int WorkTimeTicks = 600;
	//	public override bool TryMakePreToilReservations(bool errorOnFailed)
	//	{
	//		LocalTargetInfo target = job.GetTarget(CorpseInd);
	//		if (!pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
	//		{
	//			return false;
	//		}
	//		return true;
	//	}
	//	protected override IEnumerable<Toil> MakeNewToils()
	//	{
	//		this.FailOnDespawnedOrNull(TargetIndex.A);
	//		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
	//		yield return Toils_General.Wait(WorkTimeTicks, TargetIndex.A).WithProgressBarToilDelay(TargetIndex.A);
	//		yield return Toils_General.Do(delegate
	//		{
	//			var comp = pawn.GetComp<CompEvolution>();
	//			comp.evolutionPoints += 10;
	//			pawn.needs.food.CurLevel += 0.45f;
	//			var corpse = job.targetA.Thing as Corpse;
	//			corpse.Strip();
	//			corpse.Destroy();
	//		});
	//	}
	//}
}
