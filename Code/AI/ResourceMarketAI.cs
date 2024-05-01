using ColossalFramework;
using System;
using IndustriesMeetsSunsetHarbor.Managers;
using UnityEngine;
using ColossalFramework.DataBinding;
using MoreTransferReasons;

namespace IndustriesMeetsSunsetHarbor.AI
{
    public class ResourceMarketAI : PlayerBuildingAI, IExtendedBuildingAI
    {
        [CustomizableProperty("Uneducated Workers", "Workers", 0)]
        public int m_workPlaceCount0 = 5;

        [CustomizableProperty("Educated Workers", "Workers", 1)]
        public int m_workPlaceCount1 = 12;

        [CustomizableProperty("Well Educated Workers", "Workers", 2)]
        public int m_workPlaceCount2 = 9;

        [CustomizableProperty("Highly Educated Workers", "Workers", 3)]
        public int m_workPlaceCount3 = 4;

        [CustomizableProperty("Visit place count", "Visitors", 3)]
        public int m_visitPlaceCount = 2;

        [CustomizableProperty("Entertainment Accumulation")]
        public int m_entertainmentAccumulation = 100;

        [CustomizableProperty("Entertainment Radius")]
        public float m_entertainmentRadius = 400f;

        [CustomizableProperty("Noise Accumulation", "Pollution")]
        public int m_noiseAccumulation = 50;

        [CustomizableProperty("Noise Radius", "Pollution")]
        public float m_noiseRadius = 100f;

        [CustomizableProperty("Healthcare Accumulation")]
        public int m_healthCareAccumulation = 200;

        [CustomizableProperty("Healthcare Radius")]
        public float m_healthCareRadius = 500f;

        [CustomizableProperty("Attractiveness Accumulation")]
        public int m_attractivenessAccumulation = 10;

        [CustomizableProperty("Input Resource Threshold")]
        public int m_resourceThreshold = 4000;

        public int m_goodsCapacity = 20000;

        public int m_productionCapacity = 200;

        public int m_goodsSellPrice = 1500;

        public Boolean isAmount = false;

        int index = 0;

        public TransferManager.TransferReason[] m_incomingResources =
        [
            TransferManager.TransferReason.Fish,
            TransferManager.TransferReason.Grain,
            TransferManager.TransferReason.Food,
            TransferManager.TransferReason.AnimalProducts,
            TransferManager.TransferReason.Flours
        ];

        public ExtendedTransferManager.TransferReason[] m_incomingExtendedResources =
        [
            ExtendedTransferManager.TransferReason.Bread,
            ExtendedTransferManager.TransferReason.FoodSupplies,
            ExtendedTransferManager.TransferReason.DrinkSupplies,
            ExtendedTransferManager.TransferReason.CannedFish
        ];

        public int GetEntertainmentAccumulation(ushort buildingID, ref Building data)
        {
            return UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Tourism, m_entertainmentAccumulation);
        }

