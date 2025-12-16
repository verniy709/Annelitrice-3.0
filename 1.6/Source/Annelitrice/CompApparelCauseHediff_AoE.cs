using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Annelitrice
{
	public class CompApparelCauseHediff_AoE : ThingComp
	{

		public CompProperties_ApparelCauseHediff_AoE Props
		{
			get
			{
				return (CompProperties_ApparelCauseHediff_AoE)this.props;
			}
		}

		public Pawn ApparelUser => (this.parent.ParentHolder as Pawn_ApparelTracker)?.pawn;

		private bool IsPawnAffected(Pawn target)
		{
			//Added a conditional check for pawn faction
			//Removed the conditional check for PowerTrader
			if (target.Faction != null && ApparelUser != null && target.Faction == ApparelUser.Faction)
			{
				return (!target.Dead && target.health != null && (target != this.parent || this.Props.canTargetSelf) && (!this.Props.ignoreMechs || !target.RaceProps.IsMechanoid) && (!this.Props.onlyTargetMechs || target.RaceProps.IsMechanoid) && target.PositionHeld.DistanceTo(this.parent.PositionHeld) <= this.range);
			}
			return false;
		}

		public override void PostPostMake()
		{
			base.PostPostMake();
			this.range = this.Props.range;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<float>(ref this.range, "range", 0f, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && this.range <= 0f)
			{
				this.range = this.Props.range;
			}
		}

		public override void CompTick()
		{
			this.MaintainSustainer();
			if (!this.parent.IsHashIntervalTick(this.Props.checkInterval))
			{
				return;
			}
			this.lastIntervalActive = false;
			if (!this.parent.SpawnedOrAnyParentSpawned)
			{
				return;
			}
			foreach (Pawn pawn in this.parent.MapHeld.mapPawns.AllPawnsSpawned)
			{
				if (this.parent.MapHeld == null)
				{
					return;
				}
				if (this.IsPawnAffected(pawn))
				{
					this.GiveOrUpdateHediff(pawn);
				}
				Pawn target;
				if ((target = (pawn.carryTracker.CarriedThing as Pawn)) != null && this.IsPawnAffected(target))
				{
					this.GiveOrUpdateHediff(target);
				}
			}
		}

		private void GiveOrUpdateHediff(Pawn target)
		{
			Hediff hediff = target.health.hediffSet.GetFirstHediffOfDef(this.Props.hediff, false);
			if (hediff == null)
			{
				hediff = target.health.AddHediff(this.Props.hediff, target.health.hediffSet.GetBrain(), null, null);
				hediff.Severity = 1f;
				HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
				if (hediffComp_Link != null)
				{
					hediffComp_Link.drawConnection = false;
					hediffComp_Link.other = this.parent;
				}
			}
			HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
			if (hediffComp_Disappears == null)
			{
				Log.ErrorOnce("CompCauseHediff_AoE has a hediff in props which does not have a HediffComp_Disappears", 78945945);
			}
			else
			{
				hediffComp_Disappears.ticksToDisappear = this.Props.checkInterval + 500;
			}
			this.lastIntervalActive = true;
		}

		private void MaintainSustainer()
		{
			if (this.lastIntervalActive && this.Props.activeSound != null)
			{
				if (this.activeSustainer == null || this.activeSustainer.Ended)
				{
					this.activeSustainer = this.Props.activeSound.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(this.parent), MaintenanceType.None));
				}
				this.activeSustainer.Maintain();
				return;
			}
			if (this.activeSustainer != null)
			{
				this.activeSustainer.End();
				this.activeSustainer = null;
			}
		}

		public override void PostDraw()
		{
			if (!this.Props.drawLines)
			{
				return;
			}
			int num = Mathf.Max(this.parent.Map.Size.x, this.parent.Map.Size.y);
			if (Find.Selector.SelectedObjectsListForReading.Contains(this.parent) && this.range < (float)num)
			{
				foreach (Pawn pawn in this.parent.Map.mapPawns.AllPawnsSpawned)
				{
					if (this.IsPawnAffected(pawn))
					{
						GenDraw.DrawLineBetween(pawn.DrawPos, this.parent.DrawPos);
					}
				}
			}
		}

		public float range;

		private Sustainer activeSustainer;

		private bool lastIntervalActive;
	}
}
