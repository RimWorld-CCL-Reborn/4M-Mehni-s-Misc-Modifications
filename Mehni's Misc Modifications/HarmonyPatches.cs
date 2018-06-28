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

            harmony.Patch(AccessTools.Method(typeof(AutoUndrafter), "ShouldAutoUndraft"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(StayWhereIPutYou_Postfix)), null);

            harmony.Patch(AccessTools.Method(typeof(Lord), nameof(Lord.SetJob)), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(FleeTrigger_PostFix)), null);

            //harmony.Patch(AccessTools.Method(typeof(Lord), nameof(Lord.SetJob)), null, null,
            //    new HarmonyMethod(typeof(HarmonyPatches), nameof(FleeTrigger_Transpiler)));

            harmony.Patch(AccessTools.Method(typeof(ManhunterPackIncidentUtility), nameof(ManhunterPackIncidentUtility.ManhunterAnimalWeight)),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(BigManhunterPackFix)), null, null);

            harmony.Patch(AccessTools.Method(typeof(CaravansBattlefield), "CheckWonBattle"), null, null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(ForceExitTimeExtender_Transpiler)));

            harmony.Patch(AccessTools.Method(typeof(SettlementDefeatUtility), nameof(SettlementDefeatUtility.CheckDefeated)), null, null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(ForceExitTimeExtender_Transpiler)));

            harmony.Patch(AccessTools.Method(typeof(Site), "CheckStartForceExitAndRemoveMapCountdown"), null, null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(CheckStartForceExitAndRemoveMapCountdown_Transpiler)));

            harmony.Patch(AccessTools.Method(typeof(PlantProperties), "SpecialDisplayStats"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(DisplayYieldInfo)));

            harmony.Patch(AccessTools.Method(typeof(StartingPawnUtility), nameof(StartingPawnUtility.NewGeneratedStartingPawn)), null, null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(NewGeneratedStartingPawns_Transpiler)));

            harmony.Patch(AccessTools.Method(typeof(Dialog_AssignBuildingOwner), nameof(Dialog_AssignBuildingOwner.DoWindowContents)), null, null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(DoWindowContents_Transpiler)));

            harmony.Patch(AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.NotifyPlayerOfKilled)),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(NotifyPlayerOfKilledAnimal_Prefix)), null, null);

            harmony.Patch(AccessTools.Method(typeof(PawnUtility), nameof(PawnUtility.HumanFilthChancePerCell)), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(HumanFilthChancePerCell_Postfix)));
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
        private static void StayWhereIPutYou_Postfix(AutoUndrafter __instance, ref bool __result)
        {
            if (MeMiMoSettings.modifyAutoUndrafter)
            {
                int lastNonWaitingTick = Traverse.Create(__instance).Field("lastNonWaitingTick").GetValue<int>();
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

                __result = __result
                    && MentalBreakHelper(pawn)
                    && Find.TickManager.TicksGame - lastNonWaitingTick >= 6000 + MeMiMoSettings.extendUndraftTimeBy
                    && GunsFiringHelper();
            }
        }

        private static bool GunsFiringHelper()
        {
            int lastShotHeardAt = 0;
            int ticksToWait = 100;
            if (MeMiMoSettings.whenGunsAreFiring)
            {
                if (Find.SoundRoot.oneShotManager.PlayingOneShots.Any((SampleOneShot s)
                    => (s.subDef.parentDef == SoundDefOf_M4.BulletImpact_Flesh)
                    || (s.subDef.parentDef == SoundDefOf_M4.BulletImpact_Metal)
                    || (s.subDef.parentDef == SoundDefOf_M4.BulletImpact_Wood)
                    || (s.subDef.parentDef == SoundDefOf.BulletImpact_Ground)))
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
                if (MeMiMoSettings.dontExtendWhenMoodAt == "   Extreme Break Risk") return pawn.mindState.mentalBreaker.BreakExtremeIsImminent;
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
                    if (lordJob.lord.Graph.transitions[j].target is LordToil_PanicFlee)
                    {
                        for (int i = 0; i < lordJob.lord.Graph.transitions[j].triggers.Count; i++)
                        {
                            if (lordJob.lord.Graph.transitions[j].triggers[i] is Trigger_FractionPawnsLost)
                            {
                                if (MeMiMoSettings.variableRaidRetreat)
                                    lordJob.lord.Graph.transitions[j].triggers[i] = new Trigger_FractionPawnsLost(randomRetreatvalue);
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
                MethodInfo countDown = AccessTools.Method(typeof(TimedForcedExit), nameof(TimedForcedExit.StartForceExitAndRemoveMapCountdown), new Type[] { });
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

                    if (!patched && instructionList[(i + 1)].operand == countDown)
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

        #region DisplayYieldInfo
        //Thanks to XeoNovaDan
        public static void DisplayYieldInfo(PlantProperties __instance, ref IEnumerable<StatDrawEntry> __result)
        {
            ThingDef harvestedThingDef = Traverse.Create(__instance).Field("harvestedThingDef").GetValue<ThingDef>();
            float harvestYield = Traverse.Create(__instance).Field("harvestYield").GetValue<float>();

            if (harvestedThingDef == null) return;

            string harvestedThingDefLabel = harvestedThingDef.label;

            string extendedYieldInfo = String.Format("M4_HarvestYieldThingDetailInit".Translate(), harvestedThingDefLabel) + "\n\n";
            float thingMarketValue = harvestedThingDef.GetStatValueAbstract(StatDefOf.MarketValue, null);
            extendedYieldInfo += StatDefOf.MarketValue.label.CapitalizeFirst() + ": " + thingMarketValue.ToString();
            if (harvestedThingDef.IsNutritionGivingIngestible)
            {
                float thingNutrition = harvestedThingDef.GetStatValueAbstract(StatDefOf.Nutrition, null);
                FoodTypeFlags thingNutritionType = harvestedThingDef.ingestible.foodType;
                IDictionary<FoodTypeFlags, string> nutritionTypeToReportString = new Dictionary<FoodTypeFlags, string>()
                {
                    {FoodTypeFlags.VegetableOrFruit, "FoodTypeFlags_VegetableOrFruit"}, {FoodTypeFlags.Meat, "FoodTypeFlags_Meat"}, {FoodTypeFlags.Seed, "FoodTypeFlags_Seed"}
                };
                string nutritionTypeReportString = nutritionTypeToReportString.TryGetValue(thingNutritionType, out nutritionTypeReportString) ? nutritionTypeReportString : "StatsReport_OtherStats";
                extendedYieldInfo += "\n" + StatDefOf.Nutrition.label.CapitalizeFirst() + ": " + thingNutrition.ToString() +
                    " (" + nutritionTypeReportString.Translate() + ")";
            }

            if (harvestedThingDef != null && harvestYield > 0)
            {
                StatDrawEntry statDrawEntry = new StatDrawEntry(StatCategoryDefOf.Basics, "M4_HarvestYieldThing".Translate(), harvestedThingDef.label.CapitalizeFirst(), 0, extendedYieldInfo);
                __result = __result.Add(statDrawEntry);
            }
        }
        #endregion DisplayYieldInfo

        #region TutorialStyleRolling (No non-violents)
        public static IEnumerable<CodeInstruction> NewGeneratedStartingPawns_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            MethodInfo tutorialMode = AccessTools.Property(typeof(TutorSystem), nameof(TutorSystem.TutorialMode)).GetGetMethod();
            MethodInfo noNonViolents = AccessTools.Property(typeof(HarmonyPatches), nameof(HarmonyPatches.NoNonViolents)).GetGetMethod();

            List<CodeInstruction> instructionList = codeInstructions.ToList();
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (instructionList[i].operand == tutorialMode) instructionList[i].operand = noNonViolents;
                yield return instructionList[i];
            }
        }

        public static bool NoNonViolents
        {
            get
            {
                return TutorSystem.TutorialMode || MeMiMoSettings.enableTutorialStyleRolling;
            }
        }
        #endregion

        #region showLovers
        public static IEnumerable<CodeInstruction> DoWindowContents_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            MethodInfo pawnName = AccessTools.Property(typeof(Entity), nameof(Entity.LabelCap)).GetGetMethod();
            MethodInfo isLover = AccessTools.Method(typeof(HarmonyPatches), nameof(HarmonyPatches.ShowLovers));
            MethodInfo concat = AccessTools.Method(typeof(HarmonyPatches), nameof(HarmonyPatches.Concatenate));

            List<CodeInstruction> instructionList = codeInstructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (instructionList[i].opcode == OpCodes.Ldloc_S && instructionList[(i + 1)].operand == pawnName)
                {
                    yield return instructionList[i]; //pawn
                    yield return new CodeInstruction(OpCodes.Callvirt, isLover); //call ShowLovers (pawn), get <3 or empty string back
                    yield return instructionList[i]; //pawn
                    yield return new CodeInstruction(OpCodes.Callvirt, pawnName); //get pawn name
                    yield return new CodeInstruction(OpCodes.Callvirt, concat); //Concatenate Showlovers + Pawn name, load onto stack as single string.
                    instructionList[i + 1].opcode = OpCodes.Nop; //we already called pawnName. Could remove it from stack, but nop is easier.
                }
                else
                    yield return instructionList[i];
            }
        }

        private static string Concatenate(string a, string b)
        {
            return b + a;
        }

        public static string ShowLovers(Pawn pawn)
        {
            if (LovePartnerRelationUtility.HasAnyLovePartner(pawn))
            {
                return " ♥";
            }
            return string.Empty;
        }
        #endregion showLovers

        #region DeathMessagesForAnimals;
        private static bool NotifyPlayerOfKilledAnimal_Prefix(Pawn ___pawn)
        {
            if (___pawn.RaceProps.Animal) return MeMiMoSettings.deathMessagesForAnimals;
            return true;
        }
        #endregion DeathMessagesForAnimals

        #region LessLitterLouting
        public static void HumanFilthChancePerCell_Postfix(ref float __result)
        {
            __result *= (MeMiMoSettings.humanFilthRate / 5);
        }
        #endregion
    }

    [DefOf]
    public static class SoundDefOf_M4
    {
        public static SoundDef BulletImpact_Wood;
        public static SoundDef BulletImpact_Flesh;
        public static SoundDef BulletImpact_Metal;
    }
}
