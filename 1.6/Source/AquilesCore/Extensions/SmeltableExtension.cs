using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace AquilesCore
{
    public class SmeltableExtension : DefModExtension
    {
        public int? randomSmeltProductsCount;
        public bool allowDuplicateItems = true;
        public bool guaranteeFirstProduct = false;
    }

    [HarmonyPatch(typeof(Thing), "SmeltProducts")]
    public static class Thing_SmeltProducts_Patch
    {
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, Thing __instance)
        {
            var extension = __instance.def.GetModExtension<SmeltableExtension>();
            if (extension != null && extension.randomSmeltProductsCount.HasValue && __instance.def.smeltProducts != null)
            {
                var originalProducts = __instance.def.smeltProducts;
                if (originalProducts != null && originalProducts.Count > 0 && extension.randomSmeltProductsCount.Value > 0)
                {
                    var selectedProducts = new List<Thing>();
                    var availableProducts = new List<ThingDefCountClass>(originalProducts);
                    int picksRemaining = extension.randomSmeltProductsCount.Value;
                    if (extension.guaranteeFirstProduct)
                    {
                        var firstProductDefCount = availableProducts[0];
                        var firstThing = ThingMaker.MakeThing(firstProductDefCount.thingDef);
                        firstThing.stackCount = firstProductDefCount.count;
                        selectedProducts.Add(firstThing);
                        if (extension.allowDuplicateItems is false)
                        {
                            availableProducts.RemoveAt(0);
                        }
                        picksRemaining--;
                    }

                    for (int i = 0; i < picksRemaining; i++)
                    {
                        if (availableProducts.Count == 0)
                        {
                            break;
                        }

                        var productDefCount = availableProducts.RandomElement();
                        if (extension.allowDuplicateItems)
                        {
                            availableProducts.Remove(productDefCount);
                        }
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
