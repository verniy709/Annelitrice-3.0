using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Annelitrice
{
	//public class HediffCompProperties_GenerateEvolutionPoints : HediffCompProperties
	//{
	//	public int evolutionPointsPerDay;
	//	public HediffCompProperties_GenerateEvolutionPoints()
	//	{
	//		compClass = typeof(HediffComp_GenerateEvolutionPoints);
	//	}
	//}

	//public class HediffComp_GenerateEvolutionPoints : HediffComp
	//{
	//	HediffCompProperties_GenerateEvolutionPoints Props => base.props as HediffCompProperties_GenerateEvolutionPoints;
	//	public override void CompPostTick(ref float severityAdjustment)
	//	{
	//		base.CompPostTick(ref severityAdjustment);
	//		if ((Find.TickManager.TicksGame % (int)(GenDate.TicksPerDay / (float)Props.evolutionPointsPerDay)) == 0)
 //           {
	//			if (this.Pawn.TryGetCompEvolution(out var comp))
 //               {
	//				comp.evolutionPoints++;
 //               }
 //           }
	//	}
	//}
}