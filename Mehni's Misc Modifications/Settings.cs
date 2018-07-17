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
        #region CatHunting
        public static bool catsCanHunt = true;
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
        public static bool randomRaidRetreat = false;
        public static float retreatAtPercentageDefeated = 0.5f;
        public static FloatRange retreatDefeatRange = new FloatRange(0.5f, 0.5f);
        #endregion

        #region DontLeaveJustYet
        public static bool allowLongerStays = false;
        public static int extraDaysUntilKickedOut = 3;
        #endregion

        #region ShowLovers
        public static bool showLoversOnAssignBed = true;
        #endregion

        #region RerollingPawns
        public static bool enableTutorialStyleRolling = true;
        #endregion

        #region AlertEmergency
        //TODO: Maybe implement as option.
        public static bool enableAlert = false;
        #endregion

        #region DeathMessages
        public static bool deathMessagesForAnimals = true;
        #endregion

        #region LessLitterLouting
        public static float humanFilthRate = 5f;
        #endregion

        #region NoMortarForcedSlowdown
        public static bool forcedSlowDownOnMortarFire = true;
        #endregion

        #region AnimalHandlingSanity
        public static bool obedientPredatorsDeferHuntingTameDesignatedAnimals = true;
        public static int animalInteractionHourLimit = 20;
        #endregion

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
            options.CheckboxLabeled("M4_SettingCatsCanHunt".Translate(), ref catsCanHunt, "M4_SettingCatsCanHuntToolTip".Translate());
            options.GapLine();

            options.CheckboxLabeled("M4_SettingEnableLargePacks".Translate(), ref enableLargePacks, "M4_SettingLargePackToolTip".Translate());
            options.GapLine();

            #region AutoUndrafter
            //options.CheckboxLabeled("M4_SettingModifyAutoUndrafter".Translate(), ref modifyAutoUndrafter, "M4_SettingModifyAutoUndrafterToolTip".Translate());
            //if (!modifyAutoUndrafter)
            //{
            //    GUI.color = Color.grey;
            //}
            //options.SliderLabeled("M4_SettingExtendUndraftTimeBy".Translate(), ref extendUndraftTimeBy, extendUndraftTimeBy.ToStringTicksToPeriod(), 0, 60000);
            //options.CheckboxLabeled("M4_SettingWithGunsBlazing".Translate(), ref whenGunsAreFiring, "M4_SettingGunsBlazingToolTip".Translate());
            //options.CheckboxLabeled("M4_SettingLowMoodUndraft".Translate(), ref allowAutoUndraftAtLowMood, "M4_SettingLowMoodUndraftDesc".Translate());
            //GUI.color = defaultColor;
            //if (!modifyAutoUndrafter || !allowAutoUndraftAtLowMood)
            //{
            //    GUI.color = Color.grey;
            //}
            //options.AddLabeledRadioList(string.Empty, mentalBreakRisks, ref dontExtendWhenMoodAt);
            //GUI.color = defaultColor;
            //options.GapLine();
            #endregion AutoUndrafter

            options.CheckboxLabeled("M4_SettingVariableRaidRetreat".Translate(), ref variableRaidRetreat, "M4_SettingVariableRaidToolTip".Translate());
            if (!variableRaidRetreat)
            {
                GUI.color = Color.grey;
            }
            options.Gap(2);
            options.FloatRange("M4_SettingRetreatAtPercentageDefeated".Translate(), ref retreatDefeatRange, 0f, 1f, "M4_SettingRandomRaidRetreatToolTip".Translate(new object[] 
            {
                retreatDefeatRange.min.ToStringByStyle(ToStringStyle.PercentZero),
                retreatDefeatRange.max.ToStringByStyle(ToStringStyle.PercentZero)
            }), ToStringStyle.PercentZero);
            options.GapLine();

            options.CheckboxLabeled("M4_SettingDontLeaveJustYet".Translate(), ref allowLongerStays, "M4_SettingDontLeaveJustYetToolTip".Translate());
            if (!allowLongerStays)
            {
                GUI.color = Color.grey;
            }
            options.SliderLabeled("M4_SettingDaysUntilKickedOut".Translate(), ref extraDaysUntilKickedOut, extraDaysUntilKickedOut.ToString(), 1, 5);
            GUI.color = defaultColor;
            options.GapLine();
            options.CheckboxLabeled("M4_TutorialStyleRolling".Translate(), ref enableTutorialStyleRolling, "M4_TutorialStyleRollingDesc".Translate());
            options.GapLine();
            options.CheckboxLabeled("M4_NotifyDeadAnimals".Translate(), ref deathMessagesForAnimals, "M4_NotifyDeadAnimalsDesc".Translate());
            options.GapLine();
            options.SliderLabeled("M4_LessLitterLouting".Translate(), ref humanFilthRate, Math.Round(humanFilthRate, 2).ToString(), 0, 25, "M4_LessLitterLoutingToolTip".Translate());
            options.GapLine();
            options.CheckboxLabeled("M4_NoForcedMortarSlowDown".Translate(), ref forcedSlowDownOnMortarFire, "M4_NoForcedMortarSlowDownDesc".Translate());
            options.GapLine();
            options.CheckboxLabeled("M4_ObedientPredatorsDontHuntTameDesignatedPawns".Translate(), ref obedientPredatorsDeferHuntingTameDesignatedAnimals, "M4_ObedientPredatorsDontHuntTameDesignatedPawnsDesc".Translate());
            options.SliderLabeled("M4_AnimalInteractionHourLimit".Translate(), ref animalInteractionHourLimit, animalInteractionHourLimit.ToString() + "h", 0, 24, "M4_AnimalInteractionHourLimit_Desc".Translate());
            options.Gap();

            options.End();

            Mod.GetSettings<MeMiMoSettings>().Write();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref catsCanHunt, "catsCanHunt", true);
            Scribe_Values.Look(ref modifyAutoUndrafter, "modifyAutoUndrafter", false);
            Scribe_Values.Look(ref whenGunsAreFiring, "whenGunsAreFiring", true);
            Scribe_Values.Look(ref extendUndraftTimeBy, "extendUndraftTimeBy", 5000);
            Scribe_Values.Look(ref allowAutoUndraftAtLowMood, "allowAutoUndraftAtLowMood", true);
            Scribe_Values.Look(ref dontExtendWhenMoodAt, "dontExtendWhenMoodAt", "  Major Break Risk");
            Scribe_Values.Look(ref enableLargePacks, "enableLargePacks", true);
            Scribe_Values.Look(ref variableRaidRetreat, "variableRaidRetreat", false); 
            Scribe_Values.Look(ref randomRaidRetreat, "randomRaidRetreat", false); //kept for backwards comp
            Scribe_Values.Look(ref retreatAtPercentageDefeated, "retreatAtPercentageDefeated", 0.5f); //kept for backwards comp
            Scribe_Values.Look(ref retreatDefeatRange, "retreatDefeatRange", new FloatRange(0.5f, 0.5f));
            Scribe_Values.Look(ref allowLongerStays, "allowLongerStays", false);
            Scribe_Values.Look(ref extraDaysUntilKickedOut, "daysUntilKickedOut", 3);
            Scribe_Values.Look(ref enableTutorialStyleRolling, "tutorialStyleRolling", true);
            Scribe_Values.Look(ref deathMessagesForAnimals, "deathMessageForAnimals", true);
            Scribe_Values.Look(ref humanFilthRate, "humanFilthRate", 5f);
            Scribe_Values.Look(ref forcedSlowDownOnMortarFire, "forcedSlowDownOnMortarFire", false);
            Scribe_Values.Look(ref obedientPredatorsDeferHuntingTameDesignatedAnimals, "obedientPredatorsDeferHuntingTameDesignatedAnimals", true);
            Scribe_Values.Look(ref animalInteractionHourLimit, "animalInteractionHourLimit", 20);
        }
    }

    public class MehniMiscMods : Mod
    {
        public MeMiMoSettings settings;

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
