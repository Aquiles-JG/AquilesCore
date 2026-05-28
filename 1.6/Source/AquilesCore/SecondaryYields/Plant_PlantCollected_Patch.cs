using HarmonyLib;
using RimWorld;
using Verse;

namespace AquilesCore
{
    [HarmonyPatch(typeof(Plant), "PlantCollected")]
    public static class Plant_PlantCollected_Patch
    {
        public static void Prefix(Plant __instance, Pawn by)
        {
            if (__instance.LifeStage == PlantLifeStage.Mature)
            {
                SecondaryYieldsHelper.SpawnSecondaryYields(__instance.def, __instance.Position, __instance.Map, by);
            }
        }
    }
}
