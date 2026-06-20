using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace AquilesCore
{
    public class StatPart_FacilityJobBonus : StatPart
    {
        private class StatBonusCacheEntry
        {
            public float offset;
            public float factor;
            public int tick;
        }

        private static readonly ConcurrentDictionary<(int, StatDef), StatBonusCacheEntry> _statBonusCache = new ConcurrentDictionary<(int, StatDef), StatBonusCacheEntry>();

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is Pawn pawn && pawn.RaceProps.Humanlike)
            {
                var (offset, factor) = GetOrCalculateStatBonuses(pawn, parentStat);
                val += offset;
                val *= factor;
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.Thing is not Pawn pawn || !pawn.RaceProps.Humanlike)
                return null;

            var relevantFacilities = GetFacilitiesForPawn(pawn)
                .Where(f => f.parent.TryGetComp<CompFacility>().CanBeActive)
                .Where(f => f.Props?.jobStatBonuses != null)
                .ToList();

            if (!relevantFacilities.Any())
                return null;

            var explanation = new StringBuilder();
            var hasFacilityModifiers = false;

            foreach (var comp in relevantFacilities)
            {
                foreach (var jobBonus in comp.Props.jobStatBonuses)
                {
                    if (pawn.jobs?.curJob?.def != jobBonus.jobDef)
                        continue;

                    if (jobBonus.statOffsets != null && jobBonus.statOffsets.Exists(x => x.stat == parentStat))
                    {
                        hasFacilityModifiers = true;
                        var valueStr = jobBonus.statOffsets.First(se => se.stat == parentStat).ValueToStringAsOffset;
                        explanation.AppendLine($"    {comp.parent.LabelCap}: {valueStr}");
                    }

                    if (jobBonus.statFactors != null && jobBonus.statFactors.Exists(x => x.stat == parentStat))
                    {
                        hasFacilityModifiers = true;
                        var factorStr = jobBonus.statFactors.First(se => se.stat == parentStat).ToStringAsFactor;
                        explanation.AppendLine($"    {comp.parent.LabelCap}: x{factorStr}");
                    }
                }
            }

            if (hasFacilityModifiers)
            {
                return "StatsReport_Facilities".Translate() + "\n" + explanation.ToString().TrimEndNewlines();
            }

            return null;
        }

        private static (float offset, float factor) GetOrCalculateStatBonuses(Pawn pawn, StatDef stat)
        {
            var currentTick = 0;
            if (Current.ProgramState == ProgramState.Playing && Find.TickManager != null)
            {
                currentTick = Find.TickManager.TicksGame;
            }

            var cacheKey = (pawn.thingIDNumber, stat);

            if (_statBonusCache.TryGetValue(cacheKey, out StatBonusCacheEntry cachedEntry) && currentTick - cachedEntry.tick < 60)
            {
                return (cachedEntry.offset, cachedEntry.factor);
            }

            var totalOffset = 0f;
            var totalFactor = 1f;

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
            var pawnPos = pawn.Position;
            return pawnPos == building.InteractionCell || building.OccupiedRect().Contains(pawnPos);
        }
    }
}
