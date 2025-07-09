using HarmonyLib;
using RimWorld;
using Verse;

namespace AquilesCore
{
    public class PreventsInfestation : DefModExtension
    {
    }

    [HarmonyPatch(typeof(InfestationCellFinder), nameof(InfestationCellFinder.TryFindCell))]
    public static class InfestationCellFinder_TryFindCell_Patch
    {
        public static void Postfix(ref IntVec3 cell, Map map, ref bool __result)
        {
            if (__result && cell.IsValid)
            {
                TerrainDef terrain = cell.GetTerrain(map);
                if (terrain != null && terrain.HasModExtension<PreventsInfestation>())
                {
                    cell = IntVec3.Invalid;
                    __result = false;
                }
            }
        }
    }
}
