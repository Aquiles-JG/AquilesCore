using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace XenotypeRestrictions
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            new Harmony("XenotypeRestrictionsMod").PatchAll();
        }
    }

    public class XenotypeExtension : DefModExtension
    {
        public List<XenotypeDef> exclusiveXenotypes;
        public Dictionary<ThingDef, ThingDef> overridenSpawnedParts;
        public ThingDef customHemogenPack;
    }


    [HarmonyPatch(typeof(PawnApparelGenerator), "GenerateStartingApparelFor")]
    public static class PawnApparelGenerator_GenerateStartingApparelFor_Patch
    {
        public static List<ThingStuffPair> staticList = new List<ThingStuffPair>();
        public static void Prefix(Pawn pawn)
        {
            var xenotype = pawn.genes?.xenotype;
            if (xenotype != null)
            {
                staticList = PawnApparelGenerator.allApparelPairs.Where(x => x.thing.CanWear(xenotype) is false).ToList();
                PawnApparelGenerator.allApparelPairs.RemoveAll(x => staticList.Contains(x));
            }
        }

        public static bool CanWear(this ThingDef apparel, XenotypeDef def)
        {
            var extension = apparel.GetModExtension<XenotypeExtension>();
            if (extension?.exclusiveXenotypes != null)
            {
                return extension.exclusiveXenotypes.Contains(def);
            }
            return true;
        }

        public static void Postfix()
        {
            if (staticList.Any())
            {
                PawnApparelGenerator.allApparelPairs.AddRange(staticList);
                staticList.Clear();
            }
        }
    }

    [HarmonyPatch(typeof(ApparelProperties), "PawnCanWear", new Type[] { typeof(Pawn), typeof(bool) })]
    public static class ApparelProperties_PawnCanWear_Patch
    {
        public static Dictionary<ApparelProperties, ThingDef> mappedApparels = new Dictionary<ApparelProperties, ThingDef>();
        public static void Postfix(ApparelProperties __instance, Pawn pawn, ref bool __result)
        {
            if (__result)
            {
                if (mappedApparels.TryGetValue(__instance, out var def) is false)
                {
                    mappedApparels[__instance] = def = DefDatabase<ThingDef>.AllDefs.FirstOrDefault(x => x.apparel == __instance);
                }
                if (def != null)
                {
                    if (def.CanWear(pawn.genes.xenotype) is false)
                    {
                        __result = false;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(JobGiver_OptimizeApparel), "ApparelScoreGain")]
    public static class JobGiver_OptimizeApparel_ApparelScoreGain_Patch
    {
        public static void Postfix(Pawn pawn, Apparel ap, ref float __result)
        {
            if (ap.def.CanWear(pawn.genes?.xenotype) is false)
            {
                __result = -1000;
            }
        }
    }

    [HarmonyPatch(typeof(EquipmentUtility), "CanEquip", [typeof(Thing), typeof(Pawn), typeof(string), typeof(bool)],
    [ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal])]
    public static class EquipmentUtility_CanEquip_Patch
    {
        public static void Postfix(ref bool __result, Thing thing, Pawn pawn, ref string cantReason)
        {
            if (__result && thing.def.CanWear(pawn.genes?.xenotype) is false)
            {
                cantReason = "WrongXenotype".Translate();
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(MedicalRecipesUtility), "SpawnNaturalPartIfClean")]
    public static class MedicalRecipesUtility_SpawnNaturalPartIfClean_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var field = AccessTools.Field(typeof(BodyPartDef), "spawnThingOnRemoved");
            foreach (var instruction in codeInstructions )
            {
                yield return instruction;
                if (instruction.LoadsField(field))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, 
                        AccessTools.Method(typeof(MedicalRecipesUtility_SpawnNaturalPartIfClean_Patch), "TryOverrideSpawnedPart"));
                }
            }
        }

        public static ThingDef TryOverrideSpawnedPart(ThingDef naturalPart, Pawn pawn)
        {
            if (naturalPart != null)
            {
                var xenotype = pawn.genes?.xenotype?.GetModExtension<XenotypeExtension>();
                if (xenotype != null)
                {
                    if (xenotype.overridenSpawnedParts != null && xenotype.overridenSpawnedParts.TryGetValue(naturalPart, out var otherPart))
                    {
                        return otherPart;
                    }
                }
            }
            return naturalPart;
        }
    }

    [HarmonyPatch(typeof(Recipe_ExtractHemogen), "OnSurgerySuccess")]
    public static class Recipe_ExtractHemogen_OnSurgerySuccess_Patch
    {
        public static void Prefix(Pawn pawn, out ThingDef __state)
        {
            __state = ThingDefOf.HemogenPack;
            var xenotype = pawn.genes?.xenotype?.GetModExtension<XenotypeExtension>();
            if (xenotype?.customHemogenPack != null)
            {
                ThingDefOf.HemogenPack = xenotype.customHemogenPack;
            }
        }

        public static void Postfix(ThingDef __state)
        {
            ThingDefOf.HemogenPack = __state;
        }
    }
}
