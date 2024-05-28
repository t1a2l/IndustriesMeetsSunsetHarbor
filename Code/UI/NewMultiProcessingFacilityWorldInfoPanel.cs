using System;
using System.Collections;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using UnityEngine;
using IndustriesMeetsSunsetHarbor.AI;
using System.Collections.Generic;
using IndustriesMeetsSunsetHarbor.Managers;
using MoreTransferReasons;
using System.Reflection;

namespace IndustriesMeetsSunsetHarbor.UI
{
    public class NewMultiProcessingFacilityWorldInfoPanel : BuildingWorldInfoPanel
    {
        private UIPanel m_mainPanel;

        private UILabel m_status;

        private UIButton m_RebuildButton;

        private UICheckBox m_OnOff;

        private UIPanel m_horizontalLine;

        private UIPanel m_inputContainer;

        private UIPanel m_outputContainer;

        private UITemplateList<UIPanel> m_inputs;

        private UITemplateList<UIPanel> m_outputs;

        private int m_inputResourceCount;

        private int m_outputResourceCount;

        private UISlider m_productionSlider;

        private UILabel m_productionRateLabel;

        private UILabel m_workplaces;

        private UILabel m_generatedInfo;

        private bool m_IsRelocating;

        private UIButton m_MoveButton;

        private UILabel m_Upkeep;

        private UILabel m_income;

        private UILabel m_expenses;

        private MultiProcessingFacilityAI m_multiProcessingFacilityAI;

        private UIComponent m_MovingPanel;

        private List<string> m_inputItems;

        private List<string> m_outputItems;

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

            var Diagram = Find<UIPanel>("Diagram");

            GameObject outputContainer = Instantiate(m_inputContainer.gameObject, Diagram.transform);
            m_outputContainer = outputContainer.GetComponent<UIPanel>();

            m_outputContainer.relativePosition = new Vector3(m_inputContainer.relativePosition.x, 200);

            var InputResource = m_outputContainer.Find<UIPanel>("UniqueFactoryInputResource");

            GameObject outputResource = Instantiate(InputResource.gameObject, InputResource.transform);
            outputResource.name = "UniqueFactoryOutputResource";

            outputResource.transform.SetParent(outputContainer.transform);

            m_outputContainer.AttachUIComponent(outputResource);

            var outputResource_Panel = outputResource.GetComponent<UIPanel>();

            outputResource_Panel.relativePosition = new Vector3(InputResource.relativePosition.x, 200);

            var m_productStorage = Find<UIPanel>("ProductStorage");
            var m_BigArrow = Find<UISprite>("Big Arrow");

            m_productStorage.transform.SetParent(outputResource_Panel.transform);
            m_BigArrow.transform.SetParent(outputResource_Panel.transform);

            outputResource_Panel.AttachUIComponent(m_productStorage.gameObject);
            outputResource_Panel.AttachUIComponent(m_BigArrow.gameObject);

            m_productStorage.relativePosition = new Vector3(m_productStorage.relativePosition.x, 50);
            m_BigArrow.relativePosition = new Vector3(m_BigArrow.relativePosition.x, 0);

            DestroyImmediate(InputResource.gameObject);

            var outputResourceArrow = outputResource_Panel.Find<UISprite>("Arrow");
            DestroyImmediate(outputResourceArrow.gameObject);

            var outputResourceStorage = outputResource_Panel.Find<UIPanel>("Storage");
            DestroyImmediate(outputResourceStorage.gameObject);

            var UniqueFactoryInputResource = m_inputContainer.Find<UIPanel>("UniqueFactoryInputResource");

            UniqueFactoryInputResource.transform.parent = null;

            outputResource.transform.parent = null;

            m_inputs = new UITemplateList<UIPanel>(m_inputContainer, "UniqueFactoryInputResource");
            m_outputs = new UITemplateList<UIPanel>(m_outputContainer, "UniqueFactoryOutputResource");

            m_productionSlider = Find<UISlider>("ProductionSlider");
            m_productionSlider.eventValueChanged += OnProductionRateChanged;
            m_productionRateLabel = Find<UILabel>("LabelProductionRate");
            m_workplaces = Find<UILabel>("LabelWorkplaces");
            m_MoveButton = Find<UIButton>("RelocateAction");
            m_RebuildButton = Find<UIButton>("RebuildButton");
            m_OnOff = Find<UICheckBox>("On/Off");
            m_OnOff.eventCheckChanged += OnOnOffChanged;
            m_Upkeep = Find<UILabel>("Upkeep");
            m_income = Find<UILabel>("IncomeLabel");
            m_expenses = Find<UILabel>("ExpensesLabel");
            m_mainPanel = Find<UIPanel>("(Library) NewUniqueFactoryWorldInfoPanel");

