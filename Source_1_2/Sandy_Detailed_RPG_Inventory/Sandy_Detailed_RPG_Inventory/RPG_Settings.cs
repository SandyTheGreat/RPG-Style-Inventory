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

        public override void ExposeData()
        {
            Scribe_Values.Look(ref rpgTabHeight, "rpgTabHeight", 500f);
            Scribe_Values.Look(ref rpgTabWidth, "rpgTabWidth", 706f);
            Scribe_Values.Look(ref displayAllSlots, "displayAllSlots", false);
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
            if(listingStandard.ButtonText("AutoFitWidth_Button_Label".Translate()))
            {
                Sandy_Detailed_RPG_GearTab.MakePreps(Sandy_RPG_Settings.displayAllSlots);
                float recomWidth = Sandy_Detailed_RPG_GearTab.slotPanelWidth + 128f + 20f + 20f + 10f;
                if (Sandy_RPG_Settings.rpgTabWidth < recomWidth)
                {
                    tabWidth = recomWidth.ToString();
                }

                if (Sandy_RPG_Settings.rpgTabWidth > recomWidth + 128f)
                {
                    tabWidth = (recomWidth + 128f).ToString(); //stats on the side
                }
                else if (Sandy_RPG_Settings.rpgTabWidth > recomWidth)
                {
                    tabWidth = recomWidth.ToString(); //pawn model on the bottom
                }
            }
            listingStandard.Gap();
            listingStandard.Label("RPG_Inventory_Height".Translate());
            listingStandard.TextFieldNumeric(ref Sandy_RPG_Settings.rpgTabHeight, ref tabHeight);
            //
            Action<bool> action = delegate (bool val)
            {
                Sandy_Detailed_RPG_GearTab.MakePreps(val, true);
                float recomWidth = Sandy_Detailed_RPG_GearTab.slotPanelWidth + 128f + 20f + 20f + 10f;
                if (Sandy_RPG_Settings.rpgTabWidth < recomWidth)
                {
                    tabWidth = recomWidth.ToString();
                }
            };
            CustomCheckboxLabeled(listingStandard, "RPG_Display_All_Slots_Label".Translate(), ref Sandy_RPG_Settings.displayAllSlots, "RPG_Display_All_Slots_Note".Translate(), action);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public static void CustomCheckboxLabeled(Listing listing, string label, ref bool checkOn, string tooltip = null, Action<bool> onPress = null)
        {
            float lineHeight = Text.LineHeight;
            Rect rect = listing.GetRect(lineHeight);
            if (!tooltip.NullOrEmpty())
            {
                if (Mouse.IsOver(rect))
                {
                    Widgets.DrawHighlight(rect);
                }
                TooltipHandler.TipRegion(rect, tooltip);
            }
            //Widgets.CheckboxLabeled(rect, label, ref checkOn, false, null, null, false);
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;

            Widgets.Label(rect, label);
            if (Widgets.ButtonInvisible(rect, true))
            {
                checkOn = !checkOn;
                if (checkOn)
                {
                    SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera(null);
                }
                else
                {
                    SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera(null);
                }
                onPress?.Invoke(checkOn);
            }
            //Widgets.CheckboxDraw(rect.x + rect.width - 24f, rect.y, checkOn, disabled, 24f, null, null);
            Color color = GUI.color;
            Texture2D image;
            if (checkOn)
            {
                image = Widgets.CheckboxOnTex;
            }
            else
            {
                image = Widgets.CheckboxOffTex;
            }
            GUI.DrawTexture(new Rect(rect.x + rect.width - 24f, rect.y, 24f, 24f), image);
            Text.Anchor = anchor;
            listing.Gap(listing.verticalSpacing);
        }

        public override string SettingsCategory()
        {
            return "RPG_Style_Inventory_Title".Translate();
        }
    }
}