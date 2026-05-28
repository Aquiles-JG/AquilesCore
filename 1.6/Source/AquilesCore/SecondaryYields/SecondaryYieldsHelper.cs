using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AquilesCore
{
    public static class SecondaryYieldsHelper
    {
        public static IEnumerable<Thing> GenerateSecondaryYields(ThingDef def)
        {
            var modExtension = def.GetModExtension<SecondaryYieldsExtension>();
            if (modExtension != null && Rand.Chance(modExtension.chanceToSpawnSecondaryOutput) && !modExtension.yieldOutputThings.NullOrEmpty())
            {
                var yieldOutput = modExtension.yieldOutputThings.RandomElement();
                var defToMake = yieldOutput.GetThingDef();
                if (defToMake != null)
                {
                    var thing = ThingMaker.MakeThing(defToMake);
                    thing.stackCount = yieldOutput.quantityRange.RandomInRange;
                    if (thing.stackCount > 0)
                    {
                        yield return thing;
                    }
                }
            }
        }

        public static void ForbidIfNecessary(Thing thing, Pawn pawn)
        {
            if ((pawn == null || !pawn.IsColonist) && thing.def.EverHaulable && !thing.def.designateHaulable)
            {
                thing.SetForbidden(true, false);
            }
        }

        public static void SpawnSecondaryYields(ThingDef def, IntVec3 position, Map map, Pawn pawn)
        {
            foreach (var thing in GenerateSecondaryYields(def))
            {
                GenPlace.TryPlaceThing(thing, position, map, ThingPlaceMode.Near, (t, count) => ForbidIfNecessary(t, pawn));
            }
        }
    }
}
