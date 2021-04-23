using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Sandy_Detailed_RPG_Inventory
{
    public class ItemSlotDef: Def
    {
        //public Func<ApparelProperties, bool> validator = null;
        public int xPos = int.MinValue;
        public int yPos = int.MinValue;
        public int validationOrder = 100;
        public List<ApparelLayerDef> apparelLayers = new List<ApparelLayerDef>();
        public List<BodyPartGroupDef> bodyPartGroups = new List<BodyPartGroupDef>();
        [Unsaved(false)]
        public int listid = int.MinValue;

        public bool Valid(ApparelProperties apparelProperties)
        {
            bool result;

            if (apparelLayers.NullOrEmpty())
            {
                result = true;
            }
            else
            {
                result = false;
                foreach (var layer in apparelLayers)
                    if (apparelProperties.layers.Contains(layer))
                    {
                        result = true;
                        break;
                    }
            }
            
            if (result && !bodyPartGroups.NullOrEmpty())
            {
                result = false;
                foreach (var bpg in bodyPartGroups)
                    if (apparelProperties.bodyPartGroups.Contains(bpg))
                    {
                        result = true;
                        break;
                    }
            }

            return result;
        }

    }

    public class Sandy_Detailed_RPG_GearTab : ITab_Pawn_Gear
	{
        private Vector2 scrollPosition = Vector2.zero;
		private float scrollViewHeight;
		private const float TopPadding = 20f;
		private const float ThingIconSize = 28f;
		private const float ThingRowHeight = 28f;
		private const float ThingLeftX = 36f;
		private const float StandardLineHeight = 22f;
		//private static List<Thing> workingInvList = new List<Thing>();
		public static readonly Vector3 PawnTextureCameraOffset = new Vector3(0f, 0f, 0f);
		private bool viewList = false;

        public Sandy_Detailed_RPG_GearTab()
		{
			this.labelKey = "TabGear";
			this.tutorTag = "Gear";
            Sandy_Detailed_RPG_GearTab.MakePreps(Sandy_RPG_Settings.displayAllSlots);
            this.UpdateSize();
		}

		protected override void UpdateSize()
		{
			this.size = new Vector2(Sandy_RPG_Settings.rpgTabWidth, Sandy_RPG_Settings.rpgTabHeight);
		}

		public override bool IsVisible
		{
			get
			{
				Pawn selPawnForGear = this.SelPawnForGear;
				return this.ShouldShowInventory(selPawnForGear) || this.ShouldShowApparel(selPawnForGear) || this.ShouldShowEquipment(selPawnForGear);
			}
		}

		/*private bool colonist
		{
			get
			{
				Pawn selPawnForGear = this.SelPawnForGear;
				return !this.SelPawnForGear.RaceProps.IsMechanoid && !this.SelPawnForGear.RaceProps.Animal;
			}
		}*/

		private bool CanControl
		{
			get
			{
				Pawn selPawnForGear = this.SelPawnForGear;
				return !selPawnForGear.Downed && !selPawnForGear.InMentalState && (selPawnForGear.Faction == Faction.OfPlayer || selPawnForGear.IsPrisonerOfColony) && (!selPawnForGear.IsPrisonerOfColony || !selPawnForGear.Spawned || selPawnForGear.Map.mapPawns.AnyFreeColonistSpawned) && (!selPawnForGear.IsPrisonerOfColony || (!PrisonBreakUtility.IsPrisonBreaking(selPawnForGear) && (selPawnForGear.CurJob == null || !selPawnForGear.CurJob.exitMapOnArrival)));
			}
		}

		private bool CanControlColonist
		{
			get
			{
				return this.CanControl && this.SelPawnForGear.IsColonistPlayerControlled;
			}
		}

		private Pawn SelPawnForGear
		{
			get
			{
				if (base.SelPawn != null)
				{
					return base.SelPawn;
				}
				Corpse corpse = base.SelThing as Corpse;
				if (corpse != null)
				{
					return corpse.InnerPawn;
				}
				throw new InvalidOperationException("Gear tab on non-pawn non-corpse " + base.SelThing);
			}
		}

        protected override void FillTab()
		{
            Text.Font = GameFont.Small;
			Rect rect0 = new Rect(20f, 0f, 100f, 30f);
			Widgets.CheckboxLabeled(rect0, "Sandy_ViewList".Translate(), ref viewList, false, null, null, false);
            Rect rect = new Rect(0f, 20f, this.size.x, this.size.y - 20f);
			Rect rect2 = rect.ContractedBy(10f);
			Rect position = new Rect(rect2.x, rect2.y, rect2.width, rect2.height);
			GUI.BeginGroup(position);
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
			Rect outRect = new Rect(0f, 0f, position.width, position.height);
			Rect viewRect = new Rect(0f, 0f, position.width - 20f, this.scrollViewHeight);
			Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);
			float num = 0f;

            if (viewList)
            {
                this.DrawViewList(ref num, viewRect);
            }
            else
            {
                //float curPanelWidth = Sandy_RPG_Settings.displayAllSlots ? 10f + xidx.Count() * 74f : slotPanelWidth;
                float curPanelWidth = slotPanelWidth;
                //basics
                if (this.SelPawnForGear.RaceProps.Humanlike)
                {
                    bool rightMost = Sandy_RPG_Settings.rpgTabWidth - curPanelWidth - 128f * 2 - 40f >= 0f;
                    float statsLeft = rightMost ? curPanelWidth + 128f + 20f : curPanelWidth;

                    //apparel basics
                    if (this.IsVisible)
                    {
                        //stats
                        this.DrawStats(ref num, statsLeft);
                        //Color color = new Color(1f, 1f, 1f, 1f);
                        GUI.color = new Color(1f, 1f, 1f, 1f);
                        //Pawn
                        float pawnTop = rightMost ? 0f : num;
                        Rect PawnRect = new Rect(curPanelWidth, pawnTop, 128f, 128f);
                        this.DrawColonist(PawnRect, this.SelPawnForGear);
                        num = rightMost ? Math.Max(num, 74f * 2) : num + 100f;
                    }
                    //equipment
                    List<Thing> unslotedEquipment = new List<Thing>();
                    if (this.ShouldShowEquipment(this.SelPawnForGear))
                    {
                        ThingWithComps secondary = null;
                        foreach (ThingWithComps current in this.SelPawnForGear.equipment.AllEquipmentListForReading)
                        {
                            if (current != this.SelPawnForGear.equipment.Primary)
                            {
                                if (secondary == null)
                                {
                                    secondary = current;
                                }
                                else
                                {
                                    unslotedEquipment.Add(current);
                                }
                            }
                        }

                        float val = rightMost ? 74f * 2 : ((int)(num / 74) + 1) * 74f;
                        if (secondary == null && !Sandy_RPG_Settings.displayAllSlots)
                        {
                            Rect newRect1 = new Rect(curPanelWidth + 32f, val, 64f, 64f);
                            GUI.DrawTexture(newRect1, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
                            if (this.SelPawnForGear.equipment.Primary == null)
                            {
                                Rect tipRect = newRect1.ContractedBy(12f);
                                TooltipHandler.TipRegion(newRect1, "Primary_Weapon".Translate());
                            }
                            else
                            {
                                this.DrawThingRow1(newRect1, this.SelPawnForGear.equipment.Primary, false, true);
                            }
                        }
                        else
                        {
                            Rect newRect1 = new Rect(curPanelWidth, val, 64f, 64f);
                            GUI.DrawTexture(newRect1, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
                            if (this.SelPawnForGear.equipment.Primary == null)
                            {
                                Rect tipRect = newRect1.ContractedBy(12f);
                                TooltipHandler.TipRegion(newRect1, "Primary_Weapon".Translate());
                            }
                            else
                            {
                                this.DrawThingRow1(newRect1, this.SelPawnForGear.equipment.Primary, false, true);
                            }
                            //
                            Rect newRect2 = new Rect(newRect1.x + 74f, newRect1.y, newRect1.width, newRect1.height);
                            GUI.DrawTexture(newRect2, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
                            if (secondary == null)
                            {
                                Rect tipRect = newRect2.ContractedBy(12f);
                                TooltipHandler.TipRegion(newRect2, "Secondary_Weapon".Translate());
                            }
                            else
                            {
                                this.DrawThingRow1(newRect2, secondary, false, true);
                            }
                        }

                        num = rightMost ? Math.Max(num, 74f * 3) : ((int)(num / 74) + 2) * 74f;
                    }
                    //apparel
                    List<Thing> unslotedApparel = new List<Thing>();
                    if (this.ShouldShowApparel(this.SelPawnForGear))
                    {
                        HashSet<int> usedSlots = new HashSet<int>();
                        foreach (Apparel current2 in this.SelPawnForGear.apparel.WornApparel)
                        {
                            ItemSlotDef slot = dict[current2.def];
                            if (slot == null)
                            {
                                unslotedApparel.Add(current2);
                            }
                            else
                            {
                                usedSlots.Add(slot.listid);
                                Rect apRect = new Rect(10f + xidx[slot.xPos] * 74f, yidx[slot.yPos] * 74f, 64f, 64f);
                                GUI.DrawTexture(apRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
                                this.DrawThingRow1(apRect, current2, false);
                            }
                        }

                        //List<ItemSlotDef> slotlist = Sandy_RPG_Settings.displayAllSlots ? slots : activeSlots;
                        List<ItemSlotDef> slotlist = activeSlots;
                        foreach (ItemSlotDef slot in slotlist)
                        {
                            if (!usedSlots.Contains(slot.listid))
                            {
                                Rect apRect = new Rect(10f + xidx[slot.xPos] * 74f, yidx[slot.yPos] * 74f, 64f, 64f);
                                GUI.DrawTexture(apRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
                                Rect tipRect = apRect.ContractedBy(12f);
                                TooltipHandler.TipRegion(apRect, slot.label);
                            }
                        }
                    }
                    num = Math.Max(num, slotPanelHeight);

                    if (unslotedEquipment.Count > 0)
                        this.DrawInventory(unslotedEquipment, "Equipment", viewRect, ref num);

                    if (unslotedApparel.Count > 0)
                        this.DrawInventory(unslotedApparel, "Apparel", viewRect, ref num);
                }
                else
                {
                    this.TryDrawMassInfo(ref num, viewRect.width);
                    this.TryDrawComfyTemperatureRange(ref num, viewRect.width);
                }
            }
            //inventory
            if (this.ShouldShowInventory(this.SelPawnForGear))
            {
                this.DrawInventory(this.SelPawnForGear.inventory.innerContainer, "Inventory", viewRect, ref num, true);
            }
            //
			if (Event.current.type == EventType.Layout)
			{
				this.scrollViewHeight = num + 30f;
			}
			Widgets.EndScrollView();
			GUI.EndGroup();
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private void DrawColonist(Rect rect, Pawn pawn)
		{
			Vector2 pos = new Vector2(rect.width, rect.height);
			GUI.DrawTexture(rect, PortraitsCache.Get(pawn, pos, PawnTextureCameraOffset, 1.18f));
		}

		private void DrawThingRow1(Rect rect, Thing thing, bool inventory = false, bool equipment = false)
		{
			QualityCategory c;
			if (thing.TryGetQuality(out c))
			{
				switch (c)
				{
					case QualityCategory.Legendary:
						{
							GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("UI/Frames/RPG_Legendary", true));
							break;
						}
					case QualityCategory.Masterwork:
						{
							GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("UI/Frames/RPG_Masterwork", true));
							break;
						}
					case QualityCategory.Excellent:
						{
							GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("UI/Frames/RPG_Excellent", true));
							break;
						}
					case QualityCategory.Good:
						{
							GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("UI/Frames/RPG_Good", true));
							break;
						}
					case QualityCategory.Normal:
						{
							GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("UI/Frames/RPG_Normal", true));
							break;
						}
					case QualityCategory.Poor:
						{
							GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("UI/Frames/RPG_Poor", true));
							break;
						}
					case QualityCategory.Awful:
						{
							GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("UI/Frames/RPG_Awful", true));
							break;
						}
				}
			}
			float mass = thing.GetStatValue(StatDefOf.Mass, true) * (float)thing.stackCount;
			string smass = mass.ToString("G") + " kg";
			string text = thing.LabelCap;
			Rect rect5 = rect.ContractedBy(2f);
			float num2 = rect5.height * ((float)thing.HitPoints / (float)thing.MaxHitPoints);
			rect5.yMin = rect5.yMax - num2;
			rect5.height = num2;
			GUI.DrawTexture(rect5, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Not_Tattered"));
			if ((float)thing.HitPoints <= ((float)thing.MaxHitPoints / 2))
			{
				Rect tattered = rect5;
				GUI.DrawTexture(tattered, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Tattered"));
			}
			if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
			{
				Rect rect1 = new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, rect.height - 8f);
				Widgets.ThingIcon(rect1, thing, 1f);
			}
			bool flag = false;
            if (Mouse.IsOver(rect))
            {
                Color oldcolor = GUI.color;
                GUI.color = Sandy_Detailed_RPG_GearTab.HighlightColor;
                GUI.DrawTexture(rect, TexUI.HighlightTex);
                Widgets.InfoCardButton(rect.x, rect.y, thing);
                if (this.CanControl && (inventory || this.CanControlColonist || (this.SelPawnForGear.Spawned && !this.SelPawnForGear.Map.IsPlayerHome)))
                {
                    Rect rect2 = new Rect(rect.xMax - 24f, rect.y, 24f, 24f);
                    bool flag2 = this.SelPawnForGear.IsQuestLodger() && !(thing is Apparel);
                    Apparel apparel;
                    bool flag3 = (apparel = (thing as Apparel)) != null && this.SelPawnForGear.apparel != null && this.SelPawnForGear.apparel.IsLocked(apparel);
                    flag = (flag2 || flag3);
                    if (Mouse.IsOver(rect2))
                    {
                        if (flag3)
                        {
                            TooltipHandler.TipRegion(rect2, "DropThingLocked".Translate());
                        }
                        else if (flag2)
                        {
                            TooltipHandler.TipRegion(rect2, "DropThingLodger".Translate());
                        }
                        else
                        {
                            TooltipHandler.TipRegion(rect2, "DropThing".Translate());
                        }
                    }
                    Color color = flag ? Color.grey : Color.white;
                    Color mouseoverColor = flag ? color : GenUI.MouseoverColor;
                    if (Widgets.ButtonImage(rect2, ContentFinder<Texture2D>.Get("UI/Buttons/Drop", true), color, mouseoverColor, !flag) && !flag)
                    {
                        SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                        this.InterfaceDrop(thing);
                    }
                }

                GUI.color = oldcolor;
            }
            else
            {
                GUI.color = Color.white;
            }
			Apparel apparel2 = thing as Apparel;
			if (apparel2 != null && this.SelPawnForGear.outfits != null && apparel2.WornByCorpse)
			{
				Rect rect3 = new Rect(rect.xMax - 20f, rect.yMax - 20f, 20f, 20f);
				GUI.DrawTexture(rect3, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Tainted_Icon", true));
				TooltipHandler.TipRegion(rect3, "WasWornByCorpse".Translate());
			}
			if (apparel2 != null && this.SelPawnForGear.outfits != null && this.SelPawnForGear.outfits.forcedHandler.IsForced(apparel2))
			{
				text += ", " + "ApparelForcedLower".Translate();
				Rect rect4 = new Rect(rect.x, rect.yMax - 20f, 20f, 20f);
				GUI.DrawTexture(rect4, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Forced_Icon", true));
				TooltipHandler.TipRegion(rect4, "ForcedApparel".Translate());
			}
			if (apparel2 != null && this.SelPawnForGear.outfits != null && RPG_ModCheck.IsRWoMActive() && ShouldDrawEnchantmentIcon(apparel2))
			{
				Rect rectM = new Rect(rect.x, rect.yMax - 40f, 20f, 20f);
				GUI.DrawTexture(rectM, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Enchanted_Icon", true));
				TooltipHandler.TipRegion(rectM, RPG_ModCheck.GetEnchantmentString(apparel2));
			}
			if (flag)
			{
				text += " (" + "ApparelLockedLower".Translate() + ")";
			}
			Text.WordWrap = true;
			string text2 = thing.DescriptionDetailed;
			string text3 = text + "\n" + text2 + "\n" + smass;
			if (thing.def.useHitPoints)
			{
				string text4 = text3;
				text3 = string.Concat(new object[]
				{
					text4,
					"\n",
					thing.HitPoints,
					" / ",
					thing.MaxHitPoints
				});
			}
			TooltipHandler.TipRegion(rect, text3);
            if (equipment)
            {

                if (this.SelPawnForGear.story.traits.HasTrait(TraitDefOf.Brawler) && thing.def.IsRangedWeapon)
                {
                    Rect rect6 = new Rect(rect.x, rect.yMax - 20f, 20f, 20f);
                    GUI.DrawTexture(rect6, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Forced_Icon", true));
                    TooltipHandler.TipRegion(rect6, "BrawlerHasRangedWeapon".Translate());
                }
                if (RPG_ModCheck.IsRWoMActive() && ShouldDrawEnchantmentIcon(thing))
                {
                    Rect rectM = new Rect(rect.x, rect.yMax - 40f, 20f, 20f);
                    GUI.DrawTexture(rectM, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Enchanted_Icon", true));
                    TooltipHandler.TipRegion(rectM, RPG_ModCheck.GetEnchantmentString(thing));
                }
            }
		}

		public void TryDrawOverallArmor1(Rect rect, StatDef stat, string label, Texture image)
		{
			float num = 0f;
			float num2 = Mathf.Clamp01(this.SelPawnForGear.GetStatValue(stat, true) / 2f);
			List<BodyPartRecord> allParts = this.SelPawnForGear.RaceProps.body.AllParts;
			List<Apparel> list = (this.SelPawnForGear.apparel == null) ? null : this.SelPawnForGear.apparel.WornApparel;
			for (int i = 0; i < allParts.Count; i++)
			{
				float num3 = 1f - num2;
				if (list != null)
				{
					for (int j = 0; j < list.Count; j++)
					{
						if (list[j].def.apparel.CoversBodyPart(allParts[i]))
						{
							float num4 = Mathf.Clamp01(list[j].GetStatValue(stat, true) / 2f);
							num3 *= 1f - num4;
						}
					}
				}
				num += allParts[i].coverageAbs * (1f - num3);
			}
			num = Mathf.Clamp(num * 2f, 0f, 2f);
			Rect rect1 = new Rect(rect.x, rect.y, 24f, 27f);
			GUI.DrawTexture(rect1, image);
			TooltipHandler.TipRegion(rect1, label);
			Rect rect2 = new Rect(rect.x + 30f, rect.y + 3f, 104f, 24f);
			Widgets.Label(rect2, string.Concat(new string[]
            {
                " ",
                num.ToStringPercent()
            }));
		}

		private void TryDrawMassInfo1(Rect rect)
		{
			if (this.SelPawnForGear.Dead || !this.ShouldShowInventory(this.SelPawnForGear))
			{
				return;
			}
			Rect rect1 = new Rect(rect.x, rect.y, 24f, 24f);
			GUI.DrawTexture(rect1, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_MassCarried_Icon", true));
			TooltipHandler.TipRegion(rect1, "SandyMassCarried".Translate());
			float num = MassUtility.GearAndInventoryMass(this.SelPawnForGear);
			float num2 = MassUtility.Capacity(this.SelPawnForGear, null);
			Rect rect2 = new Rect(rect.x + 30f, rect.y + 2f, 104f, 24f);
			Widgets.Label(rect2, "SandyMassValue".Translate(num.ToString("0.##"), num2.ToString("0.##")));
		}

		private void TryDrawComfyTemperatureRange1(Rect rect)
		{
			if (this.SelPawnForGear.Dead)
			{
				return;
			}
			Rect rect1 = new Rect(rect.x, rect.y + 26f, 24f, 24f);
			GUI.DrawTexture(rect1, ContentFinder<Texture2D>.Get("UI/Icons/Min_Temperature", true));
			TooltipHandler.TipRegion(rect1, "ComfyTemperatureRange".Translate());
			float statValue = this.SelPawnForGear.GetStatValue(StatDefOf.ComfyTemperatureMin, true);
			Rect rect2 = new Rect(rect.x + 30f, rect.y + 28f, 104f, 24f);
			Widgets.Label(rect2, string.Concat(new string[]
			{
				" ",
				statValue.ToStringTemperature("F0")
			}));

			rect1 = new Rect(rect.x, rect.y + 52f, 24f, 24f);
			GUI.DrawTexture(rect1, ContentFinder<Texture2D>.Get("UI/Icons/Max_Temperature", true));
			TooltipHandler.TipRegion(rect1, "ComfyTemperatureRange".Translate());
			float statValue2 = this.SelPawnForGear.GetStatValue(StatDefOf.ComfyTemperatureMax, true);
			rect2 = new Rect(rect.x + 30f, rect.y + 56f, 104f, 24f);
			Widgets.Label(rect2, string.Concat(new string[]
			{
				" ",
				statValue2.ToStringTemperature("F0")
			}));
		}

		private void DrawThingRow(ref float y, float width, Thing thing, bool inventory = false)
		{
			Rect rect = new Rect(0f, y, width, 28f);
			Widgets.InfoCardButton(rect.width - 24f, y, thing);
			rect.width -= 24f;
			bool flag = false;
			if (this.CanControl && (inventory || this.CanControlColonist || (this.SelPawnForGear.Spawned && !this.SelPawnForGear.Map.IsPlayerHome)))
			{
				Rect rect2 = new Rect(rect.width - 24f, y, 24f, 24f);
				bool flag2 = false;
				if (this.SelPawnForGear.IsQuestLodger())
				{
					flag2 = (inventory || !EquipmentUtility.QuestLodgerCanUnequip(thing, this.SelPawnForGear));
				}
				Apparel apparel;
				bool flag3 = (apparel = (thing as Apparel)) != null && this.SelPawnForGear.apparel != null && this.SelPawnForGear.apparel.IsLocked(apparel);
				flag = (flag2 || flag3);
				if (Mouse.IsOver(rect2))
				{
					if (flag3)
					{
						TooltipHandler.TipRegion(rect2, "DropThingLocked".Translate());
					}
					else if (flag2)
					{
						TooltipHandler.TipRegion(rect2, "DropThingLodger".Translate());
					}
					else
					{
						TooltipHandler.TipRegion(rect2, "DropThing".Translate());
					}
				}
				Color color = flag ? Color.grey : Color.white;
				Color mouseoverColor = flag ? color : GenUI.MouseoverColor;
				if (Widgets.ButtonImage(rect2, ContentFinder<Texture2D>.Get("UI/Buttons/Drop", true), color, mouseoverColor, !flag) && !flag)
				{
					SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
					this.InterfaceDrop(thing);
				}
				rect.width -= 24f;
			}
			if (this.CanControlColonist)
			{
				if (FoodUtility.WillIngestFromInventoryNow(this.SelPawnForGear, thing))
				{
					Rect rect3 = new Rect(rect.width - 24f, y, 24f, 24f);
					TooltipHandler.TipRegionByKey(rect3, "ConsumeThing", thing.LabelNoCount, thing);
					if (Widgets.ButtonImage(rect3, ContentFinder<Texture2D>.Get("UI/Buttons/Ingest", true), true))
					{
						SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
						FoodUtility.IngestFromInventoryNow(this.SelPawnForGear, thing);
					}
				}
				rect.width -= 24f;
			}
			Rect rect4 = rect;
			rect4.xMin = rect4.xMax - 60f;
			CaravanThingsTabUtility.DrawMass(thing, rect4);
			rect.width -= 60f;
			if (Mouse.IsOver(rect))
			{
				GUI.color = ITab_Pawn_Gear.HighlightColor;
				GUI.DrawTexture(rect, TexUI.HighlightTex);
			}
			if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
			{
				Widgets.ThingIcon(new Rect(4f, y, 28f, 28f), thing, 1f);
			}
			Text.Anchor = TextAnchor.MiddleLeft;
			GUI.color = ITab_Pawn_Gear.ThingLabelColor;
			Rect rect5 = new Rect(36f, y, rect.width - 36f, rect.height);
			string text = thing.LabelCap;
			Apparel apparel2 = thing as Apparel;
			if (apparel2 != null && this.SelPawnForGear.outfits != null && this.SelPawnForGear.outfits.forcedHandler.IsForced(apparel2))
			{
				text += ", " + "ApparelForcedLower".Translate();
			}
			if (flag)
			{
				text += " (" + "ApparelLockedLower".Translate() + ")";
			}
			Text.WordWrap = false;
			Widgets.Label(rect5, text.Truncate(rect5.width, null));
			Text.WordWrap = true;
			if (Mouse.IsOver(rect))
			{
				string text2 = thing.DescriptionDetailed;
				if (thing.def.useHitPoints)
				{
					text2 = string.Concat(new object[]
					{
						text2,
						"\n",
						thing.HitPoints,
						" / ",
						thing.MaxHitPoints
					});
				}
				TooltipHandler.TipRegion(rect, text2);
			}
			y += 28f;
		}

		private void TryDrawOverallArmor(ref float curY, float width, StatDef stat, string label)
		{
			float num = 0f;
			float num2 = Mathf.Clamp01(this.SelPawnForGear.GetStatValue(stat, true) / 2f);
			List<BodyPartRecord> allParts = this.SelPawnForGear.RaceProps.body.AllParts;
			List<Apparel> list = (this.SelPawnForGear.apparel == null) ? null : this.SelPawnForGear.apparel.WornApparel;
			for (int i = 0; i < allParts.Count; i++)
			{
				float num3 = 1f - num2;
				if (list != null)
				{
					for (int j = 0; j < list.Count; j++)
					{
						if (list[j].def.apparel.CoversBodyPart(allParts[i]))
						{
							float num4 = Mathf.Clamp01(list[j].GetStatValue(stat, true) / 2f);
							num3 *= 1f - num4;
						}
					}
				}
				num += allParts[i].coverageAbs * (1f - num3);
			}
			num = Mathf.Clamp(num * 2f, 0f, 2f);
			Rect rect = new Rect(0f, curY, width, 100f);
			Widgets.Label(rect, label.Truncate(120f, null));
			rect.xMin += 120f;
			Widgets.Label(rect, num.ToStringPercent());
			curY += 22f;
		}

		private void TryDrawMassInfo(ref float curY, float width)
		{
			if (this.SelPawnForGear.Dead || !this.ShouldShowInventory(this.SelPawnForGear))
			{
				return;
			}
			Rect rect = new Rect(0f, curY, width, 22f);
			float num = MassUtility.GearAndInventoryMass(this.SelPawnForGear);
			float num2 = MassUtility.Capacity(this.SelPawnForGear, null);
			Widgets.Label(rect, "MassCarried".Translate(num.ToString("0.##"), num2.ToString("0.##")));
			curY += 22f;
		}

		private void TryDrawComfyTemperatureRange(ref float curY, float width)
		{
			if (this.SelPawnForGear.Dead)
			{
				return;
			}
			Rect rect = new Rect(0f, curY, width, 22f);
			float statValue = this.SelPawnForGear.GetStatValue(StatDefOf.ComfyTemperatureMin, true);
			float statValue2 = this.SelPawnForGear.GetStatValue(StatDefOf.ComfyTemperatureMax, true);
			Widgets.Label(rect, string.Concat(new string[]
			{
				"ComfyTemperatureRange".Translate(),
				": ",
				statValue.ToStringTemperature("F0"),
				" ~ ",
				statValue2.ToStringTemperature("F0")
			}));
			curY += 22f;
		}

		private void InterfaceDrop(Thing t)
		{
			ThingWithComps thingWithComps = t as ThingWithComps;
			Apparel apparel = t as Apparel;
			if (apparel != null && this.SelPawnForGear.apparel != null && this.SelPawnForGear.apparel.WornApparel.Contains(apparel))
			{
				this.SelPawnForGear.jobs.TryTakeOrderedJob(new Job(JobDefOf.RemoveApparel, apparel), JobTag.Misc);
			}
			else if (thingWithComps != null && this.SelPawnForGear.equipment != null && this.SelPawnForGear.equipment.AllEquipmentListForReading.Contains(thingWithComps))
			{
				this.SelPawnForGear.jobs.TryTakeOrderedJob(new Job(JobDefOf.DropEquipment, thingWithComps), JobTag.Misc);
			}
			else if (!t.def.destroyOnDrop)
			{
				Thing thing;
				this.SelPawnForGear.inventory.innerContainer.TryDrop(t, this.SelPawnForGear.Position, this.SelPawnForGear.Map, ThingPlaceMode.Near, out thing, null, null);
			}
		}

		private void InterfaceIngest(Thing t)
		{
			Job job = new Job(JobDefOf.Ingest, t);
			job.count = Mathf.Min(t.stackCount, t.def.ingestible.maxNumToIngestAtOnce);
			job.count = Mathf.Min(job.count, FoodUtility.WillIngestStackCountOf(this.SelPawnForGear, t.def, t.GetStatValue(StatDefOf.Nutrition, true)));
			this.SelPawnForGear.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}

		private bool ShouldShowInventory(Pawn p)
		{
			return p.RaceProps.Humanlike || p.inventory.innerContainer.Any;
		}

		private bool ShouldShowApparel(Pawn p)
		{
			return p.apparel != null && (p.RaceProps.Humanlike || p.apparel.WornApparel.Any<Apparel>());
		}

		private bool ShouldShowEquipment(Pawn p)
		{
			return p.equipment != null;
		}

		private bool ShouldShowOverallArmor(Pawn p)
		{
			return p.RaceProps.Humanlike || this.ShouldShowApparel(p) || p.GetStatValue(StatDefOf.ArmorRating_Sharp, true) > 0f || p.GetStatValue(StatDefOf.ArmorRating_Blunt, true) > 0f || p.GetStatValue(StatDefOf.ArmorRating_Heat, true) > 0f;
		}

		private bool ShouldDrawEnchantmentIcon(Thing item)
		{
			bool isEnchanted = false;
			TorannMagic.Enchantment.CompEnchantedItem enchantedItem = item.TryGetComp<TorannMagic.Enchantment.CompEnchantedItem>();
			if (enchantedItem != null && enchantedItem.HasEnchantment)
			{
				isEnchanted = true;
			}
			return isEnchanted;
		}

        protected void DrawViewList(ref float num, Rect viewRect)
        {
            this.TryDrawMassInfo(ref num, viewRect.width);
            this.TryDrawComfyTemperatureRange(ref num, viewRect.width);
            //armor
            if (this.ShouldShowOverallArmor(this.SelPawnForGear))
            {
                Widgets.ListSeparator(ref num, viewRect.width, "OverallArmor".Translate());
                this.TryDrawOverallArmor(ref num, viewRect.width, StatDefOf.ArmorRating_Sharp, "ArmorSharp".Translate());
                this.TryDrawOverallArmor(ref num, viewRect.width, StatDefOf.ArmorRating_Blunt, "ArmorBlunt".Translate());
                this.TryDrawOverallArmor(ref num, viewRect.width, StatDefOf.ArmorRating_Heat, "ArmorHeat".Translate());
            }
            //equipment
            if (this.ShouldShowEquipment(this.SelPawnForGear))
            {
                Widgets.ListSeparator(ref num, viewRect.width, "Equipment".Translate());
                foreach (ThingWithComps thing in this.SelPawnForGear.equipment.AllEquipmentListForReading)
                {
                    this.DrawThingRow(ref num, viewRect.width, thing, false);
                }
            }
            //apparel
            if (this.ShouldShowApparel(this.SelPawnForGear))
            {
                Widgets.ListSeparator(ref num, viewRect.width, "Apparel".Translate());
                foreach (Apparel thing2 in from ap in this.SelPawnForGear.apparel.WornApparel
                                           orderby ap.def.apparel.bodyPartGroups[0].listOrder descending
                                           select ap)
                {
                    this.DrawThingRow(ref num, viewRect.width, thing2, false);
                }
            }
            //inventory
            //if (this.ShouldShowInventory(this.SelPawnForGear))
            //{
            //    this.DrawInventory(this.SelPawnForGear.inventory.innerContainer, "Inventory", viewRect, ref num, true);
            //}
        }

        protected void DrawInventory(IEnumerable<Thing> list, string title, Rect viewRect, ref float num, bool inventory = false)
        {
            Widgets.ListSeparator(ref num, viewRect.width, title.Translate());
            foreach(var item in list)
            {
                this.DrawThingRow(ref num, viewRect.width, item, inventory);
            }
        }

        private static int[] xidx = null;
        private static int[] yidx = null;
        public static Dictionary<ThingDef, ItemSlotDef> dict = null;
        public static List<ItemSlotDef> slots = null;
        public static List<ItemSlotDef> activeSlots = null;
        public static float slotPanelWidth = 530f;
        public static float slotPanelHeight = 440f;

        public static void MakePreps(bool displayAllSlots, bool reset = false)
        {
            if (Sandy_Detailed_RPG_GearTab.dict != null && !reset)
            {
                return;
            }
            //
            HashSet<int> rows = new HashSet<int>();
            HashSet<int> columns = new HashSet<int>();
            int maxrow = 4;
            int maxcolumn = 4;
            //creating basics
            dict = new Dictionary<ThingDef, ItemSlotDef>();
            activeSlots = new List<ItemSlotDef>();
            slots = DefDatabase<ItemSlotDef>.AllDefsListForReading.OrderBy(x => x.validationOrder).ToList();
            //get max values on the grid
            foreach (var slot in slots)
            {
                maxrow = Math.Max(maxrow, slot.yPos);
                maxcolumn = Math.Max(maxcolumn, slot.xPos);
            }
            //generating cache while exploring the boundaries
            foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.apparel != null))
            {
                foreach (var slot in slots)
                {
                    if (slot.Valid(def.apparel))
                    {
                        dict[def] = slot;
                        activeSlots.Add(slot);
                        slot.listid = activeSlots.Count - 1;
                        rows.Add(slot.yPos);
                        columns.Add(slot.xPos);
                        break;
                    }
                    dict[def] = null;
                }   
            }
            //offsetting boundaries
            xidx = new int[maxcolumn + 1];
            yidx = new int[maxrow + 1];

            if (displayAllSlots)
            {
                activeSlots = slots;
                for (var i = 0; i <= maxrow; i++)
                {
                    yidx[i] = i;
                }
                for (var i = 0; i <= maxcolumn; i++)
                {
                    xidx[i] = i;
                }
            }
            else
            {
                int offset = 0;
                for (var i = 0; i <= maxrow; i++)
                {
                    if (!rows.Contains(i))
                        offset--;
                    yidx[i] = i + offset;
                }
                offset = 0;
                for (var i = 0; i <= maxcolumn; i++)
                {
                    if (!columns.Contains(i))
                        offset--;
                    xidx[i] = i + offset;
                }
            }

            //resetting size values
            slotPanelWidth = 10f + (xidx[maxcolumn] + 1) * 74f;
            slotPanelHeight = (yidx[maxrow] + 1) * 74f;
        }

        protected virtual void DrawStats(ref float top, float left)
        {
            Rect rectstat = new Rect(left, top, 128f, 50f);
            this.TryDrawMassInfo1(rectstat);
            this.TryDrawComfyTemperatureRange1(rectstat);
            top += 84f;

            if (this.ShouldShowOverallArmor(this.SelPawnForGear))
            {
                Rect rectsharp = new Rect(left, top, 128f, 27f);
                this.TryDrawOverallArmor1(rectsharp, StatDefOf.ArmorRating_Sharp, "ArmorSharp".Translate(),
                                         ContentFinder<Texture2D>.Get("UI/Icons/Sandy_ArmorSharp_Icon", true));
                top += 30f;
                Rect rectblunt = new Rect(left, top, 128f, 27f);
                this.TryDrawOverallArmor1(rectblunt, StatDefOf.ArmorRating_Blunt, "ArmorBlunt".Translate(),
                                         ContentFinder<Texture2D>.Get("UI/Icons/Sandy_ArmorBlunt_Icon", true));
                top += 30f;
                Rect rectheat = new Rect(left, top, 128f, 27f);
                this.TryDrawOverallArmor1(rectheat, StatDefOf.ArmorRating_Heat, "ArmorHeat".Translate(),
                                         ContentFinder<Texture2D>.Get("UI/Icons/Sandy_ArmorHeat_Icon", true));
                top += 30f;
                if (RPG_ModCheck.IsRWoMActive())
                {
                    Rect rectharmony = new Rect(left, top, 128f, 27f);
                    TryDrawOverallArmor1(rectharmony, RPG_ModCheck.GetHarmonyStatDef(), "RPG_Style_Inventory_ArmorHarmony".Translate(),
                                         ContentFinder<Texture2D>.Get("UI/Icons/Sandy_ArmorHarmony_Icon", true));
                    top += 30f;
                }

            }
        }
    }
}