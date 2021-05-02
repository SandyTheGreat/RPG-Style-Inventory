using System;
using HarmonyLib;
using Verse;
using RimWorld;
using System.Reflection;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Sandy_Detailed_RPG_Inventory;
using Sandy_Detailed_RPG_Inventory.MODIntegrations;
using CombatExtended;
using Verse.Sound;
using Verse.AI;
using System.Reflection.Emit;

namespace CEPatches
{
    [StaticConstructorOnStartup]
    static class RPG_CEPatch
    {
        static RPG_CEPatch()
        {
            if (ModsConfig.ActiveModsInLoadOrder.FirstOrDefault(x => x.PackageId == "ceteam.combatextended") == null
                || ModsConfig.ActiveModsInLoadOrder.FirstOrDefault(x => x.PackageId == "sandy.rpgstyleinventory") == null)
                return;
            //
            //shenanigans to hide HarmonyLib from StartupConstructor
            //so game doesn't freak out if Harmony doesn't exist
            RPG_CEPatches.DoPatch();
        }

        static class RPG_CEPatches
        {
            public static readonly Texture2D texBulk = ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Bulk_Icon", true);
            static MethodInfo LDrawStats1;
            static MethodInfo LDrawStats;
            static FieldInfo LstatIconSize;
            static FieldInfo LstdThingIconSize;
            static FieldInfo LstdThingRowHeight;
            static FieldInfo LstdLineHeight;
            static MethodInfo LTryDrawComfyTemperatureRange1;
            static MethodInfo LTryDrawComfyTemperatureRange;
            static PropertyInfo LSelPawnForGear;
            static PropertyInfo LSelPawn;
            static PropertyInfo LCanControl;
            static MethodInfo LInterfaceDrop;
            static MethodInfo LInterfaceIngest;
            static MethodInfo LDrawThingRow1;
            static MethodInfo LDrawThingRow;
            static MethodInfo LShouldShowInventory;
            static MethodInfo LShouldShowOverallArmor;
            static MethodInfo LThingDetailedTip;
            static MethodInfo LDrawInventory;
            static MethodInfo LFillTab;
            static FieldInfo LviewList;
            //
            static Type Utility_HoldTracker;
            static MethodInfo LHoldTrackerIsHeld;
            static MethodInfo LHoldTrackerForget;

