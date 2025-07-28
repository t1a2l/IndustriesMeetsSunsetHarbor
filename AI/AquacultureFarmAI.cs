using ColossalFramework;
using UnityEngine;
using IndustriesMeetsSunsetHarbor.Managers;
using System;
using ColossalFramework.Math;
using ColossalFramework.DataBinding;
using MoreTransferReasons;
using MoreTransferReasons.AI;

namespace IndustriesMeetsSunsetHarbor.AI
{
    public class AquacultureFarmAI : PlayerBuildingAI, IExtendedBuildingAI
    {
        [CustomizableProperty("Uneducated Workers", "Workers", 0)]
        public int m_workPlaceCount0 = 5;

        [CustomizableProperty("Educated Workers", "Workers", 1)]
        public int m_workPlaceCount1 = 12;

        [CustomizableProperty("Well Educated Workers", "Workers", 2)]
        public int m_workPlaceCount2 = 9;

        [CustomizableProperty("Highly Educated Workers", "Workers", 3)]
        public int m_workPlaceCount3 = 4;

        [CustomizableProperty("Noise Accumulation", "Pollution")]
        public int m_noiseAccumulation = 50;

        [CustomizableProperty("Noise Radius", "Pollution")]
        public float m_noiseRadius = 100f;

        [CustomizableProperty("Production cycle duration")]
        public int m_productionCycleDuration = 10;

        [CustomizableProperty("Production Rate")]
        public int m_productionRate = 1000;

        [CustomizableProperty("Vehicle Count")]
        public int m_outputVehicleCount = 10;

        [CustomizableProperty("Storage Buffer Size")]
        public int m_storageBufferSize = 16000;

        public ItemClass m_vehicleClass;

        [CustomizableProperty("Output Resource")]
        public ExtendedTransferManager.TransferReason m_outputResource = ExtendedTransferManager.TransferReason.None;

        [NonSerialized]
        protected float m_quayOffset;

        public override VehicleInfo.VehicleCategory GetRequiredVehicleAccess(ushort buildingAI, ref Building data)
        {
            return base.GetRequiredVehicleAccess(buildingAI, ref data) | VehicleInfo.VehicleCategory.CargoTruck;
        }

        public override void InitializePrefab()
        {
            base.InitializePrefab();
            float num = m_info.m_generatedInfo.m_max.z - 7f;
            if (m_info.m_paths != null)
            {
                for (int i = 0; i < m_info.m_paths.Length; i++)
                {
                    if ((object)m_info.m_paths[i].m_netInfo != null && m_info.m_paths[i].m_netInfo.m_class.m_service == ItemClass.Service.Road && m_info.m_paths[i].m_netInfo.m_placementStyle == ItemClass.Placement.Manual && m_info.m_paths[i].m_nodes != null)
                    {
                        for (int j = 0; j < m_info.m_paths[i].m_nodes.Length; j++)
                        {
                            num = Mathf.Min(num, -16f - m_info.m_paths[i].m_netInfo.m_halfWidth - m_info.m_paths[i].m_nodes[j].z);
                        }
                    }
                }
            }
            m_quayOffset = num;
        }

        public override ImmaterialResourceManager.ResourceData[] GetImmaterialResourceRadius(ushort buildingID, ref Building data)
        {
            return
            [
                new ImmaterialResourceManager.ResourceData
                {
                    m_resource = ImmaterialResourceManager.Resource.NoisePollution,
                    m_radius = ((m_noiseAccumulation == 0) ? 0f : m_noiseRadius)
                }
            ];
        }

