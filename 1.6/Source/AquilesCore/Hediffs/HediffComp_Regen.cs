using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AquilesCore
{
    public class HediffCompProperties_Regen : HediffCompProperties
    {
        public int regenHealVal;

        public int regenHoursMax;

        public int regenHoursMin;

        public bool restoreMissingLimbs;

        public bool cureScars;

        public bool includeBrainScar;
        public HediffCompProperties_Regen()
        {
            compClass = typeof(HediffComp_Regen);
        }
    }

    public class HediffComp_Regen : HediffComp
    {
        private int ticksToHeal;

        public HediffCompProperties_Regen Props => (HediffCompProperties_Regen)props;

        public override void CompPostMake()
        {
            base.CompPostMake();
            ResetTicksToHeal();
        }

        public void ResetTicksToHeal()
        {
            int num = 2500;
            if (Props.regenHoursMin > 0 && Props.regenHoursMax > 0 && Props.regenHoursMax >= Props.regenHoursMin)
            {
                ticksToHeal = Rand.Range(Props.regenHoursMin, Props.regenHoursMax) * num;
            }
            else
            {
                ticksToHeal = Rand.Range(12, 24) * num;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            ticksToHeal--;
            if (ticksToHeal <= 0)
            {
                TryHealRandomOldWound();
                ResetTicksToHeal();
            }
        }

        public void TryHealRandomOldWound()
        {
            if (Pawn?.health?.hediffSet == null)
            {
                return;
            }

            int num = Props.regenHealVal;
            var list = new List<Hediff>();

            if (Props.cureScars)
            {
                var hediffs = Pawn.health.hediffSet.hediffs;
                if (hediffs != null)
                {
                    for (int i = 0; i < hediffs.Count; i++)
                    {
                        Hediff hediff = hediffs[i];
                        if (hediff.IsPermanent())
                        {
                            if (hediff.Part != Pawn.health.hediffSet.GetBrain() || Props.includeBrainScar)
                            {
                                list.Add(hediff);
                            }
                        }
                    }
                }
            }

            if (Props.restoreMissingLimbs)
            {
                var missingParts = Pawn.health.hediffSet.GetMissingPartsCommonAncestors();
                if (missingParts != null)
                {
                    foreach (var mp in missingParts)
                    {
                        list.Add(mp);
                    }
                }
            }

            if (list.Count <= 0)
            {
                return;
            }

            if (!list.TryRandomElement(out var result) || result == null)
            {
                return;
            }

            if (result is Hediff_MissingPart missingPart)
            {
                var part = missingPart.Part;
                Pawn.health.RestorePart(part);
                if (PawnUtility.ShouldSendNotificationAbout(Pawn))
                {
                    Messages.Message("Regen.RecoveredLimb".Translate(Pawn.Named("PAWN")), MessageTypeDefOf.PositiveEvent);
                }
            }
            else
            {

                if (result.IsTended())
                {
                    num = (int)((float)num * 1.2f);
                    var healFactor = GetHealFactor(result);
                    if (healFactor > 0f)
                    {
                        num = (int)((float)num * healFactor);
                        if (num < 1)
                        {
                            num = 1;
                        }
                    }
                }
                if (result.Severity - (float)num > 0f)
                {
                    result.Severity -= num;
                }
                else
                {
                    result.Severity = 0f;
                }
                if (result.Severity <= 0f && PawnUtility.ShouldSendNotificationAbout(Pawn))
                {
                    Messages.Message("Regen.HealedScar".Translate(Pawn.Named("PAWN")), MessageTypeDefOf.PositiveEvent);
                }
            }
        }

        internal float GetHealFactor(Hediff h)
        {
            float result = 1f;
            if (h.def == DefsOf.Scratch)
            {
                result = 1.2f;
            }
            else if (h.def == DefsOf.Bruise)
            {
                result = 1.5f;
            }
            else if (h.def == DefsOf.Burn)
            {
                result = 1f;
            }
            else if (h.def == DefsOf.Crack)
            {
                result = 0.8f;
            }
            else if (h.def == DefsOf.Crush)
            {
                result = 0.8f;
            }
            else if (h.def == DefsOf.Frostbite)
            {
                result = 0.8f;
            }
            return result;
        }

        internal bool IsRegenInjury(Hediff h)
        {
            return h.Bleeding || h.def == HediffDefOf.Cut || h.def == DefsOf.Burn || h.def == DefsOf.Gunshot || h.def == DefsOf.Scratch || h.def == DefsOf.Stab || h.def == DefsOf.Bruise || h.def == HediffDefOf.Bite || h.def == DefsOf.Shredded || h.IsPermanent() || h.def == DefsOf.Crack || h.def == DefsOf.Crush || h.def == DefsOf.Frostbite;
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref ticksToHeal, "ticksToHeal", 0);
        }

        public override string CompDebugString()
        {
            return "ticksToHeal: " + ticksToHeal;
        }
    }
}
