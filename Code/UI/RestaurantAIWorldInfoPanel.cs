using System;
using System.Collections;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using UnityEngine;
using IndustriesMeetsSunsetHarbor.AI;
using MoreTransferReasons;
using IndustriesMeetsSunsetHarbor.Managers;
using IndustriesMeetsSunsetHarbor.Utils;

namespace IndustriesMeetsSunsetHarbor.UI
{
    public class RestaurantAIWorldInfoPanel : BuildingWorldInfoPanel
    {
        private UILabel m_status;

        private UIButton m_RebuildButton;

        private UICheckBox m_OnOff;

        private UIPanel m_horizontalLine;

        private UIPanel m_inputContainer;

        private UITemplateList<UIPanel> m_inputs;

        private int m_inputResourceCount;

        private UILabel m_productLabel;

        private UIProgressBar m_productBuffer;

        private UISlider m_productionSlider;

        private UILabel m_productionRateLabel;

        private UILabel m_workplaces;

        private UILabel m_generatedInfo;

        private bool m_IsRelocating;

        private UIButton m_MoveButton;

        private UIPanel m_productStorage;

        private UILabel m_Upkeep;

        private UILabel m_income;

        private UILabel m_expenses;

        private UIComponent m_MovingPanel;

        private bool IsBread = true;

        private UISprite m_outputSprite;

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

        protected override void Start()
        {
            m_status = Find<UILabel>("Status");
            base.Start();
            m_generatedInfo = Find<UILabel>("LabelInfo");
            m_horizontalLine = Find<UIPanel>("HorizontalLinePanel");
            m_inputContainer = Find<UIPanel>("LayoutPanel");
            m_inputs = new UITemplateList<UIPanel>(m_inputContainer, "UniqueFactoryInputResource");
            m_productLabel = Find<UILabel>("ProductLabel");
            m_productBuffer = Find<UIProgressBar>("ProductBuffer");
            m_productionSlider = Find<UISlider>("ProductionSlider");
            m_productionSlider.eventValueChanged += OnProductionRateChanged;
            m_productionRateLabel = Find<UILabel>("LabelProductionRate");
            m_workplaces = Find<UILabel>("LabelWorkplaces");
            m_MoveButton = Find<UIButton>("RelocateAction");
            m_RebuildButton = Find<UIButton>("RebuildButton");
            m_OnOff = Find<UICheckBox>("On/Off");
            m_OnOff.eventCheckChanged += OnOnOffChanged;
            m_productStorage = Find<UIPanel>("ProductStorage");
            m_Upkeep = Find<UILabel>("Upkeep");
            m_income = Find<UILabel>("IncomeLabel");
            m_expenses = Find<UILabel>("ExpensesLabel");
            m_outputSprite = Find<UISprite>("LuxuryProductIcon");
            m_outputSprite.atlas = TextureUtils.GetAtlas("RestaurantAtlas");
            m_outputSprite.spriteName = "Meal";
        }

        private void OnProductionRateChanged(UIComponent component, float value)
        {
            m_productionRateLabel.text = value + "%";
            if (Singleton<SimulationManager>.exists && m_InstanceID.Building != 0)
            {
                Singleton<SimulationManager>.instance.AddAction(SetProductionRate(m_InstanceID.Building, Mathf.RoundToInt(value)));
            }
        }

        private IEnumerator SetProductionRate(ushort id, int value)
        {
            if (Singleton<BuildingManager>.exists)
            {
                BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[id].Info;
                info.m_buildingAI.SetProductionRate(id, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[id], (byte)value);
            }
            yield return 0;
        }

        protected override void OnSetTarget()
        {
            base.OnSetTarget();
            RestaurantAI restaurantAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI as RestaurantAI;
            m_inputResourceCount = GetInputResourceCount(restaurantAI);
            m_inputs.SetItemCount(m_inputResourceCount);
            m_horizontalLine.width = m_inputContainer.width;
            m_productLabel.text = (!Locale.Exists("UNIQUEFACTORYPANEL_PRODUCT", restaurantAI.m_info.name)) ? Locale.Get("UNIQUEFACTORYPANEL_LUXURYGOODS") : Locale.Get("UNIQUEFACTORYPANEL_PRODUCT", restaurantAI.m_info.name);
            m_productBuffer.progressColor = Color.Lerp(Color.grey, Color.black, 0.2f);
            for (int i = 0; i < m_inputResourceCount; i++)
            {
                UILabel uILabel = m_inputs.items[i].Find<UILabel>("ResourceLabel");
                UISprite uISprite = m_inputs.items[i].Find<UISprite>("ResourceIcon");
                var game_atlas = uISprite.atlas;
                uILabel.text = GetInputResourceName(i);
                if (!IsBread)
                {
                    if(i < 2)
                    {
                        uISprite.atlas = TextureUtils.GetAtlas("RestaurantAtlas");
                        uISprite.spriteName = AtlasUtils.ResourceSpriteName(GetExtendedInputResource(i));
                    }
                    else
                    {
                        uISprite.atlas = game_atlas;
                        uISprite.spriteName = IndustryWorldInfoPanel.ResourceSpriteName(GetInputResource(i));
                    }
                }
                else
                {
                    if(i < 3)
                    {
                        uISprite.atlas = TextureUtils.GetAtlas("RestaurantAtlas");
                        uISprite.spriteName = AtlasUtils.ResourceSpriteName(GetExtendedInputResource(i));
                    }
                    else
                    {
                        uISprite.atlas = game_atlas;
                        uISprite.spriteName = IndustryWorldInfoPanel.ResourceSpriteName(GetInputResource(i));
                    }
                }
            }
            byte productionRate = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_productionRate;
            if (productionRate > 0)
            {
                m_productionSlider.value = (int)productionRate;
            }
        }

        private int GetInputResourceCount(RestaurantAI ai)
        {
            if (ai.m_inputResource3 == ExtendedTransferManager.TransferReason.None)
            {
                if (ai.m_inputResource4 == TransferManager.TransferReason.None)
                {
                    return 2;
                }
                if (ai.m_inputResource5 == TransferManager.TransferReason.None)
                {
                    IsBread = false;
                    return 3;
                }
                if (ai.m_inputResource6 == TransferManager.TransferReason.None)
                {
                    IsBread = false;
                    return 4;
                }
                if (ai.m_inputResource7 == TransferManager.TransferReason.None)
                {
                    IsBread = false;
                    return 5;
                }
                IsBread = false;
                return 6;
            }
            else
            {
                if (ai.m_inputResource4 == TransferManager.TransferReason.None)
                {
                    return 3;
                }
                if (ai.m_inputResource5 == TransferManager.TransferReason.None)
                {
                    return 4;
                }
                if (ai.m_inputResource6 == TransferManager.TransferReason.None)
                {
                    return 5;
                }
                if (ai.m_inputResource7 == TransferManager.TransferReason.None)
                {
                    return 6;
                }
                return 7;
            }
        }

