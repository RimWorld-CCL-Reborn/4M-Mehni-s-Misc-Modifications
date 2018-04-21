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
        #endregion

        #region DontLeaveJustYet
        public static bool allowLongerStays = false;
        public static int extraDaysUntilKickedOut = 3;
        #endregion

        //#region RerollingPawns
        //public static bool enableTutorialStyleRolling = false;
        //#endregion

        public void DoWindowContents(Rect wrect)
        {
            Listing_Standard options = new Listing_Standard();
            Color defaultColor = GUI.color;
            options.Begin(wrect);

            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = Color.yellow;
            options.Label("M4.GreySettingsIgnored".Translate());
            GUI.color = defaultColor;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            options.Gap();
            options.CheckboxLabeled("M4.SettingCatsCanHunt".Translate(), ref catsCanHunt, "M4.SettingCatsCanHuntToolTip".Translate());
            options.GapLine();

            options.CheckboxLabeled("M4.SettingEnableLargePacks".Translate(), ref enableLargePacks, "M4.SettingLargePackToolTip".Translate());
            options.GapLine();

            #region AutoUndrafter
            options.CheckboxLabeled("M4.SettingModifyAutoUndrafter".Translate(), ref modifyAutoUndrafter, "M4.SettingModifyAutoUndrafterToolTip".Translate());
            if (!modifyAutoUndrafter)
            {
                GUI.color = Color.grey;
            }
            options.SliderLabeled("M4.SettingExtendUndraftTimeBy".Translate(), ref extendUndraftTimeBy, extendUndraftTimeBy.ToStringTicksToPeriod(), 0, 60000);
            options.CheckboxLabeled("M4.SettingWithGunsBlazing".Translate(), ref whenGunsAreFiring, "M4.SettingGunsBlazingToolTip".Translate());
            options.CheckboxLabeled("M4.SettingLowMoodUndraft".Translate(), ref allowAutoUndraftAtLowMood, "M4.SettingLowMoodUndraftDesc".Translate());
            GUI.color = defaultColor;
            if (!modifyAutoUndrafter || !allowAutoUndraftAtLowMood)
            {
                GUI.color = Color.grey;
            }
            options.AddLabeledRadioList(string.Empty, mentalBreakRisks, ref dontExtendWhenMoodAt);
            GUI.color = defaultColor;
            options.GapLine();
            #endregion AutoUndrafter

            options.CheckboxLabeled("M4.SettingVariableRaidRetreat".Translate(), ref variableRaidRetreat, "M4.SettingVariableRaidToolTip".Translate());
            if (!variableRaidRetreat)
            {
                GUI.color = Color.grey;
            }
            options.CheckboxLabeled("M4.SettingRandomRaidRetreat".Translate(), ref randomRaidRetreat, "M4.SettingRandomRaidRetreatToolTip".Translate());
            GUI.color = defaultColor;
            if (!variableRaidRetreat || randomRaidRetreat)
            {
                GUI.color = Color.grey;
            }
            options.SliderLabeled("M4.SettingRetreatAtPercentageDefeated".Translate(), ref retreatAtPercentageDefeated, retreatAtPercentageDefeated.ToStringPercent(), 0.1f, 1f);
            GUI.color = defaultColor;
            options.GapLine();

            options.CheckboxLabeled("M4.SettingDontLeaveJustYet".Translate(), ref allowLongerStays, "M4.SettingDontLeaveJustYetToolTip".Translate());
            if (!allowLongerStays)
            {
                GUI.color = Color.grey;
            }
            options.SliderLabeled("M4.SettingDaysUntilKickedOut".Translate(), ref extraDaysUntilKickedOut, extraDaysUntilKickedOut.ToString(), 1, 5);
            GUI.color = defaultColor;
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
            Scribe_Values.Look(ref randomRaidRetreat, "randomRaidRetreat", false);
            Scribe_Values.Look(ref retreatAtPercentageDefeated, "retreatAtPercentageDefeated", 0.5f);
            Scribe_Values.Look(ref allowLongerStays, "allowLongerStays", false);
            Scribe_Values.Look(ref extraDaysUntilKickedOut, "daysUntilKickedOut", 3);
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
