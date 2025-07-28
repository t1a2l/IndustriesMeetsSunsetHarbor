using System;
using ColossalFramework;
using ColossalFramework.DataBinding;
using MoreTransferReasons;
using ColossalFramework.Math;
using UnityEngine;
using MoreTransferReasons.AI;

namespace IndustriesMeetsSunsetHarbor.AI
{

    public class NewExtractingFacilityAI : IndustryBuildingAI, IExtendedBuildingAI
    {
        public TransportInfo m_transportInfo;

        public TransferManager.TransferReason m_outputResource = TransferManager.TransferReason.None;

        public ExtendedTransferManager.TransferReason m_extendedOutputResource = ExtendedTransferManager.TransferReason.None;

        [CustomizableProperty("Extraction Range")]
        public float m_extractRadius = 100f;

        [CustomizableProperty("Extraction Rate")]
        public int m_extractRate = 1000;

        [CustomizableProperty("Output Resource Rate")]
        public int m_outputRate = 1000;

        [CustomizableProperty("Output Vehicle Count")]
        public int m_outputVehicleCount = 10;

        public int m_variationGroupID;

        [NonSerialized]
        public BuildingInfo m_dummyInfo;

        public NaturalResourceManager.Resource NaturalResourceType = NaturalResourceManager.Resource.Fertility;

        protected override uint SearchKey => (uint)(((m_variationGroupID & 0xFF) << 24) | ((int)TransferManager.TransferReason.Grain << 16) | (m_info.m_cellWidth << 8) | m_info.m_cellLength);

        public override void InitializePrefab()
        {
            base.InitializePrefab();
            m_info.m_colorizeEverything = true;
            m_dummyInfo = null;
            if (m_info.m_subBuildings == null)
            {
                return;
            }
            for (int i = 0; i < m_info.m_subBuildings.Length; i++)
            {
                BuildingInfo buildingInfo = m_info.m_subBuildings[i].m_buildingInfo;
                if ((object)buildingInfo != null)
                {
                    ExtractingDummyAI component = buildingInfo.GetComponent<ExtractingDummyAI>();
                    if ((object)component != null)
                    {
                        component.m_extractRadius = m_extractRadius;
                        component.m_extractRate = m_extractRate;
                        component.m_outputRate = m_outputRate;
                        m_dummyInfo = buildingInfo;
                    }
                }
            }
        }

