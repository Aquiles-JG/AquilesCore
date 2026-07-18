using System.Linq;
using RimWorld;
using Verse;

namespace AquilesCore;

public class CompUseEffect_PassionGiver : CompUseEffect
{
	private const float XpGainAmount = 50000f;

	public override void DoEffect(Pawn usedBy)
	{
		base.DoEffect(usedBy);
		if (usedBy.skills == null)
		{
			return;
		}
		var skill = usedBy.skills.skills.Where(s => !s.TotallyDisabled).RandomElement();
		if (skill == null)
		{
			if (PawnUtility.ShouldSendNotificationAbout(usedBy))
			{
				Messages.Message("Aq_PassionSerumNoSkill".Translate(usedBy.LabelShort, usedBy.Named("USER")), usedBy, MessageTypeDefOf.NeutralEvent);
			}
			return;
		}
		if (skill.passion == Passion.Major)
		{
			int level = skill.GetLevel();
			usedBy.skills.Learn(skill.def, XpGainAmount, direct: true);
			int level2 = skill.GetLevel();
			if (PawnUtility.ShouldSendNotificationAbout(usedBy))
			{
				Messages.Message("Aq_PassionSerumXp".Translate(usedBy.LabelShort, skill.def.LabelCap, level, level2, usedBy.Named("USER")), usedBy, MessageTypeDefOf.PositiveEvent);
			}
			return;
		}
		skill.passion = skill.passion.IncrementPassion();
		if (PawnUtility.ShouldSendNotificationAbout(usedBy))
		{
			Messages.Message("Aq_PassionSerumGained".Translate(usedBy.LabelShort, skill.def.LabelCap, usedBy.Named("USER")), usedBy, MessageTypeDefOf.PositiveEvent);
		}
	}

	public override AcceptanceReport CanBeUsedBy(Pawn p)
	{
		if (p.skills == null)
		{
			return false;
		}
		return base.CanBeUsedBy(p);
	}
}
