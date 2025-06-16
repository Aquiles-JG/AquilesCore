using HarmonyLib;
using RimWorld;
using Verse;

[HarmonyPatch(typeof(ThoughtWorker_Pretty), "CurrentSocialStateInternal")]
public static class ThoughtWorker_Pretty_CurrentSocialStateInternal_Patch
{
    [HarmonyPriority(int.MinValue)]
    public static void Postfix(Pawn pawn, Pawn other, ref ThoughtState __result)
    {
        if (__result.Active is false)
        {
            return;
        }

        var beauty = other.GetStatValue(StatDefOf.PawnBeauty);
        if (beauty >= 3f)
        {
            __result = ThoughtState.ActiveAtStage(2);
        }
    }
}
