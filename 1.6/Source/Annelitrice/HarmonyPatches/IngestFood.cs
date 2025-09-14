using HarmonyLib;
using AlienRace;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Annelitrice.HarmonyPatches
{
    [StaticConstructorOnStartup]
    public class IngestFood
    {
        [HarmonyPatch(typeof(Thing), "Ingested")]
        static class Ingested_Patch
        {
            [HarmonyPostfix]
            static void PostFix(Pawn ingester, float __result)
            {
                ingester.TryGetComp<CompLarva>()?.IngestFood(__result);
            }
        }
    }
}
