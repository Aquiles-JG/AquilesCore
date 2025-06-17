using HarmonyLib;
using RimWorld;
using Verse;

[HarmonyPatch(typeof(CompRefuelable), nameof(CompRefuelable.ShouldAutoRefuelNow), MethodType.Getter)]
public static class CompRefuelable_ShouldAutoRefuelNow_Patch
{
    public static bool Prefix(CompRefuelable __instance, ref bool __result)
    {
        var fuelData = __instance.GetFuelData();
        float currentFuelPercent = __instance.Fuel / __instance.TargetFuelLevel;
        if (currentFuelPercent < fuelData.refuelAt)
        {
            __result = __instance.ShouldAutoRefuelNowIgnoringFuelPct;
            return false;
        }
        return true;
    }
}
