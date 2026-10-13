using HarmonyLib;
using Verse;

namespace Annelitrice.HarmonyPatches
{
    [StaticConstructorOnStartup]
    public class IngestFood
    {
        [HarmonyPatch(typeof(Thing), "Ingested")]
        static class Patch_Thing_Ingested
		{
            [HarmonyPostfix]
            static void PostFix(Pawn ingester, float __result)
            {
                ingester.TryGetComp<CompLarva>()?.IngestFood(__result);
            }
        }
    }
}
