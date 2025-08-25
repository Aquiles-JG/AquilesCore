using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AquilesCore
{
    public class Recipe_PartialHeal : Recipe_Surgery
    {
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            List<Hediff_Injury> injuries = pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>().ToList();
            int injuriesToRemove = Rand.RangeInclusive(3, 5);
            injuriesToRemove = Mathf.Min(injuriesToRemove, injuries.Count);
            
            for (int i = 0; i < injuriesToRemove; i++)
            {
                int index = Rand.Range(0, injuries.Count);
                pawn.health.RemoveHediff(injuries[index]);
                injuries.RemoveAt(index);
            }
        }
        
        public override string GetLabelWhenUsedOn(Pawn pawn, BodyPartRecord part)
        {
            return recipe.label;
        }
    }
}
