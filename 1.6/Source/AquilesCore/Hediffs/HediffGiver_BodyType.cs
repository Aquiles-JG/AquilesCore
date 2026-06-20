using RimWorld;
using Verse;

namespace AquilesCore
{
    public class HediffGiver_BodyType : HediffGiver
    {
        public BodyTypeDef bodyType;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (!pawn.RaceProps.Humanlike)
            {
                return;
            }

            var shouldHave = AquilesCoreMod.settings.bodyTypeMatters
                && pawn.story.bodyType == bodyType
                && !IsBlacklisted(pawn);

            var has = pawn.health.hediffSet.HasHediff(hediff);
            if (shouldHave && !has)
            {
                TryApply(pawn);
            }
            else if (!shouldHave && has)
            {
                pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(hediff));
            }
        }

        private bool IsBlacklisted(Pawn pawn)
        {
            return pawn.genes?.Xenotype != null
                && AquilesCoreMod.settings.xenotypeBlacklist.Contains(pawn.genes.Xenotype.defName);
        }
    }
}
