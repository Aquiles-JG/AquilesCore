using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AquilesCore;

[HarmonyPatch(typeof(Designator_Dropdown), "SetupFloatMenu")]
public static class Designator_Dropdown_SetupFloatMenu_Patch
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var floatMenuCtor = AccessTools.Constructor(typeof(FloatMenu), new Type[] { typeof(List<FloatMenuOption>) });
        var sortMethod = AccessTools.Method(typeof(Designator_Build_ProcessInput_Patch), "SortOptions");
        bool patched = false;
        foreach (var instruction in instructions)
        {
            if (!patched && instruction.opcode == OpCodes.Newobj && instruction.operand == (object)floatMenuCtor)
            {
                yield return new CodeInstruction(OpCodes.Ldloc_0);
                yield return new CodeInstruction(OpCodes.Call, sortMethod);
                patched = true;
            }
            yield return instruction;
        }

        if (!patched)
        {
            Log.Error("AquilesCore: Transpiler injection failed in Designator_Dropdown.SetupFloatMenu.");
        }
    }
}
