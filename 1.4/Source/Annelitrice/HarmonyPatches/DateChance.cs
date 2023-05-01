using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Annelitrice
{
	[StaticConstructorOnStartup]

	//race specific flirt chance increase
	[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "RandomSelectionWeight")]
	public class RandomSelectionWeight_Patch
	{
		[HarmonyPriority(Priority.First)]
		private static bool Prefix(ref float __result, Pawn initiator, Pawn recipient)
		{
			if (initiator.def == AnnelitriceDefOf.Annelitrice)
			{
				__result = RandomSelectionWeight(initiator, recipient);
				return false;
			}
			return true;
		}

		public static float RandomSelectionWeight(Pawn initiator, Pawn recipient)
		{
			if (TutorSystem.TutorialMode)
			{
				return 0f;
			}
			if (LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient))
			{
				return 0f;
			}
			var num = initiator.relations.SecondaryRomanceChanceFactor(recipient);
			if (num < 0.15f)
			{
				return 0f;
			}
			var num2 = 5f;
			var num3 = initiator.relations.OpinionOf(recipient);
			if (num3 < num2)
			{
				return 0f;
			}
			if (recipient.relations.OpinionOf(initiator) < num2)
			{
				return 0f;
			}
			var num4 = 1f;
			if (!new HistoryEvent(initiator.GetHistoryEventForLoveRelationCountPlusOne(), initiator.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo())
			{
				var pawn = LovePartnerRelationUtility.ExistingMostLikedLovePartner(initiator, allowDead: false);
				if (pawn != null)
				{
					float value = initiator.relations.OpinionOf(pawn);
					num4 = Mathf.InverseLerp(50f, -50f, value);
				}
			}
			var num5 = 1f;
			var num6 = Mathf.InverseLerp(0.15f, 1f, num);
			var num7 = Mathf.InverseLerp(num2, 100f, num3);
			var num8 = (initiator.gender == recipient.gender) ? ((!initiator.story.traits.HasTrait(TraitDefOf.Gay) || !recipient.story.traits.HasTrait(TraitDefOf.Gay)) ? 0.15f : 1f) : ((initiator.story.traits.HasTrait(TraitDefOf.Gay) || recipient.story.traits.HasTrait(TraitDefOf.Gay)) ? 0.15f : 1f);
			return 1.15f * num5 * num6 * num7 * num4 * num8;
		}
	}

	//Apparel textures switch patch
	[HarmonyPatch(typeof(PawnGraphicSet))]
	[HarmonyPatch("ResolveApparelGraphics")]
	public class Annelitrice_ApparelHarmonyPatch
	{
		[HarmonyAfter(new string[] { "rimworld.erdelf.alien_race.main" })]
		[HarmonyPriority(390)]
		private static void Postfix(PawnGraphicSet __instance)
		{
			var count = __instance.apparelGraphics.Count();
			for (var i = 0; i < count; i++)
			{
				var comp = __instance.apparelGraphics[i].sourceApparel.GetComp<CompApparelSecialTex>();
				if (comp == null)
				{
					continue;
				}
				var apparel = __instance.apparelGraphics[i].sourceApparel;
				var apparelDef = apparel.def;
				var list = comp.list;
				var data = list.FirstOrDefault(x => __instance.apparelGraphics.Exists(y => y.sourceApparel.def == x.apparelDef));
				if (data != null)
				{
					var shader = data.shader;
					if (shader == null)
					{
						shader = ShaderDatabase.Cutout;
						if (apparelDef.apparel.useWornGraphicMask)
						{
							shader = ShaderDatabase.CutoutComplex;
						}
					}
					var path = data.path + "_" + __instance.pawn.story.bodyType.defName;
					var graphic = GraphicDatabase.Get<Graphic_Multi>(path, shader, apparelDef.graphicData.drawSize, apparel.DrawColor);
					__instance.apparelGraphics[i] = new ApparelGraphicRecord(graphic, apparel);
				}
			}
		}
	}
}
