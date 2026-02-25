using System;
using ColossalFramework;
using ColossalFramework.DataBinding;
using UnityEngine;
using IndustriesMeetsSunsetHarbor.Managers;

namespace IndustriesMeetsSunsetHarbor.AI
{
    public class ExtendedProcessingFacilityAI : IndustryBuildingAI
    {
        [CustomizableProperty("Input Resource Rate 1")]
        public int m_inputRate1;

        [CustomizableProperty("Input Resource Rate 2")]
        public int m_inputRate2;

        [CustomizableProperty("Output Resource Rate 1")]
        public int m_outputRate1 = 1000;

        [CustomizableProperty("Output Resource Rate 2")]
        public int m_outputRate2 = 1000;

        [CustomizableProperty("Output 1 Vehicle Count")]
        public int m_outputVehicleCount1 = 10;

        [CustomizableProperty("Output 2 Vehicle Count")]
        public int m_outputVehicleCount2 = 10;

        public int m_variationGroupID;

        [NonSerialized]
        private bool m_hasBufferStatusMeshes;

        protected override uint SearchKey
        {
            get
            {
                return (uint)(((m_variationGroupID & 255) << 24) | (int)((int)m_outputResource1 << 16) | (m_info.m_cellWidth << 8) | m_info.m_cellLength);
            }
        }

        public bool IsFishFactory
        {
            get
            {
                return m_inputResource1 == TransferManager.TransferReason.Fish;
            }
        }

        [CustomizableProperty("Input Resource 1")]
        public TransferManager.TransferReason m_inputResource1 = TransferManager.TransferReason.None;

        [CustomizableProperty("Input Resource 2")]
        public TransferManager.TransferReason m_inputResource2 = TransferManager.TransferReason.None;

        [CustomizableProperty("Output Resource 1")]
        public TransferManager.TransferReason m_outputResource1 = TransferManager.TransferReason.None;

        [CustomizableProperty("Output Resource 2")]
        public TransferManager.TransferReason m_outputResource2 = TransferManager.TransferReason.None;

        public override void InitializePrefab()
        {
            base.InitializePrefab();
            m_hasBufferStatusMeshes = false;
            if (m_info.m_subMeshes != null)
            {
                for (int i = 0; i < m_info.m_subMeshes.Length; i++)
                {
                    if ((m_info.m_subMeshes[i].m_flagsRequired & Building.Flags.CapacityFull) != Building.Flags.None)
                    {
                        m_hasBufferStatusMeshes = true;
                        break;
                    }
                }
            }
        }

