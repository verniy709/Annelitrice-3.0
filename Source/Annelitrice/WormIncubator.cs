using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Annelitrice
{
    //public class WormIncubator : Building_Storage
    //{
    //    public const int MaximumEvolutionPoints = 100000;
    //    public int evolutionPoints;
    //    public List<Worm> StoredWorms
    //    {
    //        get
    //        {
    //            List<Worm> worms = new List<Worm>();
    //            foreach (var cell in AllSlotCellsList())
    //            {
    //                worms.AddRange(this.Map.thingGrid.ThingsListAtFast(cell).OfType<Worm>());
    //            }
    //            return worms;
    //        }
    //    }

    //    public float GetFuelConsumptionRate()
    //    {
    //        var storedWorms = this.StoredWorms;
    //        if (storedWorms.Any())
    //        {
    //            return (storedWorms.Count() * 0.1f) / 60000f;
    //        }
    //        return 0;
    //    }
    //    public override void Tick()
    //    {
    //        base.Tick();
    //        if (Find.TickManager.TicksGame % GenDate.TicksPerDay == 0)
    //        {
    //            var evolutionPointsToAdd = StoredWorms.Count;
    //            var freeSpace = MaximumEvolutionPoints - evolutionPoints;
    //            evolutionPoints += Mathf.Min(freeSpace, evolutionPointsToAdd);
    //        }
    //    }

    //    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
    //    {
    //        foreach (var opt in base.GetFloatMenuOptions(selPawn))
    //        {
    //            yield return opt;
    //        }
    //        var comp = selPawn.GetComp<CompEvolution>();
    //        if (comp != null)
    //        {
    //            if (comp.evolutionPoints > 0)
    //            {
    //                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Annely.InjectEPs".Translate(), delegate
    //                {
    //                    int b = 1;
    //                    int to2 = comp.evolutionPoints;
    //                    Dialog_Slider window2 = new Dialog_Slider("Annely.InjectEPsCount".Translate("Annely.EPs".Translate()), 1, to2, delegate (int count)
    //                    {
    //                        Job job = JobMaker.MakeJob(AnnelitriceDefOf.Annely_InjectEPsToWormIncubator, this);
    //                        job.count = count;
    //                        selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
    //                    });
    //                    Find.WindowStack.Add(window2);
    //                }, MenuOptionPriority.High), selPawn, this);
    //            }

    //            if (this.evolutionPoints > 0)
    //            {
    //                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Annely.GainEPs".Translate(), delegate
    //                {
    //                    int b = 1;
    //                    int to2 = this.evolutionPoints;
    //                    Dialog_Slider window2 = new Dialog_Slider("Annely.GainEPsCount".Translate("Annely.EPs".Translate()), 1, to2, delegate (int count)
    //                    {
    //                        Job job = JobMaker.MakeJob(AnnelitriceDefOf.Annely_GainEPsFromWormIncubator, this);
    //                        job.count = count;
    //                        selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
    //                    });
    //                    Find.WindowStack.Add(window2);
    //                }, MenuOptionPriority.High), selPawn, this);
    //            }
    //        }
    //    }
    //    public override string GetInspectString()
    //    {
    //        StringBuilder stringBuilder = new StringBuilder();
    //        stringBuilder.Append(base.GetInspectString());
    //        if (stringBuilder.Length != 0)
    //        {
    //            stringBuilder.AppendLine();
    //        }
    //        stringBuilder.AppendLine("Annely.StoredEvolutionPoints".Translate(evolutionPoints));
    //        return stringBuilder.ToString().TrimEndNewlines();
    //    }
    //    public override void ExposeData()
    //    {
    //        base.ExposeData();
    //        Scribe_Values.Look(ref evolutionPoints, "evolutionPoints");
    //    }
    //}
}