        public override Color GetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode)
        {
            switch (infoMode)
            {
                case InfoManager.InfoMode.NoisePollution:
                    {
                        int noiseAccumulation = m_noiseAccumulation;
                        return CommonBuildingAI.GetNoisePollutionColor(noiseAccumulation);
                    }
                case InfoManager.InfoMode.Fishing:
                    if ((data.m_flags & Building.Flags.Active) != 0)
                    {
                        return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_inactiveColor;
                case InfoManager.InfoMode.Connections:
                    if (subInfoMode == InfoManager.SubInfoMode.WaterPower)
                    {
                        ExtendedTransferManager.TransferReason outputResource = m_outputResource;
                        if (outputResource != ExtendedTransferManager.TransferReason.None && (data.m_tempExport != 0 || data.m_finalExport != 0))
                        {
                            return Singleton<ExtendedTransferManager>.instance.m_properties.m_resourceColors[(int)outputResource];
                        }
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                default:
                    return base.GetColor(buildingID, ref data, infoMode, subInfoMode);
            }
        }

        public override string GetDebugString(ushort buildingID, ref Building data)
        {
            string text = base.GetDebugString(buildingID, ref data);
            if (m_outputResource != ExtendedTransferManager.TransferReason.None)
            {
                int customBuffer = data.m_customBuffer1;
                int cycleBufferSize = GetCycleBufferSize(buildingID, ref data);
                int storageBufferSize = GetStorageBufferSize(buildingID, ref data);
                int customBuffer2 = data.m_customBuffer2;
                text = StringUtils.SafeFormat("{0}\n{1}:\ngrowth cycle: {2} / {3}\noutput: {4} / {5}", text, m_outputResource.ToString(), customBuffer, cycleBufferSize, customBuffer2, storageBufferSize);
            }
            int count = 0;
            int cargo = 0;
            int capacity = 0;
            int outside = 0;
            ExtendedVehicleManager.CalculateOwnVehicles(buildingID, ref data, m_outputResource, ref count, ref cargo, ref capacity, ref outside);
            return StringUtils.SafeFormat("{0}\nOutgoing trucks: {1}", text, count);
        }

        public override int GetResourceRate(ushort buildingID, ref Building data, ImmaterialResourceManager.Resource resource)
        {
            if (resource == ImmaterialResourceManager.Resource.NoisePollution)
            {
                return m_noiseAccumulation;
            }
            return base.GetResourceRate(buildingID, ref data, resource);
        }

        public override void GetPlacementInfoMode(out InfoManager.InfoMode mode, out InfoManager.SubInfoMode subMode, float elevation)
        {
            mode = InfoManager.InfoMode.Fishing;
            subMode = InfoManager.SubInfoMode.WaterPower;
        }

        protected override string GetLocalizedStatusActive(ushort buildingID, ref Building data)
        {
            if ((data.m_flags & Building.Flags.RateReduced) != 0)
            {
                return ColossalFramework.Globalization.Locale.Get("BUILDING_STATUS_REDUCED");
            }
            return ColossalFramework.Globalization.Locale.Get("BUILDING_STATUS_DEFAULT");
        }

        public override string GetLocalizedStats(ushort buildingID, ref Building data)
        {
            int cycleBufferSize = GetCycleBufferSize(buildingID, ref data);
            int customBuffer = data.m_customBuffer1;
            float num = Mathf.Clamp(5 * customBuffer / cycleBufferSize + 1, 1, 5);
            string text = LocaleFormatter.FormatGeneric("INFO_FISH_FARM_STATS", num, 5) + Environment.NewLine;

            if (m_outputResource != ExtendedTransferManager.TransferReason.None && m_outputVehicleCount != 0)
            {
                int budget = GetBudget(buildingID, ref data);
                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                int num2 = (productionRate * m_outputVehicleCount + 99) / 100;
                int count = 0;
                int cargo = 0;
                int capacity = 0;
                int outside = 0;
                ExtendedVehicleManager.CalculateOwnVehicles(buildingID, ref data, m_outputResource, ref count, ref cargo, ref capacity, ref outside);
                text += LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLES", count, num2);
                if (AquacultureFarmManager.AquacultureFarms.ContainsKey(buildingID))
                {
                    int extractors_count = AquacultureFarmManager.AquacultureFarms[buildingID].Count;
                    text += Environment.NewLine;
                    text += string.Format("Number of Extractors used: {0}", extractors_count);
                }
            }
            return text;
        }

        public override void CreateBuilding(ushort buildingID, ref Building data)
        {
            base.CreateBuilding(buildingID, ref data);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, 0, workCount, 0, 0, 0);
        }

        public override void ReleaseBuilding(ushort buildingID, ref Building data)
        {
            base.ReleaseBuilding(buildingID, ref data);
            if (Singleton<UnlockManager>.instance.m_properties != null &&  !Singleton<UnlockManager>.instance.m_properties.m_ServicePolicyMilestones[28].IsPassed() && m_info.m_class.m_service == ItemClass.Service.Fishing && m_info.m_class.m_subService == ItemClass.SubService.None && m_info.m_class.m_level == ItemClass.Level.Level3)
            {
                DistrictManager instance = Singleton<DistrictManager>.instance;
                for (int i = 0; i < instance.m_districts.m_size; i++)
                {
                    instance.m_districts.m_buffer[i].m_servicePolicies &= ~DistrictPolicies.Services.AlgaeBasedWaterFiltering;
                }
                instance.NamesModified();
            }
        }

        public override void BuildingLoaded(ushort buildingID, ref Building data, uint version)
        {
            base.BuildingLoaded(buildingID, ref data, version);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            EnsureCitizenUnits(buildingID, ref data, 0, workCount, 0, 0);
        }

        public override void EndRelocating(ushort buildingID, ref Building data)
        {
            base.EndRelocating(buildingID, ref data);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            EnsureCitizenUnits(buildingID, ref data, 0, workCount, 0, 0);
        }

        protected override void ManualActivation(ushort buildingID, ref Building buildingData)
        {
            if (m_noiseAccumulation != 0)
            {
                Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Sad, ImmaterialResourceManager.Resource.NoisePollution, m_noiseAccumulation, m_noiseRadius);
            }
        }

