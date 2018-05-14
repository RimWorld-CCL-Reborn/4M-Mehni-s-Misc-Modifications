using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.Sound;
using Verse.AI;
using Verse.AI.Group;
using Harmony;
using RimWorld.Planet;
using System.Reflection.Emit;
using System.Reflection;

namespace Mehni.Misc.Modifications
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("Mehni.RimWorld.4M.Main");

            harmony.Patch(AccessTools.Method(typeof(FoodUtility), nameof(FoodUtility.IsAcceptablePreyFor)), 
                new HarmonyMethod(typeof(HarmonyPatches), nameof(IsAcceptablePreyForBugFix_Prefix)), null, null);

            harmony.Patch(AccessTools.Method(typeof(AutoUndrafter), "ShouldAutoUndraft"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(StayWhereIPutYou_Prefix)), null, null);

            harmony.Patch(AccessTools.Method(typeof(Lord), nameof(Lord.SetJob)), null, 
                new HarmonyMethod(typeof(HarmonyPatches), nameof(FleeTrigger_PostFix)), null);

            //harmony.Patch(AccessTools.Method(typeof(Lord), nameof(Lord.SetJob)), null, null,
            //    new HarmonyMethod(typeof(HarmonyPatches), nameof(FleeTrigger_Transpiler)));

            harmony.Patch(AccessTools.Method(typeof(ManhunterPackIncidentUtility), nameof(ManhunterPackIncidentUtility.ManhunterAnimalWeight)),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(BigManhunterPackFix)), null, null);

            harmony.Patch(AccessTools.Method(typeof(CaravansBattlefield), "CheckWonBattle"), null, null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(ForceExitTimeExtender_Transpiler)));

            harmony.Patch(AccessTools.Method(typeof(FactionBaseDefeatUtility), nameof(FactionBaseDefeatUtility.CheckDefeated)), null, null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(ForceExitTimeExtender_Transpiler)));

            harmony.Patch(AccessTools.Method(typeof(Site), "CheckStartForceExitAndRemoveMapCountdown"), null, null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(CheckStartForceExitAndRemoveMapCountdown_Transpiler)));

            //harmony.Patch(AccessTools.Method(typeof(Dialog_AssignBuildingOwner), nameof(Dialog_AssignBuildingOwner.DoWindowContents)), null, null,
            //    new HarmonyMethod(typeof(HarmonyPatches), nameof(DoWindowContents_Transpiler)));
        }
        

        #region CatsCanHunt
        private static bool IsAcceptablePreyForBugFix_Prefix(ref Pawn predator, ref Pawn prey, ref bool __result)
        {
            if (MeMiMoSettings.catsCanHunt)
            {
                if (!prey.RaceProps.canBePredatorPrey || !prey.RaceProps.IsFlesh || prey.BodySize > predator.RaceProps.maxPreyBodySize)
                {
                    __result = false;
                    return false;
                }

                if (!prey.Downed)
                {
                    if (prey.kindDef.combatPower > 2f * predator.kindDef.combatPower)
                    {
                        __result = false;
                        return false;
                    }
                    float num = prey.kindDef.combatPower * prey.health.summaryHealth.SummaryHealthPercent * (prey.ageTracker.CurLifeStage.bodySizeFactor * prey.RaceProps.baseBodySize);
                    float num2 = predator.kindDef.combatPower * predator.health.summaryHealth.SummaryHealthPercent * (predator.ageTracker.CurLifeStage.bodySizeFactor * predator.RaceProps.baseBodySize);
                    if (num > 0.85f * num2)
                    {
                        __result = false;
                        return false;
                    }
                }
                __result = (predator.Faction == null || prey.Faction == null || predator.HostileTo(prey)) && (predator.Faction != Faction.OfPlayer || prey.Faction != Faction.OfPlayer) && (!predator.RaceProps.herdAnimal || predator.def != prey.def);
                return false;
            }
            else return true;
        }
        #endregion

        #region StayWhereIPutYou
        private static bool StayWhereIPutYou_Prefix(AutoUndrafter __instance, ref bool __result)
        {
            if (MeMiMoSettings.modifyAutoUndrafter)
            {
                int lastNonWaitingTick = Traverse.Create(__instance).Field("lastNonWaitingTick").GetValue<int>();
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

                __result = Find.TickManager.TicksGame - lastNonWaitingTick >= 5000
                    && !Traverse.Create(__instance).Method("AnyHostilePreventingAutoUndraft").GetValue<bool>()
                    && MentalBreakHelper(pawn)
                    && Find.TickManager.TicksGame - lastNonWaitingTick >= 5000 + MeMiMoSettings.extendUndraftTimeBy
                    && GunsFiringHelper();
                return false;
            }
            else return true;
        }

        private static bool GunsFiringHelper()
        {
            int lastShotHeardAt = 0;
            int ticksToWait = 100;
            if (MeMiMoSettings.whenGunsAreFiring)
            {
                if (Find.SoundRoot.oneShotManager.PlayingOneShots.Any((SampleOneShot s)
                    => (s.subDef.parentDef == SoundDefOf.BulletImpactFlesh)
                    || (s.subDef.parentDef == SoundDefOf.BulletImpactMetal)
                    || (s.subDef.parentDef == SoundDefOf.BulletImpactGround)))
                {
                    lastShotHeardAt = Find.TickManager.TicksGame;
                    return false;
                }
                if (Find.TickManager.TicksGame - lastShotHeardAt > ticksToWait)
                {
                    //slight delay between last shot and undraft
                    return false;
                }
            }
            return true;
        }

        private static bool MentalBreakHelper(Pawn pawn)
        {
            if (MeMiMoSettings.allowAutoUndraftAtLowMood)
            {
                if (MeMiMoSettings.dontExtendWhenMoodAt == "   Minor Break Risk") return pawn.mindState.mentalBreaker.BreakMinorIsImminent;
                if (MeMiMoSettings.dontExtendWhenMoodAt == "   Major Break Risk") return pawn.mindState.mentalBreaker.BreakMajorIsImminent;
                if (MeMiMoSettings.dontExtendWhenMoodAt == "   Extreme Break Risk")  return pawn.mindState.mentalBreaker.BreakExtremeIsImminent;
            }
            return false;
        }
        #endregion

        #region DynamicFleeing

        private static void FleeTrigger_PostFix(ref LordJob lordJob)
        {
            float randomRetreatvalue = Rand.Range(MeMiMoSettings.retreatDefeatRange.min, MeMiMoSettings.retreatDefeatRange.max);
            if (lordJob.lord.faction != null && lordJob.lord.faction.def.autoFlee && MeMiMoSettings.variableRaidRetreat)
            {
                for (int j = 0; j < lordJob.lord.Graph.transitions.Count; j++)
                {
                    if (lordJob.lord.Graph.transitions[j].target.GetType() == typeof(LordToil_PanicFlee))
                    {
                        for (int i = 0; i < lordJob.lord.Graph.transitions[j].triggers.Count; i++)
                        {
                            if (lordJob.lord.Graph.transitions[j].triggers[i].GetType() == typeof(Trigger_FractionPawnsLost))
                            {
                                if (MeMiMoSettings.variableRaidRetreat)
                                    lordJob.lord.Graph.transitions[j].triggers[i] = new Trigger_FractionPawnsLost(randomRetreatvalue);
                                Log.Message(randomRetreatvalue.ToString());
                            }
                        }
                    }
                }
            }
        }
        #endregion DynamicFleeing

        #region BigManhunterPacks
        private static bool BigManhunterPackFix(PawnKindDef animal, float points, ref float __result)
        {
            //6000 is based on the Manhunter results table in the devtools. At around 6~7k points, there's only one or two critters dangerous enough.
            if (MeMiMoSettings.enableLargePacks && points >= 6000)
            {
                if (animal.combatPower > 89)
                    __result = 1f;
                return false;
            }
            return true;
        }
        #endregion

        #region INeedMoreTime
        public static IEnumerable<CodeInstruction> ForceExitTimeExtender_Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            if (MeMiMoSettings.allowLongerStays)
            {
                bool patched = false;
                MethodInfo countDown = AccessTools.Method(typeof(TimedForcedExit), nameof(TimedForcedExit.StartForceExitAndRemoveMapCountdown), new Type[] { } );
                MethodInfo countDownWithCount = AccessTools.Method(typeof(TimedForcedExit), nameof(TimedForcedExit.StartForceExitAndRemoveMapCountdown), new Type[] { typeof(int) });
                int timeInTicksToLeave = GenDate.TicksPerDay + (MeMiMoSettings.extraDaysUntilKickedOut * GenDate.TicksPerDay);

                List<CodeInstruction> instructionList = instructions.ToList();
                for (int i = 0; i < instructionList.Count; i++)
                {
                    CodeInstruction instruction = instructionList[i];
                    if (instruction.opcode == OpCodes.Ldc_I4)
                    {
                        //change message to leave
                        instruction.operand = timeInTicksToLeave;
                    }

                    yield return instructionList[i];

                    if (!patched && instructionList[(i+1)].operand == countDown)
                    {
                        //change actual time to leave
                        patched = true;
                        yield return new CodeInstruction(OpCodes.Ldc_I4, timeInTicksToLeave);
                        instructionList[(i + 1)].operand = countDownWithCount;
                    }
                }
            }
            else //becomes a no-op otherwise.
            {
                foreach (CodeInstruction i in instructions)
                {
                    yield return i;
                }
            }
        }

        public static IEnumerable<CodeInstruction> CheckStartForceExitAndRemoveMapCountdown_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            if (MeMiMoSettings.allowLongerStays)
            {
                float timeInTicksToLeave = MeMiMoSettings.extraDaysUntilKickedOut * GenDate.TicksPerDay;
                List<CodeInstruction> instructionList = instructions.ToList();

                for (int i = 0; i < instructionList.Count; i++)
                {
                    CodeInstruction instruction = instructionList[i];
                    yield return instruction;
                    if (instruction.opcode == OpCodes.Mul)
                    {
                        yield return new CodeInstruction(OpCodes.Ldc_R4, timeInTicksToLeave);
                        yield return new CodeInstruction(OpCodes.Add);
                    }
                }
            }
            else //becomes a no-op otherwise.
            {
                foreach (CodeInstruction i in instructions)
                {
                    yield return i;
                }
            }
        }
        #endregion

        //#region showLovers
        //public static IEnumerable<CodeInstruction> DoWindowContents_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        //{
        //    MethodInfo pawnName = AccessTools.Property(typeof(Entity), nameof(Entity.LabelCap)).GetGetMethod();
        //    MethodInfo pawnLabel = AccessTools.Property(typeof(Pawn), nameof(Pawn.Label)).GetGetMethod();
        //    //MethodInfo pawnLongName = AccessTools.Property(typeof(Entity), nameof(Entity.LabelShort)).GetGetMethod();

        //    Pawn pawn = null;
        //    List<CodeInstruction> instructionList = codeInstructions.ToList();

        //    for (int i = 0; i < instructionList.Count; i++)
        //    {
        //        if (instructionList[i].opcode == OpCodes.Ldloc_S && instructionList[(i+1)].operand == pawnName)
        //        {
        //            pawn = instructionList[i].operand as Pawn;
        //            if (LovePartnerRelationUtility.HasAnyLovePartner(pawn))
        //            {

        //            }
        //            Log.Message(instructionList[i].operand.ToString());
        //            instructionList[(i + 1)].operand = ;
        //            Log.Message(pawn?.Label);
        //            //instructionList[i].operand = pawnLabel;
        //            yield return instructionList[i];
        //        }
        //        else
        //            yield return instructionList[i];
        //    }
        //}
        //#endregion showLovers
    }
}
