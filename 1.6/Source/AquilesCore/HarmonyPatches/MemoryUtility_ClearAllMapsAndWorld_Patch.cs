using HarmonyLib;
using Verse.Profile;

namespace AquilesCore
{
    [HarmonyPatch(typeof(MemoryUtility), nameof(MemoryUtility.ClearAllMapsAndWorld))]
    public static class MemoryUtility_ClearAllMapsAndWorld_Patch
    {
        public static void Prefix()
        {
            ResourceReadoutStateHelper.state = new ResourceReadoutState();
        }
    }
}
