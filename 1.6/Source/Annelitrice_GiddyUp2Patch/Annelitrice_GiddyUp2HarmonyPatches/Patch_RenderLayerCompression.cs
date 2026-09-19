using System;
using System.Reflection;
using HarmonyLib;
using Verse;
using Annelitrice;
using GiddyUpCore;

namespace Annelitrice.GiddyUp2HarmonyPatches
{
    [HarmonyPatch]
    public static class Patch_RenderLayerCompression
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                "GiddyUpCore.Core.MountedRiderRenderLayerCompression:" +
                "TryGetCompressedAltitude"
            );
        }

        [HarmonyPrefix]
        public static bool Prefix(
            Pawn pawn,
            float sourceLayer,
            ref float altitude,
            ref bool __result)
        {
            if (pawn?.def != AnnelitriceDefOf.Annelitrice)
                return true;

            altitude = 0f;
            __result = false;

            return false;
        }
    }
}
