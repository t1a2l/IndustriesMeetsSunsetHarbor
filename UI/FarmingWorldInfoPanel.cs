using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using IndustriesMeetsSunsetHarbor.Managers;
using MoreTransferReasons;
using UnityEngine;

/// <summary>
/// Extended Industry Info Panel for the custom Farming specialization.
/// Supports: Fruits/Vegetables/Grain/Cotton → Cows/HighlandCows/Sheep/Pigs → 8 products
/// </summary>
public class FarmingWorldInfoPanel : WorldInfoPanel
{
    // -------------------------------------------------------------------------
    // Progress bar helper (identical to original)
    // -------------------------------------------------------------------------
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
            m_inProgressColor = new Color32(154, 189, 221, 255);
            m_allCompletedColor = Color.yellow;
            m_parentPanel = panel;
            m_bars = new UIProgressBar[5];
            for (int i = 0; i < 5; i++)
                m_bars[i] = panel.Find<UIProgressBar>("Level" + (i + 1) + "Bar");
        }

        public void Update(int parkID)
        {
            if (parkID <= -1) return;
            int level = ParkLevel(parkID);
            int i;
            for (i = 0; i < level; i++)
            {
                m_bars[i].value = 1f;
                m_bars[i].progressColor = (level < 5) ? m_normalColor : m_allCompletedColor;
            }
            if (i < 5)
            {
                m_bars[i].value = IndustryWorldInfoPanel.ProgressBarUpdater.ProgressToNextLevel(parkID);
                m_bars[i].progressColor = m_inProgressColor;
                i++;
            }
            for (; i < 5; i++)
                m_bars[i].value = 0f;

            m_parentPanel.tooltip = StringUtils.SafeFormat(
                Locale.Get("PARKINFOPANEL_LEVEL"),
                level.ToString());
        }

        public static int ParkLevel(int parkID) =>
            (int)Singleton<DistrictManager>.instance.m_parks.m_buffer[parkID].m_parkLevel;
    }

    // -------------------------------------------------------------------------
    // UI Fields — header / common
    // -------------------------------------------------------------------------
    private UITextField m_ParkName;
    private UILabel m_size;
    private UILabel m_upkeepLabel;
    private UILabel m_profit;
    private UILabel m_efficiency;
    private UILabel m_pollution;
    private UILabel m_levelBonusLabel;
    private UILabel m_productionProgress;
    private UILabel m_workersProgress;
    private UIPanel m_mainBuildingOff;
    private UIPanel m_levelRequirements;
    private UIPanel m_noSpecialization;
    private UITabstrip m_tabStrip;
    private UITabContainer m_tabContainer;
    private ProgressBarUpdater m_progressBarUpdater;
    private UITemplateList<UIPanel> m_PoliciesList;

    // Education / workers tooltip (unchanged from original)
    private UIPanel m_workersTooltip;
    private UILabel m_workersInfoLabel;
    private UILabel m_OverWorkSituation;
    private UILabel m_UneducatedPlaces, m_EducatedPlaces, m_WellEducatedPlaces, m_HighlyEducatedPlaces;
    private UILabel m_UneducatedWorkers, m_EducatedWorkers, m_WellEducatedWorkers, m_HighlyEducatedWorkers;
    private UILabel m_JobsAvailLegend;
    private UIRadialChart m_WorkPlacesEducationChart;
    private UIRadialChart m_WorkersEducationChart;
    private UIComponent m_policiesTooltip;

    // -------------------------------------------------------------------------
    // UI Fields — Acquisition row (4 raw materials)
    // -------------------------------------------------------------------------
    private UILabel m_fruitsAcquisition;        // "xx tons" label on Fruits node arrow
    private UILabel m_vegetablesAcquisition;
    private UILabel m_grainAcquisition;
    private UILabel m_cottonAcquisition;

    private UILabel m_fruitsExpenses;           // import cost label (€0 or €xx)
    private UILabel m_vegetablesExpenses;
    private UILabel m_grainExpenses;
    private UILabel m_cottonExpenses;

    private UIRadialChart m_fruitsPieChart;
    private UIRadialChart m_vegetablesPieChart;
    private UIRadialChart m_grainPieChart;
    private UIRadialChart m_cottonPieChart;

    private UIPanel m_fruitsPieChartSection;
    private UIPanel m_vegetablesPieChartSection;
    private UIPanel m_grainPieChartSection;
    private UIPanel m_cottonPieChartSection;

    // -------------------------------------------------------------------------
    // UI Fields — Animal row (middle level)
    // -------------------------------------------------------------------------
    private UILabel m_cowsInputTons;            // tons label on arrow above Cows box
    private UILabel m_highlandCowsInputTons;
    private UILabel m_sheepInputTons;
    private UILabel m_pigsInputTons;

    // -------------------------------------------------------------------------
    // UI Fields — Output row (8 products)
    // Naming: <product>Output = "xx tons", <product>Income = "€x,xxx"
    // -------------------------------------------------------------------------
    private UILabel m_beefMeatOutput;
    private UILabel m_beefMeatIncome;

    private UILabel m_cowMilkOutput;
    private UILabel m_cowMilkIncome;

    private UILabel m_highlandBeefOutput;
    private UILabel m_highlandBeefIncome;

    private UILabel m_highlandMilkOutput;
    private UILabel m_highlandMilkIncome;

    private UILabel m_lambMeatOutput;
    private UILabel m_lambMeatIncome;

    private UILabel m_sheepMilkOutput;
    private UILabel m_sheepMilkIncome;

    private UILabel m_woolOutput;
    private UILabel m_woolIncome;

    private UILabel m_porkMeatOutput;
    private UILabel m_porkMeatIncome;

    // -------------------------------------------------------------------------
    // Colors (serialized, set in Unity Inspector or via code)
    // -------------------------------------------------------------------------
    public Color m_colorFruits;
    public Color m_colorVegetables;
    public Color m_colorGrain;
    public Color m_colorCotton;
    public Color m_colorCows;
    public Color m_colorHighlandCows;
    public Color m_colorSheep;
    public Color m_colorPigs;
    public Color m_colorBeefMeat;
    public Color m_colorCowMilk;
    public Color m_colorHighlandBeef;
    public Color m_colorHighlandMilk;
    public Color m_colorLambMeat;
    public Color m_colorSheepMilk;
    public Color m_colorWool;
    public Color m_colorPorkMeat;

    public Color32 m_UneducatedColor;
    public Color32 m_EducatedColor;
    public Color32 m_WellEducatedColor;
    public Color32 m_HighlyEducatedColor;
    public Color32 m_UnoccupiedWorkplaceColor;
    public float m_WorkersColorScalar = 0.5f;

    public Color m_policyActiveColor;
    public Color m_policyInactiveColor;
    public Color m_requirementInProgressColor;
    public Color m_requirementCompletedColor;

    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------
    private static FarmingWorldInfoPanel m_instance;
    public static FarmingWorldInfoPanel instance
    {
        get
        {
            if (m_instance == null)
                m_instance = UIView.library.Get<FarmingWorldInfoPanel>("FarmingWorldInfoPanel");
            return m_instance;
        }
    }

    // -------------------------------------------------------------------------
    // Policies tooltip (same pattern as original)
    // -------------------------------------------------------------------------
    protected UIComponent policiesTooltip
    {
        get
        {
            if (m_policiesTooltip == null)
                m_policiesTooltip = UIView.Find("FootballPoliciesTooltip");
            return m_policiesTooltip;
        }
    }

    // -------------------------------------------------------------------------
    // Data accessors — replace CustomTransferReason.XXX with your actual
    // TransferReason values (or custom resource enum) from your mod
    // -------------------------------------------------------------------------

    // Raw material data from DistrictPark buffer
    // TODO: Replace with your actual DistrictAreaResourceData fields m_InstanceID.Park
    private DistrictAreaResourceData fruitsData => FarmingParkDataManager.GetFarmingPark(m_InstanceID.Park).m_fruitsData;
    private DistrictAreaResourceData vegetablesData => FarmingParkDataManager.GetFarmingPark(m_InstanceID.Park).m_vegetablesData;
    private DistrictAreaResourceData grainData => GetPark().m_grainData;
    private DistrictAreaResourceData cottonData => FarmingParkDataManager.GetFarmingPark(m_InstanceID.Park).m_cottonData;

    // Animal intermediate data
    private DistrictAreaResourceData cowsData => FarmingParkDataManager.GetFarmingPark(m_InstanceID.Park).m_cowsData;
    private DistrictAreaResourceData highlandCowsData => FarmingParkDataManager.GetFarmingPark(m_InstanceID.Park).m_highlandCowsData;
    private DistrictAreaResourceData sheepData => FarmingParkDataManager.GetFarmingPark(m_InstanceID.Park).m_sheepData;
    private DistrictAreaResourceData pigsData => FarmingParkDataManager.GetFarmingPark(m_InstanceID.Park).m_pigsData;

    // Product output data
    private DistrictAreaResourceData beefMeatData => FarmingParkDataManager.GetFarmingPark(m_InstanceID.Park).m_beefMeatData;
    private DistrictAreaResourceData cowMilkData => FarmingParkDataManager.GetFarmingPark(m_InstanceID.Park).m_cowMilkData;
    private DistrictAreaResourceData highlandBeefData => FarmingParkDataManager.GetFarmingPark(m_InstanceID.Park).m_highlandBeefData;
    private DistrictAreaResourceData highlandMilkData => FarmingParkDataManager.GetFarmingPark(m_InstanceID.Park).m_highlandMilkData;
    private DistrictAreaResourceData lambMeatData => FarmingParkDataManager.GetFarmingPark(m_InstanceID.Park).m_lambMeatData;
    private DistrictAreaResourceData sheepMilkData => FarmingParkDataManager.GetFarmingPark(m_InstanceID.Park).m_sheepMilkData;
    private DistrictAreaResourceData woolData => FarmingParkDataManager.GetFarmingPark(m_InstanceID.Park).m_woolData;
    private DistrictAreaResourceData porkMeatData => FarmingParkDataManager.GetFarmingPark(m_InstanceID.Park).m_porkMeatData;

    private ref DistrictPark GetPark() => ref Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park];

    private void InitEducationChartColors()
    {
        Color[] colors = { m_UneducatedColor, m_EducatedColor, m_WellEducatedColor, m_HighlyEducatedColor };
        for (int i = 0; i < 4; i++)
        {
            var wpSlice = m_WorkPlacesEducationChart.GetSlice(i);
            Color32 c = colors[i];
            wpSlice.outterColor = c;
            wpSlice.innerColor = c;

            var wSlice = m_WorkersEducationChart.GetSlice(i);
            Color32 dim = MultiplyColor(colors[i], m_WorkersColorScalar);
            wSlice.outterColor = dim;
            wSlice.innerColor = dim;
        }
        var unoccupied = m_WorkersEducationChart.GetSlice(4);
        unoccupied.outterColor = m_UnoccupiedWorkplaceColor;
        unoccupied.innerColor = m_UnoccupiedWorkplaceColor;

        m_UneducatedPlaces.color = m_UneducatedColor;
        m_EducatedPlaces.color = m_EducatedColor;
        m_WellEducatedPlaces.color = m_WellEducatedColor;
        m_HighlyEducatedPlaces.color = m_HighlyEducatedColor;

        m_UneducatedWorkers.color = MultiplyColor(m_UneducatedColor, m_WorkersColorScalar);
        m_EducatedWorkers.color = MultiplyColor(m_EducatedColor, m_WorkersColorScalar);
        m_WellEducatedWorkers.color = MultiplyColor(m_WellEducatedColor, m_WorkersColorScalar);
        m_HighlyEducatedWorkers.color = MultiplyColor(m_HighlyEducatedColor, m_WorkersColorScalar);
    }

    // -------------------------------------------------------------------------
    // OnSetTarget — called when clicking a farming area in-game
    // -------------------------------------------------------------------------
    protected override void OnSetTarget()
    {
        m_ParkName.text = GetName();

        // Only show full UI for Farming type
        if (Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park].m_parkType
            != DistrictPark.ParkType.Farming)
        {
            m_tabContainer.isVisible = false;
            m_tabStrip.isVisible = false;
            m_noSpecialization.isVisible = true;
            return;
        }

        m_tabContainer.isVisible = true;
        m_tabStrip.isVisible = true;
        m_noSpecialization.isVisible = false;

        float dummy;
        UpdateWorkersAndTotalUpkeep(out dummy, onSetTarget: true);
    }

    // -------------------------------------------------------------------------
    // UpdateBindings — called every simulation tick while panel is open
    // -------------------------------------------------------------------------
    protected override void UpdateBindings()
    {
        base.UpdateBindings();

        byte park = m_InstanceID.Park;
        var dm = Singleton<DistrictManager>.instance;

        m_size.text = StringUtils.SafeFormat(
            Locale.Get("PARKINFOPANEL_SIZE"),
            Mathf.RoundToInt(dm.m_parks.m_buffer[park].sizeInCells));

        // --- Acquisition row ---
        UpdateAcquisitionLabel(m_fruitsAcquisition, m_fruitsExpenses,
            m_fruitsPieChart, m_fruitsPieChartSection,
            fruitsData, ExtendedTransferManager.Fruits);

        UpdateAcquisitionLabel(m_vegetablesAcquisition, m_vegetablesExpenses,
            m_vegetablesPieChart, m_vegetablesPieChartSection,
            vegetablesData, ExtendedTransferManager.Vegetables);

        UpdateAcquisitionLabel(m_grainAcquisition, m_grainExpenses,
            m_grainPieChart, m_grainPieChartSection,
            grainData, TransferManager.TransferReason.Grain);

        UpdateAcquisitionLabel(m_cottonAcquisition, m_cottonExpenses,
            m_cottonPieChart, m_cottonPieChartSection,
            cottonData, ExtendedTransferManager.Cotton);

        // --- Animal input arrows (tons consumed by each animal group) ---
        m_cowsInputTons.text = IndustryWorldInfoPanel.FormatResourceWithUnit(cowsData.m_finalConsumption, ExtendedTransferManager.Fruits);
        m_highlandCowsInputTons.text = IndustryWorldInfoPanel.FormatResourceWithUnit(highlandCowsData.m_finalConsumption, ExtendedTransferManager.Vegetables);
        m_sheepInputTons.text = IndustryWorldInfoPanel.FormatResourceWithUnit(sheepData.m_finalConsumption, TransferManager.TransferReason.Grain);
        m_pigsInputTons.text = IndustryWorldInfoPanel.FormatResourceWithUnit(pigsData.m_finalConsumption, ExtendedTransferManager.Cotton);

        // --- Output row ---
        long totalIncome = 0;

        totalIncome += UpdateOutputLabel(m_beefMeatOutput, m_beefMeatIncome, beefMeatData, ExtendedTransferManager.BeefMeat);
        totalIncome += UpdateOutputLabel(m_cowMilkOutput, m_cowMilkIncome, cowMilkData, ExtendedTransferManager.CowMilk);
        totalIncome += UpdateOutputLabel(m_highlandBeefOutput, m_highlandBeefIncome, highlandBeefData, ExtendedTransferManager.HighlandBeefMeat);
        totalIncome += UpdateOutputLabel(m_highlandMilkOutput, m_highlandMilkIncome, highlandMilkData, ExtendedTransferManager.HighlandCowMilk);
        totalIncome += UpdateOutputLabel(m_lambMeatOutput, m_lambMeatIncome, lambMeatData, ExtendedTransferManager.LambMeat);
        totalIncome += UpdateOutputLabel(m_sheepMilkOutput, m_sheepMilkIncome, sheepMilkData, ExtendedTransferManager.SheepMilk);
        totalIncome += UpdateOutputLabel(m_woolOutput, m_woolIncome, woolData, ExtendedTransferManager.Wool);
        totalIncome += UpdateOutputLabel(m_porkMeatOutput, m_porkMeatIncome, porkMeatData, ExtendedTransferManager.PorkMeat);

        // --- Acquisition costs ---
        long totalAcquisitionCost =
            GetImportCost(fruitsData, ExtendedTransferManager.Fruits) +
            GetImportCost(vegetablesData, ExtendedTransferManager.Vegetables) +
            GetImportCost(grainData, TransferManager.TransferReason.Grain) +
            GetImportCost(cottonData, ExtendedTransferManager.Cotton);

        // --- Upkeep + profit ---
        UpdateWorkersAndTotalUpkeep(out float totalUpkeepScaled);
        long profit = totalIncome - totalAcquisitionCost + (long)totalUpkeepScaled;
        m_profit.text = profit.ToString(Settings.moneyFormatNoCents, LocaleManager.cultureInfo);

        RefreshPolicies();

        // --- Level progress ---
        bool isMaxLevel = dm.m_parks.m_buffer[park].m_parkLevel == DistrictPark.ParkLevel.Level5;
        bool mainBuildingOff = IsMaintBuildingOff(park);

        if (!mainBuildingOff)
        {
            if (!isMaxLevel)
            {
                ulong produced = dm.m_parks.m_buffer[park].m_totalProductionAmount;
                uint prodReq = (uint)dm.m_properties.m_parkProperties.m_industryLevelInfo[ParkLevel(park)].m_productionLevelupRequirement;
                ushort workers = dm.m_parks.m_buffer[park].m_finalWorkerCount;
                int workReq = dm.m_properties.m_parkProperties.m_industryLevelInfo[ParkLevel(park)].m_workerLevelupRequirement;

                m_productionProgress.text = StringUtils.SafeFormat(
                    Locale.Get("INDUSTRYPANEL_PRODUCTIONREQUIREMENT"),
                    IndustryWorldInfoPanel.FormatResource(produced),
                    IndustryWorldInfoPanel.FormatResource(prodReq));
                m_productionProgress.textColor = (produced < prodReq)
                    ? m_requirementInProgressColor : m_requirementCompletedColor;

                m_workersProgress.text = StringUtils.SafeFormat(
                    Locale.Get("INDUSTRYPANEL_WORKERSREQUIREMENT"), workers, workReq);
                m_workersProgress.textColor = (workers < workReq)
                    ? m_requirementInProgressColor : m_requirementCompletedColor;
            }
            else
            {
                m_productionProgress.text = StringUtils.SafeFormat(
                    Locale.Get("INDUSTRYPANEL_PRODUCTION"),
                    IndustryWorldInfoPanel.FormatResource(dm.m_parks.m_buffer[park].m_totalProductionAmount));
                m_workersProgress.text = StringUtils.SafeFormat(
                    Locale.Get("INDUSTRYPANEL_WORKERS"),
                    dm.m_parks.m_buffer[park].m_finalWorkerCount);
                m_productionProgress.textColor = m_requirementInProgressColor;
                m_workersProgress.textColor = m_requirementInProgressColor;
            }
        }

        bool bonusVisible = dm.m_parks.m_buffer[park].m_parkLevel != DistrictPark.ParkLevel.Level1
                         && dm.m_parks.m_buffer[park].m_parkLevel != DistrictPark.ParkLevel.None
                         && !mainBuildingOff;

        m_mainBuildingOff.isVisible = mainBuildingOff;
        m_workersProgress.isVisible = !mainBuildingOff;
        m_productionProgress.isVisible = !mainBuildingOff;
        m_levelBonusLabel.isVisible = bonusVisible;
        m_efficiency.isVisible = bonusVisible;
        m_pollution.isVisible = bonusVisible;
        m_levelRequirements.isVisible = !mainBuildingOff;

        if (bonusVisible)
        {
            dm.m_parks.m_buffer[park].GetProductionFactors(
                out var processingFactor, out var pollutionFactor);
            m_efficiency.text = StringUtils.SafeFormat(
                Locale.Get("INDUSTRYPANEL_EFFICIENCY"),
                (-(100 - processingFactor)).ToString("+0;-#"));
            m_pollution.text = StringUtils.SafeFormat(
                Locale.Get("INDUSTRYPANEL_POLLUTION"), -(100 - pollutionFactor));
        }

        m_progressBarUpdater.Update(park);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>Updates one acquisition (raw material) row: tons label, import cost, pie chart.</summary>
    private void UpdateAcquisitionLabel(
        UILabel acqLabel, UILabel expensesLabel,
        UIRadialChart pieChart, UIPanel pieSection,
        DistrictAreaResourceData data, TransferManager.TransferReason reason)
    {
        uint ownProduction = data.m_finalProduction;
        uint total = data.m_finalImport + ownProduction;

        acqLabel.text = IndustryWorldInfoPanel.FormatResourceWithUnit(total, reason);

        long importCost = data.m_finalImport * IndustryBuildingAI.GetResourcePrice(reason) / 10000;
        expensesLabel.text = importCost.ToString(Settings.moneyFormatNoCents, LocaleManager.cultureInfo);

        pieSection.isVisible = (total + ownProduction) != 0;
        if (pieSection.isVisible)
        {
            float ownRatio = (total == 0) ? 0f : (float)ownProduction / (float)total;
            pieChart.GetSlice(0).endValue = ownRatio;
            pieChart.GetSlice(1).startValue = ownRatio;
        }
    }

    /// <summary>Updates one output product row and returns income for profit calculation.</summary>
    private long UpdateOutputLabel(
        UILabel outputLabel, UILabel incomeLabel,
        DistrictAreaResourceData data, TransferManager.TransferReason reason)
    {
        uint exported = data.m_finalExport;
        outputLabel.text = IndustryWorldInfoPanel.FormatResourceWithUnit(exported, reason);
        long income = exported * IndustryBuildingAI.GetResourcePrice(reason) / 10000;
        incomeLabel.text = income.ToString(Settings.moneyFormatNoCents, LocaleManager.cultureInfo);
        return income;
    }

    private long GetImportCost(DistrictAreaResourceData data, TransferManager.TransferReason reason) =>
        data.m_finalImport * IndustryBuildingAI.GetResourcePrice(reason) / 10000;

    private bool IsMaintBuildingOff(byte park)
    {
        ushort mainGate = Singleton<DistrictManager>.instance.m_parks.m_buffer[park].m_mainGate;
        var buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
        return buildings[mainGate].m_productionRate == 0
            || (buildings[mainGate].m_flags & Building.Flags.Collapsed) != 0;
    }

    private int ParkLevel(int parkID) =>
        (int)Singleton<DistrictManager>.instance.m_parks.m_buffer[parkID].m_parkLevel;

    private Color32 MultiplyColor(Color col, float scalar)
    {
        Color c = col * scalar;
        c.a = col.a;
        return c;
    }

    // -------------------------------------------------------------------------
    // Workers & upkeep (identical logic to original, no type filtering needed)
    // -------------------------------------------------------------------------
    private void UpdateWorkersAndTotalUpkeep(out float totalUpkeepScaled, bool onSetTarget = false)
    {
        totalUpkeepScaled = 0f;
        if (!Singleton<CitizenManager>.exists) return;

        var cm = Singleton<CitizenManager>.instance;
        var bm = Singleton<BuildingManager>.instance;
        int uneducated = 0, educated = 0, wellEdu = 0, highEdu = 0, total = 0;
        int placesU = 0, placesE = 0, placesW = 0, placesH = 0;
        int upkeep = 0;

        var serviceBuildings = bm.GetServiceBuildings(ItemClass.Service.PlayerIndustry);
        for (int i = 0; i < serviceBuildings.m_size; i++)
        {
            ushort bid = serviceBuildings.m_buffer[i];
            byte bPark = Singleton<DistrictManager>.instance.GetPark(bm.m_buildings.m_buffer[bid].m_position);
            if (bPark != m_InstanceID.Park) continue;

            var industryAI = bm.m_buildings.m_buffer[bid].Info.GetAI() as IndustryBuildingAI;
            var mainAI = bm.m_buildings.m_buffer[bid].Info.GetAI() as MainIndustryBuildingAI;

            var mainGateAI = bm.m_buildings.m_buffer[
                Singleton<DistrictManager>.instance.m_parks.m_buffer[m_InstanceID.Park].m_mainGate
            ].Info.m_buildingAI as MainIndustryBuildingAI;

            bool belongs = (industryAI != null && mainGateAI != null
                           && industryAI.m_industryType == mainGateAI.m_industryType)
                        || mainAI != null;
            if (!belongs) continue;

            var ai = (BuildingAI)(industryAI ?? (BuildingAI)mainAI);
            upkeep += ai.GetResourceRate(bid, ref bm.m_buildings.m_buffer[bid], EconomyManager.Resource.Maintenance);

            var iwb = industryAI ?? (IndustryBuildingAI)(object)mainAI;
            if (industryAI != null)
            {
                placesU += industryAI.m_workPlaceCount0;
                placesE += industryAI.m_workPlaceCount1;
                placesW += industryAI.m_workPlaceCount2;
                placesH += industryAI.m_workPlaceCount3;
            }
            else if (mainAI != null)
            {
                placesU += mainAI.m_workPlaceCount0;
                placesE += mainAI.m_workPlaceCount1;
                placesW += mainAI.m_workPlaceCount2;
                placesH += mainAI.m_workPlaceCount3;
            }

            uint unitID = bm.m_buildings.m_buffer[bid].m_citizenUnits;
            int guard = 0;
            while (unitID != 0)
            {
                uint next = cm.m_units.m_buffer[unitID].m_nextUnit;
                if ((cm.m_units.m_buffer[unitID].m_flags & CitizenUnit.Flags.Work) != 0)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        uint cid = cm.m_units.m_buffer[unitID].GetCitizen(j);
                        if (cid == 0 || cm.m_citizens.m_buffer[cid].Dead
                            || (cm.m_citizens.m_buffer[cid].m_flags & Citizen.Flags.MovingIn) != 0)
                            continue;
                        total++;
                        switch (cm.m_citizens.m_buffer[cid].EducationLevel)
                        {
                            case Citizen.Education.Uneducated: uneducated++; break;
                            case Citizen.Education.OneSchool: educated++; break;
                            case Citizen.Education.TwoSchools: wellEdu++; break;
                            case Citizen.Education.ThreeSchools: highEdu++; break;
                        }
                    }
                }
                unitID = next;
                if (++guard > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list!\n" + System.Environment.StackTrace);
                    break;
                }
            }
        }

        m_upkeepLabel.text = LocaleFormatter.FormatUpkeep(upkeep, isDistanceBased: false);
        totalUpkeepScaled = upkeep * 0.0016f;

        int totalPlaces = placesU + placesE + placesW + placesH;

        // Overeducated workers
        int overEdu = 0, slack = placesU - uneducated;
        if (educated > placesE) overEdu += Mathf.Max(0, Mathf.Min(slack, educated - placesE)); slack += placesE - educated;
        if (wellEdu > placesW) overEdu += Mathf.Max(0, Mathf.Min(slack, wellEdu - placesW)); slack += placesW - wellEdu;
        if (highEdu > placesH) overEdu += Mathf.Max(0, Mathf.Min(slack, highEdu - placesH));

        m_OverWorkSituation.text = StringUtils.SafeFormat(
            Locale.Get(overEdu != 1 ? "ZONEDBUILDING_OVEREDUCATEDWORKERS" : "ZONEDBUILDING_OVEREDUCATEDWORKER"), overEdu);
        m_OverWorkSituation.isVisible = overEdu > 0;

        m_UneducatedPlaces.text = placesU.ToString();
        m_EducatedPlaces.text = placesE.ToString();
        m_WellEducatedPlaces.text = placesW.ToString();
        m_HighlyEducatedPlaces.text = placesH.ToString();
        m_UneducatedWorkers.text = uneducated.ToString();
        m_EducatedWorkers.text = educated.ToString();
        m_WellEducatedWorkers.text = wellEdu.ToString();
        m_HighlyEducatedWorkers.text = highEdu.ToString();
        m_JobsAvailLegend.text = (totalPlaces - total).ToString();
        m_workersInfoLabel.text = StringUtils.SafeFormat(
            Locale.Get("ZONEDBUILDING_WORKERS"), total, totalPlaces);

        int v0 = GetChartValue(placesU, totalPlaces), v1 = GetChartValue(placesE, totalPlaces),
            v2 = GetChartValue(placesW, totalPlaces), v3 = GetChartValue(placesH, totalPlaces);
        int sum = v0 + v1 + v2 + v3;
        if (sum != 0 && sum != 100) v0 = 100 - (v1 + v2 + v3);
        m_WorkPlacesEducationChart.SetValues(v0, v1, v2, v3);

        int w0 = GetChartValue(uneducated, totalPlaces), w1 = GetChartValue(educated, totalPlaces),
            w2 = GetChartValue(wellEdu, totalPlaces), w3 = GetChartValue(highEdu, totalPlaces);
        m_WorkersEducationChart.SetValues(w0, w1, w2, w3, 100 - (w0 + w1 + w2 + w3));
    }

    private static int GetChartValue(int value, int total) =>
        Mathf.Clamp(Mathf.FloorToInt((float)value / Mathf.Max(1, total) * 100f), 0, 100);

    // -------------------------------------------------------------------------
    // Policies (identical to original)
    // -------------------------------------------------------------------------
    private void InitPolicies()
    {
        var policies = Utils.GetOrderedEnumData<DistrictPolicies.Policies>("IndustryArea");
        m_PoliciesList = new UITemplateList<UIPanel>(Find<UIPanel>("PoliciesPanel"), "IndustryPolicyTemplate");
        m_PoliciesList.SetItemCount(policies.Length);
        for (int i = 0; i < m_PoliciesList.items.Count; i++)
        {
            string name = policies[i].enumName;
            m_PoliciesList.items[i].Find<UILabel>("PolicyName").text = Locale.Get("POLICIES", name);
            var btn = m_PoliciesList.items[i].Find<UIButton>("PolicySpriteButtonForeground");
            btn.normalFgSprite = "IconPolicy" + name;
            btn.hoveredFgSprite = "IconPolicy" + name + "Hovered";
            btn.pressedFgSprite = "IconPolicy" + name + "Pressed";
            btn.objectUserData = i.ToString();
            btn.eventClick += OnPolicyClicked;
            var btn2 = m_PoliciesList.items[i].Find<UIButton>("PolicyButton");
            btn2.objectUserData = i.ToString();
            btn2.eventClick += OnPolicyClicked;
        }
    }

    private void RefreshPolicies()
    {
        var policies = Utils.GetOrderedEnumData<DistrictPolicies.Policies>("IndustryArea");
        for (int i = 0; i < m_PoliciesList.items.Count; i++)
        {
            var policy = policies[i].enumValue;
            bool active = Singleton<DistrictManager>.instance.IsParkPolicySet(policy, m_InstanceID.Park);
            ColorButton(m_PoliciesList.items[i].Find<UIButton>("PolicyButton"), active ? m_policyActiveColor : m_policyInactiveColor);
            ColorButton(m_PoliciesList.items[i].Find<UIButton>("PolicySpriteButton"), active ? m_policyActiveColor : m_policyInactiveColor);
        }
    }

    private void RefreshPolicyToolTips()
    {
        var policies = Utils.GetOrderedEnumData<DistrictPolicies.Policies>("IndustryArea");
        for (int i = 0; i < m_PoliciesList.items.Count; i++)
        {
            string tip = TooltipHelper.Format(
                "title", Locale.Get("POLICIES", policies[i].enumName),
                "text", Locale.Get("POLICIES_DETAIL", policies[i].enumName));
            m_PoliciesList.items[i].Find<UIButton>("PolicyButton").tooltip = tip;
            m_PoliciesList.items[i].Find<UIButton>("PolicyButton").tooltipBox = policiesTooltip;
            m_PoliciesList.items[i].Find<UIButton>("PolicySpriteButtonForeground").tooltip = tip;
            m_PoliciesList.items[i].Find<UIButton>("PolicySpriteButtonForeground").tooltipBox = policiesTooltip;
        }
    }

    private void OnPolicyClicked(UIComponent comp, UIMouseEventParameter e)
    {
        var policies = Utils.GetOrderedEnumData<DistrictPolicies.Policies>("IndustryArea");
        int.TryParse(comp.objectUserData as string, out int idx);
        TogglePolicy(policies[idx].enumValue);
    }

    private void TogglePolicy(DistrictPolicies.Policies policy)
    {
        Singleton<SimulationManager>.instance.AddAction(delegate
        {
            var dm = Singleton<DistrictManager>.instance;
            if (dm.IsParkPolicySet(policy, m_InstanceID.Park))
                dm.UnsetParkPolicy(policy, m_InstanceID.Park);
            else
                dm.SetParkPolicy(policy, m_InstanceID.Park);
        });
    }

    private void ColorButton(UIButton b, Color c)
    {
        b.color = b.focusedColor = b.hoveredColor = b.pressedColor = c;
    }

    // -------------------------------------------------------------------------
    // Rename
    // -------------------------------------------------------------------------
    private void OnRename(UIComponent comp, string text) => StartCoroutine(SetName(text));

    private System.Collections.IEnumerator SetName(string newName)
    {
        if (Singleton<SimulationManager>.exists && m_InstanceID.Type == InstanceType.Park && m_InstanceID.Park != 0)
        {
            var task = Singleton<SimulationManager>.instance.AddAction(
                Singleton<DistrictManager>.instance.SetParkName(m_InstanceID.Park, newName));
            yield return task.WaitTaskCompleted(this);
        }
        m_ParkName.text = GetName();
    }

    private string GetName()
    {
        if (m_InstanceID.Type == InstanceType.Park && m_InstanceID.Park != 0)
            return Singleton<DistrictManager>.instance.GetParkName(m_InstanceID.Park);
        return string.Empty;
    }

    // -------------------------------------------------------------------------
    // Locale events
    // -------------------------------------------------------------------------
    private void OnEnable() { if (SingletonLite<LocaleManager>.exists) LocaleManager.eventLocaleChanged += OnLocaleChanged; }
    private void OnDisable() { if (SingletonLite<LocaleManager>.exists) LocaleManager.eventLocaleChanged -= OnLocaleChanged; }
    private void OnLocaleChanged() { InitPolicies(); RefreshPolicyToolTips(); }

    public void OpenIndustryOverviewPanel() =>
        UIView.library.Show<IndustryOverviewPanel>("IndustryOverviewPanel");

    // ─────────────────────────────────────────────────────────────────────────────
    // Layout Constants
    // ─────────────────────────────────────────────────────────────────────────────
    private const float PANEL_W = 875f;
    private const float PANEL_H = 720f;

    // Section left-edge X positions (4 animal columns)
    // Cows=2 outputs, HighlandCows=2 outputs, Sheep=3 outputs, Pigs=1 output
    private static readonly float[] SEC_X = { 8f, 228f, 448f, 728f };
    private static readonly float[] SEC_W = { 212f, 212f, 272f, 139f };

    // Row Y positions (relative to the info-tab content panel)
    private const float Y_ACQ_HDR = 8f;
    private const float Y_RAW = 30f;    // Fruits / Veg / Grain / Cotton nodes
    private const float Y_RAW_ARROW = 85f;    // arrow + "xx tons" label
    private const float Y_ANIMAL = 118f;   // Cows / HighlandCows / Sheep / Pigs nodes
    private const float Y_PROD_ARROW = 178f;
    private const float Y_PRODUCT = 208f;   // BeefMeat / CowMilk / … nodes
    private const float Y_OUT_ARROW = 258f;
    private const float Y_OUTPUT = 284f;   // Output boxes (xx tons + €)
    private const float Y_DIVIDER = 375f;
    private const float Y_LEVEL = 390f;
    private const float Y_WORKERS = 440f;
    private const float Y_UPKEEP = 490f;
    private const float Y_PROFIT = 520f;
    private const float INFO_TAB_H = 575f;

    // Node sizes
    private static readonly Vector2 SZ_RAW = new Vector2(110f, 52f);
    private static readonly Vector2 SZ_ANIMAL = new Vector2(130f, 58f);
    private static readonly Vector2 SZ_PRODUCT = new Vector2(100f, 48f);
    private static readonly Vector2 SZ_SHEEP_P = new Vector2(84f, 48f);  // 3-wide
    private static readonly Vector2 SZ_OUTPUT = new Vector2(100f, 72f);
    private static readonly Vector2 SZ_SHEEP_O = new Vector2(84f, 72f);

    // Node colors
    private static readonly Color32 C_RAW = new Color32(55, 78, 28, 235);
    private static readonly Color32 C_ANIMAL = new Color32(90, 55, 18, 235);
    private static readonly Color32 C_PRODUCT = new Color32(48, 62, 95, 235);
    private static readonly Color32 C_OUTPUT = new Color32(35, 48, 70, 220);
    private static readonly Color32 C_MONEY = new Color32(108, 220, 95, 255);
    private static readonly Color32 C_TONS = new Color32(255, 255, 255, 210);
    private static readonly Color32 C_HEADER = new Color32(200, 200, 200, 255);

    // ─────────────────────────────────────────────────────────────────────────────
    // Start — build UI first, then skip all Find<> calls
    // ─────────────────────────────────────────────────────────────────────────────
    protected override void Start()
    {
        base.Start();
        BuildUI();                   // creates every child component + assigns all fields
        InitEducationChartColors();
        InitPolicies();
        RefreshPolicyToolTips();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // BuildUI — root builder, assembles all sections
    // ─────────────────────────────────────────────────────────────────────────────
    private void BuildUI()
    {
        var root = component as UIPanel;

        root.size = new Vector2(PANEL_W, PANEL_H);
        root.backgroundSprite = "MenuPanel2";
        root.color = new Color32(58, 88, 104, 255);

        BuildTitleBar(root);
        BuildTabStrip(root);
        BuildNoSpecializationPanel(root);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Title bar
    // ─────────────────────────────────────────────────────────────────────────────
    private void BuildTitleBar(UIPanel root)
    {
        // Close button and area name text field are expected by WorldInfoPanel base
        m_ParkName = AddComponent<UITextField>(root, "AreaName");
        m_ParkName.size = new Vector2(PANEL_W - 90f, 28f);
        m_ParkName.relativePosition = new Vector3(10f, 8f);
        m_ParkName.font = UIView.GetAView().defaultFont;
        m_ParkName.textScale = 1.1f;
        m_ParkName.padding = new RectOffset(4, 4, 4, 4);
        m_ParkName.builtinKeyNavigation = true;
        m_ParkName.eventTextSubmitted += OnRename;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Tab strip + containers
    // ─────────────────────────────────────────────────────────────────────────────
    private void BuildTabStrip(UIPanel root)
    {
        float tabY = 42f;

        m_tabStrip = AddComponent<UITabstrip>(root, "Tabstrip");
        m_tabStrip.size = new Vector2(PANEL_W - 20f, 30f);
        m_tabStrip.relativePosition = new Vector3(10f, tabY);

        m_tabContainer = AddComponent<UITabContainer>(root, "TabContainer");
        m_tabContainer.size = new Vector2(PANEL_W - 20f, INFO_TAB_H);
        m_tabContainer.relativePosition = new Vector3(10f, tabY + 32f);

        AddTab("Info", BuildInfoTab);
        AddTab("Policies", BuildPoliciesTab);
    }

    private void AddTab(string label, System.Action<UIPanel> builder)
    {
        UIButton btn = m_tabStrip.AddTab(label);
        btn.textScale = 0.9f;
        btn.size = new Vector2(100f, 28f);

        UIPanel page = m_tabContainer.components[m_tabContainer.components.Count - 1] as UIPanel;
        page.backgroundSprite = "GenericPanel";
        page.color = new Color32(40, 55, 70, 200);
        builder?.Invoke(page);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // INFO TAB — main farming flow diagram
    // ─────────────────────────────────────────────────────────────────────────────
    private void BuildInfoTab(UIPanel tab)
    {
        // Acquisition header
        var acqHeader = AddLabel(tab, "AcquisitionHeader", "Acquisition", new Vector3(8f, Y_ACQ_HDR));
        acqHeader.textScale = 0.85f;
        acqHeader.textColor = C_HEADER;

        // ── Row 1: Raw material nodes ───────────────────────────────────────────
        BuildRawNode(tab, "Fruits", SEC_X[0], SEC_W[0], "resourceIconFood",
            ref m_fruitsAcquisition, ref m_fruitsExpenses, ref m_fruitsPieChart, ref m_fruitsPieChartSection);

        BuildRawNode(tab, "Vegetables", SEC_X[1], SEC_W[1], "resourceIconFood",
            ref m_vegetablesAcquisition, ref m_vegetablesExpenses, ref m_vegetablesPieChart, ref m_vegetablesPieChartSection);

        BuildRawNode(tab, "Grain", SEC_X[2], SEC_W[2], "resourceIconCrops",
            ref m_grainAcquisition, ref m_grainExpenses, ref m_grainPieChart, ref m_grainPieChartSection);

        BuildRawNode(tab, "Cotton", SEC_X[3], SEC_W[3], "resourceIconFood",
            ref m_cottonAcquisition, ref m_cottonExpenses, ref m_cottonPieChart, ref m_cottonPieChartSection);

        // ── Arrows raw → animal ─────────────────────────────────────────────────
        m_cowsInputTons = BuildArrowWithLabel(tab, "CowsInputTons", CenterX(SEC_X[0], SEC_W[0]), Y_RAW_ARROW);
        m_highlandCowsInputTons = BuildArrowWithLabel(tab, "HighlandCowsInputTons", CenterX(SEC_X[1], SEC_W[1]), Y_RAW_ARROW);
        m_sheepInputTons = BuildArrowWithLabel(tab, "SheepInputTons", CenterX(SEC_X[2], SEC_W[2]), Y_RAW_ARROW);
        m_pigsInputTons = BuildArrowWithLabel(tab, "PigsInputTons", CenterX(SEC_X[3], SEC_W[3]), Y_RAW_ARROW);

        // ── Row 2: Animal nodes ─────────────────────────────────────────────────
        BuildAnimalNode(tab, "Cows", SEC_X[0], SEC_W[0], "resourceIconAnimalProducts");
        BuildAnimalNode(tab, "HighlandCows", SEC_X[1], SEC_W[1], "resourceIconAnimalProducts");
        BuildAnimalNode(tab, "Sheep", SEC_X[2], SEC_W[2], "resourceIconAnimalProducts");
        BuildAnimalNode(tab, "Pigs", SEC_X[3], SEC_W[3], "resourceIconAnimalProducts");

        // ── Arrows animal → products  ───────────────────────────────────────────
        // Cows: 2 branches
        float cowL = SEC_X[0] + 8f;
        float cowR = SEC_X[0] + SEC_W[0] - SZ_PRODUCT.x - 8f;
        BuildBranchArrow(tab, CenterX(SEC_X[0], SEC_W[0]), Y_PROD_ARROW, cowL + SZ_PRODUCT.x / 2, cowR + SZ_PRODUCT.x / 2);

        // HighlandCows: 2 branches
        float hcL = SEC_X[1] + 8f;
        float hcR = SEC_X[1] + SEC_W[1] - SZ_PRODUCT.x - 8f;
        BuildBranchArrow(tab, CenterX(SEC_X[1], SEC_W[1]), Y_PROD_ARROW, hcL + SZ_PRODUCT.x / 2, hcR + SZ_PRODUCT.x / 2);

        // Sheep: 3 branches
        float spGap = (SEC_W[2] - 3 * SZ_SHEEP_P.x) / 4f;
        float sp1 = SEC_X[2] + spGap;
        float sp2 = sp1 + SZ_SHEEP_P.x + spGap;
        float sp3 = sp2 + SZ_SHEEP_P.x + spGap;
        BuildBranchArrow3(tab, CenterX(SEC_X[2], SEC_W[2]), Y_PROD_ARROW,
            sp1 + SZ_SHEEP_P.x / 2, sp2 + SZ_SHEEP_P.x / 2, sp3 + SZ_SHEEP_P.x / 2);

        // Pigs: 1 branch (straight arrow)
        BuildArrowSprite(tab, new Vector3(CenterX(SEC_X[3], SEC_W[3]) - 5f, Y_PROD_ARROW), new Vector2(10f, 30f));

        // ── Row 3: Product nodes ────────────────────────────────────────────────
        // Cows
        BuildProductNode(tab, "BeefMeat", new Vector3(cowL, Y_PRODUCT), SZ_PRODUCT, "resourceIconAnimalProducts");
        BuildProductNode(tab, "CowMilk", new Vector3(cowR, Y_PRODUCT), SZ_PRODUCT, "resourceIconAnimalProducts");

        // Highland Cows
        BuildProductNode(tab, "HighlandBeef", new Vector3(hcL, Y_PRODUCT), SZ_PRODUCT, "resourceIconAnimalProducts");
        BuildProductNode(tab, "HighlandMilk", new Vector3(hcR, Y_PRODUCT), SZ_PRODUCT, "resourceIconAnimalProducts");

        // Sheep (3)
        BuildProductNode(tab, "LambMeat", new Vector3(sp1, Y_PRODUCT), SZ_SHEEP_P, "resourceIconAnimalProducts");
        BuildProductNode(tab, "SheepMilk", new Vector3(sp2, Y_PRODUCT), SZ_SHEEP_P, "resourceIconAnimalProducts");
        BuildProductNode(tab, "Wool", new Vector3(sp3, Y_PRODUCT), SZ_SHEEP_P, "resourceIconAnimalProducts");

        // Pigs (1)
        float pgX = CenterX(SEC_X[3], SEC_W[3]) - SZ_PRODUCT.x / 2;
        BuildProductNode(tab, "PorkMeat", new Vector3(pgX, Y_PRODUCT), SZ_PRODUCT, "resourceIconAnimalProducts");

        // ── Arrows product → output boxes ──────────────────────────────────────
        foreach (float cx in new[] {
        cowL + SZ_PRODUCT.x / 2, cowR + SZ_PRODUCT.x / 2,
        hcL  + SZ_PRODUCT.x / 2, hcR  + SZ_PRODUCT.x / 2,
        sp1  + SZ_SHEEP_P.x / 2, sp2  + SZ_SHEEP_P.x / 2, sp3 + SZ_SHEEP_P.x / 2,
        pgX  + SZ_PRODUCT.x / 2 })
            BuildArrowSprite(tab, new Vector3(cx - 5f, Y_OUT_ARROW), new Vector2(10f, 22f));

        // ── Row 4: Output boxes ─────────────────────────────────────────────────
        BuildOutputBox(tab, "BeefMeat", new Vector3(cowL, Y_OUTPUT), SZ_OUTPUT, ref m_beefMeatOutput, ref m_beefMeatIncome);
        BuildOutputBox(tab, "CowMilk", new Vector3(cowR, Y_OUTPUT), SZ_OUTPUT, ref m_cowMilkOutput, ref m_cowMilkIncome);
        BuildOutputBox(tab, "HighlandBeef", new Vector3(hcL, Y_OUTPUT), SZ_OUTPUT, ref m_highlandBeefOutput, ref m_highlandBeefIncome);
        BuildOutputBox(tab, "HighlandMilk", new Vector3(hcR, Y_OUTPUT), SZ_OUTPUT, ref m_highlandMilkOutput, ref m_highlandMilkIncome);
        BuildOutputBox(tab, "LambMeat", new Vector3(sp1, Y_OUTPUT), SZ_SHEEP_O, ref m_lambMeatOutput, ref m_lambMeatIncome);
        BuildOutputBox(tab, "SheepMilk", new Vector3(sp2, Y_OUTPUT), SZ_SHEEP_O, ref m_sheepMilkOutput, ref m_sheepMilkIncome);
        BuildOutputBox(tab, "Wool", new Vector3(sp3, Y_OUTPUT), SZ_SHEEP_O, ref m_woolOutput, ref m_woolIncome);
        BuildOutputBox(tab, "PorkMeat", new Vector3(pgX, Y_OUTPUT), SZ_OUTPUT, ref m_porkMeatOutput, ref m_porkMeatIncome);

        // ── Divider ─────────────────────────────────────────────────────────────
        var divider = AddComponent<UIPanel>(tab, "Divider");
        divider.size = new Vector2(PANEL_W - 30f, 1f);
        divider.relativePosition = new Vector3(0f, Y_DIVIDER);
        divider.backgroundSprite = "WhiteRect";
        divider.color = new Color32(100, 120, 140, 120);

        // ── Level progress ───────────────────────────────────────────────────────
        BuildLevelSection(tab);

        // ── Workers / upkeep / profit ────────────────────────────────────────────
        BuildStatsSection(tab);

        // ── Workers tooltip (hidden by default) ─────────────────────────────────
        BuildWorkersTooltip(tab);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Raw material node  (Fruits / Vegetables / Grain / Cotton)
    // ─────────────────────────────────────────────────────────────────────────────
    private void BuildRawNode(UIPanel parent, string name,
        float secX, float secW, string iconSprite,
        ref UILabel acqLabel, ref UILabel expLabel,
        ref UIRadialChart pieChart, ref UIPanel pieSection)
    {
        float nodeX = secX + (secW - SZ_RAW.x) / 2f;

        // Colored background box
        var box = AddColoredPanel(parent, name + "Node", C_RAW,
            new Vector3(nodeX, Y_RAW), SZ_RAW);

        // Icon
        var icon = AddComponent<UISprite>(box, "Icon");
        icon.spriteName = iconSprite;
        icon.size = new Vector2(24f, 24f);
        icon.relativePosition = new Vector3(4f, (SZ_RAW.y - 24f) / 2f);

        // Name label
        var lbl = AddLabel(box, "Name", name, new Vector3(32f, (SZ_RAW.y - 14f) / 2f));
        lbl.textScale = 0.78f;
        lbl.textColor = Color.white;
        lbl.autoSize = false;
        lbl.size = new Vector2(SZ_RAW.x - 36f, 16f);

        // Pie chart section (own vs import indicator)
        pieSection = AddComponent<UIPanel>(parent, name + "PieChartSection");
        pieSection.size = new Vector2(30f, 30f);
        pieSection.relativePosition = new Vector3(nodeX + SZ_RAW.x - 35f, Y_RAW + SZ_RAW.y - 34f);
        pieSection.isVisible = true;

        pieChart = AddComponent<UIRadialChart>(pieSection, name + "PieChart");
        pieChart.size = new Vector2(28f, 28f);
        pieChart.relativePosition = Vector3.zero;
        pieChart.AddSlice();
        pieChart.AddSlice();
        pieChart.GetSlice(0).innerColor = new Color32(80, 200, 80, 255);
        pieChart.GetSlice(0).outterColor = new Color32(80, 200, 80, 255);
        pieChart.GetSlice(1).innerColor = new Color32(220, 130, 40, 255);
        pieChart.GetSlice(1).outterColor = new Color32(220, 130, 40, 255);

        // Acquisition "xx tons" label (shown on the arrow below)
        acqLabel = AddLabel(parent, name + "AcquisitionPerWeek", "0 tons",
            new Vector3(nodeX, Y_RAW + SZ_RAW.y + 2f));
        acqLabel.textScale = 0.7f;
        acqLabel.textColor = C_TONS;
        acqLabel.autoSize = false;
        acqLabel.size = new Vector2(SZ_RAW.x, 14f);
        acqLabel.textAlignment = UIHorizontalAlignment.Center;

        // Import cost "€0"
        expLabel = AddLabel(parent, name + "AcquisitionExpenses", "€0",
            new Vector3(nodeX, Y_RAW + SZ_RAW.y + 16f));
        expLabel.textScale = 0.65f;
        expLabel.textColor = C_MONEY;
        expLabel.autoSize = false;
        expLabel.size = new Vector2(SZ_RAW.x, 14f);
        expLabel.textAlignment = UIHorizontalAlignment.Center;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Animal node  (Cows / HighlandCows / Sheep / Pigs)
    // ─────────────────────────────────────────────────────────────────────────────
    private void BuildAnimalNode(UIPanel parent, string name,
        float secX, float secW, string iconSprite)
    {
        float nodeX = secX + (secW - SZ_ANIMAL.x) / 2f;
        var box = AddColoredPanel(parent, name + "Node", C_ANIMAL,
            new Vector3(nodeX, Y_ANIMAL), SZ_ANIMAL);

        var icon = AddComponent<UISprite>(box, "Icon");
        icon.spriteName = iconSprite;
        icon.size = new Vector2(28f, 28f);
        icon.relativePosition = new Vector3(4f, (SZ_ANIMAL.y - 28f) / 2f);

        var lbl = AddLabel(box, "Name", name, new Vector3(36f, (SZ_ANIMAL.y - 14f) / 2f));
        lbl.textScale = 0.8f;
        lbl.textColor = Color.white;
        lbl.autoSize = false;
        lbl.size = new Vector2(SZ_ANIMAL.x - 42f, 16f);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Product node  (BeefMeat / CowMilk / …)
    // ─────────────────────────────────────────────────────────────────────────────
    private void BuildProductNode(UIPanel parent, string name,
        Vector3 pos, Vector2 size, string iconSprite)
    {
        var box = AddColoredPanel(parent, name + "Node", C_PRODUCT, pos, size);

        var icon = AddComponent<UISprite>(box, "Icon");
        icon.spriteName = iconSprite;
        icon.size = new Vector2(20f, 20f);
        icon.relativePosition = new Vector3(3f, (size.y - 20f) / 2f);

        string display = System.Text.RegularExpressions.Regex.Replace(name, "([A-Z])", " $1").Trim();
        var lbl = AddLabel(box, "Name", display, new Vector3(26f, (size.y - 26f) / 2f));
        lbl.textScale = 0.62f;
        lbl.textColor = Color.white;
        lbl.wordWrap = true;
        lbl.autoSize = false;
        lbl.size = new Vector2(size.x - 30f, size.y - 4f);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Output box  (Output / xx tons / €x,xxx)
    // ─────────────────────────────────────────────────────────────────────────────
    private void BuildOutputBox(UIPanel parent, string name,
        Vector3 pos, Vector2 size,
        ref UILabel outputLabel, ref UILabel incomeLabel)
    {
        var box = AddColoredPanel(parent, name + "OutputBox", C_OUTPUT, pos, size);

        var title = AddLabel(box, "Title", "Output", new Vector3(0f, 4f));
        title.textScale = 0.65f;
        title.textColor = new Color32(180, 200, 220, 255);
        title.autoSize = false;
        title.size = new Vector2(size.x, 14f);
        title.textAlignment = UIHorizontalAlignment.Center;

        outputLabel = AddLabel(box, name + "OutputPerWeek", "0 tons", new Vector3(0f, 20f));
        outputLabel.textScale = 0.72f;
        outputLabel.textColor = C_TONS;
        outputLabel.autoSize = false;
        outputLabel.size = new Vector2(size.x, 16f);
        outputLabel.textAlignment = UIHorizontalAlignment.Center;

        incomeLabel = AddLabel(box, name + "IncomePerWeek", "€0", new Vector3(0f, 38f));
        incomeLabel.textScale = 0.78f;
        incomeLabel.textColor = C_MONEY;
        incomeLabel.autoSize = false;
        incomeLabel.size = new Vector2(size.x, 18f);
        incomeLabel.textAlignment = UIHorizontalAlignment.Center;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Arrow helpers
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>Straight down arrow with a "xx tons" label centered on it.</summary>
    private UILabel BuildArrowWithLabel(UIPanel parent, string labelName, float cx, float y)
    {
        BuildArrowSprite(parent, new Vector3(cx - 5f, y), new Vector2(10f, 28f));

        var lbl = AddLabel(parent, labelName, "0 tons", new Vector3(cx - 30f, y + 30f));
        lbl.textScale = 0.65f;
        lbl.textColor = C_TONS;
        lbl.autoSize = false;
        lbl.size = new Vector2(60f, 14f);
        lbl.textAlignment = UIHorizontalAlignment.Center;
        return lbl;
    }

    /// <summary>Horizontal splitter + two downward arrows for 2-output animals.</summary>
    private void BuildBranchArrow(UIPanel parent, float stemCX, float y, float leftCX, float rightCX)
    {
        // Vertical stem
        BuildArrowSprite(parent, new Vector3(stemCX - 4f, y), new Vector2(8f, 16f));

        // Horizontal bar
        float barX = Mathf.Min(leftCX, rightCX) - 4f;
        float barW = Mathf.Abs(rightCX - leftCX) + 8f;
        var bar = AddComponent<UIPanel>(parent, "Branch_" + stemCX);
        bar.backgroundSprite = "WhiteRect";
        bar.color = ColorArrow;
        bar.size = new Vector2(barW, 4f);
        bar.relativePosition = new Vector3(barX, y + 16f);

        // Two down arrows
        BuildArrowSprite(parent, new Vector3(leftCX - 4f, y + 18f), new Vector2(8f, 14f));
        BuildArrowSprite(parent, new Vector3(rightCX - 4f, y + 18f), new Vector2(8f, 14f));
    }

    /// <summary>Horizontal splitter + three downward arrows for Sheep.</summary>
    private void BuildBranchArrow3(UIPanel parent, float stemCX, float y,
        float cx1, float cx2, float cx3)
    {
        BuildArrowSprite(parent, new Vector3(stemCX - 4f, y), new Vector2(8f, 16f));

        float barX = cx1 - 4f;
        float barW = (cx3 - cx1) + 8f;
        var bar = AddComponent<UIPanel>(parent, "Branch3_" + stemCX);
        bar.backgroundSprite = "WhiteRect";
        bar.color = ColorArrow;
        bar.size = new Vector2(barW, 4f);
        bar.relativePosition = new Vector3(barX, y + 16f);

        BuildArrowSprite(parent, new Vector3(cx1 - 4f, y + 18f), new Vector2(8f, 14f));
        BuildArrowSprite(parent, new Vector3(cx2 - 4f, y + 18f), new Vector2(8f, 14f));
        BuildArrowSprite(parent, new Vector3(cx3 - 4f, y + 18f), new Vector2(8f, 14f));
    }

    private static readonly Color32 ColorArrow = new Color32(130, 210, 70, 255);

    private void BuildArrowSprite(UIPanel parent, Vector3 pos, Vector2 size)
    {
        var s = AddComponent<UIPanel>(parent, "Arrow_" + pos.x + "_" + pos.y);
        s.backgroundSprite = "WhiteRect";
        s.color = ColorArrow;
        s.size = size;
        s.relativePosition = pos;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Level + stats section
    // ─────────────────────────────────────────────────────────────────────────────
    private void BuildLevelSection(UIPanel tab)
    {
        var levelPanel = AddComponent<UIPanel>(tab, "LevelProgress");
        levelPanel.size = new Vector2(PANEL_W - 30f, 40f);
        levelPanel.relativePosition = new Vector3(0f, Y_LEVEL);
        m_progressBarUpdater = new ProgressBarUpdater(levelPanel);

        for (int i = 0; i < 5; i++)
        {
            var bar = AddComponent<UIProgressBar>(levelPanel, "Level" + (i + 1) + "Bar");
            bar.size = new Vector2((PANEL_W - 50f) / 5f - 4f, 14f);
            bar.relativePosition = new Vector3(i * ((PANEL_W - 50f) / 5f), 0f);
            bar.backgroundSprite = "LevelBarBackground";
            bar.progressSprite = "LevelBarForeground";
            bar.value = 0f;
        }

        var progPanel = AddComponent<UIPanel>(tab, "Progress");
        progPanel.size = new Vector2(PANEL_W - 30f, 44f);
        progPanel.relativePosition = new Vector3(0f, Y_LEVEL + 18f);
        m_levelRequirements = progPanel;

        m_productionProgress = AddLabel(progPanel, "ProductionTilNextLevel", "", new Vector3(0f, 0f));
        m_productionProgress.textScale = 0.72f;
        m_productionProgress.autoSize = true;

        m_workersProgress = AddLabel(progPanel, "WorkersTilNextLevel", "", new Vector3(0f, 18f));
        m_workersProgress.textScale = 0.72f;
        m_workersProgress.autoSize = true;

        m_levelBonusLabel = AddLabel(tab, "LevelBonusLabel", "Area Bonuses:", new Vector3(0f, Y_WORKERS));
        m_levelBonusLabel.textScale = 0.72f;
        m_levelBonusLabel.textColor = C_HEADER;

        m_efficiency = AddLabel(tab, "EfficiencyLabel", "", new Vector3(0f, Y_WORKERS + 16f));
        m_efficiency.textScale = 0.7f;
        m_efficiency.textColor = new Color32(100, 220, 100, 255);

        m_pollution = AddLabel(tab, "PollutionLabel", "", new Vector3(120f, Y_WORKERS + 16f));
        m_pollution.textScale = 0.7f;
        m_pollution.textColor = new Color32(220, 180, 80, 255);

        m_mainBuildingOff = AddComponent<UIPanel>(tab, "MainBuildingOff");
        m_mainBuildingOff.size = new Vector2(PANEL_W - 30f, 20f);
        m_mainBuildingOff.relativePosition = new Vector3(0f, Y_WORKERS + 16f);
        var offLabel = AddLabel(m_mainBuildingOff, "OffLabel", "Main building is off", Vector3.zero);
        offLabel.textColor = new Color32(220, 80, 80, 255);
        offLabel.textScale = 0.75f;
        m_mainBuildingOff.Hide();
    }

    private void BuildStatsSection(UIPanel tab)
    {
        float rightX = PANEL_W * 0.55f;

        // Left side: produced / workers
        m_productionProgress = m_productionProgress ?? AddLabel(tab, "ProductionTilNextLevel", "", new Vector3(0f, Y_UPKEEP));
        m_size = AddLabel(tab, "SizeLabel", "", new Vector3(0f, Y_UPKEEP + 18f));
        m_size.textScale = 0.72f;

        // Right side: upkeep
        m_upkeepLabel = AddLabel(tab, "Upkeep", "", new Vector3(rightX, Y_UPKEEP));
        m_upkeepLabel.textScale = 0.75f;
        m_upkeepLabel.textColor = new Color32(220, 140, 40, 255);

        // Profit section
        var profitHeader = AddLabel(tab, "ProfitHeader", "TOTAL PROFIT", new Vector3(rightX, Y_PROFIT));
        profitHeader.textScale = 0.78f;
        profitHeader.textColor = new Color32(220, 220, 80, 255);

        m_profit = AddLabel(tab, "ProfitLabel", "€0", new Vector3(rightX, Y_PROFIT + 18f));
        m_profit.textScale = 1.05f;
        m_profit.textColor = C_MONEY;

        // Industry overview button
        var overviewBtn = AddComponent<UIButton>(tab, "OverviewBtn");
        overviewBtn.text = "Industry Areas Overview";
        overviewBtn.size = new Vector2(190f, 26f);
        overviewBtn.relativePosition = new Vector3(rightX, Y_PROFIT + 42f);
        overviewBtn.textScale = 0.72f;
        overviewBtn.normalBgSprite = "ButtonMenu";
        overviewBtn.hoveredBgSprite = "ButtonMenuHovered";
        overviewBtn.eventClick += (c, e) => OpenIndustryOverviewPanel();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Workers tooltip
    // ─────────────────────────────────────────────────────────────────────────────
    private void BuildWorkersTooltip(UIPanel tab)
    {
        m_workersTooltip = AddComponent<UIPanel>(tab, "WorkersTooltip");
        m_workersTooltip.backgroundSprite = "InfoviewPanel";
        m_workersTooltip.size = new Vector2(240f, 160f);
        m_workersTooltip.relativePosition = new Vector3(10f, Y_PROFIT - 170f);
        m_workersTooltip.Hide();

        m_workersInfoLabel = AddLabel(m_workersTooltip, "TotalWorkerInfo", "", new Vector3(5f, 5f));
        m_OverWorkSituation = AddLabel(m_workersTooltip, "OverWorkSituation", "", new Vector3(5f, 22f));
        m_UneducatedPlaces = AddLabel(m_workersTooltip, "UneducatedPlaces", "", new Vector3(5f, 42f));
        m_EducatedPlaces = AddLabel(m_workersTooltip, "EducatedPlaces", "", new Vector3(5f, 57f));
        m_WellEducatedPlaces = AddLabel(m_workersTooltip, "WellEducatedPlaces", "", new Vector3(5f, 72f));
        m_HighlyEducatedPlaces = AddLabel(m_workersTooltip, "HighlyEducatedPlaces", "", new Vector3(5f, 87f));
        m_UneducatedWorkers = AddLabel(m_workersTooltip, "UneducatedWorkers", "", new Vector3(130f, 42f));
        m_EducatedWorkers = AddLabel(m_workersTooltip, "EducatedWorkers", "", new Vector3(130f, 57f));
        m_WellEducatedWorkers = AddLabel(m_workersTooltip, "WellEducatedWorkers", "", new Vector3(130f, 72f));
        m_HighlyEducatedWorkers = AddLabel(m_workersTooltip, "HighlyEducatedWorkers", "", new Vector3(130f, 87f));
        m_JobsAvailLegend = AddLabel(m_workersTooltip, "JobsAvailAmount", "", new Vector3(5f, 105f));

        m_WorkPlacesEducationChart = AddComponent<UIRadialChart>(m_workersTooltip, "WorkPlacesEducationChart");
        m_WorkPlacesEducationChart.size = new Vector2(50f, 50f);
        m_WorkPlacesEducationChart.relativePosition = new Vector3(5f, 108f);
        for (int i = 0; i < 4; i++) m_WorkPlacesEducationChart.AddSlice();

        m_WorkersEducationChart = AddComponent<UIRadialChart>(m_workersTooltip, "WorkersEducationChart");
        m_WorkersEducationChart.size = new Vector2(50f, 50f);
        m_WorkersEducationChart.relativePosition = new Vector3(130f, 108f);
        for (int i = 0; i < 5; i++) m_WorkersEducationChart.AddSlice();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Policies tab
    // ─────────────────────────────────────────────────────────────────────────────
    private void BuildPoliciesTab(UIPanel tab)
    {
        var policiesPanel = AddComponent<UIScrollablePanel>(tab, "PoliciesPanel");
        policiesPanel.size = new Vector2(PANEL_W - 30f, INFO_TAB_H - 10f);
        policiesPanel.relativePosition = new Vector3(0f, 0f);
        policiesPanel.autoLayout = true;
        policiesPanel.autoLayoutDirection = LayoutDirection.Vertical;
        policiesPanel.autoLayoutPadding = new RectOffset(0, 0, 2, 2);
        policiesPanel.scrollWheelDirection = UIOrientation.Vertical;

        // m_PoliciesList is populated in InitPolicies() using "IndustryPolicyTemplate"
        // which already exists in the game's atlas — no need to create it here
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // No-specialization fallback
    // ─────────────────────────────────────────────────────────────────────────────
    private void BuildNoSpecializationPanel(UIPanel root)
    {
        m_noSpecialization = AddComponent<UIPanel>(root, "NoSpecialization");
        m_noSpecialization.backgroundSprite = "GenericPanel";
        m_noSpecialization.size = new Vector2(PANEL_W - 20f, 80f);
        m_noSpecialization.relativePosition = new Vector3(10f, 80f);

        var lbl = AddLabel(m_noSpecialization, "NoSpecLabel",
            "This area has no farming specialization.", new Vector3(10f, 28f));
        lbl.textScale = 0.85f;
        lbl.textColor = C_HEADER;
        m_noSpecialization.Hide();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Low-level UIComponent factory helpers
    // ─────────────────────────────────────────────────────────────────────────────
    private static T AddComponent<T>(UIComponent parent, string name) where T : UIComponent
    {
        var c = parent.AddUIComponent<T>();
        c.name = name;
        return c;
    }

    private static UIPanel AddColoredPanel(UIComponent parent, string name,
        Color32 color, Vector3 pos, Vector2 size)
    {
        var p = parent.AddUIComponent<UIPanel>();
        p.name = name;
        p.backgroundSprite = "GenericPanel";
        p.color = color;
        p.size = size;
        p.relativePosition = pos;
        return p;
    }

    private static UILabel AddLabel(UIComponent parent, string name,
        string text, Vector3 pos)
    {
        var l = parent.AddUIComponent<UILabel>();
        l.name = name;
        l.text = text;
        l.relativePosition = pos;
        l.autoSize = true;
        l.textScale = 0.75f;
        l.textColor = Color.white;
        return l;
    }

    // Returns the horizontal center X of a section
    private static float CenterX(float secX, float secW) => secX + secW / 2f;
}