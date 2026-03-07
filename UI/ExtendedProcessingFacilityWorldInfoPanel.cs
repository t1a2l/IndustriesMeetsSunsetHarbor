using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Threading;
using ColossalFramework.UI;
using ICities;
using IndustriesMeetsSunsetHarbor.AI;
using IndustriesMeetsSunsetHarbor.Managers;
using MoreTransferReasons;
using MoreTransferReasons.Utils;
using UnityEngine;
using static RenderManager;

namespace IndustriesMeetsSunsetHarbor.UI
{
    public sealed class ExtendedProcessingFacilityWorldInfoPanel : BuildingWorldInfoPanel
    {
        private UIPanel m_right;

        private UIPanel m_mainBottom;

        private UIPanel m_wrapper;

        private UIPanel m_layout;

        private UIButton m_BudgetButton;

        private UIButton m_MoveButton;

        private UIPanel m_ActionPanel;

        private UIButton m_RebuildButton;

        private UIComponent m_MovingPanel;

        private UILabel m_Type;

        private UILabel m_Status;

        private UILabel m_Upkeep;

        private UISprite m_Thumbnail;

        private UILabel m_BuildingInfo;

        private UILabel m_BuildingDesc;

        private UISprite m_BuildingService;

        private UIPanel m_Parkbuttons;

        private UIButton m_ShowIndustryInfoButton;

        private UICheckBox m_OnOff;

        private bool m_IsRelocating;

        private UIPanel m_VariationPanel;

        private UIDropDown m_VariationDropdown;

        private IndustryBuildingAI m_IndustryBuildingAI;

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

        private UIProgressBar m_input1Buffer;

        private UIProgressBar m_input2Buffer;

        private UILabel m_input1Label;

        private UILabel m_input2Label;

        private UIPanel m_inputSection;

        private UIPanel m_storageInput1;

        private UIPanel m_storageInput2;

        private UIProgressBar m_output1Buffer;

        private UIProgressBar m_output2Buffer;

        private UILabel m_output1Label;

        private UILabel m_output2Label;

        private UIPanel m_outputSection;

        private UIPanel m_storageOutput1;

        private UIPanel m_storageOutput2;

        private PlayerBuildingAI m_playerBuildingAI;

        private ExtendedProcessingFacilityAI m_extendedProcessingFacilityAI;

        private UISprite m_arrow1;

        private UISprite m_arrow2;

        private UISprite m_arrow3;

        private UISprite m_input1Sprite;

        private UISprite m_input2Sprite;

        private UISprite m_output1Sprite;

        private UISprite m_output2Sprite;

        private UIPanel m_inputOutputSection;

        private bool m_needResetTarget;

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

