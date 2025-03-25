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
using rjw;
using rjw.RenderNodeWorkers;

namespace Annelitrice.RJWHarmonyPatches
{
    [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_DrawNude), nameof(PawnRenderNodeWorker_Apparel_DrawNude.CanDrawNowSub))]
    public static class Annelitrice_Render_Patch
    {
        public static bool Prefix(PawnRenderNode node, PawnDrawParms parms, ref bool __result)
        {
            if (parms.pawn != null && parms.pawn.def.defName == "Annelitrice")
            {
                __result = true;
                return false;      
            }

            return true;
        }
    }
}
