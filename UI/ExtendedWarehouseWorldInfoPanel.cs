using System;
using System.Collections;
using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using MoreTransferReasons;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.UI
{
    public class ExtendedWarehouseWorldInfoPanel : BuildingWorldInfoPanel
    {
        private enum WarehouseModes
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

        private readonly WarehouseModes[] m_warehouseModes =
        [
                WarehouseModes.Balanced,
                WarehouseModes.Import,
                WarehouseModes.Export
        ];

        public UIComponent MovingPanel
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

        public bool IsCityServiceEnabled
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

        private WarehouseModes WarehouseMode
        {
            get
            {
                if ((Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_flags & Building.Flags.Filling) == 0 && (Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_flags & Building.Flags.Downgrading) == 0)
                {
                    return WarehouseModes.Balanced;
                }
                if ((Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_flags & Building.Flags.Downgrading) != Building.Flags.None)
                {
                    return WarehouseModes.Export;
                }
                return WarehouseModes.Import;
            }
            set
            {
                switch (value)
                {
                    case WarehouseModes.Balanced:
                        Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI.SetEmptying(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building], emptying: false);
                        Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI.SetFilling(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building], filling: false);
                        break;
                    case WarehouseModes.Export:
                        Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI.SetEmptying(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building], emptying: true);
                        Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI.SetFilling(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building], filling: false);
                        break;
                    case WarehouseModes.Import:
                        Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI.SetEmptying(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building], emptying: false);
                        Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI.SetFilling(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building], filling: true);
                        break;
                }
            }
        }

        protected override void Start()
        {
            base.Start();
            List<TransferManager.TransferReason> list =
            [
                TransferManager.TransferReason.None,
                TransferManager.TransferReason.AnimalProducts,
                TransferManager.TransferReason.Flours,
                TransferManager.TransferReason.Paper,
                TransferManager.TransferReason.PlanedTimber,
                TransferManager.TransferReason.Petroleum,
                TransferManager.TransferReason.Plastics,
                TransferManager.TransferReason.Glass,
                TransferManager.TransferReason.Metals,
                TransferManager.TransferReason.LuxuryProducts,
                TransferManager.TransferReason.Lumber,
                TransferManager.TransferReason.Food,
                TransferManager.TransferReason.Coal,
                TransferManager.TransferReason.Petrol,
                TransferManager.TransferReason.Goods,
                ExtendedTransferManager.Milk,
                ExtendedTransferManager.Pork,
                ExtendedTransferManager.Fruits,
                ExtendedTransferManager.Vegetables,
                ExtendedTransferManager.Wool,
                ExtendedTransferManager.Cotton,
                ExtendedTransferManager.Cows,
                ExtendedTransferManager.HighlandCows,
                ExtendedTransferManager.Sheep,
                ExtendedTransferManager.Pigs,
                ExtendedTransferManager.ProcessedVegetableOil,
                ExtendedTransferManager.Leather,
                ExtendedTransferManager.FoodProducts,
                ExtendedTransferManager.BeverageProducts,
                ExtendedTransferManager.BakedGoods,
                ExtendedTransferManager.CannedFish,
                ExtendedTransferManager.Furnitures,
                ExtendedTransferManager.ElectronicProducts,
                ExtendedTransferManager.IndustrialSteel,
                ExtendedTransferManager.Tupperware,
                ExtendedTransferManager.Toys,
                ExtendedTransferManager.PrintedProducts,
                ExtendedTransferManager.TissuePaper,
                ExtendedTransferManager.Cloths,
                ExtendedTransferManager.PetroleumProducts,
                ExtendedTransferManager.Cars,
                ExtendedTransferManager.Footwear,
                ExtendedTransferManager.HouseParts,
            ];
            List<TransferManager.TransferReason> list2 = list;
            if (Singleton<LoadingManager>.instance.SupportsExpansion(Expansion.Urban))
            {
                list2.Add(TransferManager.TransferReason.Fish);
                list2.Add(ExtendedTransferManager.Anchovy);
                list2.Add(ExtendedTransferManager.Salmon);
                list2.Add(ExtendedTransferManager.Shellfish);
                list2.Add(ExtendedTransferManager.Tuna);
                list2.Add(ExtendedTransferManager.Algae);
                list2.Add(ExtendedTransferManager.Seaweed);
                list2.Add(ExtendedTransferManager.Trout);
            }
            m_transferReasons = [.. list2];
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
            Color[] array = [m_UneducatedColor, m_EducatedColor, m_WellEducatedColor, m_HighlyEducatedColor];
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
            m_originalHeight = component.height;
            m_resourceDescription = Find<UILabel>("ResourceDescription");
            m_resourceSprite = Find<UISprite>("ResourceIcon");
        }

        private void OnDropdownModeChanged(UIComponent component, int index)
        {
            WarehouseMode = m_warehouseModes[index];
        }

        private void OnDropdownResourceChanged(UIComponent component, int index)
        {
            WarehouseAI ai = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI as WarehouseAI;
            Singleton<SimulationManager>.instance.AddAction(delegate
            {
                ai.SetTransferReason(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building], m_transferReasons[index]);
            });
        }

        private void RefreshDropdownLists()
        {
            string[] array = new string[m_transferReasons.Length];
            for (int i = 0; i < m_transferReasons.Length; i++)
            {
                string text = Locale.Get("WAREHOUSEPANEL_RESOURCE", m_transferReasons[i].ToString());
                array[i] = text;
            }
            m_dropdownResource.items = array;
            array = new string[m_warehouseModes.Length];
            for (int j = 0; j < m_warehouseModes.Length; j++)
            {
                string text2 = Locale.Get("WAREHOUSEPANEL_MODE", m_warehouseModes[j].ToString());
                array[j] = text2;
            }
            m_dropdownMode.items = array;
        }

        protected override void OnSetTarget()
        {
            base.OnSetTarget();
            WarehouseAI warehouseAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI as WarehouseAI;
            m_resourcePanel.isVisible = warehouseAI.m_storageType == TransferManager.TransferReason.None;
            component.height = !m_resourcePanel.isVisible ? m_originalHeight - m_resourcePanel.height : m_originalHeight;
            if (m_resourcePanel.isVisible)
            {
                int num = 0;
                TransferManager.TransferReason[] transferReasons = m_transferReasons;
                foreach (TransferManager.TransferReason transferReason in transferReasons)
                {
                    if (transferReason == warehouseAI.GetTransferReason(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building]))
                    {
                        m_dropdownResource.selectedIndex = num;
                        break;
                    }
                    num++;
                }
            }
            m_dropdownMode.selectedIndex = (int)WarehouseMode;
        }

        protected override void UpdateBindings()
        {
            base.UpdateBindings();
            ushort building = m_InstanceID.Building;
            BuildingManager instance = Singleton<BuildingManager>.instance;
            Building building2 = instance.m_buildings.m_buffer[building];
            BuildingInfo info = building2.Info;
            BuildingAI buildingAI = info.m_buildingAI;
            m_Type.text = Singleton<BuildingManager>.instance.GetDefaultBuildingName(building, InstanceID.Empty);
            m_Status.text = buildingAI.GetLocalizedStatus(building, ref instance.m_buildings.m_buffer[m_InstanceID.Building]);
            m_Upkeep.text = LocaleFormatter.FormatUpkeep(buildingAI.GetResourceRate(building, ref instance.m_buildings.m_buffer[building], EconomyManager.Resource.Maintenance), isDistanceBased: false);
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
            if ((building2.m_flags & Building.Flags.Collapsed) != Building.Flags.None)
            {
                m_RebuildButton.tooltip = !IsDisasterServiceRequired() ? LocaleFormatter.FormatCost(buildingAI.GetRelocationCost(), isDistanceBased: false) : Locale.Get("CITYSERVICE_TOOLTIP_DISASTERSERVICEREQUIRED");
                m_RebuildButton.isVisible = Singleton<LoadingManager>.instance.SupportsExpansion(Expansion.NaturalDisasters);
                m_RebuildButton.isEnabled = CanRebuild();
                m_ActionPanel.isVisible = false;
            }
            else
            {
                m_RebuildButton.isVisible = false;
                m_ActionPanel.isVisible = true;
            }
            WarehouseAI warehouseAI = buildingAI as WarehouseAI;
            int num = building2.m_customBuffer1 * 100;
            m_resourceProgressBar.value = num / (float)warehouseAI.m_storageCapacity;
            TransferManager.TransferReason transferReason = warehouseAI.GetTransferReason(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building]);
            TransferManager.TransferReason actualTransferReason = warehouseAI.GetActualTransferReason(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building]);
            m_resourceProgressBar.progressColor = IndustryWorldInfoPanel.instance.GetResourceColor(actualTransferReason);
            m_resourceLabel.text = Locale.Get("WAREHOUSEPANEL_RESOURCE", actualTransferReason.ToString());
            m_emptyingOldResource.isVisible = transferReason != actualTransferReason;
            m_resourceDescription.isVisible = transferReason != TransferManager.TransferReason.None;
            m_resourceDescription.text = GenerateResourceDescription(transferReason, isForWarehousePanel: true);
            m_resourceSprite.spriteName = IndustryWorldInfoPanel.ResourceSpriteName(actualTransferReason);
            string text = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), IndustryWorldInfoPanel.FormatResource((uint)num), IndustryWorldInfoPanel.FormatResourceWithUnit((uint)warehouseAI.m_storageCapacity, actualTransferReason));
            m_buffer.tooltip = text;
            m_capacityLabel.text = text;
            m_Info.text = buildingAI.GetLocalizedStats(building, ref instance.m_buildings.m_buffer[building]);
            if (warehouseAI != null)
            {
                UpdateWorkers(warehouseAI, ref instance.m_buildings.m_buffer[building]);
            }
        }

        private void OnOnOffChanged(UIComponent comp, bool value)
        {
            IsCityServiceEnabled = value;
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
                if (info is not null && (instance.m_buildings.m_buffer[buildingID].m_flags & Building.Flags.Collapsed) != Building.Flags.None)
                {
                    int relocationCost = info.m_buildingAI.GetRelocationCost();
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Construction, relocationCost, info.m_class);
                    Vector3 position = instance.m_buildings.m_buffer[buildingID].m_position;
                    float angle = instance.m_buildings.m_buffer[buildingID].m_angle;
                    RebuildBuilding(info, position, angle, buildingID, info.m_fixedHeight);
                    if (info.m_subBuildings != null && info.m_subBuildings.Length != 0)
                    {
                        Matrix4x4 matrix4x = default;
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
            bool flag = false;
            ushort building;
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
            if (IsUnlocked(UnlockManager.Feature.Economy))
            {
                mainToolbar.ShowEconomyPanel(1);
                Hide<WarehouseWorldInfoPanel>();
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
            if (GetTool<BuildingTool>() != null)
            {
                GetTool<BuildingTool>().m_relocateCompleted += RelocateCompleted;
            }
            keepThisWorldInfoPanel = true;
            BuildingTool buildingTool = SetTool<BuildingTool>();
            buildingTool.m_prefab = null;
            buildingTool.m_relocate = m_InstanceID.Building;
            m_IsRelocating = true;
            TempHide();
        }

        private void TempHide()
        {
            cameraController.ClearTarget();
            ValueAnimator.Animate("Relocating", delegate (float val)
            {
                component.opacity = val;
            }, new AnimatedFloat(1f, 0f, 0.33f), delegate
            {
                UIView.library.Hide(GetType().Name);
            });
            MovingPanel.Find<UILabel>("MovingLabel").text = LocaleFormatter.FormatGeneric("BUILDING_MOVING", buildingName);
            MovingPanel.Show();
        }

        public void TempShow(Vector3 worldPosition, InstanceID instanceID)
        {
            MovingPanel.Hide();
            Show<WarehouseWorldInfoPanel>(worldPosition, instanceID);
            ValueAnimator.Animate("Relocating", delegate (float val)
            {
                component.opacity = val;
            }, new AnimatedFloat(0f, 1f, 0.33f));
        }

        private void Update()
        {
            if (m_IsRelocating)
            {
                BuildingTool currentTool = GetCurrentTool<BuildingTool>();
                if (currentTool != null && IsValidTarget() && currentTool.m_relocate != 0 && !MovingPanel.isVisible)
                {
                    MovingPanel.Show();
                    return;
                }
                if (!IsValidTarget() || currentTool != null && currentTool.m_relocate == 0)
                {
                    mainToolbar.ResetLastTool();
                    MovingPanel.Hide();
                    m_IsRelocating = false;
                }
            }
            if (component.isVisible)
            {
                bool flag = IsCityServiceEnabled;
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
            GetTool<BuildingTool>().CancelRelocate();
        }

        private void RelocateCompleted(InstanceID newID)
        {
            if (GetTool<BuildingTool>() != null)
            {
                GetTool<BuildingTool>().m_relocateCompleted -= RelocateCompleted;
            }
            m_IsRelocating = false;
            if (!newID.IsEmpty)
            {
                m_InstanceID = newID;
            }
            if (IsValidTarget())
            {
                BuildingTool tool = GetTool<BuildingTool>();
                if (tool == GetCurrentTool<BuildingTool>())
                {
                    SetTool<DefaultTool>();
                    if (InstanceManager.GetPosition(m_InstanceID, out var position, out var _, out var size))
                    {
                        position.y += size.y * 0.8f;
                    }
                    TempShow(position, m_InstanceID);
                }
            }
            else
            {
                MovingPanel.Hide();
                BuildingTool tool2 = GetTool<BuildingTool>();
                if (tool2 == GetCurrentTool<BuildingTool>())
                {
                    SetTool<DefaultTool>();
                }
                Hide();
            }
        }

        protected override void OnHide()
        {
            if (m_IsRelocating && GetTool<BuildingTool>() != null)
            {
                GetTool<BuildingTool>().m_relocateCompleted -= RelocateCompleted;
            }
            MovingPanel.Hide();
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
            float num = value / (float)total;
            return Mathf.Clamp(Mathf.FloorToInt(num * 100f), 0, 100);
        }

        private Color32 MultiplyColor(Color col, float scalar)
        {
            Color color = col * scalar;
            color.a = col.a;
            return color;
        }

        private void UpdateWorkers(WarehouseAI warehouseAI, ref Building building)
        {
            if (!Singleton<CitizenManager>.exists)
            {
                return;
            }
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint num = building.m_citizenUnits;
            int num2 = 0;
            int num3 = 0;
            int num9 = 0;
            int num10 = 0;
            int num11 = 0;
            int num12 = 0;
            int num5 = warehouseAI.m_workPlaceCount0;
            int num6 = warehouseAI.m_workPlaceCount1;
            int num7 = warehouseAI.m_workPlaceCount2;
            int num8 = warehouseAI.m_workPlaceCount3;
            int num4 = num5 + num6 + num7 + num8;
            while (num != 0)
            {
                uint nextUnit = instance.m_units.m_buffer[num].m_nextUnit;
                if ((instance.m_units.m_buffer[num].m_flags & CitizenUnit.Flags.Work) != CitizenUnit.Flags.None)
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
            string format = Locale.Get(num13 != 1 ? "ZONEDBUILDING_OVEREDUCATEDWORKERS" : "ZONEDBUILDING_OVEREDUCATEDWORKER");
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
            int num18 = value4 + value5 + value6 + value7;
            int num17 = 100 - num18;
            m_WorkersEducationChart.SetValues(value4, value5, value6, value7, num17);
            m_workersInfoLabel.text = StringUtils.SafeFormat(Locale.Get("ZONEDBUILDING_WORKERS"), num3, num4);
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
            text = resource switch
            {
                TransferManager.TransferReason.Oil or TransferManager.TransferReason.Ore or TransferManager.TransferReason.Logs or TransferManager.TransferReason.Grain => text + "- " + Locale.Get("RESOURCE_CANBEIMPORTED_COST"),
                TransferManager.TransferReason.Goods or TransferManager.TransferReason.Coal or TransferManager.TransferReason.Petrol or TransferManager.TransferReason.Food or TransferManager.TransferReason.Lumber => text + "- " + Locale.Get("RESOURCE_CANBEIMPORTED"),
                _ => text + "- " + Locale.Get("RESOURCE_CANNOTBEIMPORTED"),
            };
            text += Environment.NewLine;
            text = resource != TransferManager.TransferReason.Food && resource != TransferManager.TransferReason.Coal && resource != TransferManager.TransferReason.Petrol && resource != TransferManager.TransferReason.Lumber && resource != TransferManager.TransferReason.Goods ? text + "- " + Locale.Get("RESOURCE_CANBEEXPORTED_COST") : text + "- " + Locale.Get("RESOURCE_CANBEEXPORTED");
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
    }
}
