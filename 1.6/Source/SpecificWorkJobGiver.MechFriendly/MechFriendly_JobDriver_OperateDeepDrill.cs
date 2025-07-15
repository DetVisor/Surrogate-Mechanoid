using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SpecificWorkJobGiver.MechFriendly;

[HarmonyPatch]
public static class MechFriendly_JobDriver_OperateDeepDrill
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(AccessTools.Inner(typeof(JobDriver_OperateDeepDrill), "<>c__DisplayClass1_0"), "<MakeNewToils>b__1");
	}

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        bool shouldModify = ModLister.GetActiveModWithIdentifier("Porio.TunnulerFix") == null;
        MethodInfo learnMethod = AccessTools.Method(typeof(Pawn_SkillTracker), "Learn");

        var code = new List<CodeInstruction>(instructions);

        Label? postEditLabel = null;
        Label? postLearnLabel = null;
        int edits = 0;

        for (int i = 0; i < code.Count; i++)
        {
            var instr = code[i];

            if (!shouldModify)
            {
                yield return instr;
                continue;
            }

            if (instr.opcode == OpCodes.Ldfld && instr.operand is FieldInfo fi && fi.Name == "skills")
            {
                // Insert safety check
                yield return instr; // original ldfld
                yield return new CodeInstruction(OpCodes.Dup);
                postEditLabel = il.DefineLabel();
                postLearnLabel = il.DefineLabel();

                yield return new CodeInstruction(OpCodes.Brtrue_S, postEditLabel.Value);
                yield return new CodeInstruction(OpCodes.Pop);
                yield return new CodeInstruction(OpCodes.Br_S, postLearnLabel.Value);

                edits++;
                continue;
            }

            if (postEditLabel != null)
            {
                instr.labels.Add(postEditLabel.Value);
                postEditLabel = null;
            }

            if (postLearnLabel != null)
            {
                instr.labels.Add(postLearnLabel.Value);
                postLearnLabel = null;
            }

            yield return instr;
        }

        if (shouldModify && edits != 1)
        {
            Log.Warning("[MechFriendly] Making Deep-drills Mech friendly failed");
        }
    }

}