            static public void DoPatch()
            {
                Type RPGTab = typeof(Sandy_Detailed_RPG_GearTab);
                //that's why private stuff sucks ass
                LDrawStats1 = AccessTools.Method(RPGTab, "DrawStats1");
                LDrawStats = AccessTools.Method(RPGTab, "DrawStats");
                LstatIconSize = AccessTools.Field(RPGTab, "statIconSize");
                LstdThingIconSize = AccessTools.Field(RPGTab, "stdThingIconSize"); ;
                LstdThingRowHeight = AccessTools.Field(RPGTab, "stdThingRowHeight");
                LstdLineHeight = AccessTools.Field(RPGTab, "stdLineHeight");
                LTryDrawComfyTemperatureRange1 = AccessTools.Method(RPGTab, "TryDrawComfyTemperatureRange1");
                LTryDrawComfyTemperatureRange = AccessTools.Method(RPGTab, "TryDrawComfyTemperatureRange");
                LSelPawnForGear = AccessTools.Property(RPGTab, "SelPawnForGear");
                LSelPawn = AccessTools.Property(typeof(ITab), "SelPawn");
                LCanControl = AccessTools.Property(RPGTab, "CanControl");
                LInterfaceDrop = AccessTools.Method(RPGTab, "InterfaceDrop");
                LInterfaceIngest = AccessTools.Method(RPGTab, "InterfaceIngest");
                LDrawThingRow1 = AccessTools.Method(RPGTab, "DrawThingRow1");
                LDrawThingRow = AccessTools.Method(RPGTab, "DrawThingRow");
                LShouldShowInventory = AccessTools.Method(RPGTab, "ShouldShowInventory");
                LShouldShowOverallArmor = AccessTools.Method(RPGTab, "ShouldShowOverallArmor");
                LThingDetailedTip = AccessTools.Method(RPGTab, "ThingDetailedTip");
                LDrawInventory = AccessTools.Method(RPGTab, "DrawInventory");
                LFillTab = AccessTools.Method(RPGTab, "FillTab");
                LviewList = AccessTools.Field(RPGTab, "viewList");
                //
                Utility_HoldTracker = AccessTools.TypeByName("Utility_HoldTracker");
                LHoldTrackerIsHeld = AccessTools.Method(Utility_HoldTracker, "HoldTrackerIsHeld");
                LHoldTrackerForget = AccessTools.Method(Utility_HoldTracker, "HoldTrackerForget");
                //
                var harmonyInstance = new Harmony("net.avilmask.rimworld.mod.RPG_CEPatches");
                //
                HarmonyMethod hm = new HarmonyMethod(typeof(RPG_CEPatches), nameof(RPG_CEPatches.DrawStats1Prefix));
                harmonyInstance.Patch(LDrawStats1, hm, null);
                //
                hm = new HarmonyMethod(typeof(RPG_CEPatches), nameof(RPG_CEPatches.DrawThingRow1Postfix));
                harmonyInstance.Patch(LDrawThingRow1, null, hm);
                //
                hm = new HarmonyMethod(typeof(RPG_CEPatches), nameof(RPG_CEPatches.ThingDetailedTipPrefix));
                harmonyInstance.Patch(LThingDetailedTip, hm);
                //
                hm = new HarmonyMethod(typeof(RPG_CEPatches), nameof(RPG_CEPatches.DrawThingRowPostfix));
                harmonyInstance.Patch(LDrawThingRow, null, hm);
                //
                hm = new HarmonyMethod(typeof(RPG_CEPatches), nameof(RPG_CEPatches.InterfaceDropPrefix));
                harmonyInstance.Patch(LInterfaceDrop, hm);
                //
                hm = new HarmonyMethod(typeof(RPG_CEPatches), nameof(RPG_CEPatches.DrawStatsPrefix));
                harmonyInstance.Patch(LDrawStats, hm);
                //
                hm = new HarmonyMethod(typeof(RPG_CEPatches), nameof(RPG_CEPatches.DrawInventoryPrefix));
                harmonyInstance.Patch(LDrawInventory, hm);
                
                hm = new HarmonyMethod(typeof(RPG_CEPatches), nameof(RPG_CEPatches.FillTabTranspiler));
                harmonyInstance.Patch(LFillTab, null, null, hm);
            }

            static bool DrawStats1Prefix(object __instance, ref float top, float left)
            {
                TryDrawMassInfo1(__instance, ref top, left, Sandy_Detailed_RPG_GearTab.statPanelWidth);
                object[] args = new object[] { top, left, Sandy_Detailed_RPG_GearTab.statPanelWidth };
                LTryDrawComfyTemperatureRange1.Invoke(__instance, args);
                top = (float)args[0];
                //
                Pawn pawn = (Pawn)LSelPawnForGear.GetValue(__instance);
                bool flag = (bool)LShouldShowOverallArmor.Invoke(__instance, new object[] { pawn });
                if (flag)
                {
                    TryDrawOverallArmor1(__instance, ref top, left, (float)args[2], StatDefOf.ArmorRating_Sharp, "ArmorSharp".Translate(),
                        "CE_mmRHA".Translate(), Sandy_Utility.texArmorSharp);
                    TryDrawOverallArmor1(__instance, ref top, left, (float)args[2], StatDefOf.ArmorRating_Blunt, "ArmorBlunt".Translate(),
                        "CE_MPa".Translate(), Sandy_Utility.texArmorBlunt);
                    TryDrawOverallArmor1(__instance, ref top, left, (float)args[2], StatDefOf.ArmorRating_Heat, "ArmorHeat".Translate(),
                        "%", Sandy_Utility.texArmorHeat);
                }
                //
                MODIntegration.DrawStats1((Sandy_Detailed_RPG_GearTab)__instance, ref top, left, flag);
                return false;
            }

