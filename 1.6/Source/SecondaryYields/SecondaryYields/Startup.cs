using HarmonyLib;
using Verse;

namespace SecondaryYields
{
	[StaticConstructorOnStartup]
	public static class Startup
	{
		static Startup()
		{
			new Harmony("SecondaryYields.Mod").PatchAll();
		}
	}
}
