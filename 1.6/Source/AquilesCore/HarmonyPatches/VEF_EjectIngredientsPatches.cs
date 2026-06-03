using System.Text;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace AquilesCore
{
    [HarmonyPatch]
    [StaticConstructorOnStartup]
    public static class VEF_EjectIngredientsPatches
    {
        private const string VEF_PackageId = "OskarPotocki.VanillaFactionsExpanded.Core";
        private const string VEF_CompAdvancedResourceProcessor_TypeName = "PipeSystem.CompAdvancedResourceProcessor";
        private static Texture2D ejectIcon;
        public static Type processorType;
        private static PropertyInfo prop_Process;
        private static PropertyInfo prop_Progress;
        private static PropertyInfo prop_IngredientsOwners;
        private static PropertyInfo prop_OwnerCount;
        private static PropertyInfo prop_OwnerThingDef;
        private static FieldInfo field_CompParent;
        private static FieldInfo field_OwnerLastThingStored;
        private static FieldInfo field_OwnerStuffOfLastThingStored;
        private static MethodInfo method_OwnerReset;
        private static MethodBase cachedInspectMethod;
        private static MethodBase cachedTargetMethod;
        private static bool vefLoaded;

        static VEF_EjectIngredientsPatches()
        {
            vefLoaded = ModsConfig.IsActive(VEF_PackageId);
            if (vefLoaded)
            {
                ejectIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/EjectIngredients", reportFailure: false);
            }
        }

        [HarmonyPrepare]
        public static bool Prepare()
        {
            if (!vefLoaded) return false;

            processorType = AccessTools.TypeByName(VEF_CompAdvancedResourceProcessor_TypeName);
            if (processorType == null)
            {
                Log.Error($"[AquilesCore] Could not find type {VEF_CompAdvancedResourceProcessor_TypeName}. Disabling VEF Eject Ingredients patch.");
                return false;
            }

            cachedTargetMethod = AccessTools.Method(processorType, nameof(ThingComp.CompGetGizmosExtra));
            if (cachedTargetMethod == null)
            {
                Log.Error($"[AquilesCore] Could not find method CompGetGizmosExtra on {VEF_CompAdvancedResourceProcessor_TypeName}. Disabling VEF Eject Ingredients patch.");
                return false;
            }

            prop_Process = AccessTools.Property(processorType, "Process");
            if (prop_Process == null) return Fail("Property 'Process' on CompAdvancedResourceProcessor");

            field_CompParent = AccessTools.Field(processorType, "parent");
            if (field_CompParent == null) return Fail("Field 'parent' on CompAdvancedResourceProcessor");

            var processType = prop_Process.PropertyType;
            prop_Progress = AccessTools.Property(processType, "Progress");
            if (prop_Progress == null) return Fail("Property 'Progress' on Process");

            prop_IngredientsOwners = AccessTools.Property(processType, "IngredientsOwners");
            if (prop_IngredientsOwners == null) return Fail("Property 'IngredientsOwners' on Process");

            var ownersType = prop_IngredientsOwners.PropertyType;
            var ownerType = ownersType.GetGenericArguments().FirstOrDefault();
            if (ownerType == null) return Fail("Generic argument 'ownerType' in IngredientsOwners list");

            prop_OwnerCount = AccessTools.Property(ownerType, "Count");
            if (prop_OwnerCount == null) return Fail("Property 'Count' on ThingAndResourceOwner");

            prop_OwnerThingDef = AccessTools.Property(ownerType, "ThingDef");
            if (prop_OwnerThingDef == null) return Fail("Property 'ThingDef' on ThingAndResourceOwner");

            field_OwnerLastThingStored = AccessTools.Field(ownerType, "lastThingStored");
            if (field_OwnerLastThingStored == null) return Fail("Field 'lastThingStored' on ThingAndResourceOwner");

            field_OwnerStuffOfLastThingStored = AccessTools.Field(ownerType, "stuffOfLastThingStored");
            if (field_OwnerStuffOfLastThingStored == null) return Fail("Field 'stuffOfLastThingStored' on ThingAndResourceOwner");

            method_OwnerReset = AccessTools.Method(ownerType, "Reset");
            if (method_OwnerReset == null) return Fail("Method 'Reset' on ThingAndResourceOwner");

            return true;
        }

        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            return cachedTargetMethod;
        }

        private static bool Fail(string memberName)
        {
            Log.Error($"[AquilesCore] Could not find {memberName}. Disabling VEF Eject Ingredients patch.");
            vefLoaded = false;
            return false;
        }

        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, object __instance)
        {
            if (__result != null)
            {
                foreach (var g in __result)
                    yield return g;
            }

            var process = prop_Process.GetValue(__instance);
            if (process == null) yield break;

            if ((float)prop_Progress.GetValue(process) > 0f) yield break;

            if (!CanEjectIngredients(process)) yield break;

            var action = new Command_Action
            {
                icon = ejectIcon,
                defaultLabel = "Aq_EjectIngredients".Translate(),
                defaultDesc = "Aq_EjectIngredientsDesc".Translate(),
                action = () => EjectIngredients(__instance, process)
            };

            yield return action;
        }

        private static bool CanEjectIngredients(object process)
        {
            var ingredientsOwners = prop_IngredientsOwners.GetValue(process) as System.Collections.IEnumerable;
            foreach (var owner in ingredientsOwners)
            {
                if ((int)prop_OwnerCount.GetValue(owner) > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static void EjectIngredients(object comp, object process)
        {
            var parent = field_CompParent.GetValue(comp) as ThingWithComps;
            var map = parent.Map;
            var position = parent.Position;

            var ingredientsOwners = prop_IngredientsOwners.GetValue(process) as System.Collections.IEnumerable;
            foreach (var owner in ingredientsOwners)
            {
                var thingDef = field_OwnerLastThingStored.GetValue(owner) as ThingDef;
                var count = (int)prop_OwnerCount.GetValue(owner);
                var stuff = field_OwnerStuffOfLastThingStored.GetValue(owner) as ThingDef;

                if (thingDef != null && count > 0)
                {
                    Thing toSpawn = ThingMaker.MakeThing(thingDef, stuff);
                    toSpawn.stackCount = count;
                    GenPlace.TryPlaceThing(toSpawn, position, map, ThingPlaceMode.Near);
                    toSpawn.SetForbidden(true);
                }

                method_OwnerReset.Invoke(owner, null);
            }
        }

        [HarmonyPatch]
        [HotSwappable]
        private static class InspectStringPatch
        {
            [HarmonyPrepare]
            public static bool Prepare()
            {
                if (!vefLoaded) return false;

                cachedInspectMethod = AccessTools.Method(processorType, "CompInspectStringExtra");
                if (cachedInspectMethod == null)
                {
                    return Fail($"CompInspectStringExtra on {VEF_CompAdvancedResourceProcessor_TypeName}");
                }
                return true;
            }

            [HarmonyTargetMethod]
            public static MethodBase TargetMethod() => cachedInspectMethod;
            public static void Postfix(object __instance, ref string __result)
            {
                var process = prop_Process.GetValue(__instance);
                if (process == null) return;
                var owners = prop_IngredientsOwners.GetValue(process) as System.Collections.IEnumerable;
                if (owners == null) return;
                var ingredients = new List<string>();
                foreach (var owner in owners)
                {
                    int count = (int)prop_OwnerCount.GetValue(owner);
                    if (count <= 0) continue;

                    ThingDef thingDef = field_OwnerLastThingStored.GetValue(owner) as ThingDef ?? prop_OwnerThingDef.GetValue(owner) as ThingDef;
                    if (thingDef == null) continue;
                    ingredients.Add($"{count}x {thingDef.label}");
                }
                if (ingredients.Any())
                {
                    __result = __result.ReplaceFirst("%", $"% ({ingredients.ToStringSafeEnumerable()})");
                }
            }
        }
    }
}
