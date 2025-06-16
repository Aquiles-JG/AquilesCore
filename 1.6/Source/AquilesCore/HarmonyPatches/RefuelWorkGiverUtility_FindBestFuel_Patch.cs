using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

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

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var displayClassType = typeof(RefuelWorkGiverUtility).GetNestedTypes(AccessTools.all)
            .First(t => t.Name.Contains("DisplayClass2_0"));

        var targetField = AccessTools.Field(displayClassType, "filter");

        var helperMethod = AccessTools.Method(
            typeof(RefuelWorkGiverUtility_FindBestFuel_Patch),
            nameof(GetCorrectFuelFilter),
            new Type[] { typeof(ThingFilter), typeof(Thing) }
        );

        bool patched = false;

        foreach (var instruction in instructions)
        {
            yield return instruction;

            if (instruction.opcode == OpCodes.Stfld && instruction.operand == (object)targetField)
            {
                yield return new CodeInstruction(OpCodes.Ldloc_0);
                yield return new CodeInstruction(OpCodes.Ldloc_0);
                yield return new CodeInstruction(OpCodes.Ldfld, targetField);
                yield return new CodeInstruction(OpCodes.Ldarg_1);
                yield return new CodeInstruction(OpCodes.Call, helperMethod);
                yield return new CodeInstruction(OpCodes.Stfld, targetField);
                patched = true;
            }
        }

        if (!patched)
        {
            Log.Error("AquilesCore: Transpiler injection failed. Could not find target stfld instruction.");
        }
    }

    public static void Prefix(Thing refuelable)
    {
        RefuelTrackingHelper.currentRefuelableThing = refuelable;
    }

    [HarmonyPostfix]
    public static void Postfix()
    {
        RefuelTrackingHelper.currentRefuelableThing = null;
    }
}
