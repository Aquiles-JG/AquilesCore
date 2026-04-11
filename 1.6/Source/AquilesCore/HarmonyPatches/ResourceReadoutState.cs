using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AquilesCore
{
    public class ResourceReadoutState : IExposable
    {
        public Dictionary<string, int> categoryOpenBits = new Dictionary<string, int>();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref categoryOpenBits, "categoryOpenBits", LookMode.Value, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                categoryOpenBits ??= new Dictionary<string, int>();
            }
        }
    }
}
