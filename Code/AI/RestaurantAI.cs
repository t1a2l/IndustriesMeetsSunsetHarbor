using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.Math;
using System;
using UnityEngine;
using MoreTransferReasons;
using IndustriesMeetsSunsetHarbor.Managers;
using ICities;
using System.Linq;

namespace IndustriesMeetsSunsetHarbor.AI
{
    public class RestaurantAI : PlayerBuildingAI, IExtendedBuildingAI
    {
        [CustomizableProperty("Uneducated Workers", "Workers", 0)]
        public int m_workPlaceCount0 = 3;

        [CustomizableProperty("Educated Workers", "Workers", 1)]
        public int m_workPlaceCount1 = 2;

        [CustomizableProperty("Well Educated Workers", "Workers", 2)]
        public int m_workPlaceCount2 = 3;

        [CustomizableProperty("Highly Educated Workers", "Workers", 3)]
        public int m_workPlaceCount3 = 2;

        [CustomizableProperty("Low Wealth", "Visitors", 0)]
        public int m_visitPlaceCount0 = 10;

        [CustomizableProperty("Medium Wealth", "Visitors", 1)]
        public int m_visitPlaceCount1 = 10;

        [CustomizableProperty("High Wealth", "Visitors", 2)]
        public int m_visitPlaceCount2 = 10;

        [CustomizableProperty("Delivery Vehicle Count")]
        public int m_DeliveryVehicleCount = 4;

        [CustomizableProperty("Food Delivery Accumulation")]
        public int m_foodDeliveryAccumulation = 100;

        [CustomizableProperty("Noise Accumulation")]
        public int m_noiseAccumulation;

        [CustomizableProperty("Food Delivery Radius")]
        public float m_foodDeliveryRadius = 400f;

        [CustomizableProperty("Noise Radius")]
        public float m_noiseRadius = 200f;

        [CustomizableProperty("Input Resource Capacity 1")]
        public int m_inputCapacity1 = 100;

        [CustomizableProperty("Input Resource Capacity 2")]
        public int m_inputCapacity2 = 100;

        [CustomizableProperty("Input Resource Capacity 3")]
        public int m_inputCapacity3 = 50;

        [CustomizableProperty("Input Resource Capacity 4")]
        public int m_inputCapacity4 = 50;

        [CustomizableProperty("Input Resource Capacity 5")]
        public int m_inputCapacity5 = 50;

        [CustomizableProperty("Input Resource Capacity 6")]
        public int m_inputCapacity6 = 50;

        [CustomizableProperty("Input Resource Capacity 7")]
        public int m_inputCapacity7 = 50;

        [CustomizableProperty("Output Meals Count")]
        public int m_outputCount = 50;

        [NonSerialized]
        public int m_finalProductionRate;

        [CustomizableProperty("Input Resource Threshold")]
        public int m_resourceThreshold = 10;

        [CustomizableProperty("Food Sale Price")]
        public int m_goodsSellPrice = 1500;

        [CustomizableProperty("Quality (values: 1-3 including 1 and 3)")]
        public int quality = 2;

        [CustomizableProperty("Allow delivery")]
        public bool allow_delivery = true;

        [NonSerialized]
        private bool m_hasBufferStatusMeshes;

        public VehicleInfo delivery_vehicle;

        [CustomizableProperty("Extended Input Resource 1")]
        public ExtendedTransferManager.TransferReason m_inputResource1 = ExtendedTransferManager.TransferReason.DrinkSupplies;

        [CustomizableProperty("Extended Input Resource 2")]
        public ExtendedTransferManager.TransferReason m_inputResource2 = ExtendedTransferManager.TransferReason.FoodSupplies;

        [CustomizableProperty("Extended Input Resource 3")]
        public ExtendedTransferManager.TransferReason m_inputResource3 = ExtendedTransferManager.TransferReason.None;

        [CustomizableProperty("Input Resource 4")]
        public TransferManager.TransferReason m_inputResource4 = TransferManager.TransferReason.None;

        [CustomizableProperty("Input Resource 5")]
        public TransferManager.TransferReason m_inputResource5 = TransferManager.TransferReason.None;

        [CustomizableProperty("Input Resource 6")]
        public TransferManager.TransferReason m_inputResource6 = TransferManager.TransferReason.None;

        [CustomizableProperty("Input Resource 7")]
        public TransferManager.TransferReason m_inputResource7 = TransferManager.TransferReason.None;

