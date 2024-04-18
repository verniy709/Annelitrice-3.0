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
	public class PawnRenderSubWorker_NoDraw : PawnRenderSubWorker
	{
		public override bool CanDrawNowSub(PawnRenderNode node, PawnDrawParms parms)
		{
			if (parms.pawn.def.defName == AnnelitriceDefOf.Annelitrice.defName)
			{
				return false;
			}
			return true;
		}

		public PawnRenderSubWorker_NoDraw()
		{
		}
	}
}
