using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Annelitrice
{
    public class CompProperties_Evolution : CompProperties
    {
        public CompProperties_Evolution()
        {
            this.compClass = typeof(CompEvolution);
        }
    }
    public class CompEvolution : ThingComp
    {
		//public const int MaxBilePoints = 20;
		//public int blueBilePoints;
		//public int greenBilePoints;
		//public int redBilePoints;
		//public const int BilePointCost = 25;
		//public int evolutionPoints;
		//public const int HealingCost = 50;
		//public const int AppendageCost = 200;

		//public Dictionary<BodyPartDef, HediffDef> rightAppendagesActive;
		//public Dictionary<BodyPartDef, HediffDef> leftAppendagesActive;
		//public Dictionary<BodyPartDef, HediffDef> appendagesActive;
		private Dictionary<Hediff_MissingPart, int> missingParts = new Dictionary<Hediff_MissingPart, int>();

		public string livingFace;
		public string deadFace;
		//public bool CanShowFace
		//{
		//    get
		//    {
		//        foreach (var hediff in this.pawn.health.hediffSet.hediffs)
		//        {
		//            var extension = hediff.def.GetModExtension<HediffExtension>();
		//            if (extension != null && extension.hideFace)
		//            {
		//                return false;
		//            }
		//        }
		//        return true;
		//    }
		//}

		//public bool CanShowLegs
		//{
		//    get
		//    {
		//        foreach (var hediff in this.pawn.health.hediffSet.hediffs)
		//        {
		//            var extension = hediff.def.GetModExtension<HediffExtension>();
		//            if (extension != null && extension.hideLegs)
		//            {
		//                return false;
		//            }
		//        }
		//        return true;
		//    }
		//}
		//private Graphic mouthGraphic;
		//public Graphic MouthGraphic
		//{
		//    get
		//    {
		//        if (mouthGraphic is null)
		//        {
		//            Rand.PushState();
		//            Rand.Seed = this.pawn.thingIDNumber;
		//            var path = LoadAllFiles("Anneli_Face/MouthTypes/").RandomElement();
		//            mouthGraphic = GraphicDatabase.Get<Graphic_Multi>(path, ShaderUtility.GetSkinShader(pawn.story.SkinColorOverriden), Vector2.one, pawn.story.SkinColor);
		//            Rand.PopState();
		//        }
		//        return mouthGraphic;
		//    }
		//}

		//private Graphic eyeBrowsGraphic;
		//public Graphic EyeBrowsGraphic
		//{
		//    get
		//    {
		//        if (eyeBrowsGraphic is null)
		//        {
		//            Rand.PushState();
		//            Rand.Seed = this.pawn.thingIDNumber;
		//            var path = LoadAllFiles("Anneli_Face/EyeBrowsTypes/").RandomElement();
		//            eyeBrowsGraphic = GraphicDatabase.Get<Graphic_Multi>(path, ShaderUtility.GetSkinShader(pawn.story.SkinColorOverriden), Vector2.one, pawn.story.hairColor);
		//            Rand.PopState();
		//        }
		//        return eyeBrowsGraphic;
		//    }
		//}

		//private Graphic eyesGraphic;
		//private Graphic deadEyesGraphic;
		//public Graphic EyesGraphic
		//{
		//    get
		//    {
		//        if (pawn.Dead)
		//        {
		//            if (deadEyesGraphic is null)
		//            {
		//                Rand.PushState();
		//                Rand.Seed = this.pawn.thingIDNumber;
		//                var path = LoadAllFiles("Anneli_Face/DeadEyesTypes/").RandomElement();
		//                var randomR = Rand.RangeInclusive(80, 255);
		//                var randomG = Rand.RangeInclusive(80, 255);
		//                var randomB = Rand.RangeInclusive(80, 255);
		//                deadEyesGraphic = GraphicDatabase.Get<Graphic_Multi>(path, ShaderUtility.GetSkinShader(pawn.story.SkinColorOverriden), Vector2.one, new ColorInt(randomR, randomG, randomB).ToColor);
		//                Rand.PopState();
		//            }
		//            return deadEyesGraphic;
		//        }
		//        else
		//        {
		//            if (eyesGraphic is null)
		//            {
		//                Rand.PushState();
		//                Rand.Seed = this.pawn.thingIDNumber;
		//                var path = LoadAllFiles("Anneli_Face/EyesTypes/").RandomElement();
		//                var randomR = Rand.RangeInclusive(80, 255);
		//                var randomG = Rand.RangeInclusive(80, 255);
		//                var randomB = Rand.RangeInclusive(80, 255);
		//                eyesGraphic = GraphicDatabase.Get<Graphic_Multi>(path, ShaderUtility.GetSkinShader(pawn.story.SkinColorOverriden), Vector2.one, new ColorInt(randomR, randomG, randomB).ToColor);
		//                Rand.PopState();
		//            }
		//            return eyesGraphic;
		//        }

		//    }
		//}

		private HashSet<string> LoadAllFiles(string folderPath)
        {
            var list = new HashSet<string>();
            foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading)
            {
                foreach (var f in ModContentPack.GetAllFilesForMod(mod, "Textures/" + folderPath))
                {
                    var path = f.Value.FullName;
                    if (path.EndsWith(".png"))
                    {
                        path = path.Replace("\\", "/");
                        path = path.Substring(path.IndexOf("/Textures/") + 10);
                        path = path.Replace(".png", "");
                        path = Regex.Replace(path, @"(.*)_.*", "$1");
                        list.Add(path);
                    }
                }
            }
            return list;
        }
		//private Graphic rightLegGraphic;
		//public Graphic RightLegGraphic
		//{
		//    get
		//    {
		//        if (rightLegGraphic is null)
		//        {
		//            rightLegGraphic = GraphicDatabase.Get<Graphic_Multi>("Anneli_Body/Naked_RightLeg", ShaderUtility.GetSkinShader(pawn.story.SkinColorOverriden), Vector2.one, pawn.story.SkinColor);
		//        }
		//        return rightLegGraphic;
		//    }
		//}
		//private Graphic leftLegGraphic;
		//public Graphic LeftLegGraphic
		//{
		//    get
		//    {
		//        if (leftLegGraphic is null)
		//        {
		//            leftLegGraphic = GraphicDatabase.Get<Graphic_Multi>("Anneli_Body/Naked_LeftLeg", ShaderUtility.GetSkinShader(pawn.story.SkinColorOverriden), Vector2.one, pawn.story.SkinColor);
		//        }
		//        return leftLegGraphic;
		//    }
		//}
		private void PreInit()
		{
			//if (rightAppendagesActive is null)
			//{
			//	rightAppendagesActive = new Dictionary<BodyPartDef, HediffDef>();
			//}
			//if (leftAppendagesActive is null)
			//{
			//	leftAppendagesActive = new Dictionary<BodyPartDef, HediffDef>();
			//}
			//if (appendagesActive is null)
			//{
			//	appendagesActive = new Dictionary<BodyPartDef, HediffDef>();
			//}
			if (missingParts is null)
			{
				missingParts = new Dictionary<Hediff_MissingPart, int>();
			}
		}
		public CompEvolution()
        {
            PreInit();
        }

        private Pawn __pawn;
        private Pawn pawn
        {
            get
            {
                if (__pawn is null)
                {
                    __pawn = this.parent as Pawn;
                }
                return __pawn;
            }
        }

		public override void PostExposeData()
		{
			base.PostExposeData();
			//Scribe_Values.Look(ref blueBilePoints, "blueBilePoints");
			//Scribe_Values.Look(ref greenBilePoints, "greenBilePoints");
			//Scribe_Values.Look(ref redBilePoints, "redBilePoints");
			//Scribe_Values.Look(ref evolutionPoints, "evolutionPoints");
			Scribe_Values.Look(ref nextHealTick, "nextHealTick");

			Scribe_Values.Look(ref livingFace, "livingFace");
			Scribe_Values.Look(ref deadFace, "deadFace");

			//Scribe_Collections.Look(ref rightAppendagesActive, "rightAppendagesActive", LookMode.Def, LookMode.Def, ref bodyPartKeys1, ref hediffDefValues1);
			//Scribe_Collections.Look(ref leftAppendagesActive, "leftAppendagesActive", LookMode.Def, LookMode.Def, ref bodyPartKeys2, ref hediffDefValues2);
			//Scribe_Collections.Look(ref appendagesActive, "appendagesActive", LookMode.Def, LookMode.Def, ref bodyPartKeys3, ref hediffDefValues3);
			Scribe_Collections.Look(ref missingParts, "missingParts", LookMode.Reference, LookMode.Value, ref missingPartsKeys, ref intValues);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				PreInit();
				missingParts.RemoveAll(x => x.Key.pawn != pawn || !pawn.health.hediffSet.hediffs.Contains(x.Key));
			}
		}

		//Regeneration rate
		private int nextHealTick;
        private int GetNextHealTick
        {
            get
            {
                var baseValue = 250;
                //if (greenBilePoints > 0)
                //{
                //    baseValue -= greenBilePoints * 6;
                //}
                return baseValue;
            }
        }

		//CrownType change for dead alive pawns
        public static void SetAlienHead(Pawn pawn, string head)
        {
            var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
            if (alienComp != null)
            {
                alienComp.crownType = head;
            }
        }


        public static string GetAlienHead(Pawn pawn)
        {
            string sRet = "(unknown)";
            var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
            if (alienComp != null)
            {
                sRet = alienComp.crownType;
            }
            return sRet;
        }

		//Regeneration 
        public override void CompTick()
        {
            base.CompTick();
            foreach (var part in pawn.health.hediffSet.GetMissingPartsCommonAncestors())
            {
                if (!missingParts.ContainsKey(part))
                {
                    missingParts[part] = Find.TickManager.TicksGame + 1200;
                }
            }
            
            if (Find.TickManager.TicksGame >= nextHealTick)
            {
                nextHealTick = Find.TickManager.TicksGame + GetNextHealTick;
                foreach (var missingPart in missingParts.InRandomOrder().ToList())
                {
                    if (Find.TickManager.TicksGame >= missingPart.Value)
                    {
                        var part = missingPart.Key.Part;
                        pawn.health.RestorePart(part);
                        var injury = HediffMaker.MakeHediff(AnnelitriceDefOf.Annely_Regeneration, pawn, part);
                        injury.Severity = part.def.GetMaxHealth(pawn) - 1;
                        pawn.health.AddHediff(injury);
                        missingParts.Remove(missingPart.Key);
                        return;
                    }
                }


                foreach (var part in pawn.health.hediffSet.GetInjuredParts().InRandomOrder())
                {
                    var curHP = pawn.health.hediffSet.GetPartHealth(part);
                    var maxHP = part.def.GetMaxHealth(pawn);
                    if (maxHP > curHP)
                    {
                        var diff = (int)Mathf.Clamp(maxHP - curHP, 1, int.MaxValue);
                        var injuries = pawn.health.hediffSet.hediffs.Where(x => x is Hediff_Injury && x.Part == part);
                        for (var i = 0; i < diff; i++)
                        {
                            var hediffs = injuries.Where(x => x.Severity > 0);
                            if (hediffs.TryRandomElement(out var hediff))
                            {
                                hediff.Heal(1);
                                return;
                            }
                        }
                    }
                }
            }
        }

		//public BodyPartRecord GetRightPart(BodyPartDef bodyPartDef)
		//{
		//    var parts = pawn.health.hediffSet.GetNotMissingParts().Where(x => x.def == bodyPartDef);
		//    if (bodyPartDef == BodyPartDefOf.Arm)
		//    {
		//        return parts.FirstOrDefault(x => x.untranslatedCustomLabel == "right arm");
		//    }
		//    else if (bodyPartDef == BodyPartDefOf.Leg)
		//    {
		//        return parts.FirstOrDefault(x => x.untranslatedCustomLabel == "right leg");
		//    }
		//    return null;
		//}
		//public BodyPartRecord GetLeftPart(BodyPartDef bodyPartDef)
		//{
		//    var parts = pawn.health.hediffSet.GetNotMissingParts().Where(x => x.def == bodyPartDef);
		//    if (bodyPartDef == BodyPartDefOf.Arm)
		//    {
		//        return parts.FirstOrDefault(x => x.untranslatedCustomLabel == "left arm");
		//    }
		//    else if (bodyPartDef == BodyPartDefOf.Leg)
		//    {
		//        return parts.FirstOrDefault(x => x.untranslatedCustomLabel == "left leg");
		//    }
		//    return null;
		//}

		private List<BodyPartDef> bodyPartKeys1;
		private List<HediffDef> hediffDefValues1;

		private List<BodyPartDef> bodyPartKeys2;
		private List<HediffDef> hediffDefValues2;

		private List<BodyPartDef> bodyPartKeys3;
		private List<HediffDef> hediffDefValues3;

		private List<Hediff_MissingPart> missingPartsKeys;
		private List<int> intValues;
	}
}
