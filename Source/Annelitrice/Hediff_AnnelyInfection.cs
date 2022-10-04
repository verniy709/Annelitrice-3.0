using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Annelitrice
{
    public class Hediff_AnnelyInfection : HediffWithComps
    {
        public override bool ShouldRemove => base.ShouldRemove;
        public override void Tick()
        {
            base.Tick();
            if (this.Severity >= 1f)
            {
                var pos = this.pawn.Position;
                var map = this.pawn.Map;
                this.pawn.Kill(null, this);
                this.pawn.Corpse.Destroy();
                if (Rand.Chance(0.5f))
                {
                    var meat = ThingMaker.MakeThing(ThingDef.Named("Meat_Megaspider"));
                    meat.stackCount = 50;
                    GenSpawn.Spawn(meat, pos, map);
                }
                else
                {
                    var newPawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(AnnelitriceDefOf.Anneli_Player, Faction.OfPlayer, fixedBiologicalAge: 14, fixedChronologicalAge: 14));
                    newPawn.apparel.DestroyAll();
                    newPawn.inventory.DestroyAll();
                    newPawn.equipment.DestroyAllEquipment();
                    GenSpawn.Spawn(newPawn, pos, map);
                }
                var bloodCount = Rand.RangeInclusive(3, 5);
                for (var i = 0; i < bloodCount; i++)
                {
                    GenSpawn.Spawn(Rand.Bool ? ThingDefOf.Filth_Blood : ThingDef.Named("Filth_BloodInsect"), pos, map);
                }
            }
        }
    }
}