            static void DrawThingRow1Postfix(object __instance, Rect rect, Thing thing)
            {
                var flag = Widgets.ButtonInvisible(rect, true) && Event.current.button == 1;
                if (!flag) return;
                //
                DropDownThingMenu(__instance, thing);
            }

            static bool ThingDetailedTipPrefix(ref string __result, Thing thing, bool inventory)
            {
                __result = thing.DescriptionDetailed;

                if (inventory)
                {
                    __result = string.Concat(new object[] { __result, "\n", thing.GetBulkTip() });
                }
                else
                {
                    __result = string.Concat(new object[] { __result, "\n", thing.GetWeightAndBulkTip() });
                }

                if (thing.def.useHitPoints)
                {
                    __result = string.Concat(new object[]
                    {
                        __result,
                        "\n",
                        thing.HitPoints,
                        " / ",
                        thing.MaxHitPoints
                    });
                }

                return false;
            }

            static void DrawThingRowPostfix(object __instance, ref float y, float width, Thing thing)
            {
                Rect rect = new Rect(0, y - (float)LstdThingRowHeight.GetValue(null), width, (float)LstdThingRowHeight.GetValue(null));
                bool flag = Widgets.ButtonInvisible(rect, true) && Event.current.button == 1;
                if (!flag) return;
                DropDownThingMenu(__instance, thing);
            }

            static void InterfaceDropPrefix(object __instance, Thing t)
            {
                Pawn pawn = (Pawn)LSelPawnForGear.GetValue(__instance);
                bool flag = (bool)LHoldTrackerIsHeld.Invoke(null, new object[] { pawn, t });
                if (flag)
                {
                    LHoldTrackerForget.Invoke(null, new object[] { pawn, t });
                }
            }

            static bool DrawStatsPrefix(object __instance, ref float top, Rect rect)
            {
                object[] args = new object[] { top, rect.width };
                LTryDrawComfyTemperatureRange.Invoke(__instance, args);
                top = (float)args[0];
                Widgets.ListSeparator(ref top, rect.width, "OverallArmor".Translate());
                Pawn pawn = (Pawn)LSelPawnForGear.GetValue(__instance);
                bool showArmor = (bool)LShouldShowOverallArmor.Invoke(__instance, new object[] { pawn });
                if (showArmor)
                {
                    TryDrawOverallArmor(__instance, ref top, rect.width, StatDefOf.ArmorRating_Blunt, "ArmorBlunt".Translate(), " " + "CE_MPa".Translate());
                    TryDrawOverallArmor(__instance, ref top, rect.width, StatDefOf.ArmorRating_Sharp, "ArmorSharp".Translate(), "CE_mmRHA".Translate());
                    TryDrawOverallArmor(__instance, ref top, rect.width, StatDefOf.ArmorRating_Heat, "ArmorHeat".Translate(), "%");
                }

                MODIntegration.DrawStats((Sandy_Detailed_RPG_GearTab)__instance, ref top, rect, showArmor);
                return false;
            }

            static void DrawInventoryPrefix(object __instance, ref Rect viewRect, ref float num, bool inventory = false)
            {
                if (!inventory) return;
                //  
                Pawn pawn = (Pawn)LSelPawnForGear.GetValue(__instance);
                bool flag;
                Loadout loadout = pawn.GetLoadout();
                if (pawn.IsColonist && (loadout == null || loadout.Slots.NullOrEmpty<LoadoutSlot>()))
                {
                    if (!pawn.inventory.innerContainer.Any<Thing>())
                    {
                        Pawn_EquipmentTracker equipment = pawn.equipment;
                        flag = (((equipment != null) ? equipment.Primary : null) != null);
                    }
                    else
                    {
                        flag = true;
                    }
                }
                else
                {
                    flag = false;
                }
                //
                if (!flag) return;

                num += 3f;
                Rect rect = new Rect(viewRect.width / 2f + 2f, num, viewRect.width / 2f - 2f, 26f);

                bool pressed = Widgets.ButtonText(rect, "CE_MakeLoadout".Translate(), true, true, true);
                if (pressed)
                {
                    loadout = pawn.GenerateLoadoutFromPawn();
                    LoadoutManager.AddLoadout(loadout);
                    pawn.SetLoadout(loadout);
                    Find.WindowStack.Add(new Dialog_ManageLoadouts(pawn.GetLoadout()));
                }

                num += 3f;
            }