        public override Color GetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode)
        {
            switch (infoMode)
            {
                case InfoManager.InfoMode.Connections:
                    switch (subInfoMode)
                    {
                        case InfoManager.SubInfoMode.Default:
                            if (m_inputResource1 != TransferManager.TransferReason.None && ((data.m_tempImport | data.m_finalImport) & 1) != 0)
                            {
                                return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource1];
                            }
                            if (m_inputResource2 != TransferManager.TransferReason.None && ((data.m_tempImport | data.m_finalImport) & 2) != 0)
                            {
                                return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource2];
                            }
                            break;
                        case InfoManager.SubInfoMode.WaterPower:
                            {
                                if (m_outputResource1 != TransferManager.TransferReason.None && (data.m_tempExport != 0 || data.m_finalExport != 0))
                                {
                                    return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_outputResource1];
                                }
                                if (m_outputResource2 != TransferManager.TransferReason.None && (data.m_tempExport != 0 || data.m_finalExport != 0))
                                {
                                    return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_outputResource2];
                                }
                                if (DistrictPark.IsPedestrianReason(m_outputResource1, out var index1))
                                {
                                    byte park = Singleton<DistrictManager>.instance.GetPark(data.m_position);
                                    if (park != 0 && Singleton<DistrictManager>.instance.m_parks.m_buffer[park].IsPedestrianZone && (Singleton<DistrictManager>.instance.m_parks.m_buffer[park].m_tempExport[index1] != 0 || Singleton<DistrictManager>.instance.m_parks.m_buffer[park].m_finalExport[index1] != 0))
                                    {
                                        return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_outputResource1];
                                    }
                                }
                                if (DistrictPark.IsPedestrianReason(m_outputResource2, out var index2))
                                {
                                    byte park = Singleton<DistrictManager>.instance.GetPark(data.m_position);
                                    if (park != 0 && Singleton<DistrictManager>.instance.m_parks.m_buffer[park].IsPedestrianZone && (Singleton<DistrictManager>.instance.m_parks.m_buffer[park].m_tempExport[index2] != 0 || Singleton<DistrictManager>.instance.m_parks.m_buffer[park].m_finalExport[index2] != 0))
                                    {
                                        return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_outputResource2];
                                    }
                                }
                                break;
                            }
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                case InfoManager.InfoMode.Fishing:
                    if (m_inputResource1 == TransferManager.TransferReason.Fish)
                    {
                        if ((data.m_flags & Building.Flags.Active) != 0)
                        {
                            return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
                        }
                        return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_inactiveColor;
                    }
                    return base.GetColor(buildingID, ref data, infoMode, subInfoMode);
                default:
                    return base.GetColor(buildingID, ref data, infoMode, subInfoMode);
            }
        }

        public override void GetPlacementInfoMode(out InfoManager.InfoMode mode, out InfoManager.SubInfoMode subMode, float elevation)
        {
            if (!IsFishFactory)
            {
                base.GetPlacementInfoMode(out mode, out subMode, elevation);
            }
            else
            {
                mode = InfoManager.InfoMode.Fishing;
                subMode = InfoManager.SubInfoMode.Default;
            }
        }

        public override string GetDebugString(ushort buildingID, ref Building data)
        {
            var custom_buffers = CustomBuffersManager.GetCustomBuffer(buildingID);
            string text = base.GetDebugString(buildingID, ref data);
            if (m_inputResource1 != TransferManager.TransferReason.None)
            {
                int inputBufferSize1 = GetInputBufferSize(ref data, m_inputRate1);
                int customBuffer1 = (int)custom_buffers.m_customBuffer1;
                int count1 = 0;
                int cargo1 = 0;
                int capacity1 = 0;
                int outside1 = 0;
                base.CalculateGuestVehicles(buildingID, ref data, m_inputResource1, ref count1, ref cargo1, ref capacity1, ref outside1);
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}",
                [
                    text,
                    m_inputResource1.ToString(),
                    customBuffer1,
                    cargo1,
                    inputBufferSize1
                ]);
            }
            if (m_inputResource2 != TransferManager.TransferReason.None)
            {
                int inputBufferSize2 = GetInputBufferSize(ref data, m_inputRate2);
                int customBuffer2 = (int)custom_buffers.m_customBuffer2;
                int count2 = 0;
                int cargo2 = 0;
                int capacity2 = 0;
                int outside2 = 0;
                base.CalculateGuestVehicles(buildingID, ref data, m_inputResource2, ref count2, ref cargo2, ref capacity2, ref outside2);
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}",
                [
                    text,
                    m_inputResource2.ToString(),
                    customBuffer2,
                    cargo2,
                    inputBufferSize2
                ]);
            }
            if (m_outputResource1 != TransferManager.TransferReason.None)
            {
                int outputBufferSize1 = GetOutputBufferSize(ref data, m_outputRate1, m_outputVehicleCount1);
                int outputBuffer1 = (int)custom_buffers.m_customBuffer9;
                int count1 = 0;
                int cargo1 = 0;
                int capacity1 = 0;
                int outside1 = 0;
                base.CalculateGuestVehicles(buildingID, ref data, m_outputResource1, ref count1, ref cargo1, ref capacity1, ref outside1);
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}",
                [
                    text,
                    m_outputResource1.ToString(),
                    outputBuffer1,
                    cargo1,
                    outputBufferSize1
                ]);
            }
            if (m_outputResource2 != TransferManager.TransferReason.None)
            {
                int outputBufferSize2 = GetOutputBufferSize(ref data, m_outputRate1, m_outputVehicleCount1);
                int outputBuffer2 = (int)custom_buffers.m_customBuffer9;
                int count2 = 0;
                int cargo2 = 0;
                int capacity2 = 0;
                int outside2 = 0;
                base.CalculateGuestVehicles(buildingID, ref data, m_outputResource2, ref count2, ref cargo2, ref capacity2, ref outside2);
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}",
                [
                    text,
                    m_outputResource2.ToString(),
                    outputBuffer2,
                    cargo2,
                    outputBufferSize2
                ]);
            }
            return text;
        }

        public override int GetResourceRate(ushort buildingID, ref Building data, EconomyManager.Resource resource)
        {
            if (resource == EconomyManager.Resource.Maintenance)
            {
                int num = (int)data.m_productionRate;
                if ((data.m_flags & Building.Flags.Evacuating) != Building.Flags.None)
                {
                    num = 0;
                }
                int budget = base.GetBudget(buildingID, ref data);
                int num2 = GetMaintenanceCost() / 100;
                num2 = num * budget / 100 * num2;
                int num3 = num2;
                DistrictManager instance = Singleton<DistrictManager>.instance;
                byte b = instance.GetPark(data.m_position);
                if (b != 0)
                {
                    if (!instance.m_parks.m_buffer[(int)b].IsIndustry)
                    {
                        b = 0;
                    }
                    else if (m_industryType == DistrictPark.ParkType.Industry || m_industryType != instance.m_parks.m_buffer[(int)b].m_parkType)
                    {
                        b = 0;
                    }
                }
                DistrictPolicies.Park parkPolicies = instance.m_parks.m_buffer[(int)b].m_parkPolicies;
                if ((parkPolicies & DistrictPolicies.Park.ImprovedLogistics) != DistrictPolicies.Park.None)
                {
                    num3 += num2 / 10;
                }
                if ((parkPolicies & DistrictPolicies.Park.AdvancedAutomation) != DistrictPolicies.Park.None)
                {
                    num3 += num2 / 10;
                }
                return -num3;
            }
            return base.GetResourceRate(buildingID, ref data, resource);
        }

        public override void CreateBuilding(ushort buildingID, ref Building data)
        {
            base.CreateBuilding(buildingID, ref data);
            if (m_info != null && m_info.m_class != null && m_info.m_class.m_service == ItemClass.Service.Fishing)
            {
                GuideController properties = Singleton<GuideManager>.instance.m_properties;
                if (properties != null && Singleton<BuildingManager>.instance.m_intercityBusStationBuilt != null)
                {
                    Singleton<BuildingManager>.instance.m_intercityBusStationBuilt.Activate(properties.m_fishFactoryMarketBuilt, buildingID);
                }
            }
        }

        public override void StartTransfer(ushort buildingID, ref Building data, TransferManager.TransferReason material, TransferManager.TransferOffer offer)
	{
	    if (material == m_outputResource1)
	    {
	        int variationGroupID = m_variationGroupID;
	        ItemClass.Level level = ((variationGroupID == 1 || variationGroupID == 9) ? ItemClass.Level.Level2 : ItemClass.Level.Level1);
	        VehicleInfo transferVehicleService = WarehouseAI.GetTransferVehicleService(material, level, ref Singleton<SimulationManager>.instance.m_randomizer);
	        if (transferVehicleService == null)
	        {
		        return;
	        }
	        Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
	        if (Singleton<VehicleManager>.instance.CreateVehicle(out var vehicle, ref Singleton<SimulationManager>.instance.m_randomizer, transferVehicleService, data.m_position, material, transferToSource: false, transferToTarget: true))
	        {
		    vehicles.m_buffer[vehicle].m_gateIndex = (byte)m_variationGroupID;
		    transferVehicleService.m_vehicleAI.SetSource(vehicle, ref vehicles.m_buffer[vehicle], buildingID);
		    transferVehicleService.m_vehicleAI.StartTransfer(vehicle, ref vehicles.m_buffer[vehicle], material, offer);
		    ushort building = offer.Building;
		    if (building != 0 && (Singleton<BuildingManager>.instance.m_buildings.m_buffer[building].m_flags & Building.Flags.IncomingOutgoing) != 0)
		    {
		        transferVehicleService.m_vehicleAI.GetSize(vehicle, ref vehicles.m_buffer[vehicle], out var size, out var _);
		        ExportResource(buildingID, ref data, material, size);
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
            var custom_buffers = CustomBuffersManager.GetCustomBuffer(buildingID);
            if (material == m_inputResource1)
            {
                int inputBufferSize1 = GetInputBufferSize(ref data, m_inputRate1);
                int m_customBuffer1 = (int)custom_buffers.m_customBuffer1;
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer1, inputBufferSize1 - m_customBuffer1);
                m_customBuffer1 += amountDelta;
                custom_buffers.m_customBuffer1 = (ushort)m_customBuffer1;
            }
            else if (material == m_inputResource2)
            {
                int inputBufferSize2 = GetInputBufferSize(ref data, m_inputRate2);
                int m_customBuffer2 = (int)custom_buffers.m_customBuffer2;
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer2, inputBufferSize2 - m_customBuffer2);
                m_customBuffer2 += amountDelta;
                custom_buffers.m_customBuffer2 = (ushort)m_customBuffer2;
            }
            else if (material == m_outputResource1)
            {
                int outputBufferSize1 = GetOutputBufferSize(ref data, m_outputRate1, m_outputVehicleCount1);
                int m_customBuffer3 = (int)custom_buffers.m_customBuffer3;
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer3, outputBufferSize1 - m_customBuffer3);
                m_customBuffer3 += amountDelta;
                custom_buffers.m_customBuffer3 = (ushort)m_customBuffer3;
            }
            else if (material == m_outputResource2)
            {
                int outputBufferSize1 = GetOutputBufferSize(ref data, m_outputRate2, m_outputVehicleCount2);
                int m_customBuffer4 = (int)custom_buffers.m_customBuffer4;
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer4, outputBufferSize1 - m_customBuffer4);
                m_customBuffer4 += amountDelta;
                custom_buffers.m_customBuffer4 = (ushort)m_customBuffer4;
            }
            else
            {
                base.ModifyMaterialBuffer(buildingID, ref data, material, ref amountDelta);
            }
            CustomBuffersManager.SetCustomBuffer(buildingID, custom_buffers);
        }

        public override void BuildingDeactivated(ushort buildingID, ref Building data)
        {
            TransferManager.TransferOffer transferOffer = default;
            transferOffer.Building = buildingID;
            if (m_inputResource1 != TransferManager.TransferReason.None)
            {
                Singleton<TransferManager>.instance.RemoveIncomingOffer(m_inputResource1, transferOffer);
            }
            if (m_inputResource2 != TransferManager.TransferReason.None)
            {
                Singleton<TransferManager>.instance.RemoveIncomingOffer(m_inputResource2, transferOffer);
            }
            if (m_outputResource1 != TransferManager.TransferReason.None)
            {
                Singleton<TransferManager>.instance.RemoveOutgoingOffer(m_outputResource1, transferOffer);
            }
            if (m_outputResource2 != TransferManager.TransferReason.None)
            {
                Singleton<TransferManager>.instance.RemoveOutgoingOffer(m_outputResource2, transferOffer);
            }
            base.BuildingDeactivated(buildingID, ref data);
        }

        public override void SimulationStep(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            base.SimulationStep(buildingID, ref buildingData, ref frameData);
            if ((Singleton<SimulationManager>.instance.m_currentFrameIndex & 4095U) >= 3840U)
            {
                buildingData.m_finalImport = buildingData.m_tempImport;
                buildingData.m_finalExport = buildingData.m_tempExport;
                buildingData.m_tempImport = 0;
                buildingData.m_tempExport = 0;
            }
            if (m_hasBufferStatusMeshes)
            {
                CheckCapacity(buildingID, ref buildingData);
            }
            if (m_info != null && m_info.m_class != null && m_info.m_class.m_service == ItemClass.Service.Fishing)
            {
                GuideController properties = Singleton<GuideManager>.instance.m_properties;
                if (properties != null && Singleton<BuildingManager>.instance.m_intercityBusStationBuilt != null)
                {
                    Singleton<BuildingManager>.instance.m_intercityBusStationBuilt.Activate(properties.m_fishFactoryMarketBuilt, buildingID);
                }
            }
        }

        protected override void ProduceGoods(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(buildingData.m_position);
            byte b = instance.GetPark(buildingData.m_position);
            if (b != 0)
            {
                if (!instance.m_parks.m_buffer[(int)b].IsIndustry)
                {
                    b = 0;
                }
                else if (m_industryType == DistrictPark.ParkType.Industry || m_industryType != instance.m_parks.m_buffer[(int)b].m_parkType)
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
                    if (m_info.m_subBuildings[i].m_buildingInfo != null)
                    {
                        float num5 = (float)m_info.m_subBuildings[i].m_buildingInfo.m_cellWidth;
                        float num6 = (float)m_info.m_subBuildings[i].m_buildingInfo.m_cellLength;
                        float x = m_info.m_subBuildings[i].m_position.x;
                        float num7 = -m_info.m_subBuildings[i].m_position.z;
                        num = Mathf.Min(num, x - num5 * 4f);
                        num2 = Mathf.Max(num2, x + num5 * 4f);
                        num3 = Mathf.Min(num3, num7 - num6 * 4f);
                        num4 = Mathf.Max(num4, num7 + num6 * 4f);
                    }
                }
            }
            float angle = buildingData.m_angle;
            float num8 = -(num + num2) * 0.5f;
            float num9 = -(num3 + num4) * 0.5f;
            float num10 = Mathf.Sin(angle);
            float num11 = Mathf.Cos(angle);
            Vector3 vector = buildingData.m_position - new Vector3(num11 * num8 + num10 * num9, 0f, num10 * num8 - num11 * num9);
            Notification.ProblemStruct problemStruct = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem1.NoResources | Notification.Problem1.NoPlaceforGoods | Notification.Problem1.NoInputProducts | Notification.Problem1.NoFishingGoods);
            bool flag = m_info.m_class.m_service == ItemClass.Service.Fishing;
            DistrictPolicies.Park parkPolicies = instance.m_parks.m_buffer[(int)b].m_parkPolicies;
            DistrictPark[] buffer = instance.m_parks.m_buffer;
            byte b2 = b;
            buffer[(int)b2].m_parkPoliciesEffect = buffer[(int)b2].m_parkPoliciesEffect | (parkPolicies & (DistrictPolicies.Park.ImprovedLogistics | DistrictPolicies.Park.WorkSafety | DistrictPolicies.Park.AdvancedAutomation));
            if ((parkPolicies & DistrictPolicies.Park.ImprovedLogistics) != DistrictPolicies.Park.None)
            {
                int num12 = GetMaintenanceCost() / 100;
                num12 = finalProductionRate * num12 / 1000;
                if (num12 != 0)
                {
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Maintenance, num12, m_info.m_class);
                }
            }
            int num13 = m_outputRate1 + m_outputRate2;
            if ((parkPolicies & DistrictPolicies.Park.AdvancedAutomation) != DistrictPolicies.Park.None)
            {
                num13 = (num13 * 110 + 50) / 100;
                int num14 = GetMaintenanceCost() / 100;
                num14 = finalProductionRate * num14 / 1000;
                if (num14 != 0)
                {
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Maintenance, num14, m_info.m_class);
                }
            }
            if ((parkPolicies & DistrictPolicies.Park.WorkSafety) != DistrictPolicies.Park.None)
            {
                int num15 = (aliveWorkerCount + (int)((Singleton<SimulationManager>.instance.m_currentFrameIndex >> 8) & 15U)) / 16;
                if (num15 != 0)
                {
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, num15, m_info.m_class);
                }
            }
            var custom_buffers = CustomBuffersManager.GetCustomBuffer(buildingID);
            if (finalProductionRate != 0)
            {
                int num16 = m_pollutionAccumulation;
                if (b != 0)
                {
                    instance.m_parks.m_buffer[(int)b].GetProductionFactors(out int num17, out int num18);
                    finalProductionRate = (finalProductionRate * num17 + 50) / 100;
                    num16 = (num16 * num18 + 50) / 100;
                }
                else if (m_industryType != DistrictPark.ParkType.Industry)
                {
                    finalProductionRate = 0;
                }

                int InputBufferSize1 = 0;
                int CustomInputBuffer1 = 0;
                if (m_inputResource1 != TransferManager.TransferReason.None)
                {
                    InputBufferSize1 = GetInputBufferSize(ref buildingData, m_inputRate1);
                    CustomInputBuffer1 = (int)custom_buffers.m_customBuffer1;
                    int Input1ProductionRate = (m_inputRate1 * finalProductionRate + 99) / 100;
                    if (CustomInputBuffer1 < Input1ProductionRate)
                    {
                        finalProductionRate = (CustomInputBuffer1 * 100 + m_inputRate1 - 1) / m_inputRate1;
                        problemStruct = Notification.AddProblems(problemStruct, (!flag) ? ((!IsRawMaterial(m_inputResource1)) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources) : Notification.Problem1.NoFishingGoods);
                    }
                }
                int InputBufferSize2 = 0;
                int CustomInputBuffer2 = 0;
                if (m_inputResource2 != TransferManager.TransferReason.None)
                {
                    InputBufferSize2 = GetInputBufferSize(ref buildingData, m_inputRate2);
                    CustomInputBuffer2 = (int)custom_buffers.m_customBuffer2;
                    int Input2ProductionRate = (m_inputRate2 * finalProductionRate + 99) / 100;
                    if (CustomInputBuffer2 < Input2ProductionRate)
                    {
                        finalProductionRate = (CustomInputBuffer2 * 100 + m_inputRate2 - 1) / m_inputRate2;
                        problemStruct = Notification.AddProblems(problemStruct, (!flag) ? ((!IsRawMaterial(m_inputResource2)) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources) : Notification.Problem1.NoFishingGoods);
                    }
                }
                int OutputBufferSize1 = 0;
                int CustomOutputBuffer1 = 0;
                if (m_outputResource1 != TransferManager.TransferReason.None)
                {
                    OutputBufferSize1 = GetOutputBufferSize(ref buildingData, m_outputRate1, m_outputVehicleCount1);
                    CustomOutputBuffer1 = (int)custom_buffers.m_customBuffer3;
                    int OutputProductionRate = (num13 * finalProductionRate + 99) / 100;
                    if (OutputBufferSize1 - CustomOutputBuffer1 < OutputProductionRate)
                    {
                        OutputProductionRate = Mathf.Max(0, OutputBufferSize1 - CustomOutputBuffer1);
                        finalProductionRate = (OutputProductionRate * 100 + num13 - 1) / num13;
                        if (m_outputVehicleCount1 != 0)
                        {
                            problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.NoPlaceforGoods);
                            if (m_info.m_class.m_service == ItemClass.Service.PlayerIndustry)
                            {
                                GuideController properties = Singleton<GuideManager>.instance.m_properties;
                                if (properties != null)
                                {
                                    Singleton<BuildingManager>.instance.m_warehouseNeeded.Activate(properties.m_warehouseNeeded, buildingID);
                                }
                            }
                        }
                    }
                }
                int OutputBufferSize2 = 0;
                int CustomOutputBuffer2 = 0;
                if (m_outputResource2 != TransferManager.TransferReason.None)
                {
                    OutputBufferSize2 = GetOutputBufferSize(ref buildingData, m_outputRate2, m_outputVehicleCount2);
                    CustomOutputBuffer2 = (int)custom_buffers.m_customBuffer4;
                    int OutputProductionRate = (num13 * finalProductionRate + 99) / 100;
                    if (OutputBufferSize2 - CustomOutputBuffer2 < OutputProductionRate)
                    {
                        OutputProductionRate = Mathf.Max(0, OutputBufferSize2 - CustomOutputBuffer2);
                        finalProductionRate = (OutputProductionRate * 100 + num13 - 1) / num13;
                        if (m_outputVehicleCount2 != 0)
                        {
                            problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.NoPlaceforGoods);
                            if (m_info.m_class.m_service == ItemClass.Service.PlayerIndustry)
                            {
                                GuideController properties = Singleton<GuideManager>.instance.m_properties;
                                if (properties != null)
                                {
                                    Singleton<BuildingManager>.instance.m_warehouseNeeded.Activate(properties.m_warehouseNeeded, buildingID);
                                }
                            }
                        }
                    }
                }
                if (m_inputResource1 != TransferManager.TransferReason.None)
                {
                    int Input1ProductionRate = (m_inputRate1 * finalProductionRate + 99) / 100;
                    CustomInputBuffer1 = Mathf.Max(0, CustomInputBuffer1 - Input1ProductionRate);
                    custom_buffers.m_customBuffer1 = (ushort)CustomInputBuffer1;
                    instance.m_parks.m_buffer[b].AddConsumptionAmount(m_inputResource1, Input1ProductionRate);
                }
                if (m_inputResource2 != TransferManager.TransferReason.None)
                {
                    int Input2ProductionRate = (m_inputRate2 * finalProductionRate + 99) / 100;
                    CustomInputBuffer2 = Mathf.Max(0, CustomInputBuffer2 - Input2ProductionRate);
                    custom_buffers.m_customBuffer2 = (ushort)CustomInputBuffer2;
                    instance.m_parks.m_buffer[b].AddConsumptionAmount(m_inputResource2, Input2ProductionRate);
                }
                if (m_outputResource1 != TransferManager.TransferReason.None)
                {
                    int OutputProductionRate = (num13 * finalProductionRate + 99) / 100;
                    CustomOutputBuffer1 = Mathf.Min(OutputBufferSize1, CustomOutputBuffer1 + OutputProductionRate);
                    custom_buffers.m_customBuffer3 = (ushort)CustomOutputBuffer1;
                    instance.m_parks.m_buffer[b].AddProductionAmount(m_outputResource1, OutputProductionRate);
                }
                if (m_outputResource2 != TransferManager.TransferReason.None)
                {
                    int OutputProductionRate = (num13 * finalProductionRate + 99) / 100;
                    CustomOutputBuffer2 = Mathf.Min(OutputBufferSize2, CustomOutputBuffer2 + OutputProductionRate);
                    custom_buffers.m_customBuffer4 = (ushort)CustomOutputBuffer2;
                    instance.m_parks.m_buffer[b].AddProductionAmount(m_outputResource2, OutputProductionRate);
                }
                num16 = (finalProductionRate * num16 + 50) / 100;
                if (num16 != 0)
                {
                    num16 = UniqueFacultyAI.DecreaseByBonus(UniqueFacultyAI.FacultyBonus.Science, num16);
                    Singleton<NaturalResourceManager>.instance.TryDumpResource(NaturalResourceManager.Resource.Pollution, num16, num16, vector, m_pollutionRadius);
                }
                base.HandleDead2(buildingID, ref buildingData, ref behaviour, totalWorkerCount);
                if (b != 0 || m_industryType == DistrictPark.ParkType.Industry)
                {
                    int TempOutput = 0;
                    if (m_inputResource1 != TransferManager.TransferReason.None)
                    {
                        int count1 = 0;
                        int cargo1 = 0;
                        int capacity1 = 0;
                        int outside1 = 0;
                        base.CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource1, ref count1, ref cargo1, ref capacity1, ref outside1);
                        if (outside1 != 0)
                        {
                            TempOutput |= 1;
                        }
                        int InputSize1 = InputBufferSize1 - CustomInputBuffer1 - cargo1;
                        if (InputSize1 >= 4000)
                        {
                            TransferManager.TransferOffer transferOffer = default;
                            transferOffer.Priority = Mathf.Max(1, InputSize1 * 8 / InputBufferSize1);
                            transferOffer.Building = buildingID;
                            transferOffer.Position = buildingData.m_position;
                            transferOffer.Amount = 1;
                            transferOffer.Active = false;
                            Singleton<TransferManager>.instance.AddIncomingOffer(m_inputResource1, transferOffer);
                        }
                        instance.m_parks.m_buffer[b].AddBufferStatus(m_inputResource1, CustomInputBuffer1, cargo1, InputBufferSize1);
                    }
                    if (m_inputResource2 != TransferManager.TransferReason.None)
                    {
                        int count2 = 0;
                        int cargo2 = 0;
                        int capacity2 = 0;
                        int outside2 = 0;
                        base.CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource2, ref count2, ref cargo2, ref capacity2, ref outside2);
                        if (outside2 != 0)
                        {
                            TempOutput |= 2;
                        }
                        int InputSize2 = InputBufferSize2 - CustomInputBuffer2 - cargo2;
                        if (InputSize2 >= 4000)
                        {
                            TransferManager.TransferOffer transferOffer = default;
                            transferOffer.Priority = Mathf.Max(1, InputSize2 * 8 / InputBufferSize2);
                            transferOffer.Building = buildingID;
                            transferOffer.Position = buildingData.m_position;
                            transferOffer.Amount = 1;
                            transferOffer.Active = false;
                            Singleton<TransferManager>.instance.AddIncomingOffer(m_inputResource2, transferOffer);
                        }
                        instance.m_parks.m_buffer[b].AddBufferStatus(m_inputResource2, CustomInputBuffer2, cargo2, InputBufferSize2);
                    }
                    buildingData.m_tempImport |= (byte)TempOutput;
                    if (m_outputResource1 != TransferManager.TransferReason.None)
                    {
                        if (m_outputVehicleCount1 == 0)
			{
			    if (CustomOutputBuffer1 == OutputBufferSize1)
			    {
				int num42 = (CustomOutputBuffer1 * GetResourcePrice(m_outputResource1) + 50) / 100;
				if ((instance.m_districts.m_buffer[district].m_cityPlanningPolicies & DistrictPolicies.CityPlanning.SustainableFishing) != 0)
				{
				    num42 = (num42 * 105 + 99) / 100;
				}
				num42 = UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Science, num42);
				Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.ResourcePrice, num42, m_info.m_class);
                                CustomOutputBuffer1 = 0;
                                custom_buffers.m_customBuffer3 = (ushort)CustomOutputBuffer1;
				buildingData.m_tempExport = byte.MaxValue;
			    }
			}
                        else
                        {
                            int count1 = 0;
                            int cargo1 = 0;
                            int capacity1 = 0;
                            int outside1 = 0;
                            base.CalculateOwnVehicles(buildingID, ref buildingData, m_outputResource1, ref count1, ref cargo1, ref capacity1, ref outside1);
                            buildingData.m_tempExport = (byte)Mathf.Clamp(outside1, buildingData.m_tempExport, 255);
                            int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
                            int productionRate1 = GetProductionRate(100, budget);
                            int OutputProductionRate = (productionRate1 * m_outputVehicleCount1 + 99) / 100;
                            if (CustomOutputBuffer1 >= 8000 && count1 < OutputProductionRate)
                            {
                                TransferManager.TransferOffer transferOffer = default;
                                transferOffer.Priority = Mathf.Max(1, CustomOutputBuffer1 * 8 / OutputBufferSize1);
                                transferOffer.Building = buildingID;
                                transferOffer.Position = buildingData.m_position;
                                transferOffer.Amount = 1;
                                transferOffer.Active = true;
                                Singleton<TransferManager>.instance.AddOutgoingOffer(m_outputResource1, transferOffer);
                            }
                            instance.m_parks.m_buffer[b].AddBufferStatus(m_outputResource1, CustomOutputBuffer1, 0, OutputBufferSize1);
                        }
                    }
                    if (m_outputResource2 != TransferManager.TransferReason.None)
                    {
                        if (m_outputVehicleCount2 == 0)
                        {
                            if (CustomOutputBuffer2 == OutputBufferSize1)
                            {
                                int num42 = (CustomOutputBuffer2 * GetResourcePrice(m_outputResource2) + 50) / 100;
                                if ((instance.m_districts.m_buffer[district].m_cityPlanningPolicies & DistrictPolicies.CityPlanning.SustainableFishing) != 0)
                                {
                                    num42 = (num42 * 105 + 99) / 100;
                                }
                                num42 = UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Science, num42);
                                Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.ResourcePrice, num42, m_info.m_class);
                                CustomOutputBuffer2 = 0;
                                custom_buffers.m_customBuffer4 = (ushort)CustomOutputBuffer2;
                                buildingData.m_tempExport = byte.MaxValue;
                            }
                        }
                        else
                        {
                            int count2 = 0;
                            int cargo2 = 0;
                            int capacity2 = 0;
                            int outside2 = 0;
                            base.CalculateOwnVehicles(buildingID, ref buildingData, m_outputResource1, ref count2, ref cargo2, ref capacity2, ref outside2);
                            buildingData.m_tempExport = (byte)Mathf.Clamp(outside2, buildingData.m_tempExport, 255);
                            int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
                            int productionRate2 = GetProductionRate(100, budget);
                            int OutputProductionRate = (productionRate2 * m_outputVehicleCount1 + 99) / 100;
                            if (CustomOutputBuffer2 >= 8000 && count2 < OutputProductionRate)
                            {
                                TransferManager.TransferOffer transferOffer = default;
                                transferOffer.Priority = Mathf.Max(1, CustomOutputBuffer2 * 8 / OutputBufferSize2);
                                transferOffer.Building = buildingID;
                                transferOffer.Position = buildingData.m_position;
                                transferOffer.Amount = 1;
                                transferOffer.Active = true;
                                Singleton<TransferManager>.instance.AddOutgoingOffer(m_outputResource2, transferOffer);
                            }
                            instance.m_parks.m_buffer[b].AddBufferStatus(m_outputResource1, CustomOutputBuffer1, 0, OutputBufferSize1);
                        }
                    }
                }
                if (buildingData.m_finalImport != 0)
                {
                    District[] buffer2 = instance.m_districts.m_buffer;
                    byte b3 = district;
                    buffer2[(int)b3].m_playerConsumption.m_finalImportAmount = buffer2[(int)b3].m_playerConsumption.m_finalImportAmount + (uint)buildingData.m_finalImport;
                }
                if (buildingData.m_finalExport != 0)
                {
                    District[] buffer3 = instance.m_districts.m_buffer;
                    byte b4 = district;
                    buffer3[(int)b4].m_playerConsumption.m_finalExportAmount = buffer3[(int)b4].m_playerConsumption.m_finalExportAmount + (uint)buildingData.m_finalExport;
                }
                int num67 = finalProductionRate * m_noiseAccumulation / 100;
                if (num67 != 0)
                {
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, num67, vector, m_noiseRadius);
                }
            }
            buildingData.m_problems = problemStruct;
            custom_buffers.m_customBuffer5 = (byte)Mathf.Clamp(finalProductionRate * num13 / Mathf.Max(1, m_outputRate1), 0, 255);
            custom_buffers.m_customBuffer6 = (byte)Mathf.Clamp(finalProductionRate * num13 / Mathf.Max(1, m_outputRate2), 0, 255);
            buildingData.m_health = (byte)Mathf.Clamp(finalProductionRate, 0, 255);
            CustomBuffersManager.SetCustomBuffer(buildingID, custom_buffers);
            if (b != 0)
            {
                instance.m_parks.m_buffer[b].AddWorkers(aliveWorkerCount);
            }
            else if (m_industryType != DistrictPark.ParkType.Industry)
            {
                GuideController properties2 = Singleton<GuideManager>.instance.m_properties;
                if (properties2 != null)
                {
                    Singleton<BuildingManager>.instance.m_industryBuildingOutsideIndustryArea.Activate(properties2.m_industryBuildingOutsideIndustryArea, buildingID);
                }
            }
            base.ProduceGoods(buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
        }

        private void CheckCapacity(ushort buildingID, ref Building buildingData)
        {
            var custom_buffers = CustomBuffersManager.GetCustomBuffer(buildingID);
            int outputBufferSize = GetOutputBufferSize(ref buildingData, m_outputRate1, m_outputVehicleCount1) + GetOutputBufferSize(ref buildingData, m_outputRate2, m_outputVehicleCount2);
            float customBuffer = custom_buffers.m_customBuffer9 + custom_buffers.m_customBuffer10;
            if (customBuffer * 3 >= outputBufferSize * 2)
            {
                if ((buildingData.m_flags & Building.Flags.CapacityFull) != Building.Flags.CapacityFull)
                {
                    buildingData.m_flags |= Building.Flags.CapacityFull;
                }
            }
            else if (customBuffer * 3 >= outputBufferSize)
            {
                if ((buildingData.m_flags & Building.Flags.CapacityFull) != Building.Flags.CapacityStep2)
                {
                    buildingData.m_flags = (buildingData.m_flags & ~(Building.Flags.CapacityStep1 | Building.Flags.CapacityStep2)) | Building.Flags.CapacityStep2;
                }
            }
            else if (customBuffer >= m_outputRate1 * 2)
            {
                if ((buildingData.m_flags & Building.Flags.CapacityFull) != Building.Flags.CapacityStep1)
                {
                    buildingData.m_flags = (buildingData.m_flags & ~(Building.Flags.CapacityStep1 | Building.Flags.CapacityStep2)) | Building.Flags.CapacityStep1;
                }
            }
            else if ((buildingData.m_flags & Building.Flags.CapacityFull) != Building.Flags.None)
            {
                buildingData.m_flags &= ~(Building.Flags.CapacityStep1 | Building.Flags.CapacityStep2);
            }
        }

        public override string GetLocalizedTooltip()
        {
            string text_water = LocaleFormatter.FormatGeneric("AIINFO_WATER_CONSUMPTION", [GetWaterConsumption() * 16]) + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_ELECTRICITY_CONSUMPTION", [GetElectricityConsumption() * 16]);
            string text_prod = LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_PRODUCTION_RATE", [m_outputRate1 * 16, m_outputRate2 * 16]);
            if (m_outputResource1 != TransferManager.TransferReason.None && m_outputVehicleCount1 != 0)
            {
                text_prod = text_prod + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLE_COUNT", [m_outputVehicleCount1]);
            }
            if (m_outputResource2 != TransferManager.TransferReason.None && m_outputVehicleCount2 != 0)
            {
                text_prod = text_prod + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLE_COUNT", [m_outputVehicleCount2]);
            }
            string baseTooltip = TooltipHelper.Append(base.GetLocalizedTooltip(), TooltipHelper.Format(
            [
                LocaleFormatter.Info1,
                text_water,
                LocaleFormatter.Info2,
                text_prod,
            ]));
            bool flag1 = m_inputResource1 != TransferManager.TransferReason.None;
            string text1 = ((m_inputResource1 == TransferManager.TransferReason.None) ? string.Empty : MoreTransferReasons.Utils.AtlasUtils.GetSpriteName(m_inputResource1, false));
            bool flag2 = m_inputResource2 != TransferManager.TransferReason.None;
            string text2 = ((m_inputResource2 == TransferManager.TransferReason.None) ? string.Empty : MoreTransferReasons.Utils.AtlasUtils.GetSpriteName(m_inputResource2, false));
            bool flag3 = m_outputResource1 != TransferManager.TransferReason.None;
            string text3 = ((m_outputResource1 == TransferManager.TransferReason.None) ? string.Empty : MoreTransferReasons.Utils.AtlasUtils.GetSpriteName(m_outputResource1, false));
            bool flag4 = m_outputResource2 != TransferManager.TransferReason.None;
            string text4 = ((m_outputResource2 == TransferManager.TransferReason.None) ? string.Empty : MoreTransferReasons.Utils.AtlasUtils.GetSpriteName(m_outputResource2, false));

            string addTooltip = TooltipHelper.Format(
            [
                "arrowVisible",
                "true",
                "input1Visible",
                flag1.ToString(),
                "input2Visible",
                flag2.ToString(),
                "output1Visible",
                flag3.ToString(),
                "output2Visible",
                flag4.ToString()
            ]);
            string addTooltip2 = TooltipHelper.Format(
            [
                "input1",
                text1,
                "input2",
                text2,
                "output1",
                text3,
                "output2",
                text4
            ]);
            baseTooltip = TooltipHelper.Append(baseTooltip, addTooltip);
            return TooltipHelper.Append(baseTooltip, addTooltip2);
        }

        public override string GetLocalizedStats(ushort buildingID, ref Building data)
        {
            var custom_buffers = CustomBuffersManager.GetCustomBuffer(buildingID);
            int m_customBuffer5 = (int)custom_buffers.m_customBuffer5;
            int m_customBuffer6 = (int)custom_buffers.m_customBuffer6;
            int output1_production_rate = m_customBuffer5 * m_outputRate1 * 16 / 100;
            int output2_production_rate = m_customBuffer6 * m_outputRate2 * 16 / 100;
            string text = LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_PRODUCTION_RATE", [output1_production_rate, output2_production_rate]);
            if (m_outputResource1 != TransferManager.TransferReason.None && m_outputVehicleCount1 != 0)
            {
                int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
                int productionRate = GetProductionRate(100, budget);
                int vehicle_count = (productionRate * m_outputVehicleCount1 + 99) / 100;
                int count = 0;
                int capacity = 0;
                int cargo = 0;
                int outside = 0;
                base.CalculateOwnVehicles(buildingID, ref data, m_outputResource1, ref count, ref capacity, ref cargo, ref outside);
                text = text + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLES", [count, vehicle_count]);
            }
            if (m_outputResource2 != TransferManager.TransferReason.None && m_outputVehicleCount2 != 0)
            {
                int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
                int productionRate = GetProductionRate(100, budget);
                int vehicle_count = (productionRate * m_outputVehicleCount2 + 99) / 100;
                int count = 0;
                int capacity = 0;
                int cargo = 0;
                int outside = 0;
                base.CalculateOwnVehicles(buildingID, ref data, m_outputResource2, ref count, ref capacity, ref cargo, ref outside);
                text = text + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLES", [count, vehicle_count]);
            }
            return text;
        }

        public override bool RequireRoadAccess()
        {
            return base.RequireRoadAccess() || m_inputResource1 != TransferManager.TransferReason.None
                || m_inputResource2 != TransferManager.TransferReason.None || m_outputResource1 != TransferManager.TransferReason.None
                || m_outputResource2 != TransferManager.TransferReason.None;
        }

        public int GetInputBufferSize(ref Building data, int inputRate)
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
            int num = inputRate * 32 + 8000;
            if ((park.m_parkPolicies & DistrictPolicies.Park.ImprovedLogistics) != DistrictPolicies.Park.None)
            {
                num = (num * 6 + 4) / 5;
            }
            num = (num * (100 + m_finalStorageDelta) + 50) / 100;
            return Mathf.Clamp(num, 8000, 64000);
        }

        public int GetOutputBufferSize(ref Building data, int outputRate, int vehicleCount)
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
            if (vehicleCount == 0)
            {
                int num = outputRate * 100;
                return Mathf.Clamp(num, 1, 64000);
            }
            int num2 = outputRate * 32 + 8000;
            if ((park.m_parkPolicies & DistrictPolicies.Park.ImprovedLogistics) != DistrictPolicies.Park.None)
            {
                num2 = (num2 * 6 + 4) / 5;
            }
            num2 = (num2 * (100 + m_finalStorageDelta) + 50) / 100;
            return Mathf.Clamp(num2, 8000, 64000);
        }

        private bool IsRawMaterial(TransferManager.TransferReason material)
        {
            return material switch
            {
                TransferManager.TransferReason.Oil or TransferManager.TransferReason.Ore or TransferManager.TransferReason.Logs or TransferManager.TransferReason.Grain => true,
                _ => false,
            };
        }

    }
}