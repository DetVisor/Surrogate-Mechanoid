using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace SpecificWorkJobGiver.MechFriendly;

[HarmonyPatch]
public static class MechFriendly_JobDriver_PlayStatic
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(JobDriver_PlayStatic), "<Play>b__3_0");
	}

	private static bool IsSurrogate(Pawn pawn)
	{
		return pawn.def.defName == "Mech_Surrogate";
	}

	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
	{
		bool branchNext = false;
		new List<CodeInstruction>();
		Label interactSkip = il.DefineLabel();
		foreach (CodeInstruction t in instructions)
		{
			if (t.Calls(AccessTools.Method(typeof(Pawn_InteractionsTracker), "TryInteractWith")))
			{
				Label interactJump = il.DefineLabel();
				yield return new CodeInstruction(OpCodes.Ldarg_0);
				yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(JobDriver), "pawn"));
				yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MechFriendly_JobDriver_PlayStatic), "IsSurrogate"));
				yield return new CodeInstruction(OpCodes.Brfalse, interactJump);
				yield return new CodeInstruction(OpCodes.Pop);
				yield return new CodeInstruction(OpCodes.Pop);
				yield return new CodeInstruction(OpCodes.Br, interactSkip);
				t.labels.Add(interactJump);
				yield return t;
				branchNext = true;
			}
			else
			{
				if (branchNext)
				{
					t.labels.Add(interactSkip);
					branchNext = false;
				}
				yield return t;
			}
		}
	}
}
