using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Annelitrice
{
	public class JobDriver_UseNutrientSolution : JobDriver
	{
		private ThingWithComps Target
		{
			get
			{
				return (ThingWithComps)job.GetTarget(TargetIndex.A).Thing;
			}
		}
		private Thing Item
		{
			get
			{
				return job.GetTarget(TargetIndex.B).Thing;
			}
		}
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Target, job, 1, -1, null, errorOnFailed) && pawn.Reserve(Item, job, 1, -1, null, errorOnFailed);
		}
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.B).FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, false, false, true);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
			Toil toil = Toils_General.Wait(600, TargetIndex.None);
			toil.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			toil.FailOnDespawnedOrNull(TargetIndex.A);
			toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			toil.tickAction = delegate ()
			{
				CompUsable compUsable = Item.TryGetComp<CompUsable>();
				if (compUsable != null && warmupMote == null && compUsable.Props.warmupMote != null)
				{
					warmupMote = MoteMaker.MakeAttachedOverlay(Target, compUsable.Props.warmupMote, Vector3.zero, 1f, -1f);
				}
				Mote mote = warmupMote;
				if (mote == null)
				{
					return;
				}
				mote.Maintain();
			};
			yield return toil;
			yield return Toils_General.Do(
				delegate ()
            {
				SoundDefOf.MechSerumUsed.PlayOneShot(SoundInfo.InMap(new TargetInfo(Target.Position, Target.Map), MaintenanceType.None));
				if (Target.TryGetComp<CompEgg>() is CompEgg egg)
				{
					egg.Hatch();
				}
                else
                {
					if(Target is Corpse corpse && (corpse.def.defName == "Corpse_Annelitrice" || corpse.def.defName == "Corpse_AnnelitriceLarvaAsAnimal"))
                    {
						ResurrectionUtility.TryResurrectWithSideEffects(corpse.InnerPawn);
						MoteMaker.MakeStaticMote(corpse.Position, corpse.Map, ThingDefOf.Mote_ResurrectFlash, 1f);
					}
				}
				Item.SplitOff(1).Destroy(DestroyMode.Vanish);
			}
			);
			yield break;
		}
		private Mote warmupMote;
	}
}