        protected override void UpdateBindings()
        {
            base.UpdateBindings();
            ushort building = m_InstanceID.Building;
            BuildingManager instance = Singleton<BuildingManager>.instance;
            Building building2 = instance.m_buildings.m_buffer[building];
            RestaurantAI restaurantAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI as RestaurantAI;
            m_Upkeep.text = LocaleFormatter.FormatUpkeep(restaurantAI.GetResourceRate(building, ref instance.m_buildings.m_buffer[building], EconomyManager.Resource.Maintenance), isDistanceBased: false);
            m_status.text = restaurantAI.GetLocalizedStatus(building, ref instance.m_buildings.m_buffer[m_InstanceID.Building]);
            int customBuffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_customBuffer1;
            int outputBufferSize = restaurantAI.GetOutputBufferSize();
            m_productBuffer.value = IndustryWorldInfoPanel.SafelyNormalize(customBuffer, outputBufferSize);
            m_productStorage.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), IndustryWorldInfoPanel.FormatResource((uint)customBuffer), IndustryWorldInfoPanel.FormatResourceWithUnit((uint)outputBufferSize, TransferManager.TransferReason.None));
            for (int i = 0; i < m_inputResourceCount; i++)
            {
                UIProgressBar uIProgressBar = m_inputs.items[i].Find<UIProgressBar>("ResourceBuffer");
                uIProgressBar.value = GetInputBufferProgress(i, out var amount, out var capacity);
                uIProgressBar.progressColor = IndustryWorldInfoPanel.instance.GetResourceColor(GetInputResource(i));
                TransferManager.TransferReason inputResource = GetInputResource(i);
                string text = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), IndustryWorldInfoPanel.FormatResource((uint)amount), IndustryWorldInfoPanel.FormatResourceWithUnit((uint)capacity, inputResource));
                text = (uIProgressBar.tooltip = text + Environment.NewLine + Environment.NewLine + StringUtils.SafeFormat(Locale.Get("RESOURCEDESCRIPTION", inputResource.ToString())));
            }
            m_workplaces.text = StringUtils.SafeFormat(Locale.Get("UNIQUEFACTORYPANEL_WORKPLACES"), (restaurantAI.m_workPlaceCount0 + restaurantAI.m_workPlaceCount1 + restaurantAI.m_workPlaceCount2 + restaurantAI.m_workPlaceCount3).ToString());
            if ((building2.m_flags & Building.Flags.Collapsed) != 0)
            {
                m_RebuildButton.tooltip = ((!IsDisasterServiceRequired()) ? LocaleFormatter.FormatCost(restaurantAI.GetRelocationCost(), isDistanceBased: false) : Locale.Get("CITYSERVICE_TOOLTIP_DISASTERSERVICEREQUIRED"));
                m_RebuildButton.isVisible = Singleton<LoadingManager>.instance.SupportsExpansion(Expansion.NaturalDisasters);
                m_RebuildButton.isEnabled = CanRebuild();
                m_MoveButton.isVisible = false;
            }
            else
            {
                m_RebuildButton.isVisible = false;
                m_MoveButton.isVisible = true;
            }
            m_generatedInfo.text = restaurantAI.GetLocalizedStats(building, ref instance.m_buildings.m_buffer[building]);
            int priceRate1 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_health * restaurantAI.m_inputRate1 * 16 / 100;
            long price1 = priceRate1 * 10000 / 10000;
            int priceRate2 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_health * restaurantAI.m_inputRate2 * 16 / 100;
            long price2 = priceRate2 * 10000 / 10000;
            int priceRate3 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_health * restaurantAI.m_inputRate3 * 16 / 100;
            long price3 = priceRate3 * 10000 / 10000;
            int priceRate4 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_health * restaurantAI.m_inputRate4 * 16 / 100;
            long price4 = priceRate4 * IndustryBuildingAI.GetResourcePrice(restaurantAI.m_inputResource4) / 10000;
            int priceRate5 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_health * restaurantAI.m_inputRate5 * 16 / 100;
            long price5 = priceRate5 * IndustryBuildingAI.GetResourcePrice(restaurantAI.m_inputResource5) / 10000;
            int priceRate6 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_health * restaurantAI.m_inputRate6 * 16 / 100;
            long price6 = priceRate6 * IndustryBuildingAI.GetResourcePrice(restaurantAI.m_inputResource6) / 10000;
            int priceRate7 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_health * restaurantAI.m_inputRate7 * 16 / 100;
            long price7 = priceRate7 * IndustryBuildingAI.GetResourcePrice(restaurantAI.m_inputResource7) / 10000;
            m_expenses.text = (price1 + price2 + price3 + price4 + price5 + price6 + price7).ToString(Settings.moneyFormatNoCents, LocaleManager.cultureInfo);
            int num9 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_education3 * restaurantAI.m_outputRate * 16 / 100;
            long num10 = num9 * 10000 / 10000;
            m_income.text = num10.ToString(Settings.moneyFormatNoCents, LocaleManager.cultureInfo);
            if (Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_productionRate > 0)
            {
                m_productionSlider.isEnabled = true;
                m_productionSlider.tooltip = string.Empty;
            }
            else
            {
                m_productionSlider.isEnabled = false;
                m_productionSlider.tooltip = Locale.Get("UNIQUEFACTORYPANEL_SLIDERDISABLEDTOOLTIP");
            }
        }

        private float GetInputBufferProgress(int resourceIndex, out int amount, out int capacity)
        {
            var custom_buffers = BuildingCustomBuffersManager.GetCustomBuffer(m_InstanceID.Building);
            RestaurantAI restaurantAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI as RestaurantAI;
            amount = 0;
            capacity = 0;
            if(!IsBread)
            {
                switch (resourceIndex)
                {
                    case 0:
                        amount = custom_buffers.m_customBuffer1;
                        capacity = restaurantAI.GetInputBufferSize1();
                        break;
                    case 1:
                        amount = custom_buffers.m_customBuffer2;
                        capacity = restaurantAI.GetInputBufferSize2();
                        break;
                    case 2:
                        amount = custom_buffers.m_customBuffer4;
                        capacity = restaurantAI.GetInputBufferSize4();
                        break;
                    case 3:
                        amount = custom_buffers.m_customBuffer5;
                        capacity = restaurantAI.GetInputBufferSize5();
                        break;
                    case 4:
                        amount = custom_buffers.m_customBuffer6;
                        capacity = restaurantAI.GetInputBufferSize6();
                        break;
                    case 5:
                        amount = custom_buffers.m_customBuffer7;
                        capacity = restaurantAI.GetInputBufferSize7();
                        break;
                }
            }
            else
            {
                switch (resourceIndex)
                {
                    case 0:
                        amount = custom_buffers.m_customBuffer1;
                        capacity = restaurantAI.GetInputBufferSize1();
                        break;
                    case 1:
                        amount = custom_buffers.m_customBuffer2;
                        capacity = restaurantAI.GetInputBufferSize2();
                        break;
                    case 2:
                        amount = custom_buffers.m_customBuffer3;
                        capacity = restaurantAI.GetInputBufferSize3();
                        break;
                    case 3:
                        amount = custom_buffers.m_customBuffer4;
                        capacity = restaurantAI.GetInputBufferSize4();
                        break;
                    case 4:
                        amount = custom_buffers.m_customBuffer5;
                        capacity = restaurantAI.GetInputBufferSize4();
                        break;
                    case 5:
                        amount = custom_buffers.m_customBuffer6;
                        capacity = restaurantAI.GetInputBufferSize5();
                        break;
                    case 6:
                        amount = custom_buffers.m_customBuffer7;
                        capacity = restaurantAI.GetInputBufferSize6();
                        break;
                }
            }
            
            return IndustryWorldInfoPanel.SafelyNormalize(amount, capacity);
        }

        private string GetInputResourceName(int resourceIndex)
        {
            RestaurantAI restaurantAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI as RestaurantAI;
            string key = "N/A";
            if(!IsBread)
            {
                switch (resourceIndex)
                {
                    case 0:
                        key = restaurantAI.m_inputResource1.ToString();
                        break;
                    case 1:
                        key = restaurantAI.m_inputResource2.ToString();
                        break;
                    case 2:
                        key = restaurantAI.m_inputResource4.ToString();
                        break;
                    case 3:
                        key = restaurantAI.m_inputResource5.ToString();
                        break;
                    case 4:
                        key = restaurantAI.m_inputResource6.ToString();
                        break;
                    case 5:
                        key = restaurantAI.m_inputResource7.ToString();
                        break;
                }
                if (resourceIndex < 2)
                {
                    return key;
                }
                return Locale.Get("WAREHOUSEPANEL_RESOURCE", key);
            }
            else
            {
                switch (resourceIndex)
                {
                    case 0:
                        key = restaurantAI.m_inputResource1.ToString();
                        break;
                    case 1:
                        key = restaurantAI.m_inputResource2.ToString();
                        break;
                    case 2:
                        key = restaurantAI.m_inputResource3.ToString();
                        break;
                    case 3:
                        key = restaurantAI.m_inputResource4.ToString();
                        break;
                    case 4:
                        key = restaurantAI.m_inputResource5.ToString();
                        break;
                    case 5:
                        key = restaurantAI.m_inputResource6.ToString();
                        break;
                    case 6:
                        key = restaurantAI.m_inputResource7.ToString();
                        break;
                }
                if (resourceIndex < 3)
                {
                    return key;
                }
                return Locale.Get("WAREHOUSEPANEL_RESOURCE", key);
            }
           
        }

        private TransferManager.TransferReason GetInputResource(int resourceIndex)
        {
            RestaurantAI restaurantAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI as RestaurantAI;
            if(!IsBread)
            {
                return resourceIndex switch
                {
                    2 => restaurantAI.m_inputResource4,
                    3 => restaurantAI.m_inputResource5,
                    4 => restaurantAI.m_inputResource6,
                    5 => restaurantAI.m_inputResource7,
                    _ => TransferManager.TransferReason.None,
                };
            }
            else
            {
                return resourceIndex switch
                {
                    3 => restaurantAI.m_inputResource4,
                    4 => restaurantAI.m_inputResource5,
                    5 => restaurantAI.m_inputResource6,
                    6 => restaurantAI.m_inputResource7,
                    _ => TransferManager.TransferReason.None,
                };
            }

            
        }

        private ExtendedTransferManager.TransferReason GetExtendedInputResource(int resourceIndex)
        {
            RestaurantAI restaurantAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI as RestaurantAI;
            return resourceIndex switch
            {
                0 => restaurantAI.m_inputResource1,
                1 => restaurantAI.m_inputResource2,
                2 => restaurantAI.m_inputResource3,
                _ => ExtendedTransferManager.TransferReason.None,
            };

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
            Show<NewUniqueFactoryWorldInfoPanel>(worldPosition, instanceID);
            ValueAnimator.Animate("Relocating", delegate (float val)
            {
                base.component.opacity = val;
            }, new AnimatedFloat(0f, 1f, 0.33f));
        }

        private void Update()
        {
            if (m_IsRelocating)
            {
                BuildingTool currentTool = GetCurrentTool<BuildingTool>();
                if (currentTool != null && IsValidTarget() && currentTool.m_relocate != 0 && !movingPanel.isVisible)
                {
                    movingPanel.Show();
                    return;
                }
                if (!IsValidTarget() || (currentTool != null && currentTool.m_relocate == 0))
                {
                    mainToolbar.ResetLastTool();
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
                movingPanel.Hide();
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
            movingPanel.Hide();
        }
    }
}