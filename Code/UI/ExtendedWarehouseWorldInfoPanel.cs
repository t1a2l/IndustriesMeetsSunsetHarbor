using System;
using ColossalFramework.UI;
using System.Collections.Generic;
using UnityEngine;
using ColossalFramework;
using ICities;
using System.Collections;
using ColossalFramework.Globalization;
using MoreTransferReasons;
using IndustriesMeetsSunsetHarbor.AI;
using IndustriesMeetsSunsetHarbor.Utils;
using IndustriesMeetsSunsetHarbor.Managers;

namespace IndustriesMeetsSunsetHarbor.UI
{
    public class ExtendedWarehouseWorldInfoPanel : BuildingWorldInfoPanel
    {
        private enum WarehouseMode
        {
            Balanced,
            Import,
            Export
        }

        private UIPanel m_ActionPanel;

        private UIButton m_RebuildButton;

        private UILabel m_Type;

        private UILabel m_Upkeep;

        private UILabel m_Info;

        private UISprite m_Thumbnail;

        private UILabel m_BuildingDesc;

        private UICheckBox m_OnOff;

        private bool m_IsRelocating;

        private UIDropDown m_dropdownResource;

        private UIDropDown m_dropdownMode;

        private UIProgressBar m_resourceProgressBar;

        private UIPanel m_resourcePanel;

        private UILabel m_resourceLabel;

        private UIPanel m_emptyingOldResource;

        private UIPanel m_buffer;

        private UILabel m_capacityLabel;

        private UILabel m_Status;

        private UIPanel m_workersTooltip;

        private UILabel m_workersInfoLabel;

        private UILabel m_UneducatedPlaces;

        private UILabel m_EducatedPlaces;

        private UILabel m_WellEducatedPlaces;

        private UILabel m_HighlyEducatedPlaces;

        private UILabel m_UneducatedWorkers;

        private UILabel m_EducatedWorkers;

        private UILabel m_WellEducatedWorkers;

        private UILabel m_HighlyEducatedWorkers;

        private UILabel m_OverWorkSituation;

        private UILabel m_JobsAvailLegend;

        private UIRadialChart m_WorkPlacesEducationChart;

        private UIRadialChart m_WorkersEducationChart;

        public Color32 m_UneducatedColor;

        public Color32 m_EducatedColor;

        public Color32 m_WellEducatedColor;

        public Color32 m_HighlyEducatedColor;

        public Color32 m_UnoccupiedWorkplaceColor;

        public float m_WorkersColorScalar = 0.5f;

        private float m_originalHeight;

        private UIComponent m_MovingPanel;

        private UILabel m_resourceDescription;

        private UISprite m_resourceSprite;

        private TransferManager.TransferReason[] m_transferReasons;

        private ExtendedTransferManager.TransferReason[] m_extendedTransferReasons;

        private WarehouseMode[] m_warehouseModes = new WarehouseMode[3]
        {
                WarehouseMode.Balanced,
                WarehouseMode.Import,
                WarehouseMode.Export
        };

        public UIComponent movingPanel
        {
            get
            {
                if (m_MovingPanel == null)
                {
                    m_MovingPanel = UIView.Find("MovingPanel");
                    m_MovingPanel.Find<UIButton>("Close").eventClick += OnMovingPanelCloseClicked;
                    m_MovingPanel.Hide();
                }
                return m_MovingPanel;
            }
        }

        public bool isCityServiceEnabled
        {
            get
            {
                if (Singleton<BuildingManager>.exists && m_InstanceID.Building != 0)
                {
                    return Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_productionRate != 0;
                }
                return false;
            }
            set
            {
                if (Singleton<SimulationManager>.exists && m_InstanceID.Building != 0)
                {
                    Singleton<SimulationManager>.instance.AddAction(ToggleBuilding(m_InstanceID.Building, value));
                }
            }
        }

