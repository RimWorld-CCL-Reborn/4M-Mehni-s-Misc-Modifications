﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Mehni.Misc.Modifications
{

    //thanks to AlexTD for the below
    internal static class SettingsHelper
    {
        //private static float gap = 12f;

        public static void SliderLabeled(this Listing_Standard ls, string label, ref int val, string format, float min = 0f, float max = 100f, string tooltip = null)
        {
            float fVal = val;
            ls.SliderLabeled(label, ref fVal, format, min, max, tooltip);
            val = (int)fVal;
        }
        public static void SliderLabeled(this Listing_Standard ls, string label, ref float val, string format, float min = 0f, float max = 1f, string tooltip = null)
        {
            Rect rect = ls.GetRect(Text.LineHeight);
            Rect rect2 = rect.LeftPart(.70f).Rounded();
            Rect rect3 = rect.RightPart(.30f).Rounded().LeftPart(.67f).Rounded();
            Rect rect4 = rect.RightPart(.10f).Rounded();

            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect2, label);

            float result = Widgets.HorizontalSlider(rect3, val, min, max, true);
            val = result;
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(rect4, String.Format(format, val));
            if (!tooltip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }

            Text.Anchor = anchor;
            ls.Gap(ls.verticalSpacing);
        }

        public static void FloatRange(this Listing_Standard ls, string label, ref FloatRange range, float min = 0f, float max = 1f, string tooltip = null, ToStringStyle valueStyle = ToStringStyle.FloatTwo)
        {
            Rect rect = ls.GetRect(Text.LineHeight);
            Rect rect2 = rect.LeftPart(.70f).Rounded();
            Rect rect3 = rect.RightPart(.30f).Rounded().LeftPart(.9f).Rounded();
            rect3.yMin -= 5f;
            //Rect rect4 = rect.RightPart(.10f).Rounded();

            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect2, label);

            Text.Anchor = TextAnchor.MiddleRight;
            int id = ls.CurHeight.GetHashCode();
            Widgets.FloatRange(rect3, id, ref range, min, max, null, valueStyle);
            if (!tooltip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }
            Text.Anchor = anchor;
            ls.Gap(ls.verticalSpacing);
        }


        public static Rect GetRect(this Listing_Standard listing_Standard, float? height = null)
        {
            return listing_Standard.GetRect(height ?? Text.LineHeight);
        }

        //thanks to Why_is_that for the below, who in turn got his stuff from
        // REFERENCE: https://github.com/erdelf/GodsOfRimworld/blob/master/Source/Ankh/ModControl.cs
        // REFERENCE: https://github.com/erdelf/PrisonerRansom/
        public static void AddLabeledRadioList(this Listing_Standard listing_Standard, string header, string[] labels, ref string val, float? headerHeight = null)
        {
            //listing_Standard.Gap();
            if (header != string.Empty) { Widgets.Label(listing_Standard.GetRect(headerHeight), header); }
            listing_Standard.AddRadioList<string>(GenerateLabeledRadioValues(labels), ref val);
        }

        //public static void AddLabeledRadioList<T>(this Listing_Standard listing_Standard, string header, Dictionary<string, T> dict, ref T val, float? headerHeight = null)
        //{
        //    listing_Standard.Gap();
        //    if (header != string.Empty) { Widgets.Label(listing_Standard.GetRect(headerHeight), header); }
        //    listing_Standard.AddRadioList<T>(GenerateLabeledRadioValues<T>(dict), ref val);
        //}

        private static void AddRadioList<T>(this Listing_Standard listing_Standard, List<LabeledRadioValue<T>> items, ref T val, float? height = null)
        {
            foreach (LabeledRadioValue<T> item in items)
            {
                //listing_Standard.Gap();
                Rect lineRect = listing_Standard.GetRect(height);
                if (Widgets.RadioButtonLabeled(lineRect, item.Label, EqualityComparer<T>.Default.Equals(item.Value, val)))
                    val = item.Value;
            }
        }

        private static List<LabeledRadioValue<string>> GenerateLabeledRadioValues(string[] labels)
        {
            List<LabeledRadioValue<string>> list = new List<LabeledRadioValue<string>>();
            foreach (string label in labels)
            {
                list.Add(new LabeledRadioValue<string>(label, label));
            }
            return list;
        }

        public static void AddLabeledTextField(this Listing_Standard listing_Standard, string label, ref string settingsValue, float leftPartPct = 0.5f)
        {
            //listing_Standard.Gap(Gap);
            listing_Standard.LineRectSpilter(out Rect leftHalf, out Rect rightHalf, leftPartPct);

            // TODO: tooltips
            //Widgets.DrawHighlightIfMouseover(lineRect);
            //TooltipHandler.TipRegion(lineRect, "TODO: TIP GOES HERE");

            Widgets.Label(leftHalf, label);

            string buffer = settingsValue.ToString();
            settingsValue = Widgets.TextField(rightHalf, buffer);
        }

        public static void AddLabeledNumericalTextField<T>(this Listing_Standard listing_Standard, string label, ref T settingsValue, float leftPartPct = 0.5f, float minValue = 1f, float maxValue = 100000f) where T : struct
        {
            //listing_Standard.Gap(Gap);
            listing_Standard.LineRectSpilter(out Rect leftHalf, out Rect rightHalf, leftPartPct);

            // TODO: tooltips
            //Widgets.DrawHighlightIfMouseover(lineRect);
            //TooltipHandler.TipRegion(lineRect, "TODO: TIP GOES HERE");

            Widgets.Label(leftHalf, label);

            string buffer = settingsValue.ToString();
            Widgets.TextFieldNumeric<T>(rightHalf, ref settingsValue, ref buffer, minValue, maxValue);
        }

        public static Rect LineRectSpilter(this Listing_Standard listing_Standard, out Rect leftHalf, float leftPartPct = 0.5f, float? height = null)
        {
            Rect lineRect = listing_Standard.GetRect(height);
            leftHalf = lineRect.LeftPart(leftPartPct).Rounded();
            return lineRect;
        }

        public static Rect LineRectSpilter(this Listing_Standard listing_Standard, out Rect leftHalf, out Rect rightHalf, float leftPartPct = 0.5f, float? height = null)
        {
            Rect lineRect = listing_Standard.LineRectSpilter(out leftHalf, leftPartPct, height);
            rightHalf = lineRect.RightPart(1f - leftPartPct).Rounded();
            return lineRect;
        }

        //// (label, value) => (key, value)
        //private static List<LabeledRadioValue<T>> GenerateLabeledRadioValues<T>(Dictionary<string, T> dict)
        //{
        //    List<LabeledRadioValue<T>> list = new List<LabeledRadioValue<T>>();
        //    foreach (KeyValuePair<string, T> entry in dict)
        //    {
        //        list.Add(new LabeledRadioValue<T>(entry.Key, entry.Value));
        //    }
        //    return list;
        //}

        public class LabeledRadioValue<T>
        {
            public LabeledRadioValue(string label, T val)
            {
                Label = label;
                Value = val;
            }

            public string Label { get; set; }

            public T Value { get; set; }

        }
    }
}