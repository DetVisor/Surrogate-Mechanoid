using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace SpecificWorkJobGiver.MechFriendly;

// Toddlers adds a crib play type with the same interaction as JobDriver_PlayToys. Toddlers is optional,
// so the target is looked up by name and the patch is skipped when it isn't found.
[HarmonyPatch]
public static class MechFriendly_JobDriver_PlayCrib
{
	private static MethodBase FindTarget()
	{
		return AccessTools.Method(AccessTools.TypeByName("Toddlers.JobDriver_PlayCrib"), "<Play>b__5_0");
	}

	private static bool Prepare()
	{
		return FindTarget() != null;
	}

	private static MethodBase TargetMethod()
	{
		return FindTarget();
	}

	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		return MechFriendly_JobDriver_PlayToys.MakeInteractionNullSafe(instructions);
	}
}
