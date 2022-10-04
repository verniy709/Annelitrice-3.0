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
	public class JobDriver_StandSilentlyFaceTarget : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil toil = new Toil();
			toil.tickAction = delegate
			{
				base.pawn.rotationTracker.FaceTarget(TargetA);
			};
			toil.socialMode = RandomSocialMode.Off;
			toil.defaultCompleteMode = ToilCompleteMode.Never;
			toil.handlingFacing = true;
			yield return toil;
		}
	}

	public class JobDriver_InfestHuman : JobDriver
    {
		public const TargetIndex PawnInd = TargetIndex.A;
		public const int WorkTimeTicks = 600;
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			LocalTargetInfo target = job.GetTarget(PawnInd);
			if (!pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
			return true;
		}
		private Pawn Target => (Pawn)job.GetTarget(TargetIndex.A).Thing;

		private int curDuration;
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
			Toil toil = new Toil();
			toil.tickAction = delegate
			{
				Pawn victim = Target;
				pawn.rotationTracker.FaceTarget(victim);
				Map map = pawn.Map;
				if (!pawn.pather.Moving)
				{
					IntVec3 intVec = IntVec3.Invalid;
					if (pawn.Position.DistanceTo(Target.Position) > 1.42f)
                    {
						for (int i = 0; i < 9 && (i != 8 || !intVec.IsValid); i++)
						{
							IntVec3 intVec2 = victim.Position + GenAdj.AdjacentCellsAndInside[i];
							if (intVec2.InBounds(map) && intVec2.Walkable(map) && intVec2 != pawn.Position && pawn.CanReach(intVec2, PathEndMode.OnCell, Danger.Deadly)
							&& (!intVec.IsValid || pawn.Position.DistanceToSquared(intVec2) < pawn.Position.DistanceToSquared(intVec)))
							{
								intVec = intVec2;
							}
						}
						if (intVec.IsValid)
						{
							pawn.pather.StartPath(intVec, PathEndMode.OnCell);
						}
						else
                        {
							Log.Message("Ending");
							this.EndJobWith(JobCondition.Incompletable);
                        }
					}
					else
					{
						DoInfestingTick();
					}
				}

				if (curDuration >= WorkTimeTicks)
				{
					//pawn.needs.food.CurLevelPercentage -= 0.1f;
					var hediff = HediffMaker.MakeHediff(AnnelitriceDefOf.Annely_Infection, Target);
					Target.health.AddHediff(hediff);
					Target.jobs.StopAll();
					pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
				}

			};
			toil.handlingFacing = true;
			toil.socialMode = RandomSocialMode.Off;
			toil.defaultCompleteMode = ToilCompleteMode.Never;
			toil.WithProgressBar(PawnInd, () => (float)curDuration / (float)WorkTimeTicks, true);
			yield return toil;
		}

		private void DoInfestingTick()
		{
			Pawn friend = Target;
			if (friend.CurJobDef != AnnelitriceDefOf.Annely_StandStill)
			{
				Job job = JobMaker.MakeJob(AnnelitriceDefOf.Annely_StandStill, pawn);
				friend.jobs.TryTakeOrderedJob(job);
				friend.pather.StopDead();
			}
			pawn.GainComfortFromCellIfPossible();
			curDuration++;
		}
	}
}
