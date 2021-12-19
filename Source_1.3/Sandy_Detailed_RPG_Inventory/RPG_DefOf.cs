using System;
using Verse;
using RimWorld;

namespace Sandy_Detailed_RPG_Inventory
{
    [DefOf]
    public static class Sandy_Gear_DefOf
    {
        public static BodyPartGroupDef Teeth;
        public static BodyPartGroupDef Mouth;
        public static BodyPartGroupDef Neck;
        public static BodyPartGroupDef Shoulders;
        public static BodyPartGroupDef Arms;
        public static BodyPartGroupDef Hands;
        public static BodyPartGroupDef Waist;
        public static BodyPartGroupDef Feet;

        //Jewelry
        //Two def files were added, they are in Defs\Jewelry_compat
        public static BodyPartGroupDef Ears;
        public static ApparelLayerDef Accessories;

        //Combat Shields
        public static ApparelLayerDef Shield;

        //UnderWhere
        public static ApparelLayerDef UnderwearTop;
        public static ApparelLayerDef Underwear;

        //VFE
        public static ApparelLayerDef VFEC_OuterShell;
    }
}