using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;
namespace AquilesCore;
[HarmonyPatch(typeof(RefuelWorkGiverUtility), "FindBestFuel")]
public static class RefuelWorkGiverUtility_FindBestFuel_Patch
{
    public static ThingFilter GetCorrectFuelFilter(ThingFilter originalFilter, Thing refuelable)
    {
        var customComp = refuelable.TryGetComp<CompRefuelable>();
        if (customComp != null && customComp.GetFuelThingFilter() is ThingFilter filter)
        {
            return filter;
        }
        return originalFilter;
    }

    public static float GetCustomSearchRadius(float originalRadius, Thing refuelable)
    {
        var customComp = refuelable.TryGetComp<CompRefuelable>();
        if (customComp != null)
        {
            var data = customComp.GetFuelData();
            if (data.searchRadius < 999f)
            {
                return data.searchRadius;
            }
        }
        return originalRadius;
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var displayClassType = typeof(RefuelWorkGiverUtility).GetNestedTypes(AccessTools.all)
            .First(t => t.Name.Contains("DisplayClass2_0"));

        var targetField = AccessTools.Field(displayClassType, "filter");

        var filterHelperMethod = AccessTools.Method(
            typeof(RefuelWorkGiverUtility_FindBestFuel_Patch),
            nameof(GetCorrectFuelFilter),
            new Type[] { typeof(ThingFilter), typeof(Thing) }
        );

        var radiusHelperMethod = AccessTools.Method(
            typeof(RefuelWorkGiverUtility_FindBestFuel_Patch),
            nameof(GetCustomSearchRadius),
            new Type[] { typeof(float), typeof(Thing) }
        );

        bool patchedFilter = false;
        bool patchedRadius = false;

        foreach (var instruction in instructions)
        {
            yield return instruction;

            if (!patchedFilter && instruction.opcode == OpCodes.Stfld && instruction.operand == (object)targetField)
            {
                yield return new CodeInstruction(OpCodes.Ldloc_0);
                yield return new CodeInstruction(OpCodes.Ldloc_0);
                yield return new CodeInstruction(OpCodes.Ldfld, targetField);
                yield return new CodeInstruction(OpCodes.Ldarg_1);
                yield return new CodeInstruction(OpCodes.Call, filterHelperMethod);
                yield return new CodeInstruction(OpCodes.Stfld, targetField);
                patchedFilter = true;
            }

            if (!patchedRadius && instruction.opcode == OpCodes.Ldc_R4 && instruction.operand is float f && f == 9999f)
            {
                yield return new CodeInstruction(OpCodes.Ldarg_1);
                yield return new CodeInstruction(OpCodes.Call, radiusHelperMethod);
                patchedRadius = true;
            }
        }

        if (!patchedFilter || !patchedRadius)
        {
            Log.Error("AquilesCore: Transpiler injection failed in FindBestFuel.");
        }
    }
}
