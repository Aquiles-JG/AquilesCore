using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AquilesCore;

[HarmonyPatch(typeof(Designator_Build), nameof(Designator_Build.ProcessInput))]
public static class Designator_Build_ProcessInput_Patch
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var getCountMethod = AccessTools.Method(typeof(List<FloatMenuOption>), "get_Count");
        var sortMethod = AccessTools.Method(typeof(Designator_Build_ProcessInput_Patch), nameof(SortOptions));
        bool patched = false;
        foreach (var instruction in instructions)
        {
            if (!patched && instruction.Calls(getCountMethod))
            {
                yield return new CodeInstruction(OpCodes.Ldloc_2);
                yield return new CodeInstruction(OpCodes.Call, sortMethod);
                patched = true;
            }
            yield return instruction;
        }

        if (!patched)
        {
            Log.Error("AquilesCore: Transpiler injection failed in Designator_Build.ProcessInput.");
        }
    }

    public static void SortOptions(List<FloatMenuOption> options)
    {
        options.Sort((a, b) => string.Compare(a.Label, b.Label, StringComparison.Ordinal));
    }
}
