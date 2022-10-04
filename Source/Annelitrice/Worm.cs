using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Annelitrice
{
    public class Worm : ThingWithComps
    {
        public Pawn savedPawn;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref savedPawn, "savedPawn", saveDestroyedThings: true);
        }
    }
}
