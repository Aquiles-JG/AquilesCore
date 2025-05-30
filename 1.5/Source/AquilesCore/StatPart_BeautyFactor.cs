using RimWorld;
using Verse;
using UnityEngine;
namespace AquilesCore;
public class StatPart_BeautyFactor : StatPart
{
    public override string ExplanationPart(StatRequest req)
    {
        if (!req.HasThing || !(req.Thing is Pawn pawn) || !pawn.RaceProps.Humanlike)
        {
            return null;
        }

        var beauty = pawn.GetStatValue(StatDefOf.PawnBeauty);
        var offset = GetOffset(beauty);
        return "StatsReport_BeautyOffset".Translate() + ": " + offset.ToStringPercentSigned("0.#");
    }

    public override void TransformValue(StatRequest req, ref float val)
    {
        if (!req.HasThing || !(req.Thing is Pawn pawn) || !pawn.RaceProps.Humanlike)
        {
            return;
        }

        var beauty = pawn.GetStatValue(StatDefOf.PawnBeauty);
        val += GetOffset(beauty);
    }

    private float GetOffset(float beauty)
    {

        var baseOffset = 0f;
        if (parentStat == StatDefOf.NegotiationAbility)
        {
            baseOffset += beauty * 0.115f;
        }
        else if (parentStat == StatDefOf.ConversionPower)
        {
            baseOffset += beauty * 0.115f;
        }
        else if (parentStat == StatDefOf.TradePriceImprovement)
        {
            baseOffset += beauty * 0.015f;
        }
        else if (parentStat == StatDefOf.SocialImpact)
        {
            baseOffset += beauty * 0.11f;
        }

        return baseOffset;
    }
}
