using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AquilesCore
{
    public class DestroyOnTerrainChangeExtension : DefModExtension
    {
        public string destroyMessageKey;
    }
    [HarmonyPatch]
    public static class TerrainGrid_SetTerrain_Patch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(TerrainGrid), nameof(TerrainGrid.SetTerrain));
            yield return AccessTools.Method(typeof(TerrainGrid), nameof(TerrainGrid.SetTempTerrain));
            yield return AccessTools.Method(typeof(TerrainGrid), "RemoveTempTerrain");
        }

        public static void Postfix(TerrainGrid __instance, IntVec3 c)
        {
            var map = __instance.map;
            foreach (var thing in map.thingGrid.ThingsAt(c).ToList())
            {
                var ext = thing.def.GetModExtension<DestroyOnTerrainChangeExtension>();
                if (ext == null)
                    continue;

                var affordance = thing.def.terrainAffordanceNeeded;
                var terrain = c.GetTerrain(map);
                if (affordance != null && terrain?.affordances != null && terrain.affordances.Contains(affordance))
                    continue;

                thing.Destroy();
                Messages.Message(ext.destroyMessageKey.Translate(), new TargetInfo(c, map), MessageTypeDefOf.NeutralEvent);
            }
        }
    }
}
