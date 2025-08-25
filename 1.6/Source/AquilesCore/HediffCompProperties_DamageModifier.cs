using Verse;
using System.Collections.Generic;

namespace AquilesCore
{
    public class DamageModifier
    {
        public DamageDef damageDef;
        public float multiplier = 1f;
    }

    public class HediffCompProperties_DamageModifier : HediffCompProperties
    {
        public List<DamageDef> immunities;
        public List<DamageModifier> damageMultipliers;

        public HediffCompProperties_DamageModifier()
        {
            compClass = typeof(HediffComp_DamageModifier);
        }
    }
}