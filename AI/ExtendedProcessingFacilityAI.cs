using System;
using ColossalFramework;
using ColossalFramework.DataBinding;
using UnityEngine;
using IndustriesMeetsSunsetHarbor.Managers;
using MoreTransferReasons;

namespace IndustriesMeetsSunsetHarbor.AI
{
    public class ExtendedProcessingFacilityAI : IndustryBuildingAI
    {
        [CustomizableProperty("Input Resource Rate 1")]
        public int m_inputRate1;

        [CustomizableProperty("Input Resource Rate 2")]
        public int m_inputRate2;

        [CustomizableProperty("Input Resource Rate 3")]
        public int m_inputRate3;

        [CustomizableProperty("Input Resource Rate 4")]
        public int m_inputRate4;

        [CustomizableProperty("Input Resource Rate 5")]
        public int m_inputRate5;

        [CustomizableProperty("Input Resource Rate 6")]
        public int m_inputRate6;

        [CustomizableProperty("Input Resource Rate 7")]
        public int m_inputRate7;

        [CustomizableProperty("Input Resource Rate 8")]
        public int m_inputRate8;

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
                return m_inputResource1.Length != 0 && m_inputResource1[0] == TransferManager.TransferReason.Fish;
            }
        }

        [CustomizableProperty("Input Resource 1")]
        public TransferManager.TransferReason[] m_inputResource1 = [];

        [CustomizableProperty("Input Resource 2")]
        public TransferManager.TransferReason[] m_inputResource2 = [];

        [CustomizableProperty("Input Resource 3")]
        public TransferManager.TransferReason[] m_inputResource3 = [];

        [CustomizableProperty("Input Resource 4")]
        public TransferManager.TransferReason[] m_inputResource4 = [];

        [CustomizableProperty("Input Resource 5")]
        public TransferManager.TransferReason[] m_inputResource5 = [];

        [CustomizableProperty("Input Resource 6")]
        public TransferManager.TransferReason[] m_inputResource6 = [];

        [CustomizableProperty("Input Resource 7")]
        public TransferManager.TransferReason[] m_inputResource7 = [];

        [CustomizableProperty("Input Resource 8")]
        public TransferManager.TransferReason[] m_inputResource8 = [];

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
                            if (m_inputResource1.Length != 0 && ((data.m_tempImport | data.m_finalImport) & 1) != 0)
                            {
                                return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource1[0]];
                            }
                            if (m_inputResource2.Length != 0 && ((data.m_tempImport | data.m_finalImport) & 2) != 0)
                            {
                                return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource2[0]];
                            }
                            if (m_inputResource3.Length != 0 && ((data.m_tempImport | data.m_finalImport) & 4) != 0)
                            {
                                return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource3[0]];
                            }
                            if (m_inputResource4.Length != 0 && ((data.m_tempImport | data.m_finalImport) & 8) != 0)
                            {
                                return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource4[0]];
                            }
                            if (m_inputResource5.Length != 0 && ((data.m_tempImport | data.m_finalImport) & 16) != 0)
                            {
                                return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource5[0]];
                            }
                            if (m_inputResource6.Length != 0 && ((data.m_tempImport | data.m_finalImport) & 32) != 0)
                            {
                                return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource6[0]];
                            }
                            if (m_inputResource7.Length != 0 && ((data.m_tempImport | data.m_finalImport) & 64) != 0)
                            {
                                return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource7[0]];
                            }
                            if (m_inputResource8.Length != 0 && ((data.m_tempImport | data.m_finalImport) & 128) != 0)
                            {
                                return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource8[0]];
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
                    if (m_inputResource1.Length != 0 && m_inputResource1[0] == TransferManager.TransferReason.Fish)
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
            if (m_inputResource1.Length != 0)
            {
                int inputBufferSize1 = GetInputBufferSize(ref data, m_inputRate1);
                int customBuffer1 = 0;
                int count1 = 0;
                int cargo1 = 0;
                int capacity1 = 0;
                int outside1 = 0;
                foreach (var inputResource in m_inputResource1)
                {
                    if (inputResource != TransferManager.TransferReason.None)
                    {
                        customBuffer1 += (int)custom_buffers.Get((int)inputResource);
                        base.CalculateGuestVehicles(buildingID, ref data, inputResource, ref count1, ref cargo1, ref capacity1, ref outside1);
                    }
                }
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}",
                [
                    text,
                    m_inputResource1.ToString(),
                    customBuffer1,
                    cargo1,
                    inputBufferSize1
                ]);
            }
            if (m_inputResource2.Length != 0)
            {
                int inputBufferSize2 = GetInputBufferSize(ref data, m_inputRate2);
                int customBuffer2 = 0;
                int count2 = 0;
                int cargo2 = 0;
                int capacity2 = 0;
                int outside2 = 0;
                foreach (var inputResource in m_inputResource2)
                {
                    if (inputResource != TransferManager.TransferReason.None)
                    {
                        customBuffer2 += (int)custom_buffers.Get((int)inputResource);
                        base.CalculateGuestVehicles(buildingID, ref data, inputResource, ref count2, ref cargo2, ref capacity2, ref outside2);
                    }
                }
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}",
                [
                    text,
                    m_inputResource2.ToString(),
                    customBuffer2,
                    cargo2,
                    inputBufferSize2
                ]);
            }
            if (m_inputResource3.Length != 0)
            {
                int inputBufferSize3 = GetInputBufferSize(ref data, m_inputRate3);
                int customBuffer3 = 0;
                int count3 = 0;
                int cargo3 = 0;
                int capacity3 = 0;
                int outside3 = 0;
                foreach (var inputResource in m_inputResource3)
                {
                    if (inputResource != TransferManager.TransferReason.None)
                    {
                        customBuffer3 += (int)custom_buffers.Get((int)inputResource);
                        base.CalculateGuestVehicles(buildingID, ref data, inputResource, ref count3, ref cargo3, ref capacity3, ref outside3);
                    }
                }
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}",
                [
                    text,
                    m_inputResource3.ToString(),
                    customBuffer3,
                    cargo3,
                    inputBufferSize3
                ]);
            }
            if (m_inputResource4.Length != 0)
            {
                int inputBufferSize4 = GetInputBufferSize(ref data, m_inputRate4);
                int customBuffer4 = 0;
                int count4 = 0;
                int cargo4 = 0;
                int capacity4 = 0;
                int outside4 = 0;
                foreach (var inputResource in m_inputResource4)
                {
                    if (inputResource != TransferManager.TransferReason.None)
                    {
                        customBuffer4 += (int)custom_buffers.Get((int)inputResource);
                        base.CalculateGuestVehicles(buildingID, ref data, inputResource, ref count4, ref cargo4, ref capacity4, ref outside4);
                    }
                }
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}",
                [
                    text,
                    m_inputResource4.ToString(),
                    customBuffer4,
                    cargo4,
                    inputBufferSize4
                ]);
            }
            if (m_inputResource5.Length != 0)
            {
                int inputBufferSize5 = GetInputBufferSize(ref data, m_inputRate5);
                int customBuffer5 = 0;
                int count5 = 0;
                int cargo5 = 0;
                int capacity5 = 0;
                int outside5 = 0;
                foreach (var inputResource in m_inputResource5)
                {
                    if (inputResource != TransferManager.TransferReason.None)
                    {
                        customBuffer5 += (int)custom_buffers.Get((int)inputResource);
                        base.CalculateGuestVehicles(buildingID, ref data, inputResource, ref count5, ref cargo5, ref capacity5, ref outside5);
                    }
                }
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}",
                [
                    text,
                    m_inputResource5.ToString(),
                    customBuffer5,
                    cargo5,
                    inputBufferSize5
                ]);
            }
            if (m_inputResource6.Length != 0)
            {
                int inputBufferSize6 = GetInputBufferSize(ref data, m_inputRate6);
                int customBuffer6 = 0;
                int count6 = 0;
                int cargo6 = 0;
                int capacity6 = 0;
                int outside6 = 0;
                foreach (var inputResource in m_inputResource6)
                {
                    if (inputResource != TransferManager.TransferReason.None)
                    {
                        customBuffer6 += (int)custom_buffers.Get((int)inputResource);
                        base.CalculateGuestVehicles(buildingID, ref data, inputResource, ref count6, ref cargo6, ref capacity6, ref outside6);
                    }
                }
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}",
                [
                    text,
                    m_inputResource6.ToString(),
                    customBuffer6,
                    cargo6,
                    inputBufferSize6
                ]);
            }
            if (m_inputResource7.Length != 0)
            {
                int inputBufferSize7 = GetInputBufferSize(ref data, m_inputRate7);
                int customBuffer7 = 0;
                int count7 = 0;
                int cargo7 = 0;
                int capacity7 = 0;
                int outside7 = 0;
                foreach (var inputResource in m_inputResource7)
                {
                    if (inputResource != TransferManager.TransferReason.None)
                    {
                        customBuffer7 += (int)custom_buffers.Get((int)inputResource);
                        base.CalculateGuestVehicles(buildingID, ref data, inputResource, ref count7, ref cargo7, ref capacity7, ref outside7);
                    }
                }
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}",
                [
                    text,
                    m_inputResource7.ToString(),
                    customBuffer7,
                    cargo7,
                    inputBufferSize7
                ]);
            }
            if (m_inputResource8.Length != 0)
            {
                int inputBufferSize8 = GetInputBufferSize(ref data, m_inputRate8);
                int customBuffer8 = 0;
                int count8 = 0;
                int cargo8 = 0;
                int capacity8 = 0;
                int outside8 = 0;
                foreach (var inputResource in m_inputResource8)
                {
                    if (inputResource != TransferManager.TransferReason.None)
                    {
                        customBuffer8 += (int)custom_buffers.Get((int)inputResource);
                        base.CalculateGuestVehicles(buildingID, ref data, inputResource, ref count8, ref cargo8, ref capacity8, ref outside8);
                    }
                }
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}",
                [
                    text,
                    m_inputResource8.ToString(),
                    customBuffer8,
                    cargo8,
                    inputBufferSize8
                ]);
            }
            if (m_outputResource1 != TransferManager.TransferReason.None)
            {
                int outputBufferSize1 = GetOutputBufferSize(ref data, m_outputRate1, m_outputVehicleCount1);
                int outputBuffer1 = (int)custom_buffers.Get((int)m_outputResource1);
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
                int outputBufferSize2 = GetOutputBufferSize(ref data, m_outputRate2, m_outputVehicleCount2);
                int outputBuffer2 = (int)custom_buffers.Get((int)m_outputResource2);
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
            if (Array.IndexOf(m_inputResource1, material) != -1)
            {
                int inputBufferSize1 = GetInputBufferSize(ref data, m_inputRate1);
                int m_customBuffer1 = (int)custom_buffers.Get((int)material);
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer1, inputBufferSize1 - m_customBuffer1);
                m_customBuffer1 += amountDelta;
                custom_buffers.Add((int)material, m_customBuffer1);
            }
            else if (Array.IndexOf(m_inputResource2, material) != -1)
            {
                int inputBufferSize2 = GetInputBufferSize(ref data, m_inputRate2);
                int m_customBuffer2 = (int)custom_buffers.Get((int)material);
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer2, inputBufferSize2 - m_customBuffer2);
                m_customBuffer2 += amountDelta;
                custom_buffers.Add((int)material, m_customBuffer2);
            }
            else if (Array.IndexOf(m_inputResource3, material) != -1)
            {
                int inputBufferSize3 = GetInputBufferSize(ref data, m_inputRate3);
                int m_customBuffer3 = (int)custom_buffers.Get((int)material);
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer3, inputBufferSize3 - m_customBuffer3);
                m_customBuffer3 += amountDelta;
                custom_buffers.Add((int)material, m_customBuffer3);
            }
            else if (Array.IndexOf(m_inputResource4, material) != -1)
            {
                int inputBufferSize4 = GetInputBufferSize(ref data, m_inputRate4);
                int m_customBuffer4 = (int)custom_buffers.Get((int)material);
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer4, inputBufferSize4 - m_customBuffer4);
                m_customBuffer4 += amountDelta;
                custom_buffers.Add((int)material, m_customBuffer4);
            }
            else if (Array.IndexOf(m_inputResource5, material) != -1)
            {
                int inputBufferSize5 = GetInputBufferSize(ref data, m_inputRate5);
                int m_customBuffer5 = (int)custom_buffers.Get((int)material);
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer5, inputBufferSize5 - m_customBuffer5);
                m_customBuffer5 += amountDelta;
                custom_buffers.Add((int)material, m_customBuffer5);
            }
            else if (Array.IndexOf(m_inputResource6, material) != -1)
            {
                int inputBufferSize6 = GetInputBufferSize(ref data, m_inputRate6);
                int m_customBuffer6 = (int)custom_buffers.Get((int)material);
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer6, inputBufferSize6 - m_customBuffer6);
                m_customBuffer6 += amountDelta;
                custom_buffers.Add((int)material, m_customBuffer6);
            }
            else if (Array.IndexOf(m_inputResource7, material) != -1)
            {
                int inputBufferSize7 = GetInputBufferSize(ref data, m_inputRate7);
                int m_customBuffer7 = (int)custom_buffers.Get((int)material);
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer7, inputBufferSize7 - m_customBuffer7);
                m_customBuffer7 += amountDelta;
                custom_buffers.Add((int)material, m_customBuffer7);
            }
            else if (Array.IndexOf(m_inputResource8, material) != -1)
            {
                int inputBufferSize8 = GetInputBufferSize(ref data, m_inputRate8);
                int m_customBuffer8 = (int)custom_buffers.Get((int)material);
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer8, inputBufferSize8 - m_customBuffer8);
                m_customBuffer8 += amountDelta;
                custom_buffers.Add((int)material, m_customBuffer8);
            }
            else if (material == m_outputResource1)
            {
                int outputBufferSize9 = GetOutputBufferSize(ref data, m_outputRate1, m_outputVehicleCount1);
                int m_customBuffer9 = (int)custom_buffers.Get((int)material);
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer9, outputBufferSize9 - m_customBuffer9);
                m_customBuffer9 += amountDelta;
                custom_buffers.Add((int)material, m_customBuffer9);
            }
            else if (material == m_outputResource2)
            {
                int outputBufferSize10 = GetOutputBufferSize(ref data, m_outputRate2, m_outputVehicleCount2);
                int m_customBuffer10 = (int)custom_buffers.Get((int)material);
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer10, outputBufferSize10 - m_customBuffer10);
                m_customBuffer10 += amountDelta;
                custom_buffers.Add((int)material, m_customBuffer10);
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
            if (m_inputResource1.Length != 0)
            {
                foreach (var inputResource in m_inputResource1)
                {
                    if (inputResource != TransferManager.TransferReason.None)
                    {
                        Singleton<TransferManager>.instance.RemoveIncomingOffer(inputResource, transferOffer);
                    }
                }
            }
            if (m_inputResource2.Length != 0)
            {
                foreach (var inputResource in m_inputResource2)
                {
                    if (inputResource != TransferManager.TransferReason.None)
                    {
                        Singleton<TransferManager>.instance.RemoveIncomingOffer(inputResource, transferOffer);
                    }
                }
            }
            if (m_inputResource3.Length != 0)
            {
                foreach (var inputResource in m_inputResource3)
                {
                    if (inputResource != TransferManager.TransferReason.None)
                    {
                        Singleton<TransferManager>.instance.RemoveIncomingOffer(inputResource, transferOffer);
                    }
                }
            }
            if (m_inputResource4.Length != 0)
            {
                foreach (var inputResource in m_inputResource4)
                {
                    if (inputResource != TransferManager.TransferReason.None)
                    {
                        Singleton<TransferManager>.instance.RemoveIncomingOffer(inputResource, transferOffer);
                    }
                }
            }
            if (m_inputResource5.Length != 0)
            {
                foreach (var inputResource in m_inputResource5)
                {
                    if (inputResource != TransferManager.TransferReason.None)
                    {
                        Singleton<TransferManager>.instance.RemoveIncomingOffer(inputResource, transferOffer);
                    }
                }
            }
            if (m_inputResource6.Length != 0)
            {
                foreach (var inputResource in m_inputResource6)
                {
                    if (inputResource != TransferManager.TransferReason.None)
                    {
                        Singleton<TransferManager>.instance.RemoveIncomingOffer(inputResource, transferOffer);
                    }
                }
            }
            if (m_inputResource7.Length != 0)
            {
                foreach (var inputResource in m_inputResource7)
                {
                    if (inputResource != TransferManager.TransferReason.None)
                    {
                        Singleton<TransferManager>.instance.RemoveIncomingOffer(inputResource, transferOffer);
                    }
                }
            }
            if (m_inputResource8.Length != 0)
            {
                foreach (var inputResource in m_inputResource8)
                {
                    if (inputResource != TransferManager.TransferReason.None)
                    {
                        Singleton<TransferManager>.instance.RemoveIncomingOffer(inputResource, transferOffer);
                    }
                }
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
                if (m_inputResource1.Length != 0)
                {
                    InputBufferSize1 = GetInputBufferSize(ref buildingData, m_inputRate1);
                    foreach (var inputResource in m_inputResource1)
                    {
                        if (inputResource != TransferManager.TransferReason.None)
                        {
                            CustomInputBuffer1 += (int)custom_buffers.Get((int)inputResource);
                        }
                    }
                    int Input1ProductionRate = (m_inputRate1 * finalProductionRate + 99) / 100;
                    if (CustomInputBuffer1 < Input1ProductionRate)
                    {
                        finalProductionRate = (CustomInputBuffer1 * 100 + m_inputRate1 - 1) / m_inputRate1;
                        problemStruct = Notification.AddProblems(problemStruct, (!flag) ? ((!IsRawMaterial(m_inputResource1[0])) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources) : Notification.Problem1.NoFishingGoods);
                    }
                }
                int InputBufferSize2 = 0;
                int CustomInputBuffer2 = 0;
                if (m_inputResource2.Length != 0)
                {
                    InputBufferSize2 = GetInputBufferSize(ref buildingData, m_inputRate2);
                    foreach (var inputResource in m_inputResource2)
                    {
                        if (inputResource != TransferManager.TransferReason.None)
                        {
                            CustomInputBuffer2 += (int)custom_buffers.Get((int)inputResource);
                        }
                    }
                    int Input2ProductionRate = (m_inputRate2 * finalProductionRate + 99) / 100;
                    if (CustomInputBuffer2 < Input2ProductionRate)
                    {
                        finalProductionRate = (CustomInputBuffer2 * 100 + m_inputRate2 - 1) / m_inputRate2;
                        problemStruct = Notification.AddProblems(problemStruct, (!flag) ? ((!IsRawMaterial(m_inputResource2[0])) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources) : Notification.Problem1.NoFishingGoods);
                    }
                }
                int InputBufferSize3 = 0;
                int CustomInputBuffer3 = 0;
                if (m_inputResource3.Length != 0)
                {
                    InputBufferSize3 = GetInputBufferSize(ref buildingData, m_inputRate3);
                    foreach (var inputResource in m_inputResource3)
                    {
                        if (inputResource != TransferManager.TransferReason.None)
                        {
                            CustomInputBuffer3 += (int)custom_buffers.Get((int)inputResource);
                        }
                    }
                    int Input3ProductionRate = (m_inputRate3 * finalProductionRate + 99) / 100;
                    if (CustomInputBuffer3 < Input3ProductionRate)
                    {
                        finalProductionRate = (CustomInputBuffer3 * 100 + m_inputRate3 - 1) / m_inputRate3;
                        problemStruct = Notification.AddProblems(problemStruct, (!flag) ? ((!IsRawMaterial(m_inputResource3[0])) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources) : Notification.Problem1.NoFishingGoods);
                    }
                }
                int InputBufferSize4 = 0;
                int CustomInputBuffer4 = 0;
                if (m_inputResource4.Length != 0)
                {
                    InputBufferSize4 = GetInputBufferSize(ref buildingData, m_inputRate4);
                    foreach (var inputResource in m_inputResource4)
                    {
                        if (inputResource != TransferManager.TransferReason.None)
                        {
                            CustomInputBuffer4 += (int)custom_buffers.Get((int)inputResource);
                        }
                    }
                    int Input4ProductionRate = (m_inputRate4 * finalProductionRate + 99) / 100;
                    if (CustomInputBuffer4 < Input4ProductionRate)
                    {
                        finalProductionRate = (CustomInputBuffer4 * 100 + m_inputRate4 - 1) / m_inputRate4;
                        problemStruct = Notification.AddProblems(problemStruct, (!flag) ? ((!IsRawMaterial(m_inputResource4[0])) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources) : Notification.Problem1.NoFishingGoods);
                    }
                }
                int InputBufferSize5 = 0;
                int CustomInputBuffer5 = 0;
                if (m_inputResource5.Length != 0)
                {
                    InputBufferSize5 = GetInputBufferSize(ref buildingData, m_inputRate5);
                    foreach (var inputResource in m_inputResource5)
                    {
                        if (inputResource != TransferManager.TransferReason.None)
                        {
                            CustomInputBuffer5 += (int)custom_buffers.Get((int)inputResource);
                        }
                    }
                    int Input5ProductionRate = (m_inputRate5 * finalProductionRate + 99) / 100;
                    if (CustomInputBuffer5 < Input5ProductionRate)
                    {
                        finalProductionRate = (CustomInputBuffer5 * 100 + m_inputRate5 - 1) / m_inputRate5;
                        problemStruct = Notification.AddProblems(problemStruct, (!flag) ? ((!IsRawMaterial(m_inputResource5[0])) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources) : Notification.Problem1.NoFishingGoods);
                    }
                }
                int InputBufferSize6 = 0;
                int CustomInputBuffer6 = 0;
                if (m_inputResource6.Length != 0)
                {
                    InputBufferSize6 = GetInputBufferSize(ref buildingData, m_inputRate6);
                    foreach (var inputResource in m_inputResource6)
                    {
                        if (inputResource != TransferManager.TransferReason.None)
                        {
                            CustomInputBuffer6 += (int)custom_buffers.Get((int)inputResource);
                        }
                    }
                    int Input6ProductionRate = (m_inputRate6 * finalProductionRate + 99) / 100;
                    if (CustomInputBuffer6 < Input6ProductionRate)
                    {
                        finalProductionRate = (CustomInputBuffer6 * 100 + m_inputRate6 - 1) / m_inputRate6;
                        problemStruct = Notification.AddProblems(problemStruct, (!flag) ? ((!IsRawMaterial(m_inputResource6[0])) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources) : Notification.Problem1.NoFishingGoods);
                    }
                }
                int InputBufferSize7 = 0;
                int CustomInputBuffer7 = 0;
                if (m_inputResource7.Length != 0)
                {
                    InputBufferSize7 = GetInputBufferSize(ref buildingData, m_inputRate7);
                    foreach (var inputResource in m_inputResource7)
                    {
                        if (inputResource != TransferManager.TransferReason.None)
                        {
                            CustomInputBuffer7 += (int)custom_buffers.Get((int)inputResource);
                        }
                    }
                    int Input7ProductionRate = (m_inputRate7 * finalProductionRate + 99) / 100;
                    if (CustomInputBuffer7 < Input7ProductionRate)
                    {
                        finalProductionRate = (CustomInputBuffer7 * 100 + m_inputRate7 - 1) / m_inputRate7;
                        problemStruct = Notification.AddProblems(problemStruct, (!flag) ? ((!IsRawMaterial(m_inputResource7[0])) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources) : Notification.Problem1.NoFishingGoods);
                    }
                }
                int InputBufferSize8 = 0;
                int CustomInputBuffer8 = 0;
                if (m_inputResource8.Length != 0)
                {
                    InputBufferSize8 = GetInputBufferSize(ref buildingData, m_inputRate8);
                    foreach (var inputResource in m_inputResource8)
                    {
                        if (inputResource != TransferManager.TransferReason.None)
                        {
                            CustomInputBuffer8 += (int)custom_buffers.Get((int)inputResource);
                        }
                    }
                    int Input8ProductionRate = (m_inputRate8 * finalProductionRate + 99) / 100;
                    if (CustomInputBuffer8 < Input8ProductionRate)
                    {
                        finalProductionRate = (CustomInputBuffer8 * 100 + m_inputRate8 - 1) / m_inputRate8;
                        problemStruct = Notification.AddProblems(problemStruct, (!flag) ? ((!IsRawMaterial(m_inputResource8[0])) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources) : Notification.Problem1.NoFishingGoods);
                    }
                }
                int OutputBufferSize1 = 0;
                int CustomOutputBuffer1 = 0;
                if (m_outputResource1 != TransferManager.TransferReason.None)
                {
                    OutputBufferSize1 = GetOutputBufferSize(ref buildingData, m_outputRate1, m_outputVehicleCount1);
                    CustomOutputBuffer1 = (int)custom_buffers.Get((int)m_outputResource1);
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
                    CustomOutputBuffer2 = (int)custom_buffers.Get((int)m_outputResource2);
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
                if (m_inputResource1.Length != 0)
                {
                    int Input1ProductionRate = (m_inputRate1 * finalProductionRate + 99) / 100;
                    CustomInputBuffer1 = Mathf.Max(0, CustomInputBuffer1 - Input1ProductionRate);
                    AddConsumptionAmountReturnBufferSum(b, instance, m_inputResource1, Input1ProductionRate, ref CustomInputBuffer1, ref custom_buffers);
                }
                if (m_inputResource2.Length != 0)
                {
                    int Input2ProductionRate = (m_inputRate2 * finalProductionRate + 99) / 100;
                    CustomInputBuffer2 = Mathf.Max(0, CustomInputBuffer2 - Input2ProductionRate);
                    AddConsumptionAmountReturnBufferSum(b, instance, m_inputResource2, Input2ProductionRate, ref CustomInputBuffer2, ref custom_buffers);
                }
                if (m_inputResource3.Length != 0)
                {
                    int Input3ProductionRate = (m_inputRate3 * finalProductionRate + 99) / 100;
                    CustomInputBuffer3 = Mathf.Max(0, CustomInputBuffer3 - Input3ProductionRate);
                    AddConsumptionAmountReturnBufferSum(b, instance, m_inputResource3, Input3ProductionRate, ref CustomInputBuffer3, ref custom_buffers);
                }
                if (m_inputResource4.Length != 0)
                {
                    int Input4ProductionRate = (m_inputRate4 * finalProductionRate + 99) / 100;
                    CustomInputBuffer4 = Mathf.Max(0, CustomInputBuffer4 - Input4ProductionRate);
                    AddConsumptionAmountReturnBufferSum(b, instance, m_inputResource4, Input4ProductionRate, ref CustomInputBuffer4, ref custom_buffers);
                }
                if (m_inputResource5.Length != 0)
                {
                    int Input5ProductionRate = (m_inputRate5 * finalProductionRate + 99) / 100;
                    CustomInputBuffer5 = Mathf.Max(0, CustomInputBuffer5 - Input5ProductionRate);
                    AddConsumptionAmountReturnBufferSum(b, instance, m_inputResource5, Input5ProductionRate, ref CustomInputBuffer5, ref custom_buffers);
                }
                if (m_inputResource6.Length != 0)
                {
                    int Input6ProductionRate = (m_inputRate6 * finalProductionRate + 99) / 100;
                    CustomInputBuffer6 = Mathf.Max(0, CustomInputBuffer6 - Input6ProductionRate);
                    AddConsumptionAmountReturnBufferSum(b, instance, m_inputResource6, Input6ProductionRate, ref CustomInputBuffer6, ref custom_buffers);
                }
                if (m_inputResource7.Length != 0)
                {
                    int Input7ProductionRate = (m_inputRate7 * finalProductionRate + 99) / 100;
                    CustomInputBuffer7 = Mathf.Max(0, CustomInputBuffer7 - Input7ProductionRate);
                    AddConsumptionAmountReturnBufferSum(b, instance, m_inputResource7, Input7ProductionRate, ref CustomInputBuffer7, ref custom_buffers);
                }
                if (m_inputResource8.Length != 0)
                {
                    int Input8ProductionRate = (m_inputRate8 * finalProductionRate + 99) / 100;
                    CustomInputBuffer8 = Mathf.Max(0, CustomInputBuffer8 - Input8ProductionRate);
                    AddConsumptionAmountReturnBufferSum(b, instance, m_inputResource8, Input8ProductionRate, ref CustomInputBuffer8, ref custom_buffers);
                }
                if (m_outputResource1 != TransferManager.TransferReason.None)
                {
                    int OutputProductionRate = (num13 * finalProductionRate + 99) / 100;
                    CustomOutputBuffer1 = Mathf.Min(OutputBufferSize1, CustomOutputBuffer1 + OutputProductionRate);
                    custom_buffers.Set((int)m_outputResource1, CustomOutputBuffer1);
                    instance.m_parks.m_buffer[b].AddProductionAmount(m_outputResource1, OutputProductionRate);
                }
                if (m_outputResource2 != TransferManager.TransferReason.None)
                {
                    int OutputProductionRate = (num13 * finalProductionRate + 99) / 100;
                    CustomOutputBuffer2 = Mathf.Min(OutputBufferSize2, CustomOutputBuffer2 + OutputProductionRate);
                    custom_buffers.Set((int)m_outputResource1, CustomOutputBuffer2);
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
                    if (m_inputResource1.Length != 0)
                    {
                        TempOutput = AddIncomingOffer(buildingID, ref buildingData, m_inputResource1, TempOutput, 1, InputBufferSize1, CustomInputBuffer1, b, instance);
                    }
                    if (m_inputResource2.Length != 0)
                    {
                        TempOutput = AddIncomingOffer(buildingID, ref buildingData, m_inputResource2, TempOutput, 2, InputBufferSize2, CustomInputBuffer2, b, instance);
                    }
                    if (m_inputResource3.Length != 0)
                    {
                        TempOutput = AddIncomingOffer(buildingID, ref buildingData, m_inputResource3, TempOutput, 4, InputBufferSize3, CustomInputBuffer3, b, instance);
                    }
                    if (m_inputResource4.Length != 0)
                    {
                        TempOutput = AddIncomingOffer(buildingID, ref buildingData, m_inputResource4, TempOutput, 8, InputBufferSize4, CustomInputBuffer4, b, instance);
                    }
                    if (m_inputResource5.Length != 0)
                    {
                        TempOutput = AddIncomingOffer(buildingID, ref buildingData, m_inputResource5, TempOutput, 16, InputBufferSize5, CustomInputBuffer5, b, instance);
                    }
                    if (m_inputResource6.Length != 0)
                    {
                        TempOutput = AddIncomingOffer(buildingID, ref buildingData, m_inputResource6, TempOutput, 32, InputBufferSize6, CustomInputBuffer6, b, instance);
                    }
                    if (m_inputResource7.Length != 0)
                    {
                        TempOutput = AddIncomingOffer(buildingID, ref buildingData, m_inputResource7, TempOutput, 64, InputBufferSize7, CustomInputBuffer7, b, instance);
                    }
                    if (m_inputResource8.Length != 0)
                    {
                        TempOutput = AddIncomingOffer(buildingID, ref buildingData, m_inputResource8, TempOutput, 128, InputBufferSize8, CustomInputBuffer8, b, instance);
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
                                if (b != 0)
                                {
                                    instance.m_parks.m_buffer[b].AddExportAmount(m_outputResource1, CustomOutputBuffer1);
                                }
                                CustomOutputBuffer1 = 0;
                                custom_buffers.Set((int)m_outputResource1, CustomOutputBuffer1);
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
                                if (b != 0)
                                {
                                    instance.m_parks.m_buffer[b].AddExportAmount(m_outputResource2, CustomOutputBuffer1);
                                }
                                CustomOutputBuffer2 = 0;
                                custom_buffers.Set((int)m_outputResource2, CustomOutputBuffer2);
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
            var rate = (byte)Mathf.Clamp(finalProductionRate * num13 / Mathf.Max(1, m_outputRate1), 0, 255);
            buildingData.m_education3 = (byte)(rate + (byte)Mathf.Clamp(finalProductionRate * num13 / Mathf.Max(1, m_outputRate2), 0, 255));
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
            var index1 = CustomBuffersManager.GetIndex((int)m_outputResource1);
            var index2 = CustomBuffersManager.GetIndex((int)m_outputResource2);
            float customBuffer = custom_buffers.m_volumes[index1] + custom_buffers.m_volumes[index2];
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
            bool flag1 = m_inputResource1.Length != 0;
            string text1 = (m_inputResource1.Length == 0) ? string.Empty : GetTransfersNames(m_inputResource1);
            bool flag2 = m_inputResource2.Length != 0;
            string text2 = (m_inputResource2.Length == 0) ? string.Empty : GetTransfersNames(m_inputResource2);
            bool flag3 = m_inputResource3.Length != 0;
            string text3 = (m_inputResource3.Length == 0) ? string.Empty : GetTransfersNames(m_inputResource3);
            bool flag4 = m_inputResource4.Length != 0;
            string text4 = (m_inputResource4.Length == 0) ? string.Empty : GetTransfersNames(m_inputResource4);
            bool flag5 = m_inputResource5.Length != 0;
            string text5 = (m_inputResource5.Length == 0) ? string.Empty : GetTransfersNames(m_inputResource5);
            bool flag6 = m_inputResource6.Length != 0;
            string text6 = (m_inputResource6.Length == 0) ? string.Empty : GetTransfersNames(m_inputResource6);
            bool flag7 = m_inputResource7.Length != 0;
            string text7 = (m_inputResource7.Length == 0) ? string.Empty : GetTransfersNames(m_inputResource7);
            bool flag8 = m_inputResource8.Length != 0;
            string text8 = (m_inputResource8.Length == 0) ? string.Empty : GetTransfersNames(m_inputResource8);
            bool flag9 = m_outputResource1 != TransferManager.TransferReason.None;
            string text9 = ((m_outputResource1 == TransferManager.TransferReason.None) ? string.Empty : MoreTransferReasons.Utils.AtlasUtils.GetSpriteName(m_outputResource1, false));
            bool flag10 = m_outputResource2 != TransferManager.TransferReason.None;
            string text10 = ((m_outputResource2 == TransferManager.TransferReason.None) ? string.Empty : MoreTransferReasons.Utils.AtlasUtils.GetSpriteName(m_outputResource2, false));

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
                "input5Visible",
                flag5.ToString(),
                "input6Visible",
                flag6.ToString(),
                "input7Visible",
                flag7.ToString(),
                "input8Visible",
                flag8.ToString(),
                "output1Visible",
                flag9.ToString(),
                "output2Visible",
                flag10.ToString()
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
                "input5",
                text5,
                "input6",
                text6,
                "input7",
                text7,
                "input8",
                text8,
                "output1",
                text9,
                "output2",
                text10
            ]);
            baseTooltip = TooltipHelper.Append(baseTooltip, addTooltip);
            return TooltipHelper.Append(baseTooltip, addTooltip2);
        }

        public override string GetLocalizedStats(ushort buildingID, ref Building data)
        {
            var custom_buffers = CustomBuffersManager.GetCustomBuffer(buildingID);
            var index1 = CustomBuffersManager.GetIndex((int)m_outputResource1);
            int outputBuffer1 = (int)custom_buffers.m_volumes[index1];
            var index2 = CustomBuffersManager.GetIndex((int)m_outputResource2);
            int outputBuffer2 = (int)custom_buffers.m_volumes[index2];
            int output1_production_rate = outputBuffer1 * m_outputRate1 * 16 / 100;
            int output2_production_rate = outputBuffer2 * m_outputRate2 * 16 / 100;
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
            return base.RequireRoadAccess() || m_inputResource1.Length != 0
                || m_inputResource2.Length != 0 || m_inputResource3.Length != 0 || m_inputResource4.Length != 0 || m_inputResource5.Length != 0
                || m_inputResource6.Length != 0 || m_inputResource7.Length != 0 || m_inputResource8.Length != 0 || m_outputResource1 != TransferManager.TransferReason.None
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
                TransferManager.TransferReason.Oil or TransferManager.TransferReason.Ore or TransferManager.TransferReason.Logs
                or TransferManager.TransferReason.Grain or ExtendedTransferManager.Vegetables or ExtendedTransferManager.Fruits
                or ExtendedTransferManager.Cotton => true,
                _ => false,
            };
        }

        private string GetTransfersNames(TransferManager.TransferReason[] transferReasons)
        {
            var text = "";
            if (transferReasons.Length != 0)
            {
                for (int i = 0; i < transferReasons.Length; i++)
                {
                    if (transferReasons[i] != TransferManager.TransferReason.None)
                    {
                        text += MoreTransferReasons.Utils.AtlasUtils.GetSpriteName(transferReasons[i], false);
                    }
                    if(i != transferReasons.Length - 1)
                    {
                        text = "/\n";
                    }
                }
            }
            return text;
        }

        private int AddIncomingOffer(ushort buildingID, ref Building buildingData, TransferManager.TransferReason[] transferReasons, int tempOut, int tempOutOutside,
            int InputBufferSize, int CustomInputBuffer, byte parkID, DistrictManager instance)
        {
            int count = 0;
            int cargo = 0;
            int capacity = 0;
            int outside = 0;
            foreach (var inputResource in transferReasons)
            {
                if (inputResource != TransferManager.TransferReason.None)
                {
                    base.CalculateGuestVehicles(buildingID, ref buildingData, inputResource, ref count, ref cargo, ref capacity, ref outside);
                }
            }
            if (outside != 0)
            {
                tempOut |= tempOutOutside;
            }
            int InputSize = InputBufferSize - CustomInputBuffer - cargo;
            if (InputSize >= 4000)
            {
                TransferManager.TransferOffer transferOffer = default;
                transferOffer.Priority = Mathf.Max(1, InputSize * 8 / InputBufferSize);
                transferOffer.Building = buildingID;
                transferOffer.Position = buildingData.m_position;
                transferOffer.Amount = 1;
                transferOffer.Active = false;
                foreach (var inputResource in transferReasons)
                {
                    if (inputResource != TransferManager.TransferReason.None)
                    {
                        Singleton<TransferManager>.instance.AddIncomingOffer(inputResource, transferOffer);
                    }
                }
            }
            AddBufferStatus(parkID, instance, m_inputResource1, CustomInputBuffer, cargo, InputBufferSize);
            return tempOut;
        }

        private void AddConsumptionAmountReturnBufferSum(byte parkID, DistrictManager instance, TransferManager.TransferReason[] transferReasons, int amount, ref int CustomInputBuffer, ref CustomBuffersManager.CustomBuffer customBuffer)
        {
            foreach (var inputResource in transferReasons)
            {
                if (inputResource != TransferManager.TransferReason.None)
                {
                    CustomInputBuffer += (int)customBuffer.Get((int)inputResource);
                    instance.m_parks.m_buffer[parkID].AddConsumptionAmount(inputResource, amount);
                }
            }
        }

        private void AddBufferStatus(byte parkID, DistrictManager instance, TransferManager.TransferReason[] transferReasons, int amount, int incoming, int capacity)
        {
            foreach (var inputResource in transferReasons)
            {
                if (inputResource != TransferManager.TransferReason.None)
                {
                    instance.m_parks.m_buffer[parkID].AddBufferStatus(inputResource, amount, incoming, capacity);
                }
            }
        }
    }
}