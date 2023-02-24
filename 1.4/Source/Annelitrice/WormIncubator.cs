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
    //public class eggIncubator : Building_Storage
    //{
    //    public const int MaximumEvolutionPoints = 100000;
    //    public int evolutionPoints;
    //    public List<egg> Storedeggs
    //    {
    //        get
    //        {
    //            List<egg> eggs = new List<egg>();
    //            foreach (var cell in AllSlotCellsList())
    //            {
    //                eggs.AddRange(this.Map.thingGrid.ThingsListAtFast(cell).OfType<egg>());
    //            }
    //            return eggs;
    //        }
    //    }

    //    public float GetFuelConsumptionRate()
    //    {
    //        var storedeggs = this.Storedeggs;
    //        if (storedeggs.Any())
    //        {
    //            return (storedeggs.Count() * 0.1f) / 60000f;
    //        }
    //        return 0;
    //    }
    //    public override void Tick()
    //    {
    //        base.Tick();
    //        if (Find.TickManager.TicksGame % GenDate.TicksPerDay == 0)
    //        {
    //            var evolutionPointsToAdd = Storedeggs.Count;
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
    //                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Anneli.InjectEPs".Translate(), delegate
    //                {
    //                    int b = 1;
    //                    int to2 = comp.evolutionPoints;
    //                    Dialog_Slider window2 = new Dialog_Slider("Anneli.InjectEPsCount".Translate("Anneli.EPs".Translate()), 1, to2, delegate (int count)
    //                    {
    //                        Job job = JobMaker.MakeJob(AnnelitriceDefOf.Anneli_InjectEPsToeggIncubator, this);
    //                        job.count = count;
    //                        selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
    //                    });
    //                    Find.WindowStack.Add(window2);
    //                }, MenuOptionPriority.High), selPawn, this);
    //            }

    //            if (this.evolutionPoints > 0)
    //            {
    //                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Anneli.GainEPs".Translate(), delegate
    //                {
    //                    int b = 1;
    //                    int to2 = this.evolutionPoints;
    //                    Dialog_Slider window2 = new Dialog_Slider("Anneli.GainEPsCount".Translate("Anneli.EPs".Translate()), 1, to2, delegate (int count)
    //                    {
    //                        Job job = JobMaker.MakeJob(AnnelitriceDefOf.Anneli_GainEPsFromeggIncubator, this);
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
    //        stringBuilder.AppendLine("Anneli.StoredEvolutionPoints".Translate(evolutionPoints));
    //        return stringBuilder.ToString().TrimEndNewlines();
    //    }
    //    public override void ExposeData()
    //    {
    //        base.ExposeData();
    //        Scribe_Values.Look(ref evolutionPoints, "evolutionPoints");
    //    }
    //}
}
