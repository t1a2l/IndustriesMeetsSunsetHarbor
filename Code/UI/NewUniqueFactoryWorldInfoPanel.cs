using System;
using System.Collections;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using UnityEngine;
using IndustriesMeetsSunsetHarbor.AI;
using MoreTransferReasons;
using IndustriesMeetsSunsetHarbor.Utils;

namespace IndustriesMeetsSunsetHarbor.UI
{
    public class NewUniqueFactoryWorldInfoPanel : BuildingWorldInfoPanel
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
            NewUniqueFactoryAI newUniqueFactoryAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI as NewUniqueFactoryAI;
            m_inputResourceCount = GetInputResourceCount(newUniqueFactoryAI);
            m_inputs.SetItemCount(m_inputResourceCount);
            m_horizontalLine.width = m_inputContainer.width;
            m_productLabel.text = (!Locale.Exists("UNIQUEFACTORYPANEL_PRODUCT", newUniqueFactoryAI.m_info.name)) ? Locale.Get("UNIQUEFACTORYPANEL_LUXURYGOODS") : Locale.Get("UNIQUEFACTORYPANEL_PRODUCT", newUniqueFactoryAI.m_info.name);
            m_productBuffer.progressColor = Color.Lerp(Color.grey, Color.black, 0.2f);
            for (int i = 0; i < m_inputResourceCount; i++)
            {
                UILabel uILabel = m_inputs.items[i].Find<UILabel>("ResourceLabel");
                UISprite uISprite = m_inputs.items[i].Find<UISprite>("ResourceIcon");
                var game_atlas = uISprite.atlas;
                uILabel.text = GetInputResourceName(i);
                if (uILabel.text == "Bread")
                {
                    uISprite.atlas = TextureUtils.GetAtlas("RestaurantAtlas");
                    uISprite.spriteName = uILabel.text;
                }
                else
                {
                    uISprite.atlas = game_atlas;
                    uISprite.spriteName = IndustryWorldInfoPanel.ResourceSpriteName(GetInputResource(i));
                }
            }
            byte productionRate = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_productionRate;
            if (productionRate > 0)
            {
                m_productionSlider.value = (int)productionRate;
            }
            UISprite m_outputSprite = m_productStorage.Find<UISprite>("LuxuryProductIcon");
            m_outputSprite.atlas = TextureUtils.GetAtlas("RestaurantAtlas");
            UILabel m_outputLabel = m_productStorage.Find<UILabel>("ProductLabel");
            m_outputSprite.spriteName = m_outputLabel.text;
        }

        private int GetInputResourceCount(NewUniqueFactoryAI ai)
        {
            if (ai.m_inputResource1 == TransferManager.TransferReason.None)
            {
                return 0;
            }
            if (ai.m_inputResource2 == TransferManager.TransferReason.None)
            {
                return 1;
            }
            if (ai.m_inputResource3 == TransferManager.TransferReason.None)
            {
                return 2;
            }
            if (ai.m_inputResource4 == ExtendedTransferManager.TransferReason.None)
            {
                return 3;
            }
            return 4;
        }

        protected override void UpdateBindings()
        {
            base.UpdateBindings();
            ushort building = m_InstanceID.Building;
            BuildingManager instance = Singleton<BuildingManager>.instance;
            Building building2 = instance.m_buildings.m_buffer[building];
            NewUniqueFactoryAI newUniqueFactoryAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI as NewUniqueFactoryAI;
            m_Upkeep.text = LocaleFormatter.FormatUpkeep(newUniqueFactoryAI.GetResourceRate(building, ref instance.m_buildings.m_buffer[building], EconomyManager.Resource.Maintenance), isDistanceBased: false);
            m_status.text = newUniqueFactoryAI.GetLocalizedStatus(building, ref instance.m_buildings.m_buffer[m_InstanceID.Building]);
            int customBuffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_customBuffer1;
            int outputBufferSize = newUniqueFactoryAI.GetOutputBufferSize(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building]);
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
            m_workplaces.text = StringUtils.SafeFormat(Locale.Get("UNIQUEFACTORYPANEL_WORKPLACES"), (newUniqueFactoryAI.m_workPlaceCount0 + newUniqueFactoryAI.m_workPlaceCount1 + newUniqueFactoryAI.m_workPlaceCount2 + newUniqueFactoryAI.m_workPlaceCount3).ToString());
            if ((building2.m_flags & Building.Flags.Collapsed) != 0)
            {
                m_RebuildButton.tooltip = ((!IsDisasterServiceRequired()) ? LocaleFormatter.FormatCost(newUniqueFactoryAI.GetRelocationCost(), isDistanceBased: false) : Locale.Get("CITYSERVICE_TOOLTIP_DISASTERSERVICEREQUIRED"));
                m_RebuildButton.isVisible = Singleton<LoadingManager>.instance.SupportsExpansion(Expansion.NaturalDisasters);
                m_RebuildButton.isEnabled = CanRebuild();
                m_MoveButton.isVisible = false;
            }
            else
            {
                m_RebuildButton.isVisible = false;
                m_MoveButton.isVisible = true;
            }
            m_generatedInfo.text = newUniqueFactoryAI.GetLocalizedStats(building, ref instance.m_buildings.m_buffer[building]);
            int num = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_health * newUniqueFactoryAI.m_inputRate1 * 16 / 100;
            long num2 = num * IndustryBuildingAI.GetResourcePrice(newUniqueFactoryAI.m_inputResource1) / 10000;
            int num3 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_health * newUniqueFactoryAI.m_inputRate2 * 16 / 100;
            long num4 = num3 * IndustryBuildingAI.GetResourcePrice(newUniqueFactoryAI.m_inputResource2) / 10000;
            int num5 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_health * newUniqueFactoryAI.m_inputRate3 * 16 / 100;
            long num6 = num5 * IndustryBuildingAI.GetResourcePrice(newUniqueFactoryAI.m_inputResource3) / 10000;
            int num7 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_health * newUniqueFactoryAI.m_inputRate4 * 16 / 100;
            long num8 = num7 * 10000 / 10000;
            m_expenses.text = (num2 + num4 + num6 + num8).ToString(Settings.moneyFormatNoCents, LocaleManager.cultureInfo);
            int num9 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_education3 * newUniqueFactoryAI.m_outputRate * 16 / 100;
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
            NewUniqueFactoryAI newUniqueFactoryAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI as NewUniqueFactoryAI;
            amount = 0;
            capacity = 0;
            switch (resourceIndex)
            {
                case 0:
                    amount = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_customBuffer2;
                    capacity = newUniqueFactoryAI.GetInputBufferSize1(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building]);
                    break;
                case 1:
                    amount = (Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_teens << 8) | Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_youngs;
                    capacity = newUniqueFactoryAI.GetInputBufferSize2(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building]);
                    break;
                case 2:
                    amount = (Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_adults << 8) | Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_seniors;
                    capacity = newUniqueFactoryAI.GetInputBufferSize3(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building]);
                    break;
                case 3:
                    amount = (Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_education1 << 8) | Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_education2;
                    capacity = newUniqueFactoryAI.GetInputBufferSize4(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building]);
                    break;
            }
            return IndustryWorldInfoPanel.SafelyNormalize(amount, capacity);
        }

        private string GetInputResourceName(int resourceIndex)
        {
            NewUniqueFactoryAI newUniqueFactoryAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI as NewUniqueFactoryAI;
            string key = "N/A";
            switch (resourceIndex)
            {
                case 0:
                    key = newUniqueFactoryAI.m_inputResource1.ToString();
                    break;
                case 1:
                    key = newUniqueFactoryAI.m_inputResource2.ToString();
                    break;
                case 2:
                    key = newUniqueFactoryAI.m_inputResource3.ToString();
                    break;
            }
            return Locale.Get("WAREHOUSEPANEL_RESOURCE", key);
        }

        private TransferManager.TransferReason GetInputResource(int resourceIndex)
        {
            NewUniqueFactoryAI newUniqueFactoryAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI as NewUniqueFactoryAI;
            return resourceIndex switch
            {
                0 => newUniqueFactoryAI.m_inputResource1,
                1 => newUniqueFactoryAI.m_inputResource2,
                2 => newUniqueFactoryAI.m_inputResource3,
                _ => TransferManager.TransferReason.None,
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