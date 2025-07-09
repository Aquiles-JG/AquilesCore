using HarmonyLib;
using RimWorld;
using System;
using System.Linq;
using Verse;
using Verse.Noise;

namespace SecondaryYields
{
    public class YieldOutput
    {
        public IntRange quantityRange;

        private ThingDef thingDef;
        private ThingCategoryDef thingCategory;

        public ThingDef GetThingDef()
        {
            ThingDef defToMake = thingDef;
            if (thingCategory != null)
            {
                var candidates = thingCategory.DescendantThingDefs;
                if (candidates.Any())
                {
                    defToMake = candidates.RandomElement();
                }
            }
            return defToMake;
        }
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
                    Thing thing2 = ThingMaker.MakeThing(yieldOutput.GetThingDef());
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
