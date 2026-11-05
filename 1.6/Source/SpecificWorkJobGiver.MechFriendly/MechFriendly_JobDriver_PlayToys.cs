using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace SpecificWorkJobGiver.MechFriendly;

// Mechs have no Pawn_InteractionsTracker, so the toy box play toil throws a NullReferenceException
// on its 600-tick interaction. The call is replaced with a null-safe one, which leaves the stack as it was.
[HarmonyPatch]
public static class MechFriendly_JobDriver_PlayToys
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(JobDriver_PlayToys), "<PlayToil>b__14_0");
	}

	private static bool TryInteractWith(Pawn_InteractionsTracker interactions, Pawn recipient, InteractionDef intDef)
	{
		return interactions != null && interactions.TryInteractWith(recipient, intDef);
	}

	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		return MakeInteractionNullSafe(instructions);
	}

	internal static IEnumerable<CodeInstruction> MakeInteractionNullSafe(IEnumerable<CodeInstruction> instructions)
	{
		MethodInfo original = AccessTools.Method(typeof(Pawn_InteractionsTracker), "TryInteractWith");
		MethodInfo replacement = AccessTools.Method(typeof(MechFriendly_JobDriver_PlayToys), "TryInteractWith");
		foreach (CodeInstruction t in instructions)
		{
			if (t.Calls(original))
			{
				t.opcode = OpCodes.Call;
				t.operand = replacement;
			}
			yield return t;
		}
	}
}
