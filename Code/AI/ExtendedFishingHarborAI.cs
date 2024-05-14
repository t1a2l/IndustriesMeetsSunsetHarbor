using ColossalFramework.DataBinding;
using ColossalFramework.Math;
using ColossalFramework.Threading;
using ColossalFramework;
using System;
using System.Collections.Generic;
using UnityEngine;
using MoreTransferReasons;
using ColossalFramework.Globalization;
using MoreTransferReasons.AI;
using ColossalFramework.UI;

namespace IndustriesMeetsSunsetHarbor.AI
{
    public class ExtendedFishingHarborAI : PlayerBuildingAI, IExtendedBuildingAI
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

        public ItemClass m_boatClass;

        [CustomizableProperty("Boat Count")]
        public int m_boatCount = 10;

        [CustomizableProperty("Boat Spawn Interval")]
        public uint m_boatSpawnInterval = 8u;

        public Vector3 m_boatSpawnPosition;

        public Vector3 m_boatSpawnTarget;

        public Vector3 m_boatUnspawnPosition;

        public Vector3 m_boatUnspawnTarget;

        public ItemClass m_outputVehicleClass;

        [CustomizableProperty("Output Vehicle Count")]
        public int m_outputVehicleCount = 10;

        public ExtendedTransferManager.TransferReason m_outputResource = ExtendedTransferManager.TransferReason.None;

        [CustomizableProperty("Storage Buffer Size")]
        public int m_storageBufferSize = 16000;

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
                    if ((object)m_info.m_paths[i].m_netInfo != null && m_info.m_paths[i].m_netInfo.m_class.m_service == ItemClass.Service.Road && m_info.m_paths[i].m_nodes != null)
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

        public override void GetPlacementInfoMode(out InfoManager.InfoMode mode, out InfoManager.SubInfoMode subMode, float elevation)
        {
            mode = InfoManager.InfoMode.Fishing;
            subMode = InfoManager.SubInfoMode.Default;
        }

        protected override string GetLocalizedStatusActive(ushort buildingID, ref Building data)
        {
            if ((data.m_flags & Building.Flags.RateReduced) != 0)
            {
                return Locale.Get("BUILDING_STATUS_REDUCED");
            }
            return Locale.Get("BUILDING_STATUS_DEFAULT");
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

        public override void CreateBuilding(ushort buildingID, ref Building data)
        {
            base.CreateBuilding(buildingID, ref data);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, 0, workCount);
        }

        public override void BuildingLoaded(ushort buildingID, ref Building data, uint version)
        {
            base.BuildingLoaded(buildingID, ref data, version);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            EnsureCitizenUnits(buildingID, ref data, 0, workCount);
        }

