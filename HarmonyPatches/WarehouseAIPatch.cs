using System;
using System.Runtime.CompilerServices;
using ColossalFramework;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.Managers;
using MoreTransferReasons;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(WarehouseAI))]
    public static class WarehouseAIPatch
    {
        [HarmonyPatch(typeof(WarehouseAI), "ProduceGoods")]
        [HarmonyPrefix]
        public static bool ProduceGoods(WarehouseAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount, ref int ___m_subStations)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte b = instance.GetPark(buildingData.m_position);
            if (b != 0 && !instance.m_parks.m_buffer[b].IsIndustry)
            {
                b = 0;
            }
            if (finalProductionRate != 0)
            {
                HandleDead(__instance, buildingID, ref buildingData, ref behaviour, totalWorkerCount);
                TransferManager.TransferReason actualTransferReason = __instance.GetActualTransferReason(buildingID, ref buildingData);
                TransferManager.TransferReason transferReason = __instance.GetTransferReason(buildingID, ref buildingData);
                if (actualTransferReason != TransferManager.TransferReason.None)
                {
                    int maxLoadSize = GetMaxLoadSize(__instance);
                    bool flag = __instance.IsFull(buildingID, ref buildingData);
                    int num = buildingData.m_customBuffer1 * 100;
                    int num2 = (finalProductionRate * __instance.m_truckCount + 99) / 100;
                    int count = 0;
                    int cargo = 0;
                    int capacity = 0;
                    int outside = 0;
                    __instance.CalculateOwnVehicles(buildingID, ref buildingData, actualTransferReason, ref count, ref cargo, ref capacity, ref outside);
                    buildingData.m_tempExport = (byte)Mathf.Clamp(outside, buildingData.m_tempExport, 255);
                    int count2 = 0;
                    int cargo2 = 0;
                    int capacity2 = 0;
                    int outside2 = 0;
                    __instance.CalculateGuestVehicles(buildingID, ref buildingData, actualTransferReason, ref count2, ref cargo2, ref capacity2, ref outside2);
                    buildingData.m_tempImport = (byte)Mathf.Clamp(outside2, buildingData.m_tempImport, 255);
                    if (b != 0)
                    {
                        if (actualTransferReason >= ExtendedTransferManager.MealsDeliveryLow)
                        {
                            DistrictParkManager.AddBufferStatus(b, actualTransferReason, num, cargo2, __instance.m_storageCapacity);
                        }
                        else
                        {
                            instance.m_parks.m_buffer[b].AddBufferStatus(actualTransferReason, num, cargo2, __instance.m_storageCapacity);
                        }
                    }
                    ushort num3 = buildingID;
                    if (___m_subStations > 0)
                    {
                        uint num4 = Singleton<SimulationManager>.instance.m_randomizer.UInt32((uint)(___m_subStations * 5 + 1));
                        if (num4 != 0)
                        {
                            for (ushort subBuilding = buildingData.m_subBuilding; subBuilding != 0; subBuilding = Singleton<BuildingManager>.instance.m_buildings.m_buffer[subBuilding].m_subBuilding)
                            {
                                BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[subBuilding].Info;
                                if (info != null && info.m_buildingAI is WarehouseStationAI)
                                {
                                    if (num4 <= 5)
                                    {
                                        num3 = subBuilding;
                                        break;
                                    }
                                    num4 -= 5;
                                }
                            }
                        }
                    }
                    bool flag2 = num3 == buildingID;
                    if (transferReason == actualTransferReason)
                    {
                        if (num >= maxLoadSize && (count < num2 || !flag2))
                        {
                            TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                            if ((buildingData.m_flags & Building.Flags.Filling) != Building.Flags.None)
                            {
                                offer.Priority = 0;
                            }
                            else if ((buildingData.m_flags & Building.Flags.Downgrading) != Building.Flags.None)
                            {
                                offer.Priority = Mathf.Clamp(num / Mathf.Max(1, __instance.m_storageCapacity >> 2) + 2, 0, 2);
                                if (!flag2)
                                {
                                    offer.Priority += 2;
                                }
                            }
                            else
                            {
                                offer.Priority = Mathf.Clamp(num / Mathf.Max(1, __instance.m_storageCapacity >> 2) - 1, 0, 2);
                            }
                            offer.Building = num3;
                            offer.Position = buildingData.m_position;
                            offer.Amount = ((!flag2) ? Mathf.Clamp(num / maxLoadSize, 1, 15) : Mathf.Min(Mathf.Max(1, num / maxLoadSize), num2 - count));
                            offer.Active = true;
                            offer.Exclude = flag2;
                            offer.Unlimited = !flag2;
                            Singleton<TransferManager>.instance.AddOutgoingOffer(actualTransferReason, offer);
                        }
                        if ((buildingData.m_flags & Building.Flags.Downgrading) != Building.Flags.None)
                        {
                            Vehicle[] buffer = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
                            ushort num5 = buildingData.m_guestVehicles;
                            int num6 = 0;
                            while (num5 != 0 && cargo2 > 0 && (float)(num + cargo2) > (float)__instance.m_storageCapacity * 0.2f + (float)maxLoadSize)
                            {
                                ushort nextGuestVehicle = buffer[num5].m_nextGuestVehicle;
                                if (buffer[num5].m_targetBuilding == buildingID && (TransferManager.TransferReason)buffer[num5].m_transferType == actualTransferReason)
                                {
                                    VehicleInfo info2 = buffer[num5].Info;
                                    if (info2 != null)
                                    {
                                        cargo2 = Mathf.Max(0, cargo2 - buffer[num5].m_transferSize);
                                        info2.m_vehicleAI.SetTarget(num5, ref buffer[num5], 0);
                                    }
                                }
                                num5 = nextGuestVehicle;
                                if (++num6 > 16384)
                                {
                                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                    break;
                                }
                            }
                        }
                        num += cargo2;
                        if (num < __instance.m_storageCapacity)
                        {
                            TransferManager.TransferOffer offer2 = default;
                            bool flag3 = true;
                            if ((buildingData.m_flags & Building.Flags.Downgrading) != Building.Flags.None)
                            {
                                if ((float)num < (float)__instance.m_storageCapacity * 0.2f)
                                {
                                    offer2.Priority = 0;
                                }
                                else
                                {
                                    flag3 = false;
                                }
                            }
                            else if ((buildingData.m_flags & Building.Flags.Filling) != Building.Flags.None)
                            {
                                offer2.Priority = Mathf.Clamp((__instance.m_storageCapacity - num) / Mathf.Max(1, __instance.m_storageCapacity >> 2) + 1, 0, 2);
                                if (!flag2)
                                {
                                    offer2.Priority += 2;
                                }
                            }
                            else
                            {
                                offer2.Priority = Mathf.Clamp((__instance.m_storageCapacity - num) / Mathf.Max(1, __instance.m_storageCapacity >> 2) - 1, 0, 2);
                            }
                            if (flag3)
                            {
                                offer2.Building = num3;
                                offer2.Position = buildingData.m_position;
                                offer2.Amount = Mathf.Max(1, (__instance.m_storageCapacity - num) / maxLoadSize);
                                offer2.Active = false;
                                offer2.Exclude = flag2;
                                offer2.Unlimited = !flag2;
                                Singleton<TransferManager>.instance.AddIncomingOffer(actualTransferReason, offer2);
                            }
                        }
                    }
                    else if (num > 0 && (count < num2 || !flag2))
                    {
                        TransferManager.TransferOffer offer3 = new()
                        {
                            Priority = 8,
                            Building = num3,
                            Position = buildingData.m_position,
                            Amount = ((!flag2) ? Mathf.Clamp(num / maxLoadSize, 1, 15) : Mathf.Min(Mathf.Max(1, num / maxLoadSize), num2 - count)),
                            Active = true,
                            Exclude = flag2,
                            Unlimited = !flag2
                        };
                        Singleton<TransferManager>.instance.AddOutgoingOffer(actualTransferReason, offer3);
                    }
                    bool flag4 = __instance.IsFull(buildingID, ref buildingData);
                    if (flag != flag4)
                    {
                        if (flag4)
                        {
                            __instance.m_fullPassMilestone?.Unlock();
                        }
                        else
                        {
                            __instance.m_fullPassMilestone?.Relock();
                        }
                    }
                }
                if (actualTransferReason != transferReason && buildingData.m_customBuffer1 == 0)
                {
                    buildingData.m_adults = buildingData.m_seniors;
                    SetContentFlags(__instance, buildingID, ref buildingData, transferReason);
                }
                int num7 = finalProductionRate * __instance.m_noiseAccumulation / 100;
                if (num7 != 0)
                {
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, num7, buildingData.m_position, __instance.m_noiseRadius);
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
        [HarmonyPatch(typeof(CommonBuildingAI), "HandleDead")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void HandleDead(object instance, ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, int citizenCount)
        {
            string message = "HandleDead reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(WarehouseAI), "GetMaxLoadSize")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int GetMaxLoadSize(object instance)
        {
            string message = "GetMaxLoadSize reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(WarehouseAI), "SetContentFlags")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int SetContentFlags(object instance, ushort buildingID, ref Building data, TransferManager.TransferReason material)
        {
            string message = "SetContentFlags reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

    }
}
