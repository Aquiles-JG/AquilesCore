using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using AquilesCore;
using HarmonyLib;
using RimWorld;
using Verse;

[HarmonyPatch]
public static partial class CompRefuelablePatches
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
