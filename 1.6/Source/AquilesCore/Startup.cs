using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AquilesCore
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        public static readonly HashSet<StatDef> JobBonusStats = new HashSet<StatDef>();
        static Startup()
        {
            new Harmony("AquilesCoreMod").PatchAll();
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

                var jobBonusProps = thingDef.GetCompProperties<CompProperties_FacilityJobBonus>();
                if (jobBonusProps?.jobStatBonuses != null)
                {
                    foreach (var jobBonus in jobBonusProps.jobStatBonuses)
                    {
                        if (jobBonus.statOffsets != null)
                        {
                            foreach (var statModifier in jobBonus.statOffsets)
                            {
                                JobBonusStats.Add(statModifier.stat);
                            }
                        }
                        if (jobBonus.statFactors != null)
                        {
                            foreach (var statModifier in jobBonus.statFactors)
                            {
                                JobBonusStats.Add(statModifier.stat);
                            }
                        }
                    }
                }

                foreach (var stat in JobBonusStats)
                {
                    stat.parts ??=new List<StatPart>();
                    var statPart = new StatPart_FacilityJobBonus();
                    statPart.parentStat = stat;
                    stat.parts.Add(statPart);
                    if (stat.immutable)
                    {
                        stat.immutable = false;
                        stat.Worker.SetCacheability(stat.immutable);
                    }
                }
            }
        }
    }
}
