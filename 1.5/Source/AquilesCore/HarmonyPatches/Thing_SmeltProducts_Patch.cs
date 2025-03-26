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
                // Ensure count > 0 to avoid errors later
                if (originalProducts != null && originalProducts.Count > 0 && extension.randomSmeltProductsCount.Value > 0)
                {
                    var selectedProducts = new List<Thing>();
                    var availableProducts = new List<ThingDefCountClass>(originalProducts);
                    int picksRemaining = extension.randomSmeltProductsCount.Value;

                    // Handle guaranteed first product
                    if (extension.guaranteeFirstProduct)
                    {
                        var firstProductDefCount = availableProducts[0];
                        var firstThing = ThingMaker.MakeThing(firstProductDefCount.thingDef);
                        firstThing.stackCount = firstProductDefCount.count;
                        selectedProducts.Add(firstThing);
                        if (extension.allowDuplicateItems is false)
                        {
                            availableProducts.RemoveAt(0); // Remove the first item from available pool
                        }
                        picksRemaining--;
                    }

                    // Randomly select remaining products
                    for (int i = 0; i < picksRemaining; i++)
                    {
                        // Check if there are still products available, especially if duplicates aren't allowed
                        if (availableProducts.Count == 0)
                        {
                            break; // No more products to pick from
                        }

                        var productDefCount = availableProducts.RandomElement();
                        if (extension.allowDuplicateItems)
                        {
                            // If duplicates not allowed, pick and remove
                            availableProducts.Remove(productDefCount);
                        }
                        // Create and add the thing
                        var thing = ThingMaker.MakeThing(productDefCount.thingDef);
                        thing.stackCount = productDefCount.count;
                        selectedProducts.Add(thing);
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