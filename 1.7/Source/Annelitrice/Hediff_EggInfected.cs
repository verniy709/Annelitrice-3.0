using System.Collections.Generic;
using Verse;

namespace Annelitrice
{
    public class Hediff_EggInfected : HediffWithComps
    {
		public override void PostAdd(DamageInfo? dinfo)
		{
			base.PostAdd(dinfo);

			// If pawn is annelitrice, remove this hediff
			if (pawn != null && pawn.def == AnnelitriceDefOf.Annelitrice)
			{
				pawn.health.RemoveHediff(this);
				return;
			}

			infectedTime = Find.TickManager.TicksGame - 1;
		}
		public override void PostTick()
        {
            base.PostTick();
            if((Find.TickManager.TicksGame - infectedTime) % 60000 == 0 && pawn.Spawned)
            {
                LayEggs();
            }
        }

        private void LayEggs()
        {
            int count = Rand.Range(2, 4);
            for (int i = 0; i < count; i++)
            {
                ThingWithComps x = CompEgg.MakeEgg();
                GenPlace.TryPlaceThing(x, pawn.Position, pawn.Map, ThingPlaceMode.Near);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV:lay eggs",
                    action = delegate ()
                    {
                        LayEggs();
                    }
                };
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref infectedTime, "infectedTime");
        }
        private int infectedTime;
    }
}
