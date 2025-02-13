using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace SecondaryYields
{
	[HarmonyPatch(typeof(Mineable), "TrySpawnYield", new Type[] { typeof(Map), typeof(bool), typeof(Pawn) })]
	public static class Mineable_TrySpawnYield
	{
		public static void Postfix(Mineable __instance, Map map, Pawn pawn)
		{
			MineableExtension modExtension = __instance.def.GetModExtension<MineableExtension>();
			if (modExtension != null && Rand.Chance(modExtension.chanceToSpawnSecondaryOutput))
			{
				YieldOutput yieldOutput = modExtension.yieldOutputThings.RandomElement();
				Thing thing2 = ThingMaker.MakeThing(yieldOutput.thingDef);
				thing2.stackCount = yieldOutput.quantityRange.RandomInRange;
				GenPlace.TryPlaceThing(thing2, __instance.Position, map, ThingPlaceMode.Near, ForbidIfNecessary);
			}
			void ForbidIfNecessary(Thing thing, int count)
			{
				if ((pawn == null || !pawn.IsColonist) && thing.def.EverHaulable && !thing.def.designateHaulable)
				{
					thing.SetForbidden(value: true, warnOnFail: false);
				}
			}
		}
	}
}
