using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

		//Regeneration rate
		private int nextHealTick;
		private int GetNextHealTick
		{
			get
			{
				var baseValue = 250;
				return baseValue;
			}
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
						var injury = HediffMaker.MakeHediff(AnnelitriceDefOf.Anneli_Regeneration, pawn, part);
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

		private List<Hediff_MissingPart> missingPartsKeys;
		private List<int> intValues;
	}
}
