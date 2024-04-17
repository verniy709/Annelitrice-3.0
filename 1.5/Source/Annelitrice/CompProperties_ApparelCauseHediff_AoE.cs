using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Annelitrice
{
	public class CompProperties_ApparelCauseHediff_AoE : CompProperties
	{
		public CompProperties_ApparelCauseHediff_AoE()
		{
			this.compClass = typeof(CompApparelCauseHediff_AoE);
		}

		public HediffDef hediff;

		public BodyPartDef part;

		public float range;

		public bool onlyTargetMechs;

		public bool ignoreMechs;

		public int checkInterval = 10;

		public SoundDef activeSound;

		public bool canTargetSelf = true;

		public bool drawLines = true;
	}
}
