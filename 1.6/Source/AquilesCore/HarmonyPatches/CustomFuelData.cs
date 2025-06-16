using RimWorld;
using Verse;

public class CustomFuelData : IExposable
{
    public ThingDef lastFuelThingDef;
    public ThingFilter allowedFuelFilter;
    public float refuelAt;
    public float searchRadius = 25f;
    public CustomFuelData()
    {
        allowedFuelFilter = new ThingFilter();
    }

    public CustomFuelData(CompRefuelable instance)
    {
        allowedFuelFilter = new ThingFilter();
        allowedFuelFilter.CopyAllowancesFrom(instance.Props.fuelFilter);
    }

    public void ExposeData()
    {
        Scribe_Defs.Look(ref lastFuelThingDef, "lastFuelThingDef");
        Scribe_Deep.Look(ref allowedFuelFilter, "allowedFuelFilter");
        Scribe_Values.Look(ref refuelAt, "refuelAt");
        Scribe_Values.Look(ref searchRadius, "searchRadius", 25f);
    }
}
