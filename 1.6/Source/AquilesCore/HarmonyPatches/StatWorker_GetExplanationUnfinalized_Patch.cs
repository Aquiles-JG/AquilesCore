using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AquilesCore;

[HarmonyPatch(typeof(StatWorker), "GetExplanationUnfinalized")]
public static class StatWorker_GetExplanationUnfinalized_Patch
{
	public static void Postfix(ref string __result, StatWorker __instance, StatRequest req, ToStringNumberSense numberSense)
	{
		if (req.Thing is not Pawn pawn || Startup.JobBonusStats.Contains(__instance.stat) is false)
			return;

		var relevantFacilities = StatWorker_GetValueUnfinalized_Patch.GetFacilitiesForPawn(pawn)
			.Where(f => f.parent.TryGetComp<CompFacility>().CanBeActive)
			.Where(f => f.Props?.jobStatBonuses != null)
			.ToList();

		if (!relevantFacilities.Any())
			return;

		var explanation = new StringBuilder();
		bool hasFacilityModifiers = false;

		foreach (var comp in relevantFacilities)
		{
			foreach (var jobBonus in comp.Props.jobStatBonuses)
			{
				if (pawn.jobs?.curJob?.def != jobBonus.jobDef)
					continue;

				if (jobBonus.statOffsets != null && jobBonus.statOffsets.Exists(x => x.stat == __instance.stat))
				{
					hasFacilityModifiers = true;
					string valueStr = jobBonus.statOffsets.First(se => se.stat == __instance.stat).ValueToStringAsOffset;
					explanation.AppendLine($"    {comp.parent.LabelCap}: {valueStr}");
				}

				if (jobBonus.statFactors != null && jobBonus.statFactors.Exists(x => x.stat == __instance.stat))
				{
					hasFacilityModifiers = true;
					string factorStr = jobBonus.statFactors.First(se => se.stat == __instance.stat).ToStringAsFactor;
					explanation.AppendLine($"    {comp.parent.LabelCap}): x{factorStr}");
				}
			}
		}

		if (hasFacilityModifiers)
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("StatsReport_Facilities".Translate());
			stringBuilder.Append(explanation.ToString());
			__result = __result.TrimEndNewlines() +"\n" + stringBuilder.ToString().TrimEndNewlines();
		}
	}
}
