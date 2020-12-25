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
	public class Sandy_Detailed_RPG_GearTab : ITab_Pawn_Gear
	{
		private Vector2 scrollPosition = Vector2.zero;

		private float scrollViewHeight;

		private const float TopPadding = 20f;

		public static readonly Color ThingLabelColor = new Color(0.9f, 0.9f, 0.9f, 1f);

		public static readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);

		private const float ThingIconSize = 28f;

		private const float ThingRowHeight = 28f;

		private const float ThingLeftX = 36f;

		private const float StandardLineHeight = 22f;

		private const float RPG_IconGap = 70f;

		private const float RPG_IconSize = 60f;

		private static List<Thing> workingInvList = new List<Thing>();

		public static readonly Vector3 PawnTextureCameraOffset = new Vector3(0f, 0f, 0f);

		private bool viewList = false;

		public Sandy_Detailed_RPG_GearTab()
		{
			this.size = new Vector2(Sandy_RPG_Settings.rpgTabWidth, Sandy_RPG_Settings.rpgTabHeight);
			this.labelKey = "TabGear";
			this.tutorTag = "Gear";
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
			Rect checkBox = new Rect(20f, 0f, 100f, 30f);
			Widgets.CheckboxLabeled(checkBox, "Sandy_ViewList".Translate(), ref viewList, false, null, null, false);
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
			float inventory_x = 130f;
			float apparel_x = 130f;
			float apparel_y = 0f;
			int Num_apparels = 0;

			if (!viewList)
			{
				if (this.SelPawnForGear.RaceProps.Humanlike)
				{
					Rect rectMass = new Rect(0f, 0f, 100f, 24f);
					this.TryDrawMassInfo1(rectMass);
					Rect rectTemp = new Rect(0f, 26f, 100f, 50f);
					this.TryDrawComfyTemperatureRange1(rectTemp);
				}
				else
				{
					this.TryDrawMassInfo(ref num, viewRect.width);
					this.TryDrawComfyTemperatureRange(ref num, viewRect.width);
				}
			}
			else if (viewList)
			{
				this.TryDrawMassInfo(ref num, viewRect.width);
				this.TryDrawComfyTemperatureRange(ref num, viewRect.width);
			}
			if (this.ShouldShowOverallArmor(this.SelPawnForGear) && !viewList && this.SelPawnForGear.RaceProps.Humanlike)
			{
				Rect rectarmor = new Rect(0f, 78f, 100f, 117f);
				TooltipHandler.TipRegion(rectarmor, "OverallArmor".Translate());
				Rect rectsharp = new Rect(rectarmor.x, rectarmor.y, rectarmor.width, 27f);
				this.TryDrawOverallArmor1(rectsharp, StatDefOf.ArmorRating_Sharp, "ArmorSharp".Translate(),
										 ContentFinder<Texture2D>.Get("UI/Icons/Sandy_ArmorSharp_Icon", true));
				Rect rectblunt = new Rect(rectarmor.x, rectarmor.y + 30f, rectarmor.width, 27f);
				this.TryDrawOverallArmor1(rectblunt, StatDefOf.ArmorRating_Blunt, "ArmorBlunt".Translate(),
										 ContentFinder<Texture2D>.Get("UI/Icons/Sandy_ArmorBlunt_Icon", true));
				Rect rectheat = new Rect(rectarmor.x, rectarmor.y + 60f, rectarmor.width, 27f);
				this.TryDrawOverallArmor1(rectheat, StatDefOf.ArmorRating_Heat, "ArmorHeat".Translate(),
										 ContentFinder<Texture2D>.Get("UI/Icons/Sandy_ArmorHeat_Icon", true));
				if (RPG_ModCheck.IsRWoMActive())
				{
					Rect rectharmony = new Rect(rectarmor.x, rectarmor.y + 90f, rectarmor.width, 27f);
					TryDrawOverallArmor1(rectharmony, RPG_ModCheck.GetHarmonyStatDef(), "RPG_Style_Inventory_ArmorHarmony".Translate(),
										 ContentFinder<Texture2D>.Get("UI/Icons/Sandy_ArmorHarmony_Icon", true));
				}
			}
			else if (this.ShouldShowOverallArmor(this.SelPawnForGear))
			{
				Widgets.ListSeparator(ref num, viewRect.width, "OverallArmor".Translate());
				this.TryDrawOverallArmor(ref num, viewRect.width, StatDefOf.ArmorRating_Sharp, "ArmorSharp".Translate());
				this.TryDrawOverallArmor(ref num, viewRect.width, StatDefOf.ArmorRating_Blunt, "ArmorBlunt".Translate());
				this.TryDrawOverallArmor(ref num, viewRect.width, StatDefOf.ArmorRating_Heat, "ArmorHeat".Translate());
			}
			if (this.IsVisible && this.SelPawnForGear.RaceProps.Humanlike && !viewList)
			{
				Color color = new Color(1f, 1f, 1f, 1f);
				GUI.color = color;
				//Pawn
				Rect PawnRect = new Rect(0f, 200f, 100f, 100f);
				this.DrawColonist(PawnRect, this.SelPawnForGear);
			}
			if (this.ShouldShowEquipment(this.SelPawnForGear) && !viewList && this.SelPawnForGear.RaceProps.Humanlike)
			{
				foreach (ThingWithComps RPG_thing in this.SelPawnForGear.equipment.AllEquipmentListForReading)
				{
					if (RPG_thing == this.SelPawnForGear.equipment.Primary)
					{
						Rect newRect = new Rect(20f, 310f, RPG_IconSize, RPG_IconSize);
						GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
						this.DrawThingEquipment(newRect, RPG_thing, false);
						if (this.SelPawnForGear.story.traits.HasTrait(TraitDefOf.Brawler) && this.SelPawnForGear.equipment.Primary != null && this.SelPawnForGear.equipment.Primary.def.IsRangedWeapon)
						{
							Rect rect6 = new Rect(newRect.x, newRect.yMax - 20f, 20f, 20f);
							GUI.DrawTexture(rect6, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Forced_Icon", true));
							TooltipHandler.TipRegion(rect6, "BrawlerHasRangedWeapon".Translate());
						}
						if (RPG_ModCheck.IsRWoMActive() && ShouldDrawEnchantmentIcon(RPG_thing))
						{
							Rect rectM = new Rect(newRect.x, newRect.yMax - 40f, 20f, 20f);
							GUI.DrawTexture(rectM, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Enchanted_Icon", true));
							TooltipHandler.TipRegion(rectM, RPG_ModCheck.GetEnchantmentString(RPG_thing));
						}
					}
					if (RPG_thing != this.SelPawnForGear.equipment.Primary)
					{
						Rect newRect1 = new Rect(20f, 380f, RPG_IconSize, RPG_IconSize);
						GUI.DrawTexture(newRect1, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
						this.DrawThingEquipment(newRect1, RPG_thing, false);
						if (this.SelPawnForGear.story.traits.HasTrait(TraitDefOf.Brawler) && this.SelPawnForGear.equipment.Primary != null && RPG_thing.def.IsRangedWeapon)
						{
							Rect rect6 = new Rect(newRect1.x, newRect1.yMax - 20f, 20f, 20f);
							GUI.DrawTexture(rect6, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Forced_Icon", true));
							TooltipHandler.TipRegion(rect6, "BrawlerHasRangedWeapon".Translate());
						}
						if (RPG_ModCheck.IsRWoMActive() && ShouldDrawEnchantmentIcon(RPG_thing))
						{
							Rect rectM = new Rect(newRect1.x, newRect1.yMax - 40f, 20f, 20f);
							GUI.DrawTexture(rectM, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Enchanted_Icon", true));
							TooltipHandler.TipRegion(rectM, RPG_ModCheck.GetEnchantmentString(RPG_thing));
						}
					}
				}
			}
			else if (this.ShouldShowEquipment(this.SelPawnForGear))
			{
				Widgets.ListSeparator(ref num, viewRect.width, "Equipment".Translate());
				foreach (ThingWithComps thing in this.SelPawnForGear.equipment.AllEquipmentListForReading)
				{
					this.DrawThingRow(ref num, viewRect.width, thing, false);
				}
			}
			if (this.ShouldShowApparel(this.SelPawnForGear) && !viewList && this.SelPawnForGear.RaceProps.Humanlike)
			{
				/*bool RWoMIsActive = RPG_ModCheck.IsRWoMActive();
				bool hasCloak = false;
				bool hasCape = false;
				bool hasArtifact_Neck = false;
				bool hasNeckAccessory = false;
				bool hasArtifact_LeftHand = false;
				bool hasLeftHandSkin = false;
				bool hasArtifact_RightHand = false;
				bool hasRightHandSkin = false;
				bool hasArtifact_Arms = false;
				bool hasArmsShell = false;
				int artifactCount = 0;
				if (RWoMIsActive)
				{
					hasCloak = RPG_ModCheck.HasCloak(this.SelPawnForGear);
					hasCape = RPG_ModCheck.HasCape(this.SelPawnForGear);
					hasArtifact_Neck = RPG_ModCheck.HasArtifact_Neck(this.SelPawnForGear);
					hasNeckAccessory = RPG_ModCheck.HasAccessory_Neck(this.SelPawnForGear);
					hasArtifact_LeftHand = RPG_ModCheck.HasArtifact_LeftHand(this.SelPawnForGear);
					hasLeftHandSkin = RPG_ModCheck.HasApparel_LeftHand(this.SelPawnForGear);
					hasArtifact_RightHand = RPG_ModCheck.HasArtifact_RightHand(this.SelPawnForGear);
					hasRightHandSkin = RPG_ModCheck.HasApparel_RightHand(this.SelPawnForGear);
					hasArtifact_Arms = RPG_ModCheck.HasArtifact_Arms(this.SelPawnForGear);
					hasArmsShell = RPG_ModCheck.HasApparel_Arms(this.SelPawnForGear);
				}*/

				foreach (Apparel RPG_thing2 in Enumerable.OrderByDescending<Apparel, int>(this.SelPawnForGear.apparel.WornApparel,
					(Apparel ap) => ap.def.apparel.bodyPartGroups[0].listOrder))
				{
					this.DrawThingApparel(ref apparel_x, ref apparel_y, ref num, RPG_thing2, ref Num_apparels, false);
				}
			}
			else if (this.ShouldShowApparel(this.SelPawnForGear))
			{
				Widgets.ListSeparator(ref num, viewRect.width, "Apparel".Translate());
				/*foreach (Apparel thing2 in from ap in this.SelPawnForGear.apparel.WornApparel
					   orderby ap.def.apparel.bodyPartGroups[0].listOrder descending select ap)*/
				foreach (Apparel thing2 in Enumerable.OrderByDescending<Apparel, int>(this.SelPawnForGear.apparel.WornApparel,
					(Apparel ap) => ap.def.apparel.bodyPartGroups[0].listOrder))
				{
					this.DrawThingRow(ref num, viewRect.width, thing2, false);
				}
			}
			if (this.ShouldShowInventory(this.SelPawnForGear))
			{
				if (!viewList && this.SelPawnForGear.RaceProps.Humanlike)
				{
					Rect invRect = new Rect(inventory_x, num, 420f, 20f);
					Widgets.Label(invRect, "Inventory".Translate());
					num += 20f;
					Widgets.DrawLineHorizontal(125f, num, 430f);
					num += 2f;
					Sandy_Detailed_RPG_GearTab.workingInvList.Clear();
					Sandy_Detailed_RPG_GearTab.workingInvList.AddRange(this.SelPawnForGear.inventory.innerContainer);
					for (int i = 0; i < Sandy_Detailed_RPG_GearTab.workingInvList.Count; i++)
					{
						this.DrawThingInventory(ref inventory_x, ref num, invRect.width, Sandy_Detailed_RPG_GearTab.workingInvList[i], true);
					}
					Sandy_Detailed_RPG_GearTab.workingInvList.Clear();
				}
				else
				{
					Widgets.ListSeparator(ref num, viewRect.width, "Inventory".Translate());
					Sandy_Detailed_RPG_GearTab.workingInvList.Clear();
					Sandy_Detailed_RPG_GearTab.workingInvList.AddRange(this.SelPawnForGear.inventory.innerContainer);
					for (int i = 0; i < Sandy_Detailed_RPG_GearTab.workingInvList.Count; i++)
					{
						this.DrawThingRow(ref num, viewRect.width, Sandy_Detailed_RPG_GearTab.workingInvList[i], true);
					}
					Sandy_Detailed_RPG_GearTab.workingInvList.Clear();
				}
			}
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

		private void DrawThingEquipment(Rect rect, Thing thing, bool inventory = false)
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
				GUI.color = Sandy_Detailed_RPG_GearTab.HighlightColor;
				GUI.DrawTexture(rect, TexUI.HighlightTex);
				Widgets.InfoCardButton(rect.x, rect.y, thing);
				if (this.CanControl && (inventory || this.CanControlColonist || (this.SelPawnForGear.Spawned && !this.SelPawnForGear.Map.IsPlayerHome)))
				{
					Rect rect2 = new Rect(rect.xMax - 20f, rect.y, 20f, 20f);
					bool flag2;
					if (this.SelPawnForGear.IsQuestLodger())
					{
						if (inventory)
						{
							flag2 = true;
						}
						else
						{
							CompBiocodableWeapon compBiocodableWeapon = thing.TryGetComp<CompBiocodableWeapon>();
							if (compBiocodableWeapon != null && compBiocodableWeapon.Biocoded)
							{
								flag2 = true;
							}
							else
							{
								CompBladelinkWeapon compBladelinkWeapon = thing.TryGetComp<CompBladelinkWeapon>();
								flag2 = (compBladelinkWeapon != null && compBladelinkWeapon.bondedPawn == this.SelPawnForGear);
							}
						}
					}
					else
					{
						flag2 = false;
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
				}
			}
			Apparel apparel2 = thing as Apparel;
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
		}

		private void DrawThingApparel(ref float x, ref float y, ref float num, Thing thing, ref int Num_apparels, bool inventory = false)
		{
			Rect rect = new Rect(x, y, RPG_IconSize, RPG_IconSize);
			num = y + RPG_IconGap;
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
				GUI.color = Sandy_Detailed_RPG_GearTab.HighlightColor;
				GUI.DrawTexture(rect, TexUI.HighlightTex);
				Widgets.InfoCardButton(rect.x, rect.y, thing);
				if (this.CanControl && (inventory || this.CanControlColonist || (this.SelPawnForGear.Spawned && !this.SelPawnForGear.Map.IsPlayerHome)))
				{
					Rect rect2 = new Rect(rect.xMax - 20f, rect.y, 20f, 20f);
					bool flag2;
					if (this.SelPawnForGear.IsQuestLodger())
					{
						if (inventory)
						{
							flag2 = true;
						}
						else
						{
							CompBiocodableWeapon compBiocodableWeapon = thing.TryGetComp<CompBiocodableWeapon>();
							if (compBiocodableWeapon != null && compBiocodableWeapon.Biocoded)
							{
								flag2 = true;
							}
							else
							{
								CompBladelinkWeapon compBladelinkWeapon = thing.TryGetComp<CompBladelinkWeapon>();
								flag2 = (compBladelinkWeapon != null && compBladelinkWeapon.bondedPawn == this.SelPawnForGear);
							}
						}
					}
					else
					{
						flag2 = false;
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
				}
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
			Num_apparels++;
			if (Num_apparels < 6)
            {
				x += RPG_IconGap;
            }
			else if(Num_apparels == 6)
            {
				x = 130f;
				y += RPG_IconGap;
				Num_apparels = 0;
            }
		}

		private void DrawThingInventory(ref float x, ref float y, float width, Thing thing, bool inventory = false)
		{
			Rect rect = new Rect(x, y, width, 28f);
			Rect rect1 = new Rect(rect.x, rect.y, rect.width, rect.height);
			Widgets.InfoCardButton(rect1.width - 24f, y + 2f, thing);
			//rect1.width -= 24f;
			bool flag = false;
			if (this.CanControl && (inventory || this.CanControlColonist || (this.SelPawnForGear.Spawned && !this.SelPawnForGear.Map.IsPlayerHome)))
			{
				Rect rect2 = new Rect(rect1.width - 48f, y + 2f, 24f, 24f);
				bool flag2;
				if (this.SelPawnForGear.IsQuestLodger())
				{
					if (inventory)
					{
						flag2 = true;
					}
					else
					{
						CompBiocodableWeapon compBiocodableWeapon = thing.TryGetComp<CompBiocodableWeapon>();
						if (compBiocodableWeapon != null && compBiocodableWeapon.Biocoded)
						{
							flag2 = true;
						}
						else
						{
							CompBladelinkWeapon compBladelinkWeapon = thing.TryGetComp<CompBladelinkWeapon>();
							flag2 = (compBladelinkWeapon != null && compBladelinkWeapon.bondedPawn == this.SelPawnForGear);
						}
					}
				}
				else
				{
					flag2 = false;
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
				//rect1.width -= 24f;
			}
			if (this.CanControlColonist)
			{
				if ((thing.def.IsNutritionGivingIngestible || thing.def.IsNonMedicalDrug) && thing.IngestibleNow && base.SelPawn.WillEat(thing, null, true))
				{
					Rect rect3 = new Rect(rect1.width - 72f, y + 2f, 24f, 24f);
					TooltipHandler.TipRegionByKey(rect3, "ConsumeThing", thing.LabelNoCount, thing);
					if (Widgets.ButtonImage(rect3, ContentFinder<Texture2D>.Get("UI/Buttons/Ingest", true), true))
					{
						SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
						this.InterfaceIngest(thing);
					}
				}
				//rect1.width -= 24f;
			}
			Rect rect4 = rect1;
			rect4.xMin = rect4.xMax - 60f;
			CaravanThingsTabUtility.DrawMass(thing, rect4);
			//rect1.width -= 60f;
			if (Mouse.IsOver(rect))
			{
				GUI.color = ITab_Pawn_Gear.HighlightColor;
				GUI.DrawTexture(rect, TexUI.HighlightTex);
			}
			if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
			{
				Widgets.ThingIcon(new Rect(x, y, 28f, 28f), thing, 1f);
			}
			Text.Anchor = TextAnchor.MiddleLeft;
			GUI.color = ITab_Pawn_Gear.ThingLabelColor;
			Rect rect5 = new Rect(x + 36f, y + 2f, rect1.width - 162f, rect1.height);
			string text = thing.LabelCap;
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
			Rect rect2 = new Rect(rect.x + 40f, rect.y + 3f, 76f, 27f);
			Widgets.Label(rect2, num.ToStringPercent());
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
			Rect rect2 = new Rect(rect.x + 30f, rect.y + 2f, 100f, 24f);
			Widgets.Label(rect2, "SandyMassValue".Translate(num.ToString("0.##"), num2.ToString("0.##")));
		}

		private void TryDrawComfyTemperatureRange1(Rect rect)
		{
			if (this.SelPawnForGear.Dead)
			{
				return;
			}
			Rect rect1 = new Rect(rect.x, rect.y, 24f, 24f);
			GUI.DrawTexture(rect1, ContentFinder<Texture2D>.Get("UI/Icons/Min_Temperature", true));
			TooltipHandler.TipRegion(rect1, "ComfyTemperatureRange".Translate());
			float statValue = this.SelPawnForGear.GetStatValue(StatDefOf.ComfyTemperatureMin, true);
			Rect rect2 = new Rect(rect.x + 30f, rect.y + 2f, 76f, 24f);
			Widgets.Label(rect2, string.Concat(new string[]
			{
				" ",
				statValue.ToStringTemperature("F0")
			}));

			rect1 = new Rect(rect.x, rect.y + 26f, 24f, 24f);
			GUI.DrawTexture(rect1, ContentFinder<Texture2D>.Get("UI/Icons/Max_Temperature", true));
			TooltipHandler.TipRegion(rect1, "ComfyTemperatureRange".Translate());
			float statValue2 = this.SelPawnForGear.GetStatValue(StatDefOf.ComfyTemperatureMax, true);
			rect2 = new Rect(rect.x + 30f, rect.y + 28f, 76f, 24f);
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
				bool flag2;
				if (this.SelPawnForGear.IsQuestLodger())
				{
					if (inventory)
					{
						flag2 = true;
					}
					else
					{
						CompBiocodableWeapon compBiocodableWeapon = thing.TryGetComp<CompBiocodableWeapon>();
						if (compBiocodableWeapon != null && compBiocodableWeapon.Biocoded)
						{
							flag2 = true;
						}
						else
						{
							CompBladelinkWeapon compBladelinkWeapon = thing.TryGetComp<CompBladelinkWeapon>();
							flag2 = (compBladelinkWeapon != null && compBladelinkWeapon.bondedPawn == this.SelPawnForGear);
						}
					}
				}
				else
				{
					flag2 = false;
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
				if ((thing.def.IsNutritionGivingIngestible || thing.def.IsNonMedicalDrug) && thing.IngestibleNow && base.SelPawn.WillEat(thing, null, true))
				{
					Rect rect3 = new Rect(rect.width - 24f, y, 24f, 24f);
					TooltipHandler.TipRegionByKey(rect3, "ConsumeThing", thing.LabelNoCount, thing);
					if (Widgets.ButtonImage(rect3, ContentFinder<Texture2D>.Get("UI/Buttons/Ingest", true), true))
					{
						SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
						this.InterfaceIngest(thing);
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
			Widgets.Label(rect, "ComfyTemperatureRange".Translate() + ": " + statValue.ToStringTemperature("F0") + " ~ "
				+ statValue2.ToStringTemperature("F0"));
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

		private float GetArtifactY(int num)
		{
			return (378f + (63f * num));
		}
	}
}