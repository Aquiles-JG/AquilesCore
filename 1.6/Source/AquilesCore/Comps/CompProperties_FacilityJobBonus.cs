using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AquilesCore;

public class CompProperties_FacilityJobBonus : CompProperties_Facility
{
	public List<JobStatBonus> jobStatBonuses;

	public CompProperties_FacilityJobBonus()
	{
		compClass = typeof(CompFacilityJobBonus);
	}
}

public class JobStatBonus
{
	public JobDef jobDef;

	public List<StatModifier> statOffsets;

	public List<StatModifier> statFactors;
}
