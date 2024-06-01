using System;
using System.Collections;
using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    public class NewIndustryWorldInfoPanel : WorldInfoPanel
    {
        public class ProgressBarUpdater
        {
            private UIProgressBar[] m_bars;

            private Color32 m_normalColor;

            private Color32 m_inProgressColor;

            private Color32 m_allCompletedColor;

            private UIComponent m_parentPanel;

            public ProgressBarUpdater(UIComponent panel)
            {
                m_normalColor = Color.white;
                m_inProgressColor = new Color32(154, 189, 221, byte.MaxValue);
                m_allCompletedColor = Color.yellow;
                m_parentPanel = panel;
                m_bars = new UIProgressBar[5];
                for (int i = 0; i < 5; i++)
                {
                    m_bars[i] = panel.Find<UIProgressBar>("Level" + (i + 1) + "Bar");
                }
            }

            public void Update(int parkID)
            {
                if (parkID <= -1)
                {
                    return;
                }
                int num = ParkLevel(parkID);
                int i;
                for (i = 0; i < num; i++)
                {
                    m_bars[i].value = 1f;
                    if (num < 5)
                    {
                        m_bars[i].progressColor = m_normalColor;
                    }
                    else
                    {
                        m_bars[i].progressColor = m_allCompletedColor;
                    }
                }
                if (i < 5)
                {
                    float value = ProgressToNextLevel(parkID);
                    m_bars[i].value = value;
                    m_bars[i].progressColor = m_inProgressColor;
                    i++;
                }
                for (; i < 5; i++)
                {
                    m_bars[i].value = 0f;
                }
                m_parentPanel.tooltip = StringUtils.SafeFormat(Locale.Get("PARKINFOPANEL_LEVEL"), num.ToString());
            }

            public static float ProgressToNextLevel(int parkID)
            {
                float num = Singleton<DistrictManager>.instance.m_properties.m_parkProperties.m_industryLevelInfo[ParkLevel(parkID)].m_productionLevelupRequirement;
                float num2 = Singleton<DistrictManager>.instance.m_properties.m_parkProperties.m_industryLevelInfo[ParkLevel(parkID)].m_workerLevelupRequirement;
                float a = (float)Singleton<DistrictManager>.instance.m_parks.m_buffer[parkID].m_totalProductionAmount / num;
                float b = (float)(int)Singleton<DistrictManager>.instance.m_parks.m_buffer[parkID].m_finalWorkerCount / num2;
                return Mathf.Min(a, b);
            }

            public static int ParkLevel(int parkID)
            {
                return (int)Singleton<DistrictManager>.instance.m_parks.m_buffer[parkID].m_parkLevel;
            }
        }

        private UITextField m_ParkName;

        private UILabel m_size;

        private string m_currentIndustryType;

        private UILabel m_oilLabel;

        private UILabel m_plasticLabel;

        private UILabel m_fuelLabel;

        private UIProgressBar m_oilBuffer;

        private UIProgressBar m_plasticBuffer;

        private UIProgressBar m_fuelBuffer;

        private UIPanel m_oilBufferPanel;

        private UIPanel m_plasticBufferPanel;

        private UIPanel m_fuelBufferPanel;

        private UILabel m_fuelConversion;

        private UILabel m_plasticConversion;

        private UILabel m_rawMaterialConsumption;

        private UILabel m_acquisition;

        private UILabel m_acquisitionExpenses;

        private UILabel m_left;

        private UILabel m_leftExpenses;

        private UILabel m_center;

        private UILabel m_centerExpenses;

        private UILabel m_right;

        private UILabel m_rightExpenses;

        private UITemplateList<UIPanel> m_PoliciesList;

        public Color m_policyActiveColor;

        public Color m_policyInactiveColor;

        public Color m_requirementInProgressColor;

        public Color m_requirementCompletedColor;

        private UILabel m_productionProgress;

        private UILabel m_workersProgress;

        private ProgressBarUpdater m_progressBarUpdater;

        private UITabstrip m_tabStrip;

        private UITabContainer m_tabContainer;

        private UIPanel m_noSpecialization;

        private UILabel m_efficiency;

        private UILabel m_pollution;

        private UILabel m_profit;

        private UILabel m_levelBonusLabel;

        private UIRadialChart m_acquisitionPieChart;

        public Color m_colorOil;

        public Color m_colorPetroleum;

        public Color m_colorPlastics;

        public Color m_colorCrops;

        public Color m_colorFlours;

        public Color m_colorAnimalProducts;

        public Color m_colorLogs;

        public Color m_colorPaper;

        public Color m_colorPlanedTimber;

        public Color m_colorOre;

        public Color m_colorMetal;

        public Color m_colorGlass;

        public Color m_colorLuxuryProducts;

        public Color m_colorZonedForestryProducts;

        public Color m_colorZonedFarmingProducts;

        public Color m_colorZonedOreProducts;

        public Color m_colorZonedOilProducts;

        public Color m_colorFish;

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

        public UIPanel m_acquisitionPieChartSection;

        private UILabel m_upkeepLabel;

        private UIPanel m_mainBuildingOff;

        private UIPanel m_levelRequirements;

        private UISprite m_leftProduction;

        private UISprite m_rightProduction;

        private UIPanel m_leftCenterer;

        private UIPanel m_rightCenterer;

        private UISprite m_oilSprite;

        private UISprite m_fuelSprite;

        private UISprite m_plasticSprite;

        private UIComponent m_policiesTooltip;

        private static IndustryWorldInfoPanel m_instance;

        protected UIComponent policiesTooltip
        {
            get
            {
                if (m_policiesTooltip == null)
                {
                    m_policiesTooltip = UIView.Find("FootballPoliciesTooltip");
                }
                return m_policiesTooltip;
            }
        }

        private int rawResourcePrice => IndustryBuildingAI.GetResourcePrice(rawTransferReason);

        private TransferManager.TransferReason rawTransferReason => Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park].m_parkType switch
        {
            DistrictPark.ParkType.Farming => TransferManager.TransferReason.Grain,
            DistrictPark.ParkType.Forestry => TransferManager.TransferReason.Logs,
            DistrictPark.ParkType.Ore => TransferManager.TransferReason.Ore,
            _ => TransferManager.TransferReason.Oil,
        };

        private DistrictAreaResourceData rawResourceData => Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park].m_parkType switch
        {
            DistrictPark.ParkType.Farming => Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park].m_grainData,
            DistrictPark.ParkType.Forestry => Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park].m_logsData,
            DistrictPark.ParkType.Ore => Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park].m_oreData,
            _ => Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park].m_oilData,
        };

        private DistrictAreaResourceData leftResourceData => Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park].m_parkType switch
        {
            DistrictPark.ParkType.Farming => Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park].m_floursData,
            DistrictPark.ParkType.Forestry => Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park].m_paperData,
            DistrictPark.ParkType.Ore => Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park].m_metalsData,
            _ => Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park].m_petroleumData,
        };

        private TransferManager.TransferReason leftTransferReason => Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park].m_parkType switch
        {
            DistrictPark.ParkType.Farming => TransferManager.TransferReason.Flours,
            DistrictPark.ParkType.Forestry => TransferManager.TransferReason.Paper,
            DistrictPark.ParkType.Ore => TransferManager.TransferReason.Metals,
            _ => TransferManager.TransferReason.Petroleum,
        };

        private int leftResourcePrice => IndustryBuildingAI.GetResourcePrice(leftTransferReason);

        private DistrictAreaResourceData rightResourceData => Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park].m_parkType switch
        {
            DistrictPark.ParkType.Farming => Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park].m_animalProductsData,
            DistrictPark.ParkType.Forestry => Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park].m_planedTimberData,
            DistrictPark.ParkType.Ore => Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park].m_glassData,
            _ => Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park].m_plasticsData,
        };

        private TransferManager.TransferReason rightTransferReason => Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park].m_parkType switch
        {
            DistrictPark.ParkType.Farming => TransferManager.TransferReason.AnimalProducts,
            DistrictPark.ParkType.Forestry => TransferManager.TransferReason.PlanedTimber,
            DistrictPark.ParkType.Ore => TransferManager.TransferReason.Glass,
            _ => TransferManager.TransferReason.Plastics,
        };

        private int rightResourcePrice => IndustryBuildingAI.GetResourcePrice(rightTransferReason);

        public static IndustryWorldInfoPanel instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = UIView.library.Get<IndustryWorldInfoPanel>("IndustryWorldInfoPanel");
                }
                return m_instance;
            }
        }

        protected override void Start()
        {
            base.Start();
            m_ParkName = Find<UITextField>("AreaName");
            m_ParkName.eventTextSubmitted += OnRename;
            m_progressBarUpdater = new ProgressBarUpdater(Find<UIPanel>("LevelProgress"));
            m_size = Find<UILabel>("SizeLabel");
            m_oilLabel = Find<UILabel>("StorageOilLabel");
            m_plasticLabel = Find<UILabel>("StoragePlasticLabel");
            m_fuelLabel = Find<UILabel>("StorageFuelLabel");
            m_oilBuffer = Find<UIProgressBar>("OilBuffer");
            m_fuelBuffer = Find<UIProgressBar>("FuelBuffer");
            m_plasticBuffer = Find<UIProgressBar>("PlasticBuffer");
            m_oilBufferPanel = Find<UIPanel>("StorageOil");
            m_fuelBufferPanel = Find<UIPanel>("StorageFuel");
            m_plasticBufferPanel = Find<UIPanel>("StoragePlastic");
            m_fuelConversion = Find<UILabel>("FuelProduction");
            m_plasticConversion = Find<UILabel>("PlasticProduction");
            m_rawMaterialConsumption = Find<UILabel>("OilConsumption");
            m_acquisition = Find<UILabel>("AcquisitionPerWeek");
            m_acquisitionExpenses = Find<UILabel>("AcquisitionExpensesPerWeek");
            m_left = Find<UILabel>("FuelOutputPerWeek");
            m_leftExpenses = Find<UILabel>("FuelIncomePerWeek");
            m_center = Find<UILabel>("OilOutputPerWeek");
            m_centerExpenses = Find<UILabel>("OilIncomePerWeek");
            m_right = Find<UILabel>("PlasticOutputPerWeek");
            m_rightExpenses = Find<UILabel>("PlasticIncomePerWeek");
            m_productionProgress = Find<UILabel>("ProductionTilNextLevel");
            m_workersProgress = Find<UILabel>("WorkersTilNextLevel");
            m_PoliciesList = new UITemplateList<UIPanel>(Find<UIPanel>("PoliciesPanel"), "IndustryPolicyTemplate");
            m_tabContainer = Find<UITabContainer>("TabContainer");
            m_tabStrip = Find<UITabstrip>("Tabstrip");
            m_noSpecialization = Find<UIPanel>("NoSpecialization");
            m_efficiency = Find<UILabel>("EfficiencyLabel");
            m_pollution = Find<UILabel>("PollutionLabel");
            m_profit = Find<UILabel>("ProfitLabel");
            m_acquisitionPieChart = Find<UIRadialChart>("AcquisitionPieChart");
            m_levelBonusLabel = Find<UILabel>("LevelBonusLabel");
            InitPolicies();
            RefreshPolicyToolTips();
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
            m_acquisitionPieChartSection = Find<UIPanel>("AcquisitionPieChartSection");
            m_upkeepLabel = Find<UILabel>("Upkeep");
            m_mainBuildingOff = Find<UIPanel>("MainBuildingOff");
            m_levelRequirements = Find<UIPanel>("Progress");
            m_leftProduction = Find<UISprite>("ArrowHorizontalLeft");
            m_rightProduction = Find<UISprite>("ArrowHorizontalRight");
            m_leftCenterer = Find<UIPanel>("Centerer Left");
            m_rightCenterer = Find<UIPanel>("Centerer Right");
            m_oilSprite = Find<UISprite>("ResourceIconOil");
            m_plasticSprite = Find<UISprite>("ResourceIconPlastic");
            m_fuelSprite = Find<UISprite>("ResourceIconFuel");
        }

        private void InitPolicies()
        {
            PositionData<DistrictPolicies.Policies>[] orderedEnumData = ColossalFramework.Utils.GetOrderedEnumData<DistrictPolicies.Policies>("IndustryArea");
            m_PoliciesList.SetItemCount(orderedEnumData.Length);
            for (int i = 0; i < m_PoliciesList.items.Count; i++)
            {
                string enumName = orderedEnumData[i].enumName;
                m_PoliciesList.items[i].Find<UILabel>("PolicyName").text = Locale.Get("POLICIES", enumName);
                UIButton uIButton = m_PoliciesList.items[i].Find<UIButton>("PolicySpriteButtonForeground");
                uIButton.normalFgSprite = "IconPolicy" + enumName;
                uIButton.hoveredFgSprite = "IconPolicy" + enumName + "Hovered";
                uIButton.pressedFgSprite = "IconPolicy" + enumName + "Pressed";
                uIButton.objectUserData = i.ToString();
                uIButton.eventClick += OnPolicyClicked;
                UIButton uIButton2 = m_PoliciesList.items[i].Find<UIButton>("PolicyButton");
                uIButton2.objectUserData = i.ToString();
                uIButton2.eventClick += OnPolicyClicked;
            }
        }

        private void OnPolicyClicked(UIComponent comp, UIMouseEventParameter eventParam)
        {
            PositionData<DistrictPolicies.Policies>[] orderedEnumData = ColossalFramework.Utils.GetOrderedEnumData<DistrictPolicies.Policies>("IndustryArea");
            string s = comp.objectUserData as string;
            int result = -1;
            int.TryParse(s, out result);
            TogglePolicy(orderedEnumData[result].enumValue);
        }

        protected void TogglePolicy(DistrictPolicies.Policies policy)
        {
            if (Singleton<DistrictManager>.instance.IsParkPolicySet(policy, m_InstanceID.Park))
            {
                Singleton<SimulationManager>.instance.AddAction(delegate
                {
                    Singleton<DistrictManager>.instance.UnsetParkPolicy(policy, m_InstanceID.Park);
                });
            }
            else
            {
                Singleton<SimulationManager>.instance.AddAction(delegate
                {
                    Singleton<DistrictManager>.instance.SetParkPolicy(policy, m_InstanceID.Park);
                });
            }
        }

        private void RefreshPolicyToolTips()
        {
            PositionData<DistrictPolicies.Policies>[] orderedEnumData = ColossalFramework.Utils.GetOrderedEnumData<DistrictPolicies.Policies>("IndustryArea");
            for (int i = 0; i < m_PoliciesList.items.Count; i++)
            {
                UIButton uIButton = m_PoliciesList.items[i].Find<UIButton>("PolicyButton");
                UIButton uIButton2 = m_PoliciesList.items[i].Find<UIButton>("PolicySpriteButtonForeground");
                uIButton.tooltipBox = policiesTooltip;
                uIButton.tooltip = TooltipHelper.Format("title", Locale.Get("POLICIES", orderedEnumData[i].enumName), "text", Locale.Get("POLICIES_DETAIL", orderedEnumData[i].enumName));
                uIButton2.tooltipBox = policiesTooltip;
                uIButton2.tooltip = TooltipHelper.Format("title", Locale.Get("POLICIES", orderedEnumData[i].enumName), "text", Locale.Get("POLICIES_DETAIL", orderedEnumData[i].enumName));
            }
        }

        private void ColorButton(UIButton b, Color c)
        {
            b.color = c;
            b.focusedColor = c;
            b.hoveredColor = c;
            b.pressedColor = c;
        }

        private void RefreshPolicies()
        {
            PositionData<DistrictPolicies.Policies>[] orderedEnumData = ColossalFramework.Utils.GetOrderedEnumData<DistrictPolicies.Policies>("IndustryArea");
            for (int i = 0; i < m_PoliciesList.items.Count; i++)
            {
                DistrictPolicies.Policies enumValue = orderedEnumData[i].enumValue;
                ColorButton(m_PoliciesList.items[i].Find<UIButton>("PolicyButton"), (!Singleton<DistrictManager>.instance.IsParkPolicySet(enumValue, m_InstanceID.Park)) ? m_policyInactiveColor : m_policyActiveColor);
                ColorButton(m_PoliciesList.items[i].Find<UIButton>("PolicySpriteButton"), (!Singleton<DistrictManager>.instance.IsParkPolicySet(enumValue, m_InstanceID.Park)) ? m_policyInactiveColor : m_policyActiveColor);
            }
        }

        private void OnEnable()
        {
            if (SingletonLite<LocaleManager>.exists)
            {
                LocaleManager.eventLocaleChanged += OnLocaleChanged;
            }
        }

        private void OnLocaleChanged()
        {
            InitPolicies();
            RefreshPolicyToolTips();
        }

        private void OnDisable()
        {
            if (SingletonLite<LocaleManager>.exists)
            {
                LocaleManager.eventLocaleChanged -= OnLocaleChanged;
            }
        }

        protected override void OnSetTarget()
        {
            m_ParkName.text = GetName();
            if (Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park].m_parkType == DistrictPark.ParkType.Industry)
            {
                m_tabContainer.isVisible = false;
                m_tabStrip.isVisible = false;
                m_noSpecialization.isVisible = true;
                return;
            }
            m_tabContainer.isVisible = true;
            m_tabStrip.isVisible = true;
            m_noSpecialization.isVisible = false;
            m_currentIndustryType = Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park].m_parkType.ToString();
            m_oilLabel.text = Locale.Get("INDUSTRYPANEL_RAWRESOURCE", m_currentIndustryType);
            m_fuelLabel.text = Locale.Get("INDUSTRYPANEL_REFINEDRESOURCE_LEFT", m_currentIndustryType);
            m_plasticLabel.text = Locale.Get("INDUSTRYPANEL_REFINEDRESOURCE_RIGHT", m_currentIndustryType);
            m_oilSprite.spriteName = ResourceSpriteName(rawTransferReason);
            m_fuelSprite.spriteName = ResourceSpriteName(leftTransferReason);
            m_plasticSprite.spriteName = ResourceSpriteName(rightTransferReason);
            m_oilBuffer.progressColor = GetResourceColor(rawTransferReason);
            m_fuelBuffer.progressColor = GetResourceColor(leftTransferReason);
            m_plasticBuffer.progressColor = GetResourceColor(rightTransferReason);
            float totalUpkeepScaled = 0f;
            UpdateWorkersAndTotalUpkeep(out totalUpkeepScaled, onSetTarget: true);
        }

        protected override void UpdateBindings()
        {
            base.UpdateBindings();
            byte park = m_InstanceID.Park;
            DistrictManager districtManager = Singleton<DistrictManager>.instance;
            m_size.text = StringUtils.SafeFormat(Locale.Get("PARKINFOPANEL_SIZE"), Mathf.RoundToInt(districtManager.m_parks.m_buffer[park].sizeInCells));
            DistrictAreaResourceData districtAreaResourceData = rawResourceData;
            m_oilBuffer.value = SafelyNormalize(rawResourceData.m_finalBufferAmount, rawResourceData.m_finalBufferCapacity);
            m_fuelBuffer.value = SafelyNormalize(leftResourceData.m_finalBufferAmount, leftResourceData.m_finalBufferCapacity);
            m_plasticBuffer.value = SafelyNormalize(rightResourceData.m_finalBufferAmount, rightResourceData.m_finalBufferCapacity);
            m_oilBufferPanel.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), FormatResource(rawResourceData.m_finalBufferAmount), FormatResourceWithUnit(rawResourceData.m_finalBufferCapacity, rawTransferReason));
            m_fuelBufferPanel.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), FormatResource(leftResourceData.m_finalBufferAmount), FormatResourceWithUnit(leftResourceData.m_finalBufferCapacity, leftTransferReason));
            m_plasticBufferPanel.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), FormatResource(rightResourceData.m_finalBufferAmount), FormatResourceWithUnit(rightResourceData.m_finalBufferCapacity, rightTransferReason));
            m_rawMaterialConsumption.text = FormatResourceWithUnit(rawResourceData.m_finalConsumption + rawResourceData.m_finalExport, rawTransferReason);
            m_fuelConversion.text = FormatResourceWithUnit(leftResourceData.m_finalProduction, leftTransferReason);
            m_plasticConversion.text = FormatResourceWithUnit(rightResourceData.m_finalProduction, rightTransferReason);
            uint finalProduction = rawResourceData.m_finalProduction;
            uint num = rawResourceData.m_finalImport + finalProduction;
            m_acquisition.text = FormatResourceWithUnit(num, rawTransferReason);
            long num2 = rawResourceData.m_finalImport * rawResourcePrice / 10000;
            m_acquisitionExpenses.text = num2.ToString(Settings.moneyFormatNoCents, LocaleManager.cultureInfo);
            m_acquisitionPieChartSection.isVisible = finalProduction + num != 0;
            uint finalExport = rawResourceData.m_finalExport;
            m_center.text = FormatResourceWithUnit(finalExport, rawTransferReason);
            long num3 = finalExport * rawResourcePrice / 10000;
            m_centerExpenses.text = num3.ToString(Settings.moneyFormatNoCents, LocaleManager.cultureInfo);
            uint finalExport2 = leftResourceData.m_finalExport;
            m_left.text = FormatResourceWithUnit(finalExport2, leftTransferReason);
            long num4 = finalExport2 * leftResourcePrice / 10000;
            m_leftExpenses.text = num4.ToString(Settings.moneyFormatNoCents, LocaleManager.cultureInfo);
            uint finalExport3 = rightResourceData.m_finalExport;
            m_right.text = FormatResourceWithUnit(finalExport3, rightTransferReason);
            long num5 = finalExport3 * rightResourcePrice / 10000;
            m_rightExpenses.text = num5.ToString(Settings.moneyFormatNoCents, LocaleManager.cultureInfo);
            float num6 = 0f;
            if (num != 0)
            {
                num6 = (float)finalProduction / (float)num;
            }
            m_acquisitionPieChart.GetSlice(0).endValue = num6;
            m_acquisitionPieChart.GetSlice(1).startValue = num6;
            float totalUpkeepScaled = 0f;
            UpdateWorkersAndTotalUpkeep(out totalUpkeepScaled);
            long num7 = num3 + num5 + num4 - num2 + (long)totalUpkeepScaled;
            m_profit.text = num7.ToString(Settings.moneyFormatNoCents, LocaleManager.cultureInfo);
            RefreshPolicies();
            if (districtManager.m_parks.m_buffer[park].m_parkLevel != DistrictPark.ParkLevel.Level5)
            {
                ulong totalProductionAmount = districtManager.m_parks.m_buffer[park].m_totalProductionAmount;
                uint productionLevelupRequirement = (uint)districtManager.m_properties.m_parkProperties.m_industryLevelInfo[ParkLevel(park)].m_productionLevelupRequirement;
                m_productionProgress.text = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_PRODUCTIONREQUIREMENT"), FormatResource(totalProductionAmount), FormatResource(productionLevelupRequirement));
                m_productionProgress.textColor = ((totalProductionAmount < productionLevelupRequirement) ? m_requirementInProgressColor : m_requirementCompletedColor);
                ushort finalWorkerCount = districtManager.m_parks.m_buffer[park].m_finalWorkerCount;
                int workerLevelupRequirement = districtManager.m_properties.m_parkProperties.m_industryLevelInfo[ParkLevel(park)].m_workerLevelupRequirement;
                m_workersProgress.text = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_WORKERSREQUIREMENT"), finalWorkerCount, workerLevelupRequirement);
                m_workersProgress.textColor = ((finalWorkerCount < workerLevelupRequirement) ? m_requirementInProgressColor : m_requirementCompletedColor);
            }
            else
            {
                m_productionProgress.text = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_PRODUCTION"), FormatResource(districtManager.m_parks.m_buffer[park].m_totalProductionAmount));
                m_workersProgress.text = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_WORKERS"), districtManager.m_parks.m_buffer[park].m_finalWorkerCount);
                m_productionProgress.textColor = m_requirementInProgressColor;
                m_workersProgress.textColor = m_requirementInProgressColor;
            }
            m_progressBarUpdater.Update(park);
            bool flag = districtManager.m_parks.m_buffer[park].m_parkLevel != DistrictPark.ParkLevel.Level1 && districtManager.m_parks.m_buffer[park].m_parkLevel != DistrictPark.ParkLevel.None;
            ushort mainGate = districtManager.m_parks.m_buffer[park].m_mainGate;
            bool flag2 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[mainGate].m_productionRate == 0 || (Singleton<BuildingManager>.instance.m_buildings.m_buffer[mainGate].m_flags & Building.Flags.Collapsed) != 0;
            if (flag2)
            {
                flag = false;
            }
            m_mainBuildingOff.isVisible = flag2;
            m_workersProgress.isVisible = !flag2;
            m_productionProgress.isVisible = !flag2;
            m_levelBonusLabel.isVisible = flag;
            m_efficiency.isVisible = flag;
            m_pollution.isVisible = flag;
            m_levelRequirements.isVisible = !flag2;
            if (flag)
            {
                districtManager.m_parks.m_buffer[park].GetProductionFactors(out var processingFactor, out var pollutionFactor);
                m_efficiency.text = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_EFFICIENCY"), (-(100 - processingFactor)).ToString("+0;-#"));
                m_pollution.text = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_POLLUTION"), -(100 - pollutionFactor));
            }
        }

        private int ParkLevel(int parkID)
        {
            return (int)Singleton<DistrictManager>.instance.m_parks.m_buffer[parkID].m_parkLevel;
        }

        public static float SafelyNormalize(float value, float maxValue)
        {
            if (maxValue == 0f)
            {
                return 0f;
            }
            return value / maxValue;
        }

        public static float SafelyNormalize(int value, int maxValue)
        {
            if (maxValue == 0)
            {
                return 0f;
            }
            return (float)value / (float)maxValue;
        }

        private void OnRename(UIComponent comp, string text)
        {
            StartCoroutine(SetName(text));
        }

        private IEnumerator SetName(string newName)
        {
            if (Singleton<SimulationManager>.exists)
            {
                if (m_InstanceID.Type == InstanceType.Park && m_InstanceID.Park != 0)
                {
                    AsyncTask<bool> task = Singleton<SimulationManager>.instance.AddAction(Singleton<DistrictManager>.instance.SetParkName(m_InstanceID.Park, newName));
                    yield return task.WaitTaskCompleted(this);
                }
                m_ParkName.text = GetName();
            }
        }

        private string GetName()
        {
            if (m_InstanceID.Type == InstanceType.Park && m_InstanceID.Park != 0)
            {
                return Singleton<DistrictManager>.instance.GetParkName(m_InstanceID.Park);
            }
            return string.Empty;
        }

        public static string FormatResource(ulong amount)
        {
            float num = amount;
            num /= 1000f;
            return Mathf.Round(num).ToString();
        }

        public static string FormatResourceWithUnit(uint amount, TransferManager.TransferReason type)
        {
            string text = "N/A";
            return string.Concat(str2: (type != TransferManager.TransferReason.Oil && type != TransferManager.TransferReason.Petroleum && type != TransferManager.TransferReason.Petrol) ? Locale.Get("RESOURCEUNIT_TONS") : Locale.Get("RESOURCEUNIT_BARRELS"), str0: FormatResource(amount), str1: " ");
        }

        public Color GetResourceColor(TransferManager.TransferReason resource)
        {
            return resource switch
            {
                TransferManager.TransferReason.Grain => m_colorCrops,
                TransferManager.TransferReason.AnimalProducts => m_colorAnimalProducts,
                TransferManager.TransferReason.Flours => m_colorFlours,
                TransferManager.TransferReason.Logs => m_colorLogs,
                TransferManager.TransferReason.Paper => m_colorPaper,
                TransferManager.TransferReason.PlanedTimber => m_colorPlanedTimber,
                TransferManager.TransferReason.Ore => m_colorOre,
                TransferManager.TransferReason.Glass => m_colorGlass,
                TransferManager.TransferReason.Metals => m_colorMetal,
                TransferManager.TransferReason.Oil => m_colorOil,
                TransferManager.TransferReason.Petroleum => m_colorPetroleum,
                TransferManager.TransferReason.Plastics => m_colorPlastics,
                TransferManager.TransferReason.LuxuryProducts => m_colorLuxuryProducts,
                TransferManager.TransferReason.Lumber => m_colorZonedForestryProducts,
                TransferManager.TransferReason.Food => m_colorZonedFarmingProducts,
                TransferManager.TransferReason.Coal => m_colorZonedOreProducts,
                TransferManager.TransferReason.Petrol => m_colorZonedOilProducts,
                TransferManager.TransferReason.Fish => m_colorFish,
                _ => Color.Lerp(Color.grey, Color.black, 0.2f),
            };
        }

        public void OpenIndustryOverviewPanel()
        {
            UIView.library.Show<IndustryOverviewPanel>("IndustryOverviewPanel");
        }

        private void UpdateWorkersAndTotalUpkeep(out float totalUpkeepScaled, bool onSetTarget = false)
        {
            totalUpkeepScaled = 0f;
            bool flag = false;
            bool flag2 = false;
            TransferManager.TransferReason transferReason = rightTransferReason;
            TransferManager.TransferReason transferReason2 = leftTransferReason;
            if (!Singleton<CitizenManager>.exists)
            {
                return;
            }
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            int num6 = 0;
            int num7 = 0;
            int num8 = 0;
            int num9 = 0;
            int num10 = 0;
            FastList<ushort> serviceBuildings = buildingManager.GetServiceBuildings(ItemClass.Service.PlayerIndustry);
            MainIndustryBuildingAI mainIndustryBuildingAI = buildingManager.m_buildings.m_buffer[Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park].m_mainGate].Info.m_buildingAI as MainIndustryBuildingAI;
            for (int i = 0; i < serviceBuildings.m_size; i++)
            {
                ushort num11 = serviceBuildings.m_buffer[i];
                byte park = Singleton<DistrictManager>.instance.GetPark(buildingManager.m_buildings.m_buffer[num11].m_position);
                if (park != m_InstanceID.Park)
                {
                    continue;
                }
                uint num12 = buildingManager.m_buildings.m_buffer[num11].m_citizenUnits;
                int num13 = 0;
                IndustryBuildingAI industryBuildingAI = buildingManager.m_buildings.m_buffer[num11].Info.GetAI() as IndustryBuildingAI;
                MainIndustryBuildingAI mainIndustryBuildingAI2 = buildingManager.m_buildings.m_buffer[num11].Info.GetAI() as MainIndustryBuildingAI;
                if (industryBuildingAI != null && mainIndustryBuildingAI != null && industryBuildingAI.m_industryType == mainIndustryBuildingAI.m_industryType)
                {
                    if (onSetTarget)
                    {
                        ProcessingFacilityAI processingFacilityAI = industryBuildingAI as ProcessingFacilityAI;
                        if (processingFacilityAI != null)
                        {
                            if (processingFacilityAI.m_outputResource == transferReason)
                            {
                                flag = true;
                            }
                            if (processingFacilityAI.m_outputResource == transferReason2)
                            {
                                flag2 = true;
                            }
                        }
                    }
                    num10 += industryBuildingAI.GetResourceRate(num11, ref buildingManager.m_buildings.m_buffer[num11], EconomyManager.Resource.Maintenance);
                    num2 += industryBuildingAI.m_workPlaceCount0;
                    num3 += industryBuildingAI.m_workPlaceCount1;
                    num4 += industryBuildingAI.m_workPlaceCount2;
                    num5 += industryBuildingAI.m_workPlaceCount3;
                    while (num12 != 0)
                    {
                        uint nextUnit = citizenManager.m_units.m_buffer[num12].m_nextUnit;
                        if ((citizenManager.m_units.m_buffer[num12].m_flags & CitizenUnit.Flags.Work) != 0)
                        {
                            for (int j = 0; j < 5; j++)
                            {
                                uint citizen = citizenManager.m_units.m_buffer[num12].GetCitizen(j);
                                if (citizen != 0 && !citizenManager.m_citizens.m_buffer[citizen].Dead && (citizenManager.m_citizens.m_buffer[citizen].m_flags & Citizen.Flags.MovingIn) == 0)
                                {
                                    num++;
                                    switch (citizenManager.m_citizens.m_buffer[citizen].EducationLevel)
                                    {
                                        case Citizen.Education.Uneducated:
                                            num6++;
                                            break;
                                        case Citizen.Education.OneSchool:
                                            num7++;
                                            break;
                                        case Citizen.Education.TwoSchools:
                                            num8++;
                                            break;
                                        case Citizen.Education.ThreeSchools:
                                            num9++;
                                            break;
                                    }
                                }
                            }
                        }
                        num12 = nextUnit;
                        if (++num13 > 524288)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
                else
                {
                    if (!(mainIndustryBuildingAI2 != null))
                    {
                        continue;
                    }
                    num10 += mainIndustryBuildingAI2.GetResourceRate(num11, ref buildingManager.m_buildings.m_buffer[num11], EconomyManager.Resource.Maintenance);
                    num2 += mainIndustryBuildingAI2.m_workPlaceCount0;
                    num3 += mainIndustryBuildingAI2.m_workPlaceCount1;
                    num4 += mainIndustryBuildingAI2.m_workPlaceCount2;
                    num5 += mainIndustryBuildingAI2.m_workPlaceCount3;
                    while (num12 != 0)
                    {
                        uint nextUnit2 = citizenManager.m_units.m_buffer[num12].m_nextUnit;
                        if ((citizenManager.m_units.m_buffer[num12].m_flags & CitizenUnit.Flags.Work) != 0)
                        {
                            for (int k = 0; k < 5; k++)
                            {
                                uint citizen2 = citizenManager.m_units.m_buffer[num12].GetCitizen(k);
                                if (citizen2 != 0 && !citizenManager.m_citizens.m_buffer[citizen2].Dead && (citizenManager.m_citizens.m_buffer[citizen2].m_flags & Citizen.Flags.MovingIn) == 0)
                                {
                                    num++;
                                    switch (citizenManager.m_citizens.m_buffer[citizen2].EducationLevel)
                                    {
                                        case Citizen.Education.Uneducated:
                                            num6++;
                                            break;
                                        case Citizen.Education.OneSchool:
                                            num7++;
                                            break;
                                        case Citizen.Education.TwoSchools:
                                            num8++;
                                            break;
                                        case Citizen.Education.ThreeSchools:
                                            num9++;
                                            break;
                                    }
                                }
                            }
                        }
                        num12 = nextUnit2;
                        if (++num13 > 524288)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            if (onSetTarget)
            {
                m_currentIndustryType = Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park].m_parkType.ToString();
                m_leftProduction.isEnabled = flag2;
                m_rightProduction.isEnabled = flag;
                string text = ((!flag2) ? LocaleFormatter.FormatGeneric("INDUSTRYPANEL_NOPRODUCTIONBUILDING", Locale.Get("INDUSTRYPANEL_REFINEDRESOURCE_LEFT", m_currentIndustryType)) : string.Empty);
                string text2 = ((!flag) ? LocaleFormatter.FormatGeneric("INDUSTRYPANEL_NOPRODUCTIONBUILDING", Locale.Get("INDUSTRYPANEL_REFINEDRESOURCE_RIGHT", m_currentIndustryType)) : string.Empty);
                UISprite leftProduction = m_leftProduction;
                string tooltip = text;
                m_leftCenterer.tooltip = tooltip;
                leftProduction.tooltip = tooltip;
                UISprite rightProduction = m_rightProduction;
                tooltip = text2;
                m_rightCenterer.tooltip = tooltip;
                rightProduction.tooltip = tooltip;
            }
            m_upkeepLabel.text = LocaleFormatter.FormatUpkeep(num10, isDistanceBased: false);
            totalUpkeepScaled = (float)num10 * 0.0016f;
            int num14 = 0;
            int num15 = num2 - num6;
            int num16 = num2 + num3 + num4 + num5;
            if (num7 > num3)
            {
                num14 += Mathf.Max(0, Mathf.Min(num15, num7 - num3));
            }
            num15 += num3 - num7;
            if (num8 > num4)
            {
                num14 += Mathf.Max(0, Mathf.Min(num15, num8 - num4));
            }
            num15 += num4 - num8;
            if (num9 > num5)
            {
                num14 += Mathf.Max(0, Mathf.Min(num15, num9 - num5));
            }
            string format = Locale.Get((num14 != 1) ? "ZONEDBUILDING_OVEREDUCATEDWORKERS" : "ZONEDBUILDING_OVEREDUCATEDWORKER");
            m_OverWorkSituation.text = StringUtils.SafeFormat(format, num14);
            m_OverWorkSituation.isVisible = num14 > 0;
            m_UneducatedPlaces.text = num2.ToString();
            m_EducatedPlaces.text = num3.ToString();
            m_WellEducatedPlaces.text = num4.ToString();
            m_HighlyEducatedPlaces.text = num5.ToString();
            m_UneducatedWorkers.text = num6.ToString();
            m_EducatedWorkers.text = num7.ToString();
            m_WellEducatedWorkers.text = num8.ToString();
            m_HighlyEducatedWorkers.text = num9.ToString();
            m_JobsAvailLegend.text = (num16 - (num6 + num7 + num8 + num9)).ToString();
            int num17 = GetValue(num2, num16);
            int value = GetValue(num3, num16);
            int value2 = GetValue(num4, num16);
            int value3 = GetValue(num5, num16);
            int num18 = num17 + value + value2 + value3;
            if (num18 != 0 && num18 != 100)
            {
                num17 = 100 - (value + value2 + value3);
            }
            m_WorkPlacesEducationChart.SetValues(num17, value, value2, value3);
            int value4 = GetValue(num6, num16);
            int value5 = GetValue(num7, num16);
            int value6 = GetValue(num8, num16);
            int value7 = GetValue(num9, num16);
            int num19 = 0;
            int num20 = value4 + value5 + value6 + value7;
            num19 = 100 - num20;
            m_WorkersEducationChart.SetValues(value4, value5, value6, value7, num19);
            ushort finalWorkerCount = Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park].m_finalWorkerCount;
            m_workersInfoLabel.text = StringUtils.SafeFormat(Locale.Get("ZONEDBUILDING_WORKERS"), finalWorkerCount, num16);
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

        public static string ResourceSpriteName(TransferManager.TransferReason transferReason, bool isStorageBuilding = false)
        {
            string text = transferReason.ToString();
            text = text.Replace("Grain", "Crops");
            text = text.Replace("Flours", "flours");
            text = text.Replace("Metals", "Metal");
            text = "resourceIcon" + text;
            if (isStorageBuilding)
            {
                text += "Storage";
                if (transferReason == TransferManager.TransferReason.None)
                {
                    text = "resourceIconWarehouse";
                }
            }
            return text;
        }
    }
}

