using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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

namespace IndustriesMeetsSunsetHarbor.UI
{
    public class ExtendedUniqueFactoryWorldInfoPanel : BuildingWorldInfoPanel
    {
        private UIPanel m_mainPanel;

        private UIPanel m_details;

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

        private UIPanel m_VariationPanel;

        private UIDropDown m_VariationDropdown;

        private ExtendedUniqueFactoryAI m_extendedUniqueFactoryAI;

        private UIComponent m_MovingPanel;

        private List<string> m_inputItems;

        private List<string> m_outputItems;

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

            DestroyImmediate(InputResource.gameObject);

            var outputResourceArrow = outputResource_Panel.Find<UISprite>("Arrow");
            DestroyImmediate(outputResourceArrow.gameObject);

            var outputResourceStorage = outputResource_Panel.Find<UIPanel>("Storage");
            DestroyImmediate(outputResourceStorage.gameObject);

            var m_productStorage = Find<UIPanel>("ProductStorage");
            var m_BigArrow = Find<UISprite>("Big Arrow");

            m_productStorage.transform.SetParent(outputResource_Panel.transform);
            m_BigArrow.transform.SetParent(outputResource_Panel.transform);

            outputResource_Panel.AttachUIComponent(m_productStorage.gameObject);
            outputResource_Panel.AttachUIComponent(m_BigArrow.gameObject);

            UITemplateManager instance = Singleton<UITemplateManager>.instance;

            var m_Templates = (Dictionary<string, UIComponent>)typeof(UITemplateManager).GetField("m_Templates", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance);

            m_Templates.Add("UniqueFactoryOutputResource", outputResource_Panel);

            typeof(UITemplateManager).GetField("m_Templates", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(instance, m_Templates);

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

            var _cityServiceWorldInfoPanel = UIView.library.Get<CityServiceWorldInfoPanel>(typeof(CityServiceWorldInfoPanel).Name);

            var City_VariationPanel = _cityServiceWorldInfoPanel.Find<UIPanel>("VariationPanel");

            GameObject VariationPanel = Instantiate(City_VariationPanel.gameObject, Diagram.transform);

            m_VariationPanel = VariationPanel.GetComponent<UIPanel>();

            m_VariationPanel.transform.SetParent(Diagram.transform);

            m_VariationPanel.relativePosition = new Vector3(100, 316);

            Diagram.AttachUIComponent(m_VariationPanel.gameObject);

            m_VariationDropdown = m_VariationPanel.Find<UIDropDown>("DropdownVariation");
            m_VariationDropdown.eventSelectedIndexChanged += OnVariationDropdownChanged;

            m_inputItems = [];
            m_outputItems = [];
        }