            static IEnumerable<object> FillTabTranspiler(IEnumerable<object> instrs)
            {
                MethodInfo LBeginGroup = AccessTools.Method(typeof(GUI), nameof(GUI.BeginGroup), new Type[] { typeof(Rect) });
                MethodInfo LDrawBars = AccessTools.Method(typeof(RPG_CEPatches), nameof(RPG_CEPatches.FillTab_DrawBars));
                foreach (var i in (instrs))
                {
                    if ((i as CodeInstruction).opcode == OpCodes.Call && (i as CodeInstruction).operand == (object)LBeginGroup)
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, LDrawBars);
                        yield return new CodeInstruction(OpCodes.Stloc_1);
                        yield return new CodeInstruction(OpCodes.Ldloc_1);
                    }

                    yield return i;
                }
            }

            static string FormatArmorValue(float value, string unit)
            {
                bool flag = unit.Equals("%");
                if (flag)
                {
                    value *= 100f;
                }
                return value.ToStringByStyle(flag ? ToStringStyle.FloatMaxOne : ToStringStyle.FloatMaxTwo, ToStringNumberSense.Absolute) + unit;
            }

            static float AgregateApparelStat(IEnumerable<Apparel> apparels, StatDef stat)
            {
                if (apparels.EnumerableNullOrEmpty())
                    return 0;
                //
                float num = 0;
                foreach (Apparel apparel2 in apparels)
                {
                    num += apparel2.GetStatValue(stat, true) * apparel2.def.apparel.HumanBodyCoverage;
                }
                return num;
            }

            static void TryDrawOverallArmor1(object tab, ref float top, float left, float width, StatDef stat, string label, string unit, Texture image)
            {
                Pawn pawn = (Pawn)LSelPawnForGear.GetValue(tab);
                //
                float statValue = pawn.GetStatValue(stat, true);
                float num = statValue;
                Pawn_ApparelTracker apparel = pawn.apparel;
                List<Apparel> list = pawn?.apparel.WornApparel;
                num += AgregateApparelStat(list, stat);
                //
                if (num > 0.0001f)
                {
                    float statIconSize = (float)LstatIconSize.GetValue(null);
                    float stdThingIconSize = (float)LstdThingIconSize.GetValue(null);
                    float stdThingRowHeight = (float)LstdThingRowHeight.GetValue(null);
                    //
                    string text = AgregateApparelBreakDown(pawn.RaceProps.body.AllParts, list, stat, statValue, unit);
                    //
                    Rect rect1 = new Rect(left, top, statIconSize, statIconSize);
                    GUI.DrawTexture(rect1, image);
                    TooltipHandler.TipRegion(rect1, label);
                    Rect rect2 = new Rect(left + stdThingIconSize + 4f, top + (stdThingRowHeight - statIconSize) / 2f, width - stdThingIconSize - 4f, statIconSize);
                    Widgets.Label(rect2, FormatArmorValue(num, unit));
                    TooltipHandler.TipRegion(rect2, text);
                    top += stdThingRowHeight;
                }
            }

            static string AgregateApparelBreakDown(IEnumerable<BodyPartRecord> bodyParts, IEnumerable<Apparel> apparels, StatDef stat, float natValue, string unit)
            {
                string text = "";
                foreach (BodyPartRecord bodyPartRecord in bodyParts)
                {
                    float num = bodyPartRecord.IsInGroup(CE_BodyPartGroupDefOf.CoveredByNaturalArmor) ? natValue : 0f;
                    if (bodyPartRecord.depth == BodyPartDepth.Outside
                        && (bodyPartRecord.coverage >= 0.1 || bodyPartRecord.def == BodyPartDefOf.Eye || bodyPartRecord.def == BodyPartDefOf.Neck))
                    {
                        text = text + bodyPartRecord.LabelCap + ": ";
                        if (!apparels.EnumerableNullOrEmpty())
                            foreach (Apparel apparel in apparels)
                            {
                                if (apparel.def.apparel.CoversBodyPart(bodyPartRecord))
                                {
                                    num += apparel.GetStatValue(stat, true);
                                }
                            }
                        text = text + FormatArmorValue(num, unit) + "\n";
                    }
                }
                return text;
            }

