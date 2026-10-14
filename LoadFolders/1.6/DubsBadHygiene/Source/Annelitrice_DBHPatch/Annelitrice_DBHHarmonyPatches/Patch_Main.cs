using HarmonyLib;
using JetBrains.Annotations;
using System.Reflection;
using Verse;

namespace Annelitrice.DBHHarmonyPatches
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