        private void OnVariationDropdownChanged(UIComponent component, int value)
        {
            if (!m_extendedUniqueFactoryAI.GetVariations(out var variations))
            {
                return;
            }
            Singleton<SimulationManager>.instance.AddAction(delegate
            {
                if (Singleton<BuildingManager>.exists)
                {
                    Singleton<BuildingManager>.instance.UpdateBuildingInfo(m_InstanceID.Building, variations[m_VariationDropdown.selectedIndex].m_info);
                    ThreadHelper.dispatcher.Dispatch(delegate
                    {
                        m_NameField.text = GetName();
                    });
                    IndustryBuildingAI industryBuildingAI = (IndustryBuildingAI)Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI;
                    industryBuildingAI.SetLastVariationIndex(value);
                }
            });
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
            m_extendedUniqueFactoryAI = data.Info.m_buildingAI as ExtendedUniqueFactoryAI;
            m_inputResourceCount = GetInputResourceCount(ref m_inputItems, m_extendedUniqueFactoryAI);
            m_inputs.SetItemCount(m_inputResourceCount);
            m_outputResourceCount = GetOutputResourceCount(ref m_outputItems, m_extendedUniqueFactoryAI);
            m_outputs.SetItemCount(m_outputResourceCount);
            m_horizontalLine.width = m_inputContainer.width;
            for (int i = 0; i < m_inputResourceCount; i++)
            {
                var transferReasons = GetInputResource(ref m_inputItems, i);
                if (transferReasons != null && transferReasons.Length != 0)
                {
                    UILabel uILabel = m_inputs.items[i].Find<UILabel>("ResourceLabel");
                    UISprite uISprite = m_inputs.items[i].Find<UISprite>("ResourceIcon");

                    if (transferReasons.Length == 1)
                    {
                        uILabel.text = GetInputResourceName(transferReasons[0]);
                        uISprite.atlas = UITextures.InGameAtlas;
                        uISprite.spriteName = AtlasUtils.GetSpriteName(transferReasons[0]);
                    }
                    else
                    {
                        uILabel.text = "Mixed Resources";
                        uISprite.atlas = TextureUtils.GetAtlas("MoreTransferReasonsAtlas");
                        uISprite.spriteName = "MixedResources";
                        MakeHoverDetail(m_inputs.items[i], transferReasons);
                    }
                    UIPanel Storage = m_inputs.items[i].Find<UIPanel>("Storage");
                    UISprite Arrow = m_inputs.items[i].Find<UISprite>("Arrow");
                    Storage.relativePosition = new Vector3(0, 80);
                    Arrow.relativePosition = new Vector3(58, 138);
                }
            }
            for (int i = 0; i < m_outputResourceCount; i++)
            {
                UILabel uILabel = m_outputs.items[i].Find<UILabel>("ProductLabel");
                UISprite uISprite = m_outputs.items[i].Find<UISprite>("LuxuryProductIcon");
                var text = GetOutputResourceName(ref m_outputItems, i);
                if(text.Contains("Products") && text.Length > 15)
                {
                    text = text.Replace("Products", "Prods");
                }
                uILabel.text = text;
                uISprite.atlas = GetOutputResourceAtlas(ref m_outputItems, i);
                uISprite.spriteName = GetOutputResourceSpriteName(ref m_outputItems, i);
                uILabel.relativePosition = new Vector3(-30, 145);
                uISprite.relativePosition = new Vector3(15, 113);
                UIProgressBar uIProgressBar = m_outputs.items[i].Find<UIProgressBar>("ProductBuffer");
                UISprite ArrowEnd = m_outputs.items[i].Find<UISprite>("ArrowEnd");
                UISprite BigArrow = m_outputs.items[i].Find<UISprite>("Big Arrow");
                UIPanel ProductStorage = m_outputs.items[i].Find<UIPanel>("ProductStorage");
                uIProgressBar.relativePosition = new Vector3(0, 80);
                ArrowEnd.relativePosition = new Vector3(-9, 31);
                BigArrow.relativePosition = new Vector3(55, 9);
                ProductStorage.relativePosition = new Vector3(35, -46);
            }
            m_VariationPanel.isVisible = false;
            var IsCarFactory = false;
            if(m_extendedUniqueFactoryAI.m_outputResource2 == ExtendedTransferManager.Cars)
            {
                IsCarFactory = true;
            }
            if (m_extendedUniqueFactoryAI != null && !IsCarFactory && m_extendedUniqueFactoryAI.GetVariations(out var variations) && variations.m_size > 1)
            {
                m_VariationPanel.isVisible = true;
                List<string> list = [];
                int selectedIndex = -1;
                for (int i = 0; i < variations.m_size; i++)
                {
                    string id = "FIELDVARIATION" + "_" + Singleton<SimulationManager>.instance.m_metaData.m_environment.ToUpper();
                    string empty = (Locale.Exists(id, variations.m_buffer[i].m_info.name) ? Locale.Get(id, variations.m_buffer[i].m_info.name) : ((!Locale.Exists("FIELDVARIATION", variations.m_buffer[i].m_info.name)) ? variations.m_buffer[i].m_info.GetUncheckedLocalizedTitle() : Locale.Get("FIELDVARIATION", variations.m_buffer[i].m_info.name)));
                    list.Add(empty);
                    if (m_extendedUniqueFactoryAI.m_info.name == variations.m_buffer[i].m_info.name)
                    {
                        selectedIndex = i;
                    }
                }
                m_VariationDropdown.items = [.. list];
                m_VariationDropdown.selectedIndex = selectedIndex;
            }
            byte productionRate = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_productionRate;
            if (productionRate > 0)
            {
                m_productionSlider.value = (int)productionRate;
            }
        }

