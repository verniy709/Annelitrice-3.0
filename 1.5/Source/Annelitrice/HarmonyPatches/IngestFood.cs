using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Annelitrice.HarmonyPatches
{
    [StaticConstructorOnStartup]
    public class IngestFood
    {
        [HarmonyPatch(typeof(Thing), "Ingested")]
        static class Ingested_Post
        {
            [HarmonyPostfix]
            static void PostFix(Pawn ingester, float __result)
            {
                ingester.TryGetComp<CompLarva>()?.IngestFood(__result);
            }
        }
    }
}
