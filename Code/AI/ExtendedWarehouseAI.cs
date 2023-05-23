using MoreTransferReasons;
using UnityEngine;
using ColossalFramework;
using IndustriesMeetsSunsetHarbor.Utils;
using ColossalFramework.Math;
using System;
using ColossalFramework.DataBinding;

namespace IndustriesMeetsSunsetHarbor.AI
{
    public class ExtendedWarehouseAI : PlayerBuildingAI, IExtendedBuildingAI
    {
        [CustomizableProperty("Uneducated Workers", "Workers", 0)]
        public int m_workPlaceCount0 = 2;

        [CustomizableProperty("Educated Workers", "Workers", 1)]
        public int m_workPlaceCount1 = 2;

        [CustomizableProperty("Well Educated Workers", "Workers", 2)]
        public int m_workPlaceCount2 = 1;

        [CustomizableProperty("Highly Educated Workers", "Workers", 3)]
        public int m_workPlaceCount3;

        [CustomizableProperty("Truck Count")]
        public int m_truckCount = 30;

        [CustomizableProperty("Storage Capacity")]
        public int m_storageCapacity = 1000000;

        [CustomizableProperty("Storage Type")]
        public TransferManager.TransferReason m_storageType = TransferManager.TransferReason.None;

        [CustomizableProperty("Storage Type")]
        public ExtendedTransferManager.TransferReason m_extendedStorageType = ExtendedTransferManager.TransferReason.None;

        [CustomizableProperty("Pollution Accumulation", "Pollution")]
        public int m_pollutionAccumulation;

        [CustomizableProperty("Pollution Radius", "Pollution")]
        public float m_pollutionRadius = 150f;

        [CustomizableProperty("Noise Accumulation", "Pollution")]
        public int m_noiseAccumulation = 50;

        [CustomizableProperty("Noise Radius", "Pollution")]
        public float m_noiseRadius = 100f;

        [CustomizableProperty("Animal Count")]
        public int m_animalCount = 3;

        public ManualMilestone m_fullPassMilestone;

        public override bool GetUseServicePoint(ushort buildingAI, ref Building data)
        {
            return false;
        }

        public override VehicleInfo.VehicleCategory GetRequiredVehicleAccess(ushort buildingAI, ref Building data)
        {
            return base.GetRequiredVehicleAccess(buildingAI, ref data) | VehicleInfo.VehicleCategory.CargoTruck;
        }

        public override void GetNaturalResourceRadius(ushort buildingID, ref Building data, out NaturalResourceManager.Resource resource1, out Vector3 position1, out float radius1, out NaturalResourceManager.Resource resource2, out Vector3 position2, out float radius2)
        {
            if (m_pollutionAccumulation != 0)
            {
                resource1 = NaturalResourceManager.Resource.Pollution;
                position1 = data.m_position;
                radius1 = m_pollutionRadius;
            }
            else
            {
                resource1 = NaturalResourceManager.Resource.None;
                position1 = data.m_position;
                radius1 = 0f;
            }
            resource2 = NaturalResourceManager.Resource.None;
            position2 = data.m_position;
            radius2 = 0f;
        }

        public override ImmaterialResourceManager.ResourceData[] GetImmaterialResourceRadius(ushort buildingID, ref Building data)
        {
            return new ImmaterialResourceManager.ResourceData[1]
            {
                new ImmaterialResourceManager.ResourceData
                {
                    m_resource = ImmaterialResourceManager.Resource.NoisePollution,
                    m_radius = ((m_noiseAccumulation == 0) ? 0f : m_noiseRadius)
                }
            };
        }

