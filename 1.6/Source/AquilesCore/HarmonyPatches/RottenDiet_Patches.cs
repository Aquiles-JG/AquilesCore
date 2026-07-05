using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using static RimWorld.FoodUtility;

namespace AquilesCore
{
    public static class RottenDiet_Patches
    {
        private const float MaxSearchRadius = 9999f;
        private const float RottenNutritionFactor = 0.5f;
        private const float DessicatedNutritionFactor = 0.2f;
        private const float BoneNutritionValue = 0.01f;
        public static Pawn CurrentEater
        {
            get
            {
                return JobDriver_DriverTick_Patch.eater ?? JobDriver_Notify_Starting_Patch.eater ?? JobDriver_Ingest_TryMakePreToilReservations_Patch.eater ?? JobDriver_CheckCurrentToilEndOrFail_Patch.eater ?? JobDriver_TryActuallyStartNextToil_Patch.eater ?? FoodUtility_TryFindBestFoodSourceFor_Patch.eater ?? FoodUtility_NutritionForEater_Patch.eater;
            }
        }

        [HarmonyPatch(typeof(JobDriver), nameof(JobDriver.DriverTick))]
        public static class JobDriver_DriverTick_Patch
        {
            public static Pawn eater;

            public static void Prefix(JobDriver __instance)
            {
                if (__instance is JobDriver_Ingest) eater = __instance.pawn;
            }

            public static void Finalizer()
            {
                eater = null;
            }
        }

        [HarmonyPatch(typeof(JobDriver), nameof(JobDriver.Notify_Starting))]
        public static class JobDriver_Notify_Starting_Patch
        {
            public static Pawn eater;

            public static void Prefix(JobDriver __instance)
            {
                if (__instance is JobDriver_Ingest) eater = __instance.pawn;
            }

            public static void Finalizer()
            {
                eater = null;
            }
        }

        [HarmonyPatch(typeof(JobDriver_Ingest), nameof(JobDriver_Ingest.TryMakePreToilReservations))]
        public static class JobDriver_Ingest_TryMakePreToilReservations_Patch
        {
            public static Pawn eater;

            public static void Prefix(JobDriver_Ingest __instance)
            {
                eater = __instance.pawn;
            }

            public static void Finalizer()
            {
                eater = null;
            }
        }

        [HarmonyPatch(typeof(JobDriver), nameof(JobDriver.CheckCurrentToilEndOrFail))]
        public static class JobDriver_CheckCurrentToilEndOrFail_Patch
        {
            public static Pawn eater;

            public static void Prefix(JobDriver __instance)
            {
                if (__instance is JobDriver_Ingest) eater = __instance.pawn;
            }

            public static void Finalizer()
            {
                eater = null;
            }
        }

        [HarmonyPatch(typeof(JobDriver), nameof(JobDriver.TryActuallyStartNextToil))]
        public static class JobDriver_TryActuallyStartNextToil_Patch
        {
            public static Pawn eater;

            public static void Prefix(JobDriver __instance)
            {
                if (__instance is JobDriver_Ingest) eater = __instance.pawn;
            }

            public static void Finalizer()
            {
                eater = null;
            }
        }

