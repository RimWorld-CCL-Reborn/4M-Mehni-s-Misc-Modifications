using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Harmony;

namespace Mehni.Misc.Modifications
{
    public class MeMiMoSettings : ModSettings
    {
        #region BigAnimalMigrations
        public static bool bigAnimalMigrations = false;
        #endregion

        #region AutoUndrafter
        public static bool modifyAutoUndrafter = false;
        public static bool whenGunsAreFiring = true;
        public static int extendUndraftTimeBy = 5000;
        public static bool allowAutoUndraftAtLowMood = true;
        public static string dontExtendWhenMoodAt = "  Major Break Risk";
        public static string[] mentalBreakRisks = { "   Minor Break Risk", "   Major Break Risk", "   Extreme Break Risk" };
        #endregion

        #region BigManhunterPacks
        public static bool enableLargePacks = true;
        #endregion

        #region VariableRaidRetreat
        public static bool variableRaidRetreat = false;
        public static FloatRange retreatDefeatRange = new FloatRange(0.5f, 0.5f);
        #endregion

        #region DontLeaveJustYet
        public static bool allowLongerStays = false;
        public static int extraDaysUntilKickedOut = 3;
        #endregion

        #region RerollingPawns
        public static bool enableTutorialStyleRolling = true;
        #endregion

        #region DeathMessages
        public static bool deathMessagesForAnimals = true;
        #endregion

        #region LessLitterLouting
        public static float humanFilthRate = 5f;
        #endregion

        #region AnimalHandlingSanity
        public static bool obedientPredatorsDeferHuntingTameDesignatedAnimals = true;
        public static int animalInteractionHourLimit = 20;
        #endregion

        #region WorkAssignmentMatters
        public static bool workAssignmentMatters = false;
        #endregion

        #region ToolsForModders

        public static bool iAmAModder = false;
        #region DevModeSpawnTweaks
        public static bool chooseItemStuff = true;
        public static string stuffDefName = "";
        public static bool forceItemQuality = true;
        public static int forcedItemQuality = 2;
        #endregion
        #endregion ToolsForModders

        #region BetterHostileReadouts
        public static bool betterHostileReadouts = true;
        #endregion

        #region DisplayRangedWeaponDPS
        public static bool displayRangedDPS = true;
        #endregion

        #region ThingFilterInfoCards
        public static bool thingFilterInfoCards = true;
        #endregion

        //#region HideDisfigurement
        //public static bool apparelHidesDisfigurement = true;
        //#endregion

        //[TweakValue("AAAMehniMiscMods")]
        private static float yPos = 43f;

        // Value to modify when adding new settings
        //[TweakValue("AAAMehniMiscMods", max: 500f)]
        private static float moreOptionsRecty = 270f;

        //[TweakValue("AAAMehniMiscMods")]
        private static float widthFiddler = 9f;

