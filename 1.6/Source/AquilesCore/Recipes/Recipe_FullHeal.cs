using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AquilesCore
{
    public class Recipe_FullHeal : Recipe_Surgery
    {
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs.ToList();
            foreach (Hediff hediff in hediffs)
            {
                if (hediff is Hediff_Injury)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }
        
        public override string GetLabelWhenUsedOn(Pawn pawn, BodyPartRecord part)
        {
            return recipe.label;
        }
    }
}
