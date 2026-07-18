using RimWorld;
using Verse;

namespace AquilesCore;

public class CompUseEffect_InspirationGiver : CompUseEffect
{
	public override void DoEffect(Pawn usedBy)
	{
		base.DoEffect(usedBy);
		var handler = usedBy.mindState.inspirationHandler;
		if (handler.Inspired)
		{
			return;
		}
		var def = handler.GetRandomAvailableInspirationDef();
		if (def == null)
		{
			return;
		}
		handler.TryStartInspiration(def);
	}
}
