using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace Annelitrice
{
	public class Verb_CastTargetEffectLances_AoE : Verb_CastTargetEffectLances
	{
		public float radius = 6.9f;

		public override void DrawHighlight(LocalTargetInfo target)
		{
			base.DrawHighlight(target);

			if (caster?.Map == null) return;

			GenDraw.DrawRadiusRing(target.Cell, radius);
		}

		protected override bool TryCastShot()
		{
			Map map = caster.Map;
			if (map == null) return false;

			IntVec3 center = currentTarget.Cell;
			Pawn casterPawn = caster as Pawn;

			IEnumerable<Thing> things =
				GenRadial.RadialDistinctThingsAround(center, map, radius, useCenter: true);

			foreach (Thing t in things)
			{
				Pawn targetPawn = t as Pawn;
				if (targetPawn == null) continue;
				if (!ValidateAoETarget(targetPawn)) continue;

				foreach (CompTargetEffect effect in EquipmentSource.GetComps<CompTargetEffect>())
				{
					effect.DoEffectOn(casterPawn, targetPawn);
				}
			}

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
