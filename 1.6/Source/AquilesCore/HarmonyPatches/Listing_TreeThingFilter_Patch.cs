using AquilesCore;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

public static partial class CompRefuelablePatches
{
    [HarmonyPatch]
    public static class Listing_TreeThingFilter_Patch
    {
        public static ThingDef currentThingDef;

        [HarmonyPatch(typeof(Listing_TreeThingFilter), "DoThingDef")]
        [HarmonyPrefix]
        public static void DoThingDef_Prefix(ThingDef tDef)
        {
            if (ITab_FuelFilter.IsFuelFilterTabActive)
            {
                currentThingDef = tDef;
            }
        }

        [HarmonyPatch(typeof(Listing_TreeThingFilter), "DoThingDef")]
        [HarmonyPostfix]
        public static void DoThingDef_Postfix()
        {
            if (ITab_FuelFilter.IsFuelFilterTabActive)
            {
                currentThingDef = null;
            }
        }

        [HarmonyPatch(typeof(Listing_Tree), nameof(Listing_Tree.LabelLeft))]
        [HarmonyPrefix]
        public static void LabelLeft_Prefix(ref string tipText, ref Color? textColor)
        {
            if (currentThingDef != null)
            {
                float fuelPower = currentThingDef.GetStatValueAbstract(DefsOf.Aq_FuelPower);
                if (fuelPower < 1f)
                {
                    textColor = Color.red;
                }
                else if (fuelPower > 1f)
                {
                    textColor = Color.green;
                }

                tipText += $"\n" + "AC_FuelPower".Translate(fuelPower.ToStringPercent());
            }
        }
    }
}
