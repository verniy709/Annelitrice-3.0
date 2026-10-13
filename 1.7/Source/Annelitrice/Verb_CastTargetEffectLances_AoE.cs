using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace Annelitrice
{
	public class VerbProperties_CastTargetEffectLances_AoE : VerbProperties
	{
		public float radius = 4.9f;

		public VerbProperties_CastTargetEffectLances_AoE()
		{
			verbClass = typeof(Verb_CastTargetEffectLances_AoE);
		}
	}

	public class Verb_CastTargetEffectLances_AoE : Verb_CastTargetEffectLances
	{
		private float Radius
		{
			get
			{
				if (verbProps is VerbProperties_CastTargetEffectLances_AoE props)
					return props.radius;

				return 4.9f;
			}
		}

		public override void DrawHighlight(LocalTargetInfo target)
		{
			base.DrawHighlight(target);

			if (caster?.Map == null) return;

			GenDraw.DrawRadiusRing(target.Cell, Radius);
		}

		protected override bool TryCastShot()
		{
			Map map = caster.Map;
			if (map == null)
				return false;

			IntVec3 center = currentTarget.Cell;
			Pawn casterPawn = caster as Pawn;

			IEnumerable<Thing> things =
				GenRadial.RadialDistinctThingsAround(
					center,
					map,
					Radius,
					useCenter: true
				);

			foreach (Thing t in things)
			{
				Pawn targetPawn = t as Pawn;

				if (targetPawn == null)
					continue;

				if (!ValidateAoETarget(targetPawn))
					continue;

				foreach (CompTargetEffect effect in EquipmentSource.GetComps<CompTargetEffect>())
				{
					effect.DoEffectOn(casterPawn, targetPawn);
				}
			}

			ReloadableCompSource?.UsedOnce();

			return true;
		}

		private bool ValidateAoETarget(Pawn pawn)
		{
			if (pawn.Dead) return false;
			if (pawn.kindDef != null && pawn.kindDef.isBoss) return false;
			if (pawn.GetStatValue(StatDefOf.PsychicSensitivity) <= 0f) return false;

			foreach (CompTargetEffect cte in EquipmentSource.GetComps<CompTargetEffect>())
			{
				if (!cte.CanApplyOn(pawn)) return false;
			}

			return true;
		}
	}
}
