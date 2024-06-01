using System;
using ColossalFramework;
using ColossalFramework.DataBinding;
using UnityEngine;
using MoreTransferReasons;
using IndustriesMeetsSunsetHarbor.Managers;
using MoreTransferReasons.AI;
using System.Linq;


namespace IndustriesMeetsSunsetHarbor.AI
{
    public class MultiProcessingFacilityAI : IndustryBuildingAI, IExtendedBuildingAI
    {
        [CustomizableProperty("Input Resource Rate 1")]
        public int m_inputRate1;

        [CustomizableProperty("Input Resource Rate 2")]
        public int m_inputRate2;

        [CustomizableProperty("Input Resource Rate 3")]
        public int m_inputRate3;

        [CustomizableProperty("Input Resource Rate 4")]
        public int m_inputRate4;

        [CustomizableProperty("Output Resource Rate 1")]
        public int m_outputRate1 = 1000;

        [CustomizableProperty("Output Resource Rate 2")]
        public int m_outputRate2 = 1000;

        [CustomizableProperty("Output Resource Rate 3")]
        public int m_outputRate3 = 1000;

        [CustomizableProperty("Output Resource Rate 4")]
        public int m_outputRate4 = 1000;

        [CustomizableProperty("Output 1 Vehicle Count")]
        public int m_outputVehicleCount1 = 10;

        [CustomizableProperty("Output 2 Vehicle Count")]
        public int m_outputVehicleCount2 = 10;

        [CustomizableProperty("Output 3 Vehicle Count")]
        public int m_outputVehicleCount3 = 10;

        [CustomizableProperty("Output 4 Vehicle Count")]
        public int m_outputVehicleCount4 = 10;

        public int m_variationGroupID;

        [NonSerialized]
        private bool m_hasBufferStatusMeshes;


        [CustomizableProperty("Extended Input Resource 1")]
        public ExtendedTransferManager.TransferReason m_inputResource1 = ExtendedTransferManager.TransferReason.None;

        [CustomizableProperty("Extended Input Resource 2")]
        public ExtendedTransferManager.TransferReason m_inputResource2 = ExtendedTransferManager.TransferReason.None;

        [CustomizableProperty("Extended Input Resource 3")]
        public ExtendedTransferManager.TransferReason m_inputResource3 = ExtendedTransferManager.TransferReason.None;

        [CustomizableProperty("Extended Input Resource 4")]
        public ExtendedTransferManager.TransferReason m_inputResource4 = ExtendedTransferManager.TransferReason.None;

        [CustomizableProperty("Extended Output Resource 1")]
        public ExtendedTransferManager.TransferReason m_outputResource1 = ExtendedTransferManager.TransferReason.None;

        [CustomizableProperty("Extended Output Resource 2")]
        public ExtendedTransferManager.TransferReason m_outputResource2 = ExtendedTransferManager.TransferReason.None;

        [CustomizableProperty("Extended Output Resource 3")]
        public ExtendedTransferManager.TransferReason m_outputResource3 = ExtendedTransferManager.TransferReason.None;

