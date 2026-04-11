using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace AquilesCore
{
    [HarmonyPatch(typeof(World), nameof(World.ExposeData))]
    public static class World_ExposeData_Patch
    {
        public static void Postfix()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                ResourceReadoutStateHelper.SaveCurrentState();
            }
            Scribe_Deep.Look(ref ResourceReadoutStateHelper.state, "resourceReadoutState");
            ResourceReadoutStateHelper.state ??= new ResourceReadoutState();
            if (Scribe.mode == LoadSaveMode.LoadingVars || Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                ResourceReadoutStateHelper.ResetRestoreFlag();
            }
        }
    }
}
