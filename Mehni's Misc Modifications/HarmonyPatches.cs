using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.Sound;
using Verse.AI;
using Verse.AI.Group;
using Harmony;
using RimWorld.Planet;
using System.Reflection.Emit;
using System.Reflection;
using UnityEngine;

namespace Mehni.Misc.Modifications
{

    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("Mehni.RimWorld.4M.Main");
            //HarmonyInstance.DEBUG = true;

            harmony.Patch(AccessTools.Method(typeof(IncidentWorker_HerdMigration), "GenerateAnimals"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(BigHerds_Transpiler)));

            harmony.Patch(AccessTools.Method(typeof(AutoUndrafter), "ShouldAutoUndraft"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(StayWhereIPutYou_Postfix)));

            harmony.Patch(AccessTools.Method(typeof(Lord), nameof(Lord.SetJob)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(FleeTrigger_PostFix)));

            harmony.Patch(AccessTools.Method(typeof(ManhunterPackIncidentUtility), nameof(ManhunterPackIncidentUtility.ManhunterAnimalWeight)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(BigManhunterPackFix)));

            harmony.Patch(AccessTools.Method(typeof(CaravansBattlefield), "CheckWonBattle"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(ForceExitTimeExtender_Transpiler)));

            harmony.Patch(AccessTools.Method(typeof(SettlementDefeatUtility), nameof(SettlementDefeatUtility.CheckDefeated)),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(ForceExitTimeExtender_Transpiler)));

            harmony.Patch(AccessTools.Method(typeof(Site), "CheckStartForceExitAndRemoveMapCountdown"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(CheckStartForceExitAndRemoveMapCountdown_Transpiler)));

            harmony.Patch(AccessTools.Method(typeof(PlantProperties), "SpecialDisplayStats"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(DisplayYieldInfo)));

            harmony.Patch(AccessTools.Method(typeof(StartingPawnUtility), nameof(StartingPawnUtility.NewGeneratedStartingPawn)),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(NewGeneratedStartingPawns_Transpiler)));

            harmony.Patch(AccessTools.Method(typeof(Dialog_AssignBuildingOwner), nameof(Dialog_AssignBuildingOwner.DoWindowContents)),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(DoWindowContents_Transpiler)));

            harmony.Patch(AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.NotifyPlayerOfKilled)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(NotifyPlayerOfKilledAnimal_Prefix)));

            harmony.Patch(AccessTools.Method(typeof(PawnUtility), nameof(PawnUtility.HumanFilthChancePerCell)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HumanFilthChancePerCell_Postfix)));

            //harmony.Patch(AccessTools.Method(typeof(Building_Turret), "OnAttackedTarget"), null, null,
            //    new HarmonyMethod(typeof(HarmonyPatches), nameof(OnAttackedTarget_Transpiler)));

            harmony.Patch(AccessTools.Method(typeof(FoodUtility), nameof(FoodUtility.GetPreyScoreFor)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(GetPreyScoreFor_Postfix)));

            harmony.Patch(AccessTools.Method(typeof(WorkGiver_InteractAnimal), "CanInteractWithAnimal"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(CanInteractWithAnimal_Postfix)));

            harmony.Patch(AccessTools.Property(typeof(Dialog_MessageBox), "InteractionDelayExpired").GetGetMethod(true),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(YesImAModderStopAskingMe)));

            harmony.Patch(AccessTools.Method(typeof(DebugThingPlaceHelper), nameof(DebugThingPlaceHelper.DebugSpawn)),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(TranspileDebugSpawn)));

            harmony.Patch(AccessTools.Property(typeof(Log), "ReachedMaxMessagesLimit").GetGetMethod(true),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ReachedMaxMessagesLimit_Postfix)));

            //harmony.Patch(AccessTools.Method(typeof(RelationsUtility), nameof(RelationsUtility.IsDisfigured)), null,
            //    new HarmonyMethod(typeof(HarmonyPatches), nameof(IsDisfigured_Postfix)));

            harmony.Patch(AccessTools.Method(typeof(Page_ConfigureStartingPawns), "DoNext"),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ConfigureStartingPawnsDoNextPrefix)));

            harmony.Patch(AccessTools.Method(typeof(IncidentWorker_RefugeeChased), "TryExecuteWorker"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(TryExecuteWorker_Transpiler)));

            harmony.Patch(AccessTools.Method(typeof(PawnUIOverlay), nameof(PawnUIOverlay.DrawPawnGUIOverlay)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(DrawPawnGUIOverlay_Postfix)));

            harmony.Patch(AccessTools.Method(typeof(Listing_TreeThingFilter), "DoThingDef"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(DoThingDef_Transpiler)));
        }

        private static readonly IntRange AnimalsCount = new IntRange(30, 50);

        public static IEnumerable<CodeInstruction> BigHerds_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            if (MeMiMoSettings.bigAnimalMigrations)
            {
                List<CodeInstruction> instructionList = instructions.ToList();
                FieldInfo useMoreAnimals = AccessTools.Field(typeof(HarmonyPatches), "AnimalsCount");

                for (int i = 0; i < instructionList.Count; i++)
                {
                    if (instructionList[i].opcode == OpCodes.Ldsfld)
                        instructionList[i].operand = useMoreAnimals;
                    //uses AnimalsCount in 4M instead of default.

                    yield return instructionList[i];
                }
            }
            else
                foreach (var item in instructions)
                {
                    yield return item;
                }
        }


        #region StayWhereIPutYou
        private static void StayWhereIPutYou_Postfix(ref bool __result, int ___lastNonWaitingTick, Pawn ___pawn)
        {
            if (MeMiMoSettings.modifyAutoUndrafter)
            {
                if (__result)
                {
                    if (!MentalBreakHelper(___pawn))
                    {
                        __result = (Find.TickManager.TicksGame - ___lastNonWaitingTick >= 6000 + MeMiMoSettings.extendUndraftTimeBy && GunsFiringHelper());
                    }
                }
            }
        }

        private static bool GunsFiringHelper()
        {
            if (MeMiMoSettings.whenGunsAreFiring)
            {
                if (Find.SoundRoot.oneShotManager.PlayingOneShots.Any((SampleOneShot s)
                    => (s.subDef.parentDef == DefOf_M4.BulletImpact_Flesh)
                    || (s.subDef.parentDef == DefOf_M4.BulletImpact_Metal)
                    || (s.subDef.parentDef == DefOf_M4.BulletImpact_Wood)
                    || (s.subDef.parentDef == SoundDefOf.BulletImpact_Ground)))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool MentalBreakHelper(Pawn pawn)
        {
            if (MeMiMoSettings.allowAutoUndraftAtLowMood)
            {
                switch (MeMiMoSettings.dontExtendWhenMoodAt)
                {
                    case "   Minor Break Risk":
                        return pawn.mindState.mentalBreaker.BreakMinorIsImminent || pawn.mindState.mentalBreaker.BreakMajorIsImminent || pawn.mindState.mentalBreaker.BreakExtremeIsImminent;
                    case "   Major Break Risk":
                        return pawn.mindState.mentalBreaker.BreakMajorIsImminent || pawn.mindState.mentalBreaker.BreakExtremeIsImminent;
                    case "   Extreme Break Risk":
                        return pawn.mindState.mentalBreaker.BreakExtremeIsImminent;
                }
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

                    if (!patched && instructionList[i + 1].operand == countDown)
                    {
                        //change actual time to leave
                        patched = true;
                        yield return new CodeInstruction(OpCodes.Ldc_I4, timeInTicksToLeave);
                        instructionList[i + 1].operand = countDownWithCount;
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

            string extendedYieldInfo = string.Format("M4_HarvestYieldThingDetailInit".Translate(), harvestedThingDefLabel) + "\n\n";
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

            if (harvestYield > 0)
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
            foreach (CodeInstruction t in instructionList)
            {
                if (t.operand == tutorialMode) t.operand = noNonViolents;
                yield return t;
            }
        }

        public static bool NoNonViolents => TutorSystem.TutorialMode || MeMiMoSettings.enableTutorialStyleRolling;

        static bool returnvalue = false;

        public static bool ConfigureStartingPawnsDoNextPrefix(Page_ConfigureStartingPawns __instance, Pawn ___curPawn)
        {
            MethodInfo runMe = AccessTools.Method(typeof(Page_ConfigureStartingPawns), "DoNext");

            List<Pawn> tmpList = new List<Pawn>(Find.GameInitData.startingPawnCount);
            tmpList.AddRange(Find.GameInitData.startingAndOptionalPawns.Take(Find.GameInitData.startingPawnCount));
            if (!tmpList.Contains(___curPawn))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation($"Currently viewed colonist is not selected for landing. Are you sure you wish to embark without {___curPawn.LabelCap}?",
                                                            () => { returnvalue = true; runMe.Invoke(__instance, new object[] { }); }));
            }
            else returnvalue = true;
            tmpList.Clear();
            return returnvalue;
        }

        #endregion

        #region showLovers
        public static IEnumerable<CodeInstruction> DoWindowContents_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            MethodInfo getViewRectWidth = AccessTools.Property(typeof(Rect), nameof(Rect.width)).GetGetMethod();
            MethodInfo getPawnName = AccessTools.Property(typeof(Entity), nameof(Entity.LabelCap)).GetGetMethod();
            MethodInfo makeReeect = AccessTools.Method(typeof(HarmonyPatches), nameof(HarmonyPatches.ShowHeart));

            List<CodeInstruction> instructionList = codeInstructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (i > 2 && instructionList[i - 1].operand == getPawnName)
                {
                    yield return instructionList[i];
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 1);
                    yield return new CodeInstruction(OpCodes.Call, getViewRectWidth);
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 0.55f);
                    yield return new CodeInstruction(OpCodes.Mul); //viewrect.width * 0.55
                    yield return new CodeInstruction(OpCodes.Ldloc_2); //y
                    yield return new CodeInstruction(instructionList[i - 2]); //pawn
                    yield return new CodeInstruction(OpCodes.Call, makeReeect);
                    yield return new CodeInstruction(OpCodes.Brtrue, instructionList.Where(x => x != null && x.labels != null && x.labels.Any()).Skip(instructionList.Where(x => x != null && x.labels != null && x.labels.Any()).Count() - 2).First().labels.First()); //2nd to last ret
                }
                else
                    yield return instructionList[i];
            }
        }

        private static readonly Texture2D BondIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Bond");
        private static readonly Texture2D BondBrokenIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/BondBroken");

        private static bool ShowHeart(float x, float y, Pawn pawn)
        {
            Texture2D iconFor;
            if (pawn == null || !pawn.IsColonist)
                return false;

            DirectPawnRelation directPawnRelation = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(pawn, false);
            if (directPawnRelation == null || directPawnRelation.otherPawn == null)
                iconFor = null;

            else if (!directPawnRelation.otherPawn.IsColonist || directPawnRelation.otherPawn.IsWorldPawn() || !directPawnRelation.otherPawn.relations.everSeenByPlayer)
                iconFor = null;

            else if (pawn.ownership?.OwnedBed != null && pawn.ownership?.OwnedBed == directPawnRelation.otherPawn.ownership?.OwnedBed)
                iconFor = BondIcon;

            else
                iconFor = BondBrokenIcon;

            if (iconFor != null)
            {
                Vector2 iconSize = new Vector2(iconFor.width, iconFor.height);
                Rect drawRect = new Rect(x, y, iconSize.x, iconSize.y);
                TooltipHandler.TipRegion(drawRect, directPawnRelation.otherPawn.LabelCap);
                GUI.DrawTexture(drawRect, iconFor);

                if (iconFor == BondBrokenIcon && Mouse.IsOver(drawRect) && Input.GetMouseButtonDown(0))
                {
                    if (pawn.ownership?.OwnedBed?.MaxAssignedPawnsCount >= 2)
                    {
                        pawn.ownership.OwnedBed.TryAssignPawn(directPawnRelation.otherPawn);
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion showLovers

        #region DeathMessagesForAnimals;
        private static bool NotifyPlayerOfKilledAnimal_Prefix(Pawn ___pawn)
        {
            return !___pawn.RaceProps.Animal || MeMiMoSettings.deathMessagesForAnimals;
        }
        #endregion DeathMessagesForAnimals

        #region LessLitterLouting
        public static void HumanFilthChancePerCell_Postfix(ref float __result)
        {
            __result *= (MeMiMoSettings.humanFilthRate / 5);
        }
        #endregion



        //Courtesy XND

        #region AnimalHandlingSanity
        // 'Totally didn't almost forget to actually copypaste the testing code' edition
        public static void GetPreyScoreFor_Postfix(Pawn predator, Pawn prey, ref float __result)
        {
            if (predator.Faction == Faction.OfPlayer && MeMiMoSettings.obedientPredatorsDeferHuntingTameDesignatedAnimals
                                                     && predator.training.HasLearned(TrainableDefOf.Obedience)
                                                     && prey.Map.designationManager.DesignationOn(prey, DesignationDefOf.Tame) != null)
            {
                __result -= 35f;
            }
        }

        public static void CanInteractWithAnimal_Postfix(Pawn pawn, ref bool __result)
        {
            int hourInteger = GenDate.HourInteger(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(pawn.MapHeld.Tile).x);
            if (hourInteger >= MeMiMoSettings.animalInteractionHourLimit && __result)
            {
                JobFailReason.Is("M4_CantInteractAnimalWillFallAsleepSoon".Translate());
                __result = false;
            }
        }
        #endregion

        //#region HideDisfigurement
        //public static void IsDisfigured_Postfix(ref bool __result, Pawn pawn)
        //{
        //    if (MeMiMoSettings.apparelHidesDisfigurement && Find.TickManager.TicksGame % 200 == 0 && __result)
        //    {
        //        List<Apparel> wornApparel = pawn.apparel.WornApparel;
        //        List<Hediff> disfiguringHediffs = pawn.health.hediffSet.hediffs.Where(h => h.Part.def.beautyRelated).ToList();
        //        List<bool?> eachHediffCovered = new List<bool?>();
        //        // Stairway to heaven
        //        foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
        //        {
        //            foreach (BodyPartGroupDef hediffGroup in hediff.Part.groups)
        //                foreach (Apparel apparel in wornApparel)
        //                {
        //                    foreach (BodyPartGroupDef apparelGroup in apparel.def.apparel.bodyPartGroups)
        //                        if (apparelGroup == hediffGroup)
        //                        {
        //                            eachHediffCovered.Add(true);
        //                            goto NextHediff;
        //                        }
        //                    eachHediffCovered.Add(false);
        //                }
        //            NextHediff:;
        //        }
        //        __result = eachHediffCovered.First(b => false) == null;
        //    }
        //}
        //#endregion

        #region ToolsForModders
        private static void YesImAModderStopAskingMe(ref bool __result)
        {
            if (MeMiMoSettings.iAmAModder)
                __result = true;
        }

        #region DevModeSpawning
        public static IEnumerable<CodeInstruction> TranspileDebugSpawn(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            MethodInfo randomStuffFor = AccessTools.Method(typeof(GenStuff), nameof(GenStuff.RandomStuffFor));
            MethodInfo getStuffDefFromSettings = AccessTools.Method(typeof(HarmonyPatches), nameof(GetStuffDefFromSettings));
            MethodInfo generateQualityRandomEqualChance = AccessTools.Method(typeof(QualityUtility), nameof(QualityUtility.GenerateQualityRandomEqualChance));
            MethodInfo generateQualityFromSettings = AccessTools.Method(typeof(HarmonyPatches), nameof(GenerateQualityFromSettings));

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.opcode == OpCodes.Call)
                {
                    if (instruction.operand == randomStuffFor)
                        instruction.operand = getStuffDefFromSettings;
                    else if (instruction.operand == generateQualityRandomEqualChance)
                        instruction.operand = generateQualityFromSettings;
                }

                yield return instruction;
            }
        }

        public static ThingDef GetStuffDefFromSettings(ThingDef def)
        {
            if (def.MadeFromStuff && MeMiMoSettings.chooseItemStuff && MeMiMoSettings.stuffDefName != "" && DefDatabase<ThingDef>.GetNamed(MeMiMoSettings.stuffDefName) != null)
                return DefDatabase<ThingDef>.GetNamed(MeMiMoSettings.stuffDefName);
            return GenStuff.RandomStuffFor(def);
        }

        public static QualityCategory GenerateQualityFromSettings()
        {
            if (!MeMiMoSettings.forceItemQuality)
                return QualityUtility.GenerateQualityRandomEqualChance();

            switch (MeMiMoSettings.forcedItemQuality)
            {
                case 0:
                    return QualityCategory.Awful;
                case 1:
                    return QualityCategory.Poor;
                case 3:
                    return QualityCategory.Good;
                case 4:
                    return QualityCategory.Excellent;
                case 5:
                    return QualityCategory.Masterwork;
                case 6:
                    return QualityCategory.Legendary;
                default:
                    return QualityCategory.Normal;
            }
        }
        #endregion DevModeSpawning

        public static void ReachedMaxMessagesLimit_Postfix(ref bool __result)
        {
            if (MeMiMoSettings.iAmAModder)
                __result = false;
        }
        #endregion ToolsForModders

        #region BetterHostileReadouts
        // Thanks Mehni... :sadwinnie:
        public static IEnumerable<CodeInstruction> TryExecuteWorker_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            MethodInfo alteredHostileFactionPawnReadout = AccessTools.Method(typeof(HarmonyPatches), nameof(AlteredHostileFactionPawnReadout));

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.opcode == OpCodes.Ldfld && instruction.operand == AccessTools.Field(typeof(FactionDef), "pawnsPlural"))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 5);

                    instruction.opcode = OpCodes.Call;
                    instruction.operand = alteredHostileFactionPawnReadout;
                }

                yield return instruction;
            }
        }

        private static string AlteredHostileFactionPawnReadout(FactionDef faction, IEnumerable<PawnKindDef> pawnKinds)
        {
            // Warning: nested ternaries ahead
            int pawnCount = pawnKinds.Count();
            return ((MeMiMoSettings.betterHostileReadouts) ? pawnCount.ToString() + " " + ((pawnCount > 1) ? faction.pawnsPlural : faction.pawnSingular) : faction.pawnsPlural);
        }

        public static void DrawPawnGUIOverlay_Postfix(PawnUIOverlay __instance, Pawn ___pawn)
        {
            // First two checks are just to prevent duplicates
            if (MeMiMoSettings.betterHostileReadouts && !___pawn.RaceProps.Humanlike && ___pawn.Faction != Faction.OfPlayer && ___pawn.HostileTo(Faction.OfPlayer))
                GenMapUI.DrawPawnLabel(___pawn, GenMapUI.LabelDrawPosFor(___pawn, -0.6f), font: GameFont.Tiny);
        }
        #endregion

        #region IngredientFilterInfoCards
        public static IEnumerable<CodeInstruction> DoThingDef_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            int lWidthCalls = 0;
            bool done = false;

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.opcode == OpCodes.Callvirt && instruction.operand == AccessTools.Property(typeof(Listing_Tree), "LabelWidth").GetGetMethod(true))
                {
                    lWidthCalls++;
                }


                if (lWidthCalls != 3 && instruction.opcode == OpCodes.Callvirt && instruction.operand == AccessTools.Property(typeof(Listing_Tree), "LabelWidth").GetGetMethod(true))
                {
                    yield return instruction;
                    instruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(AdjustedWidth)));
                }

                // Wasn't able to get Widgets.Checkbox since AccessTools.Method returned null even with the correct overload params
                if (!done && instruction.opcode == OpCodes.Call && instructionList[(i + 3)].opcode == OpCodes.Beq)
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Property(typeof(Listing_Tree), "LabelWidth").GetGetMethod(true));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Listing_TreeThingFilter), "curY"));
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    instruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(DoInfoCardButton)));
                    done = true;
                }

                yield return instruction;
            }
        }

        private static float AdjustedWidth(float width) =>
            width + ((MeMiMoSettings.thingFilterInfoCards) ? widthOffset : 0f);


        private static void DoInfoCardButton(float x, float y, ThingDef def)
        {
            if (MeMiMoSettings.thingFilterInfoCards)
            {
                Widgets.InfoCardButton(x + xOffset, y + yOffset, def);
            }
        }

        //[TweakValue("AAAMehniMiscMods", -50f, 50f)]
        private static float widthOffset = -19f;

        //[TweakValue("AAAMehniMiscMods", -50f, 50f)]
        private static float xOffset = -21f;

        //[TweakValue("AAAMehniMiscMods", -50f, 50f)]
        private static float yOffset = -2.25f;
        #endregion

    }

    [DefOf]
    public static class DefOf_M4
    {
        public static SoundDef BulletImpact_Wood;
        public static SoundDef BulletImpact_Flesh;
        public static SoundDef BulletImpact_Metal;
    }
}
