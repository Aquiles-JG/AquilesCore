using HarmonyLib;
using Verse;

namespace AquilesCore
{
    [HarmonyPatch(typeof(MouseoverUtility), "GetGlowLabelByValue")]
    public static class MouseoverUtility_GetGlowLabelByValue_Patch
    {
        public static void Postfix(ref string __result)
        {
            var map = Find.CurrentMap;
            if (map != null)
            {
                IntVec3 cell = UI.MouseCell();
                __result += $" ({cell.x}, 0, {cell.z})";
            }
        }
    }
}