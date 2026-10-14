using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Annelitrice.RJWHarmonyPatches
{
    [UsedImplicitly]
    [StaticConstructorOnStartup]
    public class PatchMain
    {
        static PatchMain()
        {
            var instance = new Harmony("Annelitrice_RJWHarmonyPatches");
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
