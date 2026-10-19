using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Annelitrice
{
    public class CompLarva : ThingComp
    {
        public bool AllowPupation => allowPupation;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Toggle
            {
                defaultLabel = "Anneli.LarvaAllowPupation".Translate(),
                defaultDesc = "Anneli.LarvaAllowPupationDesc".Translate(),
                icon = ContentFinder<UnityEngine.Texture2D>.Get("Anneli_Item/Pupa/PupaA"),
                isActive = () => allowPupation,
                toggleAction = () => allowPupation = !allowPupation
            };

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
            if (!allowPupation)
            {
                return;
            }

            accumulatedNutrition += nutrition;
            if(accumulatedNutrition >= SpawnNutrition)
            {
                Cocoon();
            }
        }
		public void Cocoon()
		{
			if (!allowPupation)
			{
				return;
			}

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
            Scribe_Values.Look(ref allowPupation, "allowPupation", true);
        }
        private float accumulatedNutrition;
        private bool allowPupation = true;
        public const int SpawnNutrition = 8;
    }
}