        private int GetInputResourceCount(ref List<string> items, ExtendedProcessingFacilityAI ai)
        {
            int count = 0;
            if (ai.m_inputResource1.Length != 0)
            {
                if(!items.Contains("m_inputResource1"))
                {
                    items.Add("m_inputResource1");
                }
                count++;
            }
            if (ai.m_inputResource2.Length != 0)
            {
                if(!items.Contains("m_inputResource2"))
                {
                    items.Add("m_inputResource2");
                }
                count++;
            }
            if (ai.m_inputResource3.Length != 0)
            {
                if (!items.Contains("m_inputResource3"))
                {
                    items.Add("m_inputResource3");
                }
                count++;
            }
            if (ai.m_inputResource4.Length != 0)
            {
                if (!items.Contains("m_inputResource4"))
                {
                    items.Add("m_inputResource4");
                }
                count++;
            }
            if (ai.m_inputResource5.Length != 0)
            {
                if (!items.Contains("m_inputResource5"))
                {
                    items.Add("m_inputResource5");
                }
                count++;
            }
            if (ai.m_inputResource6.Length != 0)
            {
                if (!items.Contains("m_inputResource6"))
                {
                    items.Add("m_inputResource6");
                }
                count++;
            }
            if (ai.m_inputResource7.Length != 0)
            {
                if (!items.Contains("m_inputResource7"))
                {
                    items.Add("m_inputResource7");
                }
                count++;
            }
            if (ai.m_inputResource8.Length != 0)
            {
                if (!items.Contains("m_inputResource8"))
                {
                    items.Add("m_inputResource8");
                }
                count++;
            }
            return count;
        }

        private int GetOutputResourceCount(ref List<string> items, ExtendedProcessingFacilityAI ai)
        {
            int count = 0;
            if (ai.m_outputResource1 != TransferManager.TransferReason.None)
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
            return count;
        }

