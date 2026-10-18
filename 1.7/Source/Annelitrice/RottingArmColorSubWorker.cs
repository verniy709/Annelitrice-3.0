using UnityEngine;
using Verse;

namespace Annelitrice
{
    public class PawnRenderSubWorker_RottingArmColor : PawnRenderSubWorker
    {
        public override void EditMaterialPropertyBlock(PawnRenderNode node, Material material, PawnDrawParms parms, ref MaterialPropertyBlock block)
        {
            if (block == null || material == null || parms.Statue || parms.rotDrawMode != RotDrawMode.Rotting ||
                parms.pawn?.def?.defName != "Annelitrice")
            {
                return;
            }

            block.SetColor(ShaderPropertyIDs.Color,
                PawnRenderUtility.GetRottenColor(material.color) * parms.tint);
        }
    }
}
