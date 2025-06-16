using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

[HarmonyPatch]
public static class RefuelWorkGiverUtility_FindBestFuel_Validator_Patch
{
    [HarmonyTargetMethod]
    public static MethodBase TargetMethod()
    {
        foreach (var type in typeof(RefuelWorkGiverUtility).GetNestedTypes(AccessTools.all))
        {
            foreach (var method in type.GetMethods(AccessTools.all))
            {
                if (method.Name.Contains("FindBestFuel"))
                {
                    return method;
                }
            }
        }
        
        Log.Error("AquilesCore: Could not find validator method for FindBestFuel patch.");
        return null;
    }

    public static bool Prefix(Thing x, ref bool __result)
    {
        Thing refuelable = RefuelTrackingHelper.currentRefuelableThing;
        if (refuelable != null)
        {
            var compRefuelable = refuelable.TryGetComp<CompRefuelable>();
            if (compRefuelable != null)
            {
                var fuelData = compRefuelable.GetFuelData();
                float searchRadius = fuelData.searchRadius;

                if (searchRadius < 999f)
                {
                    float distanceSq = (compRefuelable.parent.Position - x.Position).LengthHorizontalSquared;
                    if (distanceSq > searchRadius * searchRadius)
                    {
                        __result = false;
                        return false;
                    }
                }
            }
        }
        return true;
    }
}
