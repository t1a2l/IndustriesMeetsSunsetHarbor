using ColossalFramework;
using ColossalFramework.Math;
using System;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.AI
{
    class RestaurantDeliveryVehicleAI : CarAI
    {
        public int m_deliveryPersonCount = 1;

        [CustomizableProperty("Delivery capacity")]
        public int m_deliveryCapacity = 5;

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
                data.m_flags &= ~(Vehicle.Flags.Emergency2 | Vehicle.Flags.WaitingTarget);
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
                Randomizer randomizer = new Randomizer(vehicleID);
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
                data.m_flags &= ~Vehicle.Flags.Emergency2;
                if (data.m_transferSize > 0 && !ShouldReturnToSource(vehicleID, ref data))
                {
                    TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                    offer.Priority = 7;
                    offer.Vehicle = vehicleID;
                    if (data.m_sourceBuilding != 0)
                    {
                        offer.Position = data.GetLastFramePosition() * 0.25f + Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].m_position * 0.75f;
                    }
                    else
                    {
                        offer.Position = data.GetLastFramePosition();
                    }
                    offer.Amount = 1;
                    offer.Active = true;
                    Singleton<TransferManager>.instance.AddIncomingOffer((TransferManager.TransferReason)data.m_transferType, offer);
                    data.m_flags |= Vehicle.Flags.WaitingTarget;
                }
                else
                {
                    data.m_flags |= Vehicle.Flags.GoingBack;
                }
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

        public override void StartTransfer(ushort vehicleID, ref Vehicle data, TransferManager.TransferReason material, TransferManager.TransferOffer offer)
        {
            if (material == (TransferManager.TransferReason)data.m_transferType)
            {
                if ((data.m_flags & Vehicle.Flags.WaitingTarget) != 0)
                {
                    SetTarget(vehicleID, ref data, offer.Building);
                }
            }
            else
            {
                base.StartTransfer(vehicleID, ref data, material, offer);
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
            if ((vehicleData.m_flags & (Vehicle.Flags.TransferToSource | Vehicle.Flags.GoingBack)) == Vehicle.Flags.TransferToSource && ShouldReturnToSource(vehicleID, ref vehicleData))
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
                TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
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
                return true;
            }
            int amountDelta = Mathf.Min(0, data.m_transferSize - m_deliveryCapacity);
            BuildingManager instance = Singleton<BuildingManager>.instance;
            BuildingInfo info = instance.m_buildings.m_buffer[data.m_targetBuilding].Info;
            info.m_buildingAI.ModifyMaterialBuffer(data.m_targetBuilding, ref instance.m_buildings.m_buffer[data.m_targetBuilding], (TransferManager.TransferReason)data.m_transferType, ref amountDelta);
            for (int i = 0; i < m_deliveryPersonCount; i++)
            {
                CreateDeliveryGuy(vehicleID, ref data, Citizen.AgePhase.Senior0);
            }
            data.m_flags |= Vehicle.Flags.Stopped;
            SetTarget(vehicleID, ref data, 0);
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
            if ((object)groupCitizenInfo == null)
            {
                return;
            }
            int family = instance.m_randomizer.Int32(256u);
            uint citizen = 0u;
            if (instance2.CreateCitizen(out citizen, 90, family, ref instance.m_randomizer, groupCitizenInfo.m_gender))
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
                    Randomizer randomizer = new Randomizer(vehicleID);
                    info.m_buildingAI.CalculateUnspawnPosition(vehicleData.m_sourceBuilding, ref instance.m_buildings.m_buffer[leaderData.m_sourceBuilding], ref randomizer, m_info, out var _, out var target);
                    vehicleData.SetTargetPos(index++, CalculateTargetPoint(refPos, target, minSqrDistance, 2f));
                }
            }
            else if (leaderData.m_targetBuilding != 0)
            {
                BuildingManager instance2 = Singleton<BuildingManager>.instance;
                BuildingInfo info2 = instance2.m_buildings.m_buffer[leaderData.m_targetBuilding].Info;
                Randomizer randomizer2 = new Randomizer(vehicleID);
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
                    Randomizer randomizer = new Randomizer(vehicleID);
                    info.m_buildingAI.CalculateUnspawnPosition(vehicleData.m_sourceBuilding, ref instance.m_buildings.m_buffer[vehicleData.m_sourceBuilding], ref randomizer, m_info, out var _, out var target);
                    return StartPathFind(vehicleID, ref vehicleData, vehicleData.m_targetPos3, target);
                }
            }
            else if (vehicleData.m_targetBuilding != 0)
            {
                BuildingManager instance2 = Singleton<BuildingManager>.instance;
                BuildingInfo info2 = instance2.m_buildings.m_buffer[vehicleData.m_targetBuilding].Info;
                Randomizer randomizer2 = new Randomizer(vehicleID);
                info2.m_buildingAI.CalculateUnspawnPosition(vehicleData.m_targetBuilding, ref instance2.m_buildings.m_buffer[vehicleData.m_targetBuilding], ref randomizer2, m_info, out var _, out var target2);
                return StartPathFind(vehicleID, ref vehicleData, vehicleData.m_targetPos3, target2);
            }
            return false;
        }

        public override InstanceID GetTargetID(ushort vehicleID, ref Vehicle vehicleData)
        {
            InstanceID result = default(InstanceID);
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

    }
}