            m_inputItems = [];
            m_outputItems = [];
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
            m_inputItems = [];
            m_outputItems = [];
            ushort building = m_InstanceID.Building;
            Building data = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building];
            m_multiProcessingFacilityAI = data.Info.m_buildingAI as MultiProcessingFacilityAI;
            m_inputResourceCount = GetInputResourceCount(ref m_inputItems, m_multiProcessingFacilityAI);
            m_inputs.SetItemCount(m_inputResourceCount);
            m_outputResourceCount = GetOutputResourceCount(ref m_outputItems, m_multiProcessingFacilityAI);
            m_outputs.SetItemCount(m_outputResourceCount);
            m_horizontalLine.width = m_inputContainer.width;
            for (int i = 0; i < m_inputResourceCount; i++)
            {
                UILabel uILabel = m_inputs.items[i].Find<UILabel>("ResourceLabel");
                UISprite uISprite = m_inputs.items[i].Find<UISprite>("ResourceIcon");
                uILabel.text = GetInputResourceName(ref m_inputItems, i);
                uISprite.atlas = MoreTransferReasons.Utils.TextureUtils.GetAtlas("MoreTransferReasonsAtlas");
                uISprite.spriteName = GetInputResourceSpriteName(ref m_inputItems, i);
            }
            for (int i = 0; i < m_outputResourceCount; i++)
            {
                UILabel uILabel = m_outputs.items[i].Find<UILabel>("ProductLabel");
                UISprite uISprite = m_outputs.items[i].Find<UISprite>("LuxuryProductIcon");
                uILabel.text = GetOutputResourceName(ref m_outputItems, i);
                uISprite.atlas = MoreTransferReasons.Utils.TextureUtils.GetAtlas("MoreTransferReasonsAtlas");
                uISprite.spriteName = GetOutputResourceSpriteName(ref m_outputItems, i);
            }
            byte productionRate = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_productionRate;
            if (productionRate > 0)
            {
                m_productionSlider.value = (int)productionRate;
            }
        }

        private int GetInputResourceCount(ref List<string> items, MultiProcessingFacilityAI ai)
        {
            int count = 0;
            if (ai.m_inputResource1 != ExtendedTransferManager.TransferReason.None)
            {
                if(!items.Contains("m_inputResource1"))
                {
                    items.Add("m_inputResource1");
                }
                count++;
            }
            if (ai.m_inputResource2 != ExtendedTransferManager.TransferReason.None)
            {
                if(!items.Contains("m_inputResource2"))
                {
                    items.Add("m_inputResource2");
                }
                count++;
            }
            if (ai.m_inputResource3 != ExtendedTransferManager.TransferReason.None)
            {
                if(!items.Contains("m_inputResource3"))
                {
                    items.Add("m_inputResource3");
                }
                count++;
            }
            if (ai.m_inputResource4 != ExtendedTransferManager.TransferReason.None)
            {
                if(!items.Contains("m_inputResource4"))
                {
                    items.Add("m_inputResource4");
                }
                count++;
            }
            return count;
        }

        private int GetOutputResourceCount(ref List<string> items, MultiProcessingFacilityAI ai)
        {
            int count = 0;
            if (ai.m_outputResource1 != ExtendedTransferManager.TransferReason.None)
            {
                if(!items.Contains("m_outputResource1"))
                {
                    items.Add("m_outputResource1");
                }
                count++;
            }
            if (ai.m_outputResource2 != ExtendedTransferManager.TransferReason.None)
            {
                if(!items.Contains("m_outputResource2"))
                {
                    items.Add("m_outputResource2");
                }
                count++;
            }
            if (ai.m_outputResource3 != ExtendedTransferManager.TransferReason.None)
            {
                if (!items.Contains("m_outputResource3"))
                {
                    items.Add("m_outputResource3");
                }
                count++;
            }
            if (ai.m_outputResource4 != ExtendedTransferManager.TransferReason.None)
            {
                if (!items.Contains("m_outputResource4"))
                {
                    items.Add("m_outputResource4");
                }
                count++;
            }
            return count;
        }

        protected override void UpdateBindings()
        {
            base.UpdateBindings();
            ushort buildingId = m_InstanceID.Building;
            BuildingManager instance = Singleton<BuildingManager>.instance;
            Building building = instance.m_buildings.m_buffer[buildingId];
            MultiProcessingFacilityAI multiProcessingFacilityAI = building.Info.m_buildingAI as MultiProcessingFacilityAI;
            m_Upkeep.text = LocaleFormatter.FormatUpkeep(multiProcessingFacilityAI.GetResourceRate(buildingId, ref building, EconomyManager.Resource.Maintenance), isDistanceBased: false);
            m_status.text = multiProcessingFacilityAI.GetLocalizedStatus(buildingId, ref building);

            if (m_mainPanel != null)
            {
                if(m_inputResourceCount > 4)
                {
                     m_mainPanel.width = m_inputContainer.width + 22;
                }
                else
                {
                    m_mainPanel.width = 540;
                }
            }

            for (int i = 0; i < m_inputResourceCount; i++)
            {
                UIProgressBar uIProgressBar = m_inputs.items[i].Find<UIProgressBar>("ResourceBuffer");
                uIProgressBar.value = GetInputBufferProgress(ref m_inputItems, i, out var amount, out var capacity);
                var FormatResource = IndustryWorldInfoPanel.FormatResource((uint)amount);
                var inputResource = GetInputResourceExtended(ref m_inputItems, i);
                var formatResourceWithUnit = FormatResourceWithUnit((uint)capacity);
                uIProgressBar.progressColor = IndustryBuildingManager.GetExtendedResourceColor(inputResource);
                string text = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), FormatResource, formatResourceWithUnit);
                uIProgressBar.tooltip = text + Environment.NewLine + Environment.NewLine + inputResource.ToString();
            }
            for (int i = 0; i < m_outputResourceCount; i++)
            {
                UIProgressBar uIProgressBar = m_outputs.items[i].Find<UIProgressBar>("ProductBuffer");
                UIPanel productStorage = m_outputs.items[i].Find<UIPanel>("ProductStorage");
                uIProgressBar.value = GetOutputBufferProgress(ref m_outputItems, i, out var amount, out var capacity);
                var FormatResource = IndustryWorldInfoPanel.FormatResource((uint)amount);
                var outputResource = GetOutputResourceExtended(ref m_outputItems, i);
                var formatResourceWithUnit = FormatResourceWithUnit((uint)capacity);
                uIProgressBar.progressColor = IndustryBuildingManager.GetExtendedResourceColor(outputResource);
                string text = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), FormatResource, formatResourceWithUnit);
                uIProgressBar.tooltip = text + Environment.NewLine + Environment.NewLine + outputResource.ToString();
                productStorage.tooltip = text;
            }
            m_workplaces.text = StringUtils.SafeFormat(Locale.Get("UNIQUEFACTORYPANEL_WORKPLACES"), (multiProcessingFacilityAI.m_workPlaceCount0 + multiProcessingFacilityAI.m_workPlaceCount1 + multiProcessingFacilityAI.m_workPlaceCount2 + multiProcessingFacilityAI.m_workPlaceCount3).ToString());
            if ((building.m_flags & Building.Flags.Collapsed) != 0)
            {
                m_RebuildButton.tooltip = ((!IsDisasterServiceRequired()) ? LocaleFormatter.FormatCost(multiProcessingFacilityAI.GetRelocationCost(), isDistanceBased: false) : Locale.Get("CITYSERVICE_TOOLTIP_DISASTERSERVICEREQUIRED"));
                m_RebuildButton.isVisible = Singleton<LoadingManager>.instance.SupportsExpansion(Expansion.NaturalDisasters);
                m_RebuildButton.isEnabled = CanRebuild();
                m_MoveButton.isVisible = false;
            }
            else
            {
                m_RebuildButton.isVisible = false;
                m_MoveButton.isVisible = true;
            }
            m_generatedInfo.text = multiProcessingFacilityAI.GetLocalizedStats(buildingId, ref building);
            long inputs_expenses = 0;
            inputs_expenses += building.m_health * multiProcessingFacilityAI.m_inputRate1 * 16 / 100 * IndustryBuildingManager.GetExtendedResourcePrice(multiProcessingFacilityAI.m_inputResource1) / 10000;
            inputs_expenses += building.m_health * multiProcessingFacilityAI.m_inputRate2 * 16 / 100 * IndustryBuildingManager.GetExtendedResourcePrice(multiProcessingFacilityAI.m_inputResource2) / 10000;
            inputs_expenses += building.m_health * multiProcessingFacilityAI.m_inputRate3 * 16 / 100 * IndustryBuildingManager.GetExtendedResourcePrice(multiProcessingFacilityAI.m_inputResource3) / 10000;
            inputs_expenses += building.m_health * multiProcessingFacilityAI.m_inputRate4 * 16 / 100 * IndustryBuildingManager.GetExtendedResourcePrice(multiProcessingFacilityAI.m_inputResource4) / 10000;
            m_expenses.text = inputs_expenses.ToString(Settings.moneyFormatNoCents, LocaleManager.cultureInfo);

            long outputs_income = 0;
            outputs_income += building.m_education3 * multiProcessingFacilityAI.m_outputRate1 * 16 / 100 * IndustryBuildingManager.GetExtendedResourcePrice(multiProcessingFacilityAI.m_outputResource1) / 10000;
            outputs_income += building.m_education3 * multiProcessingFacilityAI.m_outputRate2 * 16 / 100 * IndustryBuildingManager.GetExtendedResourcePrice(multiProcessingFacilityAI.m_outputResource2) / 10000;
            outputs_income += building.m_education3 * multiProcessingFacilityAI.m_outputRate3 * 16 / 100 * IndustryBuildingManager.GetExtendedResourcePrice(multiProcessingFacilityAI.m_outputResource3) / 10000;
            outputs_income += building.m_education3 * multiProcessingFacilityAI.m_outputRate4 * 16 / 100 * IndustryBuildingManager.GetExtendedResourcePrice(multiProcessingFacilityAI.m_outputResource4) / 10000;
            m_income.text = outputs_income.ToString(Settings.moneyFormatNoCents, LocaleManager.cultureInfo);

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

        private float GetInputBufferProgress(ref List<string> items, int resourceIndex, out int amount, out int capacity)
        {
            ref Building buildingData = ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building];
            var custom_buffers = CustomBuffersManager.GetCustomBuffer(m_InstanceID.Building);
            MultiProcessingFacilityAI multiProcessingFacilityAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI as MultiProcessingFacilityAI;
            amount = 0;
            capacity = 0;
            switch (items[resourceIndex])
            {
                case "m_inputResource1":
                    amount = (int)custom_buffers.m_customBuffer1;
                    capacity = multiProcessingFacilityAI.GetInputBufferSize(ref buildingData, multiProcessingFacilityAI.m_inputRate1);
                    break;
                case "m_inputResource2":
                    amount = (int)custom_buffers.m_customBuffer2;
                    capacity = multiProcessingFacilityAI.GetInputBufferSize(ref buildingData, multiProcessingFacilityAI.m_inputRate2);
                    break;
                case "m_inputResource3":
                    amount = (int)custom_buffers.m_customBuffer3;
                    capacity = multiProcessingFacilityAI.GetInputBufferSize(ref buildingData, multiProcessingFacilityAI.m_inputRate3);
                    break;
                case "m_inputResource4":
                    amount = (int)custom_buffers.m_customBuffer4;
                    capacity = multiProcessingFacilityAI.GetInputBufferSize(ref buildingData, multiProcessingFacilityAI.m_inputRate4);
                    break;
            }
            return IndustryWorldInfoPanel.SafelyNormalize(amount, capacity);
        }

        private float GetOutputBufferProgress(ref List<string> items, int resourceIndex, out int amount, out int capacity)
        {
            ref Building buildingData = ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building];
            var custom_buffers = CustomBuffersManager.GetCustomBuffer(m_InstanceID.Building);
            MultiProcessingFacilityAI multiProcessingFacilityAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI as MultiProcessingFacilityAI;
            amount = 0;
            capacity = 0;
            switch (items[resourceIndex])
            {
                case "m_outputResource1":
                    amount = (int)custom_buffers.m_customBuffer5;
                    capacity = multiProcessingFacilityAI.GetOutputBufferSize(ref buildingData, multiProcessingFacilityAI.m_outputRate1, multiProcessingFacilityAI.m_outputVehicleCount1);
                    break;
                case "m_outputResource2":
                    amount = (int)custom_buffers.m_customBuffer6;
                    capacity = multiProcessingFacilityAI.GetOutputBufferSize(ref buildingData, multiProcessingFacilityAI.m_outputRate2, multiProcessingFacilityAI.m_outputVehicleCount2);
                    break;
                case "m_outputResource3":
                    amount = (int)custom_buffers.m_customBuffer7;
                    capacity = multiProcessingFacilityAI.GetOutputBufferSize(ref buildingData, multiProcessingFacilityAI.m_outputRate3, multiProcessingFacilityAI.m_outputVehicleCount3);
                    break;
                case "m_outputResource4":
                    amount = (int)custom_buffers.m_customBuffer8;
                    capacity = multiProcessingFacilityAI.GetOutputBufferSize(ref buildingData, multiProcessingFacilityAI.m_outputRate4, multiProcessingFacilityAI.m_outputVehicleCount4);
                    break;
            }
            return IndustryWorldInfoPanel.SafelyNormalize(amount, capacity);
        }

        private string GetInputResourceName(ref List<string> items, int resourceIndex)
        {
            MultiProcessingFacilityAI multiProcessingFacilityAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI as MultiProcessingFacilityAI;
            switch (items[resourceIndex])
            {
                case "m_inputResource1":
                    return multiProcessingFacilityAI.m_inputResource1.ToString();
                case "m_inputResource2":
                    return multiProcessingFacilityAI.m_inputResource2.ToString();
                case "m_inputResource3":
                    return multiProcessingFacilityAI.m_inputResource3.ToString();
                case "m_inputResource4":
                    return multiProcessingFacilityAI.m_inputResource4.ToString();
            }
            return "";
        }

        private string GetOutputResourceName(ref List<string> items, int resourceIndex)
        {
            MultiProcessingFacilityAI multiProcessingFacilityAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI as MultiProcessingFacilityAI;
            switch (items[resourceIndex])
            {
                case "m_outputResource1":
                    return multiProcessingFacilityAI.m_outputResource1.ToString();
                case "m_outputResource2":
                    return multiProcessingFacilityAI.m_outputResource2.ToString();
                case "m_outputResource3":
                    return multiProcessingFacilityAI.m_outputResource3.ToString();
                case "m_outputResource4":
                    return multiProcessingFacilityAI.m_outputResource4.ToString();
            }
            return "";
        }

        private string GetInputResourceSpriteName(ref List<string> items, int resourceIndex)
        {
            switch (items[resourceIndex])
            {
                case "m_inputResource1":
                case "m_inputResource2":
                case "m_inputResource3":
                case "m_inputResource4":
                    return IndustryBuildingManager.ResourceSpriteName(GetInputResourceExtended(ref items, resourceIndex));
            }
            return null;
        }

        private string GetOutputResourceSpriteName(ref List<string> items, int resourceIndex)
        {
            switch (items[resourceIndex])
            {
                case "m_outputResource1":
                case "m_outputResource2":
                case "m_outputResource3":
                case "m_outputResource4":
                    return IndustryBuildingManager.ResourceSpriteName(GetOutputResourceExtended(ref items, resourceIndex));
            }
            return null;
        }

        private ExtendedTransferManager.TransferReason GetInputResourceExtended(ref List<string> items, int resourceIndex)
        {
            MultiProcessingFacilityAI multiProcessingFacilityAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI as MultiProcessingFacilityAI;
            return items[resourceIndex] switch
            {
                "m_inputResource1" => multiProcessingFacilityAI.m_inputResource1,
                "m_inputResource2" => multiProcessingFacilityAI.m_inputResource2,
                "m_inputResource3" => multiProcessingFacilityAI.m_inputResource3,
                "m_inputResource4" => multiProcessingFacilityAI.m_inputResource4,
                _ => ExtendedTransferManager.TransferReason.None
            };
        }

        private ExtendedTransferManager.TransferReason GetOutputResourceExtended(ref List<string> items, int resourceIndex)
        {
            MultiProcessingFacilityAI multiProcessingFacilityAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI as MultiProcessingFacilityAI;
            return items[resourceIndex] switch
            {
                "m_outputResource1" => multiProcessingFacilityAI.m_outputResource1,
                "m_outputResource2" => multiProcessingFacilityAI.m_outputResource2,
                "m_outputResource3" => multiProcessingFacilityAI.m_outputResource3,
                "m_outputResource4" => multiProcessingFacilityAI.m_outputResource4,
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
                if (info is not null && (instance.m_buildings.m_buffer[buildingID].m_flags & Building.Flags.Collapsed) != 0)
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
            Show<NewMultiProcessingFacilityWorldInfoPanel>(worldPosition, instanceID);
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

        private static string FormatResourceWithUnit(uint amount)
        {
	    return string.Concat(str2: Locale.Get("RESOURCEUNIT_TONS"), str0: IndustryWorldInfoPanel.FormatResource(amount), str1: " ");
        }
    }
}
