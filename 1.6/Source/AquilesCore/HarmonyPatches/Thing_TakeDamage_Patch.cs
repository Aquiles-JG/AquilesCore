using HarmonyLib;
using Verse;

namespace AquilesCore
{
    [HarmonyPatch(typeof(Thing), nameof(Thing.TakeDamage))]
    public static class Thing_TakeDamage_Patch
    {
        public static bool Prefix(Thing __instance, DamageInfo dinfo, ref DamageWorker.DamageResult __result)
        {
            if (__instance is Pawn pawn)
            {
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    var comp = hediff.TryGetComp<HediffComp_DamageModifier>();
                    if (comp != null)
                    {
                        comp.ProcessDamage(ref dinfo, out bool compAbsorbed);
                        if (compAbsorbed)
                        {
                            __result = new DamageWorker.DamageResult();
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }

}
