using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace AquilesCore
{
	[HarmonyPatch(typeof(Mineable), "TrySpawnYield", new Type[] { typeof(Map), typeof(bool), typeof(Pawn) })]
	public static class Mineable_TrySpawnYield
	{
		public static void Postfix(Mineable __instance, Map map, Pawn pawn)
		{
			SecondaryYieldsHelper.SpawnSecondaryYields(__instance.def, __instance.Position, map, pawn);
		}
	}
}
