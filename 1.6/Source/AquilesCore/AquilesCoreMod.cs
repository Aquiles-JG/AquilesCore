using UnityEngine;
using Verse;

namespace AquilesCore
{
    public class AquilesCoreMod : Mod
    {
        public static AquilesCoreSettings settings;

        public AquilesCoreMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<AquilesCoreSettings>();
        }

        public override string SettingsCategory() => Content.Name;
        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoSettingsWindowContents(inRect);
        }
    }
}
