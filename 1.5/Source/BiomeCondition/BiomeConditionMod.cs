using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using UnityEngine;
using Verse;

namespace BiomeCondition
{
    public class BiomeConditionMod : Mod
    {
        public BiomeConditionMod(ModContentPack pack) : base(pack)
        {
			new Harmony("BiomeConditionMod").PatchAll();
        }
    }
    public class MineableExtension : DefModExtension
    {
        public List<BiomeRecord> adjustToMineableScatterCommonality;
        public bool onlyCoasts;
        public bool onlyCaves;
    }

    [HarmonyPatch(typeof(BuildingProperties), "SpecialDisplayStats")]
    public static class BuildingProperties_SpecialDisplayStats_Patch
    {
        public static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> __result, ThingDef parentDef, StatRequest req)
        {
            foreach (var r in __result)
            {
                yield return r;
            }
            if (req.Thing is not null)
            {
                var extension = req.Thing.def.GetModExtension<MineableExtension>();
                if (extension != null)
                {
                    if (extension.adjustToMineableScatterCommonality != null)
                    {
                        foreach (var record in extension.adjustToMineableScatterCommonality)
                        {
                            if (record.commonality < 0 && req.Thing.def.building.mineableScatterCommonality - -record.commonality <= 0)
                            {
                                yield return new StatDrawEntry(StatCategoryDefOf.Building, "BM.BiomeCommonality".Translate(record.biome.label), "No".Translate(), "BM.BiomeCommonalityDesc".Translate(), 5995);
                            }
                            else
                            {
                                yield return new StatDrawEntry(StatCategoryDefOf.Building, "BM.BiomeCommonality".Translate(record.biome.label), (record.commonality / req.Thing.def.building.mineableScatterCommonality).ToStringWithSign("0.#%"), "BM.BiomeCommonalityDesc".Translate(), 5995);
                            }
                        }
                    }

                    yield return new StatDrawEntry(StatCategoryDefOf.Building, "BM.OnlyCoasts".Translate(), extension.onlyCoasts.ToStringYesNo(), "BM.OnlyCoastsDesc".Translate(), 5994);
                    yield return new StatDrawEntry(StatCategoryDefOf.Building, "BM.OnlyCaves".Translate(), extension.onlyCaves.ToStringYesNo(), "BM.OnlyCavesDesc".Translate(), 5994);
                }
            }
        }
    }
    [HarmonyPatch(typeof(GenStep_ScatterLumpsMineable), "ScatterAt")]
    public static class GenStep_ScatterLumpsMineable_ScatterAt_Patch
    {
        public static Map mapBeingGenerated;
        public static void Prefix(Map map) => mapBeingGenerated = map;
        public static void Postfix(Map map) => mapBeingGenerated = null;
    }
    [HarmonyPatch]
    public static class GenStep_ScatterLumpsMineable_ChooseThingDef
    {
        [HarmonyTargetMethod] public static MethodBase TargetMethod()
        {
            return typeof(GenStep_ScatterLumpsMineable).GetMethods(AccessTools.all).First(x => x.Name.Contains("<ChooseThingDef>b"));
        }

        public static void Postfix(ref float __result, ThingDef d)
        {
            if (GenStep_ScatterLumpsMineable_ScatterAt_Patch.mapBeingGenerated != null && d?.building != null && __result == d.building.mineableScatterCommonality)
            {
                var map = GenStep_ScatterLumpsMineable_ScatterAt_Patch.mapBeingGenerated;
                var extension = d.GetModExtension<MineableExtension>();
                if (extension != null)
                {
                    if (extension.onlyCoasts && Find.World.CoastDirectionAt(map.Tile) == Rot4.Invalid)
                    {
                        __result = 0;
                    }
                    else if (extension.onlyCaves && Find.World.HasCaves(map.Tile) is false)
                    {
                        __result = 0;
                    }
                    else if (extension.adjustToMineableScatterCommonality != null)
                    {
                        var record = extension.adjustToMineableScatterCommonality.FirstOrDefault(x => x.biome == map.Biome);
                        if (record != null)
                        {
                            __result += record.commonality;
                        }
                    }
                    __result = Mathf.Max(0, __result);
                }
            }
        }
    }
    public class BiomeRecord
    {
        public BiomeDef biome;

        public float commonality;
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "biome", xmlRoot);
            commonality = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
        }
    }
}