        [CustomizableProperty("Extended Output Resource 4")]
        public ExtendedTransferManager.TransferReason m_outputResource4 = ExtendedTransferManager.TransferReason.None;

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
                            if (m_inputResource1 != ExtendedTransferManager.TransferReason.None && ((data.m_tempImport | data.m_finalImport) & 1) != 0)
                            {
                                return Singleton<ExtendedTransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource1];
                            }
                            if (m_inputResource2 != ExtendedTransferManager.TransferReason.None && ((data.m_tempImport | data.m_finalImport) & 2) != 0)
                            {
                                return Singleton<ExtendedTransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource2];
                            }
                            if (m_inputResource3 != ExtendedTransferManager.TransferReason.None && ((data.m_tempImport | data.m_finalImport) & 4) != 0)
                            {
                                return Singleton<ExtendedTransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource3];
                            }
                            if (m_inputResource4 != ExtendedTransferManager.TransferReason.None && ((data.m_tempImport | data.m_finalImport) & 8) != 0)
                            {
                                return Singleton<ExtendedTransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource4];
                            }
                            break;
                        case InfoManager.SubInfoMode.WaterPower:
                            {
                                if (m_outputResource1 != ExtendedTransferManager.TransferReason.None && (data.m_tempExport != 0 || data.m_finalExport != 0))
                                {
                                    return Singleton<ExtendedTransferManager>.instance.m_properties.m_resourceColors[(int)m_outputResource1];
                                }
                                if (m_outputResource2 != ExtendedTransferManager.TransferReason.None && (data.m_tempExport != 0 || data.m_finalExport != 0))
                                {
                                    return Singleton<ExtendedTransferManager>.instance.m_properties.m_resourceColors[(int)m_outputResource2];
                                }
                                if (m_outputResource3 != ExtendedTransferManager.TransferReason.None && (data.m_tempExport != 0 || data.m_finalExport != 0))
                                {
                                    return Singleton<ExtendedTransferManager>.instance.m_properties.m_resourceColors[(int)m_outputResource3];
                                }
                                if (m_outputResource4 != ExtendedTransferManager.TransferReason.None && (data.m_tempExport != 0 || data.m_finalExport != 0))
                                {
                                    return Singleton<ExtendedTransferManager>.instance.m_properties.m_resourceColors[(int)m_outputResource4];
                                }
                                if (ExtendedDistrictPark.IsPedestrianReason(m_outputResource1, out var index1))
                                {
                                    byte park = Singleton<DistrictManager>.instance.GetPark(data.m_position);
                                    if (park != 0 && Singleton<DistrictManager>.instance.m_parks.m_buffer[park].IsPedestrianZone && (Singleton<DistrictManager>.instance.m_parks.m_buffer[park].m_tempExport[index1] != 0 || Singleton<DistrictManager>.instance.m_parks.m_buffer[park].m_finalExport[index1] != 0))
                                    {
                                        return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_outputResource1];
                                    }
                                }
                                if (ExtendedDistrictPark.IsPedestrianReason(m_outputResource2, out var index2))
                                {
                                    byte park = Singleton<DistrictManager>.instance.GetPark(data.m_position);
                                    if (park != 0 && Singleton<DistrictManager>.instance.m_parks.m_buffer[park].IsPedestrianZone && (Singleton<DistrictManager>.instance.m_parks.m_buffer[park].m_tempExport[index2] != 0 || Singleton<DistrictManager>.instance.m_parks.m_buffer[park].m_finalExport[index2] != 0))
                                    {
                                        return Singleton<ExtendedTransferManager>.instance.m_properties.m_resourceColors[(int)m_outputResource2];
                                    }
                                }
                                if (ExtendedDistrictPark.IsPedestrianReason(m_outputResource3, out var index3))
                                {
                                    byte park = Singleton<DistrictManager>.instance.GetPark(data.m_position);
                                    if (park != 0 && Singleton<DistrictManager>.instance.m_parks.m_buffer[park].IsPedestrianZone && (Singleton<DistrictManager>.instance.m_parks.m_buffer[park].m_tempExport[index3] != 0 || Singleton<DistrictManager>.instance.m_parks.m_buffer[park].m_finalExport[index3] != 0))
                                    {
                                        return Singleton<ExtendedTransferManager>.instance.m_properties.m_resourceColors[(int)m_outputResource3];
                                    }
                                }
                                if (ExtendedDistrictPark.IsPedestrianReason(m_outputResource4, out var index4))
                                {
                                    byte park = Singleton<DistrictManager>.instance.GetPark(data.m_position);
                                    if (park != 0 && Singleton<DistrictManager>.instance.m_parks.m_buffer[park].IsPedestrianZone && (Singleton<DistrictManager>.instance.m_parks.m_buffer[park].m_tempExport[index4] != 0 || Singleton<DistrictManager>.instance.m_parks.m_buffer[park].m_finalExport[index4] != 0))
                                    {
                                        return Singleton<ExtendedTransferManager>.instance.m_properties.m_resourceColors[(int)m_outputResource3];
                                    }
                                }
                                break;
                            }
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                default:
                    return base.GetColor(buildingID, ref data, infoMode, subInfoMode);
            }
        }

        public override void GetPlacementInfoMode(out InfoManager.InfoMode mode, out InfoManager.SubInfoMode subMode, float elevation)
        {
            base.GetPlacementInfoMode(out mode, out subMode, elevation);
        }

        public override string GetDebugString(ushort buildingID, ref Building data)
        {
            var custom_buffers = CustomBuffersManager.GetCustomBuffer(buildingID);
            string text = base.GetDebugString(buildingID, ref data);
            if (m_inputResource1 != ExtendedTransferManager.TransferReason.None)
            {
                int inputBufferSize1 = GetInputBufferSize(ref data, m_inputRate1);
                int customBuffer1 = (int)custom_buffers.m_customBuffer1;
                int count1 = 0;
                int cargo1 = 0;
                int capacity1 = 0;
                int outside1 = 0;
                ExtendedVehicleManager.CalculateGuestVehicles(buildingID, ref data, m_inputResource1, ref count1, ref cargo1, ref capacity1, ref outside1);
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}",
                [
                    text,
                    m_inputResource1.ToString(),
                    customBuffer1,
                    cargo1,
                    inputBufferSize1
                ]);
            }
            if (m_inputResource2 != ExtendedTransferManager.TransferReason.None)
            {
                int inputBufferSize2 = GetInputBufferSize(ref data, m_inputRate2);
                int customBuffer2 = (int)custom_buffers.m_customBuffer2;
                int count2 = 0;
                int cargo2 = 0;
                int capacity2 = 0;
                int outside2 = 0;
                ExtendedVehicleManager.CalculateGuestVehicles(buildingID, ref data, m_inputResource2, ref count2, ref cargo2, ref capacity2, ref outside2);
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}",
                [
                    text,
                    m_inputResource2.ToString(),
                    customBuffer2,
                    cargo2,
                    inputBufferSize2
                ]);
            }
            if (m_inputResource3 != ExtendedTransferManager.TransferReason.None)
            {
                int inputBufferSize3 = GetInputBufferSize(ref data, m_inputRate3);
                int customBuffer3 = (int)custom_buffers.m_customBuffer3;
                int count3 = 0;
                int cargo3 = 0;
                int capacity3 = 0;
                int outside3 = 0;
                ExtendedVehicleManager.CalculateGuestVehicles(buildingID, ref data, m_inputResource3, ref count3, ref cargo3, ref capacity3, ref outside3);
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}",
                [
                    text,
                    m_inputResource3.ToString(),
                    customBuffer3,
                    cargo3,
                    inputBufferSize3
                ]);
            }
            if (m_inputResource4 != ExtendedTransferManager.TransferReason.None)
            {
                int inputBufferSize4 = GetInputBufferSize(ref data, m_inputRate4);
                int customBuffer4 = (int)custom_buffers.m_customBuffer8;
                int count4 = 0;
                int cargo4 = 0;
                int capacity4 = 0;
                int outside4 = 0;
                ExtendedVehicleManager.CalculateGuestVehicles(buildingID, ref data, m_inputResource4, ref count4, ref cargo4, ref capacity4, ref outside4);
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}",
                [
                    text,
                    m_inputResource4.ToString(),
                    customBuffer4,
                    cargo4,
                    inputBufferSize4
                ]);
            }
            if (m_outputResource1 != ExtendedTransferManager.TransferReason.None)
            {
                int outputBufferSize1 = GetOutputBufferSize(ref data, m_outputRate1, m_outputVehicleCount1);
                int outputBuffer1 = (int)custom_buffers.m_customBuffer5;
                int count9 = 0;
                int cargo9 = 0;
                int capacity9 = 0;
                int outside9 = 0;
                ExtendedVehicleManager.CalculateGuestVehicles(buildingID, ref data, m_outputResource1, ref count9, ref cargo9, ref capacity9, ref outside9);
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}",
                [
                    text,
                    m_outputResource1.ToString(),
                    outputBuffer1,
                    cargo9,
                    outputBufferSize1
                ]);
            }
            if (m_outputResource2 != ExtendedTransferManager.TransferReason.None)
            {
                int outputBufferSize2 = GetOutputBufferSize(ref data, m_outputRate2, m_outputVehicleCount2);
                int outputBuffer2 = (int)custom_buffers.m_customBuffer6;
                int count10 = 0;
                int cargo10 = 0;
                int capacity10 = 0;
                int outside10 = 0;
                ExtendedVehicleManager.CalculateGuestVehicles(buildingID, ref data, m_outputResource2, ref count10, ref cargo10, ref capacity10, ref outside10);
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}",
                [
                    text,
                    m_outputResource2.ToString(),
                    outputBuffer2,
                    cargo10,
                    outputBufferSize2
                ]);
            }
            if (m_outputResource3 != ExtendedTransferManager.TransferReason.None)
            {
                int outputBufferSize3 = GetOutputBufferSize(ref data, m_outputRate3, m_outputVehicleCount3);
                int outputBuffer3 = (int)custom_buffers.m_customBuffer7;
                int count11 = 0;
                int cargo11 = 0;
                int capacity11 = 0;
                int outside11 = 0;
                ExtendedVehicleManager.CalculateGuestVehicles(buildingID, ref data, m_outputResource3, ref count11, ref cargo11, ref capacity11, ref outside11);
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}",
                [
                    text,
                    m_outputResource3.ToString(),
                    outputBuffer3,
                    cargo11,
                    outputBufferSize3
                ]);
            }
            if (m_outputResource4 != ExtendedTransferManager.TransferReason.None)
            {
                int outputBufferSize4 = GetOutputBufferSize(ref data, m_outputRate4, m_outputVehicleCount4);
                int outputBuffer4 = (int)custom_buffers.m_customBuffer8;
                int count12 = 0;
                int cargo12 = 0;
                int capacity12 = 0;
                int outside12 = 0;
                ExtendedVehicleManager.CalculateGuestVehicles(buildingID, ref data, m_outputResource4, ref count12, ref cargo12, ref capacity12, ref outside12);
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}",
                [
                    text,
                    m_outputResource4.ToString(),
                    outputBuffer4,
                    cargo12,
                    outputBufferSize4
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
            var custom_buffers = CustomBuffersManager.GetCustomBuffer(buildingID);
            custom_buffers.m_customBuffer10 = data.m_productionRate;
            custom_buffers.m_customBuffer11 = data.m_productionRate;
            custom_buffers.m_customBuffer12 = data.m_productionRate;
            custom_buffers.m_customBuffer13 = data.m_productionRate;
            CustomBuffersManager.SetCustomBuffer(buildingID, custom_buffers);
        }

        public override void BuildingLoaded(ushort buildingID, ref Building data, uint version)
        {
            base.BuildingLoaded(buildingID, ref data, version);
            var custom_buffers = CustomBuffersManager.GetCustomBuffer(buildingID);
            custom_buffers.m_customBuffer10 = data.m_productionRate;
            custom_buffers.m_customBuffer11 = data.m_productionRate;
            custom_buffers.m_customBuffer12 = data.m_productionRate;
            custom_buffers.m_customBuffer13 = data.m_productionRate;
            CustomBuffersManager.SetCustomBuffer(buildingID, custom_buffers);
        }

        void IExtendedBuildingAI.ExtendedGetMaterialAmount(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, out int amount, out int max)
        {
            amount = 0;
            max = 0;
        }

        void IExtendedBuildingAI.ExtendedStartTransfer(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, ExtendedTransferManager.Offer offer)
        {
            if (material == m_outputResource1 || material == m_outputResource2 || material == m_outputResource3 || material == m_outputResource4)
            {
                VehicleInfo transferVehicleService = ExtendedWarehouseAI.GetExtendedTransferVehicleService(material, ItemClass.Level.Level1, ref Singleton<SimulationManager>.instance.m_randomizer);
                if (transferVehicleService == null)
                {
                    return;
                }
                if (transferVehicleService != null)
                {
                    Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
                    byte transferType = (byte)(material + 200);
                    if (ExtendedVehicleManager.CreateVehicle(out ushort vehicle, ref Singleton<SimulationManager>.instance.m_randomizer, transferVehicleService, data.m_position, transferType, false, true) && transferVehicleService.m_vehicleAI is ExtendedCargoTruckAI cargoTruckAI)
                    {
                        vehicles.m_buffer[(int)vehicle].m_gateIndex = (byte)m_variationGroupID;
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

        void IExtendedBuildingAI.ExtendedModifyMaterialBuffer(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, ref int amountDelta)
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
            else if (material == m_inputResource3)
            {
                int inputBufferSize3 = GetInputBufferSize(ref data, m_inputRate3);
                int m_customBuffer3 = (int)custom_buffers.m_customBuffer3;
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer3, inputBufferSize3 - m_customBuffer3);
                m_customBuffer3 += amountDelta;
                custom_buffers.m_customBuffer3 = (ushort)m_customBuffer3;
            }
            else if (material == m_inputResource4)
            {
                int inputBufferSize4 = GetInputBufferSize(ref data, m_inputRate4);
                int m_customBuffer4 = (int)custom_buffers.m_customBuffer4;
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer4, inputBufferSize4 - m_customBuffer4);
                m_customBuffer4 += amountDelta;
                custom_buffers.m_customBuffer4 = (ushort)m_customBuffer4;
            }
            else if (material == m_outputResource1)
            {
                int outputBufferSize1 = GetOutputBufferSize(ref data, m_outputRate1, m_outputVehicleCount1);
                int m_customBuffer5 = (int)custom_buffers.m_customBuffer5;
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer5, outputBufferSize1 - m_customBuffer5);
                m_customBuffer5 += amountDelta;
                custom_buffers.m_customBuffer5 = (ushort)m_customBuffer5;
            }
            else if (material == m_outputResource2)
            {
                int outputBufferSize2 = GetOutputBufferSize(ref data, m_outputRate2, m_outputVehicleCount2);
                int m_customBuffer6 = (int)custom_buffers.m_customBuffer6;
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer6, outputBufferSize2 - m_customBuffer6);
                m_customBuffer6 += amountDelta;
                custom_buffers.m_customBuffer6 = (ushort)m_customBuffer6;
            }
            else if (material == m_outputResource3)
            {
                int outputBufferSize3 = GetOutputBufferSize(ref data, m_outputRate3, m_outputVehicleCount3);
                int m_customBuffer7 = (int)custom_buffers.m_customBuffer7;
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer7, outputBufferSize3 - m_customBuffer7);
                m_customBuffer7 += amountDelta;
                custom_buffers.m_customBuffer7 = (ushort)m_customBuffer7;
            }
            else if (material == m_outputResource4)
            {
                int outputBufferSize4 = GetOutputBufferSize(ref data, m_outputRate4, m_outputVehicleCount4);
                int m_customBuffer8 = (int)custom_buffers.m_customBuffer8;
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer8, outputBufferSize4 - m_customBuffer8);
                m_customBuffer8 += amountDelta;
                custom_buffers.m_customBuffer8 = (ushort)m_customBuffer8;
            }
            CustomBuffersManager.SetCustomBuffer(buildingID, custom_buffers);
        }

        public override void BuildingDeactivated(ushort buildingID, ref Building data)
        {
            ExtendedTransferManager.Offer offer = default;
            offer.Building = buildingID;
            if (m_inputResource1 != ExtendedTransferManager.TransferReason.None)
            {
                Singleton<ExtendedTransferManager>.instance.RemoveIncomingOffer(m_inputResource1, offer);
            }
            if (m_inputResource2 != ExtendedTransferManager.TransferReason.None)
            {
                Singleton<ExtendedTransferManager>.instance.RemoveIncomingOffer(m_inputResource2, offer);
            }
            if (m_inputResource3 != ExtendedTransferManager.TransferReason.None)
            {
                Singleton<ExtendedTransferManager>.instance.RemoveIncomingOffer(m_inputResource3, offer);
            }
            if (m_inputResource4 != ExtendedTransferManager.TransferReason.None)
            {
                Singleton<ExtendedTransferManager>.instance.RemoveIncomingOffer(m_inputResource4, offer);
            }
            if (m_outputResource1 != ExtendedTransferManager.TransferReason.None)
            {
                Singleton<ExtendedTransferManager>.instance.RemoveOutgoingOffer(m_outputResource1, offer);
            }
            if (m_outputResource2 != ExtendedTransferManager.TransferReason.None)
            {
                Singleton<ExtendedTransferManager>.instance.RemoveOutgoingOffer(m_outputResource2, offer);
            }
            if (m_outputResource3 != ExtendedTransferManager.TransferReason.None)
            {
                Singleton<ExtendedTransferManager>.instance.RemoveOutgoingOffer(m_outputResource3, offer);
            }
            if (m_outputResource4 != ExtendedTransferManager.TransferReason.None)
            {
                Singleton<ExtendedTransferManager>.instance.RemoveOutgoingOffer(m_outputResource4, offer);
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

        public void ProduceGoods1(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            ExtendedDistrictManager instance2 = Singleton<ExtendedDistrictManager>.instance;
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
            int num13 = m_outputRate1;
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
            base.HandleDead2(buildingID, ref buildingData, ref behaviour, totalWorkerCount);
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
                int CustomBuffer1 = 0;
                if (m_inputResource1 != ExtendedTransferManager.TransferReason.None)
                {
                    InputBufferSize1 = GetInputBufferSize(ref buildingData, m_inputRate1);
                    CustomBuffer1 = (int)custom_buffers.m_customBuffer1;
                    int Input1ProductionRate = (m_inputRate1 * finalProductionRate + 99) / 100;
                    if (CustomBuffer1 < Input1ProductionRate)
                    {
                        finalProductionRate = (CustomBuffer1 * 100 + m_inputRate1 - 1) / m_inputRate1;
                        problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.NoInputProducts);
                    }
                }
                int OutputBufferSize1 = 0;
                int CustomBuffer5 = 0;
                if (m_outputResource1 != ExtendedTransferManager.TransferReason.None)
                {
                    OutputBufferSize1 = GetOutputBufferSize(ref buildingData, m_outputRate1, m_outputVehicleCount1);
                    CustomBuffer5 = (int)custom_buffers.m_customBuffer5;
                    int OutputProductionRate1 = (num13 * finalProductionRate + 99) / 100;
                    if (OutputBufferSize1 - CustomBuffer5 < OutputProductionRate1)
                    {
                        OutputProductionRate1 = Mathf.Max(0, OutputBufferSize1 - CustomBuffer5);
                        finalProductionRate = (OutputProductionRate1 * 100 + num13 - 1) / num13;
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


                if (m_inputResource1 != ExtendedTransferManager.TransferReason.None)
                {
                    int Input1ProductionRate = (m_inputRate1 * finalProductionRate + 99) / 100;
                    CustomBuffer1 = Mathf.Max(0, CustomBuffer1 - Input1ProductionRate);
                    custom_buffers.m_customBuffer1 = (ushort)CustomBuffer1;
                    instance2.m_industryParks.m_buffer[b].AddConsumptionAmount(m_inputResource1, Input1ProductionRate);
                }
                if (m_outputResource1 != ExtendedTransferManager.TransferReason.None)
                {
                    int OutputProductionRate1 = (num13 * finalProductionRate + 99) / 100;
                    CustomBuffer5 = Mathf.Min(OutputBufferSize1, CustomBuffer5 + OutputProductionRate1);
                    custom_buffers.m_customBuffer5 = (ushort)CustomBuffer5;
                    instance2.m_industryParks.m_buffer[b].AddProductionAmount(instance.m_parks.m_buffer[b], m_outputResource1, OutputProductionRate1);
                }
                CustomBuffersManager.SetCustomBuffer(buildingID, custom_buffers);
                int num19 = (finalProductionRate * num16 + 50) / 100;
                if (num19 != 0)
                {
                    num19 = UniqueFacultyAI.DecreaseByBonus(UniqueFacultyAI.FacultyBonus.Science, num19);
                    Singleton<NaturalResourceManager>.instance.TryDumpResource(NaturalResourceManager.Resource.Pollution, num19, num19, vector, m_pollutionRadius);
                }

                if (b != 0 || m_industryType == DistrictPark.ParkType.Industry)
                {
                    int TempOutput1 = 0;
                    if (m_inputResource1 != ExtendedTransferManager.TransferReason.None)
                    {
                        int count1 = 0;
                        int cargo1 = 0;
                        int capacity1 = 0;
                        int outside1 = 0;
                        ExtendedVehicleManager.CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource1, ref count1, ref cargo1, ref capacity1, ref outside1);
                        if (outside1 != 0)
                        {
                            TempOutput1 |= 1;
                        }
                        int InputSize1 = InputBufferSize1 - CustomBuffer1 - cargo1;
                        if (InputSize1 >= 4000)
                        {
                            ExtendedTransferManager.Offer transferOffer = default;
                            transferOffer.Building = buildingID;
                            transferOffer.Position = buildingData.m_position;
                            transferOffer.Amount = 1;
                            transferOffer.Active = false;
                            Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(m_inputResource1, transferOffer);
                        }
                        instance2.m_industryParks.m_buffer[b].AddBufferStatus(m_inputResource1, CustomBuffer1, cargo1, InputBufferSize1);
                    }
                    buildingData.m_tempImport |= (byte)TempOutput1;

                    int TempExport1 = 0;
                    if (m_outputResource1 != ExtendedTransferManager.TransferReason.None)
                    {
                        if (m_outputVehicleCount1 == 0)
                        {
                            if (CustomBuffer5 == OutputBufferSize1)
                            {
                                int value = (CustomBuffer5 * IndustryBuildingManager.GetExtendedResourcePrice(m_outputResource1) + 50) / 100;
                                if ((instance.m_districts.m_buffer[district].m_cityPlanningPolicies & DistrictPolicies.CityPlanning.SustainableFishing) != 0)
                                {
                                    value = (value * 105 + 99) / 100;
                                }
                                int amount = UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Science, value);
                                Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.ResourcePrice, amount, m_info.m_class);
                                CustomBuffer5 = 0;
                                custom_buffers.m_customBuffer5 = (ushort)CustomBuffer5;
                                TempExport1 = byte.MaxValue;
                            }
                        }
                        else
                        {
                            int count1 = 0;
                            int cargo1 = 0;
                            int capacity1 = 0;
                            int outside1 = 0;
                            ExtendedVehicleManager.CalculateOwnVehicles(buildingID, ref buildingData, m_outputResource1, ref count1, ref cargo1, ref capacity1, ref outside1);
                            TempExport1 = (byte)Mathf.Clamp(outside1, buildingData.m_tempExport, 255);
                            int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
                            int productionRate1 = PlayerBuildingAI.GetProductionRate(100, budget);
                            int OutputProductionRate1 = (productionRate1 * m_outputVehicleCount1 + 99) / 100;
                            if (CustomBuffer5 >= 8000 && count1 < OutputProductionRate1)
                            {
                                ExtendedTransferManager.Offer transferOffer = default;
                                transferOffer.Building = buildingID;
                                transferOffer.Position = buildingData.m_position;
                                transferOffer.Amount = 1;
                                transferOffer.Active = true;
                                Singleton<ExtendedTransferManager>.instance.AddOutgoingOffer(m_outputResource1, transferOffer);
                            }
                            instance2.m_industryParks.m_buffer[b].AddBufferStatus(m_outputResource1, CustomBuffer5, cargo1, OutputBufferSize1);
                        }
                    }
                    buildingData.m_tempExport = (byte)TempExport1;
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

                buildingData.m_problems = problemStruct;
                custom_buffers.m_customBuffer9 = (byte)Mathf.Clamp(finalProductionRate * num13 / Mathf.Max(1, m_outputRate1), 0, 255);
                buildingData.m_health = (byte)Mathf.Clamp(finalProductionRate, 0, 255);
                CustomBuffersManager.SetCustomBuffer(buildingID, custom_buffers);
                if (b != 0)
                {
                    instance.m_parks.m_buffer[(int)b].AddWorkers(aliveWorkerCount);
                }
                else if (m_industryType != DistrictPark.ParkType.Industry)
                {
                    GuideController properties2 = Singleton<GuideManager>.instance.m_properties;
                    if (properties2 != null)
                    {
                        Singleton<BuildingManager>.instance.m_industryBuildingOutsideIndustryArea.Activate(properties2.m_industryBuildingOutsideIndustryArea, buildingID);
                    }
                }
                if (finalProductionRate != 0)
                {
                    buildingData.m_flags |= Building.Flags.Active;
                }
            }
        }

        public void ProduceGoods2(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            ExtendedDistrictManager instance2 = Singleton<ExtendedDistrictManager>.instance;
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
            int num13 = m_outputRate2;
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
            base.HandleDead2(buildingID, ref buildingData, ref behaviour, totalWorkerCount);
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

                int InputBufferSize2 = 0;
                int CustomBuffer2 = 0;
                if (m_inputResource2 != ExtendedTransferManager.TransferReason.None)
                {
                    InputBufferSize2 = GetInputBufferSize(ref buildingData, m_inputRate2);
                    CustomBuffer2 = (int)custom_buffers.m_customBuffer2;
                    int Input2ProductionRate = (m_inputRate2 * finalProductionRate + 99) / 100;
                    if (CustomBuffer2 < Input2ProductionRate)
                    {
                        finalProductionRate = (CustomBuffer2 * 100 + m_inputRate2 - 1) / m_inputRate2;
                        problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.NoInputProducts);
                    }
                }
                int OutputBufferSize2 = 0;
                int CustomBuffer6 = 0;
                if (m_outputResource2 != ExtendedTransferManager.TransferReason.None)
                {
                    OutputBufferSize2 = GetOutputBufferSize(ref buildingData, m_outputRate2, m_outputVehicleCount2);
                    CustomBuffer6 = (int)custom_buffers.m_customBuffer6;
                    int OutputProductionRate2 = (num13 * finalProductionRate + 99) / 100;
                    if (OutputBufferSize2 - CustomBuffer6 < OutputProductionRate2)
                    {
                        OutputProductionRate2 = Mathf.Max(0, OutputBufferSize2 - CustomBuffer6);
                        finalProductionRate = (OutputProductionRate2 * 100 + num13 - 1) / num13;
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

                if (m_inputResource2 != ExtendedTransferManager.TransferReason.None)
                {
                    int Input2ProductionRate = (m_inputRate2 * finalProductionRate + 99) / 100;
                    CustomBuffer2 = Mathf.Max(0, CustomBuffer2 - Input2ProductionRate);
                    custom_buffers.m_customBuffer2 = (ushort)CustomBuffer2;
                    instance2.m_industryParks.m_buffer[b].AddConsumptionAmount(m_inputResource2, Input2ProductionRate);
                }
                if (m_outputResource2 != ExtendedTransferManager.TransferReason.None)
                {
                    int OutputProductionRate2 = (num13 * finalProductionRate + 99) / 100;
                    CustomBuffer6 = Mathf.Min(OutputBufferSize2, CustomBuffer6 + OutputProductionRate2);
                    custom_buffers.m_customBuffer6 = (ushort)CustomBuffer6;
                    instance2.m_industryParks.m_buffer[b].AddProductionAmount(instance.m_parks.m_buffer[b], m_outputResource2, OutputProductionRate2);
                }
                CustomBuffersManager.SetCustomBuffer(buildingID, custom_buffers);
                int num19 = (finalProductionRate * num16 + 50) / 100;
                if (num19 != 0)
                {
                    num19 = UniqueFacultyAI.DecreaseByBonus(UniqueFacultyAI.FacultyBonus.Science, num19);
                    Singleton<NaturalResourceManager>.instance.TryDumpResource(NaturalResourceManager.Resource.Pollution, num19, num19, vector, m_pollutionRadius);
                }

                if (b != 0 || m_industryType == DistrictPark.ParkType.Industry)
                {
                    int TempOutput2 = 0;
                    if (m_inputResource2 != ExtendedTransferManager.TransferReason.None)
                    {
                        int count2 = 0;
                        int cargo2 = 0;
                        int capacity2 = 0;
                        int outside2 = 0;
                        ExtendedVehicleManager.CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource2, ref count2, ref cargo2, ref capacity2, ref outside2);
                        if (outside2 != 0)
                        {
                            TempOutput2 |= 1;
                        }
                        int InputSize2 = InputBufferSize2 - CustomBuffer2 - cargo2;
                        if (InputSize2 >= 4000)
                        {
                            ExtendedTransferManager.Offer transferOffer = default;
                            transferOffer.Building = buildingID;
                            transferOffer.Position = buildingData.m_position;
                            transferOffer.Amount = 1;
                            transferOffer.Active = false;
                            Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(m_inputResource2, transferOffer);
                        }
                        instance2.m_industryParks.m_buffer[b].AddBufferStatus(m_inputResource2, CustomBuffer2, cargo2, InputBufferSize2);
                    }
                    buildingData.m_tempImport |= (byte)TempOutput2;

                    int TempExport2 = 0;
                    if (m_outputResource2 != ExtendedTransferManager.TransferReason.None)
                    {
                        if (m_outputVehicleCount2 == 0)
                        {
                            if (CustomBuffer6 == OutputBufferSize2)
                            {
                                int value = (CustomBuffer6 * IndustryBuildingManager.GetExtendedResourcePrice(m_outputResource2) + 50) / 100;
                                if ((instance.m_districts.m_buffer[district].m_cityPlanningPolicies & DistrictPolicies.CityPlanning.SustainableFishing) != 0)
                                {
                                    value = (value * 105 + 99) / 100;
                                }
                                int amount = UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Science, value);
                                Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.ResourcePrice, amount, m_info.m_class);
                                CustomBuffer6 = 0;
                                custom_buffers.m_customBuffer6 = (ushort)CustomBuffer6;
                                TempExport2 = byte.MaxValue;
                            }
                        }
                        else
                        {
                            int count2 = 0;
                            int cargo2 = 0;
                            int capacity2 = 0;
                            int outside2 = 0;
                            ExtendedVehicleManager.CalculateOwnVehicles(buildingID, ref buildingData, m_outputResource1, ref count2, ref cargo2, ref capacity2, ref outside2);
                            TempExport2 = (byte)Mathf.Clamp(outside2, buildingData.m_tempExport, 255);
                            int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
                            int productionRate2 = PlayerBuildingAI.GetProductionRate(100, budget);
                            int OutputProductionRate2 = (productionRate2 * m_outputVehicleCount2 + 99) / 100;
                            if (CustomBuffer6 >= 8000 && count2 < OutputProductionRate2)
                            {
                                ExtendedTransferManager.Offer transferOffer = default;
                                transferOffer.Building = buildingID;
                                transferOffer.Position = buildingData.m_position;
                                transferOffer.Amount = 1;
                                transferOffer.Active = true;
                                Singleton<ExtendedTransferManager>.instance.AddOutgoingOffer(m_outputResource2, transferOffer);
                            }
                            instance2.m_industryParks.m_buffer[b].AddBufferStatus(m_outputResource2, CustomBuffer6, cargo2, OutputBufferSize2);
                        }
                    }
                    buildingData.m_tempExport = (byte)TempExport2;
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

                buildingData.m_problems = problemStruct;
                custom_buffers.m_customBuffer9 = (byte)Mathf.Clamp(finalProductionRate * num13 / Mathf.Max(1, m_outputRate1), 0, 255);
                buildingData.m_health = (byte)Mathf.Clamp(finalProductionRate, 0, 255);
                CustomBuffersManager.SetCustomBuffer(buildingID, custom_buffers);
                if (b != 0)
                {
                    instance.m_parks.m_buffer[(int)b].AddWorkers(aliveWorkerCount);
                }
                else if (m_industryType != DistrictPark.ParkType.Industry)
                {
                    GuideController properties2 = Singleton<GuideManager>.instance.m_properties;
                    if (properties2 != null)
                    {
                        Singleton<BuildingManager>.instance.m_industryBuildingOutsideIndustryArea.Activate(properties2.m_industryBuildingOutsideIndustryArea, buildingID);
                    }
                }
                if (finalProductionRate != 0)
                {
                    buildingData.m_flags |= Building.Flags.Active;
                }
            }
        }

        public void ProduceGoods3(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            ExtendedDistrictManager instance2 = Singleton<ExtendedDistrictManager>.instance;
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
            int num13 = m_outputRate3;
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
            base.HandleDead2(buildingID, ref buildingData, ref behaviour, totalWorkerCount);
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

                int InputBufferSize3 = 0;
                int CustomBuffer3 = 0;
                if (m_inputResource3 != ExtendedTransferManager.TransferReason.None)
                {
                    InputBufferSize3 = GetInputBufferSize(ref buildingData, m_inputRate3);
                    CustomBuffer3 = (int)custom_buffers.m_customBuffer3;
                    int Input3ProductionRate = (m_inputRate3 * finalProductionRate + 99) / 100;
                    if (CustomBuffer3 < Input3ProductionRate)
                    {
                        finalProductionRate = (CustomBuffer3 * 100 + m_inputRate3 - 1) / m_inputRate3;
                        problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.NoInputProducts);
                    }
                }
                int OutputBufferSize3 = 0;
                int CustomBuffer7 = 0;
                if (m_outputResource3 != ExtendedTransferManager.TransferReason.None)
                {
                    OutputBufferSize3 = GetOutputBufferSize(ref buildingData, m_outputRate3, m_outputVehicleCount3);
                    CustomBuffer7 = (int)custom_buffers.m_customBuffer7;
                    int OutputProductionRate3 = (num13 * finalProductionRate + 99) / 100;
                    if (OutputBufferSize3 - CustomBuffer7 < OutputProductionRate3)
                    {
                        OutputProductionRate3 = Mathf.Max(0, OutputBufferSize3 - CustomBuffer7);
                        finalProductionRate = (OutputProductionRate3 * 100 + num13 - 1) / num13;
                        if (m_outputVehicleCount3 != 0)
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

                if (m_inputResource3 != ExtendedTransferManager.TransferReason.None)
                {
                    int Input3ProductionRate = (m_inputRate3 * finalProductionRate + 99) / 100;
                    CustomBuffer3 = Mathf.Max(0, CustomBuffer3 - Input3ProductionRate);
                    custom_buffers.m_customBuffer3 = (ushort)CustomBuffer3;
                    instance2.m_industryParks.m_buffer[b].AddConsumptionAmount(m_inputResource3, Input3ProductionRate);
                }
                if (m_outputResource3 != ExtendedTransferManager.TransferReason.None)
                {
                    int OutputProductionRate3 = (num13 * finalProductionRate + 99) / 100;
                    CustomBuffer7 = Mathf.Min(OutputBufferSize3, CustomBuffer7 + OutputProductionRate3);
                    custom_buffers.m_customBuffer7 = (ushort)CustomBuffer7;
                    instance2.m_industryParks.m_buffer[b].AddProductionAmount(instance.m_parks.m_buffer[b], m_outputResource3, OutputProductionRate3);
                }
                CustomBuffersManager.SetCustomBuffer(buildingID, custom_buffers);
                int num19 = (finalProductionRate * num16 + 50) / 100;
                if (num19 != 0)
                {
                    num19 = UniqueFacultyAI.DecreaseByBonus(UniqueFacultyAI.FacultyBonus.Science, num19);
                    Singleton<NaturalResourceManager>.instance.TryDumpResource(NaturalResourceManager.Resource.Pollution, num19, num19, vector, m_pollutionRadius);
                }

                if (b != 0 || m_industryType == DistrictPark.ParkType.Industry)
                {
                    int TempOutput3 = 0;
                    if (m_inputResource3 != ExtendedTransferManager.TransferReason.None)
                    {
                        int count3 = 0;
                        int cargo3 = 0;
                        int capacity3 = 0;
                        int outside3 = 0;
                        ExtendedVehicleManager.CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource1, ref count3, ref cargo3, ref capacity3, ref outside3);
                        if (outside3 != 0)
                        {
                            TempOutput3 |= 1;
                        }
                        int InputSize3 = InputBufferSize3 - CustomBuffer3 - cargo3;
                        if (InputSize3 >= 4000)
                        {
                            ExtendedTransferManager.Offer transferOffer = default;
                            transferOffer.Building = buildingID;
                            transferOffer.Position = buildingData.m_position;
                            transferOffer.Amount = 1;
                            transferOffer.Active = false;
                            Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(m_inputResource3, transferOffer);
                        }
                        instance2.m_industryParks.m_buffer[b].AddBufferStatus(m_inputResource3, CustomBuffer3, cargo3, InputBufferSize3);
                    }
                    buildingData.m_tempImport |= (byte)TempOutput3;

                    int TempExport3 = 0;
                    if (m_outputResource3 != ExtendedTransferManager.TransferReason.None)
                    {
                        if (m_outputVehicleCount3 == 0)
                        {
                            if (CustomBuffer7 == OutputBufferSize3)
                            {
                                int value = (CustomBuffer7 * IndustryBuildingManager.GetExtendedResourcePrice(m_outputResource3) + 50) / 100;
                                if ((instance.m_districts.m_buffer[district].m_cityPlanningPolicies & DistrictPolicies.CityPlanning.SustainableFishing) != 0)
                                {
                                    value = (value * 105 + 99) / 100;
                                }
                                int amount = UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Science, value);
                                Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.ResourcePrice, amount, m_info.m_class);
                                CustomBuffer7 = 0;
                                custom_buffers.m_customBuffer7 = (ushort)CustomBuffer7;
                                TempExport3 = byte.MaxValue;
                            }
                        }
                        else
                        {
                            int count3 = 0;
                            int cargo3 = 0;
                            int capacity3 = 0;
                            int outside3 = 0;
                            ExtendedVehicleManager.CalculateOwnVehicles(buildingID, ref buildingData, m_outputResource1, ref count3, ref cargo3, ref capacity3, ref outside3);
                            TempExport3 = (byte)Mathf.Clamp(outside3, buildingData.m_tempExport, 255);
                            int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
                            int productionRate3 = PlayerBuildingAI.GetProductionRate(100, budget);
                            int OutputProductionRate3 = (productionRate3 * m_outputVehicleCount3 + 99) / 100;
                            if (CustomBuffer7 >= 8000 && count3 < OutputProductionRate3)
                            {
                                ExtendedTransferManager.Offer transferOffer = default;
                                transferOffer.Building = buildingID;
                                transferOffer.Position = buildingData.m_position;
                                transferOffer.Amount = 1;
                                transferOffer.Active = true;
                                Singleton<ExtendedTransferManager>.instance.AddOutgoingOffer(m_outputResource3, transferOffer);
                            }
                            instance2.m_industryParks.m_buffer[b].AddBufferStatus(m_outputResource3, CustomBuffer7, cargo3, OutputBufferSize3);
                        }
                    }
                    buildingData.m_tempExport = (byte)TempExport3;
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

                buildingData.m_problems = problemStruct;
                custom_buffers.m_customBuffer9 = (byte)Mathf.Clamp(finalProductionRate * num13 / Mathf.Max(1, m_outputRate1), 0, 255);
                buildingData.m_health = (byte)Mathf.Clamp(finalProductionRate, 0, 255);
                CustomBuffersManager.SetCustomBuffer(buildingID, custom_buffers);
                if (b != 0)
                {
                    instance.m_parks.m_buffer[(int)b].AddWorkers(aliveWorkerCount);
                }
                else if (m_industryType != DistrictPark.ParkType.Industry)
                {
                    GuideController properties2 = Singleton<GuideManager>.instance.m_properties;
                    if (properties2 != null)
                    {
                        Singleton<BuildingManager>.instance.m_industryBuildingOutsideIndustryArea.Activate(properties2.m_industryBuildingOutsideIndustryArea, buildingID);
                    }
                }
                if (finalProductionRate != 0)
                {
                    buildingData.m_flags |= Building.Flags.Active;
                }
            }
        }

        public void ProduceGoods4(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            ExtendedDistrictManager instance2 = Singleton<ExtendedDistrictManager>.instance;
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
            int num13 = m_outputRate4;
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
            base.HandleDead2(buildingID, ref buildingData, ref behaviour, totalWorkerCount);
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

                int InputBufferSize4 = 0;
                int CustomBuffer4 = 0;
                if (m_inputResource4 != ExtendedTransferManager.TransferReason.None)
                {
                    InputBufferSize4 = GetInputBufferSize(ref buildingData, m_inputRate4);
                    CustomBuffer4 = (int)custom_buffers.m_customBuffer4;
                    int Input4ProductionRate = (m_inputRate4 * finalProductionRate + 99) / 100;
                    if (CustomBuffer4 < Input4ProductionRate)
                    {
                        finalProductionRate = (CustomBuffer4 * 100 + m_inputRate4 - 1) / m_inputRate4;
                        problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.NoInputProducts);
                    }
                }
                int OutputBufferSize4 = 0;
                int CustomBuffer8 = 0;
                if (m_outputResource4 != ExtendedTransferManager.TransferReason.None)
                {
                    OutputBufferSize4 = GetOutputBufferSize(ref buildingData, m_outputRate4, m_outputVehicleCount4);
                    CustomBuffer8 = (int)custom_buffers.m_customBuffer8;
                    int OutputProductionRate4 = (num13 * finalProductionRate + 99) / 100;
                    if (OutputBufferSize4 - CustomBuffer8 < OutputProductionRate4)
                    {
                        OutputProductionRate4 = Mathf.Max(0, OutputBufferSize4 - CustomBuffer8);
                        finalProductionRate = (OutputProductionRate4 * 100 + num13 - 1) / num13;
                        if (m_outputVehicleCount4 != 0)
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

                if (m_inputResource4 != ExtendedTransferManager.TransferReason.None)
                {
                    int Input4ProductionRate = (m_inputRate4 * finalProductionRate + 99) / 100;
                    CustomBuffer4 = Mathf.Max(0, CustomBuffer4 - Input4ProductionRate);
                    custom_buffers.m_customBuffer4 = (ushort)CustomBuffer4;
                    instance2.m_industryParks.m_buffer[b].AddConsumptionAmount(m_inputResource4, Input4ProductionRate);
                }
                if (m_outputResource4 != ExtendedTransferManager.TransferReason.None)
                {
                    int OutputProductionRate4 = (num13 * finalProductionRate + 99) / 100;
                    CustomBuffer8 = Mathf.Min(OutputBufferSize4, CustomBuffer8 + OutputProductionRate4);
                    custom_buffers.m_customBuffer8 = (ushort)CustomBuffer8;
                    instance2.m_industryParks.m_buffer[b].AddProductionAmount(instance.m_parks.m_buffer[b], m_outputResource4, OutputProductionRate4);
                }
                CustomBuffersManager.SetCustomBuffer(buildingID, custom_buffers);
                int num19 = (finalProductionRate * num16 + 50) / 100;
                if (num19 != 0)
                {
                    num19 = UniqueFacultyAI.DecreaseByBonus(UniqueFacultyAI.FacultyBonus.Science, num19);
                    Singleton<NaturalResourceManager>.instance.TryDumpResource(NaturalResourceManager.Resource.Pollution, num19, num19, vector, m_pollutionRadius);
                }

                if (b != 0 || m_industryType == DistrictPark.ParkType.Industry)
                {
                    int TempOutput4 = 0;
                    if (m_inputResource4 != ExtendedTransferManager.TransferReason.None)
                    {
                        int count4 = 0;
                        int cargo4 = 0;
                        int capacity4 = 0;
                        int outside4 = 0;
                        ExtendedVehicleManager.CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource4, ref count4, ref cargo4, ref capacity4, ref outside4);
                        if (outside4 != 0)
                        {
                            TempOutput4 |= 1;
                        }
                        int InputSize4 = InputBufferSize4 - CustomBuffer4 - cargo4;
                        if (InputSize4 >= 4000)
                        {
                            ExtendedTransferManager.Offer transferOffer = default;
                            transferOffer.Building = buildingID;
                            transferOffer.Position = buildingData.m_position;
                            transferOffer.Amount = 1;
                            transferOffer.Active = false;
                            Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(m_inputResource4, transferOffer);
                        }
                        instance2.m_industryParks.m_buffer[b].AddBufferStatus(m_inputResource4, CustomBuffer4, cargo4, InputBufferSize4);
                    }
                    buildingData.m_tempImport |= (byte)TempOutput4;

                    int TempExport4 = 0;
                    if (m_outputResource4 != ExtendedTransferManager.TransferReason.None)
                    {
                        if (m_outputVehicleCount4 == 0)
                        {
                            if (CustomBuffer8 == OutputBufferSize4)
                            {
                                int value = (CustomBuffer8 * IndustryBuildingManager.GetExtendedResourcePrice(m_outputResource4) + 50) / 100;
                                if ((instance.m_districts.m_buffer[district].m_cityPlanningPolicies & DistrictPolicies.CityPlanning.SustainableFishing) != 0)
                                {
                                    value = (value * 105 + 99) / 100;
                                }
                                int amount = UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Science, value);
                                Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.ResourcePrice, amount, m_info.m_class);
                                CustomBuffer8 = 0;
                                custom_buffers.m_customBuffer5 = (ushort)CustomBuffer8;
                                TempExport4 = byte.MaxValue;
                            }
                        }
                        else
                        {
                            int count4 = 0;
                            int cargo4 = 0;
                            int capacity4 = 0;
                            int outside4 = 0;
                            ExtendedVehicleManager.CalculateOwnVehicles(buildingID, ref buildingData, m_outputResource4, ref count4, ref cargo4, ref capacity4, ref outside4);
                            TempExport4 = (byte)Mathf.Clamp(outside4, buildingData.m_tempExport, 255);
                            int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
                            int productionRate4 = PlayerBuildingAI.GetProductionRate(100, budget);
                            int OutputProductionRate4 = (productionRate4 * m_outputVehicleCount4 + 99) / 100;
                            if (CustomBuffer8 >= 8000 && count4 < OutputProductionRate4)
                            {
                                ExtendedTransferManager.Offer transferOffer = default;
                                transferOffer.Building = buildingID;
                                transferOffer.Position = buildingData.m_position;
                                transferOffer.Amount = 1;
                                transferOffer.Active = true;
                                Singleton<ExtendedTransferManager>.instance.AddOutgoingOffer(m_outputResource4, transferOffer);
                            }
                            instance2.m_industryParks.m_buffer[b].AddBufferStatus(m_outputResource4, CustomBuffer8, cargo4, OutputBufferSize4);
                        }
                    }

                    buildingData.m_tempExport = (byte)TempExport4;

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

                buildingData.m_problems = problemStruct;
                custom_buffers.m_customBuffer9 = (byte)Mathf.Clamp(finalProductionRate * num13 / Mathf.Max(1, m_outputRate1), 0, 255);
                buildingData.m_health = (byte)Mathf.Clamp(finalProductionRate, 0, 255);
                CustomBuffersManager.SetCustomBuffer(buildingID, custom_buffers);
                if (b != 0)
                {
                    instance.m_parks.m_buffer[(int)b].AddWorkers(aliveWorkerCount);
                }
                else if (m_industryType != DistrictPark.ParkType.Industry)
                {
                    GuideController properties2 = Singleton<GuideManager>.instance.m_properties;
                    if (properties2 != null)
                    {
                        Singleton<BuildingManager>.instance.m_industryBuildingOutsideIndustryArea.Activate(properties2.m_industryBuildingOutsideIndustryArea, buildingID);
                    }
                }
                if (finalProductionRate != 0)
                {
                    buildingData.m_flags |= Building.Flags.Active;
                }
            }
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
            if (m_outputResource1 != ExtendedTransferManager.TransferReason.None && m_outputVehicleCount1 != 0)
            {
                text_prod = text_prod + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLE_COUNT", [m_outputVehicleCount1]);
            }
            if (m_outputResource2 != ExtendedTransferManager.TransferReason.None && m_outputVehicleCount2 != 0)
            {
                text_prod = text_prod + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLE_COUNT", [m_outputVehicleCount2]);
            }
            if (m_outputResource3 != ExtendedTransferManager.TransferReason.None && m_outputVehicleCount3 != 0)
            {
                text_prod = text_prod + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLE_COUNT", [m_outputVehicleCount3]);
            }
            if (m_outputResource4 != ExtendedTransferManager.TransferReason.None && m_outputVehicleCount4 != 0)
            {
                text_prod = text_prod + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLE_COUNT", [m_outputVehicleCount4]);
            }
            string baseTooltip = TooltipHelper.Append(base.GetLocalizedTooltip(), TooltipHelper.Format(
            [
                LocaleFormatter.Info1,
                text_water,
                LocaleFormatter.Info2,
                text_prod,
            ]));
            bool flag1 = m_inputResource1 != ExtendedTransferManager.TransferReason.None;
            string text1 = ((m_inputResource1 == ExtendedTransferManager.TransferReason.None) ? string.Empty : m_inputResource1.ToString());
            bool flag2 = m_inputResource2 != ExtendedTransferManager.TransferReason.None;
            string text2 = ((m_inputResource2 == ExtendedTransferManager.TransferReason.None) ? string.Empty : m_inputResource2.ToString());
            bool flag3 = m_inputResource3 != ExtendedTransferManager.TransferReason.None;
            string text3 = ((m_inputResource3 == ExtendedTransferManager.TransferReason.None) ? string.Empty : m_inputResource3.ToString());
            bool flag4 = m_inputResource4 != ExtendedTransferManager.TransferReason.None;
            string text4 = ((m_inputResource4 == ExtendedTransferManager.TransferReason.None) ? string.Empty : m_inputResource4.ToString());

            bool flag5 = m_outputResource1 != ExtendedTransferManager.TransferReason.None;
            string text5 = ((m_outputResource1 == ExtendedTransferManager.TransferReason.None) ? string.Empty : m_outputResource1.ToString());
            bool flag6 = m_outputResource2 != ExtendedTransferManager.TransferReason.None;
            string text6 = ((m_outputResource2 == ExtendedTransferManager.TransferReason.None) ? string.Empty : m_outputResource2.ToString());
            bool flag7 = m_outputResource3 != ExtendedTransferManager.TransferReason.None;
            string text7 = ((m_outputResource3 == ExtendedTransferManager.TransferReason.None) ? string.Empty : m_outputResource3.ToString());
            bool flag8 = m_outputResource4 != ExtendedTransferManager.TransferReason.None;
            string text8 = ((m_outputResource4 == ExtendedTransferManager.TransferReason.None) ? string.Empty : m_outputResource4.ToString());

            string addTooltip = TooltipHelper.Format(
            [
                "arrowVisible",
                "true",
                "input1Visible",
                flag1.ToString(),
                "input2Visible",
                flag2.ToString(),
                "input3Visible",
                flag3.ToString(),
                "input4Visible",
                flag4.ToString(),
                "output1Visible",
                flag5.ToString(),
                "output2Visible",
                flag6.ToString(),
                "output3Visible",
                flag7.ToString(),
                "output4Visible",
                flag8.ToString()
            ]);
            string addTooltip2 = TooltipHelper.Format(
            [
                "input1",
                text1,
                "input2",
                text2,
                "input3",
                text3,
                "input4",
                text4,
                "output1",
                text5,
                "output2",
                text6,
                "output3",
                text7,
                "output4",
                text8
            ]);
            baseTooltip = TooltipHelper.Append(baseTooltip, addTooltip);
            return TooltipHelper.Append(baseTooltip, addTooltip2);
        }

        public override string GetLocalizedStats(ushort buildingID, ref Building data)
        {
            var custom_buffers = CustomBuffersManager.GetCustomBuffer(buildingID);
            int m_customBuffer5 = (int)custom_buffers.m_customBuffer5;
            int m_customBuffer6 = (int)custom_buffers.m_customBuffer6;
            int m_customBuffer7 = (int)custom_buffers.m_customBuffer7;
            int m_customBuffer8 = (int)custom_buffers.m_customBuffer8;
            int output1_production_rate = m_customBuffer5 * m_outputRate1 * 16 / 100;
            int output2_production_rate = m_customBuffer6 * m_outputRate2 * 16 / 100;
            int output3_production_rate = m_customBuffer7 * m_outputRate3 * 16 / 100;
            int output4_production_rate = m_customBuffer8 * m_outputRate4 * 16 / 100;
            string text = LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_PRODUCTION_RATE", [output1_production_rate, output2_production_rate, output3_production_rate, output4_production_rate]);
            if (m_outputResource1 != ExtendedTransferManager.TransferReason.None && m_outputVehicleCount1 != 0)
            {
                int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                int vehicle_count = (productionRate * m_outputVehicleCount1 + 99) / 100;
                int count = 0;
                int capacity = 0;
                int cargo = 0;
                int outside = 0;
                ExtendedVehicleManager.CalculateOwnVehicles(buildingID, ref data, m_outputResource1, ref count, ref capacity, ref cargo, ref outside);
                text = text + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLES", [count, vehicle_count]);
            }
            if (m_outputResource2 != ExtendedTransferManager.TransferReason.None && m_outputVehicleCount2 != 0)
            {
                int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                int vehicle_count = (productionRate * m_outputVehicleCount2 + 99) / 100;
                int count = 0;
                int capacity = 0;
                int cargo = 0;
                int outside = 0;
                ExtendedVehicleManager.CalculateOwnVehicles(buildingID, ref data, m_outputResource2, ref count, ref capacity, ref cargo, ref outside);
                text = text + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLES", [count, vehicle_count]);
            }
            if (m_outputResource3 != ExtendedTransferManager.TransferReason.None && m_outputVehicleCount3 != 0)
            {
                int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                int vehicle_count = (productionRate * m_outputVehicleCount3 + 99) / 100;
                int count = 0;
                int capacity = 0;
                int cargo = 0;
                int outside = 0;
                ExtendedVehicleManager.CalculateOwnVehicles(buildingID, ref data, m_outputResource3, ref count, ref capacity, ref cargo, ref outside);
                text = text + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLES", [count, vehicle_count]);
            }
            if (m_outputResource4 != ExtendedTransferManager.TransferReason.None && m_outputVehicleCount4 != 0)
            {
                int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                int vehicle_count = (productionRate * m_outputVehicleCount4 + 99) / 100;
                int count = 0;
                int capacity = 0;
                int cargo = 0;
                int outside = 0;
                ExtendedVehicleManager.CalculateOwnVehicles(buildingID, ref data, m_outputResource4, ref count, ref capacity, ref cargo, ref outside);
                text = text + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLES", [count, vehicle_count]);
            }
            return text;
        }

        public override bool RequireRoadAccess()
        {
            return base.RequireRoadAccess() || m_inputResource1 != ExtendedTransferManager.TransferReason.None
                || m_inputResource2 != ExtendedTransferManager.TransferReason.None || m_inputResource3 != ExtendedTransferManager.TransferReason.None
                || m_inputResource4 != ExtendedTransferManager.TransferReason.None || m_outputResource1 != ExtendedTransferManager.TransferReason.None
                || m_outputResource2 != ExtendedTransferManager.TransferReason.None || m_outputResource3 != ExtendedTransferManager.TransferReason.None
                || m_outputResource4 != ExtendedTransferManager.TransferReason.None;
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

    }
}