using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Annelitrice
{
    public class IngestionOutcomeDoer_AnneliAnthelmintic : IngestionOutcomeDoer
    {
        private static readonly HashSet<string> CurableDiseaseNames = new HashSet<string>(StringComparer.Ordinal)
        {
            "Flu",
            "Plague",
            "Malaria",
            "GutWorms",
            "SleepingSickness",
            "MuscleParasites",
            "FibrousMechanites",
            "SensoryMechanites"
        };

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            // Removing a hediff changes the pawn's live list, so inspect a snapshot.
            List<Hediff> hediffs = new List<Hediff>(pawn.health.hediffSet.hediffs);
            foreach (Hediff hediff in hediffs)
            {
                if (hediff is Hediff_Addiction
                    || CurableDiseaseNames.Contains(hediff.def.defName)
                    || hediff.def.defName == "Anneli_AnthelminticHigh")
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }

            if (pawn.def == DefDatabase<ThingDef>.GetNamed("Annelitrice"))
            {
                return;
            }

            HediffDef foodPoisoningDef = DefDatabase<HediffDef>.GetNamed("FoodPoisoning");
            float rolledSeverity = Rand.Range(8f, 24f) / 24f;
            Hediff existingPoisoning = pawn.health.hediffSet.GetFirstHediffOfDef(foodPoisoningDef);
            if (existingPoisoning != null)
            {
                // Vanilla food poisoning loses one severity per day.
                // Keep whichever duration is longer if the pawn was already poisoned.
                if (rolledSeverity > existingPoisoning.Severity)
                {
                    existingPoisoning.Severity = rolledSeverity;
                }

                return;
            }

            Hediff foodPoisoning = HediffMaker.MakeHediff(foodPoisoningDef, pawn);
            foodPoisoning.Severity = rolledSeverity;
            pawn.health.AddHediff(foodPoisoning);
        }
    }
}