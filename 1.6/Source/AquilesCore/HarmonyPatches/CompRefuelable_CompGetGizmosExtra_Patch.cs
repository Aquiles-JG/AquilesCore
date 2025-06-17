using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;

[HarmonyPatch(typeof(CompRefuelable), nameof(CompRefuelable.CompGetGizmosExtra))]
public static class CompRefuelable_CompGetGizmosExtra_Patch
{
    public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result)
    {
        yield break;
    }
}
