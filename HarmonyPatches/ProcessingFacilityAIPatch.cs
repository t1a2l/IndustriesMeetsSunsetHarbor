using System;
using System.Runtime.CompilerServices;
using ColossalFramework;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.Managers;
using MoreTransferReasons;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(ProcessingFacilityAI))]
    public static class ProcessingFacilityAIPatch
    {
        [HarmonyPatch(typeof(ProcessingFacilityAI), "ProduceGoods")]
        [HarmonyPrefix]
        public static bool ProduceGoods(ProcessingFacilityAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
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
            Notification.ProblemStruct problemStruct = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem1.NoResources | Notification.Problem1.NoPlaceforGoods | Notification.Problem1.NoInputProducts | Notification.Problem1.NoFishingGoods);
            bool flag = __instance.m_info.m_class.m_service == ItemClass.Service.Fishing;
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
                int num16 = __instance.m_pollutionAccumulation;
                if (b != 0)
                {
                    instance.m_parks.m_buffer[b].GetProductionFactors(out var processingFactor, out var pollutionFactor);
                    finalProductionRate = (finalProductionRate * processingFactor + 50) / 100;
                    num16 = (num16 * pollutionFactor + 50) / 100;
                }
                else if (__instance.m_industryType != DistrictPark.ParkType.Industry)
                {
                    finalProductionRate = 0;
                }
                int num17 = 0;
                int num18 = 0;
                if (__instance.m_inputResource1 != TransferManager.TransferReason.None)
                {
                    num17 = GetInputBufferSize1(__instance, parkPolicies, instance.m_parks.m_buffer[b].m_finalStorageDelta);
                    num18 = buildingData.m_customBuffer2;
                    int num19 = (__instance.m_inputRate1 * finalProductionRate + 99) / 100;
                    if (num18 < num19)
                    {
                        finalProductionRate = (num18 * 100 + __instance.m_inputRate1 - 1) / __instance.m_inputRate1;
                        problemStruct = Notification.AddProblems(problemStruct, flag ? Notification.Problem1.NoFishingGoods : ((!IsRawMaterial(__instance, __instance.m_inputResource1)) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources));
                    }
                }
                int num20 = 0;
                int num21 = 0;
                if (__instance.m_inputResource2 != TransferManager.TransferReason.None)
                {
                    num20 = GetInputBufferSize2(__instance, parkPolicies, instance.m_parks.m_buffer[b].m_finalStorageDelta);
                    num21 = (buildingData.m_teens << 8) | buildingData.m_youngs;
                    int num22 = (__instance.m_inputRate2 * finalProductionRate + 99) / 100;
                    if (num21 < num22)
                    {
                        finalProductionRate = (num21 * 100 + __instance.m_inputRate2 - 1) / __instance.m_inputRate2;
                        problemStruct = Notification.AddProblems(problemStruct, flag ? Notification.Problem1.NoFishingGoods : ((!IsRawMaterial(__instance, __instance.m_inputResource2)) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources));
                    }
                }
                int num23 = 0;
                int num24 = 0;
                if (__instance.m_inputResource3 != TransferManager.TransferReason.None)
                {
                    num23 = GetInputBufferSize3(__instance, parkPolicies, instance.m_parks.m_buffer[b].m_finalStorageDelta);
                    num24 = (buildingData.m_adults << 8) | buildingData.m_seniors;
                    int num25 = (__instance.m_inputRate3 * finalProductionRate + 99) / 100;
                    if (num24 < num25)
                    {
                        finalProductionRate = (num24 * 100 + __instance.m_inputRate3 - 1) / __instance.m_inputRate3;
                        problemStruct = Notification.AddProblems(problemStruct, flag ? Notification.Problem1.NoFishingGoods : ((!IsRawMaterial(__instance, __instance.m_inputResource3)) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources));
                    }
                }
                int num26 = 0;
                int num27 = 0;
                if (__instance.m_inputResource4 != TransferManager.TransferReason.None)
                {
                    num26 = GetInputBufferSize4(__instance, parkPolicies, instance.m_parks.m_buffer[b].m_finalStorageDelta);
                    num27 = (buildingData.m_education1 << 8) | buildingData.m_education2;
                    int num28 = (__instance.m_inputRate4 * finalProductionRate + 99) / 100;
                    if (num27 < num28)
                    {
                        finalProductionRate = (num27 * 100 + __instance.m_inputRate4 - 1) / __instance.m_inputRate4;
                        problemStruct = Notification.AddProblems(problemStruct, flag ? Notification.Problem1.NoFishingGoods : ((!IsRawMaterial(__instance, __instance.m_inputResource4)) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources));
                    }
                }
                int num29 = 0;
                int num30 = 0;
                if (__instance.m_outputResource != TransferManager.TransferReason.None)
                {
                    num29 = GetOutputBufferSize(__instance, parkPolicies, instance.m_parks.m_buffer[b].m_finalStorageDelta);
                    num30 = buildingData.m_customBuffer1;
                    int num31 = (num13 * finalProductionRate + 99) / 100;
                    if (num29 - num30 < num31)
                    {
                        num31 = Mathf.Max(0, num29 - num30);
                        finalProductionRate = (num31 * 100 + num13 - 1) / num13;
                        if (__instance.m_outputVehicleCount != 0)
                        {
                            problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.NoPlaceforGoods);
                            if (__instance.m_info.m_class.m_service == ItemClass.Service.PlayerIndustry)
                            {
                                GuideController properties = Singleton<GuideManager>.instance.m_properties;
                                if (properties is not null)
                                {
                                    Singleton<BuildingManager>.instance.m_warehouseNeeded.Activate(properties.m_warehouseNeeded, buildingID);
                                }
                            }
                        }
                    }
                }
                if (__instance.m_inputResource1 != TransferManager.TransferReason.None)
                {
                    int num32 = (__instance.m_inputRate1 * finalProductionRate + 99) / 100;
                    num18 = Mathf.Max(0, num18 - num32);
                    buildingData.m_customBuffer2 = (ushort)num18;
                    if(__instance.m_inputResource1 >= ExtendedTransferManager.MealsDeliveryLow)
                    {
                        DistrictParkManager.AddConsumptionAmount(b, __instance.m_inputResource1, num32);
                    }
                    else
                    {
                        instance.m_parks.m_buffer[b].AddConsumptionAmount(__instance.m_inputResource1, num32);
                    }
                        
                }
                if (__instance.m_inputResource2 != TransferManager.TransferReason.None)
                {
                    int num33 = (__instance.m_inputRate2 * finalProductionRate + 99) / 100;
                    num21 = Mathf.Max(0, num21 - num33);
                    buildingData.m_youngs = (byte)(num21 & 0xFF);
                    buildingData.m_teens = (byte)(num21 >> 8);
                    if (__instance.m_inputResource2 >= ExtendedTransferManager.MealsDeliveryLow)
                    {
                        DistrictParkManager.AddConsumptionAmount(b, __instance.m_inputResource2, num33);
                    }
                    else
                    {
                        instance.m_parks.m_buffer[b].AddConsumptionAmount(__instance.m_inputResource2, num33);
                    }
                }
                if (__instance.m_inputResource3 != TransferManager.TransferReason.None)
                {
                    int num34 = (__instance.m_inputRate3 * finalProductionRate + 99) / 100;
                    num24 = Mathf.Max(0, num24 - num34);
                    buildingData.m_seniors = (byte)(num24 & 0xFF);
                    buildingData.m_adults = (byte)(num24 >> 8);
                    if (__instance.m_inputResource3 >= ExtendedTransferManager.MealsDeliveryLow)
                    {
                        DistrictParkManager.AddConsumptionAmount(b, __instance.m_inputResource3, num34);
                    }
                    else
                    {
                        instance.m_parks.m_buffer[b].AddConsumptionAmount(__instance.m_inputResource3, num34);
                    }
                }
                if (__instance.m_inputResource4 != TransferManager.TransferReason.None)
                {
                    int num35 = (__instance.m_inputRate4 * finalProductionRate + 99) / 100;
                    num27 = Mathf.Max(0, num27 - num35);
                    buildingData.m_education2 = (byte)(num27 & 0xFF);
                    buildingData.m_education1 = (byte)(num27 >> 8);
                    if (__instance.m_inputResource4 >= ExtendedTransferManager.MealsDeliveryLow)
                    {
                        DistrictParkManager.AddConsumptionAmount(b, __instance.m_inputResource4, num35);
                    }
                    else
                    {
                        instance.m_parks.m_buffer[b].AddConsumptionAmount(__instance.m_inputResource4, num35);
                    }
                }
                if (__instance.m_outputResource != TransferManager.TransferReason.None)
                {
                    int num36 = (num13 * finalProductionRate + 99) / 100;
                    num30 = Mathf.Min(num29, num30 + num36);
                    buildingData.m_customBuffer1 = (ushort)num30;
                    if (__instance.m_outputResource >= ExtendedTransferManager.MealsDeliveryLow)
                    {
                        DistrictParkManager.AddProductionAmount(b, __instance.m_outputResource, num36);
                    }
                    else
                    {
                        instance.m_parks.m_buffer[b].AddProductionAmount(__instance.m_outputResource, num36);
                    }
                }
                num16 = (finalProductionRate * num16 + 50) / 100;
                if (num16 != 0)
                {
                    num16 = UniqueFacultyAI.DecreaseByBonus(UniqueFacultyAI.FacultyBonus.Science, num16);
                    Singleton<NaturalResourceManager>.instance.TryDumpResource(NaturalResourceManager.Resource.Pollution, num16, num16, position, __instance.m_pollutionRadius);
                }
                HandleDead2(__instance, buildingID, ref buildingData, ref behaviour, totalWorkerCount);
                if (b != 0 || __instance.m_industryType == DistrictPark.ParkType.Industry)
                {
                    int num37 = 0;
                    if (__instance.m_inputResource1 != TransferManager.TransferReason.None)
                    {
                        int count = 0;
                        int cargo = 0;
                        int capacity = 0;
                        int outside = 0;
                        __instance.CalculateGuestVehicles(buildingID, ref buildingData, __instance.m_inputResource1, ref count, ref cargo, ref capacity, ref outside);
                        if (outside != 0)
                        {
                            num37 |= 1;
                        }
                        int num38 = num17 - num18 - cargo;
                        if (num38 >= 4000)
                        {
                            TransferManager.TransferOffer offer = new()
                            {
                                Priority = Mathf.Max(1, num38 * 8 / num17),
                                Building = buildingID,
                                Position = buildingData.m_position,
                                Amount = 1,
                                Active = false
                            };
                            Singleton<TransferManager>.instance.AddIncomingOffer(__instance.m_inputResource1, offer);
                        }
                        if (__instance.m_inputResource1 >= ExtendedTransferManager.MealsDeliveryLow)
                        {
                            DistrictParkManager.AddBufferStatus(b, __instance.m_inputResource1, num18, cargo, num17);
                        }
                        else
                        {
                            instance.m_parks.m_buffer[b].AddBufferStatus(__instance.m_inputResource1, num18, cargo, num17);
                        }
                        
                    }
                    if (__instance.m_inputResource2 != TransferManager.TransferReason.None)
                    {
                        int count2 = 0;
                        int cargo2 = 0;
                        int capacity2 = 0;
                        int outside2 = 0;
                        __instance.CalculateGuestVehicles(buildingID, ref buildingData, __instance.m_inputResource2, ref count2, ref cargo2, ref capacity2, ref outside2);
                        if (outside2 != 0)
                        {
                            num37 |= 2;
                        }
                        int num39 = num20 - num21 - cargo2;
                        if (num39 >= 4000)
                        {
                            TransferManager.TransferOffer offer2 = new()
                            {
                                Priority = Mathf.Max(1, num39 * 8 / num20),
                                Building = buildingID,
                                Position = buildingData.m_position,
                                Amount = 1,
                                Active = false
                            };
                            Singleton<TransferManager>.instance.AddIncomingOffer(__instance.m_inputResource2, offer2);
                        }
                        if (__instance.m_inputResource2 >= ExtendedTransferManager.MealsDeliveryLow)
                        {
                            DistrictParkManager.AddBufferStatus(b, __instance.m_inputResource2, num21, cargo2, num20);
                        }
                        else
                        {
                            instance.m_parks.m_buffer[b].AddBufferStatus(__instance.m_inputResource2, num21, cargo2, num20);
                        }
                    }
                    if (__instance.m_inputResource3 != TransferManager.TransferReason.None)
                    {
                        int count3 = 0;
                        int cargo3 = 0;
                        int capacity3 = 0;
                        int outside3 = 0;
                        __instance.CalculateGuestVehicles(buildingID, ref buildingData, __instance.m_inputResource3, ref count3, ref cargo3, ref capacity3, ref outside3);
                        if (outside3 != 0)
                        {
                            num37 |= 4;
                        }
                        int num40 = num23 - num24 - cargo3;
                        if (num40 >= 4000)
                        {
                            TransferManager.TransferOffer offer3 = new()
                            {
                                Priority = Mathf.Max(1, num40 * 8 / num23),
                                Building = buildingID,
                                Position = buildingData.m_position,
                                Amount = 1,
                                Active = false
                            };
                            Singleton<TransferManager>.instance.AddIncomingOffer(__instance.m_inputResource3, offer3);
                        }
                        if (__instance.m_inputResource3 >= ExtendedTransferManager.MealsDeliveryLow)
                        {
                            DistrictParkManager.AddBufferStatus(b, __instance.m_inputResource3, num24, cargo3, num23);
                        }
                        else
                        {
                            instance.m_parks.m_buffer[b].AddBufferStatus(__instance.m_inputResource3, num24, cargo3, num23);
                        }
                    }
                    if (__instance.m_inputResource4 != TransferManager.TransferReason.None)
                    {
                        int count4 = 0;
                        int cargo4 = 0;
                        int capacity4 = 0;
                        int outside4 = 0;
                        __instance.CalculateGuestVehicles(buildingID, ref buildingData, __instance.m_inputResource4, ref count4, ref cargo4, ref capacity4, ref outside4);
                        if (outside4 != 0)
                        {
                            num37 |= 8;
                        }
                        int num41 = num26 - num27 - cargo4;
                        if (num41 >= 4000)
                        {
                            TransferManager.TransferOffer offer4 = new()
                            {
                                Priority = Mathf.Max(1, num41 * 8 / num26),
                                Building = buildingID,
                                Position = buildingData.m_position,
                                Amount = 1,
                                Active = false
                            };
                            Singleton<TransferManager>.instance.AddIncomingOffer(__instance.m_inputResource4, offer4);
                        }
                        if (__instance.m_inputResource4 >= ExtendedTransferManager.MealsDeliveryLow)
                        {
                            DistrictParkManager.AddBufferStatus(b, __instance.m_inputResource4, num27, cargo4, num26);
                        }
                        else
                        {
                            instance.m_parks.m_buffer[b].AddBufferStatus(__instance.m_inputResource4, num27, cargo4, num26);
                        }
                    }
                    buildingData.m_tempImport |= (byte)num37;
                    if (__instance.m_outputResource != TransferManager.TransferReason.None)
                    {
                        if (__instance.m_outputVehicleCount == 0)
                        {
                            if (num30 == num29)
                            {
                                int num42 = (num30 * IndustryBuildingAI.GetResourcePrice(__instance.m_outputResource) + 50) / 100;
                                if ((instance.m_districts.m_buffer[district].m_cityPlanningPolicies & DistrictPolicies.CityPlanning.SustainableFishing) != DistrictPolicies.CityPlanning.None)
                                {
                                    num42 = (num42 * 105 + 99) / 100;
                                }
                                num42 = UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Science, num42);
                                Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.ResourcePrice, num42, __instance.m_info.m_class);
                                if (b != 0)
                                {
                                    if (__instance.m_outputResource >= ExtendedTransferManager.MealsDeliveryLow)
                                    {
                                        DistrictParkManager.AddExportAmount(b, __instance.m_outputResource, num30);
                                    }
                                    else
                                    {
                                        instance.m_parks.m_buffer[b].AddExportAmount(__instance.m_outputResource, num30);
                                    }
                                }
                                num30 = 0;
                                buildingData.m_customBuffer1 = (ushort)num30;
                                buildingData.m_tempExport = byte.MaxValue;
                            }
                        }
                        else
                        {
                            int count5 = 0;
                            int cargo5 = 0;
                            int capacity5 = 0;
                            int outside5 = 0;
                            __instance.CalculateOwnVehicles(buildingID, ref buildingData, __instance.m_outputResource, ref count5, ref cargo5, ref capacity5, ref outside5);
                            buildingData.m_tempExport = (byte)Mathf.Clamp(outside5, buildingData.m_tempExport, 255);
                            int budget = Singleton<EconomyManager>.instance.GetBudget(__instance.m_info.m_class);
                            int productionRate2 = PlayerBuildingAI.GetProductionRate(100, budget);
                            int num43 = (productionRate2 * __instance.m_outputVehicleCount + 99) / 100;
                            int num44 = num30;
                            if (num44 >= 8000 && count5 < num43)
                            {
                                TransferManager.TransferOffer offer5 = new TransferManager.TransferOffer
                                {
                                    Priority = Mathf.Max(1, num44 * 8 / num29),
                                    Building = buildingID,
                                    Position = buildingData.m_position,
                                    Amount = 1,
                                    Active = true
                                };
                                Singleton<TransferManager>.instance.AddOutgoingOffer(__instance.m_outputResource, offer5);
                            }
                        }
                        if (__instance.m_outputResource >= ExtendedTransferManager.MealsDeliveryLow)
                        {
                            DistrictParkManager.AddBufferStatus(b, __instance.m_outputResource, num30, 0, num29);
                        }
                        else
                        {
                            instance.m_parks.m_buffer[b].AddBufferStatus(__instance.m_outputResource, num30, 0, num29);
                        }
                    }
                }
                if (buildingData.m_finalImport != 0)
                {
                    instance.m_districts.m_buffer[district].m_playerConsumption.m_finalImportAmount += buildingData.m_finalImport;
                }
                if (buildingData.m_finalExport != 0)
                {
                    instance.m_districts.m_buffer[district].m_playerConsumption.m_finalExportAmount += buildingData.m_finalExport;
                }
                int num45 = finalProductionRate * __instance.m_noiseAccumulation / 100;
                if (num45 != 0)
                {
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, num45, position, __instance.m_noiseRadius);
                }
            }
            buildingData.m_problems = problemStruct;
            buildingData.m_education3 = (byte)Mathf.Clamp(finalProductionRate * num13 / Mathf.Max(1, __instance.m_outputRate), 0, 255);
            buildingData.m_health = (byte)Mathf.Clamp(finalProductionRate, 0, 255);
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
            BaseProduceGoods(__instance, buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
            return false;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(PlayerBuildingAI), "ProduceGoods")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void BaseProduceGoods(object instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            string message = "ProduceGoods reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ProcessingFacilityAI), "GetInputBufferSize1")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int GetInputBufferSize1(object instance, DistrictPolicies.Park policies, int storageDelta)
        {
            string message = "GetInputBufferSize1 reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ProcessingFacilityAI), "GetInputBufferSize2")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int GetInputBufferSize2(object instance, DistrictPolicies.Park policies, int storageDelta)
        {
            string message = "GetInputBufferSize2 reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ProcessingFacilityAI), "GetInputBufferSize3")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int GetInputBufferSize3(object instance, DistrictPolicies.Park policies, int storageDelta)
        {
            string message = "GetInputBufferSize3 reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ProcessingFacilityAI), "GetInputBufferSize4")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int GetInputBufferSize4(object instance, DistrictPolicies.Park policies, int storageDelta)
        {
            string message = "GetInputBufferSize4 reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ProcessingFacilityAI), "GetOutputBufferSize")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int GetOutputBufferSize(object instance, DistrictPolicies.Park policies, int storageDelta)
        {
            string message = "GetOutputBufferSize reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(CommonBuildingAI), "HandleDead2")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void HandleDead2(object instance, ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, int citizenCount)
        {
            string message = "HandleDead2 reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(CommonBuildingAI), "IsRawMaterial")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool IsRawMaterial(object instance, TransferManager.TransferReason material)
        {
            string message = "IsRawMaterial reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }
    }
}
