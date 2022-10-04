using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Annelitrice
{
	//play music system
    public class CompProperties_PlayMusic : CompProperties
    {
        public List<SoundDef> soundDefs;
        public ThoughtDef playerThought;
        public ThoughtDef audienceThought;
        public float affectRadius = 3;
        public List<EffecterDef> effecters;
        public CompProperties_PlayMusic()
        {
            this.compClass = typeof(CompPlayMusic);
        }
    }
    public class CompPlayMusic : ThingComp
    {
        public SoundDef curSoundDef;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                if (curSoundDef is null)
                {
                    curSoundDef = Props.soundDefs.First();
                }
            }
        }
        public CompProperties_PlayMusic Props => base.props as CompProperties_PlayMusic;
        public Pawn Player => (this.parent.ParentHolder as Pawn_ApparelTracker)?.pawn;

        public Sustainer curSustainer;

        private int nextEffecterSpawn;

        private Effecter effecter;
        public override void CompTick()
        {
            base.CompTick();
            var player = Player;
            if (curSustainer != null)
            {
                if (!curSustainer.Ended)
                {
                    curSustainer.Maintain();

                    if (!this.Props.effecters.NullOrEmpty() && Find.TickManager.TicksGame > nextEffecterSpawn)
                    {
                        nextEffecterSpawn = Find.TickManager.TicksGame + Rand.RangeInclusive(40, 80);
                        if (effecter != null)
                        {
                            effecter.Cleanup();
                            effecter = null;
                        }
                        effecter = this.Props.effecters.RandomElement().Spawn();
                        effecter.Trigger(player, player);
                    }

                    if (effecter != null)
                    {
                        effecter.EffectTick(player, player);
                    }
                }
                else
                {
                    curSustainer = null;
                    effecter = null;
                }
            }


        }
        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (var g in base.CompGetWornGizmosExtra())
            {
                yield return g;
            }

            yield return new Command_Action()
            {
                defaultLabel = "Annely.PlaySong".Translate(GetSongName(curSoundDef)),
                defaultDesc = "Annely.PlaySongDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("Anneli_Music/Play_Song"),
                action = delegate ()
                {
                    var player = Player;
                    var info = SoundInfo.InMap(new TargetInfo(player.Position, player.Map), MaintenanceType.PerTick);
                    if (curSustainer != null && !curSustainer.Ended)
                    {
                        curSustainer.End();
                        effecter = null;
                    }
                    curSustainer = curSoundDef.TrySpawnSustainer(info);
                    if (this.Props.playerThought != null)
                    {
                        player.needs.mood.thoughts.memories.TryGainMemory(this.Props.playerThought);
                    }
                    if (this.Props.audienceThought != null) 
                    {
                        foreach (var thing in GenRadial.RadialDistinctThingsAround(player.Position, player.Map, this.Props.affectRadius, true))
                        {
                            if (thing is Pawn pawn && pawn != player && pawn.RaceProps.Humanlike)
                            {
                                pawn.needs.mood.thoughts.memories.TryGainMemory(this.Props.audienceThought);
                            }
                        }
                    }
                }
            };

            yield return new Command_Action()
            {
                defaultLabel = "Annely.StopSong".Translate(),
                defaultDesc = "Annely.StopSongDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("Anneli_Music/Stop_Song"),
                action = delegate ()
                {
                    if (curSustainer != null && !curSustainer.Ended)
                    {
                        curSustainer.End();
                        effecter = null;
                    }
                }
            };

            yield return new Command_Action()
            {
                defaultLabel = "Annely.SelectSong".Translate(),
                defaultDesc = "Annely.SelectSongDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("Anneli_Music/Select_Song"),
                action = delegate ()
                {
                    var floatList = new List<FloatMenuOption>();
                    foreach (var song in this.Props.soundDefs)
                    {
                        floatList.Add(new FloatMenuOption(GetSongName(song), delegate ()
                        {
                            curSoundDef = song;
                        }));
                    }
                    var floatMenu = new FloatMenu(floatList);
                    Find.WindowStack.Add(floatMenu);
                }
            };
        }

        private string GetSongName(SoundDef soundDef)
        {
            foreach (var subSound in soundDef.subSounds)
            {
                foreach (var grain in subSound.grains)
                {
                    if (grain is AudioGrain_Clip clip)
                    {
                        return clip.clipPath;
                    }
                }
            }
            return null;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref curSoundDef, "curSoundDef");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (curSoundDef is null)
                {
                    curSoundDef = Props.soundDefs.First();
                }
            }
        }
    }
}