        protected override void ManualDeactivation(ushort buildingID, ref Building buildingData)
        {
            if ((buildingData.m_flags & Building.Flags.Collapsed) != 0)
            {
                Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Happy, ImmaterialResourceManager.Resource.Abandonment, -buildingData.Width * buildingData.Length, 64f);
            }
            else if (m_noiseAccumulation != 0)
            {
                Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Happy, ImmaterialResourceManager.Resource.NoisePollution, -m_noiseAccumulation, m_noiseRadius);
            }
        }

        public void ExtendedStartTransfer(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, ExtendedTransferManager.Offer offer)
        {
            if (material == m_outputResource)
            {
                VehicleInfo vehicleInfo = GetSelectedVehicle(buildingID);
                if (vehicleInfo == null)
                {
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, m_vehicleClass.m_service, m_vehicleClass.m_subService, m_vehicleClass.m_level, VehicleInfo.VehicleType.Car);
                }
                if (vehicleInfo == null)
                {
                    return;
                }

                byte transferType = (byte)(material + 200);
                Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
                if (ExtendedVehicleManager.CreateVehicle(out var vehicle, ref Singleton<SimulationManager>.instance.m_randomizer, vehicleInfo, data.m_position, transferType, transferToSource: false, transferToTarget: true) && vehicleInfo.m_vehicleAI is ExtendedCargoTruckAI cargoTruckAI)
                {
                    vehicleInfo.m_vehicleAI.SetSource(vehicle, ref vehicles.m_buffer[vehicle], buildingID);
                    ((IExtendedVehicleAI)cargoTruckAI).ExtendedStartTransfer(vehicle, ref vehicles.m_buffer[(int)vehicle], material, offer);
                    ushort building = offer.Building;
                    if (building != 0 && (Singleton<BuildingManager>.instance.m_buildings.m_buffer[building].m_flags & Building.Flags.IncomingOutgoing) != 0)
                    {
                        vehicleInfo.m_vehicleAI.GetSize(vehicle, ref vehicles.m_buffer[vehicle], out var size, out var _);
                        IndustryBuildingManager.ExportResource(buildingID, ref data, material, size);
                    }
                    data.m_outgoingProblemTimer = 0;
                }
            }
        }

        public void ExtendedModifyMaterialBuffer(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, ref int amountDelta)
        {
            if (material == m_outputResource)
            {
                int num = data.m_customBuffer2 * 100;
                amountDelta = Mathf.Clamp(amountDelta, -num, m_storageBufferSize - num);
                data.m_customBuffer2 = (ushort)((num + amountDelta) / 100);
            }
        }

        public void ExtendedGetMaterialAmount(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, out int amount, out int max)
        {
            amount = data.m_customBuffer2 * 100;
            max = m_storageBufferSize;
        }

        public override void BuildingDeactivated(ushort buildingID, ref Building data)
        {
            if (m_outputResource != ExtendedTransferManager.TransferReason.None)
            {
                ExtendedTransferManager.Offer offer = default;
                offer.Building = buildingID;
                Singleton<ExtendedTransferManager>.instance.RemoveOutgoingOffer(m_outputResource, offer);
            }
            base.BuildingDeactivated(buildingID, ref data);
        }

        public override void SimulationStep(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            base.SimulationStep(buildingID, ref buildingData, ref frameData);
            if ((Singleton<SimulationManager>.instance.m_currentFrameIndex & 0xFFF) >= 3840)
            {
                buildingData.m_finalExport = buildingData.m_tempExport;
                buildingData.m_tempExport = 0;
            }
        }

        protected override bool CanEvacuate()
        {
            return m_workPlaceCount0 != 0 || m_workPlaceCount1 != 0 || m_workPlaceCount2 != 0 || m_workPlaceCount3 != 0;
        }

        protected override void HandleWorkAndVisitPlaces(ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveWorkerCount, ref int totalWorkerCount, ref int workPlaceCount, ref int aliveVisitorCount, ref int totalVisitorCount, ref int visitPlaceCount)
        {
            workPlaceCount += m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            GetWorkBehaviour(buildingID, ref buildingData, ref behaviour, ref aliveWorkerCount, ref totalWorkerCount);
            HandleWorkPlaces(buildingID, ref buildingData, m_workPlaceCount0, m_workPlaceCount1, m_workPlaceCount2, m_workPlaceCount3, ref behaviour, aliveWorkerCount, totalWorkerCount);
        }

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
                    //Vector3 position = buildingData.CalculatePosition(building.m_position);
                    Singleton<TerrainManager>.instance.CountWaterCoverage(building.m_position, 20f, out var water, out _, out var pollution);
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
                if (m_outputResource != ExtendedTransferManager.TransferReason.None)
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
                if (m_outputResource != ExtendedTransferManager.TransferReason.None)
                {
                    int num11 = (num4 * m_outputVehicleCount + 99) / 100;
                    int count = 0;
                    int cargo = 0;
                    int capacity = 0;
                    int outside = 0;
                    ExtendedVehicleManager.CalculateOwnVehicles(buildingID, ref buildingData, m_outputResource, ref count, ref cargo, ref capacity, ref outside);
                    buildingData.m_tempExport = (byte)Mathf.Clamp(outside, buildingData.m_tempExport, 255);
                    if (buildingData.m_finalExport != 0)
                    {
                        instance.m_districts.m_buffer[district].m_playerConsumption.m_finalExportAmount += buildingData.m_finalExport;
                    }
                    int num12 = num9;
                    if (num12 >= 8000 && count < num11)
                    {
                        ExtendedTransferManager.Offer offer = default;
                        offer.Building = buildingID;
                        offer.Position = buildingData.m_position;
                        offer.Amount = 1;
                        offer.Active = true;
                        Singleton<ExtendedTransferManager>.instance.AddOutgoingOffer(m_outputResource, offer);
                    }
                }
            }
            buildingData.m_problems = problemStruct;
            buildingData.m_education3 = (byte)Mathf.Clamp(finalProductionRate * m_productionRate / Mathf.Max(1, m_productionRate), 0, 255);
            base.ProduceGoods(buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
        }

        public override ToolBase.ToolErrors CheckBuildPosition(ushort relocateID, ref Vector3 position, ref float angle, float waterHeight, float elevation, ref Segment3 connectionSegment, out int productionRate, out int constructionCost)
        {
            ToolBase.ToolErrors toolErrors = ToolBase.ToolErrors.None;
            if (m_info.m_placementMode == BuildingInfo.PlacementMode.Shoreline && BuildingTool.SnapToCanal(position, out var pos, out var dir, out var isQuay, 40f, center: false))
            {
                angle = Mathf.Atan2(dir.x, 0f - dir.z);
                pos += dir * m_quayOffset;
                position.x = pos.x;
                position.z = pos.z;
                if (!isQuay)
                {
                    toolErrors |= ToolBase.ToolErrors.ShoreNotFound;
                }
            }
            return toolErrors | base.CheckBuildPosition(relocateID, ref position, ref angle, waterHeight, elevation, ref connectionSegment, out productionRate, out constructionCost);
        }

        public override bool GetWaterStructureCollisionRange(out float min, out float max)
        {
            if (m_info.m_placementMode == BuildingInfo.PlacementMode.Shoreline)
            {
                min = (m_info.m_generatedInfo.m_max.z - m_quayOffset + 5f) / Mathf.Max(14f, (float)m_info.m_cellLength * 8f);
                max = 1f;
                return true;
            }
            return base.GetWaterStructureCollisionRange(out min, out max);
        }

        public override string GetLocalizedTooltip()
        {
            string text = LocaleFormatter.FormatGeneric("AIINFO_WATER_CONSUMPTION", GetWaterConsumption() * 16) + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_ELECTRICITY_CONSUMPTION", GetElectricityConsumption() * 16);
            string text2 = LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_PRODUCTION_RATE", m_productionRate * 16);
            if (m_outputResource != ExtendedTransferManager.TransferReason.None && m_outputVehicleCount != 0)
            {
                text2 = text2 + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLE_COUNT", m_outputVehicleCount);
            }
            string text3 = LocaleFormatter.FormatGeneric("AIINFO_WORKPLACES_ACCUMULATION", (m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3).ToString());
            string baseTooltip = TooltipHelper.Append(base.GetLocalizedTooltip(), TooltipHelper.Format(LocaleFormatter.Info1, text, LocaleFormatter.Info2, text2, LocaleFormatter.WorkplaceCount, text3));
            string addTooltip = TooltipHelper.Format("arrowVisible", "false", "input1Visible", "true", "input2Visible", "false", "input3Visible", "false", "input4Visible", "false", "outputVisible", "false");
            string addTooltip2 = TooltipHelper.Format("input1", IndustryBuildingManager.ResourceSpriteName(m_outputResource), "input2", string.Empty, "input3", string.Empty, "input4", string.Empty, "output", string.Empty);
            baseTooltip = TooltipHelper.Append(baseTooltip, addTooltip);
            return TooltipHelper.Append(baseTooltip, addTooltip2);
        }

        public override void GetPollutionAccumulation(out int ground, out int noise)
        {
            ground = 0;
            noise = m_noiseAccumulation;
        }

        public override bool RequireRoadAccess()
        {
            return base.RequireRoadAccess() || m_workPlaceCount0 != 0 || m_workPlaceCount1 != 0 || m_workPlaceCount2 != 0 || m_workPlaceCount3 != 0;
        }

        public int GetCycleBufferSize(ushort buildingID, ref Building data)
        {
            int value = m_productionRate * m_productionCycleDuration;
            return Mathf.Clamp(value, 8000, 64000);
        }

        public int GetStorageBufferSize(ushort buildingID, ref Building data)
        {
            return m_storageBufferSize;
        }

        public override void CountWorkPlaces(out int workPlaceCount0, out int workPlaceCount1, out int workPlaceCount2, out int workPlaceCount3)
        {
            workPlaceCount0 = m_workPlaceCount0;
            workPlaceCount1 = m_workPlaceCount1;
            workPlaceCount2 = m_workPlaceCount2;
            workPlaceCount3 = m_workPlaceCount3;
        }
    }
}
