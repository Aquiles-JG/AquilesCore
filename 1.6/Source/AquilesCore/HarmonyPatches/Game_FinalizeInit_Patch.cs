using HarmonyLib;
using Verse;

namespace AquilesCore
{
    [HarmonyPatch(typeof(Game), nameof(Game.FinalizeInit))]
    public static class Game_FinalizeInit_Patch
    {
        public static void Postfix()
        {
            ResourceReadoutStateHelper.RestoreState();
        }
    }
}
