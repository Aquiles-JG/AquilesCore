using Verse;

namespace AquilesCore
{
    public class HediffComp_DamageModifier : HediffComp
    {
        public HediffCompProperties_DamageModifier Props => (HediffCompProperties_DamageModifier)props;
        public void ProcessDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            if (Props.immunities != null && Props.immunities.Contains(dinfo.Def))
            {
                absorbed = true;
                return;
            }
            if (Props.damageMultipliers != null)
            {
                foreach (var mod in Props.damageMultipliers)
                {
                    if (mod.damageDef == dinfo.Def)
                    {
                        dinfo.SetAmount(dinfo.Amount * mod.multiplier);
                        break;
                    }
                }
            }
        }
    }
}
