using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Annelitrice
{
    public class CompProperties_ConnaturalAbilities : CompProperties
    {
        public CompProperties_ConnaturalAbilities()
        {
            compClass = typeof(CompConnaturalAbilities);
        }
        public List<AbilityDef> abilities = new List<AbilityDef>();
    }
}
