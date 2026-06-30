using RimWorld;
using Verse;
using AquilesCore;

public class CustomFuelData : IExposable
{
    public ThingDef lastFuelThingDef;
    public ThingFilter allowedFuelFilter;
    public float refuelAt;
    public float searchRadius;
    public CustomFuelData()
    {
        allowedFuelFilter = new ThingFilter();
        searchRadius = AquilesCoreMod.settings.defaultFuelSearchRadius;
    }

    public CustomFuelData(CompRefuelable instance)
    {
        allowedFuelFilter = new ThingFilter();
        allowedFuelFilter.CopyAllowancesFrom(instance.Props.fuelFilter);
        searchRadius = AquilesCoreMod.settings.defaultFuelSearchRadius;
    }

    public void ExposeData()
    {
        Scribe_Defs.Look(ref lastFuelThingDef, "lastFuelThingDef");
        Scribe_Deep.Look(ref allowedFuelFilter, "allowedFuelFilter");
        Scribe_Values.Look(ref refuelAt, "refuelAt");
        Scribe_Values.Look(ref searchRadius, "searchRadius", AquilesCoreMod.settings.defaultFuelSearchRadius);
    }
}