        public override Color GetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode)
        {
            switch (infoMode)
            {
                case InfoManager.InfoMode.Connections:
                    switch (subInfoMode)
                    {
                        case InfoManager.SubInfoMode.Default:
                            {
                                TransferManager.TransferReason actualTransferReason = GetActualTransferReason(buildingID, ref data);
                                ExtendedTransferManager.TransferReason actualTransferReason2 = GetActualExtendedTransferReason(buildingID, ref data);
                                if ((actualTransferReason != TransferManager.TransferReason.None || actualTransferReason2 != ExtendedTransferManager.TransferReason.None) && (data.m_tempImport != 0 || data.m_finalImport != 0))
                                {
                                    return Singleton<ExtendedTransferManager>.instance.m_properties.m_resourceColors[(int)actualTransferReason2];
                                }
                                return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                            }
                        case InfoManager.SubInfoMode.WaterPower:
                            {
                                TransferManager.TransferReason actualTransferReason = GetActualTransferReason(buildingID, ref data);
                                ExtendedTransferManager.TransferReason actualTransferReason2 = GetActualExtendedTransferReason(buildingID, ref data);
                                if ((actualTransferReason != TransferManager.TransferReason.None || actualTransferReason2 != ExtendedTransferManager.TransferReason.None) && (data.m_tempExport != 0 || data.m_finalExport != 0))
                                {
                                    return Singleton<ExtendedTransferManager>.instance.m_properties.m_resourceColors[(int)actualTransferReason];
                                }
                                return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                            }
                        default:
                            return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                    }
                case InfoManager.InfoMode.Pollution:
                    {
                        int pollutionAccumulation = m_pollutionAccumulation;
                        return ColorUtils.LinearLerp(Singleton<InfoManager>.instance.m_properties.m_neutralColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor, Mathf.Clamp01((float)pollutionAccumulation * 0.03f));
                    }
                case InfoManager.InfoMode.NoisePollution:
                    {
                        int noiseAccumulation = m_noiseAccumulation;
                        return GetNoisePollutionColor(noiseAccumulation);
                    }
                case InfoManager.InfoMode.Industry:
                    if (subInfoMode == IndustryBuildingAI.ServiceToInfoMode(m_info.m_class.m_subService))
                    {
                        if ((data.m_flags & Building.Flags.Active) != 0)
                        {
                            return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
                        }
                        return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_inactiveColor;
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                default:
                    return base.GetColor(buildingID, ref data, infoMode, subInfoMode);
            }
        }

        public override int GetResourceRate(ushort buildingID, ref Building data, NaturalResourceManager.Resource resource)
        {
            if (resource == NaturalResourceManager.Resource.Pollution)
            {
                int num = (int)data.m_customBuffer1 / 1000;
                return (num * m_pollutionAccumulation + 99) / 100;
            }
            return base.GetResourceRate(buildingID, ref data, resource);
        }

        public override int GetResourceRate(ushort buildingID, ref Building data, ImmaterialResourceManager.Resource resource)
        {
            if (resource == ImmaterialResourceManager.Resource.NoisePollution)
            {
                return m_noiseAccumulation;
            }
            return base.GetResourceRate(buildingID, ref data, resource);
        }

        public override string GetDebugString(ushort buildingID, ref Building data)
        {
            TransferManager.TransferReason actualTransferReason = GetActualTransferReason(buildingID, ref data);
            ExtendedTransferManager.TransferReason actualTransferReason2 = GetActualExtendedTransferReason(buildingID, ref data);
            if (actualTransferReason != TransferManager.TransferReason.None)
            {
                int count = 0;
                int cargo = 0;
                int capacity = 0;
                int outside = 0;
                CalculateGuestVehicles(buildingID, ref data, actualTransferReason, ref count, ref cargo, ref capacity, ref outside);
                int num = data.m_customBuffer1 * 100;
                return StringUtils.SafeFormat("{0}\n{1}: {2} (+{3})", base.GetDebugString(buildingID, ref data), actualTransferReason, num, cargo);
            }
            else if (actualTransferReason2 != ExtendedTransferManager.TransferReason.None)
            {
                int count = 0;
                int cargo = 0;
                int capacity = 0;
                int outside = 0;
                ExtedndedVehicleManager.CalculateGuestVehicles(buildingID, ref data, actualTransferReason2, ref count, ref cargo, ref capacity, ref outside);
                int num = data.m_customBuffer1 * 100;
                return StringUtils.SafeFormat("{0}\n{1}: {2} (+{3})", base.GetDebugString(buildingID, ref data), actualTransferReason2, num, cargo);
            }
            else
            {
                return base.GetDebugString(buildingID, ref data);
            }

        }

        public override void GetPlacementInfoMode(out InfoManager.InfoMode mode, out InfoManager.SubInfoMode subMode, float elevation)
        {
            mode = InfoManager.InfoMode.Industry;
            subMode = IndustryBuildingAI.ServiceToInfoMode(m_info.m_class.m_subService);
        }

        public override void InitializePrefab()
        {
            base.InitializePrefab();
            if (m_fullPassMilestone != null)
            {
                m_fullPassMilestone.SetPrefab(m_info);
            }
            if (m_storageCapacity >= 6553600)
            {
                throw new PrefabException(m_info, "Storage capacity >= 6553600");
            }
        }

        protected override string GetLocalizedStatusActive(ushort buildingID, ref Building data)
        {
            if (IsFull(buildingID, ref data))
            {
                return ColossalFramework.Globalization.Locale.Get("BUILDING_STATUS_FULL");
            }
            if ((data.m_flags & Building.Flags.Downgrading) != 0)
            {
                return ColossalFramework.Globalization.Locale.Get("BUILDING_STATUS_EMPTYING");
            }
            return base.GetLocalizedStatusActive(buildingID, ref data);
        }

        public override void CreateBuilding(ushort buildingID, ref Building data)
        {
            base.CreateBuilding(buildingID, ref data);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, 0, workCount, 0, 0, 0);
            data.m_seniors = byte.MaxValue;
            data.m_adults = byte.MaxValue;
            if (GetTransferReason(buildingID, ref data) == TransferManager.TransferReason.None && GetExtendedTransferReason(buildingID, ref data) == ExtendedTransferManager.TransferReason.None)
            {
                data.m_problems = Notification.AddProblems(data.m_problems, Notification.Problem1.ResourceNotSelected);
            }
        }

        public override void BuildingLoaded(ushort buildingID, ref Building data, uint version)
        {
            base.BuildingLoaded(buildingID, ref data, version);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            EnsureCitizenUnits(buildingID, ref data, 0, workCount, 0, 0);
        }

        public override void ReleaseBuilding(ushort buildingID, ref Building data)
        {
            if (IsFull(buildingID, ref data) && (object)m_fullPassMilestone != null)
            {
                m_fullPassMilestone.Relock();
            }
            ReleaseAnimals(buildingID, ref data);
            base.ReleaseBuilding(buildingID, ref data);
        }

        public override void EndRelocating(ushort buildingID, ref Building data)
        {
            base.EndRelocating(buildingID, ref data);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            EnsureCitizenUnits(buildingID, ref data, 0, workCount, 0, 0);
        }

