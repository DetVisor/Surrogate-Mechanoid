using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace SpecificWorkJobGiver;

public class JobGiver_SpecificWork : ThinkNode_JobGiver
{
	private float overridePriority = -1f;

	private WorkGiverDef workGiverDef = null;

	private bool factionOnly = true;

	private bool ignoreOtherReservations = false;

	private Dictionary<string, int> minNextJobTicks = new Dictionary<string, int>();

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_SpecificWork jobGiver_SpecificWork = (JobGiver_SpecificWork)base.DeepCopy(resolve);
		jobGiver_SpecificWork.overridePriority = overridePriority;
		jobGiver_SpecificWork.workGiverDef = workGiverDef;
		jobGiver_SpecificWork.factionOnly = factionOnly;
		jobGiver_SpecificWork.ignoreOtherReservations = ignoreOtherReservations;
		return jobGiver_SpecificWork;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		int ticksGame = Find.TickManager.TicksGame;
		minNextJobTicks.TryGetValue(pawn.ThingID, out var value);
		if (!(workGiverDef.Worker is WorkGiver_Scanner workGiver_Scanner) || workGiver_Scanner.ShouldSkip(pawn) || pawn.CurJobDef == JobDefOf.MechCharge || value > ticksGame)
		{
			return null;
		}
		LocalTargetInfo localTargetInfo = GenClosest.ClosestThingReachable(pawn.PositionHeld, pawn.MapHeld, workGiver_Scanner.PotentialWorkThingRequest, workGiver_Scanner.PathEndMode, TraverseParms.For(pawn), 9999f, Validator) ?? workGiver_Scanner.PotentialWorkThingsGlobal(pawn)?.Where(Validator).FirstOrFallback();
		if (localTargetInfo == null)
		{
			return null;
		}
		Job job = workGiver_Scanner.JobOnThing(pawn, localTargetInfo.Thing);
		if (job != null)
		{
			minNextJobTicks[pawn.ThingID] = ticksGame + ((value == ticksGame) ? 100 : 0);
		}
		return job;
		bool Validator(Thing x)
		{
			int result;
			if ((!factionOnly || x.Faction == pawn.Faction) && !x.IsForbidden(pawn))
			{
				CompPowerTrader compPowerTrader = x.TryGetComp<CompPowerTrader>();
				if (compPowerTrader == null || compPowerTrader.PowerOn)
				{
					result = (pawn.CanReserve(x, 1, -1, null, ignoreOtherReservations) ? 1 : 0);
					goto IL_0064;
				}
			}
			result = 0;
			goto IL_0064;
			IL_0064:
			return (byte)result != 0;
		}
	}

	public override float GetPriority(Pawn pawn)
	{
		return (overridePriority < 0f) ? 9f : overridePriority;
	}
}
