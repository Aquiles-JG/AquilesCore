using RimWorld;
using Verse;

namespace HediffRemover
{
    public class IngestionOutcomeDoer_Immunity : IngestionOutcomeDoer
    {
        public HediffDef hediffToRemove;

        public HediffData immunityHediff;

        public float severityToReduce;

        public HediffData recoveringHediff;

        public override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(immunityHediff.hediff);
            if (hediff is null)
            {
                hediff = GiveHediff(pawn, immunityHediff);

                var firstDisease = pawn.health.hediffSet.hediffs.FirstOrDefault(x => x.def == hediffToRemove);
                if (firstDisease != null)
                {
                    firstDisease.Severity -= severityToReduce;
                    if (firstDisease.Severity <= 0)
                    {
                        pawn.health.RemoveHediff(firstDisease);
                        if (recoveringHediff != null)
                        {
                            GiveHediff(pawn, recoveringHediff);
                        }
                    }
                }
            }
        }

        private Hediff GiveHediff(Pawn pawn, HediffData data)
        {
            Hediff hediff = HediffMaker.MakeHediff(data.hediff, pawn);
            pawn.health.AddHediff(hediff);
            var comp = hediff.TryGetComp<HediffComp_Disappears>();
            if (comp != null)
            {
                comp.ticksToDisappear = (int)(data.durationHours * (float)GenDate.TicksPerHour);
            }
            return hediff;
        }
    }
}
