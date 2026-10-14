using RimWorld;
using Verse;

namespace Annelitrice
{
    public class CompConnaturalAbilities : ThingComp
    {
        public CompProperties_ConnaturalAbilities Props => props as CompProperties_ConnaturalAbilities;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            RestoreAbilities();
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
