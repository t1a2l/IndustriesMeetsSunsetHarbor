using System;
using ColossalFramework;
using ColossalFramework.DataBinding;
using UnityEngine;
using MoreTransferReasons;
using IndustriesMeetsSunsetHarbor.Utils;
using IndustriesMeetsSunsetHarbor.Managers;

namespace IndustriesMeetsSunsetHarbor.AI
{
    public class NewProcessingFacilityAI : IndustryBuildingAI, IExtendedBuildingAI
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

        [CustomizableProperty("Output Resource Rate")]
        public int m_outputRate = 1000;

        [CustomizableProperty("Output Vehicle Count")]
        public int m_outputVehicleCount = 10;

        public int m_variationGroupID;

        [NonSerialized]
        private bool m_hasBufferStatusMeshes;

        protected override uint SearchKey
        {
            get
            {
                return (uint)(((m_variationGroupID & 255) << 24) | (int)((int)m_outputResource << 16) | (m_info.m_cellWidth << 8) | m_info.m_cellLength);
            }
        }

        public bool isFishFactory
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

        [CustomizableProperty("Input Resource 3")]
        public TransferManager.TransferReason m_inputResource3 = TransferManager.TransferReason.None;

        [CustomizableProperty("Input Resource 4")]
        public TransferManager.TransferReason m_inputResource4 = TransferManager.TransferReason.None;

        [CustomizableProperty("Extended Input Resource 5")]
        public ExtendedTransferManager.TransferReason m_inputResource5 = ExtendedTransferManager.TransferReason.None;

        [CustomizableProperty("Extended Input Resource 6")]
        public ExtendedTransferManager.TransferReason m_inputResource6 = ExtendedTransferManager.TransferReason.None;

        [CustomizableProperty("Extended Input Resource 7")]
        public ExtendedTransferManager.TransferReason m_inputResource7 = ExtendedTransferManager.TransferReason.None;

        [CustomizableProperty("Extended Input Resource 8")]
        public ExtendedTransferManager.TransferReason m_inputResource8 = ExtendedTransferManager.TransferReason.None;

