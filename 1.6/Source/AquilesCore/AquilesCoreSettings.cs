using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace AquilesCore
{
    public class AquilesCoreSettings : ModSettings
    {
        public bool beautyMatters = true;
        public bool bodyTypeMatters = true;
        public bool fuelUIEnabled = true;
        public int defaultFuelSearchRadius = 25;
        public List<string> xenotypeBlacklist = new List<string>();
        private Vector2 scrollPosition;
        private float settingsHeight;
        public void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            var outRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            var viewRect = new Rect(0f, 0f, inRect.width - 16f, Mathf.Max(settingsHeight, inRect.height));
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            listing.Begin(viewRect);
            listing.CheckboxLabeled("AC_BeautyMattersToggle".Translate(), ref beautyMatters, "AC_BeautyMattersToggleDesc".Translate());
            listing.CheckboxLabeled("AC_BodyTypeMattersToggle".Translate(), ref bodyTypeMatters, "AC_BodyTypeMattersToggleDesc".Translate());
            listing.CheckboxLabeled("AC_FuelUIToggle".Translate(), ref fuelUIEnabled, "AC_FuelUIToggleDesc".Translate());
            if (fuelUIEnabled)
            {
                defaultFuelSearchRadius = Mathf.RoundToInt(listing.SliderLabeled("AC_DefaultFuelSearchRadius".Translate(defaultFuelSearchRadius), defaultFuelSearchRadius, 1f, 100f, 0.4f, "AC_DefaultFuelSearchRadiusDesc".Translate()));
            }
            if (bodyTypeMatters)
            {
                listing.Gap();
                listing.Label("AC_XenotypeBlacklistHeader".Translate());
                var xenotypes = DefDatabase<XenotypeDef>.AllDefs.OrderBy(x => x.label).ToList();
                for (int i = 0; i < xenotypes.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        Widgets.DrawLightHighlight(listing.GetRect(24));
                        listing.curY -= 24;
                    }
                    var xenotype = xenotypes[i];
                    var excluded = xenotypeBlacklist.Contains(xenotype.defName);
                    listing.CheckboxLabeled(xenotype.LabelCap, ref excluded);
                    if (excluded && !xenotypeBlacklist.Contains(xenotype.defName))
                    {
                        xenotypeBlacklist.Add(xenotype.defName);
                    }
                    else if (!excluded && xenotypeBlacklist.Contains(xenotype.defName))
                    {
                        xenotypeBlacklist.Remove(xenotype.defName);
                    }
                }
            }
            listing.End();
            settingsHeight = listing.CurHeight;
            Widgets.EndScrollView();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref beautyMatters, "beautyMatters", true);
            Scribe_Values.Look(ref bodyTypeMatters, "bodyTypeMatters", true);
            Scribe_Values.Look(ref fuelUIEnabled, "fuelUIEnabled", true);
            Scribe_Values.Look(ref defaultFuelSearchRadius, "defaultFuelSearchRadius", 25);
            Scribe_Collections.Look(ref xenotypeBlacklist, "xenotypeBlacklist", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                xenotypeBlacklist ??= new List<string>();
            }
        }
    }
}
