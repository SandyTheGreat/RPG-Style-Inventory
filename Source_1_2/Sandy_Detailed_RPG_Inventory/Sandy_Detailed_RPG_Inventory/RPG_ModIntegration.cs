using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using TorannMagic;
using UnityEngine;

namespace Sandy_Detailed_RPG_Inventory.MODIntegrations
{
    //for mods that are incapable of independent patching in
    //so basically for all the non-harmony mods
    public static class MODIntegration
    {
        public static void DrawThingRow1(Sandy_Detailed_RPG_GearTab tab, Rect rect, Thing thing, bool equipment)
        {
            RWoMIntegration.DrawThingRow1(tab, rect, thing, equipment);
        }

        public static void DrawStats1(Sandy_Detailed_RPG_GearTab tab, ref float top, float left, bool showArmor)
        {
            RWoMIntegration.DrawStats1(tab, ref top, left, showArmor);
        }

        public static void DrawStats(Sandy_Detailed_RPG_GearTab tab, ref float top, Rect rect, bool showArmor)
        {
            RWoMIntegration.DrawStats(tab, ref top, rect, showArmor);
        }
    }

    static class RWoMIntegration
    {
        static bool initialized = false;
        static bool RWoMIsActive = false;

        static bool Active()
        {
            if (initialized)
            {
                return RWoMIsActive;
            }
            initialized = true;
            List<ModMetaData> mmdList = ModsConfig.ActiveModsInLoadOrder.ToList();
            for (int i = 0; i < mmdList.Count; i++)
            {
                if (mmdList[i].PackageId == "torann.arimworldofmagic")
                {
                    RWoMIsActive = true;
                }
            }
            return RWoMIsActive;
        }

        static StatDef GetHarmonyStatDef()
        {
            return TorannMagicDefOf.ArmorRating_Alignment;
        }

        static string GetEnchantmentString(Thing apparel2)
        {
            return TorannMagic.TM_Calc.GetEnchantmentsString(apparel2);
        }

        public static void DrawThingRow1(Sandy_Detailed_RPG_GearTab tab, Rect rect, Thing thing, bool equipment)
        {
            if (!Active() || !ShouldDrawEnchantmentIcon(thing))
                return;
            //
            Rect rectM = new Rect(rect.x, rect.yMax - 40f, 20f, 20f);
            GUI.DrawTexture(rectM, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Enchanted_Icon", true));
            TooltipHandler.TipRegion(rectM, GetEnchantmentString(thing));
        }

        static bool ShouldDrawEnchantmentIcon(Thing item)
        {
            bool isEnchanted = false;
            TorannMagic.Enchantment.CompEnchantedItem enchantedItem = item.TryGetComp<TorannMagic.Enchantment.CompEnchantedItem>();
            if (enchantedItem != null && enchantedItem.HasEnchantment)
            {
                isEnchanted = true;
            }
            return isEnchanted;
        }

        public static void DrawStats1(Sandy_Detailed_RPG_GearTab tab, ref float top, float left, bool showArmor)
        {
            if (!Active())
                return;

            if (showArmor)
                tab.TryDrawOverallArmor1(ref top, left, Sandy_Detailed_RPG_GearTab.statPanelWidth, GetHarmonyStatDef(), 
                    "RPG_Style_Inventory_ArmorHarmony".Translate(), ContentFinder<Texture2D>.Get("UI/Icons/Sandy_ArmorHarmony_Icon", true));
        }

        public static void DrawStats(Sandy_Detailed_RPG_GearTab tab, ref float top, Rect rect, bool showArmor)
        {
            if (!Active())
                return;

            if (showArmor)
                tab.TryDrawOverallArmor(ref top, rect.width, GetHarmonyStatDef(), "RPG_Style_Inventory_ArmorHarmony".Translate());
        }
    }
}
