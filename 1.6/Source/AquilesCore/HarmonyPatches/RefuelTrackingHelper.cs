using System.Collections.Generic;
using RimWorld;
using Verse;

public static class RefuelTrackingHelper
{
    private static readonly Dictionary<CompRefuelable, CustomFuelData> fuelData = new();
    public static Thing currentRefuelableThing;
    public static CustomFuelData GetFuelData(this CompRefuelable instance)
    {
        if (!fuelData.TryGetValue(instance, out var data))
        {
            data = new CustomFuelData(instance);
            fuelData[instance] = data;
        }
        return data;
    }

    public static void StoreFuelThing(Thing thing, CompRefuelable instance)
    {
        if (thing != null && instance != null)
        {
            GetFuelData(instance).lastFuelThingDef = thing.def;
        }
    }

    public static ThingDef GetFuelThingDef(this CompRefuelable instance)
    {
        return instance.GetFuelData().lastFuelThingDef;
    }

    public static ThingFilter GetFuelThingFilter(this CompRefuelable instance)
    {
        return instance.GetFuelData().allowedFuelFilter;
    }

    public static void HandleFuelData(CompRefuelable instance)
    {
        if (instance == null) return;

        CustomFuelData data = GetFuelData(instance);
        Scribe_Deep.Look(ref data, "customFuelData");
        if (data != null)
        {
            fuelData[instance] = data;
        }
        else
        {
            fuelData.Remove(instance);
        }
    }
}
