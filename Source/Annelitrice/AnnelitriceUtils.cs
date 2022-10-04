using AlienRace;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Annelitrice
{
    public static class AnnelitriceUtils
    {
        private static Dictionary<Pawn, CompEvolution> cachedComps = new Dictionary<Pawn, CompEvolution>();

		public static bool TryGetCompEvolution(this Pawn pawn, out CompEvolution comp)
		{
			if (!cachedComps.TryGetValue(pawn, out comp))
			{
				cachedComps[pawn] = comp = pawn.TryGetComp<CompEvolution>();
			}
			return comp != null;
		}

		//Do face change when dead
		public static List<string> savedDeadFaces;
        public static List<string> savedLivingFaces;
        public static void TryInitHeads(this Pawn pawn)
        {
            var alienDef = pawn.def as ThingDef_AlienRace;
            var list = alienDef.alienRace.generalSettings.alienPartGenerator.aliencrowntypes;
            if (savedDeadFaces is null)
            {
                savedDeadFaces = list.Where(x => x.Contains("DeadFace")).ToList();
            }
            if (savedLivingFaces is null)
            {
                savedLivingFaces = list.Where(x => !x.Contains("DeadFace")).ToList();
            }
        }

		//Not sure what this does
        public static void UpdateGraphics(this Pawn pawn)
        {
            pawn.Drawer.renderer.graphics.ResolveAllGraphics();
            PortraitsCache.SetDirty(pawn);
            PortraitsCache.PortraitsCacheUpdate();
            GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
        }

		//Spawn worms upon body destruction
        public static void TrySpawnWorm(this Pawn pawn)
        {
            var raceExtension = pawn?.def.GetModExtension<RaceExtension>();
            if (raceExtension != null)
            {
                Worm worm;
                if (raceExtension.outsiderSpawnThingAfterDestruction != null && pawn.Faction != Faction.OfPlayer)
                {
                    worm = ThingMaker.MakeThing(raceExtension.outsiderSpawnThingAfterDestruction) as Worm;
                }
                else
                {
                    worm = ThingMaker.MakeThing(raceExtension.colonistSpawnThingAfterDestruction) as Worm;
                }
                worm.savedPawn = pawn;
                GenPlace.TryPlaceThing(worm, pawn.PositionHeld, pawn.MapHeld, ThingPlaceMode.Near);
            }
        }
    }
}
