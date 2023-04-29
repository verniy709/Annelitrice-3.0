using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace Annelitrice
{
    public class CompEgg : ThingComp
    {
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV:Hatch",
                    action = delegate ()
                    {
                        Hatch();
                    }
                };
            }
        }
        public override void CompTick()
        {
            eggTime++;
            if(eggTime > SpawnTime)
            {
                Hatch();
            }
        }
        public void Hatch()
        {
            if(parent.def.defName == "Anneli_Pupa")
            {
                Hatch_Pupa();
            }
            else
            {
                Hatch_Egg();
            }
        }
        private void Hatch_Pupa()
        {
            IntVec3 pos = parent.Position;
            Map map = parent.Map;
            CompContainPawn onwer = parent.GetComp<CompContainPawn>();
            Pawn pawn = onwer.GetDirectlyHeldThings().First() as Pawn;
            if (pawn.Dead)
            {
                ResurrectionUtility.Resurrect(pawn);
            }
            onwer.GetDirectlyHeldThings().TryDrop(pawn,pos, map, ThingPlaceMode.Near,1,out _);
            
            parent.Destroy();
        }
        private void Hatch_Egg()
        {
            IntVec3 pos = parent.Position;
            Map map = parent.Map;
            Pawn pawn =PawnGenerator.GeneratePawn(PawnKindDef.Named("AnnelitriceLarvaAsAnimal"),Faction.OfPlayer);
            parent.GetComp<CompContainPawn>().GetDirectlyHeldThings().TryTransferAllToContainer(pawn.GetComp<CompContainPawn>().GetDirectlyHeldThings());
            GenSpawn.Spawn(pawn, pos, map, WipeMode.VanishOrMoveAside);
            DefDatabase<EffecterDef>.GetNamed("Anneli_PupationFinish").Spawn(pos, map);
            parent.Destroy();
        }
        public override void PostExposeData()
        {
            Scribe_Values.Look(ref eggTime, "eggTime");
        }
        private int eggTime;
        public const int SpawnTime = 600000;

        public static ThingWithComps MakeEgg(Pawn pawn)
        {
            ThingWithComps egg;
            
            if (pawn.Faction.IsPlayer)
            {
                egg = ThingMaker.MakeThing(ThingDef.Named("Anneli_ColonistEgg")) as ThingWithComps;
            }
            else
            {
                egg = ThingMaker.MakeThing(ThingDef.Named("Anneli_OutsiderEgg")) as ThingWithComps;
            }
            egg.TryGetComp<CompContainPawn>().GetDirectlyHeldThings().TryAddOrTransfer(pawn);
            return egg;
        }
        public static ThingWithComps MakeEgg()
        {
            ThingWithComps egg = ThingMaker.MakeThing(ThingDef.Named("Anneli_OutsiderEgg")) as ThingWithComps;
            Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDef.Named("Anneli_Wild"));
            egg.TryGetComp<CompContainPawn>().GetDirectlyHeldThings().TryAdd(pawn);
            return egg;
        }
    }
}