            static void DropDownThingMenu(object tab, Thing thing)
            {
                Pawn SelPawnForGear = (Pawn)LSelPawnForGear.GetValue(tab);
                Pawn SelPawn = (Pawn)LSelPawn.GetValue(tab);
                bool canControl = (bool)LCanControl.GetValue(tab);
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                list.Add(new FloatMenuOption("ThingInfo".Translate(), delegate ()
                {
                    Find.WindowStack.Add(new Dialog_InfoCard(thing));
                }, MenuOptionPriority.Default, null, null, 0f, null, null));
                //bool canControl = this.CanControl;
                if (canControl)
                {
                    ThingWithComps eq = thing as ThingWithComps;
                    bool flag10 = eq != null && eq.TryGetComp<CompEquippable>() != null;
                    if (flag10)
                    {
                        CompInventory compInventory = SelPawnForGear.TryGetComp<CompInventory>();
                        CompBiocodable compBiocodable = eq.TryGetComp<CompBiocodable>();
                        bool flag11 = compInventory != null;
                        if (flag11)
                        {
                            string value = GenLabel.ThingLabel(eq.def, eq.Stuff, 1);
                            bool flag12 = compBiocodable != null && compBiocodable.Biocoded && compBiocodable.CodedPawn != SelPawnForGear;
                            FloatMenuOption item;
                            if (flag12)
                            {
                                item = new FloatMenuOption("CannotEquip".Translate(value) + ": " + "BiocodedCodedForSomeoneElse".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                            }
                            else
                            {
                                bool flag13 = SelPawnForGear.IsQuestLodger() && !EquipmentUtility.QuestLodgerCanEquip(eq, SelPawnForGear);
                                if (flag13)
                                {
                                    TaggedString t = SelPawnForGear.equipment.AllEquipmentListForReading.Contains(eq) ? "CE_CannotPutAway".Translate(value) : "CannotEquip".Translate(value);
                                    item = new FloatMenuOption(t + ": " + "CE_CannotChangeEquipment".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                                }
                                else
                                {
                                    bool flag14 = SelPawnForGear.equipment.AllEquipmentListForReading.Contains(eq) && SelPawnForGear.inventory != null;
                                    if (flag14)
                                    {
                                        item = new FloatMenuOption("CE_PutAway".Translate(value), delegate ()
                                        {
                                            SelPawnForGear.equipment.TryTransferEquipmentToContainer(SelPawnForGear.equipment.Primary, SelPawnForGear.inventory.innerContainer);
                                        }, MenuOptionPriority.Default, null, null, 0f, null, null);
                                    }
                                    else
                                    {
                                        bool flag15 = !SelPawnForGear.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
                                        if (flag15)
                                        {
                                            item = new FloatMenuOption("CannotEquip".Translate(value), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                                        }
                                        else
                                        {
                                            string text4 = "Equip".Translate(value);
                                            bool flag16 = eq.def.IsRangedWeapon && SelPawnForGear.story != null && SelPawnForGear.story.traits.HasTrait(TraitDefOf.Brawler);
                                            if (flag16)
                                            {
                                                text4 = text4 + " " + "EquipWarningBrawler".Translate();
                                            }
                                            item = new FloatMenuOption(text4, (SelPawnForGear.story != null && SelPawnForGear.WorkTagIsDisabled(WorkTags.Violent)) ? null : new Action(delegate ()
                                            {
                                                compInventory.TrySwitchToWeapon(eq);
                                            }), MenuOptionPriority.Default, null, null, 0f, null, null);
                                        }
                                    }
                                }
                            }
                            list.Add(item);
                        }
                    }
                    //Pawn selPawnForGear = SelPawnForGear;   //??
                    List<Apparel> list2;
                    if (SelPawnForGear == null)
                    {
                        list2 = null;
                    }
                    else
                    {
                        Pawn_ApparelTracker apparel2 = SelPawnForGear.apparel;
                        list2 = ((apparel2 != null) ? apparel2.WornApparel : null);
                    }
                    List<Apparel> list3 = list2;
                    using (List<Apparel>.Enumerator enumerator = list3.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            Apparel apparel = enumerator.Current;
                            CompReloadable compReloadable = apparel.TryGetComp<CompReloadable>();
                            bool flag17 = compReloadable != null && compReloadable.AmmoDef == thing.def && compReloadable.NeedsReload(true);
                            if (flag17)
                            {
                                bool flag18 = !SelPawnForGear.Drafted;
                                if (flag18)
                                {
                                    FloatMenuOption item2 = new FloatMenuOption("CE_ReloadApparel".Translate(apparel.Label, thing.Label), delegate ()
                                    {
                                        SelPawnForGear.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Reload, apparel, thing), JobTag.Misc);
                                    }, MenuOptionPriority.Default, null, null, 0f, null, null);
                                    list.Add(item2);
                                }
                            }
                        }
                    }
                    bool flag19 = canControl && thing.IngestibleNow && SelPawn.RaceProps.CanEverEat(thing);
                    if (flag19)
                    {
                        Action action = delegate ()
                        {
                            SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                            LInterfaceIngest.Invoke(tab, new object[] { thing });
                        };
                        string text5 = thing.def.ingestible.ingestCommandString.NullOrEmpty() ? (string)"ConsumeThing".Translate(thing.LabelShort, thing) : string.Format(thing.def.ingestible.ingestCommandString, thing.LabelShort);
                        bool flag20 = SelPawnForGear.IsTeetotaler() && thing.def.IsNonMedicalDrug;
                        if (flag20)
                        {
                            List<FloatMenuOption> list4 = list;
                            string str = text5;
                            string str2 = ": ";
                            TraitDegreeData traitDegreeData = (from x in TraitDefOf.DrugDesire.degreeDatas
                                                               where x.degree == -1
                                                               select x).First<TraitDegreeData>();
                            list4.Add(new FloatMenuOption(str + str2 + ((traitDegreeData != null) ? traitDegreeData.label : null), null, MenuOptionPriority.Default, null, null, 0f, null, null));
                        }
                        else
                        {
                            list.Add(new FloatMenuOption(text5, action, MenuOptionPriority.Default, null, null, 0f, null, null));
                        }
                    }
                    bool flag21 = SelPawnForGear.IsItemQuestLocked(eq);
                    if (flag21)
                    {
                        list.Add(new FloatMenuOption("CE_CannotDropThing".Translate() + ": " + "DropThingLocked".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null));
                        list.Add(new FloatMenuOption("CE_CannotDropThingHaul".Translate() + ": " + "DropThingLocked".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null));
                    }
                    else
                    {
                        list.Add(new FloatMenuOption("DropThing".Translate(), delegate ()
                        {
                            SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                            LInterfaceDrop.Invoke(tab, new object[] { thing });
                        }, MenuOptionPriority.Default, null, null, 0f, null, null));
                        list.Add(new FloatMenuOption("CE_DropThingHaul".Translate(), delegate ()
                        {
                            SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                            InterfaceDropHaul(SelPawnForGear, thing, SelPawn);
                        }, MenuOptionPriority.Default, null, null, 0f, null, null));
                    }
                    bool flag22 = canControl && (bool)LHoldTrackerIsHeld.Invoke(null, new object[] { SelPawnForGear, thing }); //SelPawnForGear.HoldTrackerIsHeld(thing);
                    if (flag22)
                    {
                        Action action2 = delegate ()
                        {
                            SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                            LHoldTrackerForget.Invoke(null, new object[] { SelPawnForGear, thing }); //SelPawnForGear.HoldTrackerForget(thing);
                        };
                        list.Add(new FloatMenuOption("CE_HoldTrackerForget".Translate(), action2, MenuOptionPriority.Default, null, null, 0f, null, null));
                    }
                }
                FloatMenu window = new FloatMenu(list, thing.LabelCap, false);
                Find.WindowStack.Add(window);
            }

