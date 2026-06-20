using HarmonyLib;
using RimWorld;
namespace AquilesCore;

[HarmonyPatch(typeof(CompRefuelable), nameof(CompRefuelable.PostExposeData))]
public static class CompRefuelable_PostExposeData_Patch
{
    public static void Postfix(CompRefuelable __instance)
    {
        RefuelTrackingHelper.HandleFuelData(__instance);
    }
}
