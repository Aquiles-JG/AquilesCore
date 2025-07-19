using RimWorld;
using Verse;

namespace AquilesCore
{
    public class PlaceWorker_OnOceanWater : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            foreach (IntVec3 c in GenAdj.CellsOccupiedBy(loc, rot, checkingDef.Size))
            {
                var terrain = map.terrainGrid.TerrainAt(c);
                if (terrain != TerrainDefOf.WaterOceanDeep && terrain != TerrainDefOf.WaterOceanShallow)
                {
                    return new AcceptanceReport("Aq_NeedsOceanWater".Translate());
                }
            }



            return true;
        }






    }
}
