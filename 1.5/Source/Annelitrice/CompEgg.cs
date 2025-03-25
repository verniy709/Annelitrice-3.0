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
            if (parent.def.defName == "Anneli_Pupa")
            {
                eggTime++;
                if (eggTime > SpawnTime)
                {
                    Hatch_Pupa();
                }
            }
        }
		public void Hatch()
		{
			CompContainPawn owner = parent.GetComp<CompContainPawn>();
			if (owner == null)
			{
				Log.Message("No contained pawn found.");
				return;
			}

			Pawn containedPawn = owner.GetDirectlyHeldThings().FirstOrDefault() as Pawn;

			if (containedPawn == null)
			{
				Messages.Message("NoContainedPawn_Annelitrice".Translate(), MessageTypeDefOf.NeutralEvent, true);
				parent.Destroy();
				return;
			}

			if (parent.def.defName == "Anneli_Pupa")
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
                ResurrectionUtility.TryResurrectWithSideEffects(pawn);
            }
            if (pawn.Faction != Faction.OfPlayer)
            {
                pawn.SetFaction(Faction.OfPlayer);
            }
            onwer.GetDirectlyHeldThings().TryDrop(pawn,pos, map, ThingPlaceMode.Near,1,out _);
			DefDatabase<EffecterDef>.GetNamed("Anneli_PupationFinish").Spawn(pos, map);
			parent.Destroy();
        }
        private void Hatch_Egg()
        {
            IntVec3 pos = parent.Position;
            Map map = parent.Map;
            Pawn pawn =PawnGenerator.GeneratePawn(PawnKindDef.Named("AnnelitriceLarvaAsAnimal"),Faction.OfPlayer);
            parent.GetComp<CompContainPawn>().GetDirectlyHeldThings().TryTransferAllToContainer(pawn.GetComp<CompContainPawn>().GetDirectlyHeldThings());
            GenSpawn.Spawn(pawn, pos, map, WipeMode.VanishOrMoveAside);
			parent.Destroy();
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref eggTime, "eggTime");
		}
        private int eggTime;
        public const int SpawnTime = 30000;

		public static ThingWithComps MakeEgg(Pawn pawn)
        {
            ThingWithComps egg;
            
            if (pawn.Faction != null && pawn.Faction.IsPlayer)
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
			Faction AnneliWildFaction = Find.FactionManager.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamed("Anneli_Faction_Wild"));
			ThingWithComps egg = ThingMaker.MakeThing(ThingDef.Named("Anneli_OutsiderEgg")) as ThingWithComps;
			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDef.Named("Anneli_Wild"), AnneliWildFaction, PawnGenerationContext.PlayerStarter, tile: -1, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: false, mustBeCapableOfViolence: false, colonistRelationChanceFactor: 0f, forceAddFreeWarmLayerIfNeeded: false));
			pawn.ideo.SetIdeo(Faction.OfPlayer.ideos.PrimaryIdeo);
			egg.TryGetComp<CompContainPawn>().GetDirectlyHeldThings().TryAdd(pawn);
            return egg;
        }
    }
}
