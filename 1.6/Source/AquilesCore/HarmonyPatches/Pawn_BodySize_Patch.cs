using HarmonyLib;
using RimWorld;
using Verse;

namespace AquilesCore
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.BodySize), MethodType.Getter)]
    public static class Pawn_BodySize_Patch
    {
        public static void Postfix(Pawn __instance, ref float __result)
        {
            if (!AquilesCoreMod.settings.bodyTypeMatters)
            {
                return;
            }
            var bodyType = __instance.story?.bodyType;
            if (bodyType == null)
            {
                return;
            }
            if (bodyType == BodyTypeDefOf.Fat || bodyType == BodyTypeDefOf.Thin || bodyType == BodyTypeDefOf.Hulk)
            {
                try  // errors upon save load, hence the try catch block
                {
                    __result *= __instance.GetStatValue(DefsOf.Aq_BodySizeFactor, cacheStaleAfterTicks: 150);
                }
                catch
                {

                }
            }
        }
    }
}
