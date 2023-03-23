using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.Math;
using System;
using UnityEngine;
using MoreTransferReasons.Code;
using IndustriesMeetsSunsetHarbor.Utils;

namespace IndustriesMeetsSunsetHarbor.AI
{
    public class RestaurantAI : PlayerBuildingAI
    {
        [CustomizableProperty("Uneducated Workers", "Workers", 0)]
        public int m_workPlaceCount0 = 3;

        [CustomizableProperty("Educated Workers", "Workers", 1)]
        public int m_workPlaceCount1 = 2;

        [CustomizableProperty("Well Educated Workers", "Workers", 2)]
        public int m_workPlaceCount2 = 3;

        [CustomizableProperty("Highly Educated Workers", "Workers", 3)]
        public int m_workPlaceCount3 = 2;

        [CustomizableProperty("Delivery Vehicle Count")]
        public int m_DeliveryVehicleCount = 10;

        [CustomizableProperty("Visitors Capacity")]
        public int m_VisitorsCapacity = 100;

        [CustomizableProperty("Food Delivery Accumulation")]
        public int m_foodDeliveryAccumulation = 100;

        [CustomizableProperty("Noise Accumulation")]
        public int m_noiseAccumulation;

        [CustomizableProperty("Food Delivery Radius")]
        public float m_foodDeliveryRadius = 400f;

        [CustomizableProperty("Noise Radius")]
        public float m_noiseRadius = 200f;

        [CustomizableProperty("Input Resource Rate 1")]
        public int m_inputRate1 = 1000;

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

        [CustomizableProperty("Output Resource Rate")]
        public int m_outputRate = 1000;

        [CustomizableProperty("Food Sale Price")]
        public int m_goodsSellPrice = 1500;

        [CustomizableProperty("Quality (values: 1-3 including 1 and 3)")]
        public int quality = 1;

        [NonSerialized]
        private bool m_hasBufferStatusMeshes;

        public ExtendedTransferManager.TransferReason m_inputResource1 = ExtendedTransferManager.TransferReason.DrinkSupplies;

        public ExtendedTransferManager.TransferReason m_inputResource2 = ExtendedTransferManager.TransferReason.FoodSupplies;

        public ExtendedTransferManager.TransferReason m_inputResource3 = ExtendedTransferManager.TransferReason.None;

        public TransferManager.TransferReason m_inputResource4 = TransferManager.TransferReason.None;

        public TransferManager.TransferReason m_inputResource5 = TransferManager.TransferReason.None;

        public TransferManager.TransferReason m_inputResource6 = TransferManager.TransferReason.None;

        public TransferManager.TransferReason m_inputResource7 = TransferManager.TransferReason.None;

        public ExtendedTransferManager.TransferReason m_outputResource = ExtendedTransferManager.TransferReason.None; // delivery low/medium/high