        [HarmonyPatch(typeof(StatPart_IsCorpseFresh), nameof(StatPart_IsCorpseFresh.TransformValue))]
        public static class StatPart_IsCorpseFresh_TransformValue_Patch
        {
            public static bool Prefix(StatRequest req, ref float val)
            {
                if (req.HasThing && req.Thing is Corpse corpse && corpse.IsNotFresh())
                {
                    var eater = CurrentEater;
                    if (eater != null)
                    {
                        var compRot = corpse.TryGetComp<CompRottable>();
                        if (compRot != null)
                        {
                            if (compRot.Stage == RotStage.Rotting && RottenDietUtility.CanEatRotten(eater))
                            {
                                val *= RottenNutritionFactor;
                                return false;
                            }
                            if (compRot.Stage == RotStage.Dessicated && RottenDietUtility.CanEatBones(eater))
                            {
                                val *= DessicatedNutritionFactor;
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Corpse), nameof(Corpse.IngestibleNow), MethodType.Getter)]
        public static class Corpse_IngestibleNow_Patch
        {
            public static void Postfix(ref bool __result)
            {
                if (__result) return;
                var eater = CurrentEater;
                if (eater != null && (RottenDietUtility.CanEatRotten(eater) || RottenDietUtility.CanEatBones(eater)))
                {
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.TryFindBestFoodSourceFor))]
        public static class FoodUtility_TryFindBestFoodSourceFor_Patch
        {
            public static Pawn eater;

            public static void Prefix(Pawn eater, ref bool allowCorpse)
            {
                if (eater is null) return;
                FoodUtility_TryFindBestFoodSourceFor_Patch.eater = eater;
                var canEatRotten = RottenDietUtility.CanEatRotten(eater);
                var canEatBones = RottenDietUtility.CanEatBones(eater);
                if (canEatBones || canEatRotten) allowCorpse = true;
            }

            public static void Finalizer()
            {
                eater = null;
            }

            public static void Postfix(Pawn getter, Pawn eater, ref Thing foodSource, ref ThingDef foodDef, bool allowForbidden, ref bool __result)
            {
                if (__result || eater == null) return;

                var canEatRotten = RottenDietUtility.CanEatRotten(eater);
                var canEatBones = RottenDietUtility.CanEatBones(eater);
                if (!canEatRotten && !canEatBones)
                {
                    return;
                }

                Thing bestFood = null;

                if (canEatRotten || canEatBones)
                {
                    bestFood = GenClosest.ClosestThingReachable(getter.Position, getter.Map, ThingRequest.ForGroup(ThingRequestGroup.Corpse), PathEndMode.ClosestTouch, TraverseParms.For(getter), MaxSearchRadius, t =>
                    {
                        if (t is not Corpse corpse)
                        {
                            return false;
                        }
                        if (!allowForbidden && corpse.IsForbidden(getter))
                        {
                            return false;
                        }
                        if (!eater.WillEat(corpse, getter, true, false))
                        {
                            return false;
                        }
                        if (!getter.CanReserve(corpse))
                        {
                            return false;
                        }
                        if (RottenDietUtility.IsBlacklistedCorpse(corpse))
                        {
                            return false;
                        }
                        var compRot = corpse.TryGetComp<CompRottable>();
                        if (compRot == null)
                        {
                            return false;
                        }
                        return (compRot.Stage == RotStage.Rotting && canEatRotten) || (compRot.Stage == RotStage.Dessicated && canEatBones);
                    });
                }

                if (canEatBones)
                {
                    var boneDefs = RottenDietUtility.boneDefs;
                    for (var i = 0; i < boneDefs.Count; i++)
                    {
                        var bone = GenClosest.ClosestThingReachable(getter.Position, getter.Map, ThingRequest.ForDef(boneDefs[i]), PathEndMode.ClosestTouch, TraverseParms.For(getter), MaxSearchRadius, t =>
                            (!allowForbidden || !t.IsForbidden(getter))
                            && getter.CanReserve(t)
                            && eater.WillEat(t, getter, true, false));

                        if (bone != null && (bestFood == null || (getter.Position - bone.Position).LengthHorizontalSquared < (getter.Position - bestFood.Position).LengthHorizontalSquared))
                        {
                            bestFood = bone;
                        }
                    }
                }

                if (bestFood != null)
                {
                    foodSource = bestFood;
                    foodDef = FoodUtility.GetFinalIngestibleDef(bestFood);
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.NutritionForEater))]
        public static class FoodUtility_NutritionForEater_Patch
        {
            public static Pawn eater;

            public static void Prefix(Pawn eater)
            {
                FoodUtility_NutritionForEater_Patch.eater = eater;
            }

            public static void Finalizer()
            {
                eater = null;
            }

            public static void Postfix(Pawn eater, Thing food, ref float __result)
            {
                if (RottenDietUtility.IsBoneDef(food.def) && RottenDietUtility.CanEatBones(eater))
                {
                    __result = BoneNutritionValue;
                }
            }
        }

        [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.ThoughtsFromIngesting))]
        public static class FoodUtility_ThoughtsFromIngesting_Patch
        {
            public static void Postfix(Pawn ingester, Thing foodSource, ref List<ThoughtFromIngesting> __result)
            {
                if (foodSource is not Corpse corpse) return;
                var compRot = corpse.TryGetComp<CompRottable>();
                if (compRot == null || compRot.Stage == RotStage.Fresh) return;

                if ((compRot.Stage == RotStage.Rotting && RottenDietUtility.CanEatRotten(ingester))
                    || (compRot.Stage == RotStage.Dessicated && RottenDietUtility.CanEatBones(ingester)))
                {
                    __result.RemoveAll(t => t.thought.stages[0].baseMoodEffect < 0f);
                }
            }
        }

        [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.GetFoodPoisonChanceFactor))]
        public static class FoodUtility_GetFoodPoisonChanceFactor_Patch
        {
            public static void Postfix(Pawn ingester, ref float __result)
            {
                if (__result > 0f && (RottenDietUtility.CanEatRotten(ingester) || RottenDietUtility.CanEatBones(ingester)))
                {
                    __result = 0f;
                }
            }
        }

        [HarmonyPatch(typeof(FloatMenuMakerMap), nameof(FloatMenuMakerMap.GetProviderOptions))]
        public static class FloatMenuMakerMap_GetProviderOptions_Patch
        {
            public static void Prefix(FloatMenuContext context)
            {
                if (context.FirstSelectedPawn != null)
                {
                    JobDriver_DriverTick_Patch.eater = context.FirstSelectedPawn;
                }
            }

            public static void Postfix()
            {
                JobDriver_DriverTick_Patch.eater = null;
            }
        }
    }
}