        [CustomizableProperty("Extended Output Resource")]
        public ExtendedTransferManager.TransferReason m_outputResource = ExtendedTransferManager.TransferReason.None;

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
            if (infoMode == InfoManager.InfoMode.Connections)
            {
                if (subInfoMode == InfoManager.SubInfoMode.Default)
                {
                    if (m_inputResource1 != TransferManager.TransferReason.None && ((data.m_tempImport | data.m_finalImport) & 1) != 0)
                    {
                        return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource1];
                    }
                    if (m_inputResource2 != TransferManager.TransferReason.None && ((data.m_tempImport | data.m_finalImport) & 2) != 0)
                    {
                        return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource2];
                    }
                    if (m_inputResource3 != TransferManager.TransferReason.None && ((data.m_tempImport | data.m_finalImport) & 4) != 0)
                    {
                        return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource3];
                    }
                    if (m_inputResource4 != TransferManager.TransferReason.None && ((data.m_tempImport | data.m_finalImport) & 8) != 0)
                    {
                        return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource4];
                    }
                }
                return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
            }
            if (infoMode != InfoManager.InfoMode.Fishing)
            {
                return base.GetColor(buildingID, ref data, infoMode, subInfoMode);
            }
            if (m_inputResource1 != TransferManager.TransferReason.Fish)
            {
                return base.GetColor(buildingID, ref data, infoMode, subInfoMode);
            }
            if ((data.m_flags & Building.Flags.Active) != Building.Flags.None)
            {
                return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
            }
            return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_inactiveColor;
        }

        public override void GetPlacementInfoMode(out InfoManager.InfoMode mode, out InfoManager.SubInfoMode subMode, float elevation)
        {
            if (!isFishFactory)
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
            var custom_buffers = BuildingCustomBuffersManager.GetCustomBuffer(buildingID);
            string text = base.GetDebugString(buildingID, ref data);
            if (m_inputResource1 != TransferManager.TransferReason.None)
            {
                int inputBufferSize1 = GetInputBufferSize(ref data, m_inputRate1);
                int customBuffer1 = custom_buffers.m_customBuffer1;
                int count1 = 0;
                int cargo1 = 0;
                int capacity1 = 0;
                int outside1 = 0;
                base.CalculateGuestVehicles(buildingID, ref data, m_inputResource1, ref count1, ref cargo1, ref capacity1, ref outside1);
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}", new object[]
                {
                    text,
                    m_inputResource1.ToString(),
                    customBuffer1,
                    cargo1,
                    inputBufferSize1
                });
            }
            if (m_inputResource2 != TransferManager.TransferReason.None)
            {
                int inputBufferSize1 = GetInputBufferSize(ref data, m_inputRate2);
                int customBuffer2 = custom_buffers.m_customBuffer2;
                int count2 = 0;
                int cargo2 = 0;
                int capacity2 = 0;
                int outside2 = 0;
                base.CalculateGuestVehicles(buildingID, ref data, m_inputResource2, ref count2, ref cargo2, ref capacity2, ref outside2);
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}", new object[]
                {
                    text,
                    m_inputResource2.ToString(),
                    customBuffer2,
                    cargo2,
                    inputBufferSize1
                });
            }
            if (m_inputResource3 != TransferManager.TransferReason.None)
            {
                int inputBufferSize3 = GetInputBufferSize(ref data, m_inputRate3);
                int customBuffer3 = custom_buffers.m_customBuffer3;
                int count3 = 0;
                int cargo3 = 0;
                int capacity3 = 0;
                int outside3 = 0;
                base.CalculateGuestVehicles(buildingID, ref data, m_inputResource3, ref count3, ref cargo3, ref capacity3, ref outside3);
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}", new object[]
                {
                    text,
                    m_inputResource3.ToString(),
                    customBuffer3,
                    cargo3,
                    inputBufferSize3
                });
            }
            if (m_inputResource4 != TransferManager.TransferReason.None)
            {
                int inputBufferSize4 = GetInputBufferSize(ref data, m_inputRate4);
                int customBuffer4 = custom_buffers.m_customBuffer4;
                int count4 = 0;
                int cargo4 = 0;
                int capacity4 = 0;
                int outside4 = 0;
                base.CalculateGuestVehicles(buildingID, ref data, m_inputResource4, ref count4, ref cargo4, ref capacity4, ref outside4);
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}", new object[]
                {
                    text,
                    m_inputResource4.ToString(),
                    customBuffer4,
                    cargo4,
                    inputBufferSize4
                });
            }
            if (m_inputResource5 != ExtendedTransferManager.TransferReason.None)
            {
                int inputBufferSize5 = GetInputBufferSize(ref data, m_inputRate5);
                int customBuffer5 = custom_buffers.m_customBuffer5;
                int count5 = 0;
                int cargo5 = 0;
                int capacity5 = 0;
                int outside5 = 0;
                CalculateVehicles.CalculateGuestVehicles(buildingID, ref data, m_inputResource5, ref count5, ref cargo5, ref capacity5, ref outside5);
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}", new object[]
                {
                    text,
                    m_inputResource5.ToString(),
                    customBuffer5,
                    cargo5,
                    inputBufferSize5
                });
            }
            if (m_inputResource6 != ExtendedTransferManager.TransferReason.None)
            {
                int inputBufferSize6 = GetInputBufferSize(ref data, m_inputRate6);
                int customBuffer6 = custom_buffers.m_customBuffer6;
                int count6 = 0;
                int cargo6 = 0;
                int capacity6 = 0;
                int outside6 = 0;
                CalculateVehicles.CalculateGuestVehicles(buildingID, ref data, m_inputResource6, ref count6, ref cargo6, ref capacity6, ref outside6);
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}", new object[]
                {
                    text,
                    m_inputResource6.ToString(),
                    customBuffer6,
                    cargo6,
                    inputBufferSize6
                });
            }
            if (m_inputResource7 != ExtendedTransferManager.TransferReason.None)
            {
                int inputBufferSize7 = GetInputBufferSize(ref data, m_inputRate7);
                int customBuffer7 = custom_buffers.m_customBuffer7;
                int count7 = 0;
                int cargo7 = 0;
                int capacity7 = 0;
                int outside7 = 0;
                CalculateVehicles.CalculateGuestVehicles(buildingID, ref data, m_inputResource7, ref count7, ref cargo7, ref capacity7, ref outside7);
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}", new object[]
                {
                    text,
                    m_inputResource7.ToString(),
                    customBuffer7,
                    cargo7,
                    inputBufferSize7
                });
            }
            if (m_inputResource8 != ExtendedTransferManager.TransferReason.None)
            {
                int inputBufferSize8 = GetInputBufferSize(ref data, m_inputRate8);
                int customBuffer8 = custom_buffers.m_customBuffer8;
                int count8 = 0;
                int cargo8 = 0;
                int capacity8 = 0;
                int outside8 = 0;
                CalculateVehicles.CalculateGuestVehicles(buildingID, ref data, m_inputResource8, ref count8, ref cargo8, ref capacity8, ref outside8);
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}", new object[]
                {
                    text,
                    m_inputResource8.ToString(),
                    customBuffer8,
                    cargo8,
                    inputBufferSize8
                });
            }
            if (m_outputResource != ExtendedTransferManager.TransferReason.None)
            {
                int outputBufferSize = GetOutputBufferSize(ref data);
                int outputBuffer = custom_buffers.m_customBuffer9;
                int count9 = 0;
                int cargo9 = 0;
                int capacity9 = 0;
                int outside9 = 0;
                CalculateVehicles.CalculateGuestVehicles(buildingID, ref data, m_outputResource, ref count9, ref cargo9, ref capacity9, ref outside9);
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}", new object[]
                {
                    text,
                    m_outputResource.ToString(),
                    outputBuffer,
                    cargo9,
                    outputBufferSize
                });
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

        void IExtendedBuildingAI.ExtendedGetMaterialAmount(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, out int amount, out int max)
	{
	    amount = 0;
	    max = 0;
	}

        void IExtendedBuildingAI.ExtendedStartTransfer(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, ExtendedTransferManager.Offer offer)
        {
            if (material == m_outputResource)
            {
                VehicleInfo transferVehicleService = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, data.Info.m_class.m_service, data.Info.m_class.m_subService, data.Info.m_class.m_level);
                if (transferVehicleService != null)
                {
                    Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
                    var material_byte = (byte)material;
                    if (Singleton<VehicleManager>.instance.CreateVehicle(out ushort num, ref Singleton<SimulationManager>.instance.m_randomizer, transferVehicleService, data.m_position, (TransferManager.TransferReason)material_byte, false, true)
                        && transferVehicleService.m_vehicleAI is IExtendedVehicleAI extended)
                    {
                        vehicles.m_buffer[(int)num].m_gateIndex = (byte)m_variationGroupID;
                        transferVehicleService.m_vehicleAI.SetSource(num, ref vehicles.m_buffer[(int)num], buildingID);
                        extended.ExtendedStartTransfer(num, ref vehicles.m_buffer[(int)num], material, offer);
                    }
                }
            }
        }

        public override void ModifyMaterialBuffer(ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int amountDelta)
        {
            var custom_buffers = BuildingCustomBuffersManager.GetCustomBuffer(buildingID);
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
            else
            {
                base.ModifyMaterialBuffer(buildingID, ref data, material, ref amountDelta);
            }
            BuildingCustomBuffersManager.SetCustomBuffer(buildingID, custom_buffers);
        }

        void IExtendedBuildingAI.ExtendedModifyMaterialBuffer(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, ref int amountDelta)
        {
            var custom_buffers = BuildingCustomBuffersManager.GetCustomBuffer(buildingID);
            if (material == m_inputResource5)
            {
                int inputBufferSize5 = GetInputBufferSize(ref data, m_inputRate5);
                int m_customBuffer5 = (int)custom_buffers.m_customBuffer5;
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer5, inputBufferSize5 - m_customBuffer5);
                m_customBuffer5 += amountDelta;
                custom_buffers.m_customBuffer5 = (ushort)m_customBuffer5;
            }
            else if (material == m_inputResource6)
            {
                int inputBufferSize6 = GetInputBufferSize(ref data, m_inputRate6);
                int m_customBuffer6 = (int)custom_buffers.m_customBuffer6;
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer6, inputBufferSize6 - m_customBuffer6);
                m_customBuffer6 += amountDelta;
                custom_buffers.m_customBuffer6 = (ushort)m_customBuffer6;
            }
            else if (material == m_inputResource7)
            {
                int inputBufferSize7 = GetInputBufferSize(ref data, m_inputRate7);
                int m_customBuffer7 = (int)custom_buffers.m_customBuffer7;
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer7, inputBufferSize7 - m_customBuffer7);
                m_customBuffer7 += amountDelta;
                custom_buffers.m_customBuffer7 = (ushort)m_customBuffer7;
            }
            else if (material == m_inputResource8)
            {
                int inputBufferSize8 = GetInputBufferSize(ref data, m_inputRate8);
                int m_customBuffer8 = (int)custom_buffers.m_customBuffer8;
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer8, inputBufferSize8 - m_customBuffer8);
                m_customBuffer8 += amountDelta;
                custom_buffers.m_customBuffer8 = (ushort)m_customBuffer8;
            }
            else if (material == m_outputResource)
            {
                int outputBufferSize = GetOutputBufferSize(ref data);
                int m_customBuffer9 = (int)custom_buffers.m_customBuffer9;
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer9, outputBufferSize - m_customBuffer9);
                m_customBuffer9 += amountDelta;
                custom_buffers.m_customBuffer9 = (ushort)m_customBuffer9;
            }
            BuildingCustomBuffersManager.SetCustomBuffer(buildingID, custom_buffers);
        }

        public override void BuildingDeactivated(ushort buildingID, ref Building data)
        {
            TransferManager.TransferOffer transferOffer = default(TransferManager.TransferOffer);
            ExtendedTransferManager.Offer offer = default(ExtendedTransferManager.Offer);
            transferOffer.Building = buildingID;
            offer.Building = buildingID;
            if (m_inputResource1 != TransferManager.TransferReason.None)
            {
                Singleton<TransferManager>.instance.RemoveIncomingOffer(m_inputResource1, transferOffer);
            }
            if (m_inputResource2 != TransferManager.TransferReason.None)
            {
                Singleton<TransferManager>.instance.RemoveIncomingOffer(m_inputResource2, transferOffer);
            }
            if (m_inputResource3 != TransferManager.TransferReason.None)
            {
                Singleton<TransferManager>.instance.RemoveIncomingOffer(m_inputResource3, transferOffer);
            }
            if (m_inputResource4 != TransferManager.TransferReason.None)
            {
                Singleton<TransferManager>.instance.RemoveIncomingOffer(m_inputResource4, transferOffer);
            }
            if (m_inputResource5 != ExtendedTransferManager.TransferReason.None)
            {
                Singleton<ExtendedTransferManager>.instance.RemoveIncomingOffer(m_inputResource5, offer);
            }
            if (m_inputResource6 != ExtendedTransferManager.TransferReason.None)
            {
                Singleton<ExtendedTransferManager>.instance.RemoveIncomingOffer(m_inputResource6, offer);
            }
            if (m_inputResource7 != ExtendedTransferManager.TransferReason.None)
            {
                Singleton<ExtendedTransferManager>.instance.RemoveIncomingOffer(m_inputResource7, offer);
            }
            if (m_inputResource8 != ExtendedTransferManager.TransferReason.None)
            {
                Singleton<ExtendedTransferManager>.instance.RemoveIncomingOffer(m_inputResource8, offer);
            }
            if (m_outputResource != ExtendedTransferManager.TransferReason.None)
            {
                Singleton<ExtendedTransferManager>.instance.RemoveOutgoingOffer(m_outputResource, offer);
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
            int num13 = m_outputRate;
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
                var custom_buffers = BuildingCustomBuffersManager.GetCustomBuffer(buildingID);

                int InputBufferSize1 = 0;
                int CustomBuffer1 = 0;
                if (m_inputResource1 != TransferManager.TransferReason.None)
                {
                    InputBufferSize1 = GetInputBufferSize(ref buildingData, m_inputRate1);
                    CustomBuffer1 = custom_buffers.m_customBuffer1;
                    int Input1ProductionRate = (m_inputRate1 * finalProductionRate + 99) / 100;
                    if (CustomBuffer1 < Input1ProductionRate)
                    {
                        finalProductionRate = (CustomBuffer1 * 100 + m_inputRate1 - 1) / m_inputRate1;
                        problemStruct = Notification.AddProblems(problemStruct, (!flag) ? ((!IsRawMaterial(m_inputResource1)) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources) : Notification.Problem1.NoFishingGoods);
                    }
                }
                int InputBufferSize2 = 0;
                int CustomBuffer2 = 0;
                if (m_inputResource2 != TransferManager.TransferReason.None)
                {
                    InputBufferSize2 = GetInputBufferSize(ref buildingData, m_inputRate2);
                    CustomBuffer2 = custom_buffers.m_customBuffer2;
                    int Input2ProductionRate = (m_inputRate2 * finalProductionRate + 99) / 100;
                    if (CustomBuffer2 < Input2ProductionRate)
                    {
                        finalProductionRate = (CustomBuffer2 * 100 + m_inputRate2 - 1) / m_inputRate2;
                        problemStruct = Notification.AddProblems(problemStruct, (!flag) ? ((!IsRawMaterial(m_inputResource2)) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources) : Notification.Problem1.NoFishingGoods);
                    }
                }
                int InputBufferSize3 = 0;
                int CustomBuffer3 = 0;
                if (m_inputResource3 != TransferManager.TransferReason.None)
                {
                    InputBufferSize3 = GetInputBufferSize(ref buildingData, m_inputRate3);
                    CustomBuffer3 = custom_buffers.m_customBuffer3;
                    int Input3ProductionRate = (m_inputRate3 * finalProductionRate + 99) / 100;
                    if (CustomBuffer3 < Input3ProductionRate)
                    {
                        finalProductionRate = (CustomBuffer3 * 100 + m_inputRate3 - 1) / m_inputRate3;
                        problemStruct = Notification.AddProblems(problemStruct, (!flag) ? ((!IsRawMaterial(m_inputResource3)) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources) : Notification.Problem1.NoFishingGoods);
                    }
                }
                int InputBufferSize4 = 0;
                int CustomBuffer4 = 0;
                if (m_inputResource4 != TransferManager.TransferReason.None)
                {
                    InputBufferSize4 = GetInputBufferSize(ref buildingData, m_inputRate4);
                    CustomBuffer4 = custom_buffers.m_customBuffer4;
                    int Input4ProductionRate = (m_inputRate4 * finalProductionRate + 99) / 100;
                    if (CustomBuffer4 < Input4ProductionRate)
                    {
                        finalProductionRate = (CustomBuffer4 * 100 + m_inputRate4 - 1) / m_inputRate4;
                        problemStruct = Notification.AddProblems(problemStruct, (!flag) ? ((!IsRawMaterial(m_inputResource4)) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources) : Notification.Problem1.NoFishingGoods);
                    }
                }
                int InputBufferSize5 = 0;
                int CustomBuffer5 = 0;
                if (m_inputResource5 != ExtendedTransferManager.TransferReason.None)
                {
                    InputBufferSize5 = GetInputBufferSize(ref buildingData, m_inputRate5);
                    CustomBuffer5 = custom_buffers.m_customBuffer5;
                    int Input5ProductionRate = (m_inputRate5 * finalProductionRate + 99) / 100;
                    if (CustomBuffer5 < Input5ProductionRate)
                    {
                        finalProductionRate = (CustomBuffer5 * 100 + m_inputRate5 - 1) / m_inputRate5;
                        problemStruct = Notification.AddProblems(problemStruct, (!flag) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoFishingGoods);
                    }
                }
                int InputBufferSize6 = 0;
                int CustomBuffer6 = 0;
                if (m_inputResource6 != ExtendedTransferManager.TransferReason.None)
                {
                    InputBufferSize6 = GetInputBufferSize(ref buildingData, m_inputRate6);
                    CustomBuffer6 = custom_buffers.m_customBuffer6;
                    int Input6ProductionRate = (m_inputRate6 * finalProductionRate + 99) / 100;
                    if (CustomBuffer6 < Input6ProductionRate)
                    {
                        finalProductionRate = (CustomBuffer6 * 100 + m_inputRate6 - 1) / m_inputRate6;
                        problemStruct = Notification.AddProblems(problemStruct, (!flag) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoFishingGoods);
                    }
                }
                int InputBufferSize7 = 0;
                int CustomBuffer7 = 0;
                if (m_inputResource7 != ExtendedTransferManager.TransferReason.None)
                {
                    InputBufferSize7 = GetInputBufferSize(ref buildingData, m_inputRate7);
                    CustomBuffer7 = custom_buffers.m_customBuffer7;
                    int Input7ProductionRate = (m_inputRate7 * finalProductionRate + 99) / 100;
                    if (CustomBuffer7 < Input7ProductionRate)
                    {
                        finalProductionRate = (CustomBuffer7 * 100 + m_inputRate7 - 1) / m_inputRate7;
                        problemStruct = Notification.AddProblems(problemStruct, (!flag) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoFishingGoods);
                    }
                }
                int InputBufferSize8 = 0;
                int CustomBuffer8 = 0;
                if (m_inputResource8 != ExtendedTransferManager.TransferReason.None)
                {
                    InputBufferSize8 = GetInputBufferSize(ref buildingData, m_inputRate5);
                    CustomBuffer8 = custom_buffers.m_customBuffer8;
                    int Input8ProductionRate = (m_inputRate8 * finalProductionRate + 99) / 100;
                    if (CustomBuffer8 < Input8ProductionRate)
                    {
                        finalProductionRate = (CustomBuffer5 * 100 + m_inputRate8 - 1) / m_inputRate8;
                        problemStruct = Notification.AddProblems(problemStruct, (!flag) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoFishingGoods);
                    }
                }
                int OutputBufferSize = 0;
                int CustomBuffer9 = 0;
                if (m_outputResource != ExtendedTransferManager.TransferReason.None)
                {
                    OutputBufferSize = GetOutputBufferSize(ref buildingData);
                    CustomBuffer9 = (int)custom_buffers.m_customBuffer9;
                    int OutputProductionRate = (num13 * finalProductionRate + 99) / 100;
                    if (OutputBufferSize - CustomBuffer9 < OutputProductionRate)
                    {
                        OutputProductionRate = Mathf.Max(0, OutputBufferSize - CustomBuffer9);
                        finalProductionRate = (OutputProductionRate * 100 + num13 - 1) / num13;
                        if (m_outputVehicleCount != 0)
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
                var park = instance.m_parks.m_buffer[(int)b];
                if (m_inputResource1 != TransferManager.TransferReason.None)
                {
                    int Input1ProductionRate = (m_inputRate1 * finalProductionRate + 99) / 100;
                    CustomBuffer1 = Mathf.Max(0, CustomBuffer1 - Input1ProductionRate);
                    custom_buffers.m_customBuffer1 = (ushort)CustomBuffer1;
                    park.AddConsumptionAmount(m_inputResource1, Input1ProductionRate);
                }
                if (m_inputResource2 != TransferManager.TransferReason.None)
                {
                    int Input2ProductionRate = (m_inputRate2 * finalProductionRate + 99) / 100;
                    CustomBuffer2 = Mathf.Max(0, CustomBuffer2 - Input2ProductionRate);
                    custom_buffers.m_customBuffer2 = (ushort)CustomBuffer2;
                    park.AddConsumptionAmount(m_inputResource2, Input2ProductionRate);
                }
                if (m_inputResource3 != TransferManager.TransferReason.None)
                {
                    int Input3ProductionRate = (m_inputRate3 * finalProductionRate + 99) / 100;
                    CustomBuffer3 = Mathf.Max(0, CustomBuffer3 - Input3ProductionRate);
                    custom_buffers.m_customBuffer3 = (ushort)CustomBuffer3;
                    park.AddConsumptionAmount(m_inputResource3, Input3ProductionRate);
                }
                if (m_inputResource4 != TransferManager.TransferReason.None)
                {
                    int Input4ProductionRate = (m_inputRate4 * finalProductionRate + 99) / 100;
                    CustomBuffer4 = Mathf.Max(0, CustomBuffer4 - Input4ProductionRate);
                    custom_buffers.m_customBuffer4 = (ushort)CustomBuffer4;
                    park.AddConsumptionAmount(m_inputResource4, Input4ProductionRate);
                }
                if (m_inputResource5 != ExtendedTransferManager.TransferReason.None)
                {
                    int Input5ProductionRate = (m_inputRate5 * finalProductionRate + 99) / 100;
                    CustomBuffer5 = Mathf.Max(0, CustomBuffer5 - Input5ProductionRate);
                    custom_buffers.m_customBuffer5 = (ushort)CustomBuffer5;
                }
                if (m_inputResource6 != ExtendedTransferManager.TransferReason.None)
                {
                    int Input6ProductionRate = (m_inputRate6 * finalProductionRate + 99) / 100;
                    CustomBuffer6 = Mathf.Max(0, CustomBuffer6 - Input6ProductionRate);
                    custom_buffers.m_customBuffer6 = (ushort)CustomBuffer6;
                }
                if (m_inputResource7 != ExtendedTransferManager.TransferReason.None)
                {
                    int Input7ProductionRate = (m_inputRate5 * finalProductionRate + 99) / 100;
                    CustomBuffer7 = Mathf.Max(0, CustomBuffer7 - Input7ProductionRate);
                    custom_buffers.m_customBuffer7 = (ushort)CustomBuffer7;
                }
                if (m_inputResource8 != ExtendedTransferManager.TransferReason.None)
                {
                    int Input8ProductionRate = (m_inputRate8 * finalProductionRate + 99) / 100;
                    CustomBuffer8 = Mathf.Max(0, CustomBuffer8 - Input8ProductionRate);
                    custom_buffers.m_customBuffer8 = (ushort)CustomBuffer8;
                }
                if (m_outputResource != ExtendedTransferManager.TransferReason.None)
                {
                    int OutputProductionRate = (num13 * finalProductionRate + 99) / 100;
                    CustomBuffer9 = Mathf.Min(OutputBufferSize, CustomBuffer9 + OutputProductionRate);
                    custom_buffers.m_customBuffer9 = (ushort)CustomBuffer9;
                }
                BuildingCustomBuffersManager.SetCustomBuffer(buildingID, custom_buffers);
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
                        int InputSize1 = InputBufferSize1 - CustomBuffer1 - cargo1;
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
                        park.AddBufferStatus(m_inputResource1, CustomBuffer1, cargo1, InputBufferSize1);
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
                        int InputSize2 = InputBufferSize2 - CustomBuffer2 - cargo2;
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
                        park.AddBufferStatus(m_inputResource2, CustomBuffer2, cargo2, InputBufferSize2);
                    }
                    if (m_inputResource3 != TransferManager.TransferReason.None)
                    {
                        int count3 = 0;
                        int cargo3 = 0;
                        int capacity3 = 0;
                        int outside3 = 0;
                        base.CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource3, ref count3, ref cargo3, ref capacity3, ref outside3);
                        if (outside3 != 0)
                        {
                            TempOutput |= 4;
                        }
                        int InputSize3 = InputBufferSize3 - CustomBuffer3 - cargo3;
                        if (InputSize3 >= 4000)
                        {
                            TransferManager.TransferOffer transferOffer = default;
                            transferOffer.Priority = Mathf.Max(1, InputSize3 * 8 / InputBufferSize3);
                            transferOffer.Building = buildingID;
                            transferOffer.Position = buildingData.m_position;
                            transferOffer.Amount = 1;
                            transferOffer.Active = false;
                            Singleton<TransferManager>.instance.AddIncomingOffer(m_inputResource3, transferOffer);
                        }
                        park.AddBufferStatus(m_inputResource3, CustomBuffer3, cargo3, InputBufferSize3);
                    }
                    if (m_inputResource4 != TransferManager.TransferReason.None)
                    {
                        int count4 = 0;
                        int cargo4 = 0;
                        int capacity4 = 0;
                        int outside4 = 0;
                        base.CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource4, ref count4, ref cargo4, ref capacity4, ref outside4);
                        if (outside4 != 0)
                        {
                            TempOutput |= 8;
                        }
                        int InputSize4 = InputBufferSize4 - CustomBuffer4 - cargo4;
                        if (InputSize4 >= 4000)
                        {
                            TransferManager.TransferOffer transferOffer = default;
                            transferOffer.Priority = Mathf.Max(1, InputSize4 * 8 / InputBufferSize4);
                            transferOffer.Building = buildingID;
                            transferOffer.Position = buildingData.m_position;
                            transferOffer.Amount = 1;
                            transferOffer.Active = false;
                            Singleton<TransferManager>.instance.AddIncomingOffer(m_inputResource4, transferOffer);
                        }
                        park.AddBufferStatus(m_inputResource4, CustomBuffer4, cargo4, InputBufferSize4);
                    }
                    if (m_inputResource5 != ExtendedTransferManager.TransferReason.None)
                    {
                        int count5 = 0;
                        int cargo5 = 0;
                        int capacity5 = 0;
                        int outside5 = 0;
                        CalculateVehicles.CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource5, ref count5, ref cargo5, ref capacity5, ref outside5);
                        int InputSize5 = InputBufferSize5 - CustomBuffer5 - cargo5;
                        if (InputSize5 >= 4000)
                        {
                            ExtendedTransferManager.Offer transferOffer = default;
                            transferOffer.Building = buildingID;
                            transferOffer.Position = buildingData.m_position;
                            transferOffer.Amount = 1;
                            transferOffer.Active = false;
                            Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(m_inputResource5, transferOffer);
                        }
                    }
                    if (m_inputResource6 != ExtendedTransferManager.TransferReason.None)
                    {
                        int count6 = 0;
                        int cargo6 = 0;
                        int capacity6 = 0;
                        int outside6 = 0;
                        CalculateVehicles.CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource6, ref count6, ref cargo6, ref capacity6, ref outside6);
                        int InputSize6 = InputBufferSize6 - CustomBuffer6 - cargo6;
                        if (InputSize6 >= 4000)
                        {
                            ExtendedTransferManager.Offer transferOffer = default;
                            transferOffer.Building = buildingID;
                            transferOffer.Position = buildingData.m_position;
                            transferOffer.Amount = 1;
                            transferOffer.Active = false;
                            Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(m_inputResource6, transferOffer);
                        }
                    }
                    if (m_inputResource7 != ExtendedTransferManager.TransferReason.None)
                    {
                        int count7 = 0;
                        int cargo7 = 0;
                        int capacity7 = 0;
                        int outside7 = 0;
                        CalculateVehicles.CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource7, ref count7, ref cargo7, ref capacity7, ref outside7);
                        int InputSize7 = InputBufferSize7 - CustomBuffer7 - cargo7;
                        if (InputSize7 >= 4000)
                        {
                            ExtendedTransferManager.Offer transferOffer = default;
                            transferOffer.Building = buildingID;
                            transferOffer.Position = buildingData.m_position;
                            transferOffer.Amount = 1;
                            transferOffer.Active = false;
                            Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(m_inputResource7, transferOffer);
                        }
                    }
                    if (m_inputResource8 != ExtendedTransferManager.TransferReason.None)
                    {
                        int count8 = 0;
                        int cargo8 = 0;
                        int capacity8 = 0;
                        int outside8 = 0;
                        CalculateVehicles.CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource8, ref count8, ref cargo8, ref capacity8, ref outside8);
                        int InputSize8 = InputBufferSize8 - CustomBuffer8 - cargo8;
                        if (InputSize8 >= 4000)
                        {
                            ExtendedTransferManager.Offer transferOffer = default;
                            transferOffer.Building = buildingID;
                            transferOffer.Position = buildingData.m_position;
                            transferOffer.Amount = 1;
                            transferOffer.Active = false;
                            Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(m_inputResource8, transferOffer);
                        }
                    }
                    buildingData.m_tempImport |= (byte)TempOutput;
                    if (m_outputResource != ExtendedTransferManager.TransferReason.None)
                    {
                        int count9 = 0;
                        int cargo9 = 0;
                        int capacity9 = 0;
                        int outside9 = 0;
                        CalculateVehicles.CalculateOwnVehicles(buildingID, ref buildingData, m_outputResource, ref count9, ref cargo9, ref capacity9, ref outside9);
                        int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
                        int productionRate2 = PlayerBuildingAI.GetProductionRate(100, budget);
                        int OutputProductionRate = (productionRate2 * m_outputVehicleCount + 99) / 100;
                        if (CustomBuffer9 >= 8000 && count9 < OutputProductionRate)
                        {
                            ExtendedTransferManager.Offer transferOffer = default;
                            transferOffer.Building = buildingID;
                            transferOffer.Position = buildingData.m_position;
                            transferOffer.Amount = 1;
                            transferOffer.Active = true;
                            Singleton<ExtendedTransferManager>.instance.AddOutgoingOffer(m_outputResource, transferOffer);
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
            buildingData.m_education3 = (byte)Mathf.Clamp(finalProductionRate * num13 / Mathf.Max(1, m_outputRate), 0, 255);
            buildingData.m_health = (byte)Mathf.Clamp(finalProductionRate, 0, 255);
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
            base.ProduceGoods(buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
        }

        private void CheckCapacity(ushort buildingID, ref Building buildingData)
        {
            int outputBufferSize = GetOutputBufferSize(ref buildingData);
            int customBuffer = buildingData.m_customBuffer1;
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
            else if (customBuffer >= m_outputRate * 2)
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
            string text_water = LocaleFormatter.FormatGeneric("AIINFO_WATER_CONSUMPTION", new object[] { GetWaterConsumption() * 16 }) + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_ELECTRICITY_CONSUMPTION", new object[] { GetElectricityConsumption() * 16 });
            string text_prod = LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_PRODUCTION_RATE", new object[] { m_outputRate * 16 });
            if (m_outputResource != ExtendedTransferManager.TransferReason.None && m_outputVehicleCount != 0)
            {
                text_prod = text_prod + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLE_COUNT", new object[] { m_outputVehicleCount });
            }
            string baseTooltip = TooltipHelper.Append(base.GetLocalizedTooltip(), TooltipHelper.Format(new string[]
            {
                LocaleFormatter.Info1,
                text_water,
                LocaleFormatter.Info2,
                text_prod
            }));
            bool flag1 = m_inputResource1 != TransferManager.TransferReason.None;
            string text1 = ((m_inputResource1 == TransferManager.TransferReason.None) ? string.Empty : IndustryWorldInfoPanel.ResourceSpriteName(m_inputResource1, false));
            bool flag2 = m_inputResource2 != TransferManager.TransferReason.None;
            string text2 = ((m_inputResource2 == TransferManager.TransferReason.None) ? string.Empty : IndustryWorldInfoPanel.ResourceSpriteName(m_inputResource2, false));
            bool flag3 = m_inputResource3 != TransferManager.TransferReason.None;
            string text3 = ((m_inputResource3 == TransferManager.TransferReason.None) ? string.Empty : IndustryWorldInfoPanel.ResourceSpriteName(m_inputResource3, false));
            bool flag4 = m_inputResource4 != TransferManager.TransferReason.None;
            string text4 = ((m_inputResource4 == TransferManager.TransferReason.None) ? string.Empty :  IndustryWorldInfoPanel.ResourceSpriteName(m_inputResource4, false));
            bool flag5 = m_inputResource5 != ExtendedTransferManager.TransferReason.None;
            string text5 = ((m_inputResource5 == ExtendedTransferManager.TransferReason.None) ? string.Empty : m_inputResource4.ToString());
            bool flag6 = m_inputResource6 != ExtendedTransferManager.TransferReason.None;
            string text6 = ((m_inputResource6 == ExtendedTransferManager.TransferReason.None) ? string.Empty : m_inputResource4.ToString());
            bool flag7 = m_inputResource7 != ExtendedTransferManager.TransferReason.None;
            string text7 = ((m_inputResource7 == ExtendedTransferManager.TransferReason.None) ? string.Empty : m_inputResource4.ToString());
            bool flag8 = m_inputResource8 != ExtendedTransferManager.TransferReason.None;
            string text8 = ((m_inputResource8 == ExtendedTransferManager.TransferReason.None) ? string.Empty : m_inputResource4.ToString());
            string addTooltip = TooltipHelper.Format(new string[]
            {
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
                "outputVisible",
                "true"
            });
            string addTooltip2 = TooltipHelper.Format(new string[]
            {
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
                "output",
                m_outputResource.ToString()
            });
            baseTooltip = TooltipHelper.Append(baseTooltip, addTooltip);
            return TooltipHelper.Append(baseTooltip, addTooltip2);
        }

        public override string GetLocalizedStats(ushort buildingID, ref Building data)
        {
            var custom_buffers = BuildingCustomBuffersManager.GetCustomBuffer(buildingID);
            int m_customBuffer9 = custom_buffers.m_customBuffer9;
            int output_production_rate = m_customBuffer9 * m_outputRate * 16 / 100;
            string text = LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_PRODUCTION_RATE", new object[] { output_production_rate });
            if (m_outputResource != ExtendedTransferManager.TransferReason.None && m_outputVehicleCount != 0)
            {
                int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                int vehicle_count = (productionRate * m_outputVehicleCount + 99) / 100;
                int count = 0;
                int capacity = 0;
                int cargo = 0;
                int outside = 0;
                CalculateOwnVehicles(buildingID, ref data, m_outputResource, ref count, ref capacity, ref cargo, ref outside);
                text = text + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLES", new object[] { count, vehicle_count });
            }
            return text;
        }

        public override bool RequireRoadAccess()
        {
            return base.RequireRoadAccess() || m_inputResource1 != TransferManager.TransferReason.None
                || m_inputResource2 != TransferManager.TransferReason.None || m_inputResource3 != TransferManager.TransferReason.None
                || m_inputResource4 != TransferManager.TransferReason.None || m_inputResource5 != ExtendedTransferManager.TransferReason.None
                || m_inputResource6 != ExtendedTransferManager.TransferReason.None || m_inputResource7 != ExtendedTransferManager.TransferReason.None
                || m_inputResource8 != ExtendedTransferManager.TransferReason.None || m_outputResource != ExtendedTransferManager.TransferReason.None;
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

        private bool IsRawMaterial(TransferManager.TransferReason material)
        {
            switch (material)
            {
                case TransferManager.TransferReason.Oil:
                case TransferManager.TransferReason.Ore:
                case TransferManager.TransferReason.Logs:
                case TransferManager.TransferReason.Grain:
                    return true;
                default:
                    return false;
            }
        }

        protected void CalculateOwnVehicles(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort num = data.m_ownVehicles;
            int num2 = 0;
            while (num != 0)
            {
                if ((ExtendedTransferManager.TransferReason)instance.m_vehicles.m_buffer[num].m_transferType == material)
                {
                    VehicleInfo info = instance.m_vehicles.m_buffer[num].Info;
                    info.m_vehicleAI.GetSize(num, ref instance.m_vehicles.m_buffer[num], out var size, out var max);
                    cargo += Mathf.Min(size, max);
                    capacity += max;
                    count++;
                }
                num = instance.m_vehicles.m_buffer[num].m_nextOwnVehicle;
                if (++num2 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }

    }
}