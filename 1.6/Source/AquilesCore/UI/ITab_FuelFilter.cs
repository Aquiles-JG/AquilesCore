using Verse;
using RimWorld;
using UnityEngine;
using System;

[HotSwappable]
public class ITab_FuelFilter : ITab
{
    private ThingFilterUI.UIState thingFilterState = new ThingFilterUI.UIState();

    public static bool IsFuelFilterTabActive = false;

    private Gizmo interactedGiz;
    private Event interactedEvent;
    public ITab_FuelFilter()
    {
        this.labelKey = "AC_TabFuelFilter";
    }

    public override void FillTab()
    {
        interactedGiz = null;
        interactedEvent = default(Event);

        Rect rect = new Rect(0f, 0f, this.size.x, this.size.y).ContractedBy(10f);
        var compRefuelable = this.SelThing.TryGetComp<CompRefuelable>();
        var fuelData = RefuelTrackingHelper.GetFuelData(compRefuelable);

        float filterHeight = 180f;
        Rect filterRect = new Rect(rect.x, rect.y + 5, rect.width - 5, filterHeight - 5);

        IsFuelFilterTabActive = true;
        try
        {
            ThingFilterUI.DoThingFilterConfigWindow(filterRect, thingFilterState, fuelData.allowedFuelFilter, compRefuelable.Props.fuelFilter, 1, null, null, forceHideHitPointsConfig: true);
        }
        finally
        {
            IsFuelFilterTabActive = false;
        }

        Rect bottomSectionRect = new Rect(rect.x, rect.y + filterHeight + 10f, rect.width, rect.height - filterHeight - 10f);
        var size = 180f;

        Rect itemPreviewRect = new Rect(bottomSectionRect.x, bottomSectionRect.y, size + 15, size);
        Rect slidersRect = new Rect(itemPreviewRect.xMax + 10f, bottomSectionRect.y, rect.width - itemPreviewRect.width - 10f, bottomSectionRect.height);

        Widgets.DrawMenuSection(itemPreviewRect);
        if (compRefuelable.Fuel > 0f && fuelData.lastFuelThingDef != null)
        {
            GUI.color = fuelData.lastFuelThingDef.uiIconColor;
            Widgets.DrawTextureFitted(itemPreviewRect.ContractedBy(5f), fuelData.lastFuelThingDef.uiIcon, 1f);
            GUI.color = Color.white;
        }
        else
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(itemPreviewRect, "AC_NoFuel".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
        }

        float curYSliders = slidersRect.y;
        float elementHeight = 30f;
        float elementMargin = 5f;

        Rect limitCapacityRect = new Rect(slidersRect.x, curYSliders, slidersRect.width, elementHeight);
        compRefuelable.TargetFuelLevel = Widgets.HorizontalSlider(limitCapacityRect, compRefuelable.TargetFuelLevel, 0f, compRefuelable.Props.fuelCapacity, label: "AC_LimitCapacityTo".Translate(compRefuelable.TargetFuelLevel.ToString("F0")));
        curYSliders += elementHeight + elementMargin;

        Rect refuelAtRect = new Rect(slidersRect.x, curYSliders, slidersRect.width, elementHeight);
        fuelData.refuelAt = Widgets.HorizontalSlider(refuelAtRect, fuelData.refuelAt, 0f, 1f, label: "AC_RefuelAtPercent".Translate(fuelData.refuelAt.ToStringPercent()));
        curYSliders += elementHeight + elementMargin;

        Rect searchRadiusRect = new Rect(slidersRect.x, curYSliders, slidersRect.width, elementHeight);
        string radiusValue = (fuelData.searchRadius >= 999f) ? "Unlimited".TranslateSimple() : fuelData.searchRadius.ToString("F0");
        string radiusLabel = "AC_FuelSearchRadius".Translate(radiusValue);
        float sliderValue = (fuelData.searchRadius >= 999f) ? 100f : fuelData.searchRadius;
        float newSliderValue = Widgets.HorizontalSlider(searchRadiusRect, sliderValue, 0f, 100f, label: radiusLabel);
        if (newSliderValue != sliderValue)
        {
            if (newSliderValue >= 100f)
            {
                fuelData.searchRadius = 999f;
            }
            else
            {
                fuelData.searchRadius = newSliderValue;
            }
        }
        curYSliders += elementHeight + elementMargin;

        var gizmo = new Gizmo_SetFuelLevel(compRefuelable);
        GizmoRenderParms parms = new GizmoRenderParms();
        if (gizmo.disabled)
        {
            parms.lowLight = true;
            gizmo.disabled = false;
        }
        var vec = new Vector2(slidersRect.x + 20, curYSliders);

        GizmoResult result = gizmo.GizmoOnGUI(vec, slidersRect.width, parms);

        if (result.State == GizmoState.Interacted)
        {
            interactedGiz = gizmo;
            interactedEvent = result.InteractEvent;
        }

        if (interactedGiz != null && interactedGiz == gizmo)
        {
            interactedGiz.ProcessInput(interactedEvent);
            Event.current.Use();
        }
    }

    public override void UpdateSize()
    {
        this.size = new Vector2(425f, 400f);
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class HotSwappableAttribute : Attribute
{
}
