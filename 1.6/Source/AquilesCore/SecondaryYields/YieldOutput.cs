using System.Linq;
using Verse;

namespace AquilesCore
{
    public class YieldOutput
    {
        public IntRange quantityRange;
        public ThingDef thingDef;
        public ThingCategoryDef thingCategory;

        public ThingDef GetThingDef()
        {
            ThingDef defToMake = thingDef;
            if (thingCategory != null)
            {
                var candidates = thingCategory.DescendantThingDefs;
                if (candidates.Any())
                {
                    defToMake = candidates.RandomElement();
                }
            }
            return defToMake;
        }
    }
}
