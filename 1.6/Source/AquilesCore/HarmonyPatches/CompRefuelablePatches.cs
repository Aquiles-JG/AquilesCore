using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using AquilesCore;
using HarmonyLib;
using RimWorld;
using Verse;


[HarmonyPatch(typeof(CompRefuelable))]
[HarmonyPatch(nameof(CompRefuelable.Refuel), new[] { typeof(List<Thing>) })]
public static class RefuelTrackingPatch
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            yield return instruction;

            if (instruction.opcode == OpCodes.Stloc_1) // Target where Thing is stored in local variable
            {
                yield return new CodeInstruction(OpCodes.Ldloc_1); // Load Thing onto stack
                yield return new CodeInstruction(OpCodes.Ldarg_0); // Load CompRefuelable instance onto stack
                yield return CodeInstruction.Call(typeof(RefuelTrackingHelper), nameof(RefuelTrackingHelper.StoreFuelThing));
            }
        }
    }
}

public static class RefuelTrackingHelper
{
    private static readonly Dictionary<CompRefuelable, ThingDef> FuelThingDefs = new();

    public static void StoreFuelThing(Thing thing, CompRefuelable instance)
    {
        if (thing != null && instance != null)
        {
            FuelThingDefs[instance] = thing.def;
        }
    }

    public static ThingDef GetFuelThingDef(this CompRefuelable instance)
    {
        if (instance != null && FuelThingDefs.TryGetValue(instance, out var thingDef))
        {
            return thingDef;
        }
        return null;
    }

    public static void HandleFuelThingDef(CompRefuelable instance)
    {
        if (instance == null) return;

        ThingDef thingDef = null;
        if (FuelThingDefs.TryGetValue(instance, out var existingDef))
        {
            thingDef = existingDef;
        }

        Scribe_Defs.Look(ref thingDef, "storedFuelThingDef");

        if (thingDef != null)
        {
            FuelThingDefs[instance] = thingDef;
        }
    }
}

[HarmonyPatch(typeof(CompRefuelable), nameof(CompRefuelable.PostExposeData))]
public static class RefuelTrackingSaveLoadPatch
{
    [HarmonyPostfix]
    public static void PostExposeDataPatch(CompRefuelable __instance)
    {
        RefuelTrackingHelper.HandleFuelThingDef(__instance);
    }
}
//[HarmonyPatch(typeof(CompRefuelable), nameof(CompRefuelable.CompInspectStringExtra))]
//public static class RefuelTrackingInspectStringPatch
//{
//    [HarmonyPostfix]
//    public static void CompInspectStringExtraPostfix(CompRefuelable __instance, ref string __result)
//    {
//        var lastFuelDef = RefuelTrackingHelper.GetFuelThingDef(__instance);
//        if (lastFuelDef != null)
//        {
//            __result += $"\nDEV: Last refueled with: {lastFuelDef.label}";
//        }
//    }
//}
[HarmonyPatch]
public static class CompRefuelablePatches
{
    public static float ModifyFuelConsumptionRate(float originalRate, CompRefuelable instance)
    {
        // Get the current fuel definition
        ThingDef fuelDef = instance.GetFuelThingDef();
        if (fuelDef != null)
        {
            float fuelPower = fuelDef.GetStatValueAbstract(DefsOf.Aq_FuelPower);
            var modifiedRate = originalRate / fuelPower;
            return modifiedRate;
        }
        return originalRate;
    }

    public static IEnumerable<MethodBase> TargetMethods()
    {
        yield return typeof(CompRefuelable).GetMethod(nameof(CompRefuelable.CompInspectStringExtra));
        yield return typeof(CompRefuelable).GetMethod("get_ConsumptionRatePerTick",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            yield return instruction;
            if (instruction.LoadsField(typeof(CompProperties_Refuelable).GetField(nameof(CompProperties_Refuelable.fuelConsumptionRate))))
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0); // Load CompRefuelable instance
                yield return CodeInstruction.Call(typeof(CompRefuelablePatches), nameof(ModifyFuelConsumptionRate));
            }
        }
    }
}