        protected override void ManualActivation(ushort buildingID, ref Building buildingData)
        {
            if (m_pollutionAccumulation != 0 || m_noiseAccumulation != 0)
            {
                Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Sad, ImmaterialResourceManager.Resource.NoisePollution, m_noiseAccumulation, m_noiseRadius, buildingData.m_position, NotificationEvent.Type.Sad, NaturalResourceManager.Resource.Pollution, m_pollutionAccumulation, m_pollutionRadius);
            }
        }

        protected override void ManualDeactivation(ushort buildingID, ref Building buildingData)
        {
            if ((buildingData.m_flags & Building.Flags.Collapsed) != 0)
            {
                Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Happy, ImmaterialResourceManager.Resource.Abandonment, -buildingData.Width * buildingData.Length, 64f);
            }
            else if (m_pollutionAccumulation != 0 || m_noiseAccumulation != 0)
            {
                Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Happy, ImmaterialResourceManager.Resource.NoisePollution, -m_noiseAccumulation, m_noiseRadius, buildingData.m_position, NotificationEvent.Type.Happy, NaturalResourceManager.Resource.Pollution, -m_pollutionAccumulation, m_pollutionRadius);
            }
        }

        public override void SimulationStep(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            if ((m_info.m_animalPlaces == null || m_info.m_animalPlaces.Length == 0) && CountAnimals(buildingID, ref buildingData) < m_animalCount)
            {
                CreateAnimal(buildingID, ref buildingData);
            }
            base.SimulationStep(buildingID, ref buildingData, ref frameData);
            if (buildingData.m_customBuffer1 >= 1000)
            {
                int num = (int)buildingData.m_customBuffer1 / 1000;
                num = (num * m_pollutionAccumulation + 99) / 100;
                if (num != 0)
                {
                    num = UniqueFacultyAI.DecreaseByBonus(UniqueFacultyAI.FacultyBonus.Science, num);
                    Singleton<NaturalResourceManager>.instance.TryDumpResource(NaturalResourceManager.Resource.Pollution, num, num, buildingData.m_position, m_pollutionRadius);
                }
            }
            CheckCapacity(buildingID, ref buildingData);
            if ((Singleton<SimulationManager>.instance.m_currentFrameIndex & 0xFFF) >= 3840)
            {
                buildingData.m_finalExport = buildingData.m_tempExport;
                buildingData.m_finalImport = buildingData.m_tempImport;
                buildingData.m_tempExport = 0;
                buildingData.m_tempImport = 0;
            }
        }

        public override void StartTransfer(ushort buildingID, ref Building data, TransferManager.TransferReason material, TransferManager.TransferOffer offer)
        {
            if (material != GetActualTransferReason(buildingID, ref data))
            {
                base.StartTransfer(buildingID, ref data, material, offer);
                return;
            }
            VehicleInfo transferVehicleService = GetTransferVehicleService(material, ItemClass.Level.Level1, ref Singleton<SimulationManager>.instance.m_randomizer);
            if (transferVehicleService == null)
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

        void IExtendedBuildingAI.ExtendedStartTransfer(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, ExtendedTransferManager.Offer offer)
        {
            if (material != GetActualExtendedTransferReason(buildingID, ref data))
            {
                return;
            }
            VehicleInfo transferVehicleService = GetExtendedTransferVehicleService(material, ItemClass.Level.Level1, ref Singleton<SimulationManager>.instance.m_randomizer);
            if (transferVehicleService == null)
            {
                return;
            }
            Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
            if (ExtedndedVehicleManager.CreateVehicle(out var vehicle, ref Singleton<SimulationManager>.instance.m_randomizer, transferVehicleService, data.m_position, material, transferToSource: false, transferToTarget: true) && transferVehicleService.m_vehicleAI is ExtendedCargoTruckAI cargoTruckAI)
            {
                transferVehicleService.m_vehicleAI.SetSource(vehicle, ref vehicles.m_buffer[vehicle], buildingID);
                ((IExtendedVehicleAI)cargoTruckAI).ExtendedStartTransfer(vehicle, ref vehicles.m_buffer[(int)vehicle], material, offer);
                data.m_outgoingProblemTimer = 0;
            }
        }

        public static VehicleInfo GetTransferVehicleService(TransferManager.TransferReason material, ItemClass.Level level, ref Randomizer randomizer)
        {
            ItemClass.Service service = ItemClass.Service.Industrial;
            ItemClass.SubService subService = ItemClass.SubService.None;
            switch (material)
            {
                case TransferManager.TransferReason.Ore:
                case TransferManager.TransferReason.Coal:
                case TransferManager.TransferReason.Glass:
                case TransferManager.TransferReason.Metals:
                    subService = ItemClass.SubService.IndustrialOre;
                    break;
                case TransferManager.TransferReason.Logs:
                case TransferManager.TransferReason.Lumber:
                case TransferManager.TransferReason.Paper:
                case TransferManager.TransferReason.PlanedTimber:
                    subService = ItemClass.SubService.IndustrialForestry;
                    break;
                case TransferManager.TransferReason.Oil:
                case TransferManager.TransferReason.Petrol:
                case TransferManager.TransferReason.Petroleum:
                case TransferManager.TransferReason.Plastics:
                    subService = ItemClass.SubService.IndustrialOil;
                    break;
                case TransferManager.TransferReason.Grain:
                case TransferManager.TransferReason.Food:
                case TransferManager.TransferReason.Flours:
                    subService = ItemClass.SubService.IndustrialFarming;
                    break;
                case TransferManager.TransferReason.AnimalProducts:
                    service = ItemClass.Service.PlayerIndustry;
                    subService = ItemClass.SubService.PlayerIndustryFarming;
                    break;
                case TransferManager.TransferReason.Goods:
                    subService = ItemClass.SubService.IndustrialGeneric;
                    break;
                case TransferManager.TransferReason.LuxuryProducts:
                    service = ItemClass.Service.PlayerIndustry;
                    break;
                default:
                    return null;
            }
            return Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, service, subService, level);
        }

        public static VehicleInfo GetExtendedTransferVehicleService(ExtendedTransferManager.TransferReason material, ItemClass.Level level, ref Randomizer randomizer)
        {
            ItemClass.SubService subService = ItemClass.SubService.None;
            ItemClass.Service service;
            switch (material)
            {
                case ExtendedTransferManager.TransferReason.Bread:
                case ExtendedTransferManager.TransferReason.DrinkSupplies:
                case ExtendedTransferManager.TransferReason.FoodSupplies:
                    service = ItemClass.Service.PlayerIndustry;
                    break;
                default:
                    return null;
            }
            return Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref randomizer, service, subService, level);
        }

        void IExtendedBuildingAI.ExtendedGetMaterialAmount(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, out int amount, out int max)
        {
            amount = 0;
            max = 0;
        }

        public override void ModifyMaterialBuffer(ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int amountDelta)
        {
            if (material == GetActualTransferReason(buildingID, ref data))
            {
                int num = data.m_customBuffer1 * 100;
                amountDelta = Mathf.Clamp(amountDelta, -num, m_storageCapacity - num);
                data.m_customBuffer1 = (ushort)((num + amountDelta) / 100);
            }
            else
            {
                base.ModifyMaterialBuffer(buildingID, ref data, material, ref amountDelta);
            }
        }

        void IExtendedBuildingAI.ExtendedModifyMaterialBuffer(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, ref int amountDelta)
        {
            if (material == GetActualExtendedTransferReason(buildingID, ref data))
            {
                int num = data.m_customBuffer1 * 100;
                amountDelta = Mathf.Clamp(amountDelta, -num, m_storageCapacity - num);
                data.m_customBuffer1 = (ushort)((num + amountDelta) / 100);
            }
        }

        public override void BuildingDeactivated(ushort buildingID, ref Building data)
        {
            TransferManager.TransferReason actualTransferReason = GetActualTransferReason(buildingID, ref data);
            ExtendedTransferManager.TransferReason actualTransferReason2 = GetActualExtendedTransferReason(buildingID, ref data);
            if (actualTransferReason != TransferManager.TransferReason.None)
            {
                TransferManager.TransferOffer offer = default;
                offer.Building = buildingID;
                Singleton<TransferManager>.instance.RemoveIncomingOffer(actualTransferReason, offer);
                Singleton<TransferManager>.instance.RemoveOutgoingOffer(actualTransferReason, offer);
                RemoveGuestVehicles(buildingID, ref data, actualTransferReason);
            }
            else if (actualTransferReason2 != ExtendedTransferManager.TransferReason.None)
            {
                ExtendedTransferManager.Offer offer = default;
                offer.Building = buildingID;
                Singleton<ExtendedTransferManager>.instance.RemoveIncomingOffer(actualTransferReason2, offer);
                Singleton<ExtendedTransferManager>.instance.RemoveOutgoingOffer(actualTransferReason2, offer);
                RemoveExtendedGuestVehicles(buildingID, ref data, actualTransferReason2);
            }
            ReleaseAnimals(buildingID, ref data);
            if (data.m_subBuilding != 0 && data.m_parentBuilding == 0)
            {
                int num = 0;
                ushort subBuilding = data.m_subBuilding;
                while (subBuilding != 0)
                {
                    ReleaseAnimals(subBuilding, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[subBuilding]);
                    subBuilding = Singleton<BuildingManager>.instance.m_buildings.m_buffer[subBuilding].m_subBuilding;
                    if (++num > 49152)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        break;
                    }
                }
            }
            base.BuildingDeactivated(buildingID, ref data);
        }

        private void RemoveGuestVehicles(ushort buildingID, ref Building data, TransferManager.TransferReason material)
        {
            Vehicle[] buffer = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
            ushort num = data.m_guestVehicles;
            int num2 = 0;
            while (num != 0)
            {
                ushort nextGuestVehicle = buffer[num].m_nextGuestVehicle;
                if (buffer[num].m_targetBuilding == buildingID && ((uint)buffer[num].m_transferType & (uint)material) != 0)
                {
                    VehicleInfo info = buffer[num].Info;
                    if (info != null)
                    {
                        info.m_vehicleAI.SetTarget(num, ref buffer[num], 0);
                    }
                }
                num = nextGuestVehicle;
                if (++num2 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }

        private void RemoveExtendedGuestVehicles(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material)
        {
            Vehicle[] buffer = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
            ushort num = data.m_guestVehicles;
            int num2 = 0;
            while (num != 0)
            {
                ushort nextGuestVehicle = buffer[num].m_nextGuestVehicle;
                if (buffer[num].m_targetBuilding == buildingID && ((uint)buffer[num].m_transferType & (uint)material) != 0)
                {
                    VehicleInfo info = buffer[num].Info;
                    if (info != null)
                    {
                        info.m_vehicleAI.SetTarget(num, ref buffer[num], 0);
                    }
                }
                num = nextGuestVehicle;
                if (++num2 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }

        public override void CheckRoadAccess(ushort buildingID, ref Building data)
        {
            base.CheckRoadAccess(buildingID, ref data);
            Notification.ProblemStruct problems = data.m_problems;
            if (GetTransferReason(buildingID, ref data) == TransferManager.TransferReason.None && GetExtendedTransferReason(buildingID, ref data) == ExtendedTransferManager.TransferReason.None)
            {
                data.m_problems = Notification.AddProblems(data.m_problems, Notification.Problem1.ResourceNotSelected);
            }
            if (data.m_problems != problems)
            {
                Singleton<BuildingManager>.instance.UpdateNotifications(buildingID, problems, data.m_problems);
            }
        }

        protected override void HandleWorkAndVisitPlaces(ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveWorkerCount, ref int totalWorkerCount, ref int workPlaceCount, ref int aliveVisitorCount, ref int totalVisitorCount, ref int visitPlaceCount)
        {
            workPlaceCount += m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            GetWorkBehaviour(buildingID, ref buildingData, ref behaviour, ref aliveWorkerCount, ref totalWorkerCount);
            HandleWorkPlaces(buildingID, ref buildingData, m_workPlaceCount0, m_workPlaceCount1, m_workPlaceCount2, m_workPlaceCount3, ref behaviour, aliveWorkerCount, totalWorkerCount);
        }

        protected override void SimulationStepActive(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            base.SimulationStepActive(buildingID, ref buildingData, ref frameData);
        }

        protected override void ProduceGoods(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte b = instance.GetPark(buildingData.m_position);
            if (b != 0 && !instance.m_parks.m_buffer[b].IsIndustry)
            {
                b = 0;
            }
            if (finalProductionRate != 0)
            {
                HandleDead(buildingID, ref buildingData, ref behaviour, totalWorkerCount);
                TransferManager.TransferReason actualTransferReason = GetActualTransferReason(buildingID, ref buildingData);
                ExtendedTransferManager.TransferReason actualTransferReason2 = GetActualExtendedTransferReason(buildingID, ref buildingData);
                TransferManager.TransferReason transferReason = GetTransferReason(buildingID, ref buildingData);
                ExtendedTransferManager.TransferReason transferReason2 = GetExtendedTransferReason(buildingID, ref buildingData);
                if (actualTransferReason != TransferManager.TransferReason.None)
                {
                    int maxLoadSize = GetMaxLoadSize();
                    bool flag = IsFull(buildingID, ref buildingData);
                    int num = buildingData.m_customBuffer1 * 100;
                    int num2 = (finalProductionRate * m_truckCount + 99) / 100;
                    if ((buildingData.m_flags & Building.Flags.Downgrading) != 0)
                    {
                        RemoveGuestVehicles(buildingID, ref buildingData, actualTransferReason);
                    }
                    int count = 0;
                    int cargo = 0;
                    int capacity = 0;
                    int outside = 0;
                    CalculateOwnVehicles(buildingID, ref buildingData, actualTransferReason, ref count, ref cargo, ref capacity, ref outside);
                    buildingData.m_tempExport = (byte)Mathf.Clamp(outside, buildingData.m_tempExport, 255);
                    int count2 = 0;
                    int cargo2 = 0;
                    int capacity2 = 0;
                    int outside2 = 0;
                    CalculateGuestVehicles(buildingID, ref buildingData, actualTransferReason, ref count2, ref cargo2, ref capacity2, ref outside2);
                    buildingData.m_tempImport = (byte)Mathf.Clamp(outside2, buildingData.m_tempImport, 255);
                    if (b != 0)
                    {
                        instance.m_parks.m_buffer[b].AddBufferStatus(actualTransferReason, num, cargo2, m_storageCapacity);
                    }
                    if (transferReason != actualTransferReason)
                    {
                        if (num > 0 && count < num2)
                        {
                            TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                            offer.Priority = 8;
                            offer.Building = buildingID;
                            offer.Position = buildingData.m_position;
                            offer.Amount = Mathf.Min(Mathf.Max(1, num / Mathf.Max(1, maxLoadSize)), num2 - count);
                            offer.Active = true;
                            offer.Exclude = true;
                            Singleton<TransferManager>.instance.AddOutgoingOffer(actualTransferReason, offer);
                        }
                    }
                    else
                    {
                        if (num >= maxLoadSize && count < num2)
                        {
                            TransferManager.TransferOffer offer2 = default(TransferManager.TransferOffer);
                            if ((buildingData.m_flags & Building.Flags.Filling) != 0)
                            {
                                offer2.Priority = 0;
                            }
                            else if ((buildingData.m_flags & Building.Flags.Downgrading) != 0)
                            {
                                offer2.Priority = Mathf.Clamp(num / Mathf.Max(1, m_storageCapacity >> 2) + 2, 0, 2);
                            }
                            else
                            {
                                offer2.Priority = Mathf.Clamp(num / Mathf.Max(1, m_storageCapacity >> 2) - 1, 0, 2);
                            }
                            offer2.Building = buildingID;
                            offer2.Position = buildingData.m_position;
                            offer2.Amount = Mathf.Min(num / Mathf.Max(1, maxLoadSize), num2 - count);
                            offer2.Active = true;
                            offer2.Exclude = true;
                            Singleton<TransferManager>.instance.AddOutgoingOffer(actualTransferReason, offer2);
                        }
                        num += cargo2;
                        if (num < m_storageCapacity)
                        {
                            TransferManager.TransferOffer offer3 = default(TransferManager.TransferOffer);
                            if ((buildingData.m_flags & Building.Flags.Filling) != 0)
                            {
                                offer3.Priority = Mathf.Clamp((m_storageCapacity - num) / Mathf.Max(1, m_storageCapacity >> 2) + 1, 0, 2);
                            }
                            else if ((buildingData.m_flags & Building.Flags.Downgrading) != 0)
                            {
                                offer3.Priority = 0;
                            }
                            else
                            {
                                offer3.Priority = Mathf.Clamp((m_storageCapacity - num) / Mathf.Max(1, m_storageCapacity >> 2) - 1, 0, 2);
                            }
                            offer3.Building = buildingID;
                            offer3.Position = buildingData.m_position;
                            offer3.Amount = Mathf.Min(num2 - count, 1);
                            offer3.Active = false;
                            offer3.Exclude = true;
                            Singleton<TransferManager>.instance.AddIncomingOffer(actualTransferReason, offer3);
                        }
                    }
                    bool flag2 = IsFull(buildingID, ref buildingData);
                    if (flag != flag2)
                    {
                        if (flag2)
                        {
                            if ((object)m_fullPassMilestone != null)
                            {
                                m_fullPassMilestone.Unlock();
                            }
                        }
                        else if ((object)m_fullPassMilestone != null)
                        {
                            m_fullPassMilestone.Relock();
                        }
                    }
                }
                else if (actualTransferReason2 != ExtendedTransferManager.TransferReason.None)
                {
                    int maxLoadSize = GetMaxLoadSize();
                    bool flag = IsFull(buildingID, ref buildingData);
                    int num = buildingData.m_customBuffer1 * 100;
                    int num2 = (finalProductionRate * m_truckCount + 99) / 100;
                    if ((buildingData.m_flags & Building.Flags.Downgrading) != 0)
                    {
                        RemoveExtendedGuestVehicles(buildingID, ref buildingData, actualTransferReason2);
                    }
                    int count = 0;
                    int cargo = 0;
                    int capacity = 0;
                    int outside = 0;
                    ExtedndedVehicleManager.CalculateOwnVehicles(buildingID, ref buildingData, actualTransferReason2, ref count, ref cargo, ref capacity, ref outside);
                    int count2 = 0;
                    int cargo2 = 0;
                    int capacity2 = 0;
                    int outside2 = 0;
                    ExtedndedVehicleManager.CalculateGuestVehicles(buildingID, ref buildingData, actualTransferReason2, ref count2, ref cargo2, ref capacity2, ref outside2);
                    if (transferReason2 != actualTransferReason2)
                    {
                        if (num > 0 && count < num2)
                        {
                            ExtendedTransferManager.Offer offer = default;
                            offer.Building = buildingID;
                            offer.Position = buildingData.m_position;
                            offer.Amount = Mathf.Min(Mathf.Max(1, num / Mathf.Max(1, maxLoadSize)), num2 - count);
                            offer.Active = true;
                            Singleton<ExtendedTransferManager>.instance.AddOutgoingOffer(actualTransferReason2, offer);
                        }
                    }
                    else
                    {
                        if (num >= maxLoadSize && count < num2)
                        {
                            ExtendedTransferManager.Offer offer2 = default;
                            offer2.Building = buildingID;
                            offer2.Position = buildingData.m_position;
                            offer2.Amount = Mathf.Min(num / Mathf.Max(1, maxLoadSize), num2 - count);
                            offer2.Active = true;
                            Singleton<ExtendedTransferManager>.instance.AddOutgoingOffer(actualTransferReason2, offer2);
                        }
                        num += cargo2;
                        if (num < m_storageCapacity)
                        {
                            ExtendedTransferManager.Offer offer3 = default;
                            offer3.Building = buildingID;
                            offer3.Position = buildingData.m_position;
                            offer3.Amount = Mathf.Min(num2 - count, 1);
                            offer3.Active = false;
                            Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(actualTransferReason2, offer3);
                        }
                    }
                    bool flag2 = IsFull(buildingID, ref buildingData);
                    if (flag != flag2)
                    {
                        if (flag2)
                        {
                            if ((object)m_fullPassMilestone != null)
                            {
                                m_fullPassMilestone.Unlock();
                            }
                        }
                        else if ((object)m_fullPassMilestone != null)
                        {
                            m_fullPassMilestone.Relock();
                        }
                    }
                }
                if (actualTransferReason != transferReason && buildingData.m_customBuffer1 == 0)
                {
                    buildingData.m_adults = buildingData.m_seniors;
                    SetContentFlags(buildingID, ref buildingData, transferReason);
                }
                if (actualTransferReason2 != transferReason2 && buildingData.m_customBuffer1 == 0)
                {
                    buildingData.m_adults = buildingData.m_seniors;
                    SetExtendedContentFlags(buildingID, ref buildingData, transferReason2);
                }
                int num3 = finalProductionRate * m_noiseAccumulation / 100;
                if (num3 != 0)
                {
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, num3, buildingData.m_position, m_noiseRadius);
                }
            }
            base.ProduceGoods(buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
        }

        protected override bool CanEvacuate()
        {
            return m_workPlaceCount0 != 0 || m_workPlaceCount1 != 0 || m_workPlaceCount2 != 0 || m_workPlaceCount3 != 0;
        }

        private void CheckCapacity(ushort buildingID, ref Building buildingData)
        {
            int num = buildingData.m_customBuffer1 * 100;
            if (num * 3 >= m_storageCapacity * 2)
            {
                if ((buildingData.m_flags & Building.Flags.CapacityFull) != Building.Flags.CapacityFull)
                {
                    buildingData.m_flags |= Building.Flags.CapacityFull;
                }
            }
            else if (num * 3 >= m_storageCapacity)
            {
                if ((buildingData.m_flags & Building.Flags.CapacityFull) != Building.Flags.CapacityStep2)
                {
                    buildingData.m_flags = (buildingData.m_flags & ~Building.Flags.CapacityFull) | Building.Flags.CapacityStep2;
                }
            }
            else if (num >= GetMaxLoadSize())
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

        public override void CalculateSpawnPosition(ushort buildingID, ref Building data, ref Randomizer randomizer, CitizenInfo info, out Vector3 position, out Vector3 target)
        {
            if (info.m_citizenAI.IsAnimal())
            {
                if (!CalculateAnimalPosition(buildingID, ref data, ref randomizer, info, out position, out target, out var _, out var _))
                {
                    int num = data.Width * 300;
                    int num2 = data.Length * 300;
                    position.x = (float)randomizer.Int32(-num, num) * 0.1f;
                    position.y = 0f;
                    position.z = (float)randomizer.Int32(-num2, num2) * 0.1f;
                    position = data.CalculatePosition(position);
                    float f = (float)randomizer.Int32(360u) * ((float)Math.PI / 180f);
                    target = position;
                    target.x += Mathf.Cos(f);
                    target.z += Mathf.Sin(f);
                }
            }
            else
            {
                base.CalculateSpawnPosition(buildingID, ref data, ref randomizer, info, out position, out target);
            }
        }

        public override void CalculateUnspawnPosition(ushort buildingID, ref Building data, ref Randomizer randomizer, CitizenInfo info, ushort ignoreInstance, out Vector3 position, out Vector3 target, out Vector2 direction, out CitizenInstance.Flags specialFlags)
        {
            if (info.m_citizenAI.IsAnimal())
            {
                if (!CalculateAnimalPosition(buildingID, ref data, ref randomizer, info, out position, out target, out direction, out specialFlags))
                {
                    int num = data.Width * 300;
                    int num2 = data.Length * 300;
                    position.x = (float)randomizer.Int32(-num, num) * 0.1f;
                    position.y = m_info.m_size.y + (float)randomizer.Int32(1000u) * 0.1f;
                    position.z = (float)randomizer.Int32(-num2, num2) * 0.1f;
                    position = data.CalculatePosition(position);
                    target = position;
                    direction = Vector2.zero;
                    specialFlags = CitizenInstance.Flags.HangAround;
                }
            }
            else
            {
                base.CalculateUnspawnPosition(buildingID, ref data, ref randomizer, info, ignoreInstance, out position, out target, out direction, out specialFlags);
            }
        }

        private void CreateAnimal(ushort buildingID, ref Building data)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            Randomizer r = new Randomizer(buildingID);
            CitizenInfo groupAnimalInfo = instance.GetGroupAnimalInfo(ref r, m_info.m_class.m_service, m_info.m_class.m_subService);
            if ((object)groupAnimalInfo != null && instance.CreateCitizenInstance(out var instance2, ref Singleton<SimulationManager>.instance.m_randomizer, groupAnimalInfo, 0u))
            {
                groupAnimalInfo.m_citizenAI.SetSource(instance2, ref instance.m_instances.m_buffer[instance2], buildingID);
                groupAnimalInfo.m_citizenAI.SetTarget(instance2, ref instance.m_instances.m_buffer[instance2], buildingID);
            }
        }

        private void ReleaseAnimals(ushort buildingID, ref Building data)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            ushort num = data.m_targetCitizens;
            int num2 = 0;
            while (num != 0)
            {
                ushort nextTargetInstance = instance.m_instances.m_buffer[num].m_nextTargetInstance;
                if (instance.m_instances.m_buffer[num].Info.m_citizenAI.IsAnimal())
                {
                    instance.ReleaseCitizenInstance(num);
                }
                num = nextTargetInstance;
                if (++num2 > 65536)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }

        private int CountAnimals(ushort buildingID, ref Building data)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            ushort num = data.m_targetCitizens;
            int num2 = 0;
            int num3 = 0;
            while (num != 0)
            {
                ushort nextTargetInstance = instance.m_instances.m_buffer[num].m_nextTargetInstance;
                if (instance.m_instances.m_buffer[num].Info.m_citizenAI.IsAnimal())
                {
                    num2++;
                }
                num = nextTargetInstance;
                if (++num3 > 65536)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return num2;
        }

        public override void SetEmptying(ushort buildingID, ref Building data, bool emptying)
        {
            if (emptying)
            {
                data.m_flags = (data.m_flags & ~Building.Flags.Filling) | Building.Flags.Downgrading;
            }
            else
            {
                data.m_flags &= ~Building.Flags.Downgrading;
            }
        }

        public override void SetFilling(ushort buildingID, ref Building data, bool filling)
        {
            if (filling)
            {
                data.m_flags = (data.m_flags & ~Building.Flags.Downgrading) | Building.Flags.Filling;
            }
            else
            {
                data.m_flags &= ~Building.Flags.Filling;
            }
        }

        public void SetTransferReason(ushort buildingID, ref Building data, TransferManager.TransferReason material)
        {
            if (m_storageType != TransferManager.TransferReason.None)
            {
                return;
            }
            // remove old transfer reason
            if(data.m_seniors < 200)  // normal transfer manager
            {
                TransferManager.TransferReason seniors = (TransferManager.TransferReason)data.m_seniors;
                if (material != seniors)
                {
                    if (seniors != TransferManager.TransferReason.None)
                    {
                        TransferManager.TransferOffer offer = default;
                        offer.Building = buildingID;
                        Singleton<TransferManager>.instance.RemoveIncomingOffer(seniors, offer);
                        CancelIncomingTransfer(buildingID, ref data, seniors);
                    }
                }
            }
            else  // extended transfer manager
            {
                if(data.m_seniors != 255) // not none
                {
                    var material_byte =  data.m_seniors - 200;
                    ExtendedTransferManager.TransferReason extended = (ExtendedTransferManager.TransferReason)material_byte;
                    if (extended != ExtendedTransferManager.TransferReason.None)
                    {
                        ExtendedTransferManager.Offer offer = default;
                        offer.Building = buildingID;
                        Singleton<ExtendedTransferManager>.instance.RemoveIncomingOffer(extended, offer);
                        CancelExtendedIncomingTransfer(buildingID, ref data, extended);
                    }
                }
            }
            // set new transfer reason
            data.m_seniors = (byte)material;
            if (data.m_customBuffer1 == 0)
            {
                data.m_adults = (byte)material;
                SetContentFlags(buildingID, ref data, material);
            }
            Notification.ProblemStruct problems = data.m_problems;
            if (material == TransferManager.TransferReason.None)
            {
                data.m_problems = Notification.AddProblems(data.m_problems, Notification.Problem1.ResourceNotSelected);
            }
            else
            {
                data.m_problems = Notification.RemoveProblems(data.m_problems, Notification.Problem1.ResourceNotSelected);
            }
            if (data.m_problems != problems)
            {
                Singleton<BuildingManager>.instance.UpdateNotifications(buildingID, problems, data.m_problems);
            }
        }

        public void SetExtendedTransferReason(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material)
        {
            if (m_extendedStorageType != ExtendedTransferManager.TransferReason.None)
            {
                return;
            }
            // remove old transfer reason
            if(data.m_seniors >= 200)  // extended transfer manager
            {
                if(data.m_seniors != 255) // not none
                {
                    var material_byte =  data.m_seniors - 200;
                    ExtendedTransferManager.TransferReason extended = (ExtendedTransferManager.TransferReason)material_byte;
                    if (material != extended)
                    {
                        if (extended != ExtendedTransferManager.TransferReason.None)
                        {
                            ExtendedTransferManager.Offer offer = default;
                            offer.Building = buildingID;
                            Singleton<ExtendedTransferManager>.instance.RemoveIncomingOffer(extended, offer);
                            CancelExtendedIncomingTransfer(buildingID, ref data, extended);
                        }
                    }
                }
            }
            else // normal transfer manager
            {
                TransferManager.TransferReason seniors = (TransferManager.TransferReason)data.m_seniors;
                if (seniors != TransferManager.TransferReason.None)
                {
                    TransferManager.TransferOffer offer = default;
                    offer.Building = buildingID;
                    Singleton<TransferManager>.instance.RemoveIncomingOffer(seniors, offer);
                    CancelIncomingTransfer(buildingID, ref data, seniors);
                }
            }
            // set new transfer reason
            data.m_seniors = (byte)((byte)material + 200);
            if (data.m_customBuffer1 == 0)
            {
                data.m_adults = (byte)((byte)material + 200);
                SetExtendedContentFlags(buildingID, ref data, material);
            }
            Notification.ProblemStruct problems = data.m_problems;
            if (material == ExtendedTransferManager.TransferReason.None)
            {
                data.m_problems = Notification.AddProblems(data.m_problems, Notification.Problem1.ResourceNotSelected);
            }
            else
            {
                data.m_problems = Notification.RemoveProblems(data.m_problems, Notification.Problem1.ResourceNotSelected);
            }
            if (data.m_problems != problems)
            {
                Singleton<BuildingManager>.instance.UpdateNotifications(buildingID, problems, data.m_problems);
            }
        }

        private void SetContentFlags(ushort buildingID, ref Building data, TransferManager.TransferReason material)
        {
            switch (material)
            {
                case TransferManager.TransferReason.AnimalProducts:
                    data.m_flags = (data.m_flags & ~Building.Flags.Content01_Forbid) | Building.Flags.LevelUpEducation;
                    break;
                case TransferManager.TransferReason.Coal:
                    data.m_flags = (data.m_flags & ~Building.Flags.Content02_Forbid) | Building.Flags.LevelUpLandValue;
                    break;
                case TransferManager.TransferReason.Flours:
                    data.m_flags = (data.m_flags & ~Building.Flags.Content03_Forbid) | Building.Flags.Content03;
                    break;
                case TransferManager.TransferReason.Food:
                    data.m_flags = (data.m_flags & ~Building.Flags.Content04_Forbid) | Building.Flags.Loading1;
                    break;
                case TransferManager.TransferReason.Petroleum:
                    data.m_flags = (data.m_flags & ~Building.Flags.Content05_Forbid) | Building.Flags.Content05;
                    break;
                case TransferManager.TransferReason.Glass:
                    data.m_flags = (data.m_flags & ~Building.Flags.Content06_Forbid) | Building.Flags.Content06;
                    break;
                case TransferManager.TransferReason.Goods:
                    data.m_flags = (data.m_flags & ~Building.Flags.Loading2) | Building.Flags.Content07;
                    break;
                case TransferManager.TransferReason.Lumber:
                    data.m_flags = (data.m_flags & ~Building.Flags.Content07) | Building.Flags.Loading2;
                    break;
                case TransferManager.TransferReason.LuxuryProducts:
                    data.m_flags = (data.m_flags & ~Building.Flags.Content06) | Building.Flags.Content06_Forbid;
                    break;
                case TransferManager.TransferReason.Metals:
                    data.m_flags = (data.m_flags & ~Building.Flags.Content05) | Building.Flags.Content05_Forbid;
                    break;
                case TransferManager.TransferReason.Paper:
                    data.m_flags = (data.m_flags & ~Building.Flags.Loading1) | Building.Flags.Content04_Forbid;
                    break;
                case TransferManager.TransferReason.Petrol:
                    data.m_flags = (data.m_flags & ~Building.Flags.Content03) | Building.Flags.Content03_Forbid;
                    break;
                case TransferManager.TransferReason.Plastics:
                    data.m_flags = (data.m_flags & ~Building.Flags.LevelUpLandValue) | Building.Flags.Content02_Forbid;
                    break;
                case TransferManager.TransferReason.PlanedTimber:
                    data.m_flags = (data.m_flags & ~Building.Flags.LevelUpEducation) | Building.Flags.Content01_Forbid;
                    break;
                default:
                    data.m_flags &= ~Building.Flags.ContentMask;
                    break;
            }
        }

        private void SetExtendedContentFlags(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material)
        {
            switch (material)
            {
                case ExtendedTransferManager.TransferReason.Bread:
                case ExtendedTransferManager.TransferReason.FoodSupplies:
                case ExtendedTransferManager.TransferReason.DrinkSupplies:
                    data.m_flags = (data.m_flags & ~Building.Flags.Content06) | Building.Flags.Content06_Forbid;
                    break;
                default:
                    data.m_flags &= ~Building.Flags.ContentMask;
                    break;
            }
        }

        private void CancelIncomingTransfer(ushort buildingID, ref Building data, TransferManager.TransferReason material)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort num = data.m_guestVehicles;
            int num2 = 0;
            while (num != 0)
            {
                ushort nextGuestVehicle = instance.m_vehicles.m_buffer[num].m_nextGuestVehicle;
                if ((TransferManager.TransferReason)instance.m_vehicles.m_buffer[num].m_transferType == material && (instance.m_vehicles.m_buffer[num].m_flags & (Vehicle.Flags.TransferToTarget | Vehicle.Flags.GoingBack)) == Vehicle.Flags.TransferToTarget && instance.m_vehicles.m_buffer[num].m_targetBuilding == buildingID)
                {
                    VehicleInfo info = instance.m_vehicles.m_buffer[num].Info;
                    info.m_vehicleAI.SetTarget(num, ref instance.m_vehicles.m_buffer[num], 0);
                }
                num = nextGuestVehicle;
                if (++num2 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }

        private void CancelExtendedIncomingTransfer(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort num = data.m_guestVehicles;
            int num2 = 0;
            while (num != 0)
            {
                ushort nextGuestVehicle = instance.m_vehicles.m_buffer[num].m_nextGuestVehicle;
                if ((ExtendedTransferManager.TransferReason)instance.m_vehicles.m_buffer[num].m_transferType == material && (instance.m_vehicles.m_buffer[num].m_flags & (Vehicle.Flags.TransferToTarget | Vehicle.Flags.GoingBack)) == Vehicle.Flags.TransferToTarget && instance.m_vehicles.m_buffer[num].m_targetBuilding == buildingID)
                {
                    VehicleInfo info = instance.m_vehicles.m_buffer[num].Info;
                    info.m_vehicleAI.SetTarget(num, ref instance.m_vehicles.m_buffer[num], 0);
                }
                num = nextGuestVehicle;
                if (++num2 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }

        public override void PlacementSucceeded()
        {
            Singleton<BuildingManager>.instance.m_warehouseNeeded?.Deactivate();
        }

        public override void GetPollutionAccumulation(out int ground, out int noise)
        {
            ground = m_pollutionAccumulation;
            noise = m_noiseAccumulation;
        }

        public override bool IsFull(ushort buildingID, ref Building data)
        {
            int num = data.m_customBuffer1 * 100;
            return num >= m_storageCapacity;
        }

        public override bool CanBeRelocated(ushort buildingID, ref Building data)
        {
            int num = data.m_customBuffer1 * 100;
            return num == 0;
        }

        public override bool CanBeEmptied()
        {
            return true;
        }

        public override bool CanBeEmptied(ushort buildingID, ref Building data)
        {
            return true;
        }

        public override string GetLocalizedTooltip()
        {
            string text = LocaleFormatter.FormatGeneric("AIINFO_WATER_CONSUMPTION", GetWaterConsumption() * 16) + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_ELECTRICITY_CONSUMPTION", GetElectricityConsumption() * 16);
            string text2 = LocaleFormatter.FormatGeneric("AIINFO_CAPACITY", m_storageCapacity);
            text2 = text2 + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLE_COUNT", m_truckCount);
            string baseTooltip = TooltipHelper.Append(base.GetLocalizedTooltip(), TooltipHelper.Format(LocaleFormatter.Info1, text, LocaleFormatter.Info2, text2));
            string addTooltip = TooltipHelper.Format("arrowVisible", "false", "input1Visible", "true", "input2Visible", "false", "input3Visible", "false", "input4Visible", "false", "outputVisible", "false");
            string addTooltip2;
            if (m_extendedStorageType != ExtendedTransferManager.TransferReason.None)
            {
                addTooltip2 = TooltipHelper.Format("input1", m_extendedStorageType.ToString(), "input2", string.Empty, "input3", string.Empty, "input4", string.Empty, "output", string.Empty);
            }
            else
            {
                addTooltip2 = TooltipHelper.Format("input1", m_storageType.ToString(), "input2", string.Empty, "input3", string.Empty, "input4", string.Empty, "output", string.Empty);
            }
            baseTooltip = TooltipHelper.Append(baseTooltip, addTooltip);
            return TooltipHelper.Append(baseTooltip, addTooltip2);
        }

        public override string GetLocalizedStats(ushort buildingID, ref Building data)
        {
            string text = string.Empty;
            TransferManager.TransferReason actualTransferReason = GetActualTransferReason(buildingID, ref data);
            ExtendedTransferManager.TransferReason actualTransferReason2 = GetActualExtendedTransferReason(buildingID, ref data);
            if (actualTransferReason != TransferManager.TransferReason.None)
            {
                int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
                int productionRate = GetProductionRate(100, budget);
                int num = (productionRate * m_truckCount + 99) / 100;
                int count = 0;
                int cargo = 0;
                int capacity = 0;
                int outside = 0;
                CalculateOwnVehicles(buildingID, ref data, actualTransferReason, ref count, ref cargo, ref capacity, ref outside);
                int num2 = data.m_customBuffer1 * 100;
                int num3 = 0;
                if (num2 != 0)
                {
                    num3 = Mathf.Max(1, num2 * 100 / m_storageCapacity);
                }
                text = text + LocaleFormatter.FormatGeneric("AIINFO_FULL", num3) + Environment.NewLine;
                text += LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLES", count, num);
            }
            else if (actualTransferReason2 != ExtendedTransferManager.TransferReason.None)
            {
                int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
                int productionRate = GetProductionRate(100, budget);
                int num = (productionRate * m_truckCount + 99) / 100;
                int count = 0;
                int cargo = 0;
                int capacity = 0;
                int outside = 0;
                ExtedndedVehicleManager.CalculateOwnVehicles(buildingID, ref data, actualTransferReason2, ref count, ref cargo, ref capacity, ref outside);
                int num2 = data.m_customBuffer1 * 100;
                int num3 = 0;
                if (num2 != 0)
                {
                    num3 = Mathf.Max(1, num2 * 100 / m_storageCapacity);
                }
                text = text + LocaleFormatter.FormatGeneric("AIINFO_FULL", num3) + Environment.NewLine;
                text += LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_VEHICLES", count, num);
            }
            return text;
        }

        public TransferManager.TransferReason GetTransferReason(ushort buildingID, ref Building data)
        {
            if (m_storageType != TransferManager.TransferReason.None)
            {
                return m_storageType;
            }
            return (TransferManager.TransferReason)data.m_seniors;
        }

        public ExtendedTransferManager.TransferReason GetExtendedTransferReason(ushort buildingID, ref Building data)
        {
            if (m_extendedStorageType != ExtendedTransferManager.TransferReason.None)
            {
                return m_extendedStorageType;
            }
            return data.m_seniors != 255 ? (ExtendedTransferManager.TransferReason)(data.m_seniors - 200) : (ExtendedTransferManager.TransferReason)data.m_seniors;
        }

        public TransferManager.TransferReason GetActualTransferReason(ushort buildingID, ref Building data)
        {
            if (m_storageType != TransferManager.TransferReason.None)
            {
                return m_storageType;
            }
            return (TransferManager.TransferReason)data.m_adults;
        }

        public ExtendedTransferManager.TransferReason GetActualExtendedTransferReason(ushort buildingID, ref Building data)
        {
            if (m_extendedStorageType != ExtendedTransferManager.TransferReason.None)
            {
                return m_extendedStorageType;
            }
            return data.m_adults != 255 ? (ExtendedTransferManager.TransferReason)(data.m_adults - 200) : (ExtendedTransferManager.TransferReason)data.m_adults;
        }

        private int GetMaxLoadSize()
        {
            return 8000;
        }

        public override bool RequireRoadAccess()
        {
            return true;
        }
    }
}
