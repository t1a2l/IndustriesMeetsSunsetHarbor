using ColossalFramework;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.AI;
using UnityEngine;
using System.Reflection;
using IndustriesMeetsSunsetHarbor.Managers;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(PlayerBuildingAI))]
    public static class PlayerBuildingAIPatch
    {
        private delegate int GetMaintenanceCostDelegate(PlayerBuildingAI instance, int productionRate, int serviceBudget, int maintenanceCost);
        private static GetMaintenanceCostDelegate GetMaintenanceCost = AccessTools.MethodDelegate<GetMaintenanceCostDelegate>(typeof(PlayerBuildingAI).GetMethod("GetMaintenanceCost", BindingFlags.Instance | BindingFlags.NonPublic), null, true);

        private delegate int AdjustMaintenanceCostDelegate(PlayerBuildingAI instance, ushort buildingID, ref Building data, int maintenanceCost);
        private static AdjustMaintenanceCostDelegate AdjustMaintenanceCost = AccessTools.MethodDelegate<AdjustMaintenanceCostDelegate>(typeof(PlayerBuildingAI).GetMethod("AdjustMaintenanceCost", BindingFlags.Instance | BindingFlags.NonPublic), null, true);

        private delegate ItemClass GetBudgetItemClassDelegate(PlayerBuildingAI instance);
        private static GetBudgetItemClassDelegate GetBudgetItemClass = AccessTools.MethodDelegate<GetBudgetItemClassDelegate>(typeof(PlayerBuildingAI).GetMethod("GetBudgetItemClass", BindingFlags.Instance | BindingFlags.NonPublic), null, true);

        private delegate void HandleWorkAndVisitPlacesDelegate(PlayerBuildingAI instance, ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveWorkerCount, ref int totalWorkerCount, ref int workPlaceCount, ref int aliveVisitorCount, ref int totalVisitorCount, ref int visitPlaceCount);
        private static HandleWorkAndVisitPlacesDelegate HandleWorkAndVisitPlaces = AccessTools.MethodDelegate<HandleWorkAndVisitPlacesDelegate>(typeof(PlayerBuildingAI).GetMethod("HandleWorkAndVisitPlaces", BindingFlags.Instance | BindingFlags.NonPublic), null, true);

        private delegate int HandleCommonConsumptionDelegate(PlayerBuildingAI instance, ushort buildingID, ref Building data, ref Building.Frame frameData, ref int electricityConsumption, ref int heatingConsumption, ref int waterConsumption, ref int sewageAccumulation, ref int garbageAccumulation, ref int mailAccumulation, int maxMail, DistrictPolicies.Services policies);
        private static HandleCommonConsumptionDelegate HandleCommonConsumption = AccessTools.MethodDelegate<HandleCommonConsumptionDelegate>(typeof(PlayerBuildingAI).GetMethod("HandleCommonConsumption", BindingFlags.Instance | BindingFlags.NonPublic), null, true);

        private delegate bool CollapseIfFloodedDelegate(CommonBuildingAI instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData);
        private static CollapseIfFloodedDelegate CollapseIfFlooded = AccessTools.MethodDelegate<CollapseIfFloodedDelegate>(typeof(CommonBuildingAI).GetMethod("CollapseIfFlooded", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate void HandleCrimeDelegate(CommonBuildingAI instance, ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount);
        private static HandleCrimeDelegate HandleCrime = AccessTools.MethodDelegate<HandleCrimeDelegate>(typeof(CommonBuildingAI).GetMethod("HandleCrime", BindingFlags.Instance | BindingFlags.NonPublic), null, true);

        private delegate void HandleFireDelegate(CommonBuildingAI instance, ushort buildingID, ref Building data, ref Building.Frame frameData, DistrictPolicies.Services policies);
        private static HandleFireDelegate HandleFire = AccessTools.MethodDelegate<HandleFireDelegate>(typeof(CommonBuildingAI).GetMethod("HandleFire", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate bool GetFireParametersDelegate(CommonBuildingAI instance, ushort buildingID, ref Building buildingData, out int fireHazard, out int fireSize, out int fireTolerance);
        private static GetFireParametersDelegate GetFireParameters = AccessTools.MethodDelegate<GetFireParametersDelegate>(typeof(BuildingAI).GetMethod("GetFireParameters", BindingFlags.Instance | BindingFlags.Public), null, true);

        private delegate void SetEvacuatingDelegate(CommonBuildingAI instance, ushort buildingID, ref Building data, bool evacuating);
        private static SetEvacuatingDelegate SetEvacuating = AccessTools.MethodDelegate<SetEvacuatingDelegate>(typeof(CommonBuildingAI).GetMethod("SetEvacuating", BindingFlags.Instance | BindingFlags.NonPublic), null, true);

        [HarmonyPatch(typeof(PlayerBuildingAI), "ProduceGoods")]
        [HarmonyPrefix]
        public static bool ProduceGoods(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            if(buildingData.Info.GetAI() is RestaurantAI && (buildingData.m_flags & Building.Flags.Active) != 0)
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(PlayerBuildingAI), "SimulationStepActive")]
        [HarmonyPrefix]
        public static bool SimulationStepActive(PlayerBuildingAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            if (buildingData.Info.GetAI() is MultiProcessingFacilityAI multiProcessingFacilityAI)
            {
                var custom_buffers = CustomBuffersManager.GetCustomBuffer(buildingID);
                int productionRate1 = (int)custom_buffers.m_customBuffer10;
                int productionRate2 = (int)custom_buffers.m_customBuffer11;
                int productionRate3 = (int)custom_buffers.m_customBuffer12;
                int productionRate4 = (int)custom_buffers.m_customBuffer13;

                int finalProductionRate1 = productionRate1;
                int finalProductionRate2 = productionRate2;
                int finalProductionRate3 = productionRate3;
                int finalProductionRate4 = productionRate4;
                if ((buildingData.m_flags & Building.Flags.Evacuating) != 0)
                {
                    finalProductionRate1 = 0;
                    finalProductionRate2 = 0;
                    finalProductionRate3 = 0;
                    finalProductionRate4 = 0;
                }
                int budget = __instance.GetBudget(buildingID, ref buildingData);
                int averageBudget = __instance.GetAverageBudget(buildingID, ref buildingData);
                int maintenanceCost = __instance.GetMaintenanceCost() / 100;
                int maintenanceCost1 = GetMaintenanceCost(__instance, finalProductionRate1, budget, maintenanceCost);
                int maintenanceCost2 = GetMaintenanceCost(__instance, finalProductionRate2, budget, maintenanceCost);
                int maintenanceCost3 = GetMaintenanceCost(__instance, finalProductionRate3, budget, maintenanceCost);
                int maintenanceCost4 = GetMaintenanceCost(__instance, finalProductionRate4, budget, maintenanceCost);
                maintenanceCost1 = AdjustMaintenanceCost(__instance, buildingID, ref buildingData, maintenanceCost1);
                maintenanceCost2 = AdjustMaintenanceCost(__instance, buildingID, ref buildingData, maintenanceCost2);
                maintenanceCost3 = AdjustMaintenanceCost(__instance, buildingID, ref buildingData, maintenanceCost3);
                maintenanceCost4 = AdjustMaintenanceCost(__instance, buildingID, ref buildingData, maintenanceCost4);

                productionRate1 = PlayerBuildingAI.GetProductionRate(productionRate1, averageBudget);
                productionRate2 = PlayerBuildingAI.GetProductionRate(productionRate2, averageBudget);
                productionRate3 = PlayerBuildingAI.GetProductionRate(productionRate3, averageBudget);
                productionRate4 = PlayerBuildingAI.GetProductionRate(productionRate4, averageBudget);

                finalProductionRate1 = PlayerBuildingAI.GetProductionRate(finalProductionRate1, budget);
                finalProductionRate2 = PlayerBuildingAI.GetProductionRate(finalProductionRate2, budget);
                finalProductionRate3 = PlayerBuildingAI.GetProductionRate(finalProductionRate3, budget);
                finalProductionRate4 = PlayerBuildingAI.GetProductionRate(finalProductionRate4, budget);

                DistrictManager instance = Singleton<DistrictManager>.instance;
                byte district = instance.GetDistrict(buildingData.m_position);
                DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[district].m_servicePolicies;
                if (productionRate1 != 0 || productionRate2 != 0 || productionRate3 != 0 || productionRate4 != 0)
                {
                    buildingData.m_problems = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem1.TurnedOff);
                }
                else
                {
                    buildingData.m_problems = Notification.AddProblems(buildingData.m_problems, Notification.Problem1.TurnedOff);
                }
                if (buildingData.m_fireIntensity != 0)
                {
                    finalProductionRate1 = 0;
                    finalProductionRate2 = 0;
                    finalProductionRate3 = 0;
                    finalProductionRate4 = 0;
                }
                if ((buildingData.m_flags & Building.Flags.Original) == 0 && maintenanceCost1 != 0)
                {
                    int num = Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Maintenance, maintenanceCost1, GetBudgetItemClass(__instance));
                    if (num < maintenanceCost1)
                    {
                        finalProductionRate1 = num * 100 / maintenanceCost1;
                    }
                }
                if ((buildingData.m_flags & Building.Flags.Original) == 0 && maintenanceCost2 != 0)
                {
                    int num = Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Maintenance, maintenanceCost2, GetBudgetItemClass(__instance));
                    if (num < maintenanceCost2)
                    {
                        finalProductionRate2 = num * 100 / maintenanceCost2;
                    }
                }
                if ((buildingData.m_flags & Building.Flags.Original) == 0 && maintenanceCost3 != 0)
                {
                    int num = Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Maintenance, maintenanceCost3, GetBudgetItemClass(__instance));
                    if (num < maintenanceCost3)
                    {
                        finalProductionRate3 = num * 100 / maintenanceCost3;
                    }
                }
                if ((buildingData.m_flags & Building.Flags.Original) == 0 && maintenanceCost4 != 0)
                {
                    int num = Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Maintenance, maintenanceCost4, GetBudgetItemClass(__instance));
                    if (num < maintenanceCost4)
                    {
                        finalProductionRate4 = num * 100 / maintenanceCost4;
                    }
                }
                int finalProductionRate = finalProductionRate1 + finalProductionRate2 + finalProductionRate3 + finalProductionRate4;

                int electricityConsumption = finalProductionRate * __instance.m_electricityConsumption / 100;
                int waterConsumption = finalProductionRate * __instance.m_waterConsumption / 100;
                int sewageAccumulation = finalProductionRate * __instance.m_sewageAccumulation / 100;
                int garbageAccumulation = finalProductionRate * __instance.m_garbageAccumulation / 100;
                int heatingConsumption = 0;
                if (electricityConsumption != 0 && instance.IsPolicyLoaded(DistrictPolicies.Policies.ExtraInsulation))
                {
                    heatingConsumption = (((servicePolicies & DistrictPolicies.Services.ExtraInsulation) == 0) ? Mathf.Max(1, electricityConsumption + 2 >> 2) : Mathf.Max(1, electricityConsumption * 3 + 8 >> 4));
                }
                if (garbageAccumulation != 0)
                {
                    if ((servicePolicies & DistrictPolicies.Services.Recycling) != 0)
                    {
                        garbageAccumulation = Mathf.Max(1, garbageAccumulation * 85 / 100);
                    }
                    if (buildingData.Info.m_buildingAI is CampusBuildingAI campusBuildingAI)
                    {
                        byte area = campusBuildingAI.GetArea(buildingID, ref buildingData);
                        if (area != 0 && (Singleton<DistrictManager>.instance.m_parks.m_buffer[area].m_flags & DistrictPark.Flags.TogaParty) != 0)
                        {
                            garbageAccumulation = (garbageAccumulation * 120 + 99) / 100;
                        }
                    }
                }
                Citizen.BehaviourData behaviour = default;
                int aliveWorkerCount = 0;
                int totalWorkerCount = 0;
                int workPlaceCount = 0;
                int aliveVisitorCount = 0;
                int totalVisitorCount = 0;
                int visitPlaceCount = 0;
                HandleWorkAndVisitPlaces(__instance, buildingID, ref buildingData, ref behaviour, ref aliveWorkerCount, ref totalWorkerCount, ref workPlaceCount, ref aliveVisitorCount, ref totalVisitorCount, ref visitPlaceCount);
                int mailAccumulation = (finalProductionRate * (aliveWorkerCount + aliveVisitorCount / 5) + Singleton<SimulationManager>.instance.m_randomizer.Int32(100u)) / 100;
                bool flag = (buildingData.m_flags & Building.Flags.Active) != 0;

                if (finalProductionRate1 != 0)
                {
                    int maxMail = workPlaceCount * 50 + visitPlaceCount * 5;
                    int num2 = HandleCommonConsumption(__instance, buildingID, ref buildingData, ref frameData, ref electricityConsumption, ref heatingConsumption, ref waterConsumption, ref sewageAccumulation, ref garbageAccumulation, ref mailAccumulation, maxMail, servicePolicies);
                    finalProductionRate1 = (finalProductionRate1 * num2 + 99) / 100;
                    if (finalProductionRate1 != 0)
                    {
                        if (num2 < 100)
                        {
                            buildingData.m_flags |= Building.Flags.RateReduced;
                        }
                        else
                        {
                            buildingData.m_flags &= ~Building.Flags.RateReduced;
                        }
                        multiProcessingFacilityAI.ProduceGoods1(buildingID, ref buildingData, ref frameData, productionRate1, finalProductionRate1, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
                    }
                    else
                    {
                        buildingData.m_flags &= ~Building.Flags.RateReduced;
                        multiProcessingFacilityAI.ProduceGoods1(buildingID, ref buildingData, ref frameData, productionRate1, finalProductionRate1, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
                    }
                    if (aliveWorkerCount != 0)
                    {
                        float radius = (float)(buildingData.Width + buildingData.Length) * 2.5f;
                        Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Density, aliveWorkerCount, buildingData.m_position, radius);
                    }
                }
                else
                {
                    electricityConsumption = 0;
                    heatingConsumption = 0;
                    waterConsumption = 0;
                    sewageAccumulation = 0;
                    garbageAccumulation = 0;
                    buildingData.m_problems = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem1.Electricity | Notification.Problem1.Water | Notification.Problem1.DirtyWater | Notification.Problem1.Pollution | Notification.Problem1.Sewage | Notification.Problem1.Death | Notification.Problem1.Noise | Notification.Problem1.Flood | Notification.Problem1.Heating);
                    buildingData.m_flags &= ~Building.Flags.RateReduced;
                    CollapseIfFlooded(__instance, buildingID, ref buildingData, ref frameData);
                    multiProcessingFacilityAI.ProduceGoods1(buildingID, ref buildingData, ref frameData, productionRate1, finalProductionRate1, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
                }
                if (finalProductionRate2 != 0)
                {
                    int maxMail = workPlaceCount * 50 + visitPlaceCount * 5;
                    int num2 = HandleCommonConsumption(__instance, buildingID, ref buildingData, ref frameData, ref electricityConsumption, ref heatingConsumption, ref waterConsumption, ref sewageAccumulation, ref garbageAccumulation, ref mailAccumulation, maxMail, servicePolicies);
                    finalProductionRate2 = (finalProductionRate2 * num2 + 99) / 100;
                    if (finalProductionRate2 != 0)
                    {
                        if (num2 < 100)
                        {
                            buildingData.m_flags |= Building.Flags.RateReduced;
                        }
                        else
                        {
                            buildingData.m_flags &= ~Building.Flags.RateReduced;
                        }
                        multiProcessingFacilityAI.ProduceGoods2(buildingID, ref buildingData, ref frameData, productionRate2, finalProductionRate2, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
                    }
                    else
                    {
                        buildingData.m_flags &= ~Building.Flags.RateReduced;
                        multiProcessingFacilityAI.ProduceGoods2(buildingID, ref buildingData, ref frameData, productionRate2, finalProductionRate2, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
                    }
                    if (aliveWorkerCount != 0)
                    {
                        float radius = (float)(buildingData.Width + buildingData.Length) * 2.5f;
                        Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Density, aliveWorkerCount, buildingData.m_position, radius);
                    }
                }
                else
                {
                    electricityConsumption = 0;
                    heatingConsumption = 0;
                    waterConsumption = 0;
                    sewageAccumulation = 0;
                    garbageAccumulation = 0;
                    buildingData.m_problems = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem1.Electricity | Notification.Problem1.Water | Notification.Problem1.DirtyWater | Notification.Problem1.Pollution | Notification.Problem1.Sewage | Notification.Problem1.Death | Notification.Problem1.Noise | Notification.Problem1.Flood | Notification.Problem1.Heating);
                    buildingData.m_flags &= ~Building.Flags.RateReduced;
                    CollapseIfFlooded(__instance, buildingID, ref buildingData, ref frameData);
                    multiProcessingFacilityAI.ProduceGoods2(buildingID, ref buildingData, ref frameData, productionRate2, finalProductionRate2, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
                }
                if (finalProductionRate3 != 0)
                {
                    int maxMail = workPlaceCount * 50 + visitPlaceCount * 5;
                    int num2 = HandleCommonConsumption(__instance, buildingID, ref buildingData, ref frameData, ref electricityConsumption, ref heatingConsumption, ref waterConsumption, ref sewageAccumulation, ref garbageAccumulation, ref mailAccumulation, maxMail, servicePolicies);
                    finalProductionRate3 = (finalProductionRate3 * num2 + 99) / 100;
                    if (finalProductionRate3 != 0)
                    {
                        if (num2 < 100)
                        {
                            buildingData.m_flags |= Building.Flags.RateReduced;
                        }
                        else
                        {
                            buildingData.m_flags &= ~Building.Flags.RateReduced;
                        }
                        multiProcessingFacilityAI.ProduceGoods3(buildingID, ref buildingData, ref frameData, productionRate3, finalProductionRate3, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
                    }
                    else
                    {
                        buildingData.m_flags &= ~Building.Flags.RateReduced;
                        multiProcessingFacilityAI.ProduceGoods3(buildingID, ref buildingData, ref frameData, productionRate3, finalProductionRate3, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
                    }
                    if (aliveWorkerCount != 0)
                    {
                        float radius = (float)(buildingData.Width + buildingData.Length) * 2.5f;
                        Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Density, aliveWorkerCount, buildingData.m_position, radius);
                    }
                }
                else
                {
                    electricityConsumption = 0;
                    heatingConsumption = 0;
                    waterConsumption = 0;
                    sewageAccumulation = 0;
                    garbageAccumulation = 0;
                    buildingData.m_problems = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem1.Electricity | Notification.Problem1.Water | Notification.Problem1.DirtyWater | Notification.Problem1.Pollution | Notification.Problem1.Sewage | Notification.Problem1.Death | Notification.Problem1.Noise | Notification.Problem1.Flood | Notification.Problem1.Heating);
                    buildingData.m_flags &= ~Building.Flags.RateReduced;
                    CollapseIfFlooded(__instance, buildingID, ref buildingData, ref frameData);
                    multiProcessingFacilityAI.ProduceGoods3(buildingID, ref buildingData, ref frameData, productionRate3, finalProductionRate3, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
                }
                if (finalProductionRate4 != 0)
                {
                    int maxMail = workPlaceCount * 50 + visitPlaceCount * 5;
                    int num2 = HandleCommonConsumption(__instance, buildingID, ref buildingData, ref frameData, ref electricityConsumption, ref heatingConsumption, ref waterConsumption, ref sewageAccumulation, ref garbageAccumulation, ref mailAccumulation, maxMail, servicePolicies);
                    finalProductionRate4 = (finalProductionRate4 * num2 + 99) / 100;
                    if (finalProductionRate4 != 0)
                    {
                        if (num2 < 100)
                        {
                            buildingData.m_flags |= Building.Flags.RateReduced;
                        }
                        else
                        {
                            buildingData.m_flags &= ~Building.Flags.RateReduced;
                        }
                        multiProcessingFacilityAI.ProduceGoods4(buildingID, ref buildingData, ref frameData, productionRate4, finalProductionRate4, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
                    }
                    else
                    {
                        buildingData.m_flags &= ~Building.Flags.RateReduced;
                        multiProcessingFacilityAI.ProduceGoods4(buildingID, ref buildingData, ref frameData, productionRate4, finalProductionRate4, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
                    }
                    if (aliveWorkerCount != 0)
                    {
                        float radius = (float)(buildingData.Width + buildingData.Length) * 2.5f;
                        Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Density, aliveWorkerCount, buildingData.m_position, radius);
                    }
                }
                else
                {
                    electricityConsumption = 0;
                    heatingConsumption = 0;
                    waterConsumption = 0;
                    sewageAccumulation = 0;
                    garbageAccumulation = 0;
                    buildingData.m_problems = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem1.Electricity | Notification.Problem1.Water | Notification.Problem1.DirtyWater | Notification.Problem1.Pollution | Notification.Problem1.Sewage | Notification.Problem1.Death | Notification.Problem1.Noise | Notification.Problem1.Flood | Notification.Problem1.Heating);
                    buildingData.m_flags &= ~Building.Flags.RateReduced;
                    CollapseIfFlooded(__instance, buildingID, ref buildingData, ref frameData);
                    multiProcessingFacilityAI.ProduceGoods4(buildingID, ref buildingData, ref frameData, productionRate4, finalProductionRate4, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
                }
                if (buildingData.m_eventIndex != 0 && (__instance.m_supportEvents & EventManager.EventType.SecondaryLocation) == 0 && (buildingData.m_flags & Building.Flags.Active) == 0 && flag)
                {
                    EventManager instance2 = Singleton<EventManager>.instance;
                    instance2.m_events.m_buffer[buildingData.m_eventIndex].Info?.m_eventAI.BuildingDeactivated(buildingData.m_eventIndex, ref instance2.m_events.m_buffer[buildingData.m_eventIndex]);
                }
                int num3 = aliveWorkerCount + aliveVisitorCount;
                int num4 = behaviour.m_crimeAccumulation / 10;
                if (num3 > 255)
                {
                    num4 = (num4 * 255 + (num3 >> 1)) / num3;
                    num3 = 255;
                }
                buildingData.m_citizenCount = (byte)num3;
                if ((servicePolicies & DistrictPolicies.Services.RecreationalUse) != 0)
                {
                    num4 = num4 * 3 + 3 >> 2;
                }
                HandleCrime(__instance, buildingID, ref buildingData, num4, buildingData.m_citizenCount);
                int crimeBuffer = buildingData.m_crimeBuffer;
                crimeBuffer = ((buildingData.m_citizenCount != 0) ? ((crimeBuffer + (buildingData.m_citizenCount >> 1)) / buildingData.m_citizenCount) : 0);
                if (aliveWorkerCount != 0 || aliveVisitorCount != 0)
                {
                    int num5 = behaviour.m_educated0Count * 50 + behaviour.m_educated1Count * 25 + behaviour.m_educated2Count * 15;
                    num5 = num5 / (aliveWorkerCount + aliveVisitorCount) + 50;
                    num5 *= __instance.m_fireHazard;
                    buildingData.m_fireHazard = (byte)num5;
                }
                else
                {
                    buildingData.m_fireHazard = 0;
                }
                instance.m_districts.m_buffer[district].AddPlayerData(ref behaviour, crimeBuffer, workPlaceCount, aliveWorkerCount, Mathf.Max(0, workPlaceCount - totalWorkerCount), visitPlaceCount, aliveVisitorCount, Mathf.Max(0, visitPlaceCount - totalVisitorCount), electricityConsumption, heatingConsumption, waterConsumption, sewageAccumulation, garbageAccumulation, maintenanceCost, Mathf.Min(100, buildingData.m_garbageBuffer / 50), buildingData.m_waterPollution * 100 / 255);
                BaseSimulationStepActive(__instance, buildingID, ref buildingData, ref frameData);
                HandleFire(__instance, buildingID, ref buildingData, ref frameData, servicePolicies);
                if (buildingData.m_fireIntensity != 0)
                {
                    GuideController properties = Singleton<GuideManager>.instance.m_properties;
                    if (properties is not null)
                    {
                        Singleton<BuildingManager>.instance.m_buildingOnFire.Activate(properties.m_buildingOnFire, buildingID);
                    }
                }
                if(finalProductionRate1 == 0 && finalProductionRate2 == 0 && finalProductionRate3 == 0 && finalProductionRate4 == 0)
                {
                    buildingData.m_flags &= ~Building.Flags.Active;
                }
                return false;
            }
            return true;
        }


        [HarmonyPatch(typeof(PlayerBuildingAI), "SetProductionRate")]
        [HarmonyPostfix]
        public static void SetProductionRate(ushort buildingID, ref Building data, byte rate)
        {
            if (data.Info.GetAI() is MultiProcessingFacilityAI)
            {
                var custom_buffers = CustomBuffersManager.GetCustomBuffer(buildingID);
                custom_buffers.m_customBuffer10 = data.m_productionRate;
                custom_buffers.m_customBuffer11 = data.m_productionRate;
                custom_buffers.m_customBuffer12 = data.m_productionRate;
                custom_buffers.m_customBuffer13 = data.m_productionRate;
                CustomBuffersManager.SetCustomBuffer(buildingID, custom_buffers);
            }
        }


        private static void BaseSimulationStepActive(CommonBuildingAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            Notification.ProblemStruct problemStruct = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem1.Garbage);
            if (buildingData.m_garbageBuffer >= 2000)
            {
                int num = buildingData.m_garbageBuffer / 1000;
                if (Singleton<SimulationManager>.instance.m_randomizer.Int32(5u) == 0)
                {
                    num = UniqueFacultyAI.DecreaseByBonus(UniqueFacultyAI.FacultyBonus.Science, num);
                    Singleton<NaturalResourceManager>.instance.TryDumpResource(NaturalResourceManager.Resource.Pollution, num, num, buildingData.m_position, 0f);
                }
                int num2 = ((!(__instance is MainCampusBuildingAI) && !(__instance is ParkGateAI) && !(__instance is MainIndustryBuildingAI)) ? 3 : 4);
                if (num >= num2)
                {
                    if (Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.Garbage))
                    {
                        int num3 = ((!(__instance is MainCampusBuildingAI) && !(__instance is ParkGateAI) && !(__instance is MainIndustryBuildingAI)) ? 6 : 8);
                        problemStruct = ((num < num3) ? Notification.AddProblems(problemStruct, Notification.Problem1.Garbage) : Notification.AddProblems(problemStruct, Notification.Problem1.Garbage | Notification.Problem1.MajorProblem));
                        GuideController properties = Singleton<GuideManager>.instance.m_properties;
                        if ((object)properties != null)
                        {
                            int publicServiceIndex = ItemClass.GetPublicServiceIndex(ItemClass.Service.Garbage);
                            Singleton<GuideManager>.instance.m_serviceNeeded[publicServiceIndex].Activate(properties.m_serviceNeeded, ItemClass.Service.Garbage);
                        }
                    }
                    else
                    {
                        buildingData.m_garbageBuffer = 2000;
                    }
                }
            }
            buildingData.m_problems = problemStruct;
            float radius = (float)(buildingData.Width + buildingData.Length) * 2.5f;
            if (buildingData.m_crimeBuffer != 0)
            {
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.CrimeRate, buildingData.m_crimeBuffer, buildingData.m_position, radius);
            }
            if (GetFireParameters(__instance, buildingID, ref buildingData, out var fireHazard, out var fireSize, out var fireTolerance))
            {
                DistrictManager instance = Singleton<DistrictManager>.instance;
                byte district = instance.GetDistrict(buildingData.m_position);
                DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[district].m_servicePolicies;
                DistrictPolicies.CityPlanning cityPlanningPolicies = instance.m_districts.m_buffer[district].m_cityPlanningPolicies;
                if ((servicePolicies & DistrictPolicies.Services.SmokeDetectors) != 0)
                {
                    fireHazard = fireHazard * 75 / 100;
                }
                if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.LightningRods) != 0)
                {
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, 10, __instance.m_info.m_class);
                }
            }
            fireHazard = 100 - (10 + fireTolerance) * 50000 / ((100 + fireHazard) * (100 + fireSize));
            if (fireHazard > 0)
            {
                fireHazard = fireHazard * buildingData.Width * buildingData.Length;
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.FireHazard, fireHazard, buildingData.m_position, radius);
            }
            Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.FirewatchCoverage, 50, buildingData.m_position, 100f);
            if (Singleton<DisasterManager>.instance.IsEvacuating(buildingData.m_position))
            {
                if ((buildingData.m_flags & Building.Flags.Evacuating) == 0)
                {
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(ImmaterialResourceManager.Resource.RadioCoverage, buildingData.m_position, out var local);
                    if (Singleton<SimulationManager>.instance.m_randomizer.Int32(100u) < local + 10)
                    {
                        SetEvacuating(__instance, buildingID, ref buildingData, evacuating: true);
                    }
                }
            }
            else if ((buildingData.m_flags & Building.Flags.Evacuating) != 0)
            {
                SetEvacuating(__instance, buildingID, ref buildingData, evacuating: false);
            }
        }

    }
}
