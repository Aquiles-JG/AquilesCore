using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

[HarmonyPatch(typeof(CompRefuelable))]
[HarmonyPatch(nameof(CompRefuelable.Refuel), new[] { typeof(List<Thing>) })]
public static class CompRefuelable_Refuel_Patch
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