        protected override void UpdateBindings()
        {
            base.UpdateBindings();
            ushort buildingId = m_InstanceID.Building;
            BuildingManager instance = Singleton<BuildingManager>.instance;
            Building building = instance.m_buildings.m_buffer[buildingId];
            ExtendedUniqueFactoryAI extendedUniqueFactoryAI = building.Info.m_buildingAI as ExtendedUniqueFactoryAI;
            m_Upkeep.text = LocaleFormatter.FormatUpkeep(extendedUniqueFactoryAI.GetResourceRate(buildingId, ref building, EconomyManager.Resource.Maintenance), isDistanceBased: false);
            m_status.text = extendedUniqueFactoryAI.GetLocalizedStatus(buildingId, ref building);

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
                uIProgressBar.value = GetBufferProgress(ref m_inputItems, i, out var amount, out var capacity);
                var FormatResource = IndustryWorldInfoPanel.FormatResource((uint)amount);
                string text;
                var transferReasons = GetInputResource(ref m_inputItems, i);
                if (transferReasons != null && transferReasons.Length != 0)
                {
                    var formatResourceWithUnit = IndustryWorldInfoPanel.FormatResourceWithUnit((uint)capacity, transferReasons[0]);
                    text = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), FormatResource, formatResourceWithUnit);
                    if (transferReasons.Length == 1)
                    {
                        uIProgressBar.progressColor = IndustryWorldInfoPanel.instance.GetResourceColor(transferReasons[0]);
                        text = text + Environment.NewLine + Environment.NewLine + StringUtils.SafeFormat(Locale.Get("RESOURCEDESCRIPTION", transferReasons[0].ToString()));
                    }
                    else
                    {
                        uIProgressBar.progressColor = new Color32(128, 128, 128, 255);
                        text = text + Environment.NewLine + Environment.NewLine + StringUtils.SafeFormat(Locale.Get("RESOURCEDESCRIPTION", "MixedResources"));
                    }
                    uIProgressBar.tooltip = text;
                }                
            }
            for (int i = 0; i < m_outputResourceCount; i++)
            {
                UIProgressBar uIProgressBar = m_outputs.items[i].Find<UIProgressBar>("ProductBuffer");
                UIPanel productStorage = m_outputs.items[i].Find<UIPanel>("ProductStorage");
                uIProgressBar.value = GetBufferProgress(ref m_outputItems, i, out var amount, out var capacity);
                var FormatResource = IndustryWorldInfoPanel.FormatResource((uint)amount);
                string text;
                var outputResource = GetOutputResource(ref m_outputItems, i);
                var formatResourceWithUnit = IndustryWorldInfoPanel.FormatResourceWithUnit((uint)capacity, outputResource);
                uIProgressBar.progressColor = IndustryWorldInfoPanel.instance.GetResourceColor(outputResource);
                text = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), FormatResource, formatResourceWithUnit);
                uIProgressBar.tooltip = text + Environment.NewLine + Environment.NewLine + StringUtils.SafeFormat(Locale.Get("RESOURCEDESCRIPTION", outputResource.ToString()));
                productStorage.tooltip = text;
            }
            if ((building.m_flags & Building.Flags.Collapsed) != 0)
            {
                m_VariationDropdown.isEnabled = false;
                m_VariationDropdown.tooltip = Locale.Get("VARIATIONBUILDING_COLLAPSED_TOOLTIP");
            }
            else if (building.m_fireIntensity > 0)
            {
                m_VariationDropdown.isEnabled = false;
                m_VariationDropdown.tooltip = Locale.Get("VARIATIONBUILDING_ONFIRE_TOOLTIP");
            }
            else
            {
                m_VariationDropdown.isEnabled = true;
                m_VariationDropdown.tooltip = string.Empty;
            }
            m_workplaces.text = StringUtils.SafeFormat(Locale.Get("UNIQUEFACTORYPANEL_WORKPLACES"), (extendedUniqueFactoryAI.m_workPlaceCount0 + extendedUniqueFactoryAI.m_workPlaceCount1 + extendedUniqueFactoryAI.m_workPlaceCount2 + extendedUniqueFactoryAI.m_workPlaceCount3).ToString());
            if ((building.m_flags & Building.Flags.Collapsed) != 0)
            {
                m_RebuildButton.tooltip = ((!IsDisasterServiceRequired()) ? LocaleFormatter.FormatCost(extendedUniqueFactoryAI.GetRelocationCost(), isDistanceBased: false) : Locale.Get("CITYSERVICE_TOOLTIP_DISASTERSERVICEREQUIRED"));
                m_RebuildButton.isVisible = Singleton<LoadingManager>.instance.SupportsExpansion(Expansion.NaturalDisasters);
                m_RebuildButton.isEnabled = CanRebuild();
                m_MoveButton.isVisible = false;
            }
            else
            {
                m_RebuildButton.isVisible = false;
                m_MoveButton.isVisible = true;
            }
            m_generatedInfo.text = extendedUniqueFactoryAI.GetLocalizedStats(buildingId, ref building);
            long inputs_expenses = 0;
            // IndustryBuildingAI.GetResourcePrice(extendedUniqueFactoryAI.m_inputResource1[0])
            inputs_expenses += building.m_health * extendedUniqueFactoryAI.m_inputRate1 * 16 / 100 * 1 / 10000;
            inputs_expenses += building.m_health * extendedUniqueFactoryAI.m_inputRate2 * 16 / 100 * 1 / 10000;
            inputs_expenses += building.m_health * extendedUniqueFactoryAI.m_inputRate3 * 16 / 100 * 1 / 10000;
            inputs_expenses += building.m_health * extendedUniqueFactoryAI.m_inputRate4 * 16 / 100 * 1 / 10000;
            inputs_expenses += building.m_health * extendedUniqueFactoryAI.m_inputRate5 * 16 / 100 * 1 / 10000;
            inputs_expenses += building.m_health * extendedUniqueFactoryAI.m_inputRate6 * 16 / 100 * 1 / 10000;
            inputs_expenses += building.m_health * extendedUniqueFactoryAI.m_inputRate7 * 16 / 100 * 1 / 10000;
            inputs_expenses += building.m_health * extendedUniqueFactoryAI.m_inputRate8 * 16 / 100 * 1 / 10000;
            m_expenses.text = inputs_expenses.ToString(Settings.moneyFormatNoCents, LocaleManager.cultureInfo);

            long outputs_income = 0;
            outputs_income += building.m_education3 * extendedUniqueFactoryAI.m_outputRate1 * 16 / 100 * IndustryBuildingAI.GetResourcePrice(extendedUniqueFactoryAI.m_outputResource1) / 10000;
            outputs_income += building.m_education3 * extendedUniqueFactoryAI.m_outputRate2 * 16 / 100 * IndustryBuildingAI.GetResourcePrice(extendedUniqueFactoryAI.m_outputResource2) / 10000;
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

        private float GetBufferProgress(ref List<string> items, int resourceIndex, out int amount, out int capacity)
        {
            ref Building buildingData = ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building];
            ExtendedUniqueFactoryAI extendedUniqueFactoryAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI as ExtendedUniqueFactoryAI;
            amount = 0;
            capacity = 0;
            switch (items[resourceIndex])
            {
                case "m_inputResource1":
                    amount = GetInputBufferAmount(extendedUniqueFactoryAI.m_inputResource1);
                    capacity = extendedUniqueFactoryAI.GetInputBufferSize1(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building]);
                    break;
                case "m_inputResource2":
                    amount = GetInputBufferAmount(extendedUniqueFactoryAI.m_inputResource1);
                    capacity = extendedUniqueFactoryAI.GetInputBufferSize1(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building]);
                    break;
                case "m_inputResource3":
                    amount = GetInputBufferAmount(extendedUniqueFactoryAI.m_inputResource1);
                    capacity = extendedUniqueFactoryAI.GetInputBufferSize1(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building]);
                    break;
                case "m_inputResource4":
                    amount = GetInputBufferAmount(extendedUniqueFactoryAI.m_inputResource1);
                    capacity = extendedUniqueFactoryAI.GetInputBufferSize1(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building]);
                    break;
                case "m_inputResource5":
                    amount = GetInputBufferAmount(extendedUniqueFactoryAI.m_inputResource1);
                    capacity = extendedUniqueFactoryAI.GetInputBufferSize1(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building]);
                    break;
                case "m_inputResource6":
                    amount = GetInputBufferAmount(extendedUniqueFactoryAI.m_inputResource1);
                    capacity = extendedUniqueFactoryAI.GetInputBufferSize1(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building]);
                    break;
                case "m_inputResource7":
                    amount = GetInputBufferAmount(extendedUniqueFactoryAI.m_inputResource1);
                    capacity = extendedUniqueFactoryAI.GetInputBufferSize1(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building]);
                    break;
                case "m_inputResource8":
                    amount = GetInputBufferAmount(extendedUniqueFactoryAI.m_inputResource1);
                    capacity = extendedUniqueFactoryAI.GetInputBufferSize1(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building]);
                    break;
                case "m_outputResource1":
                    amount = GetInputBufferAmount(extendedUniqueFactoryAI.m_inputResource1);
                    capacity = extendedUniqueFactoryAI.GetInputBufferSize1(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building]);
                    break;
                case "m_outputResource2":
                    amount = GetInputBufferAmount(extendedUniqueFactoryAI.m_inputResource1);
                    capacity = extendedUniqueFactoryAI.GetInputBufferSize1(m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building]);
                    break;
            }
            return IndustryWorldInfoPanel.SafelyNormalize(amount, capacity);
        }

        private int GetInputBufferAmount(TransferManager.TransferReason[] transferReasons)
        {
            int amount = 0;
            var custom_buffers = CustomBuffersManager.GetCustomBuffer(m_InstanceID.Building);
            if (transferReasons.Length != 0)
            {
                foreach (var inputResource in transferReasons)
                {
                    if (inputResource != TransferManager.TransferReason.None)
                    {
                        amount += (int)custom_buffers.Get((int)inputResource);
                    }
                }
            }
            return amount;
        }

        private string GetInputResourceName(TransferManager.TransferReason reason)
        {
            if (reason != TransferManager.TransferReason.None)
            {
                if (reason >= ExtendedTransferManager.MealsDeliveryLow)
                {
                    return ExtendedTransferManager.GetTransferReasonName((int)reason);
                }
            }
            return reason.ToString();
        }

        private string GetOutputResourceName(ref List<string> items, int resourceIndex)
        {
            ExtendedUniqueFactoryAI extendedUniqueFactoryAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI as ExtendedUniqueFactoryAI;
            string name = "";
            switch (items[resourceIndex])
            {
                case "m_outputResource1":
                    name = extendedUniqueFactoryAI.m_outputResource1 >= ExtendedTransferManager.MealsDeliveryLow ? ExtendedTransferManager.GetTransferReasonName((int)extendedUniqueFactoryAI.m_outputResource1) : Locale.Get("WAREHOUSEPANEL_RESOURCE", extendedUniqueFactoryAI.m_outputResource1.ToString());
                    break;
                case "m_outputResource2":
                    name = extendedUniqueFactoryAI.m_outputResource2 >= ExtendedTransferManager.MealsDeliveryLow ? ExtendedTransferManager.GetTransferReasonName((int)extendedUniqueFactoryAI.m_outputResource2) : Locale.Get("WAREHOUSEPANEL_RESOURCE", extendedUniqueFactoryAI.m_outputResource2.ToString());
                    break;
            }
            return name;
        }

        private UITextureAtlas GetInputResourceAtlas(TransferManager.TransferReason reason)
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

        private UITextureAtlas GetOutputResourceAtlas(ref List<string> items, int resourceIndex)
        {
            var reason = GetOutputResource(ref items, resourceIndex);
            if (reason != TransferManager.TransferReason.None)
            {
                if (reason >= ExtendedTransferManager.MealsDeliveryLow)
                {
                    return TextureUtils.GetAtlas("MoreTransferReasonsAtlas");
                }
            }
            return UITextures.InGameAtlas;
        }

        private string GetOutputResourceSpriteName(ref List<string> items, int resourceIndex)
        {
            switch (items[resourceIndex])
            {
                case "m_outputResource1":
                case "m_outputResource2":
                    return AtlasUtils.GetSpriteName(GetOutputResource(ref items, resourceIndex));;
            }
            return null;
        }

        private TransferManager.TransferReason[] GetInputResource(ref List<string> items, int resourceIndex)
        {
            ExtendedUniqueFactoryAI extendedUniqueFactoryAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI as ExtendedUniqueFactoryAI;
            return items[resourceIndex] switch
            {
                "m_inputResource1" => extendedUniqueFactoryAI.m_inputResource1,
                "m_inputResource2" => extendedUniqueFactoryAI.m_inputResource2,
                "m_inputResource3" => extendedUniqueFactoryAI.m_inputResource3,
                "m_inputResource4" => extendedUniqueFactoryAI.m_inputResource4,
                "m_inputResource5" => extendedUniqueFactoryAI.m_inputResource5,
                "m_inputResource6" => extendedUniqueFactoryAI.m_inputResource6,
                "m_inputResource7" => extendedUniqueFactoryAI.m_inputResource7,
                "m_inputResource8" => extendedUniqueFactoryAI.m_inputResource8,

                _ => []
            };
        }

        private TransferManager.TransferReason GetOutputResource(ref List<string> items, int resourceIndex)
        {
            ExtendedUniqueFactoryAI extendedUniqueFactoryAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI as ExtendedUniqueFactoryAI;
            switch (items[resourceIndex])
            {
                case "m_outputResource1":
                    return extendedUniqueFactoryAI.m_outputResource1;
                case "m_outputResource2":
                    return extendedUniqueFactoryAI.m_outputResource2;
            }
            return TransferManager.TransferReason.None;
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
            MovingPanel.Find<UILabel>("MovingLabel").text = LocaleFormatter.FormatGeneric("BUILDING_MOVING", base.buildingName);
            MovingPanel.Show();
        }

        public void TempShow(Vector3 worldPosition, InstanceID instanceID)
        {
            MovingPanel.Hide();
            Show<ExtendedProcessingFacilityWorldInfoPanel>(worldPosition, instanceID);
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
                if (currentTool != null && IsValidTarget() && currentTool.m_relocate != 0 && !MovingPanel.isVisible)
                {
                    MovingPanel.Show();
                    return;
                }
                if (!IsValidTarget() || (currentTool != null && currentTool.m_relocate == 0))
                {
                    mainToolbar.ResetLastTool();
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

        private void MakeHoverDetail(UIPanel parent, TransferManager.TransferReason[] transferReasons)
        {
            float itemW = 36f;
            float padding = 6f;
            float panelW = transferReasons.Length * itemW + (transferReasons.Length + 1) * padding;

            var custom_buffers = CustomBuffersManager.GetCustomBuffer(m_InstanceID.Building);

            m_details = parent.AddUIComponent<UIPanel>();
            m_details.backgroundSprite = "GenericPanelLight";
            m_details.autoLayout = true;
            m_details.autoLayoutDirection = LayoutDirection.Horizontal;
            m_details.autoLayoutPadding = new RectOffset((int)padding, 0, 6, 0);
            m_details.size = new Vector2(panelW, 76f);
            m_details.relativePosition = new Vector3(0f, 68f);
            m_details.isVisible = false;
            m_details.zOrder = 9999;   // float above everything

            var cx = 100f;
            foreach (var reason in transferReasons)
            {
                var label = "";
                if (reason != TransferManager.TransferReason.None)
                {
                    if (reason >= ExtendedTransferManager.MealsDeliveryLow)
                    {
                        label = ExtendedTransferManager.GetTransferReasonName((int)reason);
                    }
                    else
                    {
                        label = reason.ToString();
                    }
                }
                var go = new GameObject();
                MakeStorageBox(go, label, reason, cx, out UIProgressBar buffer);
                cx += 200f;
                buffer.value = custom_buffers.Get((int)reason);
            }

            // Show/hide on hover
            parent.eventMouseEnter += (c, e) => { m_details.isVisible = true; m_details.BringToFront(); };
            parent.eventMouseLeave += (c, e) => m_details.isVisible = false;
        }

        private UIPanel MakeStorageBox(GameObject gameObject, string label, TransferManager.TransferReason transferReason, float cx, out UIProgressBar buffer)
        {
            gameObject = Instantiate(Find<UIPanel>("StorageOil").gameObject, m_details.transform, false);
            var box = gameObject.GetComponent<UIPanel>();
            box.name = "Storage" + label.Replace("\n", "");
            box.anchor = UIAnchorStyle.None;
            box.relativePosition = new Vector3(cx - 61f, 290f);
            box.autoLayout = false;
            box.size = new Vector2(122f, 64f);
            box.backgroundSprite = "";

            var icon = gameObject.transform.Find("ResourceIconOil").GetComponent<UISprite>();
            icon.relativePosition = new Vector3(46f, 3f);
            icon.size = new Vector2(30f, 30f);
            icon.atlas = GetInputResourceAtlas(transferReason);
            icon.spriteName = MoreTransferReasons.Utils.AtlasUtils.GetSpriteName(transferReason, false);

            buffer = gameObject.transform.Find("OilBuffer").GetComponent<UIProgressBar>();
            buffer.name = label.Replace("\n", "") + "Buffer";
            buffer.relativePosition = new Vector3(29f, -29f);
            buffer.size = new Vector2(64f, 122f);

            var lbl = gameObject.transform.Find("StorageOilLabel").GetComponent<UILabel>();
            lbl.text = label;
            lbl.name = "Storage" + label.Replace("\n", "") + "Label";
            lbl.relativePosition = new Vector3(0f, 35f);
            lbl.textScale = 0.8125f;
            lbl.textAlignment = UIHorizontalAlignment.Center;

            return box;
        }

    }
}
