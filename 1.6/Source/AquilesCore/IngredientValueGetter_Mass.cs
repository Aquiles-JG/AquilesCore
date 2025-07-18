using Verse;
using RimWorld;

namespace AquilesCore
{
    public class IngredientValueGetter_Mass : IngredientValueGetter
    {
        public override string BillRequirementsDescription(RecipeDef r, IngredientCount ing)
        {
            return "AA.BillRequires".Translate(ing.GetBaseCount()) + " (" + ing.filter.Summary + ")";
        }

        public override float ValuePerUnitOf(ThingDef t)
        {
            return t.GetStatValueAbstract(StatDefOf.Mass);
        }
    }
}
