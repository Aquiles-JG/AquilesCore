using RimWorld;
using Verse;

namespace AquilesCore
{
    public class IngestionOutcomeDoer_RemoveHediff : IngestionOutcomeDoer
    {
        public HediffDef hediffToRemove;

        public override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffToRemove);
            if (hediff != null)
            {
                pawn.health.hediffSet.hediffs.Remove(hediff);
            }
        }
    }
}
