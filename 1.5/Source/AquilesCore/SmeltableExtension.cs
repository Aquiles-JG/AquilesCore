using Verse;

namespace AquilesCore
{
    public class SmeltableExtension : DefModExtension
    {
        public int? randomSmeltProductsCount;
        public bool allowDuplicateItems = true;
        public bool guaranteeFirstProduct = false;
    }
}