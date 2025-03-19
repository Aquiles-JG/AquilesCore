using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace AquilesCore
{
    [HarmonyPatch(typeof(Thing), "SmeltProducts")]
    public static class Thing_SmeltProducts_Patch
    {
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, Thing __instance)
        {
            var extension = __instance.def.GetModExtension<SmeltableExtension>();
            if (extension != null && extension.randomSmeltProductsCount.HasValue && __instance.def.smeltProducts != null)
            {
                var originalProducts = __instance.def.smeltProducts;
                if (originalProducts != null && originalProducts.Count > 0)
                {
                    var selectedProducts = new List<Thing>();
                    var availableProducts = new List<ThingDefCountClass>(originalProducts);
                    for (int i = 0; i < extension.randomSmeltProductsCount.Value; i++)
                    {
                        ThingDefCountClass productDefCount;
                        if (extension.allowDuplicateItems)
                        {
                            productDefCount = availableProducts.RandomElement();
                        }
                        else
                        {
                            productDefCount = availableProducts.RandomElement();
                            availableProducts.Remove(productDefCount);
                        }

                        if (productDefCount != null)
                        {
                            var thing = ThingMaker.MakeThing(productDefCount.thingDef);
                            thing.stackCount = productDefCount.count;
                            selectedProducts.Add(thing);
                        }
                    }
                    foreach (var item in selectedProducts)
                    {
                        yield return item;
                    }
                    yield break;
                }
            }
            foreach (var item in __result)
            {
                yield return item;
            }
        }
    }
}