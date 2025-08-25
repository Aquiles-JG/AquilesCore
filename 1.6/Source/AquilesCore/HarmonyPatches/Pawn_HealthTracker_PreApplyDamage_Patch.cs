using HarmonyLib;
using Verse;

namespace AquilesCore
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "PreApplyDamage")]
    public static class Pawn_HealthTracker_PreApplyDamage_Patch
    {
        public static void Prefix(Pawn_HealthTracker __instance, ref DamageInfo dinfo, ref bool absorbed)
        {
            if (absorbed)
            {
                return;
            }

            Pawn pawn = __instance.pawn;
            if (pawn == null || pawn.Dead)
            {
                return;
            }
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                var comp = hediff.TryGetComp<HediffComp_DamageModifier>();
                if (comp != null)
                {
                    comp.ProcessDamage(ref dinfo, out bool compAbsorbed);
                    if (compAbsorbed)
                    {
                        absorbed = true;
                        return;
                    }
                }
            }
        }
    }
}
