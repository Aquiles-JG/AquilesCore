using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;

namespace AquilesCore;

[HarmonyPatch(typeof(CompRefuelable), nameof(CompRefuelable.CompGetGizmosExtra))]
public static class CompRefuelable_CompGetGizmosExtra_Patch
{
    public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result)
    {
        foreach (var gizmo in __result)
        {
            if (gizmo is Command_Action)
            {
                yield return gizmo;
            }
        }
    }
}
