using System.Xml;
using Verse;

namespace AquilesCore
{
    public class PatchOperationToggle : PatchOperation
    {
        private string toggle;
        private PatchOperation match;
        private PatchOperation nomatch;

        public override bool ApplyWorker(XmlDocument xml)
        {
            var value = true;
            if (toggle == "BeautyMatters")
            {
                value = AquilesCoreMod.settings.beautyMatters;
            }
            else if (toggle == "BodyTypeMatters")
            {
                value = AquilesCoreMod.settings.bodyTypeMatters;
            }
            if (value && match != null)
            {
                return match.Apply(xml);
            }
            if (!value && nomatch != null)
            {
                return nomatch.Apply(xml);
            }
            return true;
        }
    }
}
