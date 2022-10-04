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
 //   public class JobDriver_GrowAppendage : JobDriver
 //   {
	//	public const int WorkTimeTicks = 200;
	//	public override bool TryMakePreToilReservations(bool errorOnFailed)
	//	{
	//		return true;
	//	}
	//	protected override IEnumerable<Toil> MakeNewToils()
	//	{
	//		yield return Toils_General.Wait(WorkTimeTicks, TargetIndex.A)
	//			.WithProgressBarToilDelay(TargetIndex.A)
	//			.WithEffect(AnnelitriceDefOf.Anneli_Effecter_SpurtBlood, () => pawn).PlaySustainerOrSound(AnnelitriceDefOf.Anneli_Sound_SpurtBlood);
	//		yield return Toils_General.Do(delegate
	//		{
	//			var comp = pawn.GetComp<CompEvolution>();
	//			if (comp.appendagesActive != null)
 //               {
	//				foreach (var appendage in comp.appendagesActive)
	//				{
	//					var part = pawn.health.hediffSet.GetNotMissingParts().First(x => x.def == appendage.Key);
	//					if (part != null)
 //                       {
	//						pawn.health.RestorePart(part);
 //                       }
	//					var hediff = HediffMaker.MakeHediff(appendage.Value, pawn, part);
	//					pawn.health.AddHediff(hediff, part);
	//				}
	//				comp.appendagesActive.Clear();
	//			}
	//			if (comp.leftAppendagesActive != null)
 //               {
	//				foreach (var appendage in comp.leftAppendagesActive)
	//				{
	//					var part = comp.GetLeftPart(appendage.Key);
	//					var hediff = HediffMaker.MakeHediff(appendage.Value, pawn, part);
	//					pawn.health.AddHediff(hediff, part);
	//				}
	//				comp.leftAppendagesActive.Clear();
	//			}
	//			if (comp.rightAppendagesActive != null)
	//			{
	//				foreach (var appendage in comp.rightAppendagesActive)
	//				{
	//					var part = comp.GetRightPart(appendage.Key);
	//					var hediff = HediffMaker.MakeHediff(appendage.Value, pawn, part);
	//					pawn.health.AddHediff(hediff, part);
	//				}
	//				comp.rightAppendagesActive.Clear();
	//			}
	//		});
	//	}
	//}
}