        private WarehouseMode warehouseMode
        {
            get
            {
                if ((Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_flags & Building.Flags.Filling) == 0 && (Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_flags & Building.Flags.Downgrading) == 0)
                {
                    return WarehouseMode.Balanced;
                }
                if ((Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_flags & Building.Flags.Downgrading) != 0)
                {
                    return WarehouseMode.Export;
                }
                return WarehouseMode.Import;
            }
            set
            {
                switch (value)
                {
                    case WarehouseMode.Balanced:
                        Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI.SetEmptying(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building], emptying: false);
                        Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI.SetFilling(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building], filling: false);
                        break;
                    case WarehouseMode.Export:
                        Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI.SetEmptying(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building], emptying: true);
                        Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI.SetFilling(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building], filling: false);
                        break;
                    case WarehouseMode.Import:
                        Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI.SetEmptying(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building], emptying: false);
                        Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI.SetFilling(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building], filling: true);
                        break;
                }
            }
        }

        protected override void Start()
        {
            base.Start();
            List<TransferManager.TransferReason> list = new List<TransferManager.TransferReason>();
            list.Add(TransferManager.TransferReason.None);
            list.Add(TransferManager.TransferReason.AnimalProducts);
            list.Add(TransferManager.TransferReason.Flours);
            list.Add(TransferManager.TransferReason.Paper);
            list.Add(TransferManager.TransferReason.PlanedTimber);
            list.Add(TransferManager.TransferReason.Petroleum);
            list.Add(TransferManager.TransferReason.Plastics);
            list.Add(TransferManager.TransferReason.Glass);
            list.Add(TransferManager.TransferReason.Metals);
            list.Add(TransferManager.TransferReason.LuxuryProducts);
            list.Add(TransferManager.TransferReason.Lumber);
            list.Add(TransferManager.TransferReason.Food);
            list.Add(TransferManager.TransferReason.Coal);
            list.Add(TransferManager.TransferReason.Petrol);
            list.Add(TransferManager.TransferReason.Goods);
            List<TransferManager.TransferReason> list2 = list;
            if (Singleton<LoadingManager>.instance.SupportsExpansion(Expansion.Urban))
            {
                list2.Add(TransferManager.TransferReason.Fish);
            }
            m_transferReasons = list2.ToArray();
            List<ExtendedTransferManager.TransferReason> extendedlist = new List<ExtendedTransferManager.TransferReason>();
            extendedlist.Add(ExtendedTransferManager.TransferReason.Bread);
            extendedlist.Add(ExtendedTransferManager.TransferReason.DrinkSupplies);
            extendedlist.Add(ExtendedTransferManager.TransferReason.FoodSupplies);
            extendedlist.Add(ExtendedTransferManager.TransferReason.CannedFish);
            List<ExtendedTransferManager.TransferReason> extendedlist2 = extendedlist;
            m_extendedTransferReasons = extendedlist2.ToArray();
            m_Type = Find<UILabel>("Type");
            m_Upkeep = Find<UILabel>("Upkeep");
            m_Info = Find<UILabel>("Info");
            m_Thumbnail = Find<UISprite>("Thumbnail");
            m_BuildingDesc = Find<UILabel>("Desc");
            m_ActionPanel = Find<UIPanel>("ActionPanel");
            m_RebuildButton = Find<UIButton>("RebuildButton");
            m_RebuildButton.isVisible = false;
            m_OnOff = Find<UICheckBox>("On/Off");
            m_OnOff.eventCheckChanged += OnOnOffChanged;
            m_dropdownResource = Find<UIDropDown>("Dropdown");
            m_dropdownMode = Find<UIDropDown>("DropdownMode");
            RefreshDropdownLists();
            m_dropdownResource.eventSelectedIndexChanged += OnDropdownResourceChanged;
            m_dropdownMode.eventSelectedIndexChanged += OnDropdownModeChanged;
            m_resourceProgressBar = Find<UIProgressBar>("ResourceProgessBar");
            m_RebuildButton = Find<UIButton>("RebuildButton");
            m_resourcePanel = Find<UIPanel>("ResourcePanel");
            m_resourceLabel = Find<UILabel>("ResourceLabel");
            m_emptyingOldResource = Find<UIPanel>("EmptyingOldResource");
            m_buffer = Find<UIPanel>("Buffer");
            m_capacityLabel = Find<UILabel>("MaxCapacityLabel");
            m_Status = Find<UILabel>("Status");
            m_workersTooltip = Find<UIPanel>("WorkersTooltip");
            m_workersTooltip.Hide();
            m_workersInfoLabel = Find<UILabel>("TotalWorkerInfo");
            m_OverWorkSituation = Find<UILabel>("OverWorkSituation");
            m_UneducatedPlaces = Find<UILabel>("UneducatedPlaces");
            m_UneducatedWorkers = Find<UILabel>("UneducatedWorkers");
            m_EducatedPlaces = Find<UILabel>("EducatedPlaces");
            m_EducatedWorkers = Find<UILabel>("EducatedWorkers");
            m_WellEducatedPlaces = Find<UILabel>("WellEducatedPlaces");
            m_WellEducatedWorkers = Find<UILabel>("WellEducatedWorkers");
            m_HighlyEducatedPlaces = Find<UILabel>("HighlyEducatedPlaces");
            m_HighlyEducatedWorkers = Find<UILabel>("HighlyEducatedWorkers");
            m_JobsAvailLegend = Find<UILabel>("JobsAvailAmount");
            m_JobsAvailLegend.color = m_UnoccupiedWorkplaceColor;
            m_WorkPlacesEducationChart = Find<UIRadialChart>("WorkPlacesEducationChart");
            m_WorkersEducationChart = Find<UIRadialChart>("WorkersEducationChart");
            Color[] array = new Color[4] { m_UneducatedColor, m_EducatedColor, m_WellEducatedColor, m_HighlyEducatedColor };
            Color32 color;
            for (int i = 0; i < 4; i++)
            {
                UIRadialChart.SliceSettings slice = m_WorkPlacesEducationChart.GetSlice(i);
                color = array[i];
                m_WorkPlacesEducationChart.GetSlice(i).outterColor = color;
                slice.innerColor = color;
                UIRadialChart.SliceSettings slice2 = m_WorkersEducationChart.GetSlice(i);
                color = MultiplyColor(array[i], m_WorkersColorScalar);
                m_WorkersEducationChart.GetSlice(i).outterColor = color;
                slice2.innerColor = color;
            }
            UIRadialChart.SliceSettings slice3 = m_WorkersEducationChart.GetSlice(4);
            color = m_UnoccupiedWorkplaceColor;
            m_WorkersEducationChart.GetSlice(4).outterColor = color;
            slice3.innerColor = color;
            m_UneducatedPlaces.color = m_UneducatedColor;
            m_EducatedPlaces.color = m_EducatedColor;
            m_WellEducatedPlaces.color = m_WellEducatedColor;
            m_HighlyEducatedPlaces.color = m_HighlyEducatedColor;
            m_UneducatedWorkers.color = MultiplyColor(m_UneducatedColor, m_WorkersColorScalar);
            m_EducatedWorkers.color = MultiplyColor(m_EducatedColor, m_WorkersColorScalar);
            m_WellEducatedWorkers.color = MultiplyColor(m_WellEducatedColor, m_WorkersColorScalar);
            m_HighlyEducatedWorkers.color = MultiplyColor(m_HighlyEducatedColor, m_WorkersColorScalar);
            m_originalHeight = base.component.height;
            m_resourceDescription = Find<UILabel>("ResourceDescription");
            m_resourceSprite = Find<UISprite>("ResourceIcon");
        }

        private void OnDropdownModeChanged(UIComponent component, int index)
        {
            warehouseMode = m_warehouseModes[index];
        }

        private void OnDropdownResourceChanged(UIComponent component, int index)
        {
            ExtendedWarehouseAI ai = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI as ExtendedWarehouseAI;
            Singleton<SimulationManager>.instance.AddAction(delegate
            {
                if (index < m_transferReasons.Length)
                {
                    ai.SetTransferReason(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building], m_transferReasons[index]);
                }
                else
                {
                    ai.SetExtendedTransferReason(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building], m_extendedTransferReasons[index - m_transferReasons.Length]);
                }
            });
        }

        private void RefreshDropdownLists()
        {
            string[] array = new string[m_transferReasons.Length + m_extendedTransferReasons.Length];
            for (int i = 0; i < m_transferReasons.Length; i++)
            {
                string text = array[i] = Locale.Get("WAREHOUSEPANEL_RESOURCE", m_transferReasons[i].ToString());
            }
            for (int j = m_transferReasons.Length; j < array.Length; j++)
            {
                string text = (array[j] = m_extendedTransferReasons[j - m_transferReasons.Length].ToString());
            }
            m_dropdownResource.items = array;
            array = new string[m_warehouseModes.Length];
            for (int k = 0; k < m_warehouseModes.Length; k++)
            {
                string text2 = array[k] = Locale.Get("WAREHOUSEPANEL_MODE", m_warehouseModes[k].ToString());
            }
            m_dropdownMode.items = array;
        }

        protected override void OnSetTarget()
        {
            base.OnSetTarget();
            ExtendedWarehouseAI extendedWarehouseAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI as ExtendedWarehouseAI;
            m_resourcePanel.isVisible = (extendedWarehouseAI.m_storageType == TransferManager.TransferReason.None && extendedWarehouseAI.m_extendedStorageType == ExtendedTransferManager.TransferReason.None);
            base.component.height = ((!m_resourcePanel.isVisible) ? (m_originalHeight - m_resourcePanel.height) : m_originalHeight);
            if (m_resourcePanel.isVisible)
            {
                int num = 0;
                TransferManager.TransferReason[] transferReasons = m_transferReasons;
                ExtendedTransferManager.TransferReason[] extendedTransferReasons2 = m_extendedTransferReasons;
                var material_byte = extendedWarehouseAI.GetTransferReason(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building]);
                if(material_byte < 200 || material_byte == 255)
                {
                    foreach (TransferManager.TransferReason transferReason in transferReasons)
                    {
                        if ((byte)transferReason == material_byte)
                        {
                            m_dropdownResource.selectedIndex = num;
                            break;
                        }
                        num++;
                    }
                }
                else if(material_byte >= 200)
                {
                    num = 16;
                    byte extended_material_byte = (byte)(material_byte - 200);
                    foreach (ExtendedTransferManager.TransferReason extendedTransferReason in extendedTransferReasons2)
                    {
                        if ((byte)extendedTransferReason == extended_material_byte)
                        {
                            m_dropdownResource.selectedIndex = num;
                            break;
                        }
                        num++;
                    }
                }
            }
            m_dropdownMode.selectedIndex = (int)warehouseMode;
        }

        protected override void UpdateBindings()
        {
            base.UpdateBindings();
            ushort building = m_InstanceID.Building;
            BuildingManager instance = Singleton<BuildingManager>.instance;
            Building building2 = instance.m_buildings.m_buffer[building];
            BuildingInfo info = building2.Info;
            BuildingAI buildingAI = info.m_buildingAI;
            ExtendedWarehouseAI extendedWarehouseAI = buildingAI as ExtendedWarehouseAI;
            m_Type.text = Singleton<BuildingManager>.instance.GetDefaultBuildingName(building, InstanceID.Empty);
            m_Status.text = extendedWarehouseAI.GetLocalizedStatus(building, ref instance.m_buildings.m_buffer[m_InstanceID.Building]);
            m_Upkeep.text = LocaleFormatter.FormatUpkeep(extendedWarehouseAI.GetResourceRate(building, ref instance.m_buildings.m_buffer[building], EconomyManager.Resource.Maintenance), isDistanceBased: false);
            m_Thumbnail.atlas = info.m_Atlas;
            m_Thumbnail.spriteName = info.m_Thumbnail;
            if (m_Thumbnail.atlas != null && !string.IsNullOrEmpty(m_Thumbnail.spriteName))
            {
                UITextureAtlas.SpriteInfo spriteInfo = m_Thumbnail.atlas[m_Thumbnail.spriteName];
                if (spriteInfo != null)
                {
                    m_Thumbnail.size = spriteInfo.pixelSize;
                }
            }
            m_BuildingDesc.text = info.GetLocalizedDescriptionShort();
            if ((building2.m_flags & Building.Flags.Collapsed) != 0)
            {
                m_RebuildButton.tooltip = ((!IsDisasterServiceRequired()) ? LocaleFormatter.FormatCost(extendedWarehouseAI.GetRelocationCost(), isDistanceBased: false) : Locale.Get("CITYSERVICE_TOOLTIP_DISASTERSERVICEREQUIRED"));
                m_RebuildButton.isVisible = Singleton<LoadingManager>.instance.SupportsExpansion(Expansion.NaturalDisasters);
                m_RebuildButton.isEnabled = CanRebuild();
                m_ActionPanel.isVisible = false;
            }
            else
            {
                m_RebuildButton.isVisible = false;
                m_ActionPanel.isVisible = true;
            }
            int num = building2.m_customBuffer1 * 100;
            m_resourceProgressBar.value = (float)num / (float)extendedWarehouseAI.m_storageCapacity;
            byte transferReason = extendedWarehouseAI.GetTransferReason(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building]);
            byte actualTransferReason = extendedWarehouseAI.GetActualTransferReason(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building]);
            if (actualTransferReason != 255 && actualTransferReason >= 200)
            {
                byte actual_material_byte = (byte)(actualTransferReason - 200);
                byte material_byte = (byte)(transferReason - 200);
                m_resourceProgressBar.progressColor = IndustryBuildingManager.GetExtendedResourceColor((ExtendedTransferManager.TransferReason)actualTransferReason);
                var extendedTransferReason = (ExtendedTransferManager.TransferReason)actual_material_byte;
                if(extendedTransferReason == ExtendedTransferManager.TransferReason.DrinkSupplies && m_NameField.text.Contains("Lemonade Factory"))
                {
                    m_NameField.text = "Drinks Factory";
                }
                m_resourceLabel.text = extendedTransferReason.ToString();
                m_emptyingOldResource.isVisible = material_byte != actual_material_byte;
                m_resourceDescription.isVisible = material_byte != 255;
                m_resourceDescription.text = GenerateExtendedResourceDescription((ExtendedTransferManager.TransferReason)actual_material_byte, isForWarehousePanel: true);
                m_resourceSprite.atlas = TextureUtils.GetAtlas("RestaurantAtlas");
                m_resourceSprite.spriteName = extendedTransferReason.ToString();
                var FormatResource = IndustryWorldInfoPanel.FormatResource((uint)num);
                var formatResourceWithUnit = FormatResourceWithUnit((uint)extendedWarehouseAI.m_storageCapacity);
                string text = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), FormatResource, formatResourceWithUnit);
                m_buffer.tooltip = text;
                m_capacityLabel.text = text;
            }
            else
            {
                m_resourceProgressBar.progressColor = IndustryWorldInfoPanel.instance.GetResourceColor((TransferManager.TransferReason)actualTransferReason);
                m_resourceLabel.text = Locale.Get("WAREHOUSEPANEL_RESOURCE", ((TransferManager.TransferReason)actualTransferReason).ToString());
                m_emptyingOldResource.isVisible = transferReason != actualTransferReason;
                m_resourceDescription.isVisible = transferReason != 255;
                m_resourceDescription.text = GenerateResourceDescription((TransferManager.TransferReason)transferReason, isForWarehousePanel: true);
                m_resourceSprite.atlas = TextureUtils.GetGameTextureAtlas("Ingame");
                m_resourceSprite.spriteName = IndustryWorldInfoPanel.ResourceSpriteName((TransferManager.TransferReason)actualTransferReason);
                string text = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), IndustryWorldInfoPanel.FormatResource((uint)num), IndustryWorldInfoPanel.FormatResourceWithUnit((uint)extendedWarehouseAI.m_storageCapacity, (TransferManager.TransferReason)actualTransferReason));
                m_buffer.tooltip = text;
                m_capacityLabel.text = text;
            }
            m_Info.text = extendedWarehouseAI.GetLocalizedStats(building, ref instance.m_buildings.m_buffer[building]);
            if (extendedWarehouseAI != null)
            {
                UpdateWorkers(building, extendedWarehouseAI, ref instance.m_buildings.m_buffer[building]);
            }
        }

        private void OnOnOffChanged(UIComponent comp, bool value)
        {
            isCityServiceEnabled = value;
        }

        private bool IsDisasterServiceRequired()
        {
            ushort building = m_InstanceID.Building;
            if (building != 0)
            {
                return Singleton<BuildingManager>.instance.m_buildings.m_buffer[building].m_levelUpProgress != byte.MaxValue;
            }
            return false;
        }

        public void OnRebuildClicked()
        {
            ushort buildingID = m_InstanceID.Building;
            if (buildingID == 0)
            {
                return;
            }
            Singleton<SimulationManager>.instance.AddAction(delegate
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                BuildingInfo info = instance.m_buildings.m_buffer[buildingID].Info;
                if ((object)info != null && (instance.m_buildings.m_buffer[buildingID].m_flags & Building.Flags.Collapsed) != 0)
                {
                    int relocationCost = info.m_buildingAI.GetRelocationCost();
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Construction, relocationCost, info.m_class);
                    Vector3 position = instance.m_buildings.m_buffer[buildingID].m_position;
                    float angle = instance.m_buildings.m_buffer[buildingID].m_angle;
                    RebuildBuilding(info, position, angle, buildingID, info.m_fixedHeight);
                    if (info.m_subBuildings != null && info.m_subBuildings.Length != 0)
                    {
                        Matrix4x4 matrix4x = default(Matrix4x4);
                        matrix4x.SetTRS(position, Quaternion.AngleAxis(angle * 57.29578f, Vector3.down), Vector3.one);
                        for (int i = 0; i < info.m_subBuildings.Length; i++)
                        {
                            BuildingInfo buildingInfo = info.m_subBuildings[i].m_buildingInfo;
                            Vector3 position2 = matrix4x.MultiplyPoint(info.m_subBuildings[i].m_position);
                            float angle2 = info.m_subBuildings[i].m_angle * ((float)Math.PI / 180f) + angle;
                            bool fixedHeight = info.m_subBuildings[i].m_fixedHeight;
                            ushort num = RebuildBuilding(buildingInfo, position2, angle2, 0, fixedHeight);
                            if (buildingID != 0 && num != 0)
                            {
                                instance.m_buildings.m_buffer[buildingID].m_subBuilding = num;
                                instance.m_buildings.m_buffer[num].m_parentBuilding = buildingID;
                                instance.m_buildings.m_buffer[num].m_flags |= Building.Flags.Untouchable;
                                buildingID = num;
                            }
                        }
                    }
                }
            });
        }

        private IEnumerator ToggleBuilding(ushort id, bool value)
        {
            if (Singleton<BuildingManager>.exists)
            {
                BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[id].Info;
                info.m_buildingAI.SetProductionRate(id, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[id], (byte)(value ? 100 : 0));
            }
            yield return 0;
        }

        private ushort RebuildBuilding(BuildingInfo info, Vector3 position, float angle, ushort buildingID, bool fixedHeight)
        {
            ushort building = 0;
            bool flag = false;
            if (buildingID != 0)
            {
                Singleton<BuildingManager>.instance.RelocateBuilding(buildingID, position, angle);
                building = buildingID;
                flag = true;
            }
            else if (Singleton<BuildingManager>.instance.CreateBuilding(out building, ref Singleton<SimulationManager>.instance.m_randomizer, info, position, angle, 0, Singleton<SimulationManager>.instance.m_currentBuildIndex))
            {
                if (fixedHeight)
                {
                    Singleton<BuildingManager>.instance.m_buildings.m_buffer[building].m_flags |= Building.Flags.FixedHeight;
                }
                Singleton<SimulationManager>.instance.m_currentBuildIndex++;
                flag = true;
            }
            if (flag)
            {
                int publicServiceIndex = ItemClass.GetPublicServiceIndex(info.m_class.m_service);
                if (publicServiceIndex != -1)
                {
                    Singleton<BuildingManager>.instance.m_buildingDestroyed2.Disable();
                    Singleton<GuideManager>.instance.m_serviceNotUsed[publicServiceIndex].Disable();
                    Singleton<GuideManager>.instance.m_serviceNeeded[publicServiceIndex].Deactivate();
                    Singleton<CoverageManager>.instance.CoverageUpdated(info.m_class.m_service, info.m_class.m_subService, info.m_class.m_level);
                }
                BuildingTool.DispatchPlacementEffect(info, 0, position, angle, info.m_cellWidth, info.m_cellLength, bulldozing: false, collapsed: false);
            }
            return building;
        }

        public void OnBudgetClicked()
        {
            if (ToolsModifierControl.IsUnlocked(UnlockManager.Feature.Economy))
            {
                ToolsModifierControl.mainToolbar.ShowEconomyPanel(1);
                WorldInfoPanel.Hide<ExtendedWarehouseWorldInfoPanel>();
            }
        }

        public bool CanRebuild()
        {
            ushort building = m_InstanceID.Building;
            if (building != 0 && Singleton<BuildingManager>.instance.m_buildings.m_buffer[building].m_levelUpProgress == byte.MaxValue)
            {
                BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building].Info;
                if (info != null)
                {
                    int relocationCost = info.m_buildingAI.GetRelocationCost();
                    if (Singleton<EconomyManager>.instance.PeekResource(EconomyManager.Resource.Construction, relocationCost) == relocationCost)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void OnRelocateBuilding()
        {
            if (ToolsModifierControl.GetTool<BuildingTool>() != null)
            {
                ToolsModifierControl.GetTool<BuildingTool>().m_relocateCompleted += RelocateCompleted;
            }
            ToolsModifierControl.keepThisWorldInfoPanel = true;
            BuildingTool buildingTool = ToolsModifierControl.SetTool<BuildingTool>();
            buildingTool.m_prefab = null;
            buildingTool.m_relocate = m_InstanceID.Building;
            m_IsRelocating = true;
            TempHide();
        }

        private void TempHide()
        {
            ToolsModifierControl.cameraController.ClearTarget();
            ValueAnimator.Animate("Relocating", delegate (float val)
            {
                base.component.opacity = val;
            }, new AnimatedFloat(1f, 0f, 0.33f), delegate
            {
                UIView.library.Hide(GetType().Name);
            });
            movingPanel.Find<UILabel>("MovingLabel").text = LocaleFormatter.FormatGeneric("BUILDING_MOVING", base.buildingName);
            movingPanel.Show();
        }

        public void TempShow(Vector3 worldPosition, InstanceID instanceID)
        {
            movingPanel.Hide();
            WorldInfoPanel.Show<ExtendedWarehouseWorldInfoPanel>(worldPosition, instanceID);
            ValueAnimator.Animate("Relocating", delegate (float val)
            {
                base.component.opacity = val;
            }, new AnimatedFloat(0f, 1f, 0.33f));
        }

        private void Update()
        {
            if (m_IsRelocating)
            {
                BuildingTool currentTool = ToolsModifierControl.GetCurrentTool<BuildingTool>();
                if (currentTool != null && IsValidTarget() && currentTool.m_relocate != 0 && !movingPanel.isVisible)
                {
                    movingPanel.Show();
                    return;
                }
                if (!IsValidTarget() || (currentTool != null && currentTool.m_relocate == 0))
                {
                    ToolsModifierControl.mainToolbar.ResetLastTool();
                    movingPanel.Hide();
                    m_IsRelocating = false;
                }
            }
            if (base.component.isVisible)
            {
                bool flag = isCityServiceEnabled;
                if (m_OnOff.isChecked != flag)
                {
                    m_OnOff.eventCheckChanged -= OnOnOffChanged;
                    m_OnOff.isChecked = flag;
                    m_OnOff.eventCheckChanged += OnOnOffChanged;
                }
            }
        }

        private void OnMovingPanelCloseClicked(UIComponent comp, UIMouseEventParameter p)
        {
            m_IsRelocating = false;
            ToolsModifierControl.GetTool<BuildingTool>().CancelRelocate();
        }

        private void RelocateCompleted(InstanceID newID)
        {
            if (ToolsModifierControl.GetTool<BuildingTool>() != null)
            {
                ToolsModifierControl.GetTool<BuildingTool>().m_relocateCompleted -= RelocateCompleted;
            }
            m_IsRelocating = false;
            if (!newID.IsEmpty)
            {
                m_InstanceID = newID;
            }
            if (IsValidTarget())
            {
                BuildingTool tool = ToolsModifierControl.GetTool<BuildingTool>();
                if (tool == ToolsModifierControl.GetCurrentTool<BuildingTool>())
                {
                    ToolsModifierControl.SetTool<DefaultTool>();
                    if (InstanceManager.GetPosition(m_InstanceID, out var position, out var _, out var size))
                    {
                        position.y += size.y * 0.8f;
                    }
                    TempShow(position, m_InstanceID);
                }
            }
            else
            {
                movingPanel.Hide();
                BuildingTool tool2 = ToolsModifierControl.GetTool<BuildingTool>();
                if (tool2 == ToolsModifierControl.GetCurrentTool<BuildingTool>())
                {
                    ToolsModifierControl.SetTool<DefaultTool>();
                }
                Hide();
            }
        }

        protected override void OnHide()
        {
            if (m_IsRelocating && ToolsModifierControl.GetTool<BuildingTool>() != null)
            {
                ToolsModifierControl.GetTool<BuildingTool>().m_relocateCompleted -= RelocateCompleted;
            }
            movingPanel.Hide();
        }

        private void OnEnable()
        {
            LocaleManager.eventLocaleChanged += OnLocaleChanged;
        }

        private void OnDisable()
        {
            LocaleManager.eventLocaleChanged -= OnLocaleChanged;
        }

        private void OnLocaleChanged()
        {
            RefreshDropdownLists();
        }

        private static int GetValue(int value, int total)
        {
            float num = (float)value / (float)total;
            return Mathf.Clamp(Mathf.FloorToInt(num * 100f), 0, 100);
        }

        private Color32 MultiplyColor(Color col, float scalar)
        {
            Color color = col * scalar;
            color.a = col.a;
            return color;
        }

        private void UpdateWorkers(ushort buildingID, ExtendedWarehouseAI extendedWarehouseAI, ref Building building)
        {
            if (!Singleton<CitizenManager>.exists)
            {
                return;
            }
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint num = building.m_citizenUnits;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            int num6 = 0;
            int num7 = 0;
            int num8 = 0;
            int num9 = 0;
            int num10 = 0;
            int num11 = 0;
            int num12 = 0;
            num5 = extendedWarehouseAI.m_workPlaceCount0;
            num6 = extendedWarehouseAI.m_workPlaceCount1;
            num7 = extendedWarehouseAI.m_workPlaceCount2;
            num8 = extendedWarehouseAI.m_workPlaceCount3;
            num4 = num5 + num6 + num7 + num8;
            while (num != 0)
            {
                uint nextUnit = instance.m_units.m_buffer[num].m_nextUnit;
                if ((instance.m_units.m_buffer[num].m_flags & CitizenUnit.Flags.Work) != 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        uint citizen = instance.m_units.m_buffer[num].GetCitizen(i);
                        if (citizen != 0 && !instance.m_citizens.m_buffer[citizen].Dead && (instance.m_citizens.m_buffer[citizen].m_flags & Citizen.Flags.MovingIn) == 0)
                        {
                            num3++;
                            switch (instance.m_citizens.m_buffer[citizen].EducationLevel)
                            {
                                case Citizen.Education.Uneducated:
                                    num9++;
                                    break;
                                case Citizen.Education.OneSchool:
                                    num10++;
                                    break;
                                case Citizen.Education.TwoSchools:
                                    num11++;
                                    break;
                                case Citizen.Education.ThreeSchools:
                                    num12++;
                                    break;
                            }
                        }
                    }
                }
                num = nextUnit;
                if (++num2 > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            int num13 = 0;
            int num14 = num5 - num9;
            if (num10 > num6)
            {
                num13 += Mathf.Max(0, Mathf.Min(num14, num10 - num6));
            }
            num14 += num6 - num10;
            if (num11 > num7)
            {
                num13 += Mathf.Max(0, Mathf.Min(num14, num11 - num7));
            }
            num14 += num7 - num11;
            if (num12 > num8)
            {
                num13 += Mathf.Max(0, Mathf.Min(num14, num12 - num8));
            }
            string format = ColossalFramework.Globalization.Locale.Get((num13 != 1) ? "ZONEDBUILDING_OVEREDUCATEDWORKERS" : "ZONEDBUILDING_OVEREDUCATEDWORKER");
            m_OverWorkSituation.text = StringUtils.SafeFormat(format, num13);
            m_OverWorkSituation.isVisible = num13 > 0;
            m_UneducatedPlaces.text = num5.ToString();
            m_EducatedPlaces.text = num6.ToString();
            m_WellEducatedPlaces.text = num7.ToString();
            m_HighlyEducatedPlaces.text = num8.ToString();
            m_UneducatedWorkers.text = num9.ToString();
            m_EducatedWorkers.text = num10.ToString();
            m_WellEducatedWorkers.text = num11.ToString();
            m_HighlyEducatedWorkers.text = num12.ToString();
            m_JobsAvailLegend.text = (num4 - (num9 + num10 + num11 + num12)).ToString();
            int num15 = GetValue(num5, num4);
            int value = GetValue(num6, num4);
            int value2 = GetValue(num7, num4);
            int value3 = GetValue(num8, num4);
            int num16 = num15 + value + value2 + value3;
            if (num16 != 0 && num16 != 100)
            {
                num15 = 100 - (value + value2 + value3);
            }
            m_WorkPlacesEducationChart.SetValues(num15, value, value2, value3);
            int value4 = GetValue(num9, num4);
            int value5 = GetValue(num10, num4);
            int value6 = GetValue(num11, num4);
            int value7 = GetValue(num12, num4);
            int num17 = 0;
            int num18 = value4 + value5 + value6 + value7;
            num17 = 100 - num18;
            m_WorkersEducationChart.SetValues(value4, value5, value6, value7, num17);
            m_workersInfoLabel.text = StringUtils.SafeFormat(ColossalFramework.Globalization.Locale.Get("ZONEDBUILDING_WORKERS"), num3, num4);
        }

        public static string GenerateResourceDescription(TransferManager.TransferReason resource, bool isForWarehousePanel = false)
        {
            string text = Locale.Get("RESOURCEDESCRIPTION", resource.ToString());
            if (resource == TransferManager.TransferReason.None)
            {
                return text;
            }
            text += Environment.NewLine;
            text += Environment.NewLine;
            switch (resource)
            {
                case TransferManager.TransferReason.Oil:
                case TransferManager.TransferReason.Ore:
                case TransferManager.TransferReason.Logs:
                case TransferManager.TransferReason.Grain:
                    text = text + "- " + Locale.Get("RESOURCE_CANBEIMPORTED_COST");
                    break;
                case TransferManager.TransferReason.Goods:
                case TransferManager.TransferReason.Coal:
                case TransferManager.TransferReason.Petrol:
                case TransferManager.TransferReason.Food:
                case TransferManager.TransferReason.Lumber:
                    text = text + "- " + Locale.Get("RESOURCE_CANBEIMPORTED");
                    break;
                default:
                    text = text + "- " + Locale.Get("RESOURCE_CANNOTBEIMPORTED");
                    break;
            }
            text += Environment.NewLine;
            text = ((resource != TransferManager.TransferReason.Food && resource != TransferManager.TransferReason.Coal && resource != TransferManager.TransferReason.Petrol && resource != TransferManager.TransferReason.Lumber && resource != TransferManager.TransferReason.Goods) ? (text + "- " + Locale.Get("RESOURCE_CANBEEXPORTED_COST")) : (text + "- " + Locale.Get("RESOURCE_CANBEEXPORTED")));
            if (isForWarehousePanel)
            {
                return text;
            }
            text += Environment.NewLine;
            text += Environment.NewLine;
            if (resource == TransferManager.TransferReason.Ore || resource == TransferManager.TransferReason.Oil || resource == TransferManager.TransferReason.Grain || resource == TransferManager.TransferReason.Logs)
            {
                return text + "- " + LocaleFormatter.FormatGeneric("RESOURCE_STOREINSTORAGEBUILDING", Locale.Get("WAREHOUSEPANEL_RESOURCE", resource.ToString()));
            }
            return text + "- " + Locale.Get("RESOURCE_STOREINWAREHOUSE");
        }

        public static string GenerateExtendedResourceDescription(ExtendedTransferManager.TransferReason resource, bool isForWarehousePanel = false)
        {
            string text = resource.ToString();
            if (resource == ExtendedTransferManager.TransferReason.None)
            {
                return text;
            }
            text += Environment.NewLine;
            text += Environment.NewLine;
            text = text + "- " + Locale.Get("RESOURCE_CANNOTBEIMPORTED");
            text += Environment.NewLine;
            text += Environment.NewLine;
            text = "Cannot be exported";
            if (isForWarehousePanel)
            {
                return text;
            }
            text += Environment.NewLine;
            text += Environment.NewLine;
            return text + "- " + Locale.Get("RESOURCE_STOREINWAREHOUSE");
        }

        private static string FormatResourceWithUnit(uint amount)
        {
	    return string.Concat(str2: Locale.Get("RESOURCEUNIT_TONS"), str0: IndustryWorldInfoPanel.FormatResource(amount), str1: " ");
        }

    }
}
