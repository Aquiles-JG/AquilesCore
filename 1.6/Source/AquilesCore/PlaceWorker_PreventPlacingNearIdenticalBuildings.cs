using Verse;
using RimWorld;

namespace AquilesCore
{
    public class PlaceWorker_PreventPlacingNearIdenticalBuildings : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            foreach (var nearby in GenRadial.RadialDistinctThingsAround(center, map, 5, true))
            {
                if (nearby.def == def || nearby.def.entityDefToBuild == def)
                {
                    return "Aq_CannotPlaceCloseToOther".Translate(def.label);
                }
            }
            return true;
        }
    }
}
