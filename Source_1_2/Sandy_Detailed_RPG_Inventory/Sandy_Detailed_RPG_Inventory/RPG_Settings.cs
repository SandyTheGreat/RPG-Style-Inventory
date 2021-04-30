using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.Sound;

namespace Sandy_Detailed_RPG_Inventory
{
    class Sandy_RPG_Settings : ModSettings
    {
        public static float rpgTabHeight = 500f;
        public static float rpgTabWidth = 706f;
        public static bool displayAllSlots = false;
        public static bool displayTempOnTheSameLine = false;
        public static bool simplifiedView = false;
        public static Sandy_RPG_Settings instance { get { return LoadedModManager.GetMod<Sandy_Detailed_RPG_Inventory>().GetSettings<Sandy_RPG_Settings>(); }  }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref rpgTabHeight, "rpgTabHeight", 500f);
            Scribe_Values.Look(ref rpgTabWidth, "rpgTabWidth", 706f);
            Scribe_Values.Look(ref displayAllSlots, "displayAllSlots", false);
            Scribe_Values.Look(ref displayTempOnTheSameLine, "displayTempOnTheSameLine", false);
            Scribe_Values.Look(ref simplifiedView, "simplifiedView", false);
            base.ExposeData();
        }
    }

    class Sandy_Detailed_RPG_Inventory : Mod
    {
        Sandy_RPG_Settings settings;
        string tabHeight;
        string tabWidth;

        public Sandy_Detailed_RPG_Inventory(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<Sandy_RPG_Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            inRect.width /= 2;
            listingStandard.Begin(inRect);
            listingStandard.Label("RPG_Inventory_Width".Translate());
            listingStandard.TextFieldNumeric(ref Sandy_RPG_Settings.rpgTabWidth, ref tabWidth);
            string s;
            if (Sandy_Detailed_RPG_GearTab.minRecommendedWidth == Sandy_RPG_Settings.rpgTabWidth)
                s = "RPG_AutoFitWidth_Wide_Button_Label".Translate();
            else if (Sandy_Detailed_RPG_GearTab.maxRecommendedWidth == Sandy_RPG_Settings.rpgTabWidth)
                s = "RPG_AutoFitWidth_Tight_Button_Label".Translate();
            else
                s = "RPG_AutoFitWidth_Button_Label".Translate();
            if (listingStandard.ButtonText(s))
            {
                DoFit(Sandy_RPG_Settings.displayAllSlots);
            }
            listingStandard.Gap();
            listingStandard.Label("RPG_Inventory_Height".Translate());
            listingStandard.TextFieldNumeric(ref Sandy_RPG_Settings.rpgTabHeight, ref tabHeight);
            //
            if (Sandy_Utility.CustomCheckboxLabeled(listingStandard, "RPG_Display_All_Slots_Label".Translate(), ref Sandy_RPG_Settings.displayAllSlots, "RPG_Display_All_Slots_Note".Translate()))
            {
                DoFit(Sandy_RPG_Settings.displayAllSlots, true);
            }
            listingStandard.CheckboxLabeled("RPG_Dispaly_Temp_On_The_Same_Line_Label".Translate(), ref Sandy_RPG_Settings.displayTempOnTheSameLine, "RPG_Dispaly_Temp_On_The_Same_Line_Note".Translate());

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        protected void DoFit(bool displayAllSlots, bool reset = false)
        {
            Sandy_Detailed_RPG_GearTab.MakePreps(displayAllSlots, reset);
            float minWidth = Sandy_Detailed_RPG_GearTab.minRecommendedWidth;
            float maxWidth = Sandy_Detailed_RPG_GearTab.maxRecommendedWidth;

            if(!reset)
                if (Sandy_RPG_Settings.rpgTabWidth == Sandy_Detailed_RPG_GearTab.minRecommendedWidth) //ability to switch between recommended sizes
                {
                    tabWidth = maxWidth.ToString();
                    return;
                }
                else if (Sandy_RPG_Settings.rpgTabWidth == Sandy_Detailed_RPG_GearTab.maxRecommendedWidth)
                {
                    tabWidth = minWidth.ToString();
                    return;
                }

            if (Sandy_RPG_Settings.rpgTabWidth < minWidth) // setting minimum size (pawn model on the bottom)
                {
                    tabWidth = minWidth.ToString();
                }
                else if (Sandy_RPG_Settings.rpgTabWidth > minWidth) //stats on the side
                {
                    if (Sandy_RPG_Settings.rpgTabWidth < maxWidth)
                    {
                        tabWidth = minWidth.ToString();
                    }
                    else
                    {
                        tabWidth = maxWidth.ToString();
                    }
                }
        }

        public override string SettingsCategory()
        {
            return "RPG_Style_Inventory_Title".Translate();
        }
    }

    static class Sandy_Utility
    {
        public static bool CustomCheckboxLabeled(Listing listing, string label, ref bool checkOn, string tooltip = null)
        {
            bool result = false;
            float lineHeight = Text.LineHeight;
            bool val = checkOn;
            Func<bool, bool> check = delegate (bool pressed) { result = pressed; if (pressed) val = !val; return val; };
            Rect rect = listing.GetRect(lineHeight);
            CustomCheckboxLabeled(rect, label, check, tooltip);
            listing.Gap(listing.verticalSpacing);
            return result;
        }

        public static void CustomCheckboxLabeled(Rect rect, string label, Func<bool, bool> func, string tooltip = null)
        {
            if (!tooltip.NullOrEmpty())
            {
                if (Mouse.IsOver(rect)) Widgets.DrawHighlight(rect);
                TooltipHandler.TipRegion(rect, tooltip);
            }
            //
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;

            Widgets.Label(rect, label);
            bool val;
            if (Widgets.ButtonInvisible(rect, true))
            {
                val = func(true);
                if (val) SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera(null);
                else SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera(null);
            }
            else val = func(false);
            //
            Color color = GUI.color;
            Texture2D image;
            if (val) image = Widgets.CheckboxOnTex;
            else image = Widgets.CheckboxOffTex;
            GUI.DrawTexture(new Rect(rect.x + rect.width - 24f, rect.y, 24f, 24f), image);
            Text.Anchor = anchor;
        }
    }
}