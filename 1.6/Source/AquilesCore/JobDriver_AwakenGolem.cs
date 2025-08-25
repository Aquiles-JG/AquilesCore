using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AquilesCore
{
    public class JobDriver_AwakenGolem : JobDriver
    {
        private const TargetIndex StatueInd = TargetIndex.A;

        public static void TrainFully(Pawn tamer, Pawn tamee)
        {
            foreach (TrainableDef allDef2 in DefDatabase<TrainableDef>.AllDefs)
            {
                if (tamee.training.CanAssignToTrain(allDef2, out var _).Accepted)
                {
                    tamee.training.SetWantedRecursive(allDef2, true);
                    tamee.training.Train(allDef2, tamer, complete: true);
                }
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(StatueInd, PathEndMode.Touch);
            Toil waitToil = new Toil();
            waitToil.initAction = delegate
            {
                Pawn actor = waitToil.actor;
                Building statue = (Building)job.targetA.Thing;
                SoundDefOf.Roof_Collapse.PlayOneShot(new TargetInfo(statue.Position, statue.Map));
                var tmpMesh = LightningBoltMeshPool.RandomBoltMesh;
                WeatherEvent_LightningStrike.DoStrike(statue.Position, statue.Map, ref tmpMesh);
                for (int i = 0; i < 20; i++)
                {
                    var randomCell = statue.Position + GenRadial.RadialPattern[i];
                    if (randomCell.InBounds(statue.Map))
                    {
                        FilthMaker.TryMakeFilth(randomCell, statue.Map, ThingDefOf.Filth_RubbleRock);
                    }
                }
            };
            waitToil.defaultDuration = 120;
            yield return waitToil;
            Toil spawnGolemToil = new Toil();
            spawnGolemToil.initAction = delegate
            {
                Pawn actor = spawnGolemToil.actor;
                Building statue = (Building)job.targetA.Thing;
                var comp = statue.GetComp<CompAwakeableGolem>();
                if (comp != null)
                {
                    var golem = PawnGenerator.GeneratePawn(new PawnGenerationRequest(comp.Props.PawnKindDef, Faction.OfPlayer));
                    var spawnedGolem = (Pawn)GenSpawn.Spawn(golem, statue.Position, statue.Map);
                    if (spawnedGolem.relations != null && actor.relations != null)
                    {
                        actor.relations.AddDirectRelation(PawnRelationDefOf.Bond, spawnedGolem);
                    }
                    TrainFully(actor, spawnedGolem);
                    statue.Destroy();
                    var letterText = "GolemAwoken_Notification".Translate(spawnedGolem.KindLabel, spawnedGolem.LabelShort);
                    Find.LetterStack.ReceiveLetter("LetterLabelGolemAwoken".Translate(), letterText, LetterDefOf.PositiveEvent, spawnedGolem);
                }
            };
            yield return spawnGolemToil;
        }
    }
}
