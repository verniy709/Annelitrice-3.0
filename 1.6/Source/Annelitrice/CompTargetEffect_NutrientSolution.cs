using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Annelitrice
{
    public class CompTargetEffect_NutrientSolution : CompTargetEffect
    {
        public override void DoEffectOn(Pawn user, Thing target)
        {
			if (!user.IsColonistPlayerControlled)
			{
				return;
			}
			if (!user.CanReserveAndReach(target, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
			{
				return;
			}
			Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("Anneli_UseNutrientSolution"), target, parent);
			job.count = 1;
			user.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
		}
    }
}
