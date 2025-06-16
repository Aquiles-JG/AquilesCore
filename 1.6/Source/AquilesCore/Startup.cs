using System.Collections.Generic;
using HarmonyLib;
using Verse;
using RimWorld;
using System;

namespace AquilesCore
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                var compProps = thingDef.GetCompProperties<CompProperties_Refuelable>();
                if (compProps != null)
                {
                    thingDef.inspectorTabs ??= new List<Type>();
                    thingDef.inspectorTabs.Add(typeof(ITab_FuelFilter));
                    thingDef.inspectorTabsResolved ??= new();
                    thingDef.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(typeof(ITab_FuelFilter)));
                }
            }
        }
    }
}
