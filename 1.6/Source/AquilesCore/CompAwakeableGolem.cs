using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AquilesCore
{
    public class CompAwakeableGolem : ThingComp
    {
        public CompProperties_AwakeableGolem Props => (CompProperties_AwakeableGolem)props;
        
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            
            if (parent.Faction == Faction.OfPlayer)
            {
                Command_Action command = new Command_Action();
                command.defaultLabel = Props.IconLabel.Translate();
                command.defaultDesc = Props.IconDesc.Translate(parent.def.label);
                command.icon = ContentFinder<Texture2D>.Get(Props.IconPath);
                command.action = delegate
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    foreach (Pawn pawn in Find.CurrentMap.mapPawns.FreeColonistsSpawned)
                    {
                        if (pawn.skills.GetSkill(SkillDefOf.Intellectual).Level >= Props.minIntellectualSkill)
                        {
                            Pawn pawnForClosure = pawn;
                            options.Add(new FloatMenuOption(pawnForClosure.LabelCap, delegate
                            {
                                Job job = JobMaker.MakeJob(DefsOf.Aq_AwakenGolem, parent);
                                job.count = 1;
                                pawnForClosure.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                            }));
                        }
                    }
                    if (options.Count == 0)
                    {
                        options.Add(new FloatMenuOption("NoEligiblePawns".Translate(), null));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                };
                yield return command;
            }
        }
    }
}
