using HarmonyLib;
using RimWorld;
using System;
using Verse;
using Verse.Noise;

namespace SecondaryYields
{
    public class YieldOutput
    {
        public IntRange quantityRange;

        public ThingDef thingDef;
    }

    [HarmonyPatch(typeof(Plant), "PlantCollected")]
    public static class PlantHarvestPatch
    {
        public static void Prefix(Plant __instance, Pawn by)
        {
            if (__instance.LifeStage == PlantLifeStage.Mature)
            {
                MineableExtension modExtension = __instance.def.GetModExtension<MineableExtension>();
                if (modExtension != null && Rand.Chance(modExtension.chanceToSpawnSecondaryOutput))
                {
                    YieldOutput yieldOutput = modExtension.yieldOutputThings.RandomElement();
                    Thing thing2 = ThingMaker.MakeThing(yieldOutput.thingDef);
                    thing2.stackCount = yieldOutput.quantityRange.RandomInRange;
                    GenPlace.TryPlaceThing(thing2, __instance.Position, __instance.Map, ThingPlaceMode.Near, ForbidIfNecessary);
                }
            }
            void ForbidIfNecessary(Thing thing, int count)
            {
                if ((by == null || !by.IsColonist) && thing.def.EverHaulable && !thing.def.designateHaulable)
                {
                    thing.SetForbidden(value: true, warnOnFail: false);
                }
            }
        }
    }
}
