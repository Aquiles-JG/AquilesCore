using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AquilesCore
{
    public static class RottenDietUtility
    {
        public static List<Func<Pawn, bool>> customCanEatRotten = new List<Func<Pawn, bool>>();
        public static List<Func<Pawn, bool>> customCanEatBones = new List<Func<Pawn, bool>>();
        public static List<ThingDef> boneDefs = new List<ThingDef>();

        public static bool CanEatRotten(Pawn pawn)
        {
            if (HasRottenDietGene(pawn))
            {
                return true;
            }
            for (var i = 0; i < customCanEatRotten.Count; i++)
            {
                if (customCanEatRotten[i](pawn))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CanEatBones(Pawn pawn)
        {
            for (var i = 0; i < customCanEatBones.Count; i++)
            {
                if (customCanEatBones[i](pawn))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsBoneDef(ThingDef def)
        {
            for (var i = 0; i < boneDefs.Count; i++)
            {
                if (boneDefs[i] == def)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool HasRottenDietGene(Pawn pawn)
        {
            if (pawn?.genes == null)
            {
                return false;
            }
            var genes = pawn.genes.GenesListForReading;
            for (var i = 0; i < genes.Count; i++)
            {
                var gene = genes[i];
                if (gene.Active && gene.def.GetModExtension<RottenDietExtension>() != null)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsBlacklistedCorpse(Corpse corpse)
        {
            if (corpse.questTags != null && corpse.questTags.Any())
            {
                return true;
            }
            var inner = corpse.InnerPawn;
            if (inner == null)
            {
                return false;
            }
            if (inner.questTags != null && inner.questTags.Any())
            {
                return true;
            }
            return false;
        }
    }
}
