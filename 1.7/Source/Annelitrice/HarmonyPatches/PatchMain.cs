using HarmonyLib;
using JetBrains.Annotations;
using System.Reflection;
using Verse;

namespace Annelitrice.HarmonyPatches
{
    [UsedImplicitly]
    [StaticConstructorOnStartup]
    public class PatchMain
    {
        static PatchMain()
        {
            var instance = new Harmony("Annelitrice_HarmonyPatches");
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
