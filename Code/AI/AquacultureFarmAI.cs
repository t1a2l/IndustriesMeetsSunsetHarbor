using ColossalFramework;
using UnityEngine;
using IndustriesMeetsSunsetHarbor.Managers;
using System;

namespace IndustriesMeetsSunsetHarbor.AI
{
    public class AquacultureFarmAI : FishFarmAI
    {
        protected override void ProduceGoods(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(buildingData.m_position);
            DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[district].m_servicePolicies;
            Notification.ProblemStruct problemStruct = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem1.WaterNotConnected | Notification.Problem1.NoResources | Notification.Problem1.NoNaturalResources | Notification.Problem1.FishFarmWaterDirty | Notification.Problem1.NoPlaceForFishingGoods);
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            if (AquacultureFarmManager.AquacultureFarms != null && AquacultureFarmManager.AquacultureFarms[buildingID].Count > 0)
            {
                foreach (var aquacultureExtractorId in AquacultureFarmManager.AquacultureFarms[buildingID])
                {
                    var building = BuildingManager.instance.m_buildings.m_buffer[aquacultureExtractorId];
                    Vector3 position = buildingData.CalculatePosition(building.m_position);
                    Singleton<TerrainManager>.instance.CountWaterCoverage(position, 20f, out var water, out _, out var pollution);
                    num += Mathf.Clamp(pollution, 0, 128);
                    num3 = Mathf.Max(num3, water);
                }
                num2 = num / AquacultureFarmManager.AquacultureFarms[buildingID].Count;
            }
            else
            {
                finalProductionRate = 0;
            }
            if (num2 > 32)
            {
                GuideController properties = Singleton<GuideManager>.instance.m_properties;
                if (properties is object)
                {
                    Singleton<BuildingManager>.instance.m_fishingPollutionDetected.Activate(properties.m_fishingPollutionDetected, buildingID);
                }
            }
            if (num3 == 0)
            {
                finalProductionRate = 0;
            }
            if (finalProductionRate != 0)
            {
                int num4 = finalProductionRate;
                if (num2 > 96)
                {
                    problemStruct = Notification.Problem1.FishFarmWaterDirty | Notification.Problem1.FatalProblem;
                }
                else if (num2 > 64)
                {
                    problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.FishFarmWaterDirty | Notification.Problem1.MajorProblem);
                }
                else if (num2 > 32)
                {
                    problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.FishFarmWaterDirty);
                }
                finalProductionRate = finalProductionRate * Mathf.Clamp(255 - num2 * 2, 0, 255) / 255;
                int num5 = finalProductionRate * m_noiseAccumulation / 100;
                if (num5 != 0)
                {
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, num5, buildingData.m_position, m_noiseRadius);
                }
                HandleDead(buildingID, ref buildingData, ref behaviour, totalWorkerCount);
                int num8 = 0;
                int num9 = 0;
                if (m_outputResource != TransferManager.TransferReason.None)
                {
                    int num10 = (m_productionRate * finalProductionRate + 99) / 100;
                    if (m_info.m_class.m_level == ItemClass.Level.Level3 && (servicePolicies & DistrictPolicies.Services.AlgaeBasedWaterFiltering) != 0)
                    {
                        instance.m_districts.m_buffer[district].m_servicePoliciesEffect |= DistrictPolicies.Services.AlgaeBasedWaterFiltering;
                        num10 = (num10 * 50 + 49) / 100;
                    }
                    int num6 = GetCycleBufferSize(buildingID, ref buildingData);
                    int num7 = buildingData.m_customBuffer1;
                    num8 = GetStorageBufferSize(buildingID, ref buildingData);
                    num9 = buildingData.m_customBuffer2 * 100;
                    if (num10 >= num6 - num7)
                    {
                        if (num6 > num8 - num9)
                        {
                            num10 = num6 - num7;
                            num7 = num6;
                            problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.NoPlaceForFishingGoods);
                        }
                        else
                        {
                            num7 = num7 + num10 - num6;
                            num9 += num6;
                        }
                    }
                    else
                    {
                        num7 += num10;
                    }
                    Singleton<StatisticsManager>.instance.Acquire<StatisticInt64>(StatisticType.FishFarmed).Add(num10);
                    buildingData.m_customBuffer1 = (ushort)num7;
                    buildingData.m_customBuffer2 = (ushort)(num9 / 100);
                }
                if (m_outputResource != TransferManager.TransferReason.None)
                {
                    int num11 = (num4 * m_outputVehicleCount + 99) / 100;
                    int count = 0;
                    int cargo = 0;
                    int capacity = 0;
                    int outside = 0;
                    CalculateOwnVehicles(buildingID, ref buildingData, m_outputResource, ref count, ref cargo, ref capacity, ref outside);
                    buildingData.m_tempExport = (byte)Mathf.Clamp(outside, buildingData.m_tempExport, 255);
                    if (buildingData.m_finalExport != 0)
                    {
                        instance.m_districts.m_buffer[district].m_playerConsumption.m_finalExportAmount += buildingData.m_finalExport;
                    }
                    int num12 = num9;
                    if (num12 >= 8000 && count < num11)
                    {
                        TransferManager.TransferOffer offer = default;
                        offer.Priority = Mathf.Max(1, num12 * 8 / num8);
                        offer.Building = buildingID;
                        offer.Position = buildingData.m_position;
                        offer.Amount = 1;
                        offer.Active = true;
                        Singleton<TransferManager>.instance.AddOutgoingOffer(m_outputResource, offer);
                    }
                }
            }
            buildingData.m_problems = problemStruct;
            buildingData.m_education3 = (byte)Mathf.Clamp(finalProductionRate * m_productionRate / Mathf.Max(1, m_productionRate), 0, 255);
            base.ProduceGoods(buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
        }

        public override string GetLocalizedStats(ushort buildingID, ref Building data)
	{
	    int cycleBufferSize = GetCycleBufferSize(buildingID, ref data);
	    int customBuffer = data.m_customBuffer1;
	    float num = Mathf.Clamp(5 * customBuffer / cycleBufferSize + 1, 1, 5);
	    string text = LocaleFormatter.FormatGeneric("INFO_FISH_FARM_STATS", num, 5) + Environment.NewLine;
            int extractors_count = AquacultureFarmManager.AquacultureFarms[buildingID].Count;
	    if (m_outputResource != TransferManager.TransferReason.None && m_outputVehicleCount != 0)
	    {
		int budget = GetBudget(buildingID, ref data);
		int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
		int num2 = (productionRate * m_outputVehicleCount + 99) / 100;
		int count = 0;
		int cargo = 0;
		int capacity = 0;
		int outside = 0;
		CalculateOwnVehicles(buildingID, ref data, m_outputResource, ref count, ref cargo, ref capacity, ref outside);
		text += LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLES", count, num2);
                text += string.Format("Number of Extractors used: {0}", extractors_count);
	    }
	    return text;
	}
    }
}