        public override void EndRelocating(ushort buildingID, ref Building data)
        {
            base.EndRelocating(buildingID, ref data);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            EnsureCitizenUnits(buildingID, ref data, 0, workCount);
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

        void IExtendedBuildingAI.ExtendedStartTransfer(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, ExtendedTransferManager.Offer offer)
        {
            if (material == m_outputResource)
            {
                VehicleInfo vehicleInfo = GetSelectedVehicle(buildingID);
                if (vehicleInfo == null)
                {
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, m_outputVehicleClass.m_service, m_outputVehicleClass.m_subService, m_outputVehicleClass.m_level, VehicleInfo.VehicleType.Car);
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

        void IExtendedBuildingAI.ExtendedModifyMaterialBuffer(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, ref int amountDelta)
        {
            if (material == m_outputResource)
            {
                int num = data.m_customBuffer2 * 100;
                amountDelta = Mathf.Clamp(amountDelta, -num, m_storageBufferSize - num);
                num += amountDelta;
                data.m_customBuffer2 = (ushort)(num / 100);
                if (amountDelta > 0)
                {
                    StatisticsManager instance = Singleton<StatisticsManager>.instance;
                    StatisticBase statisticBase = instance.Acquire<StatisticInt64>(StatisticType.FishCaught);
                    statisticBase.Add(amountDelta);
                }
            }
        }

        void IExtendedBuildingAI.ExtendedGetMaterialAmount(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, out int amount, out int max)
        {
            amount = data.m_customBuffer2 * 100;
            max = m_storageBufferSize;
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
            toolErrors |= base.CheckBuildPosition(relocateID, ref position, ref angle, waterHeight, elevation, ref connectionSegment, out productionRate, out constructionCost);
            if (position.y - waterHeight > 32f)
            {
                toolErrors |= ToolBase.ToolErrors.HeightTooHigh;
            }
            return toolErrors;
        }

        public override bool GetWaterStructureCollisionRange(out float min, out float max)
        {
            if (m_info.m_placementMode == BuildingInfo.PlacementMode.Shoreline)
            {
                min = 20f / Mathf.Max(22f, (float)m_info.m_cellLength * 8f);
                max = 1f;
                return true;
            }
            return base.GetWaterStructureCollisionRange(out min, out max);
        }

        public override void PlacementFailed(ToolBase.ToolErrors errors)
        {
            if ((errors & ToolBase.ToolErrors.ShoreNotFound) != 0)
            {
                GuideController properties = Singleton<GuideManager>.instance.m_properties;
                if ((object)properties != null)
                {
                    Singleton<BuildingManager>.instance.m_buildNextToWater.Activate(properties.m_buildNextToWater);
                }
            }
        }

        public override void PlacementSucceeded()
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            if (Singleton<BuildingManager>.instance.m_buildNextToWater != null)
            {
                instance.m_buildNextToWater.Deactivate();
            }
            if (Singleton<BuildingManager>.instance.m_fishingHarborNotUsed != null)
            {
                instance.m_fishingHarborNotUsed.Disable();
            }
        }

        protected override bool CanEvacuate()
        {
            return m_workPlaceCount0 != 0 || m_workPlaceCount1 != 0 || m_workPlaceCount2 != 0 || m_workPlaceCount3 != 0;
        }

        public override void SimulationStep(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            base.SimulationStep(buildingID, ref buildingData, ref frameData);
            CheckVehicles(buildingID, ref buildingData);
            GuideController properties = Singleton<GuideManager>.instance.m_properties;
            if ((object)properties != null)
            {
                if (Singleton<BuildingManager>.instance.m_fishingHarborBuilt != null)
                {
                    Singleton<BuildingManager>.instance.m_fishingHarborBuilt.Activate(properties.m_fishingHarborBuilt, m_info);
                }
                if (Singleton<BuildingManager>.instance.m_fishTypes != null)
                {
                    Singleton<BuildingManager>.instance.m_fishTypes.Activate(properties.m_fishTypes, m_info);
                }
            }
        }

        private static void CheckVehicles(ushort buildingID, ref Building data)
        {
            bool flag = (data.m_flags & (Building.Flags.Evacuating | Building.Flags.Active)) == Building.Flags.Active;
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort num = data.m_ownVehicles;
            int num2 = 0;
            while (num != 0)
            {
                Vehicle.Flags flags = instance.m_vehicles.m_buffer[num].m_flags;
                if ((flags & Vehicle.Flags.GoingBack) != 0)
                {
                    if (flag)
                    {
                        VehicleInfo info = instance.m_vehicles.m_buffer[num].Info;
                        info.m_vehicleAI.SetTarget(num, ref instance.m_vehicles.m_buffer[num], buildingID);
                    }
                }
                else if (!flag)
                {
                    VehicleInfo info2 = instance.m_vehicles.m_buffer[num].Info;
                    info2.m_vehicleAI.SetTarget(num, ref instance.m_vehicles.m_buffer[num], 0);
                }
                num = instance.m_vehicles.m_buffer[num].m_nextOwnVehicle;
                if (++num2 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }

        protected override int HandleCommonConsumption(ushort buildingID, ref Building data, ref Building.Frame frameData, ref int electricityConsumption, ref int heatingConsumption, ref int waterConsumption, ref int sewageAccumulation, ref int garbageAccumulation, ref int mailAccumulation, int maxMail, DistrictPolicies.Services policies)
        {
            int result = base.HandleCommonConsumption(buildingID, ref data, ref frameData, ref electricityConsumption, ref heatingConsumption, ref waterConsumption, ref sewageAccumulation, ref garbageAccumulation, ref mailAccumulation, maxMail, policies);
            Notification.ProblemStruct problems = Notification.RemoveProblems(data.m_problems, Notification.Problem1.FishingRouteIncomplete | Notification.Problem1.FishingRouteWaterDirty);
            int num = 0;
            bool flag = false;
            int num2 = 0;
            Vector3 position = data.CalculatePosition(m_boatSpawnPosition);
            Vector3 position2 = data.CalculatePosition(m_boatUnspawnPosition);
            if (PathManager.FindPathPosition(position, m_boatClass.m_service, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Ship, VehicleInfo.VehicleCategory.All, allowUnderground: false, requireConnect: false, 40f, excludeLaneWidth: false, checkPedestrianStreet: false, out var pathPos) && PathManager.FindPathPosition(position2, m_boatClass.m_service, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Ship, VehicleInfo.VehicleCategory.All, allowUnderground: false, requireConnect: false, 40f, excludeLaneWidth: false, checkPedestrianStreet: false, out var pathPos2))
            {
                ushort segment = pathPos.m_segment;
                ushort segment2 = pathPos2.m_segment;
                TerrainManager instance = Singleton<TerrainManager>.instance;
                NetManager instance2 = Singleton<NetManager>.instance;
                ushort num3 = segment;
                ushort previousSegment = 0;
                while (num3 != 0 && num3 != segment2 && num2 < 50000)
                {
                    ushort startNode = instance2.m_segments.m_buffer[num3].m_startNode;
                    instance.CountWaterCoverage(instance2.m_nodes.m_buffer[startNode].m_position, 0f, out var water, out var shore, out var pollution);
                    num += Mathf.Clamp(pollution, 0, 128);
                    startNode = instance2.m_segments.m_buffer[num3].m_endNode;
                    instance.CountWaterCoverage(instance2.m_nodes.m_buffer[startNode].m_position, 0f, out water, out shore, out pollution);
                    num += Mathf.Clamp(pollution, 0, 128);
                    ushort nextConnectedSegment = GetNextConnectedSegment(num3, previousSegment);
                    previousSegment = num3;
                    num3 = nextConnectedSegment;
                    num2++;
                }
                flag = num3 == segment2 && num2 > 1;
            }
            if (flag)
            {
                int num4 = num / (num2 * 2);
                if (num4 > 96)
                {
                    problems = Notification.Problem1.FishingRouteWaterDirty | Notification.Problem1.FatalProblem;
                }
                else if (num4 > 64)
                {
                    problems = Notification.AddProblems(data.m_problems, Notification.Problem1.FishingRouteWaterDirty | Notification.Problem1.MajorProblem);
                }
                else if (num4 > 32)
                {
                    problems = Notification.AddProblems(data.m_problems, Notification.Problem1.FishingRouteWaterDirty);
                }
            }
            else
            {
                problems = Notification.AddProblems(data.m_problems, Notification.Problem1.FishingRouteIncomplete);
                result = 0;
            }
            data.m_problems = problems;
            return result;
        }

        private ushort GetNextConnectedSegment(ushort currentSegment, ushort previousSegment)
        {
            NetManager instance = Singleton<NetManager>.instance;
            ushort startNode = instance.m_segments.m_buffer[currentSegment].m_startNode;
            ushort endNode = instance.m_segments.m_buffer[currentSegment].m_endNode;
            for (int i = 0; i < 8; i++)
            {
                ushort segment = instance.m_nodes.m_buffer[startNode].GetSegment(i);
                if (segment != 0 && segment != previousSegment && segment != currentSegment)
                {
                    return segment;
                }
                segment = instance.m_nodes.m_buffer[endNode].GetSegment(i);
                if (segment != 0 && segment != previousSegment && segment != currentSegment)
                {
                    return segment;
                }
            }
            return 0;
        }

        protected override void HandleWorkAndVisitPlaces(ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveWorkerCount, ref int totalWorkerCount, ref int workPlaceCount, ref int aliveVisitorCount, ref int totalVisitorCount, ref int visitPlaceCount)
        {
            workPlaceCount += m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            GetWorkBehaviour(buildingID, ref buildingData, ref behaviour, ref aliveWorkerCount, ref totalWorkerCount);
            HandleWorkPlaces(buildingID, ref buildingData, m_workPlaceCount0, m_workPlaceCount1, m_workPlaceCount2, m_workPlaceCount3, ref behaviour, aliveWorkerCount, totalWorkerCount);
        }

        protected override void ProduceGoods(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            base.ProduceGoods(buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
            Notification.ProblemStruct problemStruct = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem1.NoPlaceForFishingGoods);
            if (finalProductionRate != 0)
            {
                int num = finalProductionRate * m_noiseAccumulation / 100;
                if (num != 0)
                {
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, num, buildingData.m_position, m_noiseRadius);
                }
                HandleDead(buildingID, ref buildingData, ref behaviour, totalWorkerCount);
                int num2 = (finalProductionRate * m_boatCount + 99) / 100;
                int num3 = CalculateOwnBoats(buildingID, ref buildingData);
                if (num3 < num2 && buildingData.m_customBuffer2 < m_storageBufferSize / 100)
                {
                    uint num4 = (Singleton<SimulationManager>.instance.m_currentFrameIndex >> 8) % m_boatSpawnInterval;
                    uint num5 = buildingID % m_boatSpawnInterval;
                    if (num4 == num5)
                    {
                        TrySpawnBoat(buildingID, ref buildingData);
                    }
                }
                DistrictManager instance = Singleton<DistrictManager>.instance;
                byte district = instance.GetDistrict(buildingData.m_position);
                if (m_outputResource != ExtendedTransferManager.TransferReason.None)
                {
                    if (m_outputVehicleCount > 0 && buildingData.m_customBuffer2 >= m_storageBufferSize / 100)
                    {
                        problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.NoPlaceForFishingGoods);
                    }
                    int num6 = (finalProductionRate * m_outputVehicleCount + 99) / 100;
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
                    int customBuffer = buildingData.m_customBuffer2;
                    if (customBuffer >= 80 && count < num6)
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
        }

        protected int CalculateOwnBoats(ushort buildingID, ref Building data)
        {
            int num = 0;
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort num2 = data.m_ownVehicles;
            int num3 = 0;
            while (num2 != 0)
            {
                if (instance.m_vehicles.m_buffer[num2].m_transferType == byte.MaxValue)
                {
                    ItemClass @class = instance.m_vehicles.m_buffer[num2].Info.m_class;
                    if (@class.m_service == m_boatClass.m_service && @class.m_subService == m_boatClass.m_subService && @class.m_level == m_boatClass.m_level)
                    {
                        num++;
                    }
                }
                num2 = instance.m_vehicles.m_buffer[num2].m_nextOwnVehicle;
                if (++num3 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return num;
        }

        public void TrySpawnBoat(ushort buildingID, ref Building buildingData)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            Vector3 position = buildingData.CalculatePosition(m_boatSpawnPosition);
            VehicleInfo vehicleInfo = GetAdditionalSelectedVehicle(buildingID);
            if ((object)vehicleInfo == null)
            {
                vehicleInfo = instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, m_boatClass.m_service, m_boatClass.m_subService, m_boatClass.m_level, VehicleInfo.VehicleType.Ship);
            }
            if ((object)vehicleInfo != null && instance.CreateVehicle(out var vehicle, ref Singleton<SimulationManager>.instance.m_randomizer, vehicleInfo, position, TransferManager.TransferReason.None, transferToSource: false, transferToTarget: false))
            {
                vehicleInfo.m_vehicleAI.SetSource(vehicle, ref instance.m_vehicles.m_buffer[vehicle], buildingID);
                vehicleInfo.m_vehicleAI.SetTarget(vehicle, ref instance.m_vehicles.m_buffer[vehicle], buildingID);
            }
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

        public override void CalculateSpawnPosition(ushort buildingID, ref Building data, ref Randomizer randomizer, VehicleInfo info, out Vector3 position, out Vector3 target)
        {
            if (info.m_vehicleType == VehicleInfo.VehicleType.Ship && info.m_class.m_service == m_boatClass.m_service && info.m_class.m_subService == m_boatClass.m_subService)
            {
                position = data.CalculatePosition(m_boatSpawnPosition);
                target = data.CalculatePosition(m_boatSpawnTarget);
            }
            else
            {
                base.CalculateSpawnPosition(buildingID, ref data, ref randomizer, info, out position, out target);
            }
        }

        public override void CalculateUnspawnPosition(ushort buildingID, ref Building data, ref Randomizer randomizer, VehicleInfo info, out Vector3 position, out Vector3 target)
        {
            if (info.m_vehicleType == VehicleInfo.VehicleType.Ship && info.m_class.m_service == m_boatClass.m_service && info.m_class.m_subService == m_boatClass.m_subService)
            {
                position = data.CalculatePosition(m_boatUnspawnPosition);
                target = data.CalculatePosition(m_boatUnspawnTarget);
            }
            else
            {
                base.CalculateUnspawnPosition(buildingID, ref data, ref randomizer, info, out position, out target);
            }
        }

        protected override int AdjustMaintenanceCost(ushort buildingID, ref Building data, int maintenanceCost)
        {
            int num = base.AdjustMaintenanceCost(buildingID, ref data, maintenanceCost);
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(data.m_position);
            DistrictPolicies.CityPlanning cityPlanningPolicies = instance.m_districts.m_buffer[district].m_cityPlanningPolicies;
            if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.SustainableFishing) != 0)
            {
                instance.m_districts.m_buffer[district].m_cityPlanningPoliciesEffect |= DistrictPolicies.CityPlanning.SustainableFishing;
                num += (num * 15 + 99) / 100;
            }
            return num;
        }

        public override void UpdateGuide(GuideController guideController)
        {
            Singleton<BuildingManager>.instance.m_fishingHarborNotUsed?.Activate(guideController.m_fishingHarborNotUsed, m_info);
            base.UpdateGuide(guideController);
        }

        public override bool EnableNotUsedGuide()
        {
            return false;
        }

        public void DeleteFishingRoute(ushort buildingID, ref Building data, BulldozeTool bulldozeTool)
        {
            Vector3 position = data.CalculatePosition(m_boatSpawnPosition);
            Vector3 position2 = data.CalculatePosition(m_boatUnspawnPosition);
            List<ushort> list = new();
            if (PathManager.FindPathPosition(position, m_boatClass.m_service, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Ship, VehicleInfo.VehicleCategory.All, allowUnderground: false, requireConnect: false, 40f, excludeLaneWidth: false, checkPedestrianStreet: false, out var pathPos) && PathManager.FindPathPosition(position2, m_boatClass.m_service, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Ship, VehicleInfo.VehicleCategory.All, allowUnderground: false, requireConnect: false, 40f, excludeLaneWidth: false, checkPedestrianStreet: false, out var pathPos2))
            {
                ushort segment = pathPos.m_segment;
                ushort segment2 = pathPos2.m_segment;
                TerrainManager instance = Singleton<TerrainManager>.instance;
                NetManager instance2 = Singleton<NetManager>.instance;
                int num = 0;
                ushort num2 = segment;
                ushort previousSegment = 0;
                while (num2 != 0 && num2 != segment2 && num < 50000)
                {
                    ushort nextConnectedSegment = GetNextConnectedSegment(num2, previousSegment);
                    previousSegment = num2;
                    num2 = nextConnectedSegment;
                    if (!list.Contains(num2) && num2 != segment && num2 != segment2)
                    {
                        list.Add(num2);
                    }
                    num++;
                }
                num2 = segment2;
                previousSegment = 0;
                while (num2 != 0 && num2 != segment && num < 50000)
                {
                    ushort nextConnectedSegment2 = GetNextConnectedSegment(num2, previousSegment);
                    previousSegment = num2;
                    num2 = nextConnectedSegment2;
                    if (!list.Contains(num2) && num2 != segment && num2 != segment2)
                    {
                        list.Add(num2);
                    }
                    num++;
                }
            }
            foreach (ushort item in list)
            {
                if (item != 0)
                {
                    Singleton<SimulationManager>.instance.AddAction(bulldozeTool.DeleteSegment(item, 0));
                }
            }
            ThreadHelper.dispatcher.Dispatch(delegate
            {
                CityServiceWorldInfoPanel cityServiceWorldInfoPanel = UIView.library.Get<CityServiceWorldInfoPanel>("CityServiceWorldInfoPanel");
                if (cityServiceWorldInfoPanel != null)
                {
                    cityServiceWorldInfoPanel.DisableDeleteFishingRouteButton();
                }
            });
        }

        public bool HasFishingRouteSegments(ushort buildingID, ref Building data)
        {
            Vector3 position = data.CalculatePosition(m_boatSpawnPosition);
            Vector3 position2 = data.CalculatePosition(m_boatUnspawnPosition);
            if (PathManager.FindPathPosition(position, m_boatClass.m_service, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Ship, VehicleInfo.VehicleCategory.All, allowUnderground: false, requireConnect: false, 40f, excludeLaneWidth: false, checkPedestrianStreet: false, out var pathPos) && PathManager.FindPathPosition(position2, m_boatClass.m_service, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Ship, VehicleInfo.VehicleCategory.All, allowUnderground: false, requireConnect: false, 40f, excludeLaneWidth: false, checkPedestrianStreet: false, out var pathPos2))
            {
                ushort segment = pathPos.m_segment;
                ushort segment2 = pathPos2.m_segment;
                if (segment != 0 && GetNextConnectedSegment(segment, 0) != 0)
                {
                    return true;
                }
                if (segment2 != 0 && GetNextConnectedSegment(segment2, 0) != 0)
                {
                    return true;
                }
            }
            return false;
        }

        public override string GetLocalizedTooltip()
        {
            string text = LocaleFormatter.FormatGeneric("AIINFO_WATER_CONSUMPTION", GetWaterConsumption() * 16) + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_ELECTRICITY_CONSUMPTION", GetElectricityConsumption() * 16);
            string text2 = LocaleFormatter.FormatGeneric("AIINFO_WORKPLACES_ACCUMULATION", (m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3).ToString());
            string text3 = string.Empty;
            if (m_outputResource != ExtendedTransferManager.TransferReason.None && m_outputVehicleCount != 0)
            {
                text3 = text3 + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLE_COUNT", m_outputVehicleCount);
            }
            string baseTooltip = TooltipHelper.Append(base.GetLocalizedTooltip(), TooltipHelper.Format(LocaleFormatter.Info1, text, LocaleFormatter.Info2, text3, LocaleFormatter.WorkplaceCount, text2));
            string addTooltip = TooltipHelper.Format("arrowVisible", "false", "input1Visible", "true", "input2Visible", "false", "input3Visible", "false", "input4Visible", "false", "outputVisible", "false");
            string addTooltip2 = TooltipHelper.Format("input1", IndustryBuildingManager.ResourceSpriteName(m_outputResource), "input2", string.Empty, "input3", string.Empty, "input4", string.Empty, "output", string.Empty);
            baseTooltip = TooltipHelper.Append(baseTooltip, addTooltip);
            return TooltipHelper.Append(baseTooltip, addTooltip2);
        }

        public override string GetLocalizedStats(ushort buildingID, ref Building data)
        {
            string result = string.Empty;
            if (m_outputResource != ExtendedTransferManager.TransferReason.None && m_outputVehicleCount != 0)
            {
                int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                int num = (productionRate * m_outputVehicleCount + 99) / 100;
                int count = 0;
                int cargo = 0;
                int capacity = 0;
                int outside = 0;
                ExtendedVehicleManager.CalculateOwnVehicles(buildingID, ref data, m_outputResource, ref count, ref cargo, ref capacity, ref outside);
                result = LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLES", count, num);
                result = result + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_FISHING_FISHING_ROUTE_EFFICIENCY", data.m_education1);
            }
            return result;
        }

        public override void GetPollutionAccumulation(out int ground, out int noise)
        {
            ground = 0;
            noise = m_noiseAccumulation;
        }

        public override bool RequireRoadAccess()
        {
            return true;
        }

        public override void CountWorkPlaces(out int workPlaceCount0, out int workPlaceCount1, out int workPlaceCount2, out int workPlaceCount3)
        {
            workPlaceCount0 = m_workPlaceCount0;
            workPlaceCount1 = m_workPlaceCount1;
            workPlaceCount2 = m_workPlaceCount2;
            workPlaceCount3 = m_workPlaceCount3;
        }

        public override bool CanChangeVehicle(ushort buildingId)
        {
            return m_outputVehicleCount > 0 && base.CanChangeVehicle(buildingId);
        }

        public override bool CanChangeAdditionalVehicle(ushort buildingId)
        {
            return m_boatCount > 0 && base.CanChangeAdditionalVehicle(buildingId);
        }

        public override VehicleInfo.VehicleType GetVehicleType()
        {
            return VehicleInfo.VehicleType.Car;
        }

        public override VehicleInfo.VehicleType GetAdditionalVehicleType()
        {
            return VehicleInfo.VehicleType.Ship;
        }

        protected override ItemClass GetVehicleItemClass()
        {
            return m_outputVehicleClass;
        }

        protected override ItemClass GetAdditionalVehicleItemClass()
        {
            return m_boatClass;
        }
    }
}
