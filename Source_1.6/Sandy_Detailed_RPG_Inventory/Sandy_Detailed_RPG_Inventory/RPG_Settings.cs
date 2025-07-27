using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace Sandy_Detailed_RPG_Inventory
{
    class Sandy_RPG_Settings : ModSettings
    {
        public static float rpgTabHeight = 600f;

        public static float rpgTabWidth = 720f;

        public static string Legendary = "Structure_Orange";

        public static string Masterwork = "Structure_Teal";

        public static string Excellent = "Structure_Magenta";

        public static string Good = "Structure_GreenPastel";

        public static string Normal = "Structure_Blue";

        public static string Poor = "Structure_LimePale";

        public static string Awful = "Structure_Black";

        public static IEnumerable<ColorDef> Colors = Enumerable.OrderBy<ColorDef, int>(Enumerable.Where<ColorDef>(DefDatabase<ColorDef>.AllDefs,
                                                                (ColorDef x) => x.colorType == ColorType.Structure), (ColorDef x) => x.displayOrder);

        public override void ExposeData()
        {
            Scribe_Values.Look(ref rpgTabHeight, "rpgTabHeight", 600f);
            Scribe_Values.Look(ref rpgTabWidth, "rpgTabWidth", 720f);

            Scribe_Values.Look(ref Legendary, "Legendary", "Structure_Orange");
            Scribe_Values.Look(ref Masterwork, "Masterwork", "Structure_Teal");
            Scribe_Values.Look(ref Excellent, "Excellent", "Structure_Magenta");
            Scribe_Values.Look(ref Good, "Good", "Structure_GreenPastel");
            Scribe_Values.Look(ref Normal, "Normal", "Structure_Blue");
            Scribe_Values.Look(ref Poor, "Poor", "Structure_LimePale");
            Scribe_Values.Look(ref Awful, "Awful", "Structure_Black");
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
            listingStandard.Begin(inRect);
            listingStandard.Label("RPG_Inventory_Width".Translate());
            listingStandard.TextFieldNumeric(ref Sandy_RPG_Settings.rpgTabWidth, ref tabWidth);

            listingStandard.Label("RPG_Inventory_Height".Translate());
            listingStandard.TextFieldNumeric(ref Sandy_RPG_Settings.rpgTabHeight, ref tabHeight);
            listingStandard.Gap();

            listingStandard.Label("RPG_Inventory_Color_Setting".Translate());

            if (listingStandard.ButtonTextLabeled("RPG_Inventory_Legendary".Translate(), "RPG_Inventory_Color_Selector".Translate(), TextAnchor.MiddleCenter))
            {
                List<FloatMenuOption> optionList = new List<FloatMenuOption>();
                foreach (ColorDef colorDef in Sandy_RPG_Settings.Colors)
                {
                    FloatMenuOption option = new FloatMenuOption(colorDef.LabelCap, delegate ()
                    {
                        //yourColorStringField = colorDef.defName;
                        Sandy_RPG_Settings.Legendary = colorDef.defName;
                    });
                    //add option to list
                    optionList.Add(option);
                }
                Find.WindowStack.Add(new FloatMenu(optionList));
            }
            listingStandard.TextEntry(Sandy_RPG_Settings.Legendary);

            if (listingStandard.ButtonTextLabeled("RPG_Inventory_Masterwork".Translate(), "RPG_Inventory_Color_Selector".Translate(), TextAnchor.MiddleCenter))
            {
                List<FloatMenuOption> optionList = new List<FloatMenuOption>();
                foreach (ColorDef colorDef in Sandy_RPG_Settings.Colors)
                {
                    FloatMenuOption option = new FloatMenuOption(colorDef.LabelCap, delegate ()
                    {
                        Sandy_RPG_Settings.Masterwork = colorDef.defName;
                    });
                    optionList.Add(option);
                }
                Find.WindowStack.Add(new FloatMenu(optionList));
            }
            listingStandard.TextEntry(Sandy_RPG_Settings.Masterwork);

            if (listingStandard.ButtonTextLabeled("RPG_Inventory_Excellent".Translate(), "RPG_Inventory_Color_Selector".Translate(), TextAnchor.MiddleCenter))
            {
                List<FloatMenuOption> optionList = new List<FloatMenuOption>();
                foreach (ColorDef colorDef in Sandy_RPG_Settings.Colors)
                {
                    FloatMenuOption option = new FloatMenuOption(colorDef.LabelCap, delegate ()
                    {
                        Sandy_RPG_Settings.Excellent = colorDef.defName;
                    });
                    optionList.Add(option);
                }
                Find.WindowStack.Add(new FloatMenu(optionList));
            }
            listingStandard.TextEntry(Sandy_RPG_Settings.Excellent);

            if (listingStandard.ButtonTextLabeled("RPG_Inventory_Good".Translate(), "RPG_Inventory_Color_Selector".Translate(), TextAnchor.MiddleCenter))
            {
                List<FloatMenuOption> optionList = new List<FloatMenuOption>();
                foreach (ColorDef colorDef in Sandy_RPG_Settings.Colors)
                {
                    FloatMenuOption option = new FloatMenuOption(colorDef.LabelCap, delegate ()
                    {
                        Sandy_RPG_Settings.Good = colorDef.defName;
                    });
                    optionList.Add(option);
                }
                Find.WindowStack.Add(new FloatMenu(optionList));
            }
            listingStandard.TextEntry(Sandy_RPG_Settings.Good);

            if (listingStandard.ButtonTextLabeled("RPG_Inventory_Normal".Translate(), "RPG_Inventory_Color_Selector".Translate(), TextAnchor.MiddleCenter))
            {
                List<FloatMenuOption> optionList = new List<FloatMenuOption>();
                foreach (ColorDef colorDef in Sandy_RPG_Settings.Colors)
                {
                    FloatMenuOption option = new FloatMenuOption(colorDef.LabelCap, delegate ()
                    {
                        Sandy_RPG_Settings.Normal = colorDef.defName;
                    });
                    optionList.Add(option);
                }
                Find.WindowStack.Add(new FloatMenu(optionList));
            }
            listingStandard.TextEntry(Sandy_RPG_Settings.Normal);

            if (listingStandard.ButtonTextLabeled("RPG_Inventory_Poor".Translate(), "RPG_Inventory_Color_Selector".Translate(), TextAnchor.MiddleCenter))
            {
                List<FloatMenuOption> optionList = new List<FloatMenuOption>();
                foreach (ColorDef colorDef in Sandy_RPG_Settings.Colors)
                {
                    FloatMenuOption option = new FloatMenuOption(colorDef.LabelCap, delegate ()
                    {
                        Sandy_RPG_Settings.Poor = colorDef.defName;
                    });
                    optionList.Add(option);
                }
                Find.WindowStack.Add(new FloatMenu(optionList));
            }
            listingStandard.TextEntry(Sandy_RPG_Settings.Poor);

            if (listingStandard.ButtonTextLabeled("RPG_Inventory_Awful".Translate(), "RPG_Inventory_Color_Selector".Translate(), TextAnchor.MiddleCenter))
            {
                List<FloatMenuOption> optionList = new List<FloatMenuOption>();
                foreach (ColorDef colorDef in Sandy_RPG_Settings.Colors)
                {
                    FloatMenuOption option = new FloatMenuOption(colorDef.LabelCap, delegate ()
                    {
                        Sandy_RPG_Settings.Awful = colorDef.defName;
                    });
                    optionList.Add(option);
                }
                Find.WindowStack.Add(new FloatMenu(optionList));
            }
            listingStandard.TextEntry(Sandy_RPG_Settings.Awful);

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "RPG_Style_Inventory_Title".Translate();
        }
    }
}