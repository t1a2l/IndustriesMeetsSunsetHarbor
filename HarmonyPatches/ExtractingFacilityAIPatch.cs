using System;
using ColossalFramework;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.Managers;
using IndustriesMeetsSunsetHarbor.Utils;
using MoreTransferReasons;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(ExtractingFacilityAI))]
    public static class ExtractingFacilityAIPatch
    {
        [HarmonyPatch(typeof(ExtractingFacilityAI), "ProduceGoods")]
        [HarmonyPrefix]
        public static bool ProduceGoods(ExtractingFacilityAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(buildingData.m_position);
            byte b = instance.GetPark(buildingData.m_position);
            if (b != 0)
            {
                if (!instance.m_parks.m_buffer[b].IsIndustry)
                {
                    b = 0;
                }
                else if (__instance.m_industryType == DistrictPark.ParkType.Industry || __instance.m_industryType != instance.m_parks.m_buffer[b].m_parkType)
                {
                    b = 0;
                }
            }
            float num = (float)buildingData.Width * -4f;
            float num2 = (float)buildingData.Width * 4f;
            float num3 = (float)buildingData.Length * -4f;
            float num4 = (float)buildingData.Length * 4f;
            if (__instance.m_info.m_subBuildings != null)
            {
                for (int i = 0; i < __instance.m_info.m_subBuildings.Length; i++)
                {
                    if (__instance.m_info.m_subBuildings[i].m_buildingInfo is not null)
                    {
                        float num5 = __instance.m_info.m_subBuildings[i].m_buildingInfo.m_cellWidth;
                        float num6 = __instance.m_info.m_subBuildings[i].m_buildingInfo.m_cellLength;
                        float x = __instance.m_info.m_subBuildings[i].m_position.x;
                        float num7 = 0f - __instance.m_info.m_subBuildings[i].m_position.z;
                        num = Mathf.Min(num, x - num5 * 4f);
                        num2 = Mathf.Max(num2, x + num5 * 4f);
                        num3 = Mathf.Min(num3, num7 - num6 * 4f);
                        num4 = Mathf.Max(num4, num7 + num6 * 4f);
                    }
                }
            }
            float angle = buildingData.m_angle;
            float num8 = (0f - (num + num2)) * 0.5f;
            float num9 = (0f - (num3 + num4)) * 0.5f;
            float num10 = Mathf.Sin(angle);
            float num11 = Mathf.Cos(angle);
            Vector3 position = buildingData.m_position - new Vector3(num11 * num8 + num10 * num9, 0f, num10 * num8 - num11 * num9);
            Notification.ProblemStruct problemStruct = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem1.NoResources | Notification.Problem1.NoPlaceforGoods | Notification.Problem1.NoNaturalResources);
            DistrictPolicies.Park parkPolicies = instance.m_parks.m_buffer[b].m_parkPolicies;
            instance.m_parks.m_buffer[b].m_parkPoliciesEffect |= parkPolicies & (DistrictPolicies.Park.ImprovedLogistics | DistrictPolicies.Park.WorkSafety | DistrictPolicies.Park.AdvancedAutomation);
            if ((parkPolicies & DistrictPolicies.Park.ImprovedLogistics) != DistrictPolicies.Park.None)
            {
                int num12 = __instance.GetMaintenanceCost() / 100;
                num12 = finalProductionRate * num12 / 1000;
                if (num12 != 0)
                {
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Maintenance, num12, __instance.m_info.m_class);
                }
            }
            int num13 = __instance.m_outputRate;
            if ((parkPolicies & DistrictPolicies.Park.AdvancedAutomation) != DistrictPolicies.Park.None)
            {
                num13 = (num13 * 110 + 50) / 100;
                int num14 = __instance.GetMaintenanceCost() / 100;
                num14 = finalProductionRate * num14 / 1000;
                if (num14 != 0)
                {
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Maintenance, num14, __instance.m_info.m_class);
                }
            }
            if ((parkPolicies & DistrictPolicies.Park.WorkSafety) != DistrictPolicies.Park.None)
            {
                int num15 = (aliveWorkerCount + (int)((Singleton<SimulationManager>.instance.m_currentFrameIndex >> 8) & 0xF)) / 16;
                if (num15 != 0)
                {
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, num15, __instance.m_info.m_class);
                }
            }
            if (finalProductionRate != 0)
            {
                Vector3 position2 = buildingData.m_position;
                if (__instance.m_dummyInfo is not null)
                {
                    int num16 = 0;
                    ushort subBuilding = buildingData.m_subBuilding;
                    while (subBuilding != 0)
                    {
                        BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[subBuilding].Info;
                        if ((object)info == __instance.m_dummyInfo)
                        {
                            position2 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[subBuilding].m_position;
                            break;
                        }
                        subBuilding = Singleton<BuildingManager>.instance.m_buildings.m_buffer[subBuilding].m_subBuilding;
                        if (++num16 > 49152)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
                int num17 = finalProductionRate;
                int num18 = __instance.m_pollutionAccumulation;
                NaturalResourceManager.Resource naturalResourceType = __instance.NaturalResourceType;
                if (b != 0)
                {
                    instance.m_parks.m_buffer[b].GetProductionFactors(out var processingFactor, out var pollutionFactor);
                    finalProductionRate = (finalProductionRate * processingFactor + 50) / 100;
                    num18 = (num18 * pollutionFactor + 50) / 100;
                }
                else if (__instance.m_industryType != DistrictPark.ParkType.Industry)
                {
                    finalProductionRate = 0;
                }
                int num19 = 0;
                int num20 = 0;
                bool flag = false;
                if (naturalResourceType != NaturalResourceManager.Resource.None)
                {
                    int num21 = (__instance.m_extractRate * finalProductionRate + 99) / 100;
                    num19 = ReversePatches.GetResourceBufferSize(__instance, parkPolicies, instance.m_parks.m_buffer[b].m_finalStorageDelta);
                    num20 = buildingData.m_customBuffer2;
                    int num22 = Singleton<NaturalResourceManager>.instance.CountResource(naturalResourceType, position2, __instance.m_extractRadius);
                    if (num22 == 0)
                    {
                        flag = true;
                    }
                    else
                    {
                        num22 = Mathf.Clamp((num22 + 9) / 10, (__instance.m_extractRate + 99) / 100, num21);
                        num22 = Mathf.Max(0, Mathf.Min(num19 - num20, num22));
                    }
                    int num23 = (num22 + Singleton<SimulationManager>.instance.m_randomizer.Int32(100u)) / 100;
                    if (num23 != 0)
                    {
                        Singleton<NaturalResourceManager>.instance.TryFetchResource(naturalResourceType, num23, num23, position2, __instance.m_extractRadius);
                    }
                    num20 += num22;
                    if (num20 < num21)
                    {
                        finalProductionRate = (num20 * 100 + __instance.m_extractRate - 1) / __instance.m_extractRate;
                        if (finalProductionRate < 10)
                        {
                            problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.NoNaturalResources);
                        }
                    }
                }
                int num24 = 0;
                int num25 = 0;
                if (__instance.m_outputResource != TransferManager.TransferReason.None)
                {
                    num24 = ReversePatches.GetOutputBufferSize(__instance, parkPolicies, instance.m_parks.m_buffer[b].m_finalStorageDelta);
                    num25 = buildingData.m_customBuffer1;
                    int num26 = (num13 * finalProductionRate + 99) / 100;
                    if (num24 - num25 < num26)
                    {
                        num26 = Mathf.Max(0, num24 - num25);
                        finalProductionRate = (num26 * 100 + num13 - 1) / num13;
                        problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.NoPlaceforGoods);
                    }
                }
                if (naturalResourceType != NaturalResourceManager.Resource.None)
                {
                    int num27 = (__instance.m_extractRate * finalProductionRate + 99) / 100;
                    num20 = Mathf.Max(0, num20 - num27);
                    buildingData.m_customBuffer2 = (ushort)num20;
                }
                if (__instance.m_outputResource != TransferManager.TransferReason.None)
                {
                    int num28 = (num13 * finalProductionRate + 99) / 100;
                    num25 = Mathf.Min(num24, num25 + num28);
                    buildingData.m_customBuffer1 = (ushort)num25;
                    if (__instance.m_outputResource >= ExtendedTransferManager.MealsDeliveryLow)
                    {
                        DistrictParkManager.AddProductionAmount(b, __instance.m_outputResource, num28);
                    }
                    else
                    {
                        instance.m_parks.m_buffer[b].AddProductionAmount(__instance.m_outputResource, num28);
                    }
                }
                num18 = (finalProductionRate * num18 + 50) / 100;
                if (num18 != 0)
                {
                    num18 = UniqueFacultyAI.DecreaseByBonus(UniqueFacultyAI.FacultyBonus.Science, num18);
                    Singleton<NaturalResourceManager>.instance.TryDumpResource(NaturalResourceManager.Resource.Pollution, num18, num18, position, __instance.m_pollutionRadius);
                }
                ReversePatches.HandleDead2(__instance, buildingID, ref buildingData, ref behaviour, totalWorkerCount);
                if (b != 0 || __instance.m_industryType == DistrictPark.ParkType.Industry)
                {
                    if (__instance.m_outputResource != TransferManager.TransferReason.None)
                    {
                        int num29 = (num17 * __instance.m_outputVehicleCount + 99) / 100;
                        int count = 0;
                        int cargo = 0;
                        int capacity = 0;
                        int outside = 0;
                        __instance.CalculateOwnVehicles(buildingID, ref buildingData, __instance.m_outputResource, ref count, ref cargo, ref capacity, ref outside);
                        buildingData.m_tempExport = (byte)Mathf.Clamp(outside, buildingData.m_tempExport, 255);
                        if (buildingData.m_finalExport != 0)
                        {
                            instance.m_districts.m_buffer[district].m_playerConsumption.m_finalExportAmount += buildingData.m_finalExport;
                        }
                        int num30 = num25;
                        if (num30 > 0 && flag)
                        {
                            num30 = Mathf.Max(num30, 16000);
                        }
                        if (num30 >= 8000 && count < num29)
                        {
                            TransferManager.TransferOffer offer = new TransferManager.TransferOffer
                            {
                                Priority = Mathf.Max(1, num30 * 8 / num24),
                                Building = buildingID,
                                Position = buildingData.m_position,
                                Amount = 1,
                                Active = true
                            };
                            Singleton<TransferManager>.instance.AddOutgoingOffer(__instance.m_outputResource, offer);
                        }
                        if (__instance.m_outputResource >= ExtendedTransferManager.MealsDeliveryLow)
                        {
                            DistrictParkManager.AddBufferStatus(b, __instance.m_outputResource, num25, 0, num24);
                        }
                        else
                        {
                            instance.m_parks.m_buffer[b].AddBufferStatus(__instance.m_outputResource, num25, 0, num24);
                        }
                    }
                    GuideController properties = Singleton<GuideManager>.instance.m_properties;
                    if (properties is not null)
                    {
                        Singleton<BuildingManager>.instance.m_extractorPlaced.Activate(properties.m_extractorPlaced);
                    }
                }
                int num31 = finalProductionRate * __instance.m_noiseAccumulation / 100;
                if (num31 != 0)
                {
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, num31, position, __instance.m_noiseRadius);
                }
            }
            buildingData.m_problems = problemStruct;
            buildingData.m_education3 = (byte)Mathf.Clamp(finalProductionRate * num13 / Mathf.Max(1, __instance.m_outputRate), 0, 255);
            if (b != 0)
            {
                instance.m_parks.m_buffer[b].AddWorkers(aliveWorkerCount);
            }
            else if (__instance.m_industryType != DistrictPark.ParkType.Industry)
            {
                GuideController properties2 = Singleton<GuideManager>.instance.m_properties;
                if ((object)properties2 != null)
                {
                    Singleton<BuildingManager>.instance.m_industryBuildingOutsideIndustryArea.Activate(properties2.m_industryBuildingOutsideIndustryArea, buildingID);
                }
            }
            instance.m_districts.m_buffer[district].AddIndustryData(__instance.m_info.m_class.m_subService, (uint)(__instance.m_info.m_cellWidth * __instance.m_info.m_cellLength), (uint)finalProductionRate);
            ReversePatches.BaseProduceGoods(__instance, buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
            return false;
        }
    }
}
