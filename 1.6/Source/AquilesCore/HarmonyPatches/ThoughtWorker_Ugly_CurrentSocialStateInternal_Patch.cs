using HarmonyLib;
using RimWorld;
using Verse;

[HarmonyPatch(typeof(ThoughtWorker_Ugly), "CurrentSocialStateInternal")]
public static class ThoughtWorker_Ugly_CurrentSocialStateInternal_Patch
{
    [HarmonyPriority(int.MinValue)]
    public static void Postfix(Pawn pawn, Pawn other, ref ThoughtState __result)
    {
        if (__result.Active is false)
        {
            return;
        }

        var beauty = other.GetStatValue(StatDefOf.PawnBeauty);
        var pawnBeauty = pawn.GetStatValue(StatDefOf.PawnBeauty);
        if (pawnBeauty < 0f && beauty < 0f)
        {
            __result = ThoughtState.Inactive;
        }
        else if (beauty <= -3f)
        {
            __result = ThoughtState.ActiveAtStage(2);
        }
    }
}
