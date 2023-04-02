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
        public int m_inputRate1 = 1000;

        [CustomizableProperty("Input Resource Rate 2")]
        public int m_inputRate2;

        [CustomizableProperty("Input Resource Rate 3")]
        public int m_inputRate3;

        [CustomizableProperty("Input Resource Rate 4")]
        public int m_inputRate4;

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

        public TransferManager.TransferReason m_inputResource1 = TransferManager.TransferReason.None;

        public TransferManager.TransferReason m_inputResource2 = TransferManager.TransferReason.None;

        public TransferManager.TransferReason m_inputResource3 = TransferManager.TransferReason.None;

        public ExtendedTransferManager.TransferReason m_inputResource4 = ExtendedTransferManager.TransferReason.None;

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
                    if (m_inputResource4 != ExtendedTransferManager.TransferReason.None && ((data.m_tempImport | data.m_finalImport) & 8) != 0)
                    {
                        return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource4];
                    }
                }
                else if (subInfoMode == InfoManager.SubInfoMode.WaterPower)
                {
                    if (m_outputResource != ExtendedTransferManager.TransferReason.None && (data.m_tempExport != 0 || data.m_finalExport != 0))
                    {
                        return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_outputResource];
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
            string text = base.GetDebugString(buildingID, ref data);
            if (m_inputResource1 != TransferManager.TransferReason.None)
            {
                int inputBufferSize = GetInputBufferSize1(buildingID, ref data);
                int customBuffer = (int)data.m_customBuffer2;
                int num = 0;
                int num2 = 0;
                int num3 = 0;
                int num4 = 0;
                base.CalculateGuestVehicles(buildingID, ref data, m_inputResource1, ref num, ref num2, ref num3, ref num4);
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}", new object[]
                {
                    text,
                    m_inputResource1.ToString(),
                    customBuffer,
                    num2,
                    inputBufferSize
                });
            }
            if (m_inputResource2 != TransferManager.TransferReason.None)
            {
                int inputBufferSize2 = GetInputBufferSize2(buildingID, ref data);
                int num5 = ((int)data.m_teens << 8) | (int)data.m_youngs;
                int num6 = 0;
                int num7 = 0;
                int num8 = 0;
                int num9 = 0;
                base.CalculateGuestVehicles(buildingID, ref data, m_inputResource2, ref num6, ref num7, ref num8, ref num9);
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}", new object[]
                {
                    text,
                    m_inputResource2.ToString(),
                    num5,
                    num7,
                    inputBufferSize2
                });
            }
            if (m_inputResource3 != TransferManager.TransferReason.None)
            {
                int inputBufferSize3 = GetInputBufferSize3(buildingID, ref data);
                int num10 = ((int)data.m_adults << 8) | (int)data.m_seniors;
                int num11 = 0;
                int num12 = 0;
                int num13 = 0;
                int num14 = 0;
                base.CalculateGuestVehicles(buildingID, ref data, m_inputResource3, ref num11, ref num12, ref num13, ref num14);
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}", new object[]
                {
                    text,
                    m_inputResource3.ToString(),
                    num10,
                    num12,
                    inputBufferSize3
                });
            }
            if (m_inputResource4 != ExtendedTransferManager.TransferReason.None)
            {
                int inputBufferSize4 = GetInputBufferSize4(buildingID, ref data);
                int num15 = ((int)data.m_education1 << 8) | (int)data.m_education2;
                int num16 = 0;
                int num17 = 0;
                int num18 = 0;
                int num19 = 0;
                CalculateVehicles.CalculateGuestVehicles(buildingID, ref data, m_inputResource4, ref num16, ref num17, ref num18, ref num19);
                text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3}) / {4}", new object[]
                {
                    text,
                    m_inputResource4.ToString(),
                    num15,
                    num17,
                    inputBufferSize4
                });
            }
            if (m_outputResource != ExtendedTransferManager.TransferReason.None)
            {
                int outputBufferSize = GetOutputBufferSize(buildingID, ref data);
                int customBuffer2 = (int)data.m_customBuffer1;
                text = StringUtils.SafeFormat("{0}\n{1}: {2} / {3}", new object[]
                {
                    text,
                    m_outputResource.ToString(),
                    customBuffer2,
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

        void IExtendedBuildingAI.GetMaterialAmount(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, out int amount, out int max)
	{
	    amount = 0;
	    max = 0;
	}

        void IExtendedBuildingAI.StartTransfer(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, ExtendedTransferManager.Offer offer)
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
                        extended.StartTransfer(num, ref vehicles.m_buffer[(int)num], material, offer);
                    }
                }
            }
        }

        public override void ModifyMaterialBuffer(ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int amountDelta)
        {
            var custom_buffers = BuildingCustomBuffersManager.GetCustomBuffer(buildingID);
            if (material == m_inputResource1)
            {
                int inputBufferSize1 = GetInputBufferSize1(buildingID, ref data);
                int m_customBuffer1 = (int)custom_buffers.m_customBuffer1;
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer1, inputBufferSize1 - m_customBuffer1);
                m_customBuffer1 += amountDelta;
                custom_buffers.m_customBuffer1 = (ushort)m_customBuffer1;
            }
            else if (material == m_inputResource2)
            {
                int inputBufferSize2 = GetInputBufferSize2(buildingID, ref data);
                int m_customBuffer2 = (int)custom_buffers.m_customBuffer2;
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer2, inputBufferSize2 - m_customBuffer2);
                m_customBuffer2 += amountDelta;
                custom_buffers.m_customBuffer2 = (ushort)m_customBuffer2;
            }
            else if (material == m_inputResource3)
            {
                int inputBufferSize3 = GetInputBufferSize3(buildingID, ref data);
                int m_customBuffer3 = (int)custom_buffers.m_customBuffer3;
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer3, inputBufferSize3 - m_customBuffer3);
                m_customBuffer3 += amountDelta;
                custom_buffers.m_customBuffer3 = (ushort)m_customBuffer3;
            }
            else
            {
                base.ModifyMaterialBuffer(buildingID, ref data, material, ref amountDelta);
            }
        }

        void IExtendedBuildingAI.ModifyMaterialBuffer(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, ref int amountDelta)
        {
            var custom_buffers = BuildingCustomBuffersManager.GetCustomBuffer(buildingID);
            if (material == m_inputResource4)
            {
                int inputBufferSize4 = GetInputBufferSize4(buildingID, ref data);
                int m_customBuffer4 = (int)custom_buffers.m_customBuffer4;
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer4, inputBufferSize4 - m_customBuffer4);
                m_customBuffer4 += amountDelta;
                custom_buffers.m_customBuffer4 = (ushort)m_customBuffer4;
            }
            else if (material == m_outputResource)
            {
                int outputBufferSize = GetOutputBufferSize(buildingID, ref data);
                int m_customBuffer5 = (int)custom_buffers.m_customBuffer5;
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer5, outputBufferSize - m_customBuffer5);
                m_customBuffer5 += amountDelta;
                custom_buffers.m_customBuffer5 = (ushort)m_customBuffer5;
            }
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
            if (m_inputResource4 != ExtendedTransferManager.TransferReason.None)
            {
                Singleton<ExtendedTransferManager>.instance.RemoveIncomingOffer(m_inputResource4, offer);
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
                    int num17;
                    int num18;
                    instance.m_parks.m_buffer[(int)b].GetProductionFactors(out num17, out num18);
                    finalProductionRate = (finalProductionRate * num17 + 50) / 100;
                    num16 = (num16 * num18 + 50) / 100;
                }
                else if (m_industryType != DistrictPark.ParkType.Industry)
                {
                    finalProductionRate = 0;
                }
                int num19 = 0;
                int num20 = 0;
                var custom_buffers = BuildingCustomBuffersManager.GetCustomBuffer(buildingID);
                if (m_inputResource1 != TransferManager.TransferReason.None)
                {
                    num19 = GetInputBufferSize1(parkPolicies, (int)instance.m_parks.m_buffer[(int)b].m_finalStorageDelta);
                    num20 = (int)custom_buffers.m_customBuffer1;
                    int num21 = (m_inputRate1 * finalProductionRate + 99) / 100;
                    if (num20 < num21)
                    {
                        finalProductionRate = (num20 * 100 + m_inputRate1 - 1) / m_inputRate1;
                        problemStruct = Notification.AddProblems(problemStruct, (!flag) ? ((!IsRawMaterial(m_inputResource1)) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources) : Notification.Problem1.NoFishingGoods);
                    }
                }
                int num22 = 0;
                int num23 = 0;
                if (m_inputResource2 != TransferManager.TransferReason.None)
                {
                    num22 = GetInputBufferSize2(parkPolicies, (int)instance.m_parks.m_buffer[(int)b].m_finalStorageDelta);
                    num23 = (int)custom_buffers.m_customBuffer2;
                    int num24 = (m_inputRate2 * finalProductionRate + 99) / 100;
                    if (num23 < num24)
                    {
                        finalProductionRate = (num23 * 100 + m_inputRate2 - 1) / m_inputRate2;
                        problemStruct = Notification.AddProblems(problemStruct, (!flag) ? ((!IsRawMaterial(m_inputResource2)) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources) : Notification.Problem1.NoFishingGoods);
                    }
                }
                int num25 = 0;
                int num26 = 0;
                if (m_inputResource3 != TransferManager.TransferReason.None)
                {
                    num25 = GetInputBufferSize3(parkPolicies, (int)instance.m_parks.m_buffer[(int)b].m_finalStorageDelta);
                    num26 = (int)custom_buffers.m_customBuffer3;
                    int num27 = (m_inputRate3 * finalProductionRate + 99) / 100;
                    if (num26 < num27)
                    {
                        finalProductionRate = (num26 * 100 + m_inputRate3 - 1) / m_inputRate3;
                        problemStruct = Notification.AddProblems(problemStruct, (!flag) ? ((!IsRawMaterial(m_inputResource3)) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources) : Notification.Problem1.NoFishingGoods);
                    }
                }
                int num28 = 0;
                int num29 = 0;
                if (m_inputResource4 != ExtendedTransferManager.TransferReason.None)
                {
                    num28 = GetInputBufferSize4(parkPolicies, (int)instance.m_parks.m_buffer[(int)b].m_finalStorageDelta);
                    num29 = (int)custom_buffers.m_customBuffer4;
                    int num30 = (m_inputRate4 * finalProductionRate + 99) / 100;
                    if (num29 < num30)
                    {
                        finalProductionRate = (num29 * 100 + m_inputRate4 - 1) / m_inputRate4;
                        problemStruct = Notification.AddProblems(problemStruct, (!flag) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoFishingGoods);
                    }
                }
                int num31 = 0;
                int num32 = 0;
                if (m_outputResource != ExtendedTransferManager.TransferReason.None)
                {
                    num31 = GetOutputBufferSize(parkPolicies, (int)instance.m_parks.m_buffer[(int)b].m_finalStorageDelta);
                    num32 = (int)custom_buffers.m_customBuffer5;
                    int num33 = (num13 * finalProductionRate + 99) / 100;
                    if (num31 - num32 < num33)
                    {
                        num33 = Mathf.Max(0, num31 - num32);
                        finalProductionRate = (num33 * 100 + num13 - 1) / num13;
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
                if (m_inputResource1 != TransferManager.TransferReason.None)
                {
                    int num34 = (m_inputRate1 * finalProductionRate + 99) / 100;
                    num20 = Mathf.Max(0, num20 - num34);
                    custom_buffers.m_customBuffer1 = (ushort)num20;
                    instance.m_parks.m_buffer[(int)b].AddConsumptionAmount(m_inputResource1, num34);
                }
                if (m_inputResource2 != TransferManager.TransferReason.None)
                {
                    int num35 = (m_inputRate2 * finalProductionRate + 99) / 100;
                    num23 = Mathf.Max(0, num23 - num35);
                    custom_buffers.m_customBuffer2 = (ushort)num23;
                    instance.m_parks.m_buffer[(int)b].AddConsumptionAmount(m_inputResource2, num35);
                }
                if (m_inputResource3 != TransferManager.TransferReason.None)
                {
                    int num36 = (m_inputRate3 * finalProductionRate + 99) / 100;
                    num26 = Mathf.Max(0, num26 - num36);
                    custom_buffers.m_customBuffer3 = (ushort)num26;
                    instance.m_parks.m_buffer[(int)b].AddConsumptionAmount(m_inputResource3, num36);
                }
                if (m_inputResource4 != ExtendedTransferManager.TransferReason.None)
                {
                    int num37 = (m_inputRate4 * finalProductionRate + 99) / 100;
                    num29 = Mathf.Max(0, num29 - num37);
                    custom_buffers.m_customBuffer4 = (ushort)num29;
                }
                if (m_outputResource != ExtendedTransferManager.TransferReason.None)
                {
                    int num38 = (num13 * finalProductionRate + 99) / 100;
                    num32 = Mathf.Min(num31, num32 + num38);
                    custom_buffers.m_customBuffer5 = (ushort)num32;
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
                    int num39 = 0;
                    if (m_inputResource1 != TransferManager.TransferReason.None)
                    {
                        int num40 = 0;
                        int num41 = 0;
                        int num42 = 0;
                        int num43 = 0;
                        base.CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource1, ref num40, ref num41, ref num42, ref num43);
                        if (num43 != 0)
                        {
                            num39 |= 1;
                        }
                        int num44 = num19 - num20 - num41;
                        if (num44 >= 4000)
                        {
                            TransferManager.TransferOffer transferOffer = default(TransferManager.TransferOffer);
                            transferOffer.Priority = Mathf.Max(1, num44 * 8 / num19);
                            transferOffer.Building = buildingID;
                            transferOffer.Position = buildingData.m_position;
                            transferOffer.Amount = 1;
                            transferOffer.Active = false;
                            Singleton<TransferManager>.instance.AddIncomingOffer(m_inputResource1, transferOffer);
                        }
                        instance.m_parks.m_buffer[(int)b].AddBufferStatus(m_inputResource1, num20, num41, num19);
                    }
                    if (m_inputResource2 != TransferManager.TransferReason.None)
                    {
                        int num45 = 0;
                        int num46 = 0;
                        int num47 = 0;
                        int num48 = 0;
                        base.CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource2, ref num45, ref num46, ref num47, ref num48);
                        if (num48 != 0)
                        {
                            num39 |= 2;
                        }
                        int num49 = num22 - num23 - num46;
                        if (num49 >= 4000)
                        {
                            TransferManager.TransferOffer transferOffer2 = default(TransferManager.TransferOffer);
                            transferOffer2.Priority = Mathf.Max(1, num49 * 8 / num22);
                            transferOffer2.Building = buildingID;
                            transferOffer2.Position = buildingData.m_position;
                            transferOffer2.Amount = 1;
                            transferOffer2.Active = false;
                            Singleton<TransferManager>.instance.AddIncomingOffer(m_inputResource2, transferOffer2);
                        }
                        instance.m_parks.m_buffer[(int)b].AddBufferStatus(m_inputResource2, num23, num46, num22);
                    }
                    if (m_inputResource3 != TransferManager.TransferReason.None)
                    {
                        int num50 = 0;
                        int num51 = 0;
                        int num52 = 0;
                        int num53 = 0;
                        base.CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource3, ref num50, ref num51, ref num52, ref num53);
                        if (num53 != 0)
                        {
                            num39 |= 4;
                        }
                        int num54 = num25 - num26 - num51;
                        if (num54 >= 4000)
                        {
                            TransferManager.TransferOffer transferOffer3 = default(TransferManager.TransferOffer);
                            transferOffer3.Priority = Mathf.Max(1, num54 * 8 / num25);
                            transferOffer3.Building = buildingID;
                            transferOffer3.Position = buildingData.m_position;
                            transferOffer3.Amount = 1;
                            transferOffer3.Active = false;
                            Singleton<TransferManager>.instance.AddIncomingOffer(m_inputResource3, transferOffer3);
                        }
                        instance.m_parks.m_buffer[(int)b].AddBufferStatus(m_inputResource3, num26, num51, num25);
                    }
                    if (m_inputResource4 != ExtendedTransferManager.TransferReason.None)
                    {
                        int num55 = 0;
                        int num56 = 0;
                        int num57 = 0;
                        int num58 = 0;
                        CalculateVehicles.CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource4, ref num55, ref num56, ref num57, ref num58);
                        if (num58 != 0)
                        {
                            num39 |= 8;
                        }
                        int num59 = num28 - num29 - num56;
                        if (num59 >= 4000)
                        {
                            ExtendedTransferManager.Offer offer4 = default(ExtendedTransferManager.Offer);
                            offer4.Building = buildingID;
                            offer4.Position = buildingData.m_position;
                            offer4.Amount = 1;
                            offer4.Active = false;
                            Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(m_inputResource4, offer4);
                        }
                    }
                    buildingData.m_tempImport |= (byte)num39;
                    if (m_outputResource != ExtendedTransferManager.TransferReason.None)
                    {
                        if (m_outputVehicleCount != 0)
                        {
                            int num61 = 0;
                            int num62 = 0;
                            int num63 = 0;
                            int num64 = 0;
                            CalculateOwnVehicles(buildingID, ref buildingData, m_outputResource, ref num61, ref num62, ref num63, ref num64);
                            buildingData.m_tempExport = (byte)Mathf.Clamp(num64, (int)buildingData.m_tempExport, 255);
                            int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
                            int productionRate2 = PlayerBuildingAI.GetProductionRate(100, budget);
                            int num65 = (productionRate2 * m_outputVehicleCount + 99) / 100;
                            int num66 = num32;
                            if (num66 >= 8000 && num61 < num65)
                            {
                                ExtendedTransferManager.Offer offer5 = default(ExtendedTransferManager.Offer);
                                offer5.Building = buildingID;
                                offer5.Position = buildingData.m_position;
                                offer5.Amount = 1;
                                offer5.Active = true;
                                Singleton<ExtendedTransferManager>.instance.AddOutgoingOffer(m_outputResource, offer5);
                            }
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
            int outputBufferSize = GetOutputBufferSize(buildingID, ref buildingData);
            int customBuffer = (int)buildingData.m_customBuffer1;
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
            string text = LocaleFormatter.FormatGeneric("AIINFO_WATER_CONSUMPTION", new object[] { GetWaterConsumption() * 16 }) + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_ELECTRICITY_CONSUMPTION", new object[] { GetElectricityConsumption() * 16 });
            string text2 = LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_PRODUCTION_RATE", new object[] { m_outputRate * 16 });
            if (m_outputResource != ExtendedTransferManager.TransferReason.None && m_outputVehicleCount != 0)
            {
                text2 = text2 + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLE_COUNT", new object[] { m_outputVehicleCount });
            }
            string text3 = TooltipHelper.Append(base.GetLocalizedTooltip(), TooltipHelper.Format(new string[]
            {
                LocaleFormatter.Info1,
                text,
                LocaleFormatter.Info2,
                text2
            }));
            bool flag = m_inputResource1 != TransferManager.TransferReason.None;
            string text5 = ((m_inputResource1 == TransferManager.TransferReason.None) ? string.Empty : IndustryWorldInfoPanel.ResourceSpriteName(m_inputResource1, false));
            bool flag2 = m_inputResource2 != TransferManager.TransferReason.None;
            string text6 = ((m_inputResource2 == TransferManager.TransferReason.None) ? string.Empty : IndustryWorldInfoPanel.ResourceSpriteName(m_inputResource2, false));
            bool flag3 = m_inputResource3 != TransferManager.TransferReason.None;
            string text7 = ((m_inputResource3 == TransferManager.TransferReason.None) ? string.Empty : IndustryWorldInfoPanel.ResourceSpriteName(m_inputResource3, false));
            bool flag4 = m_inputResource4 != ExtendedTransferManager.TransferReason.None;
            string text8 = ((m_inputResource4 == ExtendedTransferManager.TransferReason.None) ? string.Empty : m_inputResource4.ToString());
            string text9 = TooltipHelper.Format(new string[]
            {
                "arrowVisible",
                "true",
                "input1Visible",
                flag.ToString(),
                "input2Visible",
                flag2.ToString(),
                "input3Visible",
                flag3.ToString(),
                "input4Visible",
                flag4.ToString(),
                "outputVisible",
                "true"
            });
            string text10 = TooltipHelper.Format(new string[]
            {
                "input1",
                text5,
                "input2",
                text6,
                "input3",
                text7,
                "input4",
                text8,
                "output",
                m_outputResource.ToString()
            });
            text3 = TooltipHelper.Append(text3, text9);
            return TooltipHelper.Append(text3, text10);
        }

        public override string GetLocalizedStats(ushort buildingID, ref Building data)
        {
            int num = (int)data.m_education3 * m_outputRate * 16 / 100;
            string text = LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_PRODUCTION_RATE", new object[] { num });
            if (m_outputResource != ExtendedTransferManager.TransferReason.None && m_outputVehicleCount != 0)
            {
                int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                int num2 = (productionRate * m_outputVehicleCount + 99) / 100;
                int num3 = 0;
                int num4 = 0;
                int num5 = 0;
                int num6 = 0;
                CalculateOwnVehicles(buildingID, ref data, m_outputResource, ref num3, ref num4, ref num5, ref num6);
                text = text + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLES", new object[] { num3, num2 });
            }
            return text;
        }

        public override bool RequireRoadAccess()
        {
            return base.RequireRoadAccess() || m_inputResource1 != TransferManager.TransferReason.None || m_inputResource2 != TransferManager.TransferReason.None || m_inputResource3 != TransferManager.TransferReason.None || m_inputResource4 != ExtendedTransferManager.TransferReason.None || m_outputResource != ExtendedTransferManager.TransferReason.None;
        }

        public int GetInputBufferSize1(ushort buildingID, ref Building data)
        {
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
            return GetInputBufferSize1(instance.m_parks.m_buffer[(int)b].m_parkPolicies, (int)instance.m_parks.m_buffer[(int)b].m_finalStorageDelta);
        }

        private int GetInputBufferSize1(DistrictPolicies.Park policies, int storageDelta)
        {
            int num = m_inputRate1 * 32 + 8000;
            if ((policies & DistrictPolicies.Park.ImprovedLogistics) != DistrictPolicies.Park.None)
            {
                num = (num * 6 + 4) / 5;
            }
            num = (num * (100 + storageDelta) + 50) / 100;
            return Mathf.Clamp(num, 8000, 64000);
        }

        public int GetInputBufferSize2(ushort buildingID, ref Building data)
        {
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
            return GetInputBufferSize2(instance.m_parks.m_buffer[(int)b].m_parkPolicies, (int)instance.m_parks.m_buffer[(int)b].m_finalStorageDelta);
        }

        private int GetInputBufferSize2(DistrictPolicies.Park policies, int storageDelta)
        {
            int num = m_inputRate2 * 32 + 8000;
            if ((policies & DistrictPolicies.Park.ImprovedLogistics) != DistrictPolicies.Park.None)
            {
                num = (num * 6 + 4) / 5;
            }
            num = (num * (100 + storageDelta) + 50) / 100;
            return Mathf.Clamp(num, 8000, 64000);
        }

        public int GetInputBufferSize3(ushort buildingID, ref Building data)
        {
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
            return GetInputBufferSize3(instance.m_parks.m_buffer[(int)b].m_parkPolicies, (int)instance.m_parks.m_buffer[(int)b].m_finalStorageDelta);
        }

        private int GetInputBufferSize3(DistrictPolicies.Park policies, int storageDelta)
        {
            int num = m_inputRate3 * 32 + 8000;
            if ((policies & DistrictPolicies.Park.ImprovedLogistics) != DistrictPolicies.Park.None)
            {
                num = (num * 6 + 4) / 5;
            }
            num = (num * (100 + storageDelta) + 50) / 100;
            return Mathf.Clamp(num, 8000, 64000);
        }

        public int GetInputBufferSize4(ushort buildingID, ref Building data)
        {
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
            return GetInputBufferSize4(instance.m_parks.m_buffer[(int)b].m_parkPolicies, (int)instance.m_parks.m_buffer[(int)b].m_finalStorageDelta);
        }

        private int GetInputBufferSize4(DistrictPolicies.Park policies, int storageDelta)
        {
            int num = m_inputRate1 * 32 + 8000;
            if ((policies & DistrictPolicies.Park.ImprovedLogistics) != DistrictPolicies.Park.None)
            {
                num = (num * 6 + 4) / 5;
            }
            num = (num * (100 + storageDelta) + 50) / 100;
            return Mathf.Clamp(num, 8000, 64000);
        }

        public int GetOutputBufferSize(ushort buildingID, ref Building data)
        {
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
            return GetOutputBufferSize(instance.m_parks.m_buffer[(int)b].m_parkPolicies, (int)instance.m_parks.m_buffer[(int)b].m_finalStorageDelta);
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

        private int GetOutputBufferSize(DistrictPolicies.Park policies, int storageDelta)
        {
            if (m_outputVehicleCount == 0)
            {
                int num = m_outputRate * 100;
                return Mathf.Clamp(num, 1, 64000);
            }
            int num2 = m_outputRate * 32 + 8000;
            if ((policies & DistrictPolicies.Park.ImprovedLogistics) != DistrictPolicies.Park.None)
            {
                num2 = (num2 * 6 + 4) / 5;
            }
            num2 = (num2 * (100 + storageDelta) + 50) / 100;
            return Mathf.Clamp(num2, 8000, 64000);
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