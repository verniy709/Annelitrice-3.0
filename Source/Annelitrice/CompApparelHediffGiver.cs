using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Annelitrice
{

	public class CompProperties_ApparelHediffGiver : CompProperties
	{
		public CompProperties_ApparelHediffGiver()
		{
			compClass = typeof(CompApparelHediffGiver);
		}

		public HediffDef hediffDef;
		public float severityIncrease = 0f;
		public float radius;
		public int tickRate = 500;
	}

	public class CompApparelHediffGiver : ThingComp
	{
		public CompProperties_ApparelHediffGiver Props
		{
			get
			{
				return (CompProperties_ApparelHediffGiver)props;
			}
		}

		//private int ticksUntilNextUpdate = 0;

		//public override void PostExposeData()
		//{
		//	Scribe_Values.Look(ref ticksUntilNextUpdate, "ticksUntilNextUpdate", 0);
		//	base.PostExposeData();
		//}

		//public override void PostPostMake()
		//{
		//	ticksUntilNextUpdate = Find.TickManager.TicksGame;
		//	base.PostPostMake();
		//}

		public Pawn ApparelUser => (this.parent.ParentHolder as Pawn_ApparelTracker)?.pawn;

		public override void CompTick()
		{
			base.CompTick();

			var apparelUser = ApparelUser;

			//update hediffs every tickRate. Detect apparel user and hediff.
			if (Find.TickManager.TicksGame % Props.tickRate == 0 && apparelUser != null && Props.hediffDef != null)
			{
				//check radius of apparel users
				foreach (var thing in GenRadial.RadialDistinctThingsAround(apparelUser.Position, apparelUser.Map, this.Props.radius, true))
				{
					//check pawns and pawns' factions
					if (thing is Pawn pawn && pawn.Faction==apparelUser.Faction)
					{
						float adjustedSeverity = Props.severityIncrease;

						//pawns have hediffs
						if (pawn.health.hediffSet.HasHediff(Props.hediffDef) && adjustedSeverity > 0f)
						{
							pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef).Severity += adjustedSeverity;
						}

						//pawns have no hediffs
						else if (adjustedSeverity > 0f)
						{
							Hediff hediff = HediffMaker.MakeHediff(Props.hediffDef, pawn);
							hediff.Severity = adjustedSeverity;
							pawn.health.AddHediff(hediff);
						}
					}
				}
			}

		}
	}
}