        public void DoWindowContents(Rect wrect)
        {
            Listing_Standard options = new Listing_Standard();
            Color defaultColor = GUI.color;
            options.Begin(wrect);

            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = Color.yellow;
            options.Label("M4_GreySettingsIgnored".Translate());
            GUI.color = defaultColor;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            options.Gap();

            options.ColumnWidth = wrect.width / 2 - widthFiddler;

            // Left column

            options.CheckboxLabeled("M4_NotifyDeadAnimals".Translate(), ref deathMessagesForAnimals, "M4_NotifyDeadAnimalsDesc".Translate());
            options.GapLine();

            options.CheckboxLabeled("M4_TimetableAssignmentMatters".Translate(), ref workAssignmentMatters, "M4_TimetableAssignmentMatters_Desc".Translate());
            options.GapLine();

            options.CheckboxLabeled("M4_BetterHostileReadouts".Translate(), ref betterHostileReadouts, "M4_BetterHostileReadouts_Desc".Translate());
            options.GapLine();

            options.SliderLabeled("M4_LessLitterLouting".Translate(), ref humanFilthRate, Math.Round(humanFilthRate, 2).ToString(), 0, 25, "M4_LessLitterLoutingToolTip".Translate());
            options.GapLine();

            options.CheckboxLabeled("M4_ObedientPredatorsDontHuntTameDesignatedPawns".Translate(), ref obedientPredatorsDeferHuntingTameDesignatedAnimals, "M4_ObedientPredatorsDontHuntTameDesignatedPawnsDesc".Translate());
            options.SliderLabeled("M4_AnimalInteractionHourLimit".Translate(), ref animalInteractionHourLimit, animalInteractionHourLimit + "h", 0, 24, "M4_AnimalInteractionHourLimit_Desc".Translate());    

            // Right column

            options.NewColumn();
            options.Gap(yPos);

            options.CheckboxLabeled("M4_SettingBigAnimalMigrations".Translate(), ref bigAnimalMigrations, "M4_SettingBigAnimalMigrationsToolTip".Translate());
            options.GapLine();

            options.CheckboxLabeled("M4_SettingEnableLargePacks".Translate(), ref enableLargePacks, "M4_SettingLargePackToolTip".Translate());
            options.GapLine();

            options.CheckboxLabeled("M4_TutorialStyleRolling".Translate(), ref enableTutorialStyleRolling, "M4_TutorialStyleRollingDesc".Translate());
            options.GapLine();

            options.CheckboxLabeled("M4_ThingFilterInfoCards".Translate(), ref thingFilterInfoCards, "M4_ThingFilterInfoCards_Desc".Translate());
            options.GapLine();

            options.CheckboxLabeled("M4_DisplayRangedDPS".Translate(), ref displayRangedDPS, "M4_DisplayRangedDPS_Desc".Translate());

            options.Gap();
            options.End();

            // More options

            Listing_Standard moreOptions = new Listing_Standard();
            Rect moreOptionsRect = wrect;
            moreOptionsRect.y = moreOptionsRecty;
            moreOptions.Begin(moreOptionsRect);
            moreOptions.GapLine();

            if (!modifyAutoUndrafter)
            {
                GUI.color = Color.grey;
            }
            moreOptions.CheckboxLabeled("M4_SettingModifyAutoUndrafter".Translate(), ref modifyAutoUndrafter, "M4_SettingModifyAutoUndrafterToolTip".Translate());
            if (modifyAutoUndrafter)
            {
                moreOptions.SliderLabeled("M4_SettingExtendUndraftTimeBy".Translate(), ref extendUndraftTimeBy, extendUndraftTimeBy.ToStringTicksToPeriod(), 0, 60000);
                moreOptions.CheckboxLabeled("M4_SettingWithGunsBlazing".Translate(), ref whenGunsAreFiring,         "M4_SettingGunsBlazingToolTip".Translate());
                moreOptions.CheckboxLabeled("M4_SettingLowMoodUndraft".Translate(),  ref allowAutoUndraftAtLowMood, "M4_SettingLowMoodUndraftDesc".Translate());
                GUI.color = defaultColor;
                if (!modifyAutoUndrafter || !allowAutoUndraftAtLowMood)
                {
                    GUI.color = Color.grey;
                }
                moreOptions.AddLabeledRadioList(string.Empty, mentalBreakRisks, ref dontExtendWhenMoodAt);
            }

            GUI.color = defaultColor;
            moreOptions.GapLine();

            moreOptions.CheckboxLabeled("M4_SettingVariableRaidRetreat".Translate(), ref variableRaidRetreat, "M4_SettingVariableRaidToolTip".Translate());
            if (!variableRaidRetreat)
            {
                GUI.color = Color.grey;
            }
            moreOptions.Gap(2);
            moreOptions.FloatRange("M4_SettingRetreatAtPercentageDefeated".Translate(), ref retreatDefeatRange, 0f, 1f, "M4_SettingRandomRaidRetreatToolTip".Translate(
                retreatDefeatRange.min.ToStringByStyle(ToStringStyle.PercentZero),
                retreatDefeatRange.max.ToStringByStyle(ToStringStyle.PercentZero)
            ), ToStringStyle.PercentZero);
            moreOptions.GapLine();

            moreOptions.CheckboxLabeled("M4_SettingDontLeaveJustYet".Translate(), ref allowLongerStays, "M4_SettingDontLeaveJustYetToolTip".Translate());
            if (!allowLongerStays)
            {
                GUI.color = Color.grey;
            }
            moreOptions.SliderLabeled("M4_SettingDaysUntilKickedOut".Translate(), ref extraDaysUntilKickedOut, extraDaysUntilKickedOut.ToString(), 1, 5);
            GUI.color = defaultColor;
            moreOptions.GapLine();

            moreOptions.CheckboxLabeled("I am a modders FFS", ref iAmAModder, "Removes the 6 second cooldown on workshop submissions, unlocks special options.");
            if (iAmAModder)
            {
                moreOptions.Label("Dev mode spawn tweaks [Unsaved. Setting wiped on restart.]");
                moreOptions.CheckboxLabeled("Choose item stuff", ref chooseItemStuff);
                moreOptions.Gap();
                moreOptions.AddLabeledTextField("Stuff defName (blank = default stuff)", ref stuffDefName);
                moreOptions.Gap();
                moreOptions.CheckboxLabeled("Force item quality", ref forceItemQuality);
                moreOptions.Gap();
                moreOptions.SliderLabeled("Item quality", ref forcedItemQuality, ((QualityCategory)forcedItemQuality).ToString(), 0, 6);
                moreOptions.GapLine();
            }
            moreOptions.End();

            Mod.GetSettings<MeMiMoSettings>().Write();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref bigAnimalMigrations, "bigAnimalMigrations", false);
            Scribe_Values.Look(ref modifyAutoUndrafter, "modifyAutoUndrafter", false);
            Scribe_Values.Look(ref whenGunsAreFiring, "whenGunsAreFiring", true);
            Scribe_Values.Look(ref extendUndraftTimeBy, "extendUndraftTimeBy", 5000);
            Scribe_Values.Look(ref allowAutoUndraftAtLowMood, "allowAutoUndraftAtLowMood", true);
            Scribe_Values.Look(ref dontExtendWhenMoodAt, "dontExtendWhenMoodAt", "  Major Break Risk");
            Scribe_Values.Look(ref enableLargePacks, "enableLargePacks", true);
            Scribe_Values.Look(ref variableRaidRetreat, "variableRaidRetreat", false);
            Scribe_Values.Look(ref retreatDefeatRange, "retreatDefeatRange", new FloatRange(0.5f, 0.5f));
            Scribe_Values.Look(ref allowLongerStays, "allowLongerStays", false);
            Scribe_Values.Look(ref extraDaysUntilKickedOut, "daysUntilKickedOut", 3);
            Scribe_Values.Look(ref enableTutorialStyleRolling, "tutorialStyleRolling", true);
            Scribe_Values.Look(ref deathMessagesForAnimals, "deathMessageForAnimals", true);
            Scribe_Values.Look(ref humanFilthRate, "humanFilthRate", 5f);
            Scribe_Values.Look(ref obedientPredatorsDeferHuntingTameDesignatedAnimals, "obedientPredatorsDeferHuntingTameDesignatedAnimals", true);
            Scribe_Values.Look(ref animalInteractionHourLimit, "animalInteractionHourLimit", 20);
            Scribe_Values.Look(ref workAssignmentMatters, "workAssignmentMatters", false);
            Scribe_Values.Look(ref iAmAModder, "iAmAModder", false);
            Scribe_Values.Look(ref betterHostileReadouts, "betterHostileReadouts", true);
            Scribe_Values.Look(ref displayRangedDPS, "displayRangedDPS", true);
            Scribe_Values.Look(ref thingFilterInfoCards, "thingFilterInfoCards", true);
        }
    }

    public class MehniMiscMods : Mod
    {

        public MehniMiscMods(ModContentPack content) : base(content)
        {
            GetSettings<MeMiMoSettings>();
        }

        public override string SettingsCategory() => "Mehni's Miscellaneous Modifications";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            GetSettings<MeMiMoSettings>().DoWindowContents(inRect);
        }
    }
}