        protected override void Start()
        {
            m_right = Find<UIPanel>("Right");
            m_mainBottom = Find<UIPanel>("MainBottom");
            base.Start();
            m_wrapper = Find<UIPanel>("Wrapper");
            m_Type = Find<UILabel>("Type");
            m_Status = Find<UILabel>("Status");
            m_Upkeep = Find<UILabel>("Upkeep");
            m_Thumbnail = Find<UISprite>("Thumbnail");
            m_BuildingInfo = Find<UILabel>("Info");
            m_BuildingDesc = Find<UILabel>("Desc");
            m_BuildingService = Find<UISprite>("Service");
            m_BudgetButton = Find<UIButton>("Budget");
            m_MoveButton = Find<UIButton>("RelocateAction");
            m_BudgetButton.isEnabled = ToolsModifierControl.IsUnlocked(UnlockManager.Feature.Economy);

            Find<UIButton>("OpenPedestrianAreaButton").isVisible = false;
            Find<UIPanel>("WonderEffectPanel").isVisible = false;
            Find<UIPanel>("TotalWorksPanel").isVisible = false;
            Find<UIPanel>("Checkboxes").isVisible = false;
            Find<UIPanel>("IntercityTrainsPanel").isVisible = false;
            Find<UICheckBox>("AcceptIntercityTrains").isVisible = false;
            Find<UIButton>("OpenCampusPanelButton").isVisible = false;
            Find<UIButton>("OpenAirportPanelButton").isVisible = false;
            Find<UIButton>("LinesOverview").isVisible = false;
            Find<UIButton>("LevelUpButton").isVisible = false;
            Find<UIButton>("OpenPedestrianAreaButton").isVisible = false;
            Find<UIButton>("OpenMuseumCampusPanelButton").isVisible = false;
            Find<UIPanel>("TicketPriceSection").isVisible = false;
            Find<UIPanel>("AcademicWorksPanel").isVisible = false;
            Find<UILabel>("AcademicWorksPanelTitle").isVisible = false;
            Find<UIPanel>("BuildingVariationContainer").isVisible = false;
            Find<UIButton>("OpenParkPanelButton").isVisible = false;
            Find<UIPanel>("VehicleSelectorContainer").isVisible = false;
            Find<UIPanel>("AdditionalVehicleSelectorContainer").isVisible = false;
            Find<UIButton>("SpecialAction").isVisible = false;
            Find<UIButton>("DeleteFishingRouteButton").isVisible = false;

            m_ShowIndustryInfoButton = Find<UIButton>("OpenIndustryPanelButton");
            m_Parkbuttons = Find<UIPanel>("ParkButtons");
            m_ActionPanel = Find<UIPanel>("ActionPanel");
            m_RebuildButton = Find<UIButton>("RebuildButton");
            m_OnOff = Find<UICheckBox>("On/Off");
            m_OnOff.eventCheckChanged += OnOnOffChanged;
            m_VariationPanel = Find<UIPanel>("VariationPanel");
            m_VariationDropdown = Find<UIDropDown>("DropdownVariation");
            m_VariationDropdown.eventSelectedIndexChanged += OnVariationDropdownChanged;
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
            m_input1Buffer = Find<UIProgressBar>("InputBuffer");
            m_storageInput1 = Find<UIPanel>("StorageInput");
            m_input1Label = Find<UILabel>("StorageInputLabel");
            m_inputSection = Find<UIPanel>("InputSection");
            m_inputSection = Find<UIPanel>("InputSection");
            m_input1Sprite = Find<UISprite>("ResourceIconInput");
            m_output1Sprite = Find<UISprite>("ResourceIconOutput");
            m_output1Buffer = Find<UIProgressBar>("OutputBuffer");
            m_storageOutput1 = Find<UIPanel>("StorageOutput");
            m_output1Label = Find<UILabel>("StorageOutputLabel");
            m_outputSection = Find<UIPanel>("OutputSection");
            m_arrow1 = Find<UISprite>("Arrow1");
            m_arrow2 = Find<UISprite>("Arrow2");
            m_arrow3 = Find<UISprite>("Arrow3");
            m_inputOutputSection = Find<UIPanel>("InputOutputSection");
            m_layout = Find<UIPanel>("Layout");

            MakeStorageInput();
            MakeStorageOutput();

            m_storageInput2.isVisible = false;
            m_storageOutput2.isVisible = false;

            Find<UIButton>("Close").eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                Hide();
            };
            m_MoveButton.eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                OnRelocateBuilding();
            };
            m_RebuildButton.eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                OnRebuildClicked();
            };
            m_ShowIndustryInfoButton.eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                OpenIndustryInfoPanel();
            };
            // 26, 26 arrow
        }

        private void OnVariationDropdownChanged(UIComponent component, int value)
        {
            var selectedValue = m_VariationDropdown.items[value];
            if (!m_IndustryBuildingAI.GetVariations(out var variations))
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                ref Building building2 = ref instance.m_buildings.m_buffer[m_InstanceID.Building];
                if (!building2.Info.name.Contains("Slaughter House") && !building2.Info.name.Contains("Milking Parlour"))
                {
                    return;
                }   
            }
            Singleton<SimulationManager>.instance.AddAction(delegate
            {
                if (Singleton<BuildingManager>.exists)
                {
                    if(variations != null)
                    {
                        Singleton<BuildingManager>.instance.UpdateBuildingInfo(m_InstanceID.Building, variations[m_VariationDropdown.selectedIndex].m_info);
                    }
                    BuildingManager instance = Singleton<BuildingManager>.instance;
                    Building building = instance.m_buildings.m_buffer[m_InstanceID.Building];
                    string[] names = ["Animal Pasture", "Slaughter House", "Milking Parlour"];
                    if (names.Any(s => building.Info.name.Contains(s)))
                    {
                        AnimalPastureVariationChanged(selectedValue);
                    }
                    ThreadHelper.dispatcher.Dispatch(delegate
                    {
                        m_NameField.text = GetName();      
                    });
                    if (variations != null)
                    {
                        IndustryBuildingAI industryBuildingAI = (IndustryBuildingAI)Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI;
                        industryBuildingAI.SetLastVariationIndex(value);
                    }
                    else
                    {
                        var buffers = CustomBuffersManager.GetCustomBuffer(m_InstanceID.Building);
                        buffers.m_animalVariationIndex = value;
                        CustomBuffersManager.SetCustomBuffer(m_InstanceID.Building, buffers);
                    }
                }
            });
        }

        private void AnimalPastureVariationChanged(string selectedValue)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            ref Building building = ref instance.m_buildings.m_buffer[m_InstanceID.Building];
            var oldInputResource1 = m_extendedProcessingFacilityAI.m_inputResource1;
            var oldOutputResource1 = m_extendedProcessingFacilityAI.m_outputResource1;
            var oldOutputResource2 = m_extendedProcessingFacilityAI.m_outputResource2;
            TransferManager.TransferReason[] inputResource1 = [];
            TransferManager.TransferReason outputResource1 = TransferManager.TransferReason.None;
            TransferManager.TransferReason outputResource2 = TransferManager.TransferReason.None;
            if (selectedValue.Contains("Sheep"))
            {
                if (building.Info.name.Contains("Animal Pasture"))
                {
                    outputResource1 = ExtendedTransferManager.Sheep;
                    outputResource2 = ExtendedTransferManager.Wool;
                }
                else if (building.Info.name.Contains("Slaughter House"))
                {
                    inputResource1 = [ExtendedTransferManager.Sheep];
                    outputResource1 = TransferManager.TransferReason.AnimalProducts;
                }
                else if (building.Info.name.Contains("Milking Parlour"))
                {
                    inputResource1 = [ExtendedTransferManager.Sheep];
                    outputResource1 = ExtendedTransferManager.Milk;
                }
            }
            else if (selectedValue.Contains("Highland Cow"))
            {
                if (building.Info.name.Contains("Animal Pasture"))
                {
                    outputResource1 = ExtendedTransferManager.HighlandCows;
                    outputResource2 = TransferManager.TransferReason.None;
                }
                else if (building.Info.name.Contains("Slaughter House"))
                {
                    inputResource1 = [ExtendedTransferManager.HighlandCows];
                    outputResource1 = TransferManager.TransferReason.AnimalProducts;
                }
                else if (building.Info.name.Contains("Milking Parlour"))
                {
                    inputResource1 = [ExtendedTransferManager.HighlandCows];
                    outputResource1 = ExtendedTransferManager.Milk;
                }
            }
            else if (selectedValue.Contains("Pig"))
            {
                if (building.Info.name.Contains("Animal Pasture"))
                {
                    outputResource1 = ExtendedTransferManager.Pigs;
                    outputResource2 = TransferManager.TransferReason.None;
                }
                else if (building.Info.name.Contains("Slaughter House"))
                {
                    inputResource1 = [ExtendedTransferManager.Pigs];
                    outputResource1 = ExtendedTransferManager.Pork;
                }
            }
            else
            {
                if (building.Info.name.Contains("Animal Pasture"))
                {
                    outputResource1 = ExtendedTransferManager.Cows;
                    outputResource2 = TransferManager.TransferReason.None;
                }
                else if (building.Info.name.Contains("Slaughter House"))
                {
                    inputResource1 = [ExtendedTransferManager.Cows];
                    outputResource1 = TransferManager.TransferReason.AnimalProducts;
                }
                else if (building.Info.name.Contains("Milking Parlour"))
                {
                    inputResource1 = [ExtendedTransferManager.Cows];
                    outputResource1 = ExtendedTransferManager.Milk;
                }
            }
            bool resourceChanged = false;
            var custom_buffers = CustomBuffersManager.GetCustomBuffer(m_InstanceID.Building);
            if (oldInputResource1.Length != 0 && inputResource1[0] != oldInputResource1[0])
            {
                custom_buffers.Set((int)oldInputResource1[0], 0);
                resourceChanged = true;
            }
            if (outputResource1 != oldOutputResource1)
            {
                custom_buffers.Set((int)oldOutputResource1, 0);
                resourceChanged = true;
            }
            if (oldOutputResource2 != TransferManager.TransferReason.None && outputResource2 != oldOutputResource2)
            {
                custom_buffers.Set((int)oldOutputResource2, 0);
                resourceChanged = true;
            }
            if (resourceChanged)
            {
                CustomBuffersManager.SetCustomBuffer(m_InstanceID.Building, custom_buffers);
            }
            m_extendedProcessingFacilityAI.m_inputResource1 = inputResource1;
            m_extendedProcessingFacilityAI.m_outputResource1 = outputResource1;
            m_extendedProcessingFacilityAI.m_outputResource2 = outputResource2;
        }

        private void OnOnOffChanged(UIComponent comp, bool value)
        {
            IsCityServiceEnabled = value;
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

        private void OnMovingPanelCloseClicked(UIComponent comp, UIMouseEventParameter p)
        {
            m_IsRelocating = false;
            ToolsModifierControl.GetTool<BuildingTool>().CancelRelocate();
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
            MovingPanel.Find<UILabel>("MovingLabel").text = LocaleFormatter.FormatGeneric("BUILDING_MOVING", base.buildingName);
            MovingPanel.Show();
        }

        public void TempShow(Vector3 worldPosition, InstanceID instanceID)
        {
            MovingPanel.Hide();
            WorldInfoPanel.Show<CityServiceWorldInfoPanel>(worldPosition, instanceID);
            ValueAnimator.Animate("Relocating", delegate (float val)
            {
                base.component.opacity = val;
            }, new AnimatedFloat(0f, 1f, 0.33f));
        }

        private void Update()
        {
            if (m_needResetTarget)
            {
                OnSetTarget();
            }
            if (m_IsRelocating)
            {
                BuildingTool currentTool = ToolsModifierControl.GetCurrentTool<BuildingTool>();
                if (currentTool != null && IsValidTarget() && currentTool.m_relocate != 0 && !MovingPanel.isVisible)
                {
                    MovingPanel.Show();
                    return;
                }
                if (!IsValidTarget() || (currentTool != null && currentTool.m_relocate == 0))
                {
                    ToolsModifierControl.mainToolbar.ResetLastTool();
                    MovingPanel.Hide();
                    m_IsRelocating = false;
                }
            }
            if (base.component.isVisible)
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
                MovingPanel.Hide();
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
            MovingPanel.Hide();
        }

        protected override void OnSetTarget()
        {
            m_needResetTarget = false;
            base.OnSetTarget();
            if (m_InstanceID.Type != InstanceType.Building || m_InstanceID.Building == 0)
            {
                return;
            }
            ushort building = m_InstanceID.Building;
            Building data = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building];
            m_playerBuildingAI = data.Info.GetAI() as PlayerBuildingAI;
            m_IndustryBuildingAI = data.Info.GetAI() as IndustryBuildingAI;
            m_extendedProcessingFacilityAI = m_IndustryBuildingAI as ExtendedProcessingFacilityAI;
            m_ShowHideRoutesButton.isVisible = CanBuildingHaveRoutes(building);
            int num = 0;
            m_VariationPanel.isVisible = false;
            if (m_playerBuildingAI != null)
            {
                m_playerBuildingAI.CountWorkPlaces(out var workPlaceCount, out var workPlaceCount2, out var workPlaceCount3, out var workPlaceCount4);
                num = workPlaceCount + workPlaceCount2 + workPlaceCount3 + workPlaceCount4;
            }
            if (m_IndustryBuildingAI != null && m_IndustryBuildingAI.GetVariations(out var variations) && variations.m_size > 1)
            {
                m_VariationPanel.isVisible = true;
                List<string> list = [];
                int selectedIndex = -1;
                for (int i = 0; i < variations.m_size; i++)
                {
                    string id = "FIELDVARIATION" + "_" + Singleton<SimulationManager>.instance.m_metaData.m_environment.ToUpper();
                    string empty = (Locale.Exists(id, variations.m_buffer[i].m_info.name) ? Locale.Get(id, variations.m_buffer[i].m_info.name) : ((!Locale.Exists("FIELDVARIATION", variations.m_buffer[i].m_info.name)) ? variations.m_buffer[i].m_info.GetUncheckedLocalizedTitle() : Locale.Get("FIELDVARIATION", variations.m_buffer[i].m_info.name)));
                    list.Add(empty);
                    if (m_IndustryBuildingAI.m_info.name == variations.m_buffer[i].m_info.name)
                    {
                        selectedIndex = i;
                    }
                }
                m_VariationDropdown.items = [.. list];
                m_VariationDropdown.selectedIndex = selectedIndex;
            }
            if(data.Info.name.Contains("Slaughter House"))
            {
                m_VariationPanel.isVisible = true;
                m_VariationDropdown.items = ["Cows", "Highland Cows", "Pigs", "Sheep"];
                var buffers = CustomBuffersManager.GetCustomBuffer(building);
                m_VariationDropdown.selectedIndex = buffers.m_animalVariationIndex;
            }
            if (data.Info.name.Contains("Milking Parlour"))
            {
                m_VariationPanel.isVisible = true;
                m_VariationDropdown.items = ["Cows", "Highland Cows", "Sheep"];
                var buffers = CustomBuffersManager.GetCustomBuffer(building);
                m_VariationDropdown.selectedIndex = buffers.m_animalVariationIndex;
            }
            m_workersInfoLabel.isVisible = num > 0;
            m_inputOutputSection.isVisible = true;
            m_inputSection.isVisible = true;
            m_outputSection.isVisible = true;
            m_inputOutputSection.isVisible = true;

            m_storageInput2.isVisible = false;
            m_storageOutput2.isVisible = false;

            string text2 = "";
            if (m_extendedProcessingFacilityAI.m_inputResource1.Length != 0)
            {
                m_input1Buffer.progressColor = IndustryWorldInfoPanel.instance.GetResourceColor(m_extendedProcessingFacilityAI.m_inputResource1[0]);
                text2 = Locale.Get("WAREHOUSEPANEL_RESOURCE", m_extendedProcessingFacilityAI.m_inputResource1[0].ToString()); 
                m_input1Label.text = text2;
                m_input1Sprite.atlas = GetResourceAtlas(m_extendedProcessingFacilityAI.m_inputResource1[0]);
                m_input1Sprite.spriteName = AtlasUtils.GetSpriteName(m_extendedProcessingFacilityAI.m_inputResource1[0]);
                m_arrow1.size = new Vector2(26f, 26f);
                m_arrow1.relativePosition = new Vector2(18f, 7f);
                m_arrow2.size = new Vector2(26f, 26f);
                m_arrow2.relativePosition = new Vector2(201f, 7f);
            }
            if (m_extendedProcessingFacilityAI.m_inputResource2.Length != 0)
            {
                m_inputOutputSection.size = new Vector2(484f, 95f);
                m_layout.size = new Vector2(405f, 95f);
                m_inputSection.size = new Vector2(200f, 90f);
                text2 = Locale.Get("WAREHOUSEPANEL_RESOURCE", m_extendedProcessingFacilityAI.m_inputResource2[0].ToString());
                m_input2Label.text = text2;
                m_input2Sprite.atlas = GetResourceAtlas(m_extendedProcessingFacilityAI.m_inputResource2[0]);
                m_input2Sprite.spriteName = AtlasUtils.GetSpriteName(m_extendedProcessingFacilityAI.m_inputResource2[0]);
                m_arrow1.size = new Vector2(56f, 26f);
                m_arrow1.relativePosition = new Vector2(18f, 18f);
                m_arrow2.size = new Vector2(56f, 26f);
                m_arrow2.relativePosition = new Vector2(201f, 18f);
                m_storageInput2.isVisible = true;
            }

            string text3 = "";
            if (m_extendedProcessingFacilityAI.m_outputResource1 != TransferManager.TransferReason.None)
            {
                m_storageOutput1.relativePosition = new Vector2(-1f, 28f);
                m_output1Buffer.progressColor = IndustryWorldInfoPanel.instance.GetResourceColor(m_extendedProcessingFacilityAI.m_outputResource1);
                text3 = Locale.Get("WAREHOUSEPANEL_RESOURCE", m_extendedProcessingFacilityAI.m_outputResource1.ToString());
                m_output1Label.text = text3;
                m_output1Sprite.atlas = GetResourceAtlas(m_extendedProcessingFacilityAI.m_outputResource1);
                m_output1Sprite.spriteName = AtlasUtils.GetSpriteName(m_extendedProcessingFacilityAI.m_outputResource1);
                m_arrow3.size = new Vector2(26f, 26f);
                m_arrow3.relativePosition = new Vector2(185f, 7f);
            }

            if (m_extendedProcessingFacilityAI.m_inputResource2.Length != 0 && m_extendedProcessingFacilityAI.m_outputResource2 == TransferManager.TransferReason.None)
            {
                m_storageOutput1.relativePosition = new Vector2(-1f, 28f);
                m_arrow3.relativePosition = new Vector2(185f, 28f);
            }

            string text4 = "";
            if (m_extendedProcessingFacilityAI.m_outputResource2 != TransferManager.TransferReason.None)
            {
                m_storageOutput1.relativePosition = new Vector2(-1f, 0f);
                m_inputOutputSection.size = new Vector2(484f, 95f);
                m_layout.size = new Vector2(405f, 95f);
                m_outputSection.size = new Vector2(200f, 90f);
                text4 = Locale.Get("WAREHOUSEPANEL_RESOURCE", m_extendedProcessingFacilityAI.m_outputResource2.ToString());
                m_output2Label.text = text4;
                m_output2Sprite.atlas = GetResourceAtlas(m_extendedProcessingFacilityAI.m_outputResource2);
                m_output2Sprite.spriteName = AtlasUtils.GetSpriteName(m_extendedProcessingFacilityAI.m_outputResource2);
                m_arrow3.size = new Vector2(56f, 26f);
                m_arrow3.relativePosition = new Vector2(185f, 18f);
                m_storageOutput2.isVisible = true;
            }

            string tooltip = StringUtils.SafeFormat(Locale.Get("INUDSTRYBUILDING_PROCESSINGTOOLTIP"), text2, text3, text4);
            m_arrow1.tooltip = tooltip;
            m_arrow2.tooltip = tooltip;
            m_arrow3.tooltip = tooltip;        
            
            m_ShowIndustryInfoButton.isVisible = true;
        }

        private bool CanBuildingHaveRoutes(ushort id)
        {
            if (Singleton<BuildingManager>.instance.m_buildings.m_buffer[id].Info.m_circular)
            {
                return false;
            }
            if (Singleton<BuildingManager>.instance.m_buildings.m_buffer[id].Info.m_placementMode == BuildingInfo.PlacementMode.OnWater)
            {
                return false;
            }
            return true;
        }

        protected override void UpdateBindings()
        {
            base.UpdateBindings();
            if (Singleton<BuildingManager>.exists && m_InstanceID.Type == InstanceType.Building && m_InstanceID.Building != 0)
            {
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
                m_BuildingDesc.text = info.GetLocalizedDescriptionShortWithEnviroment();
                m_BuildingInfo.text = buildingAI.GetLocalizedStats(building, ref instance.m_buildings.m_buffer[building]);
                m_BuildingInfo.isVisible = m_BuildingInfo.text != string.Empty;
                ItemClass.Service service = info.GetService();
                if (service != ItemClass.Service.None)
                {
                    string text = ColossalFramework.Utils.GetNameByValue(service, "Game");
                    m_BuildingService.spriteName = "UIFilterProcessingBuildings";
                    m_BuildingService.tooltip = Locale.Get("MAIN_TOOL", text);
                }
                m_BuildingService.isVisible = service != ItemClass.Service.None;
                m_MoveButton.isEnabled = buildingAI != null && buildingAI.CanBeRelocated(building, ref instance.m_buildings.m_buffer[building]);
                if ((building2.m_flags & Building.Flags.Collapsed) != Building.Flags.None)
                {
                    m_RebuildButton.tooltip = ((!IsDisasterServiceRequired()) ? LocaleFormatter.FormatCost(buildingAI.GetRelocationCost(), isDistanceBased: false) : Locale.Get("CITYSERVICE_TOOLTIP_DISASTERSERVICEREQUIRED"));
                    m_RebuildButton.isVisible = Singleton<LoadingManager>.instance.SupportsExpansion(Expansion.NaturalDisasters);
                    m_RebuildButton.isEnabled = CanRebuild();
                    m_ActionPanel.isVisible = false;
                    m_VariationDropdown.isEnabled = false;
                    m_VariationDropdown.tooltip = Locale.Get("VARIATIONBUILDING_COLLAPSED_TOOLTIP");
                }
                else if (building2.m_fireIntensity > 0)
                {
                    m_ActionPanel.isVisible = false;
                    m_VariationDropdown.isEnabled = false;
                    m_VariationDropdown.tooltip = Locale.Get("VARIATIONBUILDING_ONFIRE_TOOLTIP");
                }
                else
                {
                    m_RebuildButton.isVisible = false;
                    m_ActionPanel.isVisible = true;
                    m_VariationDropdown.isEnabled = true;
                    m_VariationDropdown.tooltip = string.Empty;
                }
                if (m_workersInfoLabel.isVisible && m_playerBuildingAI != null)
                {
                    UpdateWorkers(ref instance.m_buildings.m_buffer[building]);
                }

                var custom_buffers = CustomBuffersManager.GetCustomBuffer(m_InstanceID.Building);

                if (m_extendedProcessingFacilityAI.m_inputResource1.Length != 0)
                {
                    int inputBufferValue1 = (int)custom_buffers.Get((int)m_extendedProcessingFacilityAI.m_inputResource1[0]);
                    int inputBufferSize1 = m_extendedProcessingFacilityAI.GetInputBufferSize1(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building]);
                    m_input1Buffer.value = IndustryWorldInfoPanel.SafelyNormalize(inputBufferValue1, inputBufferSize1);
                    m_storageInput1.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), IndustryWorldInfoPanel.FormatResource((uint)inputBufferValue1), IndustryWorldInfoPanel.FormatResourceWithUnit((uint)inputBufferSize1, m_extendedProcessingFacilityAI.m_inputResource1[0]));
                }
  
                if (m_extendedProcessingFacilityAI.m_inputResource2.Length != 0)
                {
                    int inputBufferValue2 = (int)custom_buffers.Get((int)m_extendedProcessingFacilityAI.m_inputResource2[0]);
                    int inputBufferSize2 = m_extendedProcessingFacilityAI.GetInputBufferSize2(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building]);
                    m_input2Buffer.value = IndustryWorldInfoPanel.SafelyNormalize(inputBufferValue2, inputBufferSize2);
                    m_storageInput2.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), IndustryWorldInfoPanel.FormatResource((uint)inputBufferValue2), IndustryWorldInfoPanel.FormatResourceWithUnit((uint)inputBufferSize2, m_extendedProcessingFacilityAI.m_inputResource2[0]));
                }

                if (m_extendedProcessingFacilityAI.m_outputResource1 != TransferManager.TransferReason.None)
                {
                    int outputBufferValue1 = (int)custom_buffers.Get((int)m_extendedProcessingFacilityAI.m_outputResource1);
                    int outputBufferSize1 = m_extendedProcessingFacilityAI.GetOutputBufferSize1(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building]);
                    m_output1Buffer.value = IndustryWorldInfoPanel.SafelyNormalize(outputBufferValue1, outputBufferSize1);
                    m_storageOutput1.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), IndustryWorldInfoPanel.FormatResource((uint)outputBufferValue1), IndustryWorldInfoPanel.FormatResourceWithUnit((uint)outputBufferSize1, m_extendedProcessingFacilityAI.m_outputResource1));
                }

                if (m_extendedProcessingFacilityAI.m_outputResource2 != TransferManager.TransferReason.None)
                {
                    int outputBufferValue2 = (int)custom_buffers.Get((int)m_extendedProcessingFacilityAI.m_outputResource2);
                    int outputBufferSize2 = m_extendedProcessingFacilityAI.GetOutputBufferSize1(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building]);
                    m_output2Buffer.value = IndustryWorldInfoPanel.SafelyNormalize(outputBufferValue2, outputBufferSize2);
                    m_storageOutput2.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), IndustryWorldInfoPanel.FormatResource((uint)outputBufferValue2), IndustryWorldInfoPanel.FormatResourceWithUnit((uint)outputBufferSize2, m_extendedProcessingFacilityAI.m_outputResource2));
                }

                base.component.size = m_wrapper.size;
                m_mainBottom.width = m_wrapper.width;
                m_BuildingInfo.width = m_right.width;
            }
            m_BudgetButton.isEnabled = ToolsModifierControl.IsUnlocked(UnlockManager.Feature.Economy);
            m_Parkbuttons.isVisible = true;
        }

        public void OnBudgetClicked()
        {
            if (ToolsModifierControl.IsUnlocked(UnlockManager.Feature.Economy))
            {
                ToolsModifierControl.mainToolbar.ShowEconomyPanel(1);
                WorldInfoPanel.Hide<CityServiceWorldInfoPanel>();
            }
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

        private bool IsDisasterServiceRequired()
        {
            ushort building = m_InstanceID.Building;
            if (building != 0)
            {
                return Singleton<BuildingManager>.instance.m_buildings.m_buffer[building].m_levelUpProgress != byte.MaxValue;
            }
            return false;
        }

        public void OpenParkInfoPanel()
        {
            ushort building = m_InstanceID.Building;
            Vector3 position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building].m_position;
            WorldInfoPanel.Show<ParkWorldInfoPanel>(position, new InstanceID
            {
                Park = Singleton<DistrictManager>.instance.GetPark(position)
            });
            OnCloseButton();
        }

        public void OpenIndustryInfoPanel()
        {
            ushort building = m_InstanceID.Building;
            Vector3 position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building].m_position;
            WorldInfoPanel.Show<IndustryWorldInfoPanel>(position, new InstanceID
            {
                Park = Singleton<DistrictManager>.instance.GetPark(position)
            });
            OnCloseButton();
        }

        private void UpdateWorkers(ref Building building)
        {
            if (!Singleton<CitizenManager>.exists || !(m_playerBuildingAI != null))
            {
                return;
            }
            m_playerBuildingAI.CountWorkPlaces(out var workPlaceCount, out var workPlaceCount2, out var workPlaceCount3, out var workPlaceCount4);
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint num = building.m_citizenUnits;
            int num2 = 0;
            int num3 = 0;
            int num5 = 0;
            int num6 = 0;
            int num7 = 0;
            int num8 = 0;
            int num4 = workPlaceCount + workPlaceCount2 + workPlaceCount3 + workPlaceCount4;
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
                                    num5++;
                                    break;
                                case Citizen.Education.OneSchool:
                                    num6++;
                                    break;
                                case Citizen.Education.TwoSchools:
                                    num7++;
                                    break;
                                case Citizen.Education.ThreeSchools:
                                    num8++;
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
            int num9 = 0;
            int num10 = workPlaceCount - num5;
            if (num6 > workPlaceCount2)
            {
                num9 += Mathf.Max(0, Mathf.Min(num10, num6 - workPlaceCount2));
            }
            num10 += workPlaceCount2 - num6;
            if (num7 > workPlaceCount3)
            {
                num9 += Mathf.Max(0, Mathf.Min(num10, num7 - workPlaceCount3));
            }
            num10 += workPlaceCount3 - num7;
            if (num8 > workPlaceCount4)
            {
                num9 += Mathf.Max(0, Mathf.Min(num10, num8 - workPlaceCount4));
            }
            string format = Locale.Get((num9 != 1) ? "ZONEDBUILDING_OVEREDUCATEDWORKERS" : "ZONEDBUILDING_OVEREDUCATEDWORKER");
            m_OverWorkSituation.text = StringUtils.SafeFormat(format, num9);
            m_OverWorkSituation.isVisible = num9 > 0;
            m_UneducatedPlaces.text = workPlaceCount.ToString();
            m_EducatedPlaces.text = workPlaceCount2.ToString();
            m_WellEducatedPlaces.text = workPlaceCount3.ToString();
            m_HighlyEducatedPlaces.text = workPlaceCount4.ToString();
            m_UneducatedWorkers.text = num5.ToString();
            m_EducatedWorkers.text = num6.ToString();
            m_WellEducatedWorkers.text = num7.ToString();
            m_HighlyEducatedWorkers.text = num8.ToString();
            m_JobsAvailLegend.text = Mathf.Max(0, num4 - (num5 + num6 + num7 + num8)).ToString();
            int num11 = GetValue(workPlaceCount, num4);
            int value = GetValue(workPlaceCount2, num4);
            int value2 = GetValue(workPlaceCount3, num4);
            int value3 = GetValue(workPlaceCount4, num4);
            int num12 = num11 + value + value2 + value3;
            if (num12 != 0 && num12 != 100)
            {
                num11 = 100 - (value + value2 + value3);
            }
            m_WorkPlacesEducationChart.SetValues(num11, value, value2, value3);
            int value4 = GetValue(num5, num4);
            int value5 = GetValue(num6, num4);
            int value6 = GetValue(num7, num4);
            int value7 = GetValue(num8, num4);
            int num14 = value4 + value5 + value6 + value7;
            int num13 = 100 - num14;
            m_WorkersEducationChart.SetValues(value4, value5, value6, value7, num13);
            m_workersInfoLabel.text = StringUtils.SafeFormat(Locale.Get("ZONEDBUILDING_WORKERS"), num3, num4);
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

        private void MakeStorageInput()
        {
            GameObject gameObject = Instantiate(m_storageInput1.gameObject, m_inputSection.transform, false);
            m_storageInput2 = gameObject.GetComponent<UIPanel>();
            m_storageInput2.name = "StorageInput2";
            m_storageInput2.anchor = UIAnchorStyle.None;
            m_storageInput2.relativePosition = new Vector3(m_storageInput1.relativePosition.x, 50f);
            m_storageInput2.autoLayout = false;
            m_storageInput2.size = new Vector2(160f, 35f);
            m_storageInput2.backgroundSprite = "";

            m_input2Sprite = gameObject.transform.Find("ResourceIconInput").GetComponent<UISprite>();
            m_input2Sprite.name = "ResourceIconInput2";
            m_input2Sprite.relativePosition = new Vector3(2f, 2f);
            m_input2Sprite.size = new Vector2(30f, 30f);

            m_input2Buffer = gameObject.transform.Find("InputBuffer").GetComponent<UIProgressBar>();
            m_input2Buffer.name = "Input2Buffer";
            m_input2Buffer.relativePosition = new Vector3(62f, -62f);
            m_input2Buffer.size = new Vector2(35f, 160f);
            m_input2Buffer.color = m_input1Buffer.color;

            m_input2Label = gameObject.transform.Find("StorageInputLabel").GetComponent<UILabel>();
            m_input2Label.name = "StorageInput2Label";
            m_input2Label.relativePosition = new Vector3(34f, -1.5f);
            m_input2Label.textScale = 0.8125f;
            m_input2Label.textAlignment = UIHorizontalAlignment.Left;
        }

        private void MakeStorageOutput()
        {
            GameObject gameObject = Instantiate(m_storageOutput1.gameObject, m_outputSection.transform, false);
            m_storageOutput2 = gameObject.GetComponent<UIPanel>();
            m_storageOutput2.name = "StorageOutput2";
            m_storageOutput2.anchor = UIAnchorStyle.None;
            m_storageOutput2.relativePosition = new Vector3(m_storageOutput1.relativePosition.x, 50f);
            m_storageOutput2.autoLayout = false;
            m_storageOutput2.size = new Vector2(160f, 35f);
            m_storageOutput2.backgroundSprite = "";

            m_output2Sprite = gameObject.transform.Find("ResourceIconOutput").GetComponent<UISprite>();
            m_output2Sprite.name = "ResourceIconOutput2";
            m_output2Sprite.relativePosition = new Vector3(2f, 2f);
            m_output2Sprite.size = new Vector2(30f, 30f);

            m_output2Buffer = gameObject.transform.Find("OutputBuffer").GetComponent<UIProgressBar>();
            m_output2Buffer.name = "Output2Buffer";
            m_output2Buffer.relativePosition = new Vector3(62f, -62f);
            m_output2Buffer.size = new Vector2(35f, 160f);
            m_output2Buffer.color = m_output1Buffer.color;

            m_output2Label = gameObject.transform.Find("StorageOutputLabel").GetComponent<UILabel>();
            m_output2Label.name = "StorageOutput2Label";
            m_output2Label.relativePosition = new Vector3(34f, -1.5f);
            m_output2Label.textScale = 0.8125f;
            m_output2Label.textAlignment = UIHorizontalAlignment.Left;
        }

        private UITextureAtlas GetResourceAtlas(TransferManager.TransferReason reason)
        {
            if (reason != TransferManager.TransferReason.None)
            {
                if (reason >= ExtendedTransferManager.MealsDeliveryLow)
                {
                    return TextureUtils.GetAtlas("MoreTransferReasonsAtlas");
                }
            }
            return UITextures.InGameAtlas;
        }
    }

}