        public override Color GetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode)
        {
            switch (infoMode)
            {
                case InfoManager.InfoMode.Connections:
                    if (subInfoMode == InfoManager.SubInfoMode.WaterPower)
                    {
                        TransferManager.TransferReason outputResource = m_outputResource;
                        if (outputResource != TransferManager.TransferReason.None && (data.m_tempExport != 0 || data.m_finalExport != 0))
                        {
                            return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)outputResource];
                        }
                        ExtendedTransferManager.TransferReason extendedOutputResource = m_extendedOutputResource;
                        if (extendedOutputResource != ExtendedTransferManager.TransferReason.None && (data.m_tempExport != 0 || data.m_finalExport != 0))
                        {
                            return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)extendedOutputResource];
                        }
                        if (DistrictPark.IsPedestrianReason(outputResource, out var index1))
                        {
                            byte park = Singleton<DistrictManager>.instance.GetPark(data.m_position);
                            if (park != 0 && Singleton<DistrictManager>.instance.m_parks.m_buffer[park].IsPedestrianZone && (Singleton<DistrictManager>.instance.m_parks.m_buffer[park].m_tempExport[index1] != 0 || Singleton<DistrictManager>.instance.m_parks.m_buffer[park].m_finalExport[index1] != 0))
                            {
                                return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)outputResource];
                            }
                        }
                        if (ExtendedDistrictPark.IsPedestrianReason(extendedOutputResource, out var index2))
                        {
                            byte park = Singleton<DistrictManager>.instance.GetPark(data.m_position);
                            if (park != 0 && Singleton<DistrictManager>.instance.m_parks.m_buffer[park].IsPedestrianZone && (Singleton<DistrictManager>.instance.m_parks.m_buffer[park].m_tempExport[index2] != 0 || Singleton<DistrictManager>.instance.m_parks.m_buffer[park].m_finalExport[index2] != 0))
                            {
                                return Singleton<ExtendedTransferManager>.instance.m_properties.m_resourceColors[(int)extendedOutputResource];
                            }
                        }
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                case InfoManager.InfoMode.NaturalResources:
                    return GetResourceColor(m_info.m_class.m_subService);
                default:
                    return base.GetColor(buildingID, ref data, infoMode, subInfoMode);
            }
        }

        public static Color GetResourceColor(ItemClass.SubService subService)
        {
            return subService switch
            {
                ItemClass.SubService.PlayerIndustryOre => Singleton<NaturalResourceManager>.instance.m_properties.m_resourceColors[0],
                ItemClass.SubService.PlayerIndustryOil => Singleton<NaturalResourceManager>.instance.m_properties.m_resourceColors[2],
                ItemClass.SubService.PlayerIndustryFarming => Singleton<NaturalResourceManager>.instance.m_properties.m_resourceColors[3],
                ItemClass.SubService.PlayerIndustryForestry => Singleton<NaturalResourceManager>.instance.m_properties.m_resourceColors[4],
                _ => Singleton<InfoManager>.instance.m_properties.m_neutralColor,
            };
        }

        public override void RenderBuildOverlay(RenderManager.CameraInfo cameraInfo, Color color, Vector3 position, float angle, Segment3 connectionSegment)
        {
            base.RenderBuildOverlay(cameraInfo, color, position, angle, connectionSegment);
            if (connectionSegment.LengthSqr() > 1f && m_transportInfo != null)
            {
                Material connectionMaterial = m_transportInfo.m_connectionMaterial2;
                if (connectionMaterial != null)
                {
                    Segment3 segment = default;
                    segment.a = position;
                    segment.a.y = connectionSegment.a.y;
                    segment.b = connectionSegment.a;
                    HarborAI.RenderHarborPath(cameraInfo, color, connectionMaterial, segment);
                    HarborAI.RenderHarborPath(cameraInfo, color, connectionMaterial, connectionSegment);
                }
            }
        }

        public override string GetDebugString(ushort buildingID, ref Building data)
        {
            string text = base.GetDebugString(buildingID, ref data);
            int outputBufferSize = GetOutputBufferSize(ref data);
            int customBuffer1 = data.m_customBuffer1;
            if (m_outputResource != TransferManager.TransferReason.None)
            {
                text = StringUtils.SafeFormat("{0}\n{1}: {2} / {3}", text, m_outputResource.ToString(), customBuffer1, outputBufferSize);
            }
            else if (m_extendedOutputResource != ExtendedTransferManager.TransferReason.None)
            {
                text = StringUtils.SafeFormat("{0}\n{1}: {2} / {3}", text, m_extendedOutputResource.ToString(), customBuffer1, outputBufferSize);
            }
            return text;
        }

        public override string GetConstructionInfo(int productionRate)
        {
            return StringUtils.SafeFormat(ColossalFramework.Globalization.Locale.Get("TOOL_EXTRACTION_ESTIMATE"), m_outputRate * productionRate * 16 / 100);
        }

        public override int GetResourceRate(ushort buildingID, ref Building data, EconomyManager.Resource resource)
        {
            if (resource == EconomyManager.Resource.Maintenance)
            {
                int num = data.m_productionRate;
                if ((data.m_flags & Building.Flags.Evacuating) != 0)
                {
                    num = 0;
                }
                int budget = GetBudget(buildingID, ref data);
                int num2 = GetMaintenanceCost() / 100;
                num2 = num * budget / 100 * num2;
                int num3 = num2;
                DistrictManager instance = Singleton<DistrictManager>.instance;
                byte b = instance.GetPark(data.m_position);
                if (b != 0)
                {
                    if (!instance.m_parks.m_buffer[b].IsIndustry)
                    {
                        b = 0;
                    }
                    else if (m_industryType == DistrictPark.ParkType.Industry || m_industryType != instance.m_parks.m_buffer[b].m_parkType)
                    {
                        b = 0;
                    }
                }
                DistrictPolicies.Park parkPolicies = instance.m_parks.m_buffer[b].m_parkPolicies;
                if ((parkPolicies & DistrictPolicies.Park.ImprovedLogistics) != 0)
                {
                    num3 += num2 / 10;
                }
                if ((parkPolicies & DistrictPolicies.Park.AdvancedAutomation) != 0)
                {
                    num3 += num2 / 10;
                }
                return -num3;
            }
            return base.GetResourceRate(buildingID, ref data, resource);
        }

        public override BuildingInfoSub FindCollapsedInfo(out int rotations)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            if (instance.m_common != null)
            {
                if (m_industryType == DistrictPark.ParkType.Farming || m_industryType == DistrictPark.ParkType.Forestry)
                {
                    return FindCollapsedInfo(instance.m_common.m_collapsedLow, out rotations);
                }
                return FindCollapsedInfo(instance.m_common.m_collapsedHigh, out rotations);
            }
            rotations = 0;
            return null;
        }

        public override void CreateBuilding(ushort buildingID, ref Building data)
        {
            base.CreateBuilding(buildingID, ref data);
            GuideController properties = Singleton<GuideManager>.instance.m_properties;
            if (properties is not null)
            {
                Singleton<BuildingManager>.instance.m_extractorPlaced.Activate(properties.m_extractorPlaced);
            }
        }

        public override void StartTransfer(ushort buildingID, ref Building data, TransferManager.TransferReason material, TransferManager.TransferOffer offer)
        {
            if (material == m_outputResource)
            {
                VehicleInfo transferVehicleService = WarehouseAI.GetTransferVehicleService(material, ItemClass.Level.Level1, ref Singleton<SimulationManager>.instance.m_randomizer);
                if (transferVehicleService is null)
                {
                    return;
                }
                Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
                if (Singleton<VehicleManager>.instance.CreateVehicle(out var vehicle, ref Singleton<SimulationManager>.instance.m_randomizer, transferVehicleService, data.m_position, material, transferToSource: false, transferToTarget: true))
                {
                    transferVehicleService.m_vehicleAI.SetSource(vehicle, ref vehicles.m_buffer[vehicle], buildingID);
                    transferVehicleService.m_vehicleAI.StartTransfer(vehicle, ref vehicles.m_buffer[vehicle], material, offer);
                    ushort building = offer.Building;
                    if (building != 0 && (Singleton<BuildingManager>.instance.m_buildings.m_buffer[building].m_flags & Building.Flags.IncomingOutgoing) != 0)
                    {
                        transferVehicleService.m_vehicleAI.GetSize(vehicle, ref vehicles.m_buffer[vehicle], out var size, out var _);
                        CommonBuildingAI.ExportResource(buildingID, ref data, material, size);
                    }
                    data.m_outgoingProblemTimer = 0;
                }
            }
            else
            {
                base.StartTransfer(buildingID, ref data, material, offer);
            }
        }

        public override void ModifyMaterialBuffer(ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int amountDelta)
        {
            if (material == m_outputResource)
            {
                int outputBufferSize = GetOutputBufferSize(ref data);
                int customBuffer = data.m_customBuffer1;
                amountDelta = Mathf.Clamp(amountDelta, -customBuffer, outputBufferSize - customBuffer);
                customBuffer += amountDelta;
                data.m_customBuffer1 = (ushort)customBuffer;
            }
            else
            {
                base.ModifyMaterialBuffer(buildingID, ref data, material, ref amountDelta);
            }
        }

        void IExtendedBuildingAI.ExtendedStartTransfer(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, ExtendedTransferManager.Offer offer)
        {
            if (material == m_extendedOutputResource)
            {
                VehicleInfo transferVehicleService = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Industrial, ItemClass.SubService.IndustrialFarming, ItemClass.Level.Level1);
                if (transferVehicleService != null)
                {
                    Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
                    byte transferType = (byte)(material + 200);
                    if (ExtendedVehicleManager.CreateVehicle(out ushort vehicle, ref Singleton<SimulationManager>.instance.m_randomizer, transferVehicleService, data.m_position, transferType, false, true) && transferVehicleService.m_vehicleAI is ExtendedCargoTruckAI cargoTruckAI)
                    {
                        transferVehicleService.m_vehicleAI.SetSource(vehicle, ref vehicles.m_buffer[(int)vehicle], buildingID);
                        ((IExtendedVehicleAI)cargoTruckAI).ExtendedStartTransfer(vehicle, ref vehicles.m_buffer[(int)vehicle], material, offer);
                        ushort building = offer.Building;
                        if (building != 0 && (Singleton<BuildingManager>.instance.m_buildings.m_buffer[building].m_flags & Building.Flags.IncomingOutgoing) != 0)
                        {
                            transferVehicleService.m_vehicleAI.GetSize(vehicle, ref vehicles.m_buffer[vehicle], out var size, out var _);
                            IndustryBuildingManager.ExportResource(buildingID, ref data, material, size);
                        }
                        data.m_outgoingProblemTimer = 0;
                    }
                }
            }
        }

        void IExtendedBuildingAI.ExtendedGetMaterialAmount(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, out int amount, out int max)
        {
            amount = 0;
            max = 0;
        }

        void IExtendedBuildingAI.ExtendedModifyMaterialBuffer(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, ref int amountDelta)
        {
            if (material == m_extendedOutputResource)
            {
                int outputBufferSize = GetOutputBufferSize(ref data);
                int m_customBuffer1 = (int)data.m_customBuffer1;
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer1, outputBufferSize - m_customBuffer1);
                m_customBuffer1 += amountDelta;
                data.m_customBuffer1 = (ushort)m_customBuffer1;
            }
        }

        public override void BuildingDeactivated(ushort buildingID, ref Building data)
        {
            if (m_outputResource != TransferManager.TransferReason.None)
            {
                TransferManager.TransferOffer offer = default;
                offer.Building = buildingID;
                Singleton<TransferManager>.instance.RemoveOutgoingOffer(m_outputResource, offer);
            }
            if (m_extendedOutputResource != ExtendedTransferManager.TransferReason.None)
            {
                ExtendedTransferManager.Offer offer = default;
                offer.Building = buildingID;
                Singleton<ExtendedTransferManager>.instance.RemoveOutgoingOffer(m_extendedOutputResource, offer);
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

        protected override void ProduceGoods(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            ExtendedDistrictManager instance2 = Singleton<ExtendedDistrictManager>.instance;
            byte district = instance.GetDistrict(buildingData.m_position);
            byte b = instance.GetPark(buildingData.m_position);
            if (b != 0)
            {
                if (!instance.m_parks.m_buffer[b].IsIndustry)
                {
                    b = 0;
                }
                else if (m_industryType == DistrictPark.ParkType.Industry || m_industryType != instance.m_parks.m_buffer[b].m_parkType)
                {
                    b = 0;
                }
            }
            float num = (float)buildingData.Width * -4f;
            float num2 = (float)buildingData.Width * 4f;
            float num3 = (float)buildingData.Length * -4f;
            float num4 = (float)buildingData.Length * 4f;
            if (m_info.m_subBuildings != null)
            {
                for (int i = 0; i < m_info.m_subBuildings.Length; i++)
                {
                    if (m_info.m_subBuildings[i].m_buildingInfo is not null)
                    {
                        float num5 = m_info.m_subBuildings[i].m_buildingInfo.m_cellWidth;
                        float num6 = m_info.m_subBuildings[i].m_buildingInfo.m_cellLength;
                        float x = m_info.m_subBuildings[i].m_position.x;
                        float num7 = 0f - m_info.m_subBuildings[i].m_position.z;
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
            if ((parkPolicies & DistrictPolicies.Park.ImprovedLogistics) != 0)
            {
                int num12 = GetMaintenanceCost() / 100;
                num12 = finalProductionRate * num12 / 1000;
                if (num12 != 0)
                {
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Maintenance, num12, m_info.m_class);
                }
            }
            int num13 = m_outputRate;
            if ((parkPolicies & DistrictPolicies.Park.AdvancedAutomation) != 0)
            {
                num13 = (num13 * 110 + 50) / 100;
                int num14 = GetMaintenanceCost() / 100;
                num14 = finalProductionRate * num14 / 1000;
                if (num14 != 0)
                {
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Maintenance, num14, m_info.m_class);
                }
            }
            if ((parkPolicies & DistrictPolicies.Park.WorkSafety) != 0)
            {
                int num15 = (aliveWorkerCount + (int)((Singleton<SimulationManager>.instance.m_currentFrameIndex >> 8) & 0xF)) / 16;
                if (num15 != 0)
                {
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, num15, m_info.m_class);
                }
            }
            if (finalProductionRate != 0)
            {
                Vector3 position2 = buildingData.m_position;
                if ((object)m_dummyInfo != null)
                {
                    int num16 = 0;
                    ushort subBuilding = buildingData.m_subBuilding;
                    while (subBuilding != 0)
                    {
                        BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[subBuilding].Info;
                        if ((object)info == m_dummyInfo)
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
                int num18 = m_pollutionAccumulation;
                NaturalResourceManager.Resource naturalResourceType = NaturalResourceType;
                if (b != 0)
                {
                    instance.m_parks.m_buffer[b].GetProductionFactors(out var processingFactor, out var pollutionFactor);
                    finalProductionRate = (finalProductionRate * processingFactor + 50) / 100;
                    num18 = (num18 * pollutionFactor + 50) / 100;
                }
                else if (m_industryType != DistrictPark.ParkType.Industry)
                {
                    finalProductionRate = 0;
                }
                int CustomBuffer = 0;
                bool flag = false;
                if (naturalResourceType != NaturalResourceManager.Resource.None)
                {
                    int finalExtractionRate = (m_extractRate * finalProductionRate + 99) / 100;
                    int ResourceBufferSize = GetResourceBufferSize(ref buildingData);
                    CustomBuffer = buildingData.m_customBuffer2;
                    int ResourceCount = Singleton<NaturalResourceManager>.instance.CountResource(naturalResourceType, position2, m_extractRadius);
                    if (ResourceCount == 0)
                    {
                        flag = true;
                    }
                    else
                    {
                        ResourceCount = Mathf.Clamp((ResourceCount + 9) / 10, (m_extractRate + 99) / 100, finalExtractionRate);
                        ResourceCount = Mathf.Max(0, Mathf.Min(ResourceBufferSize - CustomBuffer, ResourceCount));
                    }
                    int num23 = (ResourceCount + Singleton<SimulationManager>.instance.m_randomizer.Int32(100u)) / 100;
                    if (num23 != 0)
                    {
                        Singleton<NaturalResourceManager>.instance.TryFetchResource(naturalResourceType, num23, num23, position2, m_extractRadius);
                    }
                    CustomBuffer += ResourceCount;
                    if (CustomBuffer < finalExtractionRate)
                    {
                        finalProductionRate = (CustomBuffer * 100 + m_extractRate - 1) / m_extractRate;
                        if (finalProductionRate < 10)
                        {
                            problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.NoNaturalResources);
                        }
                    }
                }
                if (naturalResourceType != NaturalResourceManager.Resource.None)
                {
                    int finalExtractionRate = (m_extractRate * finalProductionRate + 99) / 100;
                    CustomBuffer = Mathf.Max(0, CustomBuffer - finalExtractionRate);
                    buildingData.m_customBuffer2 = (ushort)CustomBuffer;
                }

                int OutputBufferSize1 = 0;
                int CustomBuffer1 = 0;
                if (m_outputResource != TransferManager.TransferReason.None || m_extendedOutputResource != ExtendedTransferManager.TransferReason.None)
                {
                    OutputBufferSize1 = GetOutputBufferSize(ref buildingData);
                    CustomBuffer1 = buildingData.m_customBuffer1;
                    int OutputProductionRate = (num13 * finalProductionRate + 99) / 100;
                    if (OutputBufferSize1 - CustomBuffer1 < OutputProductionRate)
                    {
                        OutputProductionRate = Mathf.Max(0, OutputBufferSize1 - CustomBuffer1);
                        finalProductionRate = (OutputProductionRate * 100 + num13 - 1) / num13;
                        problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.NoPlaceforGoods);
                    }
                }
                if (m_outputResource != TransferManager.TransferReason.None || m_extendedOutputResource != ExtendedTransferManager.TransferReason.None)
                {
                    int OutputProductionRate = (num13 * finalProductionRate + 99) / 100;
                    CustomBuffer1 = Mathf.Min(OutputBufferSize1, CustomBuffer1 + OutputProductionRate);
                    buildingData.m_customBuffer1 = (ushort)CustomBuffer1;
                    if (m_outputResource != TransferManager.TransferReason.None)
                    {
                        instance.m_parks.m_buffer[b].AddProductionAmount(m_outputResource, OutputProductionRate);
                    }
                    else if (m_extendedOutputResource != ExtendedTransferManager.TransferReason.None)
                    {
                        instance2.m_industryParks.m_buffer[b].AddProductionAmount(instance.m_parks.m_buffer[b], m_extendedOutputResource, OutputProductionRate);
                    }
                }
                num18 = (finalProductionRate * num18 + 50) / 100;
                if (num18 != 0)
                {
                    num18 = UniqueFacultyAI.DecreaseByBonus(UniqueFacultyAI.FacultyBonus.Science, num18);
                    Singleton<NaturalResourceManager>.instance.TryDumpResource(NaturalResourceManager.Resource.Pollution, num18, num18, position, m_pollutionRadius);
                }
                HandleDead2(buildingID, ref buildingData, ref behaviour, totalWorkerCount);
                if (b != 0 || m_industryType == DistrictPark.ParkType.Industry)
                {
                    if (m_outputResource != TransferManager.TransferReason.None || m_extendedOutputResource != ExtendedTransferManager.TransferReason.None)
                    {
                        int num29 = (num17 * m_outputVehicleCount + 99) / 100;
                        int count = 0;
                        int cargo = 0;
                        int capacity = 0;
                        int outside = 0;
                        if (m_outputResource != TransferManager.TransferReason.None)
                        {
                            CalculateOwnVehicles(buildingID, ref buildingData, m_outputResource, ref count, ref cargo, ref capacity, ref outside);
                        }
                        else if (m_extendedOutputResource != ExtendedTransferManager.TransferReason.None)
                        {
                            ExtendedVehicleManager.CalculateOwnVehicles(buildingID, ref buildingData, m_extendedOutputResource, ref count, ref cargo, ref capacity, ref outside);
                        }
                       
                        buildingData.m_tempExport = (byte)Mathf.Clamp(outside, buildingData.m_tempExport, 255);
                        if (buildingData.m_finalExport != 0)
                        {
                            instance.m_districts.m_buffer[district].m_playerConsumption.m_finalExportAmount += buildingData.m_finalExport;
                        }
                        int OutputCustomBuffer1 = CustomBuffer1;
                        if (OutputCustomBuffer1 > 0 && flag)
                        {
                            OutputCustomBuffer1 = Mathf.Max(OutputCustomBuffer1, 16000);
                        }
                        if (OutputCustomBuffer1 >= 8000 && count < num29)
                        {
                            if (m_outputResource != TransferManager.TransferReason.None)
                            {
                                TransferManager.TransferOffer offer = default;
                                offer.Priority = Mathf.Max(1, OutputCustomBuffer1 * 8 / OutputBufferSize1);
                                offer.Building = buildingID;
                                offer.Position = buildingData.m_position;
                                offer.Amount = 1;
                                offer.Active = true;
                                Singleton<TransferManager>.instance.AddOutgoingOffer(m_outputResource, offer);
                            }
                            else if (m_extendedOutputResource != ExtendedTransferManager.TransferReason.None)
                            {
                                ExtendedTransferManager.Offer offer = default;
                                offer.Building = buildingID;
                                offer.Position = buildingData.m_position;
                                offer.Amount = 1;
                                offer.Active = true;
                                Singleton<ExtendedTransferManager>.instance.AddOutgoingOffer(m_extendedOutputResource, offer);
                            }
                            
                        }
                        if (m_outputResource != TransferManager.TransferReason.None)
                        {
                            instance.m_parks.m_buffer[b].AddBufferStatus(m_outputResource, CustomBuffer1, 0, OutputBufferSize1);
                        }
                        else if (m_extendedOutputResource != ExtendedTransferManager.TransferReason.None)
                        {
                            instance2.m_industryParks.m_buffer[b].AddBufferStatus(m_extendedOutputResource, CustomBuffer1, 0, OutputCustomBuffer1);
                        }       
                    }
                    GuideController properties = Singleton<GuideManager>.instance.m_properties;
                    if (properties is not null)
                    {
                        Singleton<BuildingManager>.instance.m_extractorPlaced.Activate(properties.m_extractorPlaced);
                    }
                }
                int num31 = finalProductionRate * m_noiseAccumulation / 100;
                if (num31 != 0)
                {
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, num31, position, m_noiseRadius);
                }
            }
            buildingData.m_problems = problemStruct;
            buildingData.m_education3 = (byte)Mathf.Clamp(finalProductionRate * num13 / Mathf.Max(1, m_outputRate), 0, 255);
            if (b != 0)
            {
                instance.m_parks.m_buffer[b].AddWorkers(aliveWorkerCount);
            }
            else if (m_industryType != DistrictPark.ParkType.Industry)
            {
                GuideController properties2 = Singleton<GuideManager>.instance.m_properties;
                if (properties2 is not null)
                {
                    Singleton<BuildingManager>.instance.m_industryBuildingOutsideIndustryArea.Activate(properties2.m_industryBuildingOutsideIndustryArea, buildingID);
                }
            }
            instance.m_districts.m_buffer[district].AddIndustryData(m_info.m_class.m_subService, (uint)(m_info.m_cellWidth * m_info.m_cellLength), (uint)finalProductionRate);
            base.ProduceGoods(buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
        }

        public override ToolBase.ToolErrors CheckBuildPosition(ushort relocateID, ref Vector3 position, ref float angle, float waterHeight, float elevation, ref Segment3 connectionSegment, out int productionRate, out int constructionCost)
        {
            ToolBase.ToolErrors result = base.CheckBuildPosition(relocateID, ref position, ref angle, waterHeight, elevation, ref connectionSegment, out productionRate, out constructionCost);
            NaturalResourceManager.Resource naturalResourceType = NaturalResourceType;
            if (naturalResourceType != NaturalResourceManager.Resource.None && m_extractRate != 0)
            {
                int num = Singleton<NaturalResourceManager>.instance.CountResource(naturalResourceType, position, m_extractRadius);
                if (num == 0)
                {
                    productionRate = 0;
                }
                else
                {
                    int num2 = Mathf.Clamp((num + 9) / 10, (m_extractRate + 99) / 100, m_extractRate);
                    productionRate = Mathf.Clamp((num2 * 100 + m_extractRate - 1) / m_extractRate, 0, 100);
                }
            }
            return result;
        }

        public override string GetLocalizedTooltip()
        {
            string text_water = LocaleFormatter.FormatGeneric("AIINFO_WATER_CONSUMPTION", GetWaterConsumption() * 16) + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_ELECTRICITY_CONSUMPTION", GetElectricityConsumption() * 16);
            string text_prod = LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_PRODUCTION_RATE", m_outputRate * 16);
            if (m_outputResource != TransferManager.TransferReason.None && m_outputVehicleCount != 0)
            {
                text_prod = text_prod + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLE_COUNT", [m_outputVehicleCount]);
            }
            if (m_extendedOutputResource != ExtendedTransferManager.TransferReason.None && m_outputVehicleCount != 0)
            {
                text_prod = text_prod + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLE_COUNT", m_outputVehicleCount);
            }
            string text_workplace = LocaleFormatter.FormatGeneric("AIINFO_WORKPLACES_ACCUMULATION", (m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3).ToString());
            string baseTooltip = TooltipHelper.Append(base.GetLocalizedTooltip(), TooltipHelper.Format(LocaleFormatter.Info1, text_water, LocaleFormatter.Info2, text_prod, LocaleFormatter.WorkplaceCount, text_workplace));

            bool flag1 = m_outputResource != TransferManager.TransferReason.None;
            string text1 = (m_outputResource == TransferManager.TransferReason.None) ? string.Empty : m_outputResource.ToString();

            bool flag2 = m_extendedOutputResource != ExtendedTransferManager.TransferReason.None;
            string text2 = (m_extendedOutputResource == ExtendedTransferManager.TransferReason.None) ? string.Empty : m_extendedOutputResource.ToString();

            string addTooltip = TooltipHelper.Format(
            [
                "arrowVisible",
                "false",
                "input1Visible",
                flag1.ToString(),
                "input2Visible",
                flag2.ToString(),
                "input3Visible",
                "false",
                "input4Visible",
                "false",
                "outputVisible",
                "false"
            ]);
            string addTooltip2 = TooltipHelper.Format(
            [
                "input1",
                text1,
                "input2",
                text2,
                "input3",
                string.Empty,
                "input4",
                string.Empty,
                "output",
                string.Empty
            ]);
            baseTooltip = TooltipHelper.Append(baseTooltip, addTooltip);
            return TooltipHelper.Append(baseTooltip, addTooltip2);
        }

        public override string GetLocalizedStats(ushort buildingID, ref Building data)
        {
            int num = data.m_education3 * m_outputRate * 16 / 100;
            string text = LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_PRODUCTION_RATE", num);
            if (m_outputResource != TransferManager.TransferReason.None && m_outputVehicleCount != 0)
            {
                int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                int num2 = (productionRate * m_outputVehicleCount + 99) / 100;
                int count = 0;
                int cargo = 0;
                int capacity = 0;
                int outside = 0;
                CalculateOwnVehicles(buildingID, ref data, m_outputResource, ref count, ref cargo, ref capacity, ref outside);
                text = text + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLES", count, num2);
            }
            else if (m_extendedOutputResource != ExtendedTransferManager.TransferReason.None && m_outputVehicleCount != 0)
            {
                int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                int vehicle_count = (productionRate * m_outputVehicleCount + 99) / 100;
                int count = 0;
                int capacity = 0;
                int cargo = 0;
                int outside = 0;
                ExtendedVehicleManager.CalculateOwnVehicles(buildingID, ref data, m_extendedOutputResource, ref count, ref capacity, ref cargo, ref outside);
                text = text + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLES", [count, vehicle_count]);
            }
            return text;
        }

        public override bool RequireRoadAccess()
        {
            return base.RequireRoadAccess() || m_outputResource != TransferManager.TransferReason.None || m_extendedOutputResource != ExtendedTransferManager.TransferReason.None;
        }

        public int GetResourceBufferSize(ref Building data)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte b = instance.GetPark(data.m_position);
            var park = instance.m_parks.m_buffer[b];
            var m_finalStorageDelta = park.m_finalStorageDelta;
            if (b != 0)
            {
                if (!park.IsIndustry)
                {
                    m_finalStorageDelta = 0;
                }
                else if (m_industryType == DistrictPark.ParkType.Industry || m_industryType != park.m_parkType)
                {
                    m_finalStorageDelta = 0;
                }
            }
            int num = m_extractRate * 32;
            if ((park.m_parkPolicies & DistrictPolicies.Park.ImprovedLogistics) != DistrictPolicies.Park.None)
            {
                num = (num * 6 + 4) / 5;
            }
            num = (num * (100 + m_finalStorageDelta) + 50) / 100;
            return Mathf.Clamp(num, 0, 64000);
        }

        public int GetOutputBufferSize(ref Building data)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte b = instance.GetPark(data.m_position);
            var park = instance.m_parks.m_buffer[b];
            var m_finalStorageDelta = park.m_finalStorageDelta;
            if (b != 0)
            {
                if (!park.IsIndustry)
                {
                    m_finalStorageDelta = 0;
                }
                else if (m_industryType == DistrictPark.ParkType.Industry || m_industryType != park.m_parkType)
                {
                    m_finalStorageDelta = 0;
                }
            }
            if (m_outputVehicleCount == 0)
            {
                int num = m_outputRate * 100;
                return Mathf.Clamp(num, 1, 64000);
            }
            int num2 = m_outputRate * 32 + 8000;
            if ((park.m_parkPolicies & DistrictPolicies.Park.ImprovedLogistics) != DistrictPolicies.Park.None)
            {
                num2 = (num2 * 6 + 4) / 5;
            }
            num2 = (num2 * (100 + m_finalStorageDelta) + 50) / 100;
            return Mathf.Clamp(num2, 8000, 64000);
        }

    }
}