using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace AquilesCore
{
    [HarmonyPatch(typeof(Thing), "ButcherProducts")]
    public static class Thing_ButcherProducts_Patch
    {
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, Thing __instance)
        {
            foreach (var item in __result)
            {
                yield return item;
            }
            var pawnDef = __instance is Corpse corpse ? corpse.InnerPawn?.def : __instance.def;
            foreach (var secondaryYield in SecondaryYieldsHelper.GenerateSecondaryYields(pawnDef))
            {
                yield return secondaryYield;
            }
        }
    }
}
