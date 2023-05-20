using ColossalFramework;
using ColossalFramework.Math;
using IndustriesMeetsSunsetHarbor.Managers;
using System;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.AI
{
    class RestaurantDeliveryVehicleAI : CarAI
    {
        public int m_deliveryPersonCount = 1;

        [CustomizableProperty("Delivery Capacity")]
        public int m_deliveryCapacity = 8;

        [CustomizableProperty("Goods added to family")]
        public int m_goodsMeal = 100;

        public override Color GetColor(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode)
        {
            if (infoMode == InfoManager.InfoMode.Happiness)
            {
                return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
            }
            return base.GetColor(vehicleID, ref data, infoMode, subInfoMode);
        }

        public override string GetLocalizedStatus(ushort vehicleID, ref Vehicle data, out InstanceID target)
        {
            if ((data.m_flags & Vehicle.Flags.GoingBack) != 0)
            {
                target = InstanceID.Empty;
                return "Going back";
            }
            if ((data.m_flags & Vehicle.Flags.WaitingTarget) != 0)
            {
                target = InstanceID.Empty;
                return "Delivering food";
            }
            if (data.m_targetBuilding != 0)
            {
                target = InstanceID.Empty;
                target.Building = data.m_targetBuilding;
                return "Delivering food to";
            }
            target = InstanceID.Empty;
            return ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_CONFUSED");
        }

        public override void GetBufferStatus(ushort vehicleID, ref Vehicle data, out string localeKey, out int current, out int max)
        {
            localeKey = "Default";
            current = data.m_transferSize;
            max = m_deliveryCapacity;
        }

        public override void CreateVehicle(ushort vehicleID, ref Vehicle data)
        {
            base.CreateVehicle(vehicleID, ref data);
            data.m_flags |= Vehicle.Flags.WaitingTarget;
            Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, 0, vehicleID, 0, 0, 0, m_deliveryPersonCount, 0);
        }

        public override void ReleaseVehicle(ushort vehicleID, ref Vehicle data)
        {
            RemoveOffers(vehicleID, ref data);
            RemoveSource(vehicleID, ref data);
            RemoveTarget(vehicleID, ref data);
            base.ReleaseVehicle(vehicleID, ref data);
        }

        public override void SimulationStep(ushort vehicleID, ref Vehicle data, Vector3 physicsLodRefPos)
        {
            if ((data.m_flags & Vehicle.Flags.WaitingTarget) != 0 && ++data.m_waitCounter > 20)
            {
                RemoveOffers(vehicleID, ref data);
                data.m_flags &= ~Vehicle.Flags.WaitingTarget;
                data.m_flags |= Vehicle.Flags.GoingBack;
                data.m_waitCounter = 0;
                if (!StartPathFind(vehicleID, ref data))
                {
                    data.Unspawn(vehicleID);
                }
            }
            base.SimulationStep(vehicleID, ref data, physicsLodRefPos);
        }

        public override void LoadVehicle(ushort vehicleID, ref Vehicle data)
        {
            base.LoadVehicle(vehicleID, ref data);
            EnsureCitizenUnits(vehicleID, ref data, m_deliveryPersonCount);
            if (data.m_sourceBuilding != 0)
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].AddOwnVehicle(vehicleID, ref data);
            }
            if (data.m_targetBuilding != 0)
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].AddGuestVehicle(vehicleID, ref data);
            }
        }

        public override void SetSource(ushort vehicleID, ref Vehicle data, ushort sourceBuilding)
        {
            RemoveSource(vehicleID, ref data);
            data.m_sourceBuilding = sourceBuilding;
            if (sourceBuilding != 0)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                BuildingInfo info = instance.m_buildings.m_buffer[sourceBuilding].Info;
                data.Unspawn(vehicleID);
                Randomizer randomizer = new(vehicleID);
                info.m_buildingAI.CalculateSpawnPosition(sourceBuilding, ref instance.m_buildings.m_buffer[sourceBuilding], ref randomizer, m_info, out var position, out var target);
                Quaternion rotation = Quaternion.identity;
                Vector3 forward = target - position;
                if (forward.sqrMagnitude > 0.01f)
                {
                    rotation = Quaternion.LookRotation(forward);
                }
                data.m_frame0 = new Vehicle.Frame(position, rotation);
                data.m_frame1 = data.m_frame0;
                data.m_frame2 = data.m_frame0;
                data.m_frame3 = data.m_frame0;
                data.m_targetPos0 = position;
                data.m_targetPos0.w = 2f;
                data.m_targetPos1 = target;
                data.m_targetPos1.w = 2f;
                data.m_targetPos2 = data.m_targetPos1;
                data.m_targetPos3 = data.m_targetPos1;
                data.m_flags |= Vehicle.Flags.WaitingCargo;
                FrameDataUpdated(vehicleID, ref data, ref data.m_frame0);
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[sourceBuilding].AddOwnVehicle(vehicleID, ref data);
            }
        }

        public override void SetTarget(ushort vehicleID, ref Vehicle data, ushort targetBuilding)
        {
            RemoveTarget(vehicleID, ref data);
            data.m_targetBuilding = targetBuilding;
            data.m_flags &= ~Vehicle.Flags.WaitingTarget;
            data.m_waitCounter = 0;
            if (targetBuilding != 0)
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetBuilding].AddGuestVehicle(vehicleID, ref data);
            }
            else
            {
                data.m_flags |= Vehicle.Flags.GoingBack;
            }
            if (!StartPathFind(vehicleID, ref data))
            {
                data.Unspawn(vehicleID);
            }
        }

        public override void BuildingRelocated(ushort vehicleID, ref Vehicle data, ushort building)
        {
            base.BuildingRelocated(vehicleID, ref data, building);
            if (building == data.m_sourceBuilding)
            {
                if ((data.m_flags & Vehicle.Flags.GoingBack) != 0)
                {
                    InvalidPath(vehicleID, ref data, vehicleID, ref data);
                }
            }
            else if (building == data.m_targetBuilding && (data.m_flags & Vehicle.Flags.GoingBack) == 0)
            {
                InvalidPath(vehicleID, ref data, vehicleID, ref data);
            }
        }

        public override void GetSize(ushort vehicleID, ref Vehicle data, out int size, out int max)
        {
            size = data.m_transferSize;
            max = m_deliveryCapacity;
        }

        public override void SimulationStep(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            base.SimulationStep(vehicleID, ref vehicleData, ref frameData, leaderID, ref leaderData, lodPhysics);
            if ((vehicleData.m_flags & Vehicle.Flags.Stopped) != 0 && CanLeave(vehicleID, ref vehicleData))
            {
                vehicleData.m_flags &= ~Vehicle.Flags.Stopped;
                vehicleData.m_flags |= Vehicle.Flags.Leaving;
            }
            if ((vehicleData.m_flags & Vehicle.Flags.GoingBack) == 0 && ShouldReturnToSource(vehicleID, ref vehicleData))
            {
                SetTarget(vehicleID, ref vehicleData, 0);
            }
        }

        private bool ShouldReturnToSource(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_sourceBuilding != 0)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                if ((instance.m_buildings.m_buffer[data.m_sourceBuilding].m_flags & Building.Flags.Active) == 0 && instance.m_buildings.m_buffer[data.m_sourceBuilding].m_fireIntensity == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private void RemoveOffers(ushort vehicleID, ref Vehicle data)
        {
            if ((data.m_flags & Vehicle.Flags.WaitingTarget) != 0)
            {
                TransferManager.TransferOffer offer = default;
                offer.Vehicle = vehicleID;
                Singleton<TransferManager>.instance.RemoveIncomingOffer((TransferManager.TransferReason)data.m_transferType, offer);
            }
        }

        private void RemoveSource(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_sourceBuilding != 0)
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].RemoveOwnVehicle(vehicleID, ref data);
                data.m_sourceBuilding = 0;
            }
        }

        private void RemoveTarget(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_targetBuilding != 0)
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].RemoveGuestVehicle(vehicleID, ref data);
                data.m_targetBuilding = 0;
            }
        }

        private bool ArriveAtTarget(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_targetBuilding == 0)
            {
                Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleID);
                return true;
            }
            var buildingId = data.m_targetBuilding;
            var deliveryData = RestaurantDeliveriesManager.RestaurantDeliveries.Find(item => item.deliveryVehicleId == vehicleID && item.buildingId == buildingId);
            BuildingManager b_instance = Singleton<BuildingManager>.instance;
            CitizenManager c_instance = Singleton<CitizenManager>.instance;
            var target_building = b_instance.m_buildings.m_buffer[buildingId];
            uint building_citizen_units = target_building.m_citizenUnits;
            int num2 = 0;
            while (building_citizen_units != 0U)
            {
                uint nextUnit = c_instance.m_units.m_buffer[building_citizen_units].m_nextUnit;
                for (int i = 0; i < 5; i++)
                {
                    var citizen_unit = c_instance.m_units.m_buffer[building_citizen_units];
                    uint citizenId = citizen_unit.GetCitizen(i);
                    var citizen = c_instance.m_citizens.m_buffer[citizenId];
                    if (citizenId != 0U && citizenId == deliveryData.citizenId && citizen.CurrentLocation != Citizen.Location.Moving)
                    {
                        citizen_unit.m_goods += (ushort)m_goodsMeal;
                        citizen.m_flags &= ~Citizen.Flags.NeedGoods;
                        data.m_transferSize -= 1;
                    }
                }
                building_citizen_units = nextUnit;
                if (++num2 > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            if (!IsCitizenWaitingForDelivery(data.m_targetBuilding))
            {
                var home_building = b_instance.m_buildings.m_buffer[(int)data.m_targetBuilding];
                home_building.m_flags &= ~Building.Flags.Incoming;
            }
            for (int j = 0; j < m_deliveryPersonCount; j++)
            {
                CreateDeliveryGuy(vehicleID, ref data, Citizen.AgePhase.Adult0);
            }
            data.m_flags |= Vehicle.Flags.Stopped;
            // check if there are other deliveries for this vehicle
            // and go to the next delivery or go back and remove data of current delivery
            RestaurantDeliveriesManager.RestaurantDeliveries.Remove(deliveryData);
            var newData = RestaurantDeliveriesManager.RestaurantDeliveries.Find(item => item.deliveryVehicleId == vehicleID);
            if(newData.citizenId != 0)
            {
                SetTarget(vehicleID, ref data, newData.buildingId);
            }
            else
            {
                SetTarget(vehicleID, ref data, 0);
            }
            return false;
        }

        public override bool CanLeave(ushort vehicleID, ref Vehicle vehicleData)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            bool flag = true;
            bool flag2 = false;
            uint num = vehicleData.m_citizenUnits;
            int num2 = 0;
            while (num != 0)
            {
                uint nextUnit = instance.m_units.m_buffer[num].m_nextUnit;
                for (int i = 0; i < 5; i++)
                {
                    uint citizen = instance.m_units.m_buffer[num].GetCitizen(i);
                    if (citizen == 0)
                    {
                        continue;
                    }
                    ushort instance2 = instance.m_citizens.m_buffer[citizen].m_instance;
                    if (instance2 == 0)
                    {
                        continue;
                    }
                    CitizenInfo info = instance.m_instances.m_buffer[instance2].Info;
                    if (info.m_class.m_service == m_info.m_class.m_service)
                    {
                        if ((instance.m_instances.m_buffer[instance2].m_flags & CitizenInstance.Flags.EnteringVehicle) == 0 && (instance.m_instances.m_buffer[instance2].m_flags & CitizenInstance.Flags.Character) != 0)
                        {
                            flag2 = true;
                        }
                        flag = false;
                    }
                }
                num = nextUnit;
                if (++num2 > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            if (!flag2 && !flag)
            {
                num = vehicleData.m_citizenUnits;
                num2 = 0;
                while (num != 0)
                {
                    uint nextUnit2 = instance.m_units.m_buffer[num].m_nextUnit;
                    for (int j = 0; j < 5; j++)
                    {
                        uint citizen2 = instance.m_units.m_buffer[num].GetCitizen(j);
                        if (citizen2 != 0)
                        {
                            ushort instance3 = instance.m_citizens.m_buffer[citizen2].m_instance;
                            if (instance3 != 0 && (instance.m_instances.m_buffer[instance3].m_flags & CitizenInstance.Flags.EnteringVehicle) == 0)
                            {
                                CitizenInfo info2 = instance.m_instances.m_buffer[instance3].Info;
                                info2.m_citizenAI.SetTarget(instance3, ref instance.m_instances.m_buffer[instance3], 0);
                            }
                        }
                    }
                    num = nextUnit2;
                    if (++num2 > 524288)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        break;
                    }
                }
            }
            return flag;
        }

        private void CreateDeliveryGuy(ushort vehicleID, ref Vehicle data, Citizen.AgePhase agePhase)
        {
            SimulationManager instance = Singleton<SimulationManager>.instance;
            CitizenManager instance2 = Singleton<CitizenManager>.instance;
            CitizenInfo groupCitizenInfo = instance2.GetGroupCitizenInfo(ref instance.m_randomizer, m_info.m_class.m_service, Citizen.Gender.Male, Citizen.SubCulture.Generic, agePhase);
            if (groupCitizenInfo == null)
            {
                return;
            }
            int family = instance.m_randomizer.Int32(256u);
            if (instance2.CreateCitizen(out uint citizen, 90, family, ref instance.m_randomizer, groupCitizenInfo.m_gender))
            {
                if (instance2.CreateCitizenInstance(out var instance3, ref instance.m_randomizer, groupCitizenInfo, citizen))
                {
                    Vector3 randomDoorPosition = data.GetRandomDoorPosition(ref instance.m_randomizer, VehicleInfo.DoorType.Exit);
                    groupCitizenInfo.m_citizenAI.SetCurrentVehicle(instance3, ref instance2.m_instances.m_buffer[instance3], 0, 0u, randomDoorPosition);
                    groupCitizenInfo.m_citizenAI.SetTarget(instance3, ref instance2.m_instances.m_buffer[instance3], data.m_targetBuilding);
                    instance2.m_citizens.m_buffer[citizen].SetVehicle(citizen, vehicleID, 0u);
                }
                else
                {
                    instance2.ReleaseCitizen(citizen);
                }
            }
        }

        private bool ArriveAtSource(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_sourceBuilding == 0)
            {
                Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleID);
                return true;
            }
            int amountDelta = data.m_transferSize;
            BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].Info;
            info.m_buildingAI.ModifyMaterialBuffer(data.m_sourceBuilding, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding], (TransferManager.TransferReason)data.m_transferType, ref amountDelta);
            data.m_transferSize = (ushort)Mathf.Clamp(data.m_transferSize - amountDelta, 0, data.m_transferSize);
            RemoveSource(vehicleID, ref data);
            return true;
        }

        public override bool ArriveAtDestination(ushort vehicleID, ref Vehicle vehicleData)
        {
            if ((vehicleData.m_flags & Vehicle.Flags.WaitingTarget) != 0)
            {
                return false;
            }
            if ((vehicleData.m_flags & Vehicle.Flags.GoingBack) != 0)
            {
                return ArriveAtSource(vehicleID, ref vehicleData);
            }
            return ArriveAtTarget(vehicleID, ref vehicleData);
        }

        public override void UpdateBuildingTargetPositions(ushort vehicleID, ref Vehicle vehicleData, Vector3 refPos, ushort leaderID, ref Vehicle leaderData, ref int index, float minSqrDistance)
        {
            if ((leaderData.m_flags & Vehicle.Flags.WaitingTarget) != 0)
            {
                return;
            }
            if ((leaderData.m_flags & Vehicle.Flags.GoingBack) != 0)
            {
                if (leaderData.m_sourceBuilding != 0)
                {
                    BuildingManager instance = Singleton<BuildingManager>.instance;
                    BuildingInfo info = instance.m_buildings.m_buffer[leaderData.m_sourceBuilding].Info;
                    Randomizer randomizer = new(vehicleID);
                    info.m_buildingAI.CalculateUnspawnPosition(vehicleData.m_sourceBuilding, ref instance.m_buildings.m_buffer[leaderData.m_sourceBuilding], ref randomizer, m_info, out var _, out var target);
                    vehicleData.SetTargetPos(index++, CalculateTargetPoint(refPos, target, minSqrDistance, 2f));
                }
            }
            else if (leaderData.m_targetBuilding != 0)
            {
                BuildingManager instance2 = Singleton<BuildingManager>.instance;
                BuildingInfo info2 = instance2.m_buildings.m_buffer[leaderData.m_targetBuilding].Info;
                Randomizer randomizer2 = new(vehicleID);
                info2.m_buildingAI.CalculateUnspawnPosition(vehicleData.m_targetBuilding, ref instance2.m_buildings.m_buffer[leaderData.m_targetBuilding], ref randomizer2, m_info, out var _, out var target2);
                vehicleData.SetTargetPos(index++, CalculateTargetPoint(refPos, target2, minSqrDistance, 2f));
            }
        }

        protected override bool StartPathFind(ushort vehicleID, ref Vehicle vehicleData)
        {
            if ((vehicleData.m_flags & Vehicle.Flags.WaitingTarget) != 0)
            {
                return true;
            }
            if ((vehicleData.m_flags & Vehicle.Flags.GoingBack) != 0)
            {
                if (vehicleData.m_sourceBuilding != 0)
                {
                    BuildingManager instance = Singleton<BuildingManager>.instance;
                    BuildingInfo info = instance.m_buildings.m_buffer[vehicleData.m_sourceBuilding].Info;
                    Randomizer randomizer = new(vehicleID);
                    info.m_buildingAI.CalculateUnspawnPosition(vehicleData.m_sourceBuilding, ref instance.m_buildings.m_buffer[vehicleData.m_sourceBuilding], ref randomizer, m_info, out var _, out var target);
                    return StartPathFind(vehicleID, ref vehicleData, vehicleData.m_targetPos3, target);
                }
            }
            else if (vehicleData.m_targetBuilding != 0)
            {
                BuildingManager instance2 = Singleton<BuildingManager>.instance;
                BuildingInfo info2 = instance2.m_buildings.m_buffer[vehicleData.m_targetBuilding].Info;
                Randomizer randomizer2 = new(vehicleID);
                info2.m_buildingAI.CalculateUnspawnPosition(vehicleData.m_targetBuilding, ref instance2.m_buildings.m_buffer[vehicleData.m_targetBuilding], ref randomizer2, m_info, out var _, out var target2);
                return StartPathFind(vehicleID, ref vehicleData, vehicleData.m_targetPos3, target2);
            }
            return false;
        }

        public override InstanceID GetTargetID(ushort vehicleID, ref Vehicle vehicleData)
        {
            InstanceID result = default;
            if ((vehicleData.m_flags & Vehicle.Flags.GoingBack) != 0)
            {
                result.Building = vehicleData.m_sourceBuilding;
            }
            else
            {
                result.Building = vehicleData.m_targetBuilding;
            }
            return result;
        }

        public bool IsCitizenWaitingForDelivery(ushort target_building)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            BuildingManager b_instance = Singleton<BuildingManager>.instance;
            var home_building = b_instance.m_buildings.m_buffer[target_building];
            uint num = home_building.m_citizenUnits;
            int num2 = 0;
            while (num != 0U)
            {
                uint nextUnit = instance.m_units.m_buffer[(int)(UIntPtr)num].m_nextUnit;
                var citizen_unit = instance.m_units.m_buffer[(int)((UIntPtr)num)];
                for (int i = 0; i < 5; i++)
                {
                    uint citizenId = citizen_unit.GetCitizen(i);
                    if (citizenId == 0)
                    {
                        continue;
                    }
                    var waiting_delivery = RestaurantDeliveriesManager.RestaurantDeliveries.FindIndex(item => item.citizenId == citizenId);
                    if (waiting_delivery != -1)
                    {
                        return true;
                    }
                }
                num = nextUnit;
                if (++num2 > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return false;
        }

    }
}
