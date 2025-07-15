using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace SpecificWorkJobGiver.MechFriendly
{
    [HarmonyPatch]
    public static class Patch_BottleFeedBaby_FinishAction
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.FirstMethod(
                typeof(JobDriver_BottleFeedBaby),
                m => m.Name.Contains("b__15_2") &&
                     m.ReturnType == typeof(void) &&
                     m.GetParameters().Length == 0);
        }

        static bool Prefix(JobDriver_BottleFeedBaby __instance)
        {
            // Use reflection to access private 'Baby' property
            Pawn baby = AccessTools.PropertyGetter(typeof(JobDriver_BottleFeedBaby), "Baby")
                                   .Invoke(__instance, null) as Pawn;

            // Private field: float initialFoodPercentage
            float initialPercent = Traverse.Create(__instance)
                                           .Field<float>("initialFoodPercentage")
                                           .Value;

            Pawn feeder = __instance.pawn;

            if (baby?.needs?.food == null)
                return false;

            float gained = baby.needs.food.CurLevelPercentage - initialPercent;

            if (gained > 0.6f)
            {
                baby.needs.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.FedMe, feeder);
                feeder.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.FedBaby, baby);
            }

            if (baby.CurJobDef == JobDefOf.BabySuckle)
            {
                baby.jobs?.EndCurrentJob(JobCondition.Succeeded, true);
            }

            return false; // Skip original delegate
        }
    }
}
