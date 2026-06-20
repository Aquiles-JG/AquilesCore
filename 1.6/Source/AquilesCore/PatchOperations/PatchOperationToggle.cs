using System.Xml;
using Verse;

namespace AquilesCore
{
    public class PatchOperationToggle : PatchOperation
    {
        #pragma warning disable CS0649
    	private string toggle;
    	private PatchOperation match;
    	private PatchOperation nomatch;
        #pragma warning restore CS0649

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
