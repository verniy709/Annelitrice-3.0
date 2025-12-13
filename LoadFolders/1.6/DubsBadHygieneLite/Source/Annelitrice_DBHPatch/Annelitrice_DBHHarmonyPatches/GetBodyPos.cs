using DubsBadHygiene;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse.AI;
using Verse;

namespace Annelitrice.DBHHarmonyPatches
{
    //Add offset to pawn's positions when washing in a bathtub or hot tub
    [HarmonyPatch(typeof(PawnRenderer))]
    [HarmonyPatch("GetBodyPos")]
    public static class Patch_PawnRenderer_GetBodyPos
    {
        public static void Postfix(
            ref Vector3 __result,
            PawnPosture posture,
            Pawn ___pawn)
        {

            Pawn pawn = ___pawn;
            if (pawn == null || pawn.def.defName != "Annelitrice")
                return;

            Job curJob = pawn.CurJob;
            WashingJobDef jobDef = curJob?.def as WashingJobDef;

            bool isWashing = jobDef != null &&
                             pawn.health?.hediffSet?.HasHediff(DubDef.Washing, false) == true;
            if (!isWashing)
                return;

            Thing bath = curJob.targetA.Thing;
            if (!(bath is Building_bath || bath is Building_HotTub))
                return;

            Rot4 rot = bath.Rotation;
            const float offset = 0.4f;

            if (rot == Rot4.North)
            {
                __result.z += offset;
            }
            //Bathtub south looks different
            else if (rot == Rot4.South)
            {
                __result.z -= 0.3f;
            }
            else if (rot == Rot4.East)
            {
                __result.x += offset;
            }
            else if (rot == Rot4.West)
            {
                __result.x -= offset;
            }
        }
    }
}