        [CustomizableProperty("Extended Output Resource")]
        public ExtendedTransferManager.TransferReason m_outputResource = ExtendedTransferManager.TransferReason.Meals; // consumed by citizens and delivery 

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
            int visitorCount = m_visitPlaceCount0 + m_visitPlaceCount1 + m_visitPlaceCount2;
            Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, 0, workCount, visitorCount, 0, 0);
        }

        public override void BuildingLoaded(ushort buildingID, ref Building data, uint version)
        {
            base.BuildingLoaded(buildingID, ref data, version);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            int visitorCount = m_visitPlaceCount0 + m_visitPlaceCount1 + m_visitPlaceCount2;
            EnsureCitizenUnits(buildingID, ref data, 0, workCount, visitorCount, 0);
        }

        public override void ReleaseBuilding(ushort buildingID, ref Building data)
        {
            base.ReleaseBuilding(buildingID, ref data);
        }

        public override void EndRelocating(ushort buildingID, ref Building data)
        {
            base.EndRelocating(buildingID, ref data);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            int visitorCount = m_visitPlaceCount0 + m_visitPlaceCount1 + m_visitPlaceCount2;
            EnsureCitizenUnits(buildingID, ref data, 0, workCount, visitorCount, 0);
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
            if (m_info != null && m_info.m_class != null && m_info.m_class.m_service == ItemClass.Service.Fishing)
            {
                GuideController properties = Singleton<GuideManager>.instance.m_properties;
                if (properties != null && Singleton<BuildingManager>.instance.m_intercityBusStationBuilt != null)
                {
                    Singleton<BuildingManager>.instance.m_intercityBusStationBuilt.Activate(properties.m_fishFactoryMarketBuilt, buildingID);
                }
            }
        }

        void IExtendedBuildingAI.ExtendedStartTransfer(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, ExtendedTransferManager.Offer offer)
        {
            if (material == ExtendedTransferManager.TransferReason.MealsDeliveryLow || material == ExtendedTransferManager.TransferReason.MealsDeliveryMedium || material == ExtendedTransferManager.TransferReason.MealsDeliveryHigh)
            {
                uint citizen = offer.Citizen;
                ushort buildingByLocation = Singleton<CitizenManager>.instance.m_citizens.m_buffer[(int)(UIntPtr)citizen].GetBuildingByLocation();
                // new citizen order
                RestaurantDeliveriesManager.RestaurantDeliveries.Add(new RestaurantDeliveriesManager.RestaurantDeliveryData
                {
                    deliveryVehicleId = 0,
                    buildingId = buildingByLocation,
                    citizenId = citizen
                });
                // cook meal for this citizen who ordered
                CookOrderMeal(buildingID);
                // check if we got to the number of orders to vehicle can carry
                if (!CheckIfDeliveryOrderInProgress())
                {
                    Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
		    if (ExtedndedVehicleManager.CreateVehicle(out var vehicle, ref Singleton<SimulationManager>.instance.m_randomizer, delivery_vehicle, data.m_position, (byte)ExtendedTransferManager.TransferReason.Meals, transferToSource: false, transferToTarget: true))
		    {
                        // get a list of indexes where vehicle id is 0
                        var index_list = Enumerable.Range(0, RestaurantDeliveriesManager.RestaurantDeliveries.Count).Where(i => RestaurantDeliveriesManager.RestaurantDeliveries[i].deliveryVehicleId == 0).ToList();

                        var first_delivery = false;
                        RestaurantDeliveriesManager.RestaurantDeliveryData deliveryData = new RestaurantDeliveriesManager.RestaurantDeliveryData
                        {
                            deliveryVehicleId = 0,
                            buildingId = 0,
                            citizenId = 0
                        };
                        // assign the new delivery vehicle id to all the orders
                        foreach(var index in index_list)
                        {
                            var item = RestaurantDeliveriesManager.RestaurantDeliveries[index];
                            item.deliveryVehicleId = vehicle;
                            RestaurantDeliveriesManager.RestaurantDeliveries[index] = item;
                            if(!first_delivery)
                            {
                                // assign first order to drive to
                                deliveryData = item;
                                first_delivery = true;
                            }
                        }
                        if(first_delivery && deliveryData.buildingId != 0)
                        {
                            // go to first delivery
                            delivery_vehicle.m_vehicleAI.SetSource(vehicle, ref vehicles.m_buffer[vehicle], buildingID);
                            delivery_vehicle.m_vehicleAI.SetTarget(vehicle, ref vehicles.m_buffer[vehicle], deliveryData.buildingId);
                        }
		    }
                }
            }
        }

        private void CheckCapacity(ushort buildingID, ref Building buildingData)
        {
            var custom_buffers = CustomBuffersManager.GetCustomBuffer(buildingID);
            int customBuffer = custom_buffers.m_customBuffer1;
            if (customBuffer * 3 >= m_outputCount * 2)
            {
                if ((buildingData.m_flags & Building.Flags.CapacityFull) != Building.Flags.CapacityFull)
                {
                    buildingData.m_flags |= Building.Flags.CapacityFull;
                }
            }
            else if (customBuffer * 3 >= m_outputCount)
            {
                if ((buildingData.m_flags & Building.Flags.CapacityFull) != Building.Flags.CapacityStep2)
                {
                    buildingData.m_flags = (buildingData.m_flags & ~Building.Flags.CapacityFull) | Building.Flags.CapacityStep2;
                }
            }
            else if (customBuffer >= m_outputCount * 2)
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
            var custom_buffers = CustomBuffersManager.GetCustomBuffer(buildingID);
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
                    // citizen eat one meal
                    int m_customBuffer9 = custom_buffers.m_customBuffer9;
                    amountDelta = Mathf.Clamp(amountDelta, 0, m_customBuffer9);
                    m_customBuffer9 -= amountDelta;
                    custom_buffers.m_customBuffer9 = (ushort)m_customBuffer9;
                    data.m_outgoingProblemTimer = 0;
                    byte park = Singleton<DistrictManager>.instance.GetPark(data.m_position);
                    if (park != 0 && Singleton<DistrictManager>.instance.m_parks.m_buffer[park].IsPedestrianZone)
                    {
                        Singleton<DistrictManager>.instance.m_parks.m_buffer[park].m_tempGoodsSold -= (uint)amountDelta;
                    }
                    int cashCapacity = 4000 * 4;
                    // take money if citizen eat the meal
                    if (amountDelta != 0)
                    {
                        var customMealPrice = Singleton<SimulationManager>.instance.m_randomizer.Int32(50, 150);
                        data.m_cashBuffer = Math.Min(data.m_cashBuffer + customMealPrice, cashCapacity);
                    }
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
                        int m_customBuffer4 = custom_buffers.m_customBuffer4;
                        amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer4, m_inputCapacity4 - m_customBuffer4);
                        m_customBuffer4 += amountDelta;
                        custom_buffers.m_customBuffer4 = (ushort)m_customBuffer4;
                    }
                    else if (material == m_inputResource5)
                    {
                        int m_customBuffer5 = custom_buffers.m_customBuffer5;
                        amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer5, m_inputCapacity5 - m_customBuffer5);
                        m_customBuffer5 += amountDelta;
                        custom_buffers.m_customBuffer5 = (ushort)m_customBuffer5;
                    }
                    else if (material == m_inputResource6)
                    {
                        int m_customBuffer6 = custom_buffers.m_customBuffer6;
                        amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer6, m_inputCapacity6 - m_customBuffer6);
                        m_customBuffer6 += amountDelta;
                        custom_buffers.m_customBuffer6 = (ushort)m_customBuffer6;
                    }
                    else if (material == m_inputResource7)
                    {
                        int m_customBuffer7 = custom_buffers.m_customBuffer7;
                        amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer7, m_inputCapacity7 - m_customBuffer7);
                        m_customBuffer7 += amountDelta;
                        custom_buffers.m_customBuffer7 = (ushort)m_customBuffer7;
                    }
                    else
                    {
                        base.ModifyMaterialBuffer(buildingID, ref data, material, ref amountDelta);
                    }
                    CustomBuffersManager.SetCustomBuffer(buildingID, custom_buffers);
                    break;
            }
        }

        void IExtendedBuildingAI.ExtendedGetMaterialAmount(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, out int amount, out int max)
        {
            amount = 0;
            max = 0;
        }

        void IExtendedBuildingAI.ExtendedModifyMaterialBuffer(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, ref int amountDelta)
        {
            var custom_buffers = CustomBuffersManager.GetCustomBuffer(buildingID);
            if (material == m_inputResource1)
            {
                int customBuffer1 = custom_buffers.m_customBuffer1;
                amountDelta = Mathf.Clamp(amountDelta, -customBuffer1, m_inputCapacity1 - customBuffer1);
                customBuffer1 += amountDelta;
                custom_buffers.m_customBuffer1 = (ushort)customBuffer1;
            }
            else if (material == m_inputResource2)
            {
                int m_customBuffer2 = custom_buffers.m_customBuffer2;
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer2, m_inputCapacity2 - m_customBuffer2);
                m_customBuffer2 += amountDelta;
                custom_buffers.m_customBuffer2 = (ushort)m_customBuffer2;
            }
            else if (material == m_inputResource3)
            {
                int m_customBuffer3 = custom_buffers.m_customBuffer3;
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer3, m_inputCapacity3 - m_customBuffer3);
                m_customBuffer3 += amountDelta;
                custom_buffers.m_customBuffer3 = (ushort)m_customBuffer3;
            }
            else if (material == ExtendedTransferManager.TransferReason.MealsDeliveryLow ||
                material == ExtendedTransferManager.TransferReason.MealsDeliveryMedium ||
                material == ExtendedTransferManager.TransferReason.MealsDeliveryHigh ||
                material == ExtendedTransferManager.TransferReason.Meals)
            {
                int m_customBuffer8 = custom_buffers.m_customBuffer8;
                amountDelta = Mathf.Clamp(amountDelta, -m_customBuffer8, m_outputCount - m_customBuffer8);
                m_customBuffer8 += amountDelta;
                custom_buffers.m_customBuffer8 = (ushort)m_customBuffer8;
            }
            CustomBuffersManager.SetCustomBuffer(buildingID, custom_buffers);
        }

        public override void BuildingDeactivated(ushort buildingID, ref Building data)
        {
            TransferManager.TransferOffer offer = default;
            ExtendedTransferManager.Offer extended_offer = default;
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

        public override void VisitorEnter(ushort buildingID, ref Building data, uint citizen)
        {
            int amountDelta = 1;
            CookCustomerMeal(buildingID);
            ModifyMaterialBuffer(buildingID, ref data, TransferManager.TransferReason.Shopping, ref amountDelta);
            base.VisitorEnter(buildingID, ref data, citizen);
        }

        protected override void ProduceGoods(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(buildingData.m_position);
            DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[(int)district].m_servicePolicies;
            District[] buffer = instance.m_districts.m_buffer;
            byte b = district;
            buffer[(int)b].m_servicePoliciesEffect = buffer[(int)b].m_servicePoliciesEffect | (servicePolicies & (DistrictPolicies.Services.PowerSaving | DistrictPolicies.Services.WaterSaving | DistrictPolicies.Services.SmokeDetectors | DistrictPolicies.Services.Recycling | DistrictPolicies.Services.RecreationalUse | DistrictPolicies.Services.ExtraInsulation | DistrictPolicies.Services.NoElectricity | DistrictPolicies.Services.OnlyElectricity | DistrictPolicies.Services.FreeWifi));
            Notification.ProblemStruct problemStruct = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem1.NoResources | Notification.Problem1.NoPlaceforGoods | Notification.Problem1.NoInputProducts | Notification.Problem1.NoFishingGoods);

            var custom_buffers = CustomBuffersManager.GetCustomBuffer(buildingID);
            if (m_inputResource1 != ExtendedTransferManager.TransferReason.None)
            {
                if (custom_buffers.m_customBuffer1 == 0)
                {
                    finalProductionRate = 0;
                }
            }
            if (m_inputResource2 != ExtendedTransferManager.TransferReason.None)
            {
                if (custom_buffers.m_customBuffer2 == 0)
                {
                    finalProductionRate = 0;
                }
            }
            if (m_inputResource3 != ExtendedTransferManager.TransferReason.None)
            {
                if (custom_buffers.m_customBuffer3 == 0)
                {
                    finalProductionRate = 0;
                }
            }
            if (m_inputResource4 != TransferManager.TransferReason.None)
            {
                if (custom_buffers.m_customBuffer4 == 0)
                {
                    finalProductionRate = 0;
                }
            }
            if (m_inputResource5 != TransferManager.TransferReason.None)
            {
                if (custom_buffers.m_customBuffer5 == 0)
                {
                    finalProductionRate = 0;
                }
            }
            if (m_inputResource6 != TransferManager.TransferReason.None)
            {
                if (custom_buffers.m_customBuffer6 == 0)
                {
                    finalProductionRate = 0;
                }
            }
            if (m_inputResource7 != TransferManager.TransferReason.None)
            {
                if (custom_buffers.m_customBuffer7 == 0)
                {
                    finalProductionRate = 0;
                }
            }
            m_finalProductionRate = finalProductionRate;
            base.HandleDead(buildingID, ref buildingData, ref behaviour, totalWorkerCount + totalVisitorCount);
            int TempOutput = 0;
            if (m_inputResource1 != ExtendedTransferManager.TransferReason.None)
            {
                if (custom_buffers.m_customBuffer1 < m_resourceThreshold)
                {
                    problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.NoInputProducts);
                }
                int count1 = 0;
                int cargo1 = 0;
                int capacity1 = 0;
                int outside1 = 0;
                ExtedndedVehicleManager.CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource1, ref count1, ref cargo1, ref capacity1, ref outside1);
                int InputSize1 = custom_buffers.m_customBuffer1 + cargo1;
                if (InputSize1 <= m_resourceThreshold)
                {
                    ExtendedTransferManager.Offer offer = default;
                    offer.Building = buildingID;
                    offer.Position = buildingData.m_position;
                    offer.Amount = 1;
                    offer.Active = false;
                    Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(m_inputResource1, offer);
                }
            }
            if (m_inputResource2 != ExtendedTransferManager.TransferReason.None)
            {
                if (custom_buffers.m_customBuffer2 < m_resourceThreshold)
                {
                    problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.NoInputProducts);
                }
                int count2 = 0;
                int cargo2 = 0;
                int capacity2 = 0;
                int outside2 = 0;
                ExtedndedVehicleManager.CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource2, ref count2, ref cargo2, ref capacity2, ref outside2);
                int InputSize2 = custom_buffers.m_customBuffer2 + cargo2;
                if (InputSize2 <= m_resourceThreshold)
                {
                    ExtendedTransferManager.Offer offer2 = default;
                    offer2.Building = buildingID;
                    offer2.Position = buildingData.m_position;
                    offer2.Amount = 1;
                    offer2.Active = false;
                    Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(m_inputResource2, offer2);
                }
            }
            if (m_inputResource3 != ExtendedTransferManager.TransferReason.None)
            {
                if (custom_buffers.m_customBuffer3 < m_resourceThreshold)
                {
                    problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.NoInputProducts);
                }
                int count3 = 0;
                int cargo3 = 0;
                int capacity3 = 0;
                int outside3 = 0;
                ExtedndedVehicleManager.CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource3, ref count3, ref cargo3, ref capacity3, ref outside3);
                int InputSize3 = custom_buffers.m_customBuffer3 + cargo3;
                if (InputSize3 <= m_resourceThreshold)
                {
                    ExtendedTransferManager.Offer offer3 = default;
                    offer3.Building = buildingID;
                    offer3.Position = buildingData.m_position;
                    offer3.Amount = 1;
                    offer3.Active = false;
                    Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(m_inputResource3, offer3);
                }
            }
            if (m_inputResource4 != TransferManager.TransferReason.None)
            {
                if (custom_buffers.m_customBuffer4 < m_resourceThreshold)
                {
                    problemStruct = Notification.AddProblems(problemStruct, (!IsRawMaterial(m_inputResource4)) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources);
                }
                int count4 = 0;
                int cargo4 = 0;
                int capacity4 = 0;
                int outside4 = 0;
                CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource4, ref count4, ref cargo4, ref capacity4, ref outside4);
                if (outside4 != 0)
                {
                    TempOutput |= 1;
                }
                int InputSize4 = custom_buffers.m_customBuffer4 + cargo4;
                if (InputSize4 <= m_resourceThreshold)
                {
                    TransferManager.TransferOffer offer4 = default;
                    offer4.Priority = Mathf.Max(1, InputSize4 * 8 / m_inputCapacity4);
                    offer4.Building = buildingID;
                    offer4.Position = buildingData.m_position;
                    offer4.Amount = 1;
                    offer4.Active = false;
                    Singleton<TransferManager>.instance.AddIncomingOffer(m_inputResource4, offer4);
                }
            }
            if (m_inputResource5 != TransferManager.TransferReason.None)
            {
                if (custom_buffers.m_customBuffer5 < m_resourceThreshold)
                {
                    problemStruct = Notification.AddProblems(problemStruct, (!IsRawMaterial(m_inputResource5)) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources);
                }
                int count5 = 0;
                int cargo5 = 0;
                int capacity5 = 0;
                int outside5 = 0;
                CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource5, ref count5, ref cargo5, ref capacity5, ref outside5);
                if (outside5 != 0)
                {
                    TempOutput |= 2;
                }
                int InputSize5 = custom_buffers.m_customBuffer5 + cargo5;
                if (InputSize5 <= m_resourceThreshold)
                {
                    TransferManager.TransferOffer offer5 = default;
                    offer5.Priority = Mathf.Max(1, InputSize5 * 8 / m_inputCapacity5);
                    offer5.Building = buildingID;
                    offer5.Position = buildingData.m_position;
                    offer5.Amount = 1;
                    offer5.Active = false;
                    Singleton<TransferManager>.instance.AddIncomingOffer(m_inputResource5, offer5);
                }
            }
            if (m_inputResource6 != TransferManager.TransferReason.None)
            {
                if (custom_buffers.m_customBuffer6 < m_resourceThreshold)
                {
                    problemStruct = Notification.AddProblems(problemStruct, (!IsRawMaterial(m_inputResource6)) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources);
                }
                int count6 = 0;
                int cargo6 = 0;
                int capacity6 = 0;
                int outside6 = 0;
                CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource6, ref count6, ref cargo6, ref capacity6, ref outside6);
                if (outside6 != 0)
                {
                    TempOutput |= 4;
                }
                int InputSize6 = custom_buffers.m_customBuffer6 + cargo6;
                if (InputSize6 <= m_resourceThreshold)
                {
                    TransferManager.TransferOffer offer6 = default;
                    offer6.Priority = Mathf.Max(1, InputSize6 * 8 / m_inputCapacity6);
                    offer6.Building = buildingID;
                    offer6.Position = buildingData.m_position;
                    offer6.Amount = 1;
                    offer6.Active = false;
                    Singleton<TransferManager>.instance.AddIncomingOffer(m_inputResource6, offer6);
                }
            }
            if (m_inputResource7 != TransferManager.TransferReason.None)
            {
                if (custom_buffers.m_customBuffer7 < m_resourceThreshold)
                {
                    problemStruct = Notification.AddProblems(problemStruct, (!IsRawMaterial(m_inputResource7)) ? Notification.Problem1.NoInputProducts : Notification.Problem1.NoResources);
                }
                int count7 = 0;
                int cargo7 = 0;
                int capacity7 = 0;
                int outside7 = 0;
                CalculateGuestVehicles(buildingID, ref buildingData, m_inputResource7, ref count7, ref cargo7, ref capacity7, ref outside7);
                if (outside7 != 0)
                {
                    TempOutput |= 8;
                }
                int InputSize7 = custom_buffers.m_customBuffer7 + cargo7;
                if (InputSize7 <= m_resourceThreshold)
                {
                    TransferManager.TransferOffer offer7 = default;
                    offer7.Priority = Mathf.Max(1, InputSize7 * 8 / m_inputCapacity7);
                    offer7.Building = buildingID;
                    offer7.Position = buildingData.m_position;
                    offer7.Amount = 1;
                    offer7.Active = false;
                    Singleton<TransferManager>.instance.AddIncomingOffer(m_inputResource7, offer7);
                }
            }
            buildingData.m_tempImport |= (byte)TempOutput;
            if (m_outputResource != ExtendedTransferManager.TransferReason.None)
            {
                if (m_DeliveryVehicleCount != 0)
                {
                    int count8 = 0;
                    int cargo8 = 0;
                    int capacity8 = 0;
                    int outside8 = 0;
                    ExtedndedVehicleManager.CalculateOwnVehicles(buildingID, ref buildingData, m_outputResource, ref count8, ref cargo8, ref capacity8, ref outside8);
                    buildingData.m_tempExport = (byte)Mathf.Clamp(outside8, buildingData.m_tempExport, 255);
                    if (count8 < m_DeliveryVehicleCount && allow_delivery)
                    {
                        var material = ExtendedTransferManager.TransferReason.None;
                        if (quality == 1)
                        {
                            ExtendedTransferManager.Offer offer8 = default;
                            offer8.Building = buildingID;
                            offer8.Position = buildingData.m_position;
                            offer8.Amount = 1;
                            offer8.Active = true;
                            material = ExtendedTransferManager.TransferReason.MealsDeliveryLow;
                            Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(material, offer8);
                        }
                        else if (quality == 2)
                        {
                            ExtendedTransferManager.Offer offer9 = default;
                            offer9.Building = buildingID;
                            offer9.Position = buildingData.m_position;
                            offer9.Amount = 1;
                            offer9.Active = true;
                            material = ExtendedTransferManager.TransferReason.MealsDeliveryMedium;
                            Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(material, offer9);
                        }
                        else if (quality == 3)
                        {
                            ExtendedTransferManager.Offer offer10 = default;
                            offer10.Building = buildingID;
                            offer10.Position = buildingData.m_position;
                            offer10.Amount = 1;
                            offer10.Active = true;
                            material = ExtendedTransferManager.TransferReason.MealsDeliveryHigh;
                            Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(material, offer10);
                        }
                        if (!CheckIfDeliveryOrderInProgress() && material != ExtendedTransferManager.TransferReason.None)
                        {
                            CreateDeliveryOrder();
                        }

                    }
                }
                var outgoingTransferReason = GetOutgoingTransferReason(buildingID);
                if (outgoingTransferReason != TransferManager.TransferReason.None)
                {
                    int totalGoods = m_outputCount * 100;
                    // people that are not visiting the building but have the oppertunity to visit if they want
                    var citizenWhoCanVisit = Mathf.Max(0, visitPlaceCount - totalVisitorCount);
                    var d = visitPlaceCount * 500;
                    int f = Mathf.Max(d, totalGoods * 4);
                    var g = f;
                    if (finalProductionRate != 0)
                    {
                        g = Mathf.Min(g, f - buildingData.m_customBuffer2);
                        finalProductionRate = Mathf.Max(0, Mathf.Min(finalProductionRate, (g * 200 + f - 1) / f));
                        int num11 = (visitPlaceCount * finalProductionRate + 9) / 10;
                        if (Singleton<SimulationManager>.instance.m_isNightTime)
                        {
                            num11 = num11 + 1 >> 1;
                        }
                        num11 = Mathf.Max(0, Mathf.Min(num11, g));
                        buildingData.m_customBuffer2 += (ushort)num11;
                        finalProductionRate = (num11 + 9) / 10;
                        int num28 = buildingData.m_customBuffer2 - aliveVisitorCount * 100;
                        if (num28 >= 100 && citizenWhoCanVisit > 0)
                        {
                            TransferManager.TransferOffer offer6 = default;
                            offer6.Priority = Mathf.Max(1, num28 * 8 / f);
                            offer6.Building = buildingID;
                            offer6.Position = buildingData.m_position;
                            offer6.Amount = Mathf.Min(num28 / 100, citizenWhoCanVisit);
                            offer6.Active = true;
                            Singleton<TransferManager>.instance.AddOutgoingOffer(outgoingTransferReason, offer6);
                        }
                    }
                }
                if (Singleton<LoadingManager>.instance.SupportsExpansion(Expansion.FinanceExpansion) && Singleton<UnlockManager>.instance.Unlocked(ItemClass.SubService.PoliceDepartmentBank) && buildingData.m_fireIntensity == 0)
                {
                    int cashCapacity = 4000 * 4;
                    if (cashCapacity != 0)
                    {
                        int num54 = buildingData.m_cashBuffer;
                        if (num54 >= cashCapacity / 8 && Singleton<SimulationManager>.instance.m_randomizer.Int32(5U) == 0)
                        {
                            int num55 = 0;
                            int num56 = 0;
                            int num57 = 0;
                            int num58 = 0;
                            base.CalculateGuestVehicles(buildingID, ref buildingData, TransferManager.TransferReason.Cash, ref num55, ref num56, ref num57, ref num58);
                            num54 -= num57 - num56;
                            if (num54 >= cashCapacity / 8)
                            {
                                TransferManager.TransferOffer transferOffer3 = default;
                                transferOffer3.Priority = num54 * 8 / cashCapacity;
                                transferOffer3.Building = buildingID;
                                transferOffer3.Position = buildingData.m_position;
                                transferOffer3.Amount = 1;
                                Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Cash, transferOffer3);
                            }
                        }
                    }
                }
            }
            buildingData.m_problems = problemStruct;
            int healthAccumulation = 0;
            int wellbeingAccumulation = 0;
            if (behaviour.m_healthAccumulation != 0)
            {
                if (aliveWorkerCount + aliveVisitorCount != 0)
                {
                    healthAccumulation = (behaviour.m_healthAccumulation + (aliveWorkerCount + aliveVisitorCount >> 1)) / (aliveWorkerCount + aliveVisitorCount);
                }
            }
            if (behaviour.m_wellbeingAccumulation != 0)
            {
                if (aliveWorkerCount + aliveVisitorCount != 0)
                {
                    wellbeingAccumulation = (behaviour.m_wellbeingAccumulation + (aliveWorkerCount + aliveVisitorCount >> 1)) / (aliveWorkerCount + aliveVisitorCount);
                }
            }
            int happines = Citizen.GetHappiness(healthAccumulation, wellbeingAccumulation) * 15 / 100;
            if ((buildingData.m_problems & Notification.Problem1.MajorProblem).IsNone)
            {
                happines += 20;
            }
            if (buildingData.m_problems.IsNone)
            {
                happines += 25;
            }
            happines = Mathf.Clamp(happines, 0, 100);
            buildingData.m_happiness = (byte)happines;
            buildingData.m_citizenCount = (byte)(aliveWorkerCount + aliveVisitorCount);
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
            string text = "";
            string text_water = LocaleFormatter.FormatGeneric("AIINFO_WATER_CONSUMPTION", GetWaterConsumption() * 16) + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_ELECTRICITY_CONSUMPTION", GetElectricityConsumption() * 16);
            if (m_outputResource != ExtendedTransferManager.TransferReason.None && m_DeliveryVehicleCount != 0)
            {
                text = Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLE_COUNT", m_DeliveryVehicleCount);
            }
            string baseTooltip = TooltipHelper.Append(base.GetLocalizedTooltip(), TooltipHelper.Format(LocaleFormatter.Info1, text_water, LocaleFormatter.Info2, text));
            if (m_outputResource != ExtendedTransferManager.TransferReason.None)
            {
                int num = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
                int num2 = m_visitPlaceCount0 + m_visitPlaceCount1 + m_visitPlaceCount2;
                baseTooltip = TooltipHelper.Append(baseTooltip, TooltipHelper.Format(LocaleFormatter.WorkplaceCount, LocaleFormatter.FormatGeneric("AIINFO_WORKPLACES_ACCUMULATION", num), LocaleFormatter.Info2, LocaleFormatter.FormatGeneric("AIINFO_VISITOR_CAPACITY", num2)));
            }
            bool flag1 = m_inputResource1 != ExtendedTransferManager.TransferReason.None;
            string text1 = ((m_inputResource1 == ExtendedTransferManager.TransferReason.None) ? string.Empty : "Drink Supplies");
            bool flag2 = m_inputResource2 != ExtendedTransferManager.TransferReason.None;
            string text2 = ((m_inputResource2 == ExtendedTransferManager.TransferReason.None) ? string.Empty : "Food Supplies");
            bool flag3 = m_inputResource3 != ExtendedTransferManager.TransferReason.None;
            string text3 = ((m_inputResource3 == ExtendedTransferManager.TransferReason.None) ? string.Empty : "Bread");
            bool flag4 = m_inputResource4 != TransferManager.TransferReason.None;
            string text4 = ((m_inputResource4 == TransferManager.TransferReason.None) ? string.Empty : IndustryWorldInfoPanel.ResourceSpriteName(m_inputResource4));
            bool flag5 = m_inputResource5 != TransferManager.TransferReason.None;
            string text5 = ((m_inputResource5 == TransferManager.TransferReason.None) ? string.Empty : IndustryWorldInfoPanel.ResourceSpriteName(m_inputResource5));
            bool flag6 = m_inputResource6 != TransferManager.TransferReason.None;
            string text6 = ((m_inputResource6 == TransferManager.TransferReason.None) ? string.Empty : IndustryWorldInfoPanel.ResourceSpriteName(m_inputResource6));
            bool flag7 = m_inputResource7 != TransferManager.TransferReason.None;
            string text7 = ((m_inputResource7 == TransferManager.TransferReason.None) ? string.Empty : IndustryWorldInfoPanel.ResourceSpriteName(m_inputResource7));
            string addTooltip = TooltipHelper.Format("arrowVisible", "true", "input1Visible", flag1.ToString(), "input2Visible", flag2.ToString(), "input3Visible", flag3.ToString(), "input4Visible", flag4.ToString(), "input5Visible", flag5.ToString(), "input6Visible", flag6.ToString(), "input7Visible", flag7.ToString(), "outputVisible", "true");
            string addTooltip2 = TooltipHelper.Format("input1", text1, "input2", text2, "input3", text3, "input4", text4, "input5", text5, "input6", text6, "input7", text7, "output", "Meals");
            baseTooltip = TooltipHelper.Append(baseTooltip, addTooltip);
            return TooltipHelper.Append(baseTooltip, addTooltip2);
        }

        public override string GetLocalizedStats(ushort buildingID, ref Building data)
        {
            var custom_buffers = CustomBuffersManager.GetCustomBuffer(buildingID);
            int m_customBuffer8 = custom_buffers.m_customBuffer8;
            string text = "meals cooked " + m_customBuffer8;
            if (m_outputResource != ExtendedTransferManager.TransferReason.None && m_DeliveryVehicleCount != 0)
            {
                int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
                int productionRate = GetProductionRate(100, budget);
                int delivery_vehicle_count = (productionRate * m_DeliveryVehicleCount + 99) / 100;
                int used_count = 0;
                int cargo = 0;
                int capacity = 0;
                int outside = 0;
                var material = m_outputResource;
                if (quality == 1)
                {
                    material = ExtendedTransferManager.TransferReason.MealsDeliveryLow;
                }
                if (quality == 2)
                {
                    material = ExtendedTransferManager.TransferReason.MealsDeliveryMedium;
                }
                if (quality == 3)
                {
                    material = ExtendedTransferManager.TransferReason.MealsDeliveryHigh;
                }
                ExtedndedVehicleManager.CalculateOwnVehicles(buildingID, ref data, material, ref used_count, ref cargo, ref capacity, ref outside);
                text = text + Environment.NewLine + "delivery vehicles in use " + used_count + "/" + delivery_vehicle_count;
            }
            return text;
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

        protected override void HandleWorkAndVisitPlaces(ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveWorkerCount, ref int totalWorkerCount, ref int workPlaceCount, ref int aliveVisitorCount, ref int totalVisitorCount, ref int visitPlaceCount)
        {
            workPlaceCount += m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            base.GetWorkBehaviour(buildingID, ref buildingData, ref behaviour, ref aliveWorkerCount, ref totalWorkerCount);
            base.HandleWorkPlaces(buildingID, ref buildingData, m_workPlaceCount0, m_workPlaceCount1, m_workPlaceCount2, m_workPlaceCount3, ref behaviour, aliveWorkerCount, totalWorkerCount);
            visitPlaceCount += m_visitPlaceCount0 + m_visitPlaceCount1 + m_visitPlaceCount2;
            base.GetVisitBehaviour(buildingID, ref buildingData, ref behaviour, ref aliveVisitorCount, ref totalVisitorCount);
        }

        public override bool RequireRoadAccess()
        {
            return true;
        }

        private bool IsRawMaterial(TransferManager.TransferReason material)
        {
            return material switch
            {
                TransferManager.TransferReason.Oil or TransferManager.TransferReason.Ore or TransferManager.TransferReason.Logs or TransferManager.TransferReason.Grain => true,
                _ => false,
            };
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

        private bool CheckIfDeliveryOrderInProgress()
        {
            var orders_with_no_vehicle = RestaurantDeliveriesManager.RestaurantDeliveries.FindAll(item => item.deliveryVehicleId == 0);
            if(delivery_vehicle != null)
            {
                RestaurantDeliveryVehicleAI restaurantDeliveryVehicleAI = delivery_vehicle.m_vehicleAI as RestaurantDeliveryVehicleAI;
                if (orders_with_no_vehicle.Count <= restaurantDeliveryVehicleAI.m_deliveryCapacity)
                {
                    return true;
                }
            }
            return false;
        }

        private void CreateDeliveryOrder()
        {
            var vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Commercial, ItemClass.SubService.CommercialLow, ItemClass.Level.Level3);
            if (vehicleInfo != null)
            {
                delivery_vehicle = vehicleInfo;
            }
        }

        private void CookCustomerMeal(ushort buildingID)
        {
            var custom_buffers = CustomBuffersManager.GetCustomBuffer(buildingID);
            if (m_inputResource1 != ExtendedTransferManager.TransferReason.None)
            {
                int CustomBuffer1 = custom_buffers.m_customBuffer1;
                int Input1ProductionRate = (m_finalProductionRate + 99) / 100;
                CustomBuffer1 = Mathf.Max(0, CustomBuffer1 - Input1ProductionRate);
                custom_buffers.m_customBuffer1 = (ushort)CustomBuffer1;
            }
            if (m_inputResource2 != ExtendedTransferManager.TransferReason.None)
            {
                int CustomBuffer2 = custom_buffers.m_customBuffer2;
                int Input2ProductionRate = (m_finalProductionRate + 99) / 100;
                CustomBuffer2 = Mathf.Max(0, CustomBuffer2 - Input2ProductionRate);
                custom_buffers.m_customBuffer2 = (ushort)CustomBuffer2;
            }
            if (m_inputResource3 != ExtendedTransferManager.TransferReason.None)
            {
                int CustomBuffer3 = custom_buffers.m_customBuffer3;
                int Input3ProductionRate = (m_finalProductionRate + 99) / 100;
                CustomBuffer3 = Mathf.Max(0, CustomBuffer3 - Input3ProductionRate);
                custom_buffers.m_customBuffer3 = (ushort)CustomBuffer3;
            }
            if (m_inputResource4 != TransferManager.TransferReason.None)
            {
                int CustomBuffer4 = custom_buffers.m_customBuffer4;
                int Input4ProductionRate = (m_finalProductionRate + 99) / 100;
                CustomBuffer4 = Mathf.Max(0, CustomBuffer4 - Input4ProductionRate);
                custom_buffers.m_customBuffer4 = (ushort)CustomBuffer4;
            }
            if (m_inputResource5 != TransferManager.TransferReason.None)
            {
                int CustomBuffer5 = custom_buffers.m_customBuffer5;
                int Input5ProductionRate = (m_finalProductionRate + 99) / 100;
                CustomBuffer5 = Mathf.Max(0, CustomBuffer5 - Input5ProductionRate);
                custom_buffers.m_customBuffer5 = (ushort)CustomBuffer5;
            }
            if (m_inputResource6 != TransferManager.TransferReason.None)
            {
                int CustomBuffer6 = custom_buffers.m_customBuffer6;
                int Input6ProductionRate = (m_finalProductionRate + 99) / 100;
                CustomBuffer6 = Mathf.Max(0, CustomBuffer6 - Input6ProductionRate);
                custom_buffers.m_customBuffer6 = (ushort)CustomBuffer6;
            }
            if (m_inputResource7 != TransferManager.TransferReason.None)
            {
                int CustomBuffer7 = custom_buffers.m_customBuffer7;
                int Input7ProductionRate = (m_finalProductionRate + 99) / 100;
                CustomBuffer7 = Mathf.Max(0, CustomBuffer7 - Input7ProductionRate);
                custom_buffers.m_customBuffer7 = (ushort)CustomBuffer7;
            }
            if (m_outputResource != ExtendedTransferManager.TransferReason.None)
            {
                int CustomBuffer9 = custom_buffers.m_customBuffer9;
                int OutputProductionRate = (m_finalProductionRate + 99) / 100;
                CustomBuffer9 = Mathf.Min(m_outputCount, CustomBuffer9 + OutputProductionRate);
                custom_buffers.m_customBuffer9 = (ushort)CustomBuffer9;
            }
            CustomBuffersManager.SetCustomBuffer(buildingID, custom_buffers);
        }

        private void CookOrderMeal(ushort buildingID)
        {
            var custom_buffers = CustomBuffersManager.GetCustomBuffer(buildingID);
            if (m_inputResource1 != ExtendedTransferManager.TransferReason.None)
            {
                int CustomBuffer1 = custom_buffers.m_customBuffer1;
                int Input1ProductionRate = (m_finalProductionRate + 99) / 100;
                CustomBuffer1 = Mathf.Max(0, CustomBuffer1 - Input1ProductionRate);
                custom_buffers.m_customBuffer1 = (ushort)CustomBuffer1;
            }
            if (m_inputResource2 != ExtendedTransferManager.TransferReason.None)
            {
                int CustomBuffer2 = custom_buffers.m_customBuffer2;
                int Input2ProductionRate = (m_finalProductionRate + 99) / 100;
                CustomBuffer2 = Mathf.Max(0, CustomBuffer2 - Input2ProductionRate);
                custom_buffers.m_customBuffer2 = (ushort)CustomBuffer2;
            }
            if (m_inputResource3 != ExtendedTransferManager.TransferReason.None)
            {
                int CustomBuffer3 = custom_buffers.m_customBuffer3;
                int Input3ProductionRate = (m_finalProductionRate + 99) / 100;
                CustomBuffer3 = Mathf.Max(0, CustomBuffer3 - Input3ProductionRate);
                custom_buffers.m_customBuffer3 = (ushort)CustomBuffer3;
            }
            if (m_inputResource4 != TransferManager.TransferReason.None)
            {
                int CustomBuffer4 = custom_buffers.m_customBuffer4;
                int Input4ProductionRate = (m_finalProductionRate + 99) / 100;
                CustomBuffer4 = Mathf.Max(0, CustomBuffer4 - Input4ProductionRate);
                custom_buffers.m_customBuffer4 = (ushort)CustomBuffer4;
            }
            if (m_inputResource5 != TransferManager.TransferReason.None)
            {
                int CustomBuffer5 = custom_buffers.m_customBuffer5;
                int Input5ProductionRate = (m_finalProductionRate + 99) / 100;
                CustomBuffer5 = Mathf.Max(0, CustomBuffer5 - Input5ProductionRate);
                custom_buffers.m_customBuffer5 = (ushort)CustomBuffer5;
            }
            if (m_inputResource6 != TransferManager.TransferReason.None)
            {
                int CustomBuffer6 = custom_buffers.m_customBuffer6;
                int Input6ProductionRate = (m_finalProductionRate + 99) / 100;
                CustomBuffer6 = Mathf.Max(0, CustomBuffer6 - Input6ProductionRate);
                custom_buffers.m_customBuffer6 = (ushort)CustomBuffer6;
            }
            if (m_inputResource7 != TransferManager.TransferReason.None)
            {
                int CustomBuffer7 = custom_buffers.m_customBuffer7;
                int Input7ProductionRate = (m_finalProductionRate + 99) / 100;
                CustomBuffer7 = Mathf.Max(0, CustomBuffer7 - Input7ProductionRate);
                custom_buffers.m_customBuffer7 = (ushort)CustomBuffer7;
            }
            if (m_outputResource != ExtendedTransferManager.TransferReason.None)
            {
                int CustomBuffer8 = custom_buffers.m_customBuffer8;
                int OutputProductionRate = (m_finalProductionRate + 99) / 100;
                CustomBuffer8 = Mathf.Min(m_outputCount, CustomBuffer8 + OutputProductionRate);
                custom_buffers.m_customBuffer8 = (ushort)CustomBuffer8;
            }
            CustomBuffersManager.SetCustomBuffer(buildingID, custom_buffers);
        }
    }

}
