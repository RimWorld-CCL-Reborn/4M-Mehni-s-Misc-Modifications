using System;
using RimWorld;
using Verse;
using UnityEngine;

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
        public static bool guardingPredatorsDeferHuntingTameDesignatedAnimals = true;
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
        private static readonly float yPos = 43f;

        // Value to modify when adding new settings, pushing the scrollview down.
        //[TweakValue("AAAMehniMiscMods", max: 500f)]
        private static readonly float moreOptionsRecty = 250f;

        //[TweakValue("AAAMehniMiscMods")]
        private static readonly float widthFiddler = 9f;

        //value to modify when adding more settings to the scrollview.
        //[TweakValue("AAAMehniMiscMods", 0, 1200f)]
        private static readonly float viewHeight = 600f;

        //Value where the rect stops.
        //[TweakValue("AAAMehniMiscMods", 0, 1200f)]
        private static readonly float yMax = 620;

        //Do not touch.
        //[TweakValue("AAAMehniMiscMods", 0, 1200f)]
        private static readonly float height = 640;

        private static Vector2 scrollVector2;

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

            options.CheckboxLabeled("M4_NotifyDeadAnimals".Translate(), ref deathMessagesForAnimals, "M4_NotifyDeadAnimals_Desc".Translate());
            options.GapLine();

            options.CheckboxLabeled("M4_TimetableAssignmentMatters".Translate(), ref workAssignmentMatters, "M4_TimetableAssignmentMatters_Desc".Translate());
            options.GapLine();

            options.CheckboxLabeled("M4_BetterHostileReadouts".Translate(), ref betterHostileReadouts, "M4_BetterHostileReadouts_Desc".Translate());
            options.GapLine();

            options.SliderLabeled("M4_LessLitterLouting".Translate(), ref humanFilthRate, Math.Round(humanFilthRate, 2).ToString(), 0, 25, "M4_LessLitterLouting_Desc".Translate());
            options.GapLine();

            options.SliderLabeled(
                label: "M4_AnimalInteractionHourLimit".Translate(),
                val: ref animalInteractionHourLimit,
                format: animalInteractionHourLimit + "h",
                min: 0f, max: 24f,
                tooltip: "M4_AnimalInteractionHourLimit_Desc".Translate()
                );    

            // Right column

            options.NewColumn();
            options.Gap(yPos);

            options.CheckboxLabeled("M4_SettingBigAnimalMigrations".Translate(), ref bigAnimalMigrations, "M4_SettingBigAnimalMigrations_Desc".Translate());
            options.GapLine();

            options.CheckboxLabeled("M4_SettingEnableLargePacks".Translate(), ref enableLargePacks, "M4_SettingLargePack_Desc".Translate());
            options.GapLine();

            options.CheckboxLabeled("M4_TutorialStyleRolling".Translate(), ref enableTutorialStyleRolling, "M4_TutorialStyleRolling_Desc".Translate());
            options.GapLine();

            options.CheckboxLabeled("M4_ThingFilterInfoCards".Translate(), ref thingFilterInfoCards, "M4_ThingFilterInfoCards_Desc".Translate());
            options.GapLine();

            options.CheckboxLabeled("M4_DisplayRangedDPS".Translate(), ref displayRangedDPS, "M4_DisplayRangedDPS_Desc".Translate());

            options.Gap();
            options.End();

            // More options

            Listing_Standard gapline = new Listing_Standard();
            Rect gapliRect = new Rect(wrect.x, wrect.y + moreOptionsRecty -35f, wrect.width, wrect.height);
            gapline.Begin(gapliRect);
            gapline.GapLine();
            gapline.End();

            Listing_Standard moreOptions = new Listing_Standard();
            Rect moreOptionsRect = wrect;
            moreOptionsRect.y = (moreOptionsRecty + 20f) / 2;
            moreOptionsRect.height = height / 2;
            moreOptionsRect.yMax = yMax;

            Rect viewRect = new Rect(0,0,wrect.width -18f, viewHeight);
            viewRect.width -= 18f;

            moreOptions.Begin(moreOptionsRect);

            moreOptions.BeginScrollView(moreOptionsRect, ref scrollVector2, ref viewRect);

            moreOptions.CheckboxLabeled("M4_GuardingPredatorsDontHuntTameDesignatedPawns".Translate(),
                ref guardingPredatorsDeferHuntingTameDesignatedAnimals, "M4_GuardingPredatorsDontHuntTameDesignatedPawns_Desc".Translate());
            moreOptions.GapLine();

            if (!modifyAutoUndrafter)
            {
                GUI.color = Color.grey;
            }
            moreOptions.CheckboxLabeled("M4_SettingModifyAutoUndrafter".Translate(), ref modifyAutoUndrafter, "M4_SettingModifyAutoUndrafter_Desc".Translate());
            if (modifyAutoUndrafter)
            {
                moreOptions.SliderLabeled(
                    "M4_SettingExtendUndraftTimeBy".Translate(), ref extendUndraftTimeBy, extendUndraftTimeBy.ToStringTicksToPeriod(),
                    0, GenDate.TicksPerDay, "M4_SettingExtendUndraftTimeBy_Desc".Translate()
                    );
                moreOptions.CheckboxLabeled("M4_SettingWithGunsBlazing".Translate(), ref whenGunsAreFiring,         "M4_SettingGunsBlazing_Desc".Translate());
                moreOptions.CheckboxLabeled("M4_SettingLowMoodUndraft".Translate(),  ref allowAutoUndraftAtLowMood, "M4_SettingLowMoodUndraft_Desc".Translate());
                GUI.color = defaultColor;
                if (!modifyAutoUndrafter || !allowAutoUndraftAtLowMood)
                {
                    GUI.color = Color.grey;
                }
                moreOptions.AddLabeledRadioList(string.Empty, mentalBreakRisks, ref dontExtendWhenMoodAt);
            }

            GUI.color = defaultColor;
            moreOptions.GapLine();

            moreOptions.CheckboxLabeled("M4_SettingVariableRaidRetreat".Translate(), ref variableRaidRetreat, "M4_SettingVariableRaid_Desc".Translate());
            if (!variableRaidRetreat)
            {
                GUI.color = Color.grey;
            }
            moreOptions.Gap(2);
            moreOptions.FloatRange("M4_SettingRetreatAtPercentageDefeated".Translate(), ref retreatDefeatRange, 0f, 1f, "M4_SettingRandomRaidRetreat_Desc".Translate(
                retreatDefeatRange.min.ToStringByStyle(ToStringStyle.PercentZero),
                retreatDefeatRange.max.ToStringByStyle(ToStringStyle.PercentZero)
            ), ToStringStyle.PercentZero);
            moreOptions.GapLine();

            moreOptions.CheckboxLabeled("I am a modder", ref iAmAModder, "Removes the 6 second cooldown on workshop submissions, unlocks special options.");
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
            moreOptions.EndScrollView(ref viewRect);
            moreOptions.End();
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
            Scribe_Values.Look(ref enableTutorialStyleRolling, "tutorialStyleRolling", true);
            Scribe_Values.Look(ref deathMessagesForAnimals, "deathMessageForAnimals", true);
            Scribe_Values.Look(ref humanFilthRate, "humanFilthRate", 5f);
            Scribe_Values.Look(ref guardingPredatorsDeferHuntingTameDesignatedAnimals, "guardingPredatorsDeferHuntingTameDesignatedAnimals", true);
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
