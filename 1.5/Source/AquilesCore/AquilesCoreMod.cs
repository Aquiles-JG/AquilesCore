using HarmonyLib;
using Verse;

namespace AquilesCore
{
    [StaticConstructorOnStartup]
    public static class AquilesCoreMod
    {
        static AquilesCoreMod()
        {
            new Harmony("AquilesCoreMod").PatchAll();
        }
    }
}