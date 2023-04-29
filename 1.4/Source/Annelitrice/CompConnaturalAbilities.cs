using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Annelitrice
{
    public class CompConnaturalAbilities : ThingComp
    {
        public CompProperties_ConnaturalAbilities Props => props as CompProperties_ConnaturalAbilities;
        public override void PostPostMake()
        {
            RestoreAbilities();
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent.Faction != null && parent.Faction.IsPlayer)
            {
                foreach (var x in (parent as Pawn).abilities.abilities)
                {
                    foreach (var y in x.GetGizmos())
                    {
                        yield return y;
                    }
                }
            }

        }
        private void RestoreAbilities()
        {
            if (parent is Pawn pawn)
            {
                if (pawn.abilities == null)
                {
                    pawn.abilities = new Pawn_AbilityTracker(pawn);
                }
                foreach (var x in Props.abilities)
                {
                    pawn.abilities.GainAbility(x);
                }
            }
        }
    }
}
