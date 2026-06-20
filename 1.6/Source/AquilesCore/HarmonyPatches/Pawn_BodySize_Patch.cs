using System;
using System.Collections.Concurrent;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AquilesCore
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.BodySize), MethodType.Getter)]
    public static class Pawn_BodySize_Patch
    {
        private class CachedStat
        {
            public float value;
            public int ticks;
        }

        private static readonly ConcurrentDictionary<int, CachedStat> cache = new ConcurrentDictionary<int, CachedStat>();

        [ThreadStatic]
        private static int threadCachedPawnId = -1;
        [ThreadStatic]
        private static float threadCachedFactor;
        [ThreadStatic]
        private static int threadCachedTick;

        public static void Postfix(Pawn __instance, ref float __result)
        {
            if (!AquilesCoreMod.settings.bodyTypeMatters || __instance.story?.bodyType == null)
            {
                return;
            }

            var ticks = 0;
            if (Current.ProgramState == ProgramState.Playing && Find.TickManager != null)
            {
                ticks = Find.TickManager.TicksGame;
            }

            var pawnId = __instance.thingIDNumber;

            if (threadCachedPawnId == pawnId && (ticks - threadCachedTick) < 150)
            {
                __result *= threadCachedFactor;
                return;
            }

            if (cache.TryGetValue(pawnId, out var cached) && (ticks - cached.ticks) < 150)
            {
                threadCachedPawnId = pawnId;
                threadCachedFactor = cached.value;
                threadCachedTick = ticks;
                __result *= cached.value;
                return;
            }

            var factor = 1f;

            if (UnityData.IsInMainThread)
            {
                factor = __instance.GetStatValue(DefsOf.Aq_BodySizeFactor);

                if (cached == null)
                {
                    cached = new CachedStat();
                    cache[pawnId] = cached;
                }
                cached.value = factor;
                cached.ticks = ticks;

                threadCachedPawnId = pawnId;
                threadCachedFactor = factor;
                threadCachedTick = ticks;
            }
            else if (cached != null)
            {
                factor = cached.value;
            }

            __result *= factor;
        }
    }
}
