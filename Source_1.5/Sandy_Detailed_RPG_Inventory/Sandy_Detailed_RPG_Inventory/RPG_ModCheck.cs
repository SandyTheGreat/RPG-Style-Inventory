using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using TorannMagic;

namespace Sandy_Detailed_RPG_Inventory
{
    public abstract class RPG_ModCheck
    {
        private static bool initialized = false;
        private static bool RWoMIsActive = false;

        public static bool IsRWoMActive()
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

        public static bool HasCloak(Pawn p)
        {
            bool flag = false;
            if (IsRWoMActive())
            {
                foreach (Apparel current in p.apparel.WornApparel)
                {
                    if (current.def.apparel.layers.Contains(TorannMagicDefOf.TM_Cloak))
                    {
                        flag = true;
                    }
                }
            }
            return flag;
        }

        public static bool HasCape(Pawn p)
        {
            bool flag = false;
            foreach (Apparel current in p.apparel.WornApparel)
            {
                if (current.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Neck) && !current.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso) && current.def.apparel.layers.Contains(ApparelLayerDefOf.Shell))
                {
                    flag = true;
                }
            }
            return flag;
        }

        public static bool HasAccessory_Neck(Pawn p)
        {
            bool flag = false;
            foreach (Apparel current in p.apparel.WornApparel)
            {
                if (current.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Neck) && !current.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Shoulders)
                        && current.def.apparel.layers.Contains(ApparelLayerDefOf.Overhead) && !current.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
                {
                    flag = true;
                }
            }
            return flag;
        }

        public static bool HasArtifact_Neck(Pawn p)
        {
            bool flag = false;
            if (IsRWoMActive())
            {
                foreach (Apparel current in p.apparel.WornApparel)
                {
                    if (current.def.apparel.layers.Contains(TorannMagicDefOf.TM_Artifact) && current.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Neck))
                    {
                        flag = true;
                    }
                }
            }
            return flag;
        }

        public static bool HasArtifact_LeftHand(Pawn p)
        {
            bool flag = false;
            if (IsRWoMActive())
            {
                foreach (Apparel current in p.apparel.WornApparel)
                {
                    if (current.def.apparel.layers.Contains(TorannMagicDefOf.TM_Artifact) && current.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.LeftHand))
                    {
                        flag = true;
                    }
                }
            }
            return flag;
        }

        public static bool HasApparel_LeftHand(Pawn p)
        {
            bool flag = false;
            foreach (Apparel current in p.apparel.WornApparel)
            {
                if (current.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.LeftHand) && !current.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Hands)
                        && current.def.apparel.layers.Contains(ApparelLayerDefOf.OnSkin) && !current.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso)
                        && !current.def.apparel.layers.Contains(ApparelLayerDefOf.Middle) && !current.def.apparel.layers.Contains(ApparelLayerDefOf.Shell))
                {
                    flag = true;
                }
            }
            return flag;
        }

        public static bool HasArtifact_RightHand(Pawn p)
        {
            bool flag = false;
            if (IsRWoMActive())
            {
                foreach (Apparel current in p.apparel.WornApparel)
                {
                    if (current.def.apparel.layers.Contains(TorannMagicDefOf.TM_Artifact) && current.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.RightHand))
                    {
                        flag = true;
                    }
                }
            }
            return flag;
        }

        public static bool HasApparel_RightHand(Pawn p)
        {
            bool flag = false;
            foreach (Apparel current in p.apparel.WornApparel)
            {
                if (current.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.RightHand) && !current.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Hands)
                        && current.def.apparel.layers.Contains(ApparelLayerDefOf.OnSkin) && !current.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso)
                        && !current.def.apparel.layers.Contains(ApparelLayerDefOf.Middle) && !current.def.apparel.layers.Contains(ApparelLayerDefOf.Shell))
                {
                    flag = true;
                }
            }
            return flag;
        }

        public static bool HasArtifact_Arms(Pawn p)
        {
            bool flag = false;
            if (IsRWoMActive())
            {
                foreach (Apparel current in p.apparel.WornApparel)
                {
                    if (current.def.apparel.layers.Contains(TorannMagicDefOf.TM_Artifact) && current.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Arms))
                    {
                        flag = true;
                    }
                }
            }
            return flag;
        }

        public static bool HasApparel_Arms(Pawn p)
        {
            bool flag = false;
            foreach (Apparel current in p.apparel.WornApparel)
            {
                if (current.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Hands) && !current.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso)
                        && (current.def.apparel.layers.Contains(ApparelLayerDefOf.Shell) || current.def.apparel.layers.Contains(ApparelLayerDefOf.Overhead)))
                {
                    flag = true;
                }
            }
            return flag;
        }

        public static bool HasArtifact_Other(Pawn p)
        {
            bool flag = false;
            if (IsRWoMActive())
            {
                foreach (Apparel current in p.apparel.WornApparel)
                {
                    if (current.def.apparel.layers.Contains(TorannMagicDefOf.TM_Artifact))
                    {
                        flag = true;
                    }
                }
            }
            return flag;
        }

        public static StatDef GetHarmonyStatDef()
        {
            return TorannMagicDefOf.ArmorRating_Alignment;
        }

        public static ApparelLayerDef GetArtifactLayer()
        {
            return TorannMagicDefOf.TM_Artifact;
        }

        public static ApparelLayerDef GetCloakLayer()
        {
            return TorannMagicDefOf.TM_Cloak;
        }

        public static string GetEnchantmentString(Thing apparel2)
        {
            return TorannMagic.TM_Calc.GetEnchantmentsString(apparel2);
        }
    }
}