        public override Color GetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode)
        {
            switch (infoMode)
            {
                case InfoManager.InfoMode.Connections:
                    switch (subInfoMode)
                    {
                        case InfoManager.SubInfoMode.Default:
                            if (m_inputResource1 != ExtendedTransferManager.TransferReason.None && ((uint)(data.m_tempImport | data.m_finalImport) & (true ? 1u : 0u)) != 0)
                            {
                                return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource1];
                            }
                            if (m_inputResource2 != ExtendedTransferManager.TransferReason.None && ((uint)(data.m_tempImport | data.m_finalImport) & 2u) != 0)
                            {
                                return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource2];
                            }
                            if (m_inputResource3 != ExtendedTransferManager.TransferReason.None && ((uint)(data.m_tempImport | data.m_finalImport) & 4u) != 0)
                            {
                                return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource3];
                            }
                            if (m_inputResource4 != TransferManager.TransferReason.None && ((uint)(data.m_tempImport | data.m_finalImport) & 8u) != 0)
                            {
                                return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource4];
                            }
                            if (m_inputResource5 != TransferManager.TransferReason.None && ((uint)(data.m_tempImport | data.m_finalImport) & 16u) != 0)
                            {
                                return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource5];
                            }
                            if (m_inputResource6 != TransferManager.TransferReason.None && ((uint)(data.m_tempImport | data.m_finalImport) & 32u) != 0)
                            {
                                return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource6];
                            }
                            if (m_inputResource7 != TransferManager.TransferReason.None && ((uint)(data.m_tempImport | data.m_finalImport) & 64u) != 0)
                            {
                                return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_inputResource7];
                            }
                            break;
                        case InfoManager.SubInfoMode.WaterPower:
                            {
                                if (m_outputResource != ExtendedTransferManager.TransferReason.None && (data.m_tempExport != 0 || data.m_finalExport != 0))
                                {
                                    return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_outputResource];
                                }
                                break;
                            }
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                case InfoManager.InfoMode.BuildingLevel:
                    if (ShowConsumption(buildingID, ref data))
                    {
                        Color a = Singleton<ZoneManager>.instance.m_properties.m_zoneColors[4];
                        Color b = Singleton<ZoneManager>.instance.m_properties.m_zoneColors[5];
                        Color color = Color.Lerp(a, b, 0.5f) * 0.5f;
                        if (m_info.m_class.isCommercialLowGeneric || m_info.m_class.isCommercialHighGenegic || m_info.m_class.isCommercialWallToWall)
                        {
                            return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_neutralColor, color, 0.333f + (float)(int)data.m_level * 0.333f);
                        }
                        return color;
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                case InfoManager.InfoMode.Tourism:
                    if (ShowConsumption(buildingID, ref data))
                    {
                        switch (subInfoMode)
                        {
                            case InfoManager.SubInfoMode.Default:
                                if (data.m_tempExport != 0 || data.m_finalExport != 0)
                                {
                                    return CommonBuildingAI.GetTourismColor(Mathf.Max(data.m_tempExport, data.m_finalExport));
                                }
                                return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                            case InfoManager.SubInfoMode.WaterPower:
                                {
                                    DistrictManager instance = Singleton<DistrictManager>.instance;
                                    byte district = instance.GetDistrict(data.m_position);
                                    DistrictPolicies.CityPlanning cityPlanningPolicies = instance.m_districts.m_buffer[district].m_cityPlanningPolicies;
                                    DistrictPolicies.Taxation taxationPolicies = instance.m_districts.m_buffer[district].m_taxationPolicies;
                                    int taxRate = GetTaxRate(buildingID, ref data, taxationPolicies);
                                    GetAccumulation(new Randomizer(buildingID), data.m_adults * 100, taxRate, cityPlanningPolicies, taxationPolicies, out var _, out var attractiveness);
                                    if (attractiveness != 0)
                                    {
                                        return CommonBuildingAI.GetAttractivenessColor(attractiveness);
                                    }
                                    return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                                }
                            default:
                                return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                        }
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                case InfoManager.InfoMode.Financial:
                    if (subInfoMode == InfoManager.SubInfoMode.WaterPower)
                    {
                        Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(ImmaterialResourceManager.Resource.CashCollecting, data.m_position, out var local);
                        return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_negativeColorB, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_targetColor, Mathf.Clamp01((float)local * 0.01f));
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                case InfoManager.InfoMode.NoisePollution:
                    {
                        int noiseAccumulation = m_noiseAccumulation;
                        return CommonBuildingAI.GetNoisePollutionColor(noiseAccumulation);
                    }
                default:
                    return base.GetColor(buildingID, ref data, infoMode, subInfoMode);
            }
        }

        private int GetTaxRate(ushort buildingID, ref Building buildingData, DistrictPolicies.Taxation taxationPolicies)
        {
            return Singleton<EconomyManager>.instance.GetTaxRate(m_info.m_class.m_service, m_info.m_class.m_subService, (ItemClass.Level)buildingData.m_level, taxationPolicies);
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
            mode = InfoManager.InfoMode.Happiness;
            subMode = InfoManager.SubInfoMode.WaterPower;
        }

        public override void CreateBuilding(ushort buildingID, ref Building data)
        {
            base.CreateBuilding(buildingID, ref data);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, 0, workCount, m_VisitorsCapacity, 0, 0);
        }

        public override void BuildingLoaded(ushort buildingID, ref Building data, uint version)
        {
            base.BuildingLoaded(buildingID, ref data, version);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            EnsureCitizenUnits(buildingID, ref data, 0, workCount, m_VisitorsCapacity, 0);
        }

        public override void ReleaseBuilding(ushort buildingID, ref Building data)
        {
            base.ReleaseBuilding(buildingID, ref data);
        }

        public override void EndRelocating(ushort buildingID, ref Building data)
        {
            base.EndRelocating(buildingID, ref data);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            EnsureCitizenUnits(buildingID, ref data, 0, workCount, m_VisitorsCapacity, 0);
        }

        protected override void ManualActivation(ushort buildingID, ref Building buildingData)
        {
            Vector3 position = buildingData.m_position;
            position.y += m_info.m_size.y;
            Singleton<NotificationManager>.instance.AddEvent(NotificationEvent.Type.GainTaxes, position, 1.5f);
            Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.GainHappiness, ImmaterialResourceManager.Resource.DeathCare, m_foodDeliveryAccumulation, m_foodDeliveryRadius);
        }

        protected override void ManualDeactivation(ushort buildingID, ref Building buildingData)
        {
            if ((buildingData.m_flags & Building.Flags.Collapsed) != 0)
            {
                Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Happy, ImmaterialResourceManager.Resource.Abandonment, -buildingData.Width * buildingData.Length, 64f);
            }
            else if (m_foodDeliveryAccumulation != 0)
            {
                Vector3 position = buildingData.m_position;
                position.y += m_info.m_size.y;
                Singleton<NotificationManager>.instance.AddEvent(NotificationEvent.Type.LoseHappiness, position, 1.5f);
                Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Sad, ImmaterialResourceManager.Resource.DeathCare, -m_foodDeliveryAccumulation, m_foodDeliveryRadius);
            }
        }

        public override void SimulationStep(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            base.SimulationStep(buildingID, ref buildingData, ref frameData);
            if ((Singleton<SimulationManager>.instance.m_currentFrameIndex & 0xFFF) >= 3840)
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
            if ((object)m_info != null && (object)m_info.m_class != null && m_info.m_class.m_service == ItemClass.Service.Fishing)
            {
                GuideController properties = Singleton<GuideManager>.instance.m_properties;
                if ((object)properties != null && Singleton<BuildingManager>.instance.m_intercityBusStationBuilt != null)
                {
                    Singleton<BuildingManager>.instance.m_intercityBusStationBuilt.Activate(properties.m_fishFactoryMarketBuilt, buildingID);
                }
            }
        }

        private void CheckCapacity(ushort buildingID, ref Building buildingData)
        {
            int outputBufferSize = GetOutputBufferSize(buildingID, ref buildingData);
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
                    buildingData.m_flags = (buildingData.m_flags & ~Building.Flags.CapacityFull) | Building.Flags.CapacityStep2;
                }
            }
            else if (customBuffer >= m_outputRate * 2)
            {
                if ((buildingData.m_flags & Building.Flags.CapacityFull) != Building.Flags.CapacityStep1)
                {
                    buildingData.m_flags = (buildingData.m_flags & ~Building.Flags.CapacityFull) | Building.Flags.CapacityStep1;
                }
            }
            else if ((buildingData.m_flags & Building.Flags.CapacityFull) != 0)
            {
                buildingData.m_flags &= ~Building.Flags.CapacityFull;
            }
        }

        public override void ModifyMaterialBuffer(ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int amountDelta)
        {
            switch (material)
            {
                case TransferManager.TransferReason.Shopping:
                case TransferManager.TransferReason.ShoppingB:
                case TransferManager.TransferReason.ShoppingC:
                case TransferManager.TransferReason.ShoppingD:
                case TransferManager.TransferReason.ShoppingE:
                case TransferManager.TransferReason.ShoppingF:
                case TransferManager.TransferReason.ShoppingG:
                case TransferManager.TransferReason.ShoppingH:
                    {
                        int customBuffer2 = data.m_customBuffer2;
                        amountDelta = Mathf.Clamp(amountDelta, -customBuffer2, 0);
                        data.m_customBuffer2 = (ushort)(customBuffer2 + amountDelta);
                        data.m_outgoingProblemTimer = 0;
                        byte park = Singleton<DistrictManager>.instance.GetPark(data.m_position);
			if (park != 0 && Singleton<DistrictManager>.instance.m_parks.m_buffer[park].IsPedestrianZone)
			{
				Singleton<DistrictManager>.instance.m_parks.m_buffer[park].m_tempGoodsSold -= (uint)amountDelta;
			}
			int cashCapacity = GetCashCapacity(buildingID, ref data);
			data.m_cashBuffer = Math.Min(data.m_cashBuffer - amountDelta, cashCapacity);
			return;
                    }
                case TransferManager.TransferReason.Cash:
		    {
			int cashBuffer = data.m_cashBuffer;
			amountDelta = Mathf.Clamp(amountDelta, -cashBuffer, 0);
			data.m_cashBuffer = cashBuffer + amountDelta;
			return;
		    }
                default:
                    if (material == m_inputResource4)
                    {
                        int inputBufferSize = GetInputBufferSize4(buildingID, ref data);
                        int customBuffer = data.m_customBuffer2;
                        amountDelta = Mathf.Clamp(amountDelta, -customBuffer, inputBufferSize - customBuffer);
                        customBuffer += amountDelta;
                        data.m_customBuffer2 = (ushort)customBuffer;
                    }
                    else if (material == m_inputResource5)
                    {
                        int inputBufferSize2 = GetInputBufferSize5(buildingID, ref data);
                        int num = (data.m_teens << 8) | data.m_youngs;
                        amountDelta = Mathf.Clamp(amountDelta, -num, inputBufferSize2 - num);
                        num += amountDelta;
                        data.m_youngs = (byte)((uint)num & 0xFFu);
                        data.m_teens = (byte)(num >> 8);
                    }
                    else if (material == m_inputResource6)
                    {
                        int inputBufferSize3 = GetInputBufferSize6(buildingID, ref data);
                        int num2 = (data.m_adults << 8) | data.m_seniors;
                        amountDelta = Mathf.Clamp(amountDelta, -num2, inputBufferSize3 - num2);
                        num2 += amountDelta;
                        data.m_seniors = (byte)((uint)num2 & 0xFFu);
                        data.m_adults = (byte)(num2 >> 8);
                    }
                    else if (material == m_inputResource7)
                    {
                        int inputBufferSize4 = GetInputBufferSize7(buildingID, ref data);
                        int num3 = (data.m_education1 << 8) | data.m_education2;
                        amountDelta = Mathf.Clamp(amountDelta, -num3, inputBufferSize4 - num3);
                        num3 += amountDelta;
                        data.m_education2 = (byte)((uint)num3 & 0xFFu);
                        data.m_education1 = (byte)(num3 >> 8);
                    }
                    else
                    {
                        base.ModifyMaterialBuffer(buildingID, ref data, material, ref amountDelta);
                    }
                    break;
            }
        }

        public  void ModifyExtendedMaterialBuffer(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, ref int amountDelta)
        {
            if (material == m_inputResource1)
            {
                int inputBufferSize = GetInputBufferSize4(buildingID, ref data);
                int customBuffer = data.m_customBuffer2;
                amountDelta = Mathf.Clamp(amountDelta, -customBuffer, inputBufferSize - customBuffer);
                customBuffer += amountDelta;
                data.m_customBuffer2 = (ushort)customBuffer;
            }
            else if (material == m_inputResource2)
            {
                int inputBufferSize2 = GetInputBufferSize5(buildingID, ref data);
                int num = (data.m_teens << 8) | data.m_youngs;
                amountDelta = Mathf.Clamp(amountDelta, -num, inputBufferSize2 - num);
                num += amountDelta;
                data.m_youngs = (byte)((uint)num & 0xFFu);
                data.m_teens = (byte)(num >> 8);
            }
            else if (material == m_inputResource3)
            {
                int inputBufferSize3 = GetInputBufferSize6(buildingID, ref data);
                int num2 = (data.m_adults << 8) | data.m_seniors;
                amountDelta = Mathf.Clamp(amountDelta, -num2, inputBufferSize3 - num2);
                num2 += amountDelta;
                data.m_seniors = (byte)((uint)num2 & 0xFFu);
                data.m_adults = (byte)(num2 >> 8);
            }
            else if (material == m_outputResource)
            {
                int inputBufferSize4 = GetInputBufferSize7(buildingID, ref data);
                int num3 = (data.m_education1 << 8) | data.m_education2;
                amountDelta = Mathf.Clamp(amountDelta, -num3, inputBufferSize4 - num3);
                num3 += amountDelta;
                data.m_education2 = (byte)((uint)num3 & 0xFFu);
                data.m_education1 = (byte)(num3 >> 8);
            }
        }

        public override void BuildingDeactivated(ushort buildingID, ref Building data)
        {
            TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
            ExtendedTransferManager.Offer extended_offer = default(ExtendedTransferManager.Offer);
            offer.Building = buildingID;
            if (m_inputResource1 != ExtendedTransferManager.TransferReason.None)
            {
                Singleton<ExtendedTransferManager>.instance.RemoveIncomingOffer(m_inputResource1, extended_offer);
            }
            if (m_inputResource2 != ExtendedTransferManager.TransferReason.None)
            {
                Singleton<ExtendedTransferManager>.instance.RemoveIncomingOffer(m_inputResource2, extended_offer);
            }
            if (m_inputResource3 != ExtendedTransferManager.TransferReason.None)
            {
                Singleton<ExtendedTransferManager>.instance.RemoveIncomingOffer(m_inputResource3, extended_offer);
            }
            if (m_inputResource4 != TransferManager.TransferReason.None)
            {
                Singleton<TransferManager>.instance.RemoveIncomingOffer(m_inputResource4, offer);
            }
            if (m_inputResource5 != TransferManager.TransferReason.None)
            {
                Singleton<TransferManager>.instance.RemoveIncomingOffer(m_inputResource5, offer);
            }
            if (m_inputResource6 != TransferManager.TransferReason.None)
            {
                Singleton<TransferManager>.instance.RemoveIncomingOffer(m_inputResource6, offer);
            }
            if (m_inputResource7 != TransferManager.TransferReason.None)
            {
                Singleton<TransferManager>.instance.RemoveIncomingOffer(m_inputResource7, offer);
            }
            if (m_outputResource != ExtendedTransferManager.TransferReason.None)
            {
                Singleton<ExtendedTransferManager>.instance.RemoveOutgoingOffer(m_outputResource, extended_offer);
            }
            Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.Entertainment, offer);
            Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.EntertainmentB, offer);
            Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.EntertainmentC, offer);
            Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.EntertainmentD, offer);
            Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.TouristA, offer);
            Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.TouristB, offer);
            Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.TouristC, offer);
            Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.TouristD, offer);
            Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.Shopping, offer);
            Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.ShoppingB, offer);
            Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.ShoppingC, offer);
            Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.ShoppingD, offer);
            Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.ShoppingE, offer);
            Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.ShoppingF, offer);
            Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.ShoppingG, offer);
            Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.ShoppingH, offer);
            base.BuildingDeactivated(buildingID, ref data);
        }

        protected override void HandleWorkAndVisitPlaces(ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveWorkerCount, ref int totalWorkerCount, ref int workPlaceCount, ref int aliveVisitorCount, ref int totalVisitorCount, ref int visitPlaceCount)
        {
            workPlaceCount += m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            GetWorkBehaviour(buildingID, ref buildingData, ref behaviour, ref aliveWorkerCount, ref totalWorkerCount);
            HandleWorkPlaces(buildingID, ref buildingData, m_workPlaceCount0, m_workPlaceCount1, m_workPlaceCount2, m_workPlaceCount3, ref behaviour, aliveWorkerCount, totalWorkerCount);
        }

        public override void VisitorEnter(ushort buildingID, ref Building data, uint citizen)
        {
            int amountDelta = -Singleton<SimulationManager>.instance.m_randomizer.Int32(50, 150);
            ModifyMaterialBuffer(buildingID, ref data, TransferManager.TransferReason.Shopping, ref amountDelta);
            base.VisitorEnter(buildingID, ref data, citizen);
        }

        protected override void ProduceGoods(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(buildingData.m_position);
            byte b = instance.GetPark(buildingData.m_position);
            float num = (float)buildingData.Width * -4f;
            float num2 = (float)buildingData.Width * 4f;
            float num3 = (float)buildingData.Length * -4f;
            float num4 = (float)buildingData.Length * 4f;
            if (m_info.m_subBuildings != null)
            {
                for (int i = 0; i < m_info.m_subBuildings.Length; i++)
                {
                    if ((object)m_info.m_subBuildings[i].m_buildingInfo != null)
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
            Notification.ProblemStruct problemStruct = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem1.NoResources | Notification.Problem1.NoPlaceforGoods | Notification.Problem1.NoInputProducts | Notification.Problem1.NoFishingGoods);
            bool flag = m_info.m_class.m_service == ItemClass.Service.Fishing;
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
                int num17 = 0;
                int num18 = 0;
                if (m_inputResource1 != ExtendedTransferManager.TransferReason.None)
                {
                    num17 = GetInputBufferSize1(parkPolicies, instance.m_parks.m_buffer[b].m_finalStorageDelta);
                    num18 = buildingData.m_customBuffer2;
                    int num19 = (m_inputRate1 * finalProductionRate + 99) / 100;
                    if (num18 < num19)
                    {
                        finalProductionRate = (num18 * 100 + m_inputRate1 - 1) / m_inputRate1;
                        problemStruct = Notification.AddProblems(problemStruct, flag ? Notification.Problem1.NoFishingGoods : Notification.Problem1.NoInputProducts);
                    }
                }
                int num20 = 0;
                int num21 = 0;
                if (m_inputResource2 != ExtendedTransferManager.TransferReason.None)
                {
                    num20 = GetInputBufferSize2(parkPolicies, instance.m_parks.m_buffer[b].m_finalStorageDelta);
                    num21 = (buildingData.m_teens << 8) | buildingData.m_youngs;
                    int num22 = (m_inputRate2 * finalProductionRate + 99) / 100;
                    if (num21 < num22)
                    {
                        finalProductionRate = (num21 * 100 + m_inputRate2 - 1) / m_inputRate2;
                        problemStruct = Notification.AddProblems(problemStruct, flag ? Notification.Problem1.NoFishingGoods : Notification.Problem1.NoInputProducts);
                    }
                }
                int num23 = 0;
                int num24 = 0;
                if (m_inputResource3 != ExtendedTransferManager.TransferReason.None)
                {
                    num23 = GetInputBufferSize3(parkPolicies, instance.m_parks.m_buffer[b].m_finalStorageDelta);
                    num24 = (buildingData.m_adults << 8) | buildingData.m_seniors;
                    int num25 = (m_inputRate3 * finalProductionRate + 99) / 100;
                    if (num24 < num25)
                    {
                        finalProductionRate = (num24 * 100 + m_inputRate3 - 1) / m_inputRate3;
                        problemStruct = Notification.AddProblems(problemStruct, flag ? Notification.Problem1.NoFishingGoods : Notification.Problem1.NoInputProducts);
                    }
                }
                int num26 = 0;
                int num27 = 0;
                if (m_inputResource4 != TransferManager.TransferReason.None)
                {
                    num26 = GetInputBufferSize4(parkPolicies, instance.m_parks.m_buffer[b].m_finalStorageDelta);
                    num27 = (buildingData.m_education1 << 8) | buildingData.m_education2;
                    int num28 = (m_inputRate4 * finalProductionRate + 99) / 100;
                    if (num27 < num28)
                    {
                        finalProductionRate = (num27 * 100 + m_inputRate4 - 1) / m_inputRate4;
                        problemStruct = Notification.AddProblems(problemStruct, flag ? Notification.Problem1.NoFishingGoods : ((!IsRawMaterial(m_inputResource4)) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources));
                    }
                }
                int num29 = 0;
                int num30 = 0;
                if (m_inputResource5 != TransferManager.TransferReason.None)
                {
                    num29 = GetInputBufferSize4(parkPolicies, instance.m_parks.m_buffer[b].m_finalStorageDelta);
                    num30 = (buildingData.m_education3 << 8) | buildingData.m_children;
                    int num31 = (m_inputRate5 * finalProductionRate + 99) / 100;
                    if (num30 < num31)
                    {
                        finalProductionRate = (num29 * 100 + m_inputRate5 - 1) / m_inputRate5;
                        problemStruct = Notification.AddProblems(problemStruct, flag ? Notification.Problem1.NoFishingGoods : ((!IsRawMaterial(m_inputResource5)) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources));
                    }
                }
                int num32 = 0;
                int num33 = 0;
                if (m_inputResource6 != TransferManager.TransferReason.None)
                {
                    num32 = GetInputBufferSize4(parkPolicies, instance.m_parks.m_buffer[b].m_finalStorageDelta);
                    num33 = (buildingData.m_childHealth << 8) | buildingData.m_seniorHealth;
                    int num34 = (m_inputRate6 * finalProductionRate + 99) / 100;
                    if (num33 < num34)
                    {
                        finalProductionRate = (num32 * 100 + m_inputRate6 - 1) / m_inputRate6;
                        problemStruct = Notification.AddProblems(problemStruct, flag ? Notification.Problem1.NoFishingGoods : ((!IsRawMaterial(m_inputResource6)) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources));
                    }
                }
                int num35 = 0;
                int num36 = 0;
                if (m_inputResource7 != TransferManager.TransferReason.None)
                {
                    num35 = GetInputBufferSize4(parkPolicies, instance.m_parks.m_buffer[b].m_finalStorageDelta);
                    num36 = (buildingData.m_health << 8) | buildingData.m_healthProblemTimer;
                    int num37 = (m_inputRate7 * finalProductionRate + 99) / 100;
                    if (num36 < num37)
                    {
                        finalProductionRate = (num35 * 100 + m_inputRate7 - 1) / m_inputRate7;
                        problemStruct = Notification.AddProblems(problemStruct, flag ? Notification.Problem1.NoFishingGoods : ((!IsRawMaterial(m_inputResource7)) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources));
                    }
                }
                int num38 = 0;
                int num39 = 0;
                if (m_outputResource != ExtendedTransferManager.TransferReason.None)
                {
                    num38 = GetOutputBufferSize(parkPolicies, instance.m_parks.m_buffer[b].m_finalStorageDelta);
                    num39 = buildingData.m_customBuffer1;
                    int num40 = (num13 * finalProductionRate + 99) / 100;
                    if (num38 - num39 < num40)
                    {
                        num40 = Mathf.Max(0, num29 - num30);
                        finalProductionRate = (num40 * 100 + num13 - 1) / num13;
                        if (m_DeliveryVehicleCount != 0)
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
                    int num41 = (m_inputRate1 * finalProductionRate + 99) / 100;
                    num18 = Mathf.Max(0, num18 - num41);
                    buildingData.m_customBuffer2 = (ushort)num18;
                }
                if (m_inputResource2 != ExtendedTransferManager.TransferReason.None)
                {
                    int num42 = (m_inputRate2 * finalProductionRate + 99) / 100;
                    num21 = Mathf.Max(0, num21 - num42);
                    buildingData.m_youngs = (byte)((uint)num21 & 0xFFu);
                    buildingData.m_teens = (byte)(num21 >> 8);
                }
                if (m_inputResource3 != ExtendedTransferManager.TransferReason.None)
                {
                    int num43 = (m_inputRate3 * finalProductionRate + 99) / 100;
                    num24 = Mathf.Max(0, num24 - num43);
                    buildingData.m_seniors = (byte)((uint)num24 & 0xFFu);
                    buildingData.m_adults = (byte)(num24 >> 8);
                }
                if (m_inputResource4 != TransferManager.TransferReason.None)
                {
                    int num44 = (m_inputRate4 * finalProductionRate + 99) / 100;
                    num27 = Mathf.Max(0, num27 - num44);
                    buildingData.m_education2 = (byte)((uint)num27 & 0xFFu);
                    buildingData.m_education1 = (byte)(num27 >> 8);
                    instance.m_parks.m_buffer[b].AddConsumptionAmount(m_inputResource4, num44);
                }
                if (m_inputResource5 != TransferManager.TransferReason.None)
                {
                    int num45 = (m_inputRate4 * finalProductionRate + 99) / 100;
                    num30 = Mathf.Max(0, num30 - num45);
                    buildingData.m_education2 = (byte)((uint)num27 & 0xFFu);
                    buildingData.m_education1 = (byte)(num27 >> 8);
                    instance.m_parks.m_buffer[b].AddConsumptionAmount(m_inputResource5, num45);
                }
                if (m_inputResource6 != TransferManager.TransferReason.None)
                {
                    int num46 = (m_inputRate4 * finalProductionRate + 99) / 100;
                    num33 = Mathf.Max(0, num33 - num46);
                    buildingData.m_education2 = (byte)((uint)num27 & 0xFFu);
                    buildingData.m_education1 = (byte)(num27 >> 8);
                    instance.m_parks.m_buffer[b].AddConsumptionAmount(m_inputResource6, num46);
                }
                if (m_inputResource7 != TransferManager.TransferReason.None)
                {
                    int num47 = (m_inputRate4 * finalProductionRate + 99) / 100;
                    num36 = Mathf.Max(0, num36 - num47);
                    buildingData.m_education2 = (byte)((uint)num27 & 0xFFu);
                    buildingData.m_education1 = (byte)(num27 >> 8);
                    instance.m_parks.m_buffer[b].AddConsumptionAmount(m_inputResource7, num47);
                }
                if (m_outputResource != ExtendedTransferManager.TransferReason.None)
                {
                    int num48 = (num13 * finalProductionRate + 99) / 100;
                    num39 = Mathf.Min(num38, num39 + num48);
                    buildingData.m_customBuffer1 = (ushort)num39;
                }
                base.HandleDead(buildingID, ref buildingData, ref behaviour, totalWorkerCount + totalVisitorCount);
                if (b != 0)
                {
                    int num49 = 0;
                    if (m_inputResource1 != ExtendedTransferManager.TransferReason.None)
                    {
                        int count = 0;
                        int cargo = 0;
                        int capacity = 0;
                        int outside = 0;
                        CalculateVehicles.CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource1, ref count, ref cargo, ref capacity, ref outside);
                        if (outside != 0)
                        {
                            num49 |= 1;
                        }
                        int num50 = num17 - num18 - cargo;
                        if (num50 >= 8000)
                        {
                            ExtendedTransferManager.Offer offer = default(ExtendedTransferManager.Offer);
                            offer.Building = buildingID;
                            offer.Position = buildingData.m_position;
                            offer.Amount = 1;
                            offer.Active = false;
                            Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(m_inputResource1, offer);
                        }
                    }
                    if (m_inputResource2 != ExtendedTransferManager.TransferReason.None)
                    {
                        int count2 = 0;
                        int cargo2 = 0;
                        int capacity2 = 0;
                        int outside2 = 0;
                        CalculateVehicles.CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource2, ref count2, ref cargo2, ref capacity2, ref outside2);
                        if (outside2 != 0)
                        {
                            num49 |= 2;
                        }
                        int num51 = num20 - num21 - cargo2;
                        if (num51 >= 8000)
                        {
                            ExtendedTransferManager.Offer offer2 = default(ExtendedTransferManager.Offer);
                            offer2.Building = buildingID;
                            offer2.Position = buildingData.m_position;
                            offer2.Amount = 1;
                            offer2.Active = false;
                            Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(m_inputResource2, offer2);
                        }
                    }
                    if (m_inputResource3 != ExtendedTransferManager.TransferReason.None)
                    {
                        int count3 = 0;
                        int cargo3 = 0;
                        int capacity3 = 0;
                        int outside3 = 0;
                        CalculateVehicles.CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource3, ref count3, ref cargo3, ref capacity3, ref outside3);
                        if (outside3 != 0)
                        {
                            num49 |= 4;
                        }
                        int num52 = num23 - num24 - cargo3;
                        if (num52 >= 8000)
                        {
                            ExtendedTransferManager.Offer offer3 = default(ExtendedTransferManager.Offer);
                            offer3.Building = buildingID;
                            offer3.Position = buildingData.m_position;
                            offer3.Amount = 1;
                            offer3.Active = false;
                            Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(m_inputResource3, offer3);
                        }
                    }
                    if (m_inputResource4 != TransferManager.TransferReason.None)
                    {
                        int count4 = 0;
                        int cargo4 = 0;
                        int capacity4 = 0;
                        int outside4 = 0;
                        CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource4, ref count4, ref cargo4, ref capacity4, ref outside4);
                        if (outside4 != 0)
                        {
                            num49 |= 8;
                        }
                        int num53 = num26 - num27 - cargo4;
                        if (num53 >= 8000)
                        {
                            TransferManager.TransferOffer offer4 = default(TransferManager.TransferOffer);
                            offer4.Priority = Mathf.Max(1, num53 * 8 / num26);
                            offer4.Building = buildingID;
                            offer4.Position = buildingData.m_position;
                            offer4.Amount = 1;
                            offer4.Active = false;
                            Singleton<TransferManager>.instance.AddIncomingOffer(m_inputResource4, offer4);
                        }
                        instance.m_parks.m_buffer[b].AddBufferStatus(m_inputResource4, num27, cargo4, num26);
                    }
                    if (m_inputResource5 != TransferManager.TransferReason.None)
                    {
                        int count5 = 0;
                        int cargo5 = 0;
                        int capacity5 = 0;
                        int outside5 = 0;
                        CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource5, ref count5, ref cargo5, ref capacity5, ref outside5);
                        if (outside5 != 0)
                        {
                            num49 |= 16;
                        }
                        int num54 = num29 - num30 - cargo5;
                        if (num54 >= 8000)
                        {
                            TransferManager.TransferOffer offer5 = default(TransferManager.TransferOffer);
                            offer5.Priority = Mathf.Max(1, num54 * 8 / num29);
                            offer5.Building = buildingID;
                            offer5.Position = buildingData.m_position;
                            offer5.Amount = 1;
                            offer5.Active = false;
                            Singleton<TransferManager>.instance.AddIncomingOffer(m_inputResource5, offer5);
                        }
                        instance.m_parks.m_buffer[b].AddBufferStatus(m_inputResource4, num30, cargo5, num29);
                    }
                    if (m_inputResource6 != TransferManager.TransferReason.None)
                    {
                        int count6 = 0;
                        int cargo6 = 0;
                        int capacity6 = 0;
                        int outside6 = 0;
                        CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource6, ref count6, ref cargo6, ref capacity6, ref outside6);
                        if (outside6 != 0)
                        {
                            num49 |= 32;
                        }
                        int num55 = num32 - num33 - cargo6;
                        if (num55 >= 8000)
                        {
                            TransferManager.TransferOffer offer6 = default(TransferManager.TransferOffer);
                            offer6.Priority = Mathf.Max(1, num55 * 8 / num32);
                            offer6.Building = buildingID;
                            offer6.Position = buildingData.m_position;
                            offer6.Amount = 1;
                            offer6.Active = false;
                            Singleton<TransferManager>.instance.AddIncomingOffer(m_inputResource6, offer6);
                        }
                        instance.m_parks.m_buffer[b].AddBufferStatus(m_inputResource4, num33, cargo6, num32);
                    }
                    if (m_inputResource7 != TransferManager.TransferReason.None)
                    {
                        int count7 = 0;
                        int cargo7 = 0;
                        int capacity7 = 0;
                        int outside7 = 0;
                        CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource7, ref count7, ref cargo7, ref capacity7, ref outside7);
                        if (outside7 != 0)
                        {
                            num49 |= 64;
                        }
                        int num56 = num35 - num36 - cargo7;
                        if (num56 >= 8000)
                        {
                            TransferManager.TransferOffer offer7 = default(TransferManager.TransferOffer);
                            offer7.Priority = Mathf.Max(1, num56 * 8 / num35);
                            offer7.Building = buildingID;
                            offer7.Position = buildingData.m_position;
                            offer7.Amount = 1;
                            offer7.Active = false;
                            Singleton<TransferManager>.instance.AddIncomingOffer(m_inputResource7, offer7);
                        }
                        instance.m_parks.m_buffer[b].AddBufferStatus(m_inputResource4, num36, cargo7, num35);
                    }
                    buildingData.m_tempImport |= (byte)num49;
                    if (m_outputResource != ExtendedTransferManager.TransferReason.None)
                    {
                        if (m_DeliveryVehicleCount != 0)
                        {
                            int count8 = 0;
                            int cargo8 = 0;
                            int capacity8 = 0;
                            int outside8 = 0;
                            CalculateVehicles.CalculateOwnVehicles(buildingID, ref buildingData, m_outputResource, ref count8, ref cargo8, ref capacity8, ref outside8);
                            buildingData.m_tempExport = (byte)Mathf.Clamp(outside8, buildingData.m_tempExport, 255);
                            int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
                            int productionRate2 = PlayerBuildingAI.GetProductionRate(100, budget);
                            int num58 = (productionRate2 * m_DeliveryVehicleCount + 99) / 100;
                            int num59 = num39;
                            if (num59 >= 8000 && count8 < num58)
                            {
                                ExtendedTransferManager.Offer offer8 = default(ExtendedTransferManager.Offer);
                                offer8.Building = buildingID;
                                offer8.Position = buildingData.m_position;
                                offer8.Amount = 1;
                                offer8.Active = true;
                                Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(m_outputResource, offer8);
                            }
                        }
                        var outgoingTransferReason = GetOutgoingTransferReason(buildingID);
                        if (outgoingTransferReason != TransferManager.TransferReason.None)
                        {
                            int num60 = buildingData.m_customBuffer1 - aliveVisitorCount * 100;
                            int num61 = Mathf.Max(0, visitPlaceCount - totalVisitorCount);
                            if (num60 >= 100 && num61 > 0)
                            {
                                TransferManager.TransferOffer offer6 = default(TransferManager.TransferOffer);
                                offer6.Priority = Mathf.Max(1, num60 * 8 / num29);
                                offer6.Building = buildingID;
                                offer6.Position = buildingData.m_position;
                                offer6.Amount = Mathf.Min(num60 / 100, num61);
                                offer6.Active = false;
                                Singleton<TransferManager>.instance.AddOutgoingOffer(outgoingTransferReason, offer6);
                            }
                        }
                    }
                }
                if (buildingData.m_finalImport != 0)
                {
                    instance.m_districts.m_buffer[district].m_playerConsumption.m_finalImportAmount += buildingData.m_finalImport;
                }
                if (buildingData.m_finalExport != 0)
                {
                    instance.m_districts.m_buffer[district].m_playerConsumption.m_finalExportAmount += buildingData.m_finalExport;
                }
                int num62 = finalProductionRate * m_noiseAccumulation / 100;
                if (num62 != 0)
                {
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, num62, position, m_noiseRadius);
                }
            }
            buildingData.m_problems = problemStruct;
            buildingData.m_education3 = (byte)Mathf.Clamp(finalProductionRate * num13 / Mathf.Max(1, m_outputRate), 0, 255);
            buildingData.m_health = (byte)Mathf.Clamp(finalProductionRate, 0, 255);
            if (b != 0)
            {
                instance.m_parks.m_buffer[b].AddWorkers(aliveWorkerCount);
            }
            base.ProduceGoods(buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
        }

        protected override bool CanEvacuate()
        {
            return m_workPlaceCount0 != 0 || m_workPlaceCount1 != 0 || m_workPlaceCount2 != 0 || m_workPlaceCount3 != 0;
        }

        public override void PlacementSucceeded()
        {
            Singleton<GuideManager>.instance.m_deathCareNeeded?.Deactivate();
        }

        public override string GetLocalizedTooltip()
        {
            string text = LocaleFormatter.FormatGeneric("AIINFO_WATER_CONSUMPTION", GetWaterConsumption() * 16) + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_ELECTRICITY_CONSUMPTION", GetElectricityConsumption() * 16);
            string text2 = LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_PRODUCTION_RATE", m_outputRate * 16);
            if (m_outputResource != ExtendedTransferManager.TransferReason.None && m_DeliveryVehicleCount != 0)
            {
                text2 = text2 + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLE_COUNT", m_DeliveryVehicleCount);
            }
            string baseTooltip = TooltipHelper.Append(base.GetLocalizedTooltip(), TooltipHelper.Format(LocaleFormatter.Info1, text, LocaleFormatter.Info2, text2));
            if (m_outputResource != ExtendedTransferManager.TransferReason.None)
            {
                string text3 = LocaleFormatter.FormatGeneric("AIINFO_WORKPLACES_ACCUMULATION", (m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3).ToString());
                baseTooltip = TooltipHelper.Append(baseTooltip, TooltipHelper.Format(LocaleFormatter.WorkplaceCount, text3));
            }
            bool flag = m_inputResource1 != ExtendedTransferManager.TransferReason.None;
            string text4 = ((m_inputResource1 == ExtendedTransferManager.TransferReason.None) ? string.Empty : "Drink Supplies");
            bool flag2 = m_inputResource2 != ExtendedTransferManager.TransferReason.None;
            string text5 = ((m_inputResource2 == ExtendedTransferManager.TransferReason.None) ? string.Empty : "Food Supplies");
            bool flag3 = m_inputResource3 != ExtendedTransferManager.TransferReason.None;
            string text6 = ((m_inputResource3 == ExtendedTransferManager.TransferReason.None) ? string.Empty : "Bread");
            bool flag4 = m_inputResource4 != TransferManager.TransferReason.None;
            string text7 = ((m_inputResource4 == TransferManager.TransferReason.None) ? string.Empty : IndustryWorldInfoPanel.ResourceSpriteName(m_inputResource4));
            bool flag5 = m_inputResource4 != TransferManager.TransferReason.None;
            string text8 = ((m_inputResource4 == TransferManager.TransferReason.None) ? string.Empty : IndustryWorldInfoPanel.ResourceSpriteName(m_inputResource5));
            bool flag6 = m_inputResource4 != TransferManager.TransferReason.None;
            string text9 = ((m_inputResource4 == TransferManager.TransferReason.None) ? string.Empty : IndustryWorldInfoPanel.ResourceSpriteName(m_inputResource6));
            bool flag7 = m_inputResource4 != TransferManager.TransferReason.None;
            string text10 = ((m_inputResource4 == TransferManager.TransferReason.None) ? string.Empty : IndustryWorldInfoPanel.ResourceSpriteName(m_inputResource7));
            string addTooltip = TooltipHelper.Format("arrowVisible", "true", "input1Visible", flag.ToString(), "input2Visible", flag2.ToString(), "input3Visible", flag3.ToString(), "input4Visible", flag4.ToString(), "outputVisible", "true");
            string addTooltip2 = TooltipHelper.Format("input1", text4, "input2", text5, "input3", text6, "input4", text7, "input5", text8, "input6", text9, "input7", text10, "output", "Meals");
            baseTooltip = TooltipHelper.Append(baseTooltip, addTooltip);
            return TooltipHelper.Append(baseTooltip, addTooltip2);
        }

        public override string GetLocalizedStats(ushort buildingID, ref Building data)
        {
            int num = data.m_education3 * m_outputRate * 16 / 100;
            string text = LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_PRODUCTION_RATE", num);
            if (m_outputResource != ExtendedTransferManager.TransferReason.None && m_DeliveryVehicleCount != 0)
            {
                int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
                int productionRate = GetProductionRate(100, budget);
                int num2 = (productionRate * m_DeliveryVehicleCount + 99) / 100;
                int count = 0;
                int cargo = 0;
                int capacity = 0;
                int outside = 0;
                CalculateVehicles.CalculateOwnVehicles(buildingID, ref data, m_outputResource, ref count, ref cargo, ref capacity, ref outside);
                text = text + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLES", count, num2);
            }
            text += Environment.NewLine;
            int finalExport = (int)data.m_finalExport;
            return text + LocaleFormatter.FormatGeneric("AIINFO_TOURISTS", new object[]
            {
               finalExport
            });
        }

        private void GetAccumulation(Randomizer r, int productionRate, int taxRate, DistrictPolicies.CityPlanning cityPlanningPolicies, DistrictPolicies.Taxation taxationPolicies, out int entertainment, out int attractiveness)
        {
            entertainment = 0;
            attractiveness = 0;
            if (m_info.m_class.isCommercialLeisure)
            {
                if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.NoLoudNoises) != 0)
                {
                    entertainment = 25;
                    attractiveness = 2;
                }
                else
                {
                    entertainment = 50;
                    attractiveness = 4;
                }
                if ((taxationPolicies & DistrictPolicies.Taxation.DontTaxLeisure) != 0)
                {
                    entertainment += 50;
                    attractiveness += 4;
                }
                else
                {
                    entertainment += 50 / (taxRate + 1);
                    attractiveness += 4 / ((taxRate >> 1) + 1);
                }
            }
            else if (m_info.m_class.isCommercialTourist)
            {
                attractiveness = 8;
            }
            if (entertainment != 0)
            {
                entertainment = (productionRate * entertainment + r.Int32(100u)) / 100;
            }
            if (attractiveness != 0)
            {
                attractiveness = (productionRate * attractiveness + r.Int32(100u)) / 100;
            }
            attractiveness = UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Tourism, attractiveness);
            entertainment = UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Tourism, entertainment);
        }

        public override bool RequireRoadAccess()
        {
            return true;
        }

        public int GetInputBufferSize1(ushort buildingID, ref Building data)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte b = instance.GetPark(data.m_position);
            return GetInputBufferSize1(instance.m_parks.m_buffer[b].m_parkPolicies, instance.m_parks.m_buffer[b].m_finalStorageDelta);
        }

        private int GetInputBufferSize1(DistrictPolicies.Park policies, int storageDelta)
        {
            int num = m_inputRate1 * 32 + 8000;
            if ((policies & DistrictPolicies.Park.ImprovedLogistics) != 0)
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
            return GetInputBufferSize2(instance.m_parks.m_buffer[b].m_parkPolicies, instance.m_parks.m_buffer[b].m_finalStorageDelta);
        }

        private int GetInputBufferSize2(DistrictPolicies.Park policies, int storageDelta)
        {
            int num = m_inputRate2 * 32 + 8000;
            if ((policies & DistrictPolicies.Park.ImprovedLogistics) != 0)
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
            return GetInputBufferSize3(instance.m_parks.m_buffer[b].m_parkPolicies, instance.m_parks.m_buffer[b].m_finalStorageDelta);
        }

        private int GetInputBufferSize3(DistrictPolicies.Park policies, int storageDelta)
        {
            int num = m_inputRate3 * 32 + 8000;
            if ((policies & DistrictPolicies.Park.ImprovedLogistics) != 0)
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
            return GetInputBufferSize4(instance.m_parks.m_buffer[b].m_parkPolicies, instance.m_parks.m_buffer[b].m_finalStorageDelta);
        }

        private int GetInputBufferSize4(DistrictPolicies.Park policies, int storageDelta)
        {
            int num = m_inputRate1 * 32 + 8000;
            if ((policies & DistrictPolicies.Park.ImprovedLogistics) != 0)
            {
                num = (num * 6 + 4) / 5;
            }
            num = (num * (100 + storageDelta) + 50) / 100;
            return Mathf.Clamp(num, 8000, 64000);
        }

        public int GetInputBufferSize5(ushort buildingID, ref Building data)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte b = instance.GetPark(data.m_position);
            return GetInputBufferSize5(instance.m_parks.m_buffer[b].m_parkPolicies, instance.m_parks.m_buffer[b].m_finalStorageDelta);
        }

        private int GetInputBufferSize5(DistrictPolicies.Park policies, int storageDelta)
        {
            int num = m_inputRate1 * 32 + 8000;
            if ((policies & DistrictPolicies.Park.ImprovedLogistics) != 0)
            {
                num = (num * 6 + 4) / 5;
            }
            num = (num * (100 + storageDelta) + 50) / 100;
            return Mathf.Clamp(num, 8000, 64000);
        }

        public int GetInputBufferSize6(ushort buildingID, ref Building data)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte b = instance.GetPark(data.m_position);
            return GetInputBufferSize6(instance.m_parks.m_buffer[b].m_parkPolicies, instance.m_parks.m_buffer[b].m_finalStorageDelta);
        }

        private int GetInputBufferSize6(DistrictPolicies.Park policies, int storageDelta)
        {
            int num = m_inputRate1 * 32 + 8000;
            if ((policies & DistrictPolicies.Park.ImprovedLogistics) != 0)
            {
                num = (num * 6 + 4) / 5;
            }
            num = (num * (100 + storageDelta) + 50) / 100;
            return Mathf.Clamp(num, 8000, 64000);
        }

        public int GetInputBufferSize7(ushort buildingID, ref Building data)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte b = instance.GetPark(data.m_position);
            return GetInputBufferSize7(instance.m_parks.m_buffer[b].m_parkPolicies, instance.m_parks.m_buffer[b].m_finalStorageDelta);
        }

        private int GetInputBufferSize7(DistrictPolicies.Park policies, int storageDelta)
        {
            int num = m_inputRate1 * 32 + 8000;
            if ((policies & DistrictPolicies.Park.ImprovedLogistics) != 0)
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
            return GetOutputBufferSize(instance.m_parks.m_buffer[b].m_parkPolicies, instance.m_parks.m_buffer[b].m_finalStorageDelta);
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
            if (m_DeliveryVehicleCount == 0)
            {
                int value = m_outputRate * 100;
                return Mathf.Clamp(value, 1, 64000);
            }
            int num = m_outputRate * 32 + 8000;
            if ((policies & DistrictPolicies.Park.ImprovedLogistics) != 0)
            {
                num = (num * 6 + 4) / 5;
            }
            num = (num * (100 + storageDelta) + 50) / 100;
            return Mathf.Clamp(num, 8000, 64000);
        }

        private TransferManager.TransferReason GetOutgoingTransferReason(ushort buildingID)
        {
            int num = 10;
            if (Singleton<SimulationManager>.instance.m_randomizer.Int32(100u) < num)
            {
                return Singleton<SimulationManager>.instance.m_randomizer.Int32(8u) switch
                {
                    0 => TransferManager.TransferReason.Entertainment,
                    1 => TransferManager.TransferReason.EntertainmentB,
                    2 => TransferManager.TransferReason.EntertainmentC,
                    3 => TransferManager.TransferReason.EntertainmentD,
                    4 => TransferManager.TransferReason.TouristA,
                    5 => TransferManager.TransferReason.TouristB,
                    6 => TransferManager.TransferReason.TouristC,
                    7 => TransferManager.TransferReason.TouristD,
                    _ => TransferManager.TransferReason.Entertainment,
                };
            }
            return Singleton<SimulationManager>.instance.m_randomizer.Int32(8u) switch
            {
                0 => TransferManager.TransferReason.Shopping,
                1 => TransferManager.TransferReason.ShoppingB,
                2 => TransferManager.TransferReason.ShoppingC,
                3 => TransferManager.TransferReason.ShoppingD,
                4 => TransferManager.TransferReason.ShoppingE,
                5 => TransferManager.TransferReason.ShoppingF,
                6 => TransferManager.TransferReason.ShoppingG,
                7 => TransferManager.TransferReason.ShoppingH,
                _ => TransferManager.TransferReason.Shopping,
            };
        }

        private int GetGoodsCapacity(ushort buildingID, ref Building data)
	{
		int num = 4000;
		int num2 = CalculateVisitplaceCount(new Randomizer(buildingID));
		return Mathf.Max(num2 * 500, num * 4);
	}

	private int GetCashCapacity(ushort buildingID, ref Building data)
	{
		return GetGoodsCapacity(buildingID, ref data) * 4;
	}

        public int CalculateVisitplaceCount(Randomizer r)
	{
	    ItemClass @class = m_info.m_class;
	    int num = 0;
	    if (quality == 1)
	    {
		num = 110;
	    }
            else if (quality == 2)
	    {
		num = 250;
	    }
	    else if (quality == 3)
	    {
		num = 400;
	    }
	    if (num != 0)
	    {
		num = Mathf.Max(200, num + r.Int32(100u)) / 100;
	    }
	    return num;
	}


    }

}
