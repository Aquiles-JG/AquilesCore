using RimWorld;
using Verse;
using Verse.AI.Group;

namespace AquilesCore
{
    public class DeathActionWorker_Golems : DeathActionWorker
    {
        public override void PawnDied(Corpse corpse, Lord prevLord)
        {
            Pawn pawn = corpse.InnerPawn;
            ThingDef leatherDef = pawn.RaceProps.leatherDef;
            if (leatherDef == null)
            {
                return;
            }
            float yieldPercent = 0.7f;
            int leatherAmount = (int)(pawn.GetStatValue(StatDefOf.LeatherAmount) * yieldPercent);
            if (leatherAmount > 0)
            {
                Thing leather = ThingMaker.MakeThing(leatherDef);
                leather.stackCount = leatherAmount;
                GenPlace.TryPlaceThing(leather, corpse.Position, corpse.Map, ThingPlaceMode.Near);
            }
            corpse.Destroy();
        }
    }
}