using HarmonyLib;
using RimWorld;
using Verse;
namespace AquilesCore;

[HarmonyPatch(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.PrettinessFactor))]
public static class Pawn_RelationsTracker_PrettinessFactor_Patch
{
    public static void Postfix(Pawn_RelationsTracker __instance, Pawn otherPawn, ref float __result)
    {
        if (!__instance.pawn.RaceProps.Humanlike || !otherPawn.RaceProps.Humanlike)
        {
            return;
        }

        var pawnBeauty = __instance.pawn.GetStatValue(StatDefOf.PawnBeauty);
        var otherPawnBeauty = otherPawn.GetStatValue(StatDefOf.PawnBeauty);

        if (pawnBeauty < 0f && otherPawnBeauty < 0f)
        {
            __result = 1f;
        }
    }
}
