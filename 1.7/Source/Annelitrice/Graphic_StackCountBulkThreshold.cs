using UnityEngine;
using Verse;

namespace Annelitrice
{
    public abstract class Graphic_StackCountBulkThreshold : Graphic_StackCount
    {
        protected abstract int BulkThreshold { get; }

        public override Graphic SubGraphicFor(Thing thing)
        {
            // These graphics have a normal image first and a bulk image second.
            return subGraphics[thing.stackCount > BulkThreshold && subGraphics.Length > 1 ? 1 : 0];
        }
    }

    public class Graphic_NutrientSolutionStackCount : Graphic_StackCountBulkThreshold
    {
        protected override int BulkThreshold => 40;

        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            return GraphicDatabase.Get<Graphic_NutrientSolutionStackCount>(path, newShader, drawSize, newColor, newColorTwo, data);
        }
    }

    public class Graphic_AnthelminticStackCount : Graphic_StackCountBulkThreshold
    {
        protected override int BulkThreshold => 4;

        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            return GraphicDatabase.Get<Graphic_AnthelminticStackCount>(path, newShader, drawSize, newColor, newColorTwo, data);
        }
    }
}
