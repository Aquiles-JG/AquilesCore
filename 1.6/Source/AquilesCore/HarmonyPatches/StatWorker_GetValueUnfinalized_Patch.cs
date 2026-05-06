using System.Collections.Concurrent;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AquilesCore;

[HarmonyPatch(typeof(StatWorker), "GetValueUnfinalized")]
public static class StatWorker_GetValueUnfinalized_Patch
{
	private class StatBonusCacheEntry
	{
		public float offset;
		public float factor;
		public int tick;
	}

	private static readonly ConcurrentDictionary<(Pawn, StatDef), StatBonusCacheEntry> _statBonusCache = new ConcurrentDictionary<(Pawn, StatDef), StatBonusCacheEntry>();

	public static void Postfix(ref float __result, StatWorker __instance, StatRequest req)
	{
		if (Startup.JobBonusStats.Contains(__instance.stat) && req.Thing is Pawn pawn && pawn.RaceProps.Humanlike)
		{
			var (offset, factor) = GetOrCalculateStatBonuses(pawn, __instance.stat);
			__result += offset;
			__result *= factor;
		}
	}

	private static (float offset, float factor) GetOrCalculateStatBonuses(Pawn pawn, StatDef stat)
	{
		int currentTick = Find.TickManager.TicksGame;
		var cacheKey = (pawn, stat);

		if (_statBonusCache.TryGetValue(cacheKey, out StatBonusCacheEntry cachedEntry) && currentTick - cachedEntry.tick < 60)
		{
			return (cachedEntry.offset, cachedEntry.factor);
		}

		float totalOffset = 0f;
		float totalFactor = 1f;

		foreach (var comp in GetFacilitiesForPawn(pawn))
		{
			if (comp != null && comp.Props.jobStatBonuses != null)
			{
				foreach (var jobBonus in comp.Props.jobStatBonuses)
				{
					if (jobBonus.jobDef != null && pawn.jobs?.curJob?.def == jobBonus.jobDef)
					{
						if (!comp.parent.TryGetComp<CompFacility>().CanBeActive)
							continue;

						totalOffset += jobBonus.statOffsets?.GetStatOffsetFromList(stat) ?? 0f;
						totalFactor *= jobBonus.statFactors?.GetStatFactorFromList(stat) ?? 1f;
					}
				}
			}
		}

		var newEntry = new StatBonusCacheEntry
		{
			offset = totalOffset,
			factor = totalFactor,
			tick = currentTick
		};

		_statBonusCache[cacheKey] = newEntry;
		return (newEntry.offset, newEntry.factor);
	}

	internal static IEnumerable<CompFacilityJobBonus> GetFacilitiesForPawn(Pawn pawn)
	{
		if (pawn.Map == null || !pawn.Spawned || pawn.pather?.Moving == true)
		{
			yield break;
		}

		foreach (var building in pawn.Map.listerBuildings.allBuildingsColonist)
		{
			var affectedComp = building.TryGetComp<CompAffectedByFacilities>();
			if (affectedComp != null)
			{
				foreach (var facility in affectedComp.LinkedFacilitiesListForReading)
				{
					var jobBonusComp = facility.TryGetComp<CompFacilityJobBonus>();
					if (jobBonusComp != null && IsPawnAtBuilding(pawn, building))
					{
						yield return jobBonusComp;
					}
				}
			}
		}
	}

	private static bool IsPawnAtBuilding(Pawn pawn, Building building)
	{
		IntVec3 pawnPos = pawn.Position;
		return pawnPos == building.InteractionCell || building.OccupiedRect().Contains(pawnPos);
	}
}
