using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;

namespace Annelitrice
{
	public class JobDriver_WatchPuppetShow : JobDriver_WatchBuilding
	{
		public static HashSet<SoundDef> laughingSounds = new HashSet<SoundDef>
		{
			AnnelitriceDefOf.Anneli_Sound_AudienceLaugh1,
			AnnelitriceDefOf.Anneli_Sound_AudienceLaugh2,
			AnnelitriceDefOf.Anneli_Sound_AudienceLaugh3
		};
		protected override void WatchTickAction()
		{
			base.WatchTickAction();
			if (Rand.Chance(0.002f))
            {
				var laughSound = laughingSounds.RandomElement();
				laughSound.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
			}
		}
	}
}
