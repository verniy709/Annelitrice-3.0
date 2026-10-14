using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Annelitrice
{
	public class CompRegeneration : ThingComp
	{
		private Dictionary<Hediff_MissingPart, int> missingParts = new Dictionary<Hediff_MissingPart, int>();

		private void PreInit()
		{
			if (missingParts is null)
			{
				missingParts = new Dictionary<Hediff_MissingPart, int>();
			}
		}
		public CompRegeneration()
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
					__pawn = parent as Pawn;
				}
				return __pawn;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref nextHealTick, "nextHealTick");

			Scribe_Collections.Look(ref missingParts, "missingParts", LookMode.Reference, LookMode.Value, ref missingPartsKeys, ref intValues);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				PreInit();
				missingParts.RemoveAll(x => x.Key.pawn != pawn || !pawn.health.hediffSet.hediffs.Contains(x.Key));
			}
		}

		private int nextHealTick;
		private int GetNextHealTick
		{
			get
			{
				var baseValue = 250;
				return baseValue;
			}
		}

		private const float MinFoodToHeal = 0.1f;
		private const float FoodCostPerHeal = 0.01f;
		private const float FoodCostPerMissingPart = 0.3f;

		private bool TryConsumeFood(float cost)
		{
			if (pawn?.needs?.food == null)
				return false;

			Need_Food food = pawn.needs.food;
			if (food.CurLevel < MinFoodToHeal)
				return false;

			float newFoodLevel = Mathf.Max(food.CurLevel - cost, 0f);
			food.CurLevel = newFoodLevel;

			return true;
		}

		private bool TryConsumeFoodForHealInjury()
		{
			return TryConsumeFood(FoodCostPerHeal);
		}

		private bool TryConsumeFoodForMissingPart()
		{
			return TryConsumeFood(FoodCostPerMissingPart);
		}

		public override void CompTick()
		{
			base.CompTick();

			if (pawn.IsShambler)
			{
				return;
			}
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

				// Try restoring missing parts
				foreach (var missingPart in missingParts.InRandomOrder().ToList())
				{
					if (Find.TickManager.TicksGame >= missingPart.Value)
					{
						if (!TryConsumeFoodForMissingPart())
						{
							return;
						}

						var part = missingPart.Key.Part;
						pawn.health.RestorePart(part);
						var injury = HediffMaker.MakeHediff(AnnelitriceDefOf.Anneli_Regeneration, pawn, part);
						injury.Severity = part.def.GetMaxHealth(pawn) - 1;
						pawn.health.AddHediff(injury);
						missingParts.Remove(missingPart.Key);
						return;
					}
				}

				// Try heal injuries
				foreach (var part in pawn.health.hediffSet.GetInjuredParts().InRandomOrder())
				{
					var injuries = pawn.health.hediffSet.hediffs
						.Where(x => x is Hediff_Injury && x.Part == part)
						.Cast<Hediff_Injury>();

					if (injuries.Any())
					{
						if (!TryConsumeFoodForHealInjury())
						{
							return;
						}

						if (injuries.TryRandomElement(out var hediff))
						{
							if (hediff.IsPermanent())
							{
								hediff.Severity -= 0.1f;
							}
							else
							{
								hediff.Heal(2.0f);
							}

							if (hediff.Severity <= 0.1f)
							{
								pawn.health.RemoveHediff(hediff);
							}
							return;
						}
					}
				}
			}
		}

		private List<Hediff_MissingPart> missingPartsKeys;
		private List<int> intValues;
	}
}
