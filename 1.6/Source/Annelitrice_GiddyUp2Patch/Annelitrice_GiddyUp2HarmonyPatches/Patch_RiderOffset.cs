using System.Reflection;
using Annelitrice;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Annelitrice_GiddyUpPatch
{
    [HarmonyPatch]
    public static class GiddyUp_RiderOffset
    {
        private const float NorthZOffset = -0.38f;
        private const float SouthZOffset = -0.38f;
        private const float EastZOffset = -0.38f;
        private const float WestZOffset = -0.38f;

        private const float SouthYOffset = -0.026f;

        static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName(
                "GiddyUpCore.Core.MountedRiderRenderNodeUtility"
            );

            return AccessTools.Method(
                type,
                "GetMountedRiderOffset",
                new[]
                {
                    typeof(Pawn),
                    typeof(Pawn),
                    typeof(Rot4)
                }
            );
        }

        [HarmonyPostfix]
        public static void Postfix(
            Pawn rider,
            Pawn mount,
            Rot4 rotation,
            ref Vector3 __result)
        {
            if (rider?.def != AnnelitriceDefOf.Annelitrice)
                return;

            if (rotation == Rot4.North)
            {
                __result.z += NorthZOffset;
            }
            else if (rotation == Rot4.South)
            {
                __result.z += SouthZOffset;
                __result.y += SouthYOffset;
            }
            else if (rotation == Rot4.East)
            {
                __result.z += EastZOffset;
            }
            else if (rotation == Rot4.West)
            {
                __result.z += WestZOffset;
            }
        }
    }
}