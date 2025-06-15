using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Verse;

namespace Annelitrice
{
    public class CompLarva : ThingComp
    {
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV:Cocoon",
                    action = delegate ()
                    {
                        Cocoon();
                    }
                };
            }
        }
        public void IngestFood(float nutrition)
        {
            accumulatedNutrition += nutrition;
            if(accumulatedNutrition >= SpawnNutrition)
            {
                Cocoon();
            }
        }
		public void Cocoon()
		{
			IntVec3 pos = parent.Position;
			Map map = parent.Map;
			DefDatabase<EffecterDef>.GetNamed("Anneli_PupationStart").Spawn(pos, map);
			ThingWithComps pupa = ThingMaker.MakeThing(ThingDef.Named("Anneli_Pupa")) as ThingWithComps;
			CompContainPawn larva = parent.GetComp<CompContainPawn>();

			if (larva.GetDirectlyHeldThings().FirstOrDefault() is Pawn containedPawn)
			{
				CompContainPawn pupaContainer = pupa.GetComp<CompContainPawn>();
				larva.GetDirectlyHeldThings().TryTransferAllToContainer(pupaContainer.GetDirectlyHeldThings());
				GenSpawn.Spawn(pupa, pos, map, WipeMode.VanishOrMoveAside);
			}

			parent.Destroy();
		}
		public override void PostExposeData()
        {
            Scribe_Values.Look(ref accumulatedNutrition, "accumulatedNutrition");
        }
        private float accumulatedNutrition;
        public const int SpawnNutrition = 8;
    }
}