            static void InterfaceDropHaul(Pawn pawn, Thing thing, Pawn selPawn)
            {
                bool flag = (bool)LHoldTrackerIsHeld.Invoke(null, new object[] { pawn, thing });
                if (flag)
                {
                    LHoldTrackerForget.Invoke(null, new object[] { pawn, thing });
                }
                ThingWithComps thingWithComps = thing as ThingWithComps;
                Apparel apparel = thing as Apparel;
                bool flag2 = apparel != null && pawn.apparel != null && pawn.apparel.WornApparel.Contains(apparel);
                if (flag2)
                {
                    Job job = JobMaker.MakeJob(JobDefOf.RemoveApparel, apparel);
                    job.haulDroppedApparel = true;
                    pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                }
                else
                {
                    bool flag3 = thingWithComps != null && pawn.equipment != null && pawn.equipment.AllEquipmentListForReading.Contains(thingWithComps);
                    if (flag3)
                    {
                        pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.DropEquipment, thingWithComps), JobTag.Misc);
                    }
                    else
                    {
                        bool flag4 = !thing.def.destroyOnDrop;
                        if (flag4)
                        {
                            Thing t;
                            selPawn.inventory.innerContainer.TryDrop(thing, selPawn.Position, selPawn.Map, ThingPlaceMode.Near, out t, null, null);
                        }
                    }
                }
            }

            static void TryDrawMassInfo1(object tab, ref float top, float left, float width)
            {
                Pawn pawn = (Pawn)LSelPawnForGear.GetValue(tab);
                if (pawn.Dead || !(bool)LShouldShowInventory.Invoke(tab, new object[] { pawn }))
                    return;
                //
                CompInventory compInventory = pawn.TryGetComp<CompInventory>();
                if (compInventory == null)
                    return;
                //
                float statIconSize = (float)LstatIconSize.GetValue(null);
                float stdThingIconSize = (float)LstdThingIconSize.GetValue(null);
                float stdThingRowHeight = (float)LstdThingRowHeight.GetValue(null);
                //weight
                Rect rect1 = new Rect(left, top, statIconSize, statIconSize);
                GUI.DrawTexture(rect1, Sandy_Utility.texMass);
                TooltipHandler.TipRegion(rect1, "CE_Weight".Translate());
                float val1 = compInventory.currentWeight;
                float val2 = compInventory.capacityWeight;
                string str = val1.ToString("0.#");//MassUtility.GearAndInventoryMass(pawn);
                string str2 = CE_StatDefOf.CarryWeight.ValueToString(val2, CE_StatDefOf.CarryWeight.toStringNumberSense, true); //MassUtility.Capacity(pawn, null);
                Rect rect2 = new Rect(left + stdThingIconSize, top + (stdThingRowHeight - statIconSize) / 2f, width - stdThingIconSize, statIconSize);
                Utility_Loadouts.DrawBar(rect2, val1, val2, "", pawn.GetWeightTip());
                rect2.xMin += 4f;
                rect2.yMin += 2f;
                Widgets.Label(rect2, str + '/' + str2);
                //TooltipHandler.TipRegion(rect2, pawn.GetWeightTip());
                top += stdThingRowHeight;
                //bulk
                rect1 = new Rect(left, top, statIconSize, statIconSize);
                GUI.DrawTexture(rect1, texBulk);
                TooltipHandler.TipRegion(rect1, "CE_Bulk".Translate());
                val1 = compInventory.currentBulk;
                val2 = compInventory.capacityBulk;
                str = CE_StatDefOf.CarryBulk.ValueToString(val1, CE_StatDefOf.CarryBulk.toStringNumberSense, true);//MassUtility.GearAndInventoryMass(pawn);
                str2 = CE_StatDefOf.CarryBulk.ValueToString(val2, CE_StatDefOf.CarryBulk.toStringNumberSense, true); //MassUtility.Capacity(pawn, null);
                rect2 = new Rect(left + stdThingIconSize, top + (stdThingRowHeight - statIconSize) / 2f, width - stdThingIconSize, statIconSize);
                Utility_Loadouts.DrawBar(rect2, val1, val2, "", pawn.GetBulkTip());
                rect2.xMin += 4f;
                rect2.yMin += 2f;
                Widgets.Label(rect2, str + '/' + str2);
                //TooltipHandler.TipRegion(rect2, pawn.GetBulkTip());
                top += stdThingRowHeight;
            }

            static void TryDrawOverallArmor(object tab, ref float top, float width, StatDef stat, string label, string unit)
            {
                Pawn pawn = (Pawn)LSelPawnForGear.GetValue(tab);
                //
                float statValue = pawn.GetStatValue(stat, true);
                float num = statValue;
                Pawn_ApparelTracker apparel = pawn.apparel;
                List<Apparel> list = pawn?.apparel.WornApparel;
                num += AgregateApparelStat(list, stat);
                //
                if (num > 0.0001f)
                {
                    float statIconSize = (float)LstatIconSize.GetValue(null);
                    float stdThingIconSize = (float)LstdThingIconSize.GetValue(null);
                    float stdThingRowHeight = (float)LstdThingRowHeight.GetValue(null);
                    //
                    string text = AgregateApparelBreakDown(pawn.RaceProps.body.AllParts, list, stat, statValue, unit);
                    //
                    Rect rect1 = new Rect(0f, top, width, stdThingRowHeight);
                    Widgets.Label(rect1, label.Truncate(120f, null));

                    rect1.xMin += 120f;
                    Widgets.Label(rect1, FormatArmorValue(num, unit));
                    TooltipHandler.TipRegion(rect1, text);
                    top += (float)LstdLineHeight.GetValue(null);
                }
            }

            static Rect FillTab_DrawBars(Rect position, object tab)
            {
                bool viewList = (bool)LviewList.GetValue(tab);
                if (!viewList) return position;
                Pawn pawn = (Pawn)LSelPawn.GetValue(tab);
                CompInventory compInventory = pawn.TryGetComp<CompInventory>();
                if (compInventory == null) return position;

                PlayerKnowledgeDatabase.KnowledgeDemonstrated(CE_ConceptDefOf.CE_InventoryWeightBulk, KnowledgeAmount.FrameDisplayed);
                position.height -= 55f;
                Rect rect = new Rect(15f, position.yMax + 7.5f, position.width - 10f, 20f);
                Rect rect2 = new Rect(15f, rect.yMax + 7.5f, position.width - 10f, 20f);
                Utility_Loadouts.DrawBar(rect2, compInventory.currentBulk, compInventory.capacityBulk, "CE_Bulk".Translate(), pawn.GetBulkTip());
                Utility_Loadouts.DrawBar(rect, compInventory.currentWeight, compInventory.capacityWeight, "CE_Weight".Translate(), pawn.GetWeightTip());
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                string str = CE_StatDefOf.CarryBulk.ValueToString(compInventory.currentBulk, CE_StatDefOf.CarryBulk.toStringNumberSense, true);
                string str2 = CE_StatDefOf.CarryBulk.ValueToString(compInventory.capacityBulk, CE_StatDefOf.CarryBulk.toStringNumberSense, true);
                Widgets.Label(rect2, str + "/" + str2);
                string str3 = compInventory.currentWeight.ToString("0.#");
                string str4 = CE_StatDefOf.CarryWeight.ValueToString(compInventory.capacityWeight, CE_StatDefOf.CarryWeight.toStringNumberSense, true);
                Widgets.Label(rect, str3 + "/" + str4);
                Text.Anchor = TextAnchor.UpperLeft;
                return position;
            }
        }
    }
}