using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Annelitrice
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class HotSwappableAttribute : Attribute
	{
	}

	//[HotSwappableAttribute]
	//public class ITab_Pawn_Mimicry : ITab
	//{
	//	private Pawn PawnToShowInfoAbout
	//	{
	//		get
	//		{
	//			Pawn pawn = null;
	//			if (base.SelPawn != null)
	//			{
	//				pawn = base.SelPawn;
	//			}
	//			else
	//			{
	//				Corpse corpse = base.SelThing as Corpse;
	//				if (corpse != null)
	//				{
	//					pawn = corpse.InnerPawn;
	//				}
	//			}
	//			if (pawn == null)
	//			{
	//				Log.Error("ITab_Pawn_Mimicry tab found no selected pawn to display.");
	//				return null;
	//			}
	//			return pawn;
	//		}
	//	}

	//	private CompEvolution compEvolution;

	//	private Pawn pawn;
	//	public override bool IsVisible => PawnToShowInfoAbout.story != null;
	//	public ITab_Pawn_Mimicry()
	//	{
	//		labelKey = "Annely.Mimicry";
	//	}

	//	public Dictionary<BodyPartDef, Rect> bodyPartRects = new Dictionary<BodyPartDef, Rect>();
	//	public Dictionary<BodyPartDef, Rect> leftBodyPartRects = new Dictionary<BodyPartDef, Rect>();
	//	public Dictionary<BodyPartDef, Rect> rightBodyPartRects = new Dictionary<BodyPartDef, Rect>();

	//	public override void OnOpen()
 //       {
 //           base.OnOpen();
	//		pawn = PawnToShowInfoAbout;
	//		compEvolution = pawn.TryGetComp<CompEvolution>();
	//		rightBodyPartRects[BodyPartDefOf.Arm] = new Rect(333, 260, 80, 80);
	//		rightBodyPartRects[BodyPartDefOf.Leg] = new Rect(278, 485, 80, 80);
	//		leftBodyPartRects[BodyPartDefOf.Arm] = new Rect(88, 260, 80, 80);
	//		leftBodyPartRects[BodyPartDefOf.Leg] = new Rect(142, 485, 80, 80);
	//		bodyPartRects[AnnelitriceDefOf.Spine] = new Rect(363, 385, 80, 80);
	//		bodyPartRects[BodyPartDefOf.Torso] = new Rect(58, 385, 80, 80);
	//		bodyPartRects[BodyPartDefOf.Head] = new Rect(211, 205, 80, 80);
	//	}

	//	[TweakValue("AN", 0, 1000)] public static float menuWidth = 500;
	//	[TweakValue("AN", 0, 1000)] public static float menuHeight = 625;
	//	protected override void UpdateSize()
	//	{
	//		base.UpdateSize();
	//		size = new Vector2(menuWidth, menuHeight);
	//	}

	//	[TweakValue("AN", 0, 1000)] public static float redX;
	//	[TweakValue("AN", 0, 1000)] public static float redY;
	//	[TweakValue("AN", 0, 1000)] public static float redW = 80;
	//	[TweakValue("AN", 0, 1000)] public static float redH = 80;

	//	private Vector2 scrollPosition;

	//	private MainTabWindow_Inspect InspectPane => (MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow;

	//	private float GetUIYShift()
 //       {
	//		if (UI.screenHeight == 768)
 //           {
	//			return 90;
 //           }
	//		return 0;
 //       }
	//	protected override void FillTab()
	//	{
	//		UpdateSize();
	//		var yShift = GetUIYShift();
	//		var totalRect = new Rect(0, yShift, size.x, size.y);
	//		var rect2 = totalRect;
	//		rect2.height = size.y + yShift;
	//		Widgets.BeginScrollView(totalRect, ref scrollPosition, rect2);
	//		GUI.DrawTexture(totalRect, Textures.BackgroundMenu);
	//		var curFont = Text.Font;
	//		var curColor = GUI.color;
	//		Text.Font = GameFont.Medium;
	//		GUI.color = Color.black;
			
	//		var redBilePointsRect = new Rect(105, totalRect.y + 45, 70, 35);
	//		Widgets.Label(redBilePointsRect, compEvolution.redBilePoints + " / " + CompEvolution.MaxBilePoints);
			
	//		var greenBilePointsRect = new Rect(redBilePointsRect.x, redBilePointsRect.yMax + 15, redBilePointsRect.width, redBilePointsRect.height);
	//		Widgets.Label(greenBilePointsRect, compEvolution.greenBilePoints + " / " + CompEvolution.MaxBilePoints);
			
	//		var blueBilePointsRect = new Rect(greenBilePointsRect.x, greenBilePointsRect.yMax + 15, greenBilePointsRect.width, greenBilePointsRect.height);
	//		Widgets.Label(blueBilePointsRect, compEvolution.blueBilePoints + " / " + CompEvolution.MaxBilePoints);
			
	//		GUI.color = curColor;
	//		Vector2 point = new Vector2(redBilePointsRect.xMax + 21, redBilePointsRect.y + -14);
			
	//		DoMinusButton(ref point, ref compEvolution.redBilePoints, "Annely.Red".Translate(), Textures.Red_Minus, Textures.Red_Minus_Hovering);
	//		DoPlusButton(ref point, ref compEvolution.redBilePoints, "Annely.Red".Translate(), Textures.Red_Plus, Textures.Red_Plus_Hovering);
			
	//		DoMinusButton(ref point, ref compEvolution.greenBilePoints, "Annely.Green".Translate(), Textures.Green_Minus, Textures.Green_Minus_Hovering);
	//		DoPlusButton(ref point, ref compEvolution.greenBilePoints, "Annely.Green".Translate(), Textures.Green_Plus, Textures.Green_Plus_Hovering);
			
	//		DoMinusButton(ref point, ref compEvolution.blueBilePoints, "Annely.Blue".Translate(), Textures.Blue_Minus, Textures.Blue_Minus_Hovering);
	//		DoPlusButton(ref point, ref compEvolution.blueBilePoints, "Annely.Blue".Translate(), Textures.Blue_Plus, Textures.Blue_Plus_Hovering);
			
	//		var healRect = new Rect(350, totalRect.y + 29, 115, 115);
	//		if (Mouse.IsOver(healRect))
 //           {
	//			GUI.DrawTexture(healRect, Textures.Heal_Button_Hovering);
	//		}
	//		else
 //           {
	//			GUI.DrawTexture(healRect, Textures.Heal_Button);
 //           }
	//		TooltipHandler.TipRegion(healRect, "Annely.HealTooltip".Translate(CompEvolution.HealingCost));
	//		if (healRect.IsLeftClicked())
 //           {
	//			if (compEvolution.evolutionPoints < CompEvolution.HealingCost)
 //               {
	//				Messages.Message("Annely.NotEnoughEPs".Translate(), pawn, MessageTypeDefOf.CautionInput);
	//			}
	//			else
 //               {
	//				compEvolution.evolutionPoints -= CompEvolution.HealingCost;
	//				var missingParts = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
	//				foreach (var part in missingParts)
 //                   {
	//					pawn.health.RestorePart(part.Part);
 //                   }
	//				foreach (var injury in pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>().ToList())
 //                   {
	//					pawn.health.RemoveHediff(injury);
 //                   }
	//			}
	//		}
			
	//		curColor = GUI.color;
	//		GUI.color = Color.black;
			
	//		var evolutionPointsRect = new Rect(350, totalRect.y + 155, 125, 32);
	//		Widgets.Label(evolutionPointsRect, "Annely.EPs".Translate() + ": " + compEvolution.evolutionPoints.ToString());
			
	//		GUI.color = curColor;

	//		//foreach (var bodyPartRect in rightBodyPartRects)
 //           //{
	//		//	var part = compEvolution.GetRightPart(bodyPartRect.Key);
	//		//	var pairs = Textures.rightPairedAppendanges[bodyPartRect.Key];
	//		//	var allAppendages = pairs.Keys.ToList();
	//		//	var activeAppendage = part != null ? GetActiveAppendage(part, allAppendages, compEvolution.rightAppendagesActive) : null;
	//		//	DrawAppendages(bodyPartRect.Value, pairs, activeAppendage, part, allAppendages, compEvolution.rightAppendagesActive);
	//		//}
	//		//
	//		//foreach (var bodyPartRect in leftBodyPartRects)
	//		//{
	//		//	var part = compEvolution.GetLeftPart(bodyPartRect.Key);
	//		//	var pairs = Textures.leftPairedAppendanges[bodyPartRect.Key];
	//		//	var allAppendages = pairs.Keys.ToList();
	//		//	var activeAppendage = part != null ? GetActiveAppendage(part, allAppendages, compEvolution.leftAppendagesActive) : null;
	//		//	DrawAppendages(bodyPartRect.Value, pairs, activeAppendage, part, allAppendages, compEvolution.leftAppendagesActive);
	//		//}
	//		//
	//		//foreach (var bodyPartRect in bodyPartRects)
	//		//{
	//		//	var part = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault(x => x.def == bodyPartRect.Key);
	//		//	var pairs = Textures.pairedAppendanges[bodyPartRect.Key];
	//		//	var allAppendages = pairs.Keys.ToList();
	//		//	var activeAppendage = part != null ? GetActiveAppendage(part, allAppendages, compEvolution.appendagesActive) : null;
	//		//	DrawAppendages(bodyPartRect.Value, pairs, activeAppendage, part, allAppendages, compEvolution.appendagesActive);
	//		//}

	//		Text.Font = curFont;
	//		GUI.color = curColor;
			
	//		Widgets.EndScrollView();
	//	}

	//	private void DrawAppendages(Rect rect, Dictionary<HediffDef, Texture2D> pairs, HediffDef activeAppendage, BodyPartRecord part, List<HediffDef> allAppendages, Dictionary<BodyPartDef, HediffDef> appendagesActive)
 //       {
	//		foreach (var appendages in pairs)
	//		{
	//			if (part != null)
	//			{
	//				if (activeAppendage != null)
	//				{
	//					GUI.DrawTexture(rect, pairs[activeAppendage]);
	//					if (rect.IsLeftClicked())
	//					{
	//						if (compEvolution.evolutionPoints < CompEvolution.AppendageCost)
 //                           {
	//							Messages.Message("Annely.NotEnoughEPs".Translate(), pawn, MessageTypeDefOf.CautionInput);
 //                           }
	//						else
 //                           {
	//							CallFloatMenuForActiveAppendages(part.def, allAppendages.Where(x => x != activeAppendage).ToList(), appendagesActive);
 //                           }
	//					}
	//				}
	//				else
	//				{
	//					if (Mouse.IsOver(rect))
 //                       {
	//						GUI.DrawTexture(rect, Textures.Empty_Slot_Hovering);
	//					}
	//					if (rect.IsLeftClicked())
	//					{
	//						if (compEvolution.evolutionPoints < CompEvolution.AppendageCost)
	//						{
	//							Messages.Message("Annely.NotEnoughEPs".Translate(), pawn, MessageTypeDefOf.CautionInput);
	//						}
	//						else
 //                           {
	//							CallFloatMenuForActiveAppendages(part.def, allAppendages, appendagesActive);
 //                           }
	//					}
	//				}
	//			}
	//			else if (rect.IsLeftClicked())
 //               {
	//				Messages.Message("Annely.BodyPartIsMissing".Translate(), pawn, MessageTypeDefOf.CautionInput);
	//			}
	//		}
	//	}
	//	private void CallFloatMenuForActiveAppendages(BodyPartDef bodyPartDef, List<HediffDef> allAppendages, Dictionary<BodyPartDef, HediffDef> appendagesActive)
 //       {
	//		var floatMenuOptions = new List<FloatMenuOption>();
	//		foreach (var appendage in allAppendages)
	//		{
	//			floatMenuOptions.Add(new FloatMenuOption("CommandInstall".Translate() + " " + appendage.label, delegate
	//			{
	//				appendagesActive[bodyPartDef] = appendage;
	//				compEvolution.evolutionPoints -= CompEvolution.AppendageCost;
	//			}));
	//		}
	//		Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
	//	}
	//	private HediffDef GetActiveAppendage(BodyPartRecord bodyPart, List<HediffDef> allAppendanges, Dictionary<BodyPartDef, HediffDef> appendagesActive)
 //       {
	//		if (appendagesActive.TryGetValue(bodyPart.def, out var hediffDef))
 //           {
	//			return hediffDef;
 //           }
	//		var hediff = pawn.health.hediffSet.hediffs.FirstOrDefault(x => allAppendanges.Contains(x.def) && x.Part == bodyPart);
	//		if (hediff != null)
 //           {
	//			return hediff.def;
 //           }
	//		return null;
	//	}
	//	private void DoMinusButton(ref Vector2 point, ref int bilePointField, string colorName, Texture minusTexture, Texture hoverTexture)
 //       {
	//		var bileMinusButton = new Rect(point.x, point.y, 50, 50);
	//		if (Mouse.IsOver(bileMinusButton))
 //           {
	//			GUI.DrawTexture(bileMinusButton, hoverTexture);
	//		}
	//		else
 //           {
	//			GUI.DrawTexture(bileMinusButton, minusTexture);
	//		}
	//		TooltipHandler.TipRegion(bileMinusButton, "Annely.MinusSignTooltip".Translate(colorName, CompEvolution.BilePointCost));

	//		if (bileMinusButton.IsLeftClicked())
	//		{
	//			if (bilePointField <= 0)
	//			{
	//				Messages.Message("Annely.NoBilePoints".Translate(colorName), pawn, MessageTypeDefOf.CautionInput);
	//			}
	//			else
	//			{
	//				bilePointField -= 1;
	//				compEvolution.evolutionPoints += CompEvolution.BilePointCost;
	//			}
	//		}
	//		point.x = bileMinusButton.xMax + 4;
	//	}

	//	private void DoPlusButton(ref Vector2 point, ref int bilePointField, string colorName, Texture plusTexture, Texture hoverTexture)
	//	{
	//		var bilePlusButton = new Rect(point.x, point.y, 50, 50);
	//		if (Mouse.IsOver(bilePlusButton))
 //           {
	//			GUI.DrawTexture(bilePlusButton, hoverTexture);
	//		}
	//		else
 //           {
	//			GUI.DrawTexture(bilePlusButton, plusTexture);
	//		}
	//		TooltipHandler.TipRegion(bilePlusButton, "Annely.PlusSignTooltip".Translate(CompEvolution.BilePointCost, colorName));

	//		if (bilePlusButton.IsLeftClicked())
	//		{
	//			if (bilePointField == CompEvolution.MaxBilePoints)
	//			{
	//				Messages.Message("Annely.MaxBilePoints".Translate(colorName), pawn, MessageTypeDefOf.CautionInput);
	//			}
	//			else if (compEvolution.evolutionPoints < CompEvolution.BilePointCost)
	//			{
	//				Messages.Message("Annely.NotEnoughEPs".Translate(), pawn, MessageTypeDefOf.CautionInput);
	//			}
	//			else
	//			{
	//				bilePointField += 1;
	//				compEvolution.evolutionPoints -= CompEvolution.BilePointCost;
	//			}
	//		}
	//		point.x -= 54;
	//		point.y += 52;
	//	}
	//}
}