        public int GetAttractivenessAccumulation(ushort buildingID, ref Building data)
        {
            return UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Tourism, m_attractivenessAccumulation);
        }

        public override ImmaterialResourceManager.ResourceData[] GetImmaterialResourceRadius(ushort buildingID, ref Building data)
        {
            return new ImmaterialResourceManager.ResourceData[2]
            {
                new ImmaterialResourceManager.ResourceData
                {
                    m_resource = ImmaterialResourceManager.Resource.Entertainment,
                    m_radius = ((GetEntertainmentAccumulation(buildingID, ref data) == 0) ? 0f : m_entertainmentRadius)
                },
                new ImmaterialResourceManager.ResourceData
                {
                    m_resource = ImmaterialResourceManager.Resource.NoisePollution,
                    m_radius = ((m_noiseAccumulation == 0) ? 0f : m_noiseRadius)
                }
            };
        }

        public override Color GetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode)
        {
            int attractivenessAccumulation = GetAttractivenessAccumulation(buildingID, ref data);
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
                case InfoManager.InfoMode.Tours:
                    return CommonBuildingAI.GetAttractivenessColor(attractivenessAccumulation * 15);
                case InfoManager.InfoMode.Entertainment:
                    if (subInfoMode == InfoManager.SubInfoMode.WaterPower)
                    {
                        if ((data.m_flags & Building.Flags.Active) != 0)
                        {
                            return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
                        }
                        return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_inactiveColor;
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                case InfoManager.InfoMode.Tourism:
                    switch (subInfoMode)
                    {
                        case InfoManager.SubInfoMode.Default:
                            if (data.m_tempExport != 0 || data.m_finalExport != 0)
                            {
                                return CommonBuildingAI.GetTourismColor(Mathf.Max(data.m_tempExport, data.m_finalExport));
                            }
                            return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                        case InfoManager.SubInfoMode.WaterPower:
                            if (attractivenessAccumulation != 0)
                            {
                                return CommonBuildingAI.GetAttractivenessColor(attractivenessAccumulation * 100);
                            }
                            return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                        default:
                            return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                    }
                case InfoManager.InfoMode.Connections:
                    if (ShowConsumption(buildingID, ref data))
                    {
                        if (subInfoMode == InfoManager.SubInfoMode.Default)
                        {
                            for (int i = 0; i < m_incomingResources.Length; i++)
                            {
                                if (m_incomingResources[i] != TransferManager.TransferReason.None && (data.m_tempImport != 0 || data.m_finalImport != 0))
                                {
                                    return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_incomingResources[i]];
                                }
                            }
                            for (int i = 0; i < m_incomingExtendedResources.Length; i++)
                            {
                                if (m_incomingExtendedResources[i] != ExtendedTransferManager.TransferReason.None && (data.m_tempImport != 0 || data.m_finalImport != 0))
                                {
                                    return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_incomingExtendedResources[i]];
                                }
                            }
                            return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                        }
                        return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                default:
                    return base.GetColor(buildingID, ref data, infoMode, subInfoMode);
            }
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
            base.GetPlacementInfoMode(out mode, out subMode, elevation);
        }

        public override string GetDebugString(ushort buildingID, ref Building data)
        {
            string text = base.GetDebugString(buildingID, ref data);
            TransferManager.TransferReason[] incomingResources = m_incomingResources;
            ExtendedTransferManager.TransferReason[] extendedIncomingResources = m_incomingExtendedResources;
            var marketBuffer = ResourceMarketManager.MarketBuffers[buildingID];
            for (int i = 0; i < incomingResources.Length; i++)
            {
                int num = 0;
                int num2 = 0;
                int num3 = 0;
                int num4 = 0;
                if (incomingResources[i] != TransferManager.TransferReason.None)
                {
                    base.CalculateGuestVehicles(buildingID, ref data, incomingResources[i], ref num, ref num2, ref num3, ref num4);
                    text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3})", new object[]
                    {
                        text,
                        incomingResources[i].ToString(),
                        marketBuffer.inputAmountBuffer[i],
                        num2
                    });
                }
            }
            for (int j = 0; j < extendedIncomingResources.Length; j++)
            {
                int num = 0;
                int num2 = 0;
                int num3 = 0;
                int num4 = 0;
                if (extendedIncomingResources[j] != ExtendedTransferManager.TransferReason.None)
                {
                    var material_byte = (byte)((byte)extendedIncomingResources[j] + 200);
                    ExtedndedVehicleManager.CalculateGuestVehicles(buildingID, ref data, (ExtendedTransferManager.TransferReason)material_byte, ref num, ref num2, ref num3, ref num4);
                    text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3})", new object[]
                    {
                        text,
                        extendedIncomingResources[j].ToString(),
                        marketBuffer.inputAmountBuffer[j + incomingResources.Length],
                        num2
                    });
                }
            }
            var merged_length = m_incomingResources.Length + m_incomingExtendedResources.Length;
            TransferManager.TransferReason transferReason = TransferManager.TransferReason.Shopping;
            for (int k = 0; k < merged_length; k++)
            {
                if (transferReason != TransferManager.TransferReason.None)
                {
                    text = StringUtils.SafeFormat("{0}\n{1}: {2}", new object[]
                    {
                        text,
                        transferReason.ToString(),
                        marketBuffer.outputAmountBuffer[k]
                    });
                }
            }

            return text;
        }

        public override void CreateBuilding(ushort buildingID, ref Building data)
	{
	    base.CreateBuilding(buildingID, ref data);
	    int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
	    int visitPlaceCount = m_visitPlaceCount;
	    Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, 0, workCount, visitPlaceCount);
	    if (m_info != null && m_info.m_class != null && m_info.m_class.m_service == ItemClass.Service.Fishing)
	    {
		GuideController properties = Singleton<GuideManager>.instance.m_properties;
		if (properties != null && Singleton<BuildingManager>.instance.m_intercityBusStationBuilt != null)
		{
		    Singleton<BuildingManager>.instance.m_intercityBusStationBuilt.Activate(properties.m_fishFactoryMarketBuilt, buildingID);
		}
	    }
	}

        public override void BuildingLoaded(ushort buildingID, ref Building data, uint version)
	{
	    base.BuildingLoaded(buildingID, ref data, version);
	    EnsureCitizenUnits(buildingID, ref data);
	}

        public override void EndRelocating(ushort buildingID, ref Building data)
	{
	    base.EndRelocating(buildingID, ref data);
	    EnsureCitizenUnits(buildingID, ref data);
	}

        private void EnsureCitizenUnits(ushort buildingID, ref Building data)
	{
	    int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
	    int visitPlaceCount = m_visitPlaceCount;
	    EnsureCitizenUnits(buildingID, ref data, 0, workCount, visitPlaceCount);
	}

	protected override void ManualActivation(ushort buildingID, ref Building buildingData)
	{
	    if (m_healthCareAccumulation != 0)
	    {
		Vector3 position = buildingData.m_position;
		position.y += m_info.m_size.y;
		Singleton<NotificationManager>.instance.AddEvent(NotificationEvent.Type.GainHealth, position, 1.5f);
	    }
	    if (m_noiseAccumulation != 0)
	    {
		Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Happy, ImmaterialResourceManager.Resource.Entertainment, m_entertainmentAccumulation, m_entertainmentRadius, buildingData.m_position, NotificationEvent.Type.Sad, ImmaterialResourceManager.Resource.NoisePollution, m_noiseAccumulation, m_noiseRadius);
	    }
    }

	protected override void ManualDeactivation(ushort buildingID, ref Building buildingData)
	{
	    if ((buildingData.m_flags & Building.Flags.Collapsed) != 0)
	    {
		Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Happy, ImmaterialResourceManager.Resource.Abandonment, -buildingData.Width * buildingData.Length, 64f);
		return;
	    }
	    if (m_healthCareAccumulation != 0)
	    {
		Vector3 position = buildingData.m_position;
		position.y += m_info.m_size.y;
		Singleton<NotificationManager>.instance.AddEvent(NotificationEvent.Type.LoseHealth, position, 1.5f);
	    }
	    if (m_noiseAccumulation != 0)
	    {
		Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Sad, ImmaterialResourceManager.Resource.Entertainment, -m_entertainmentAccumulation, m_entertainmentRadius, buildingData.m_position, NotificationEvent.Type.Happy, ImmaterialResourceManager.Resource.NoisePollution, -m_noiseAccumulation, m_noiseRadius);
	    }
	}

        public override void ModifyMaterialBuffer(ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int amountDelta)
        {
            var merged_length = m_incomingResources.Length + m_incomingExtendedResources.Length;
            System.Random rnd = new();
            index = rnd.Next(0, merged_length);
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
                    int outputAmountBuffer = ResourceMarketManager.MarketBuffers[buildingID].outputAmountBuffer[index];
                    amountDelta = Mathf.Clamp(amountDelta, -outputAmountBuffer, 0);
                    ResourceMarketManager.MarketBuffers[buildingID].outputAmountBuffer[index] = (ushort)(outputAmountBuffer + amountDelta);
                    ResourceMarketManager.MarketBuffers[buildingID].amountSold1[index] = (byte)Mathf.Clamp((int)ResourceMarketManager.MarketBuffers[buildingID].amountSold1[index] + (-amountDelta + 99) / 100, 0, 255);
                    data.m_outgoingProblemTimer = 0;
                    int num = (-amountDelta * m_goodsSellPrice + 50) / 100;
                    if (num != 0)
                    {
                        Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.ResourcePrice, num, m_info.m_class);
                    }
                    break;
                }
                default:
                    if (material != TransferManager.TransferReason.Shopping)
                    {
                        var found = false;
                        for (int i = 0; i < m_incomingResources.Length; i++)
                        {
                            if (material == m_incomingResources[i])
                            {
                                index = i;
                                if (!ResourceMarketManager.MarketBuffers.ContainsKey(buildingID))
                                {
                                    AddMarketBufferToBuildingData(buildingID);
                                }
                                var marketBuffer = ResourceMarketManager.MarketBuffers[buildingID];
                                int goodsCapacity = m_goodsCapacity;
                                amountDelta = Mathf.Clamp(amountDelta, 0, goodsCapacity - (int)marketBuffer.inputAmountBuffer[i]);
                                marketBuffer.inputAmountBuffer[i] = (ushort)((int)marketBuffer.inputAmountBuffer[i] + amountDelta);
                                ResourceMarketManager.MarketBuffers[buildingID] = marketBuffer;
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            base.ModifyMaterialBuffer(buildingID, ref data, material, ref amountDelta);
                        }
                    }
                    break;
            }
        }

        void IExtendedBuildingAI.ExtendedModifyMaterialBuffer(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, ref int amountDelta)
        {
            for (int i = 0; i < m_incomingExtendedResources.Length; i++)
            {
                if (material == m_incomingExtendedResources[i])
                {
                    index = i;
                    if (!ResourceMarketManager.MarketBuffers.ContainsKey(buildingID))
                    {
                        AddMarketBufferToBuildingData(buildingID);
                    }
                    var marketBuffer = ResourceMarketManager.MarketBuffers[buildingID];
                    int goodsCapacity = m_goodsCapacity;
                    amountDelta = Mathf.Clamp(amountDelta, 0, goodsCapacity - (int)marketBuffer.inputAmountBuffer[i + m_incomingResources.Length]);
                    marketBuffer.inputAmountBuffer[i + m_incomingResources.Length] = (ushort)((int)marketBuffer.inputAmountBuffer[i + m_incomingResources.Length] + amountDelta);
                    ResourceMarketManager.MarketBuffers[buildingID] = marketBuffer;
                }
            }
        }

        public override void GetMaterialAmount(ushort buildingID, ref Building data, TransferManager.TransferReason material, out int amount, out int max)
        {
            amount = 0;
            max = m_goodsCapacity;
            var found = false;
            for (int i = 0; i < m_incomingResources.Length; i++)
            {
                if (material == m_incomingResources[i])
                {
                    amount = ResourceMarketManager.MarketBuffers[buildingID].inputAmountBuffer[i];
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                base.GetMaterialAmount(buildingID, ref data, material, out amount, out max);
            }
        }

        void IExtendedBuildingAI.ExtendedGetMaterialAmount(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, out int amount, out int max)
        {
            amount = 0;
            max = m_goodsCapacity;
            for (int i = 0; i < m_incomingExtendedResources.Length; i++)
            {
                if (material == m_incomingExtendedResources[i])
                {
                    amount = ResourceMarketManager.MarketBuffers[buildingID].inputAmountBuffer[i + m_incomingResources.Length];
                    break;
                }
            }
        }

        public override void VisitorEnter(ushort buildingID, ref Building data, uint citizen)
	{
	    int amountDelta = -Singleton<SimulationManager>.instance.m_randomizer.Int32(50, 150);
	    ModifyMaterialBuffer(buildingID, ref data, TransferManager.TransferReason.Shopping, ref amountDelta);
	    base.VisitorEnter(buildingID, ref data, citizen);
	}

        public override void BuildingDeactivated(ushort buildingID, ref Building data)
        {
            TransferManager.TransferOffer offer = default;
            offer.Building = buildingID;
            for (int i = 0; i < m_incomingResources.Length; i++)
            {
                if (m_incomingResources[i] != TransferManager.TransferReason.None)
                {
                    Singleton<TransferManager>.instance.RemoveIncomingOffer(m_incomingResources[i], offer);
                }
            }
            ExtendedTransferManager.Offer extended_offer = default;
            extended_offer.Building = buildingID;
            for (int i = 0; i < m_incomingExtendedResources.Length; i++)
            {
                if (m_incomingExtendedResources[i] != ExtendedTransferManager.TransferReason.None)
                {
                    Singleton<ExtendedTransferManager>.instance.RemoveIncomingOffer(m_incomingExtendedResources[i], extended_offer);
                }
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

        public override void SimulationStep(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            base.SimulationStep(buildingID, ref buildingData, ref frameData);
            if (!ResourceMarketManager.MarketBuffers.ContainsKey(buildingID))
            {
                AddMarketBufferToBuildingData(buildingID);
            }
            var marketBuffer = ResourceMarketManager.MarketBuffers[buildingID];
            SimulationManager instance = Singleton<SimulationManager>.instance;
            uint num = (instance.m_currentFrameIndex & 3840U) >> 8;
            if (num == 15U)
            {
                buildingData.m_finalImport = buildingData.m_tempImport;
                buildingData.m_finalExport = buildingData.m_tempExport;
                marketBuffer.amountSold2[index] = marketBuffer.amountSold1[index];
                buildingData.m_tempImport = 0;
                buildingData.m_tempExport = 0;
                marketBuffer.amountSold1[index] = 0;
            }
            if (m_info != null && m_info.m_class != null && m_info.m_class.m_service == ItemClass.Service.Fishing)
            {
                GuideController properties = Singleton<GuideManager>.instance.m_properties;
                if (properties != null && Singleton<BuildingManager>.instance.m_fishFactoryMarketBuilt != null)
                {
                    Singleton<BuildingManager>.instance.m_fishFactoryMarketBuilt.Activate(properties.m_fishFactoryMarketBuilt, buildingID);
                }
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
	    visitPlaceCount += m_visitPlaceCount;
	    GetVisitBehaviour(buildingID, ref buildingData, ref behaviour, ref aliveVisitorCount, ref totalVisitorCount);
	}

        protected override void ProduceGoods(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            base.ProduceGoods(buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
            float num = (float)buildingData.Width * -4f;
            float num2 = (float)buildingData.Width * 4f;
            float num3 = (float)buildingData.Length * -4f;
            float num4 = (float)buildingData.Length * 4f;
            if (!ResourceMarketManager.MarketBuffers.ContainsKey(buildingID))
            {
                AddMarketBufferToBuildingData(buildingID);
            }
            var marketBuffer = ResourceMarketManager.MarketBuffers[buildingID];
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
            float num8 = num4 - num3;
            float angle = buildingData.m_angle;
            float num9 = -(num + num2) * 0.5f;
            float num10 = -(num3 + num4) * 0.5f;
            float num11 = num8 * 0.25f - (num3 + num4) * 0.5f;
            float num12 = Mathf.Sin(angle);
            float num13 = Mathf.Cos(angle);
            Vector3 position = buildingData.m_position - new Vector3(num13 * num9 + num12 * num10, 0f, num12 * num9 - num13 * num10);
            Vector3 position2 = buildingData.m_position - new Vector3(num13 * num9 + num12 * num11, 0f, num12 * num9 - num13 * num11);
            float currentRange = GetCurrentRange(buildingID, ref buildingData);
            Notification.Problem1 problem = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem1.NoCustomers | Notification.Problem1.NoGoods | Notification.Problem1.NoFishingGoods);
            int num14 = productionRate * m_healthCareAccumulation / 100;
            if (num14 != 0)
            {
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.HealthCare, num14, buildingData.m_position, m_healthCareRadius);
            }
            if (finalProductionRate != 0)
            {
                if (m_entertainmentAccumulation != 0)
                {
                    int rate = productionRate * GetEntertainmentAccumulation(buildingID, ref buildingData) / 100;
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Entertainment, rate, position, currentRange);
                }
                if (m_attractivenessAccumulation != 0)
                {
                    int num15 = GetAttractivenessAccumulation(buildingID, ref buildingData);
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Attractiveness, num15);
                    num15 = productionRate * num15 * 15 / 100;
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Attractiveness, num15, position2, num8);
                }
                if (m_noiseAccumulation != 0)
                {
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, m_noiseAccumulation, buildingData.m_position, m_noiseRadius);
                }
                base.HandleDead(buildingID, ref buildingData, ref behaviour, totalWorkerCount + totalVisitorCount);
                int goodsCapacity = m_goodsCapacity;
                TransferManager.TransferReason outgoingTransferReason = GetOutgoingTransferReason(buildingID);
                var merged_count = m_incomingResources.Length + m_incomingExtendedResources.Length;
                int[] productionRateArr = new int[merged_count];
                if (productionRate != 0)
                {
                    for (int i = 0; i < merged_count; i++)
                    {
                        productionRateArr[i] = productionRate;
                        int num16 = goodsCapacity;
                        if (i < m_incomingResources.Length && m_incomingResources[i] != TransferManager.TransferReason.None)
                        {
                            num16 = Mathf.Min(num16, (int)marketBuffer.inputAmountBuffer[i]);
                        }
                        if (i >= m_incomingResources.Length && m_incomingExtendedResources[i - m_incomingResources.Length] != ExtendedTransferManager.TransferReason.None)
                        {
                            num16 = Mathf.Min(num16, (int)marketBuffer.inputAmountBuffer[i]);
                        }
                        if (outgoingTransferReason != TransferManager.TransferReason.None)
                        {
                            num16 = Mathf.Min(num16, goodsCapacity - (int)marketBuffer.outputAmountBuffer[i]);
                        }
                        productionRateArr[i] = Mathf.Max(0, Mathf.Min(productionRateArr[i], (num16 * 200 + goodsCapacity - 1) / goodsCapacity));
                        int num17 = (visitPlaceCount * productionRateArr[i] + 9) / 10;
                        if (Singleton<SimulationManager>.instance.m_isNightTime)
                        {
                            num17 = num17 + 1 >> 1;
                        }
                        num17 = Mathf.Max(0, Mathf.Min(num17, num16));
                        if (i < m_incomingResources.Length && m_incomingResources[i] != TransferManager.TransferReason.None)
                        {
                            marketBuffer.inputAmountBuffer[i] -= (ushort)num17;
                        }
                        if (i >= m_incomingResources.Length && m_incomingExtendedResources[i - m_incomingResources.Length] != ExtendedTransferManager.TransferReason.None)
                        {
                            marketBuffer.inputAmountBuffer[i] -= (ushort)num17;
                        }
                        if (outgoingTransferReason != TransferManager.TransferReason.None)
                        {
                            marketBuffer.outputAmountBuffer[i] += (ushort)num17;
                        }
                        productionRateArr[i] += (num17 + 9) / 10;
                    }
                }
                int[] countArr = new int[m_incomingResources.Length];
                int[] cargoArr = new int[m_incomingResources.Length];
                int[] capacityArr = new int[m_incomingResources.Length];
                int[] outsideArr = new int[m_incomingResources.Length];
                int[] extendedCountArr = new int[m_incomingExtendedResources.Length];
                int[] extendedCargoArr = new int[m_incomingExtendedResources.Length];
                int[] extendedCapacityArr = new int[m_incomingExtendedResources.Length];
                int[] extendedOutsideArr = new int[m_incomingExtendedResources.Length];
                for (int i = 0; i < merged_count; i++)
                {
                    if (marketBuffer.inputAmountBuffer[i] > 0)
                    {
                        isAmount = true;
                        break;
                    }
                    if (i == merged_count - 1 && isAmount)
                    {
                        isAmount = false;
                    }
                }
                for (int i = 0; i < m_incomingResources.Length; i++)
                {
                    if (m_incomingResources[i] != TransferManager.TransferReason.None)
                    {
                        base.CalculateGuestVehicles(buildingID, ref buildingData, m_incomingResources[i], ref countArr[i], ref cargoArr[i], ref capacityArr[i], ref outsideArr[i]);
                        buildingData.m_tempImport = (byte)Mathf.Clamp(outsideArr[i], (int)buildingData.m_tempImport, 255);
                    }
                }
                for (int i = 0; i < m_incomingExtendedResources.Length; i++)
                {
                    if (m_incomingExtendedResources[i] != ExtendedTransferManager.TransferReason.None)
                    {
                        var material_byte = (byte)((byte)m_incomingExtendedResources[i] + 200);
                        ExtedndedVehicleManager.CalculateGuestVehicles(buildingID, ref buildingData, (ExtendedTransferManager.TransferReason)material_byte, ref extendedCountArr[i], ref extendedCargoArr[i], ref extendedCapacityArr[i], ref extendedOutsideArr[i]);
                        buildingData.m_tempImport = (byte)Mathf.Clamp(extendedOutsideArr[i], (int)buildingData.m_tempImport, 255);
                    }
                }
                buildingData.m_tempExport = (byte)Mathf.Clamp(behaviour.m_touristCount, (int)buildingData.m_tempExport, 255);
                buildingData.m_adults = (byte)productionRateArr[0];
                int num21 = visitPlaceCount * 500;
                for (int k = 0; k < m_incomingResources.Length; k++)
                {
                    if ((int)marketBuffer.outputAmountBuffer[k] > goodsCapacity - (num21 >> 1) && aliveVisitorCount <= visitPlaceCount >> 1)
                    {
                        buildingData.m_outgoingProblemTimer = (byte)Mathf.Min(255, (int)(buildingData.m_outgoingProblemTimer + 1));
                        if (buildingData.m_outgoingProblemTimer >= 192)
                        {
                            problem = Notification.AddProblems(problem, Notification.Problem1.NoCustomers | Notification.Problem1.MajorProblem);
                        }
                        else if (buildingData.m_outgoingProblemTimer >= 128)
                        {
                            problem = Notification.AddProblems(problem, Notification.Problem1.NoCustomers);
                        }
                    }
                    else
                    {
                        buildingData.m_outgoingProblemTimer = 0;
                    }
                }
                for (int i = 0; i < merged_count; i++)
                {
                    Notification.Problem1 problem2 = Notification.Problem1.NoGoods;
                    if (i < m_incomingResources.Length && m_incomingResources[i] == TransferManager.TransferReason.Fish)
                    {
                        problem2 = Notification.Problem1.NoFishingGoods;
                    }
                    else if (i >= m_incomingResources.Length && m_incomingExtendedResources[i - m_incomingResources.Length] == ExtendedTransferManager.TransferReason.CannedFish)
                    {
                        problem2 = Notification.Problem1.NoFishingGoods;
                    }
                    if (marketBuffer.inputAmountBuffer[i] < m_resourceThreshold && !isAmount)
                    {
                        buildingData.m_incomingProblemTimer = (byte)Mathf.Min(255, (int)(buildingData.m_incomingProblemTimer + 1));
                        if (buildingData.m_incomingProblemTimer < 64)
                        {
                            problem = Notification.AddProblems(problem, problem2);
                        }
                        else
                        {
                            problem = Notification.AddProblems(problem, Notification.Problem1.MajorProblem | problem2);
                        }
                    }
                    else
                    {
                        buildingData.m_incomingProblemTimer = 0;
                    }

                }
                for (int i = 0; i < m_incomingResources.Length; i++)
                {
                    if (buildingData.m_fireIntensity == 0 && m_incomingResources[i] != TransferManager.TransferReason.None)
                    {
                        int InputSize = (int)marketBuffer.inputAmountBuffer[i] + cargoArr[i];
                        if (InputSize < m_resourceThreshold)
                        {
                            TransferManager.TransferOffer offer = default;
                            offer.Priority = Mathf.Max(1, InputSize * 8 / m_goodsCapacity);
                            offer.Building = buildingID;
                            offer.Position = buildingData.m_position;
                            offer.Amount = 1;
                            offer.Active = false;
                            Singleton<TransferManager>.instance.AddIncomingOffer(m_incomingResources[i], offer);
                        }
                    }
                }
                for (int i = 0; i < m_incomingExtendedResources.Length; i++)
                {
                    if (buildingData.m_fireIntensity == 0 && m_incomingExtendedResources[i] != ExtendedTransferManager.TransferReason.None)
                    {
                        int ExtendedInputSize = marketBuffer.inputAmountBuffer[i + m_incomingResources.Length] + extendedCargoArr[i];
                        if (ExtendedInputSize < m_resourceThreshold)
                        {
                            ExtendedTransferManager.Offer offer = default;
                            offer.Building = buildingID;
                            offer.Position = buildingData.m_position;
                            offer.Amount = 1;
                            offer.Active = false;
                            Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(m_incomingExtendedResources[i], offer);
                        }
                    }
                }
                if (buildingData.m_fireIntensity == 0 && outgoingTransferReason != TransferManager.TransferReason.None)
                {
                    int all_resources_amount = 0;
                    for (int k = 0; k < merged_count; k++)
                    {
                        all_resources_amount += (int)marketBuffer.outputAmountBuffer[k];
                    }
                    int num24 = all_resources_amount - aliveVisitorCount * 100;
                    int num25 = Mathf.Max(0, visitPlaceCount - totalVisitorCount);
                    if (num24 >= 100 && num25 > 0)
                    {
                        TransferManager.TransferOffer offer2 = default;
                        offer2.Priority = Mathf.Max(1, num24 * 8 / goodsCapacity);
                        offer2.Building = buildingID;
                        offer2.Position = buildingData.m_position;
                        offer2.Amount = Mathf.Min(num24 / 100, num25);
                        offer2.Active = false;
                        Singleton<TransferManager>.instance.AddOutgoingOffer(outgoingTransferReason, offer2);
                    }
                }
                ResourceMarketManager.MarketBuffers[buildingID] = marketBuffer;
            }
            buildingData.m_problems = problem;
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

        public override string GetLocalizedTooltip()
	{
	    string text = LocaleFormatter.FormatGeneric("AIINFO_WATER_CONSUMPTION", GetWaterConsumption() * 16) + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_ELECTRICITY_CONSUMPTION", GetElectricityConsumption() * 16);
	    string text2 = LocaleFormatter.FormatGeneric("AIINFO_WORKPLACES_ACCUMULATION", (m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3).ToString());
	    return TooltipHelper.Append(base.GetLocalizedTooltip(), TooltipHelper.Format(LocaleFormatter.Info1, text, LocaleFormatter.Info2, string.Empty, LocaleFormatter.WorkplaceCount, text2));
	}

        public override string GetLocalizedStats(ushort buildingID, ref Building data)
        {
            if (!ResourceMarketManager.MarketBuffers.ContainsKey(buildingID))
            {
                AddMarketBufferToBuildingData(buildingID);
            }
            var marketBuffer = ResourceMarketManager.MarketBuffers[buildingID];
            string str = "";
            int num;
            for (int i = 0; i < m_incomingResources.Length; i++)
            {
                string name = m_incomingResources[i].ToString();
                name = name.Replace("Grain", "Crops");
                name = name.Replace("Flours", "Flour");
                name = name.Replace("AnimalProducts", "Meat");
                num = (int)(marketBuffer.amountSold2[i] * 10);
                str += name + " Sold Last Week: " + num;
                str += Environment.NewLine;
            }
            for (int i = 0; i < m_incomingExtendedResources.Length; i++)
            {
                string name = m_incomingExtendedResources[i].ToString();
                num = (int)(marketBuffer.amountSold2[i + m_incomingResources.Length] * 10);
                str += name + " Sold Last Week: " + num;
                str += Environment.NewLine;
            }
            str += Environment.NewLine;
            for (int i = 0; i < m_incomingResources.Length; i++)
            {
                string name = m_incomingResources[i].ToString();
                name = name.Replace("Grain", "Crops");
                name = name.Replace("Flours", "Flour");
                name = name.Replace("AnimalProducts", "Meat");
                num = (int)(marketBuffer.inputAmountBuffer[i]);
                str += name + " Stored In Market: " + num + "/" + m_goodsCapacity;
                str += Environment.NewLine;
            }
            for (int i = 0; i < m_incomingExtendedResources.Length; i++)
            {
                string name = m_incomingExtendedResources[i].ToString();
                num = (int)(marketBuffer.inputAmountBuffer[i + m_incomingResources.Length]);
                str += name + " Stored In Market: " + num + "/" + m_goodsCapacity;
                str += Environment.NewLine;
            }
            str += Environment.NewLine;
            ResourceMarketManager.MarketBuffers[buildingID] = marketBuffer;
            int finalExport = (int)data.m_finalExport;
            return str + LocaleFormatter.FormatGeneric("AIINFO_TOURISTS", new object[]
            {
               finalExport
            });
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
        void IExtendedBuildingAI.ExtendedStartTransfer(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, ExtendedTransferManager.Offer offer)
        {

        }

        private void AddMarketBufferToBuildingData(ushort buildingID)
        {
            var merged_length = m_incomingResources.Length + m_incomingExtendedResources.Length;
            ResourceMarketManager.MarketData newMarketData = new()
            {
                inputAmountBuffer = new ushort[merged_length],
                outputAmountBuffer = new ushort[merged_length],
                amountSold1 = new ushort[merged_length],
                amountSold2 = new ushort[merged_length]
            };
            for (int j = 0; j < merged_length; j++)
            {
                newMarketData.inputAmountBuffer[j] = 0;
                newMarketData.outputAmountBuffer[j] = 0;
                newMarketData.amountSold1[j] = 0;
                newMarketData.amountSold2[j] = 0;
            }
            ResourceMarketManager.MarketBuffers.Add(buildingID, newMarketData);
        }

    }
}
