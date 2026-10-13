using HarmonyLib;
using JetBrains.Annotations;
using System.Reflection;
using Verse;

namespace Annelitrice.GiddyUp2HarmonyPatches
{
    [UsedImplicitly]
    [StaticConstructorOnStartup]
    public class PatchMain
    {
        static PatchMain()
        {
            var instance = new Harmony("Annelitrice_GiddyUp2HarmonyPatches");
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
