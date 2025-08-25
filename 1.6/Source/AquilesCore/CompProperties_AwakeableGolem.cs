using RimWorld;
using Verse;

namespace AquilesCore
{
    public class CompProperties_AwakeableGolem : CompProperties
    {
        public PawnKindDef PawnKindDef;
        public int minIntellectualSkill = 10;
        public string IconLabel;
        public string IconDesc;
        public string IconPath;
        
        public CompProperties_AwakeableGolem()
        {
            compClass = typeof(CompAwakeableGolem);
        }
    }
}