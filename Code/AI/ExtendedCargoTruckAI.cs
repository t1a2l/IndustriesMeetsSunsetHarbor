using ColossalFramework;
using ColossalFramework.Math;
using IndustriesMeetsSunsetHarbor.Managers;
using MoreTransferReasons;
using System;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.AI
{
    class ExtendedCargoTruckAI : CarAI, IExtendedVehicleAI
    {
        [CustomizableProperty("Cargo Capacity")]
        public int m_cargoCapacity = 1;

        public bool m_isHeavyVehicle;

        public override VehicleInfo.VehicleCategory vehicleCategory => VehicleInfo.VehicleCategory.CargoTruck;

        public override Color GetColor(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode)
        {
            switch (infoMode)
            {
                case InfoManager.InfoMode.Connections:
                {
                    var transferType = data.m_transferType;
                    if (data.m_transferType >= 200)
                    {
                        transferType = (byte)(data.m_transferType - 200);
                    }
                    if (subInfoMode == InfoManager.SubInfoMode.Default && (data.m_flags & Vehicle.Flags.Importing) != 0 && data.m_transferType != 255)
                    {
                        return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)transferType];
                    }
                    if (subInfoMode == InfoManager.SubInfoMode.WaterPower && (data.m_flags & Vehicle.Flags.Exporting) != 0 && data.m_transferType != 255)
                    {
                        return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)transferType];
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                }
                case InfoManager.InfoMode.TrafficRoutes:
                    if (subInfoMode == InfoManager.SubInfoMode.Default)
                    {
                        InstanceID empty = InstanceID.Empty;
                        empty.Vehicle = vehicleID;
                        if (Singleton<NetManager>.instance.PathVisualizer.IsPathVisible(empty))
                        {
                            return Singleton<InfoManager>.instance.m_properties.m_routeColors[4];
                        }
                        return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                    }
                    return base.GetColor(vehicleID, ref data, infoMode, subInfoMode);
                case InfoManager.InfoMode.Industry:
                {
                    ushort sourceBuilding = data.m_sourceBuilding;
                    if (sourceBuilding != 0)
                    {
                        BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[sourceBuilding].Info;
                        if ((object)info != null && IndustryBuildingAI.ServiceToInfoMode(info.m_class.m_subService) == subInfoMode)
                        {
                            return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
                        }
                    }
                    ushort targetBuilding = data.m_targetBuilding;
                    if (targetBuilding != 0)
                    {
                        BuildingInfo info2 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetBuilding].Info;
                        if ((object)info2 != null && IndustryBuildingAI.ServiceToInfoMode(info2.m_class.m_subService) == subInfoMode)
                        {
                            return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
                        }
                    }
                    var transferType = data.m_transferType;
                    if (data.m_transferType >= 200)
                    {
                        transferType = (byte)(data.m_transferType - 200);
                    }
                    if (IndustryBuildingManager.ResourceToInfoMode(transferType) == subInfoMode)
                    {
                        return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                }
                case InfoManager.InfoMode.Fishing:
                    if (data.m_transferType == 108)
                    {
                        return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                default:
                    return base.GetColor(vehicleID, ref data, infoMode, subInfoMode);
            }
        }

        public override string GetLocalizedStatus(ushort vehicleID, ref Vehicle data, out InstanceID target)
        {
            if ((data.m_flags & Vehicle.Flags.TransferToTarget) != 0)
            {
                ushort targetBuilding = data.m_targetBuilding;
                if ((data.m_flags & Vehicle.Flags.GoingBack) != 0)
                {
                    target = InstanceID.Empty;
                    return ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_CARGOTRUCK_RETURN");
                }
                if ((data.m_flags & Vehicle.Flags.WaitingTarget) != 0)
                {
                    target = InstanceID.Empty;
                    return ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_CARGOTRUCK_UNLOAD");
                }
                if (targetBuilding != 0)
                {
                    Building.Flags flags = Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetBuilding].m_flags;
                    if (data.m_transferType >= 200)
                    {
                        byte transferType = (byte)(data.m_transferType - 200);
                        ExtendedTransferManager.TransferReason transferReason = (ExtendedTransferManager.TransferReason)transferType;
                        if ((data.m_flags & Vehicle.Flags.Exporting) != 0 || (flags & Building.Flags.IncomingOutgoing) != 0)
                        {
                            target = InstanceID.Empty;
                            return "Exporting " +  transferReason.ToString();
                        }
                        if ((data.m_flags & Vehicle.Flags.Importing) != 0)
                        {
                            target = InstanceID.Empty;
                            target.Building = targetBuilding;
                            return "Importing " +  transferReason.ToString() + " to";
                        }
                        target = InstanceID.Empty;
                        target.Building = targetBuilding;
                        return "Delivering " +  transferReason.ToString() + " to";
                    }
                    else
                    {
                        byte transferType = data.m_transferType;
                        TransferManager.TransferReason transferReason = (TransferManager.TransferReason)transferType;
                        if ((data.m_flags & Vehicle.Flags.Exporting) != 0 || (flags & Building.Flags.IncomingOutgoing) != 0)
                        {
                            target = InstanceID.Empty;
                            return ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_CARGOTRUCK_EXPORT", transferReason.ToString());
                        }
                        if ((data.m_flags & Vehicle.Flags.Importing) != 0)
                        {
                            target = InstanceID.Empty;
                            target.Building = targetBuilding;
                            return ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_CARGOTRUCK_IMPORT", transferReason.ToString());
                        }
                        target = InstanceID.Empty;
                        target.Building = targetBuilding;
                        return ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_CARGOTRUCK_DELIVER", transferReason.ToString());
                    }
                }
            }
            target = InstanceID.Empty;
            return ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_CONFUSED");
        }

        public override void GetBufferStatus(ushort vehicleID, ref Vehicle data, out string localeKey, out int current, out int max)
        {
            localeKey = "Default";
            current = data.m_transferSize;
            max = m_cargoCapacity;
        }

        public override void CreateVehicle(ushort vehicleID, ref Vehicle data)
        {
            base.CreateVehicle(vehicleID, ref data);
            data.m_flags |= Vehicle.Flags.WaitingTarget;
        }

        public override void ReleaseVehicle(ushort vehicleID, ref Vehicle data)
        {
            if ((data.m_flags & Vehicle.Flags.TransferToTarget) != 0 && data.m_sourceBuilding != 0 && data.m_transferSize != 0)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                BuildingInfo info = instance.m_buildings.m_buffer[data.m_sourceBuilding].Info;
                if (info != null)
                {
                    int amountDelta = data.m_transferSize;
                    if (data.m_transferType >= 200)
                    {
                        byte transferType = (byte)(data.m_transferType - 200);
                        ((IExtendedBuildingAI)info.m_buildingAI).ExtendedModifyMaterialBuffer(data.m_sourceBuilding, ref instance.m_buildings.m_buffer[data.m_sourceBuilding], (ExtendedTransferManager.TransferReason)transferType, ref amountDelta);
                    }
                    else
                    {
                        info.m_buildingAI.ModifyMaterialBuffer(data.m_sourceBuilding, ref instance.m_buildings.m_buffer[data.m_sourceBuilding], (TransferManager.TransferReason)data.m_transferType, ref amountDelta);
                    }

                }
            }
            RemoveOffers(vehicleID, ref data);
            RemoveSource(vehicleID, ref data);
            RemoveTarget(vehicleID, ref data);
            base.ReleaseVehicle(vehicleID, ref data);
        }

        public override void SimulationStep(ushort vehicleID, ref Vehicle data, Vector3 physicsLodRefPos)
        {
            if ((data.m_flags & Vehicle.Flags.Congestion) != 0)
            {
                Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleID);
                return;
            }
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

        protected override void PathfindSuccess(ushort vehicleID, ref Vehicle data)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            if (data.m_sourceBuilding != 0)
            {
                BuildingAI.PathFindType type = (((data.m_flags & Vehicle.Flags.DummyTraffic) == 0) ? BuildingAI.PathFindType.LeavingCargo : BuildingAI.PathFindType.LeavingDummy);
                BuildingInfo info = instance.m_buildings.m_buffer[data.m_sourceBuilding].Info;
                info.m_buildingAI.PathfindSuccess(data.m_sourceBuilding, ref instance.m_buildings.m_buffer[data.m_sourceBuilding], type);
            }
            if (data.m_targetBuilding != 0)
            {
                BuildingAI.PathFindType type2 = (((data.m_flags & Vehicle.Flags.DummyTraffic) != 0) ? BuildingAI.PathFindType.EnteringDummy : BuildingAI.PathFindType.EnteringCargo);
                BuildingInfo info2 = instance.m_buildings.m_buffer[data.m_targetBuilding].Info;
                info2.m_buildingAI.PathfindSuccess(data.m_targetBuilding, ref instance.m_buildings.m_buffer[data.m_targetBuilding], type2);
            }
            base.PathfindSuccess(vehicleID, ref data);
        }

        protected override void PathfindFailure(ushort vehicleID, ref Vehicle data)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            if (data.m_sourceBuilding != 0)
            {
                BuildingAI.PathFindType type = (((data.m_flags & Vehicle.Flags.DummyTraffic) == 0) ? BuildingAI.PathFindType.LeavingCargo : BuildingAI.PathFindType.LeavingDummy);
                BuildingInfo info = instance.m_buildings.m_buffer[data.m_sourceBuilding].Info;
                info.m_buildingAI.PathfindFailure(data.m_sourceBuilding, ref instance.m_buildings.m_buffer[data.m_sourceBuilding], type);
            }
            if (data.m_targetBuilding != 0)
            {
                BuildingAI.PathFindType type2 = (((data.m_flags & Vehicle.Flags.DummyTraffic) != 0) ? BuildingAI.PathFindType.EnteringDummy : BuildingAI.PathFindType.EnteringCargo);
                BuildingInfo info2 = instance.m_buildings.m_buffer[data.m_targetBuilding].Info;
                info2.m_buildingAI.PathfindFailure(data.m_targetBuilding, ref instance.m_buildings.m_buffer[data.m_targetBuilding], type2);
            }
            base.PathfindFailure(vehicleID, ref data);
        }

        public override void LoadVehicle(ushort vehicleID, ref Vehicle data)
        {
            base.LoadVehicle(vehicleID, ref data);
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
            if (sourceBuilding == 0)
            {
                return;
            }
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
            if ((data.m_flags & Vehicle.Flags.TransferToTarget) != 0)
            {
                int amountDelta = Mathf.Min(0, data.m_transferSize - m_cargoCapacity);
                BuildingInfo info2 = instance.m_buildings.m_buffer[data.m_sourceBuilding].Info;
                if (data.m_transferType >= 200)
                {
                    byte transferType = (byte)(data.m_transferType - 200);
                    ((IExtendedBuildingAI)info2.m_buildingAI).ExtendedModifyMaterialBuffer(data.m_sourceBuilding, ref instance.m_buildings.m_buffer[data.m_sourceBuilding], (ExtendedTransferManager.TransferReason)transferType, ref amountDelta);
                }
                else
                {
                    info2.m_buildingAI.ModifyMaterialBuffer(data.m_sourceBuilding, ref instance.m_buildings.m_buffer[data.m_sourceBuilding], (TransferManager.TransferReason)data.m_transferType, ref amountDelta);
                }
                amountDelta = Mathf.Max(0, -amountDelta);
                data.m_transferSize += (ushort)amountDelta;
            }
            FrameDataUpdated(vehicleID, ref data, ref data.m_frame0);
            instance.m_buildings.m_buffer[data.m_sourceBuilding].AddOwnVehicle(vehicleID, ref data);
            if ((instance.m_buildings.m_buffer[data.m_sourceBuilding].m_flags & Building.Flags.IncomingOutgoing) != 0)
            {
                if ((data.m_flags & Vehicle.Flags.TransferToTarget) != 0)
                {
                    data.m_flags |= Vehicle.Flags.Importing;
                }
                else if ((data.m_flags & Vehicle.Flags.TransferToSource) != 0)
                {
                    data.m_flags |= Vehicle.Flags.Exporting;
                }
            }
        }

        public override void SetTarget(ushort vehicleID, ref Vehicle data, ushort targetBuilding)
        {
            if (targetBuilding == data.m_targetBuilding)
            {
                if (data.m_path == 0)
                {
                    if (!StartPathFind(vehicleID, ref data))
                    {
                        data.Unspawn(vehicleID);
                    }
                }
                else
                {
                    TrySpawn(vehicleID, ref data);
                }
                return;
            }
            RemoveTarget(vehicleID, ref data);
            data.m_targetBuilding = targetBuilding;
            data.m_flags &= ~Vehicle.Flags.WaitingTarget;
            data.m_waitCounter = 0;
            if (data.m_targetBuilding != 0)
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].AddGuestVehicle(vehicleID, ref data);
                if ((Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].m_flags & Building.Flags.IncomingOutgoing) != 0)
                {
                    if ((data.m_flags & Vehicle.Flags.TransferToTarget) != 0)
                    {
                        data.m_flags |= Vehicle.Flags.Exporting;
                    }
                    else if ((data.m_flags & Vehicle.Flags.TransferToSource) != 0)
                    {
                        data.m_flags |= Vehicle.Flags.Importing;
                    }
                }
            }
            else
            {
                if ((data.m_flags & Vehicle.Flags.TransferToTarget) != 0)
                {
                    if (data.m_transferType >= 200)
                    {
                        byte transferType = (byte)(data.m_transferType - 200);
                        if (data.m_transferSize > 0)
                        {
                            ExtendedTransferManager.Offer offer = default;
                            offer.Vehicle = vehicleID;
                            if (data.m_sourceBuilding != 0)
                            {
                                offer.Position = (data.GetLastFramePosition() + Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].m_position) * 0.5f;
                            }
                            else
                            {
                                offer.Position = data.GetLastFramePosition();
                            }
                            offer.Amount = 1;
                            offer.Active = true;
                            Singleton<ExtendedTransferManager>.instance.AddOutgoingOffer((ExtendedTransferManager.TransferReason)transferType, offer);
                            data.m_flags |= Vehicle.Flags.WaitingTarget;
                        }
                        else
                        {
                            data.m_flags |= Vehicle.Flags.GoingBack;
                        }
                    }
                    else
                    {
                        if (data.m_transferSize > 0)
                        {

                            TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                            offer.Priority = 7;
                            offer.Vehicle = vehicleID;
                            if (data.m_sourceBuilding != 0)
                            {
                                offer.Position = (data.GetLastFramePosition() + Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].m_position) * 0.5f;
                            }
                            else
                            {
                                offer.Position = data.GetLastFramePosition();
                            }
                            offer.Amount = 1;
                            offer.Active = true;
                            Singleton<TransferManager>.instance.AddOutgoingOffer((TransferManager.TransferReason)data.m_transferType, offer);
                            data.m_flags |= Vehicle.Flags.WaitingTarget;
                        }
                        else
                        {
                            data.m_flags |= Vehicle.Flags.GoingBack;
                        }
                    }
                }
                if ((data.m_flags & Vehicle.Flags.TransferToSource) != 0)
                {
                    if (data.m_transferType >= 200)
                    {
                        byte transferType = (byte)(data.m_transferType - 200);
                        if (data.m_transferSize < m_cargoCapacity)
                        {
                            ExtendedTransferManager.Offer offer2 = default;
                            offer2.Vehicle = vehicleID;
                            if (data.m_sourceBuilding != 0)
                            {
                                offer2.Position = (data.GetLastFramePosition() + Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].m_position) * 0.5f;
                            }
                            else
                            {
                                offer2.Position = data.GetLastFramePosition();
                            }
                            offer2.Amount = 1;
                            offer2.Active = true;
                            Singleton<ExtendedTransferManager>.instance.AddIncomingOffer((ExtendedTransferManager.TransferReason)transferType, offer2);
                            data.m_flags |= Vehicle.Flags.WaitingTarget;
                        }
                        else
                        {
                            data.m_flags |= Vehicle.Flags.GoingBack;
                        }
                    }
                    else
                    {
                        if (data.m_transferSize < m_cargoCapacity)
                        {
                            TransferManager.TransferOffer offer2 = default(TransferManager.TransferOffer);
                            offer2.Priority = 7;
                            offer2.Vehicle = vehicleID;
                            if (data.m_sourceBuilding != 0)
                            {
                                offer2.Position = (data.GetLastFramePosition() + Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].m_position) * 0.5f;
                            }
                            else
                            {
                                offer2.Position = data.GetLastFramePosition();
                            }
                            offer2.Amount = 1;
                            offer2.Active = true;
                            Singleton<TransferManager>.instance.AddIncomingOffer((TransferManager.TransferReason)data.m_transferType, offer2);
                            data.m_flags |= Vehicle.Flags.WaitingTarget;
                        }
                        else
                        {
                            data.m_flags |= Vehicle.Flags.GoingBack;
                        }
                    }
                }
            }
            if (data.m_cargoParent != 0)
            {
                if (data.m_path != 0)
                {
                    if (data.m_path != 0)
                    {
                        Singleton<PathManager>.instance.ReleasePath(data.m_path);
                    }
                    data.m_path = 0u;
                }
            }
            else if (!StartPathFind(vehicleID, ref data))
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

        void IExtendedVehicleAI.ExtendedStartTransfer(ushort vehicleID, ref Vehicle data, ExtendedTransferManager.TransferReason material, ExtendedTransferManager.Offer offer)
        {
            var transferType = data.m_transferType;
            if (data.m_transferType >= 200)
            {
                transferType = (byte)(data.m_transferType - 200);
            }
            if (material == (ExtendedTransferManager.TransferReason)transferType)
            {
                if ((data.m_flags & Vehicle.Flags.WaitingTarget) != 0)
                {
                    SetTarget(vehicleID, ref data, offer.Building);
                }
            }
        }

        public override void GetSize(ushort vehicleID, ref Vehicle data, out int size, out int max)
        {
            size = data.m_transferSize;
            max = m_cargoCapacity;
        }

        private void RemoveOffers(ushort vehicleID, ref Vehicle data)
        {
            if ((data.m_flags & Vehicle.Flags.WaitingTarget) != 0)
            {
                TransferManager.TransferOffer offer = default;
                offer.Vehicle = vehicleID;
                ExtendedTransferManager.Offer extended_offer = default;
                extended_offer.Vehicle = vehicleID;
                if ((data.m_flags & Vehicle.Flags.TransferToSource) != 0)
                {
                    if (data.m_transferType >= 200)
                    {
                        byte transferType = (byte)(data.m_transferType - 200);
                        Singleton<ExtendedTransferManager>.instance.RemoveIncomingOffer((ExtendedTransferManager.TransferReason)transferType, extended_offer);
                    }
                    else
                    {
                        Singleton<TransferManager>.instance.RemoveIncomingOffer((TransferManager.TransferReason)data.m_transferType, offer);
                    }
                }
                else if ((data.m_flags & Vehicle.Flags.TransferToTarget) != 0)
                {
                    if (data.m_transferType >= 200)
                    {
                        byte transferType = (byte)(data.m_transferType - 200);
                        Singleton<ExtendedTransferManager>.instance.RemoveOutgoingOffer((ExtendedTransferManager.TransferReason)transferType, extended_offer);
                    }
                    else
                    {
                        Singleton<TransferManager>.instance.RemoveOutgoingOffer((TransferManager.TransferReason)data.m_transferType, offer);
                    }
                }
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
            int amountDelta = 0;
            if ((data.m_flags & Vehicle.Flags.TransferToTarget) != 0)
            {
                amountDelta = data.m_transferSize;
            }
            if ((data.m_flags & Vehicle.Flags.TransferToSource) != 0)
            {
                amountDelta = Mathf.Min(0, data.m_transferSize - m_cargoCapacity);
            }
            BuildingManager instance = Singleton<BuildingManager>.instance;
            BuildingInfo info = instance.m_buildings.m_buffer[data.m_targetBuilding].Info;
            if (data.m_transferType >= 200 && info.m_buildingAI is not OutsideConnectionAI)
            {
                byte transferType = (byte)(data.m_transferType - 200);
                ((IExtendedBuildingAI)info.m_buildingAI).ExtendedModifyMaterialBuffer(data.m_targetBuilding, ref instance.m_buildings.m_buffer[data.m_targetBuilding], (ExtendedTransferManager.TransferReason)transferType, ref amountDelta);
            }
            else
            {
                info.m_buildingAI.ModifyMaterialBuffer(data.m_targetBuilding, ref instance.m_buildings.m_buffer[data.m_targetBuilding], (TransferManager.TransferReason)data.m_transferType, ref amountDelta);
            }
            if ((data.m_flags & Vehicle.Flags.TransferToTarget) != 0)
            {
                data.m_transferSize = (ushort)Mathf.Clamp(data.m_transferSize - amountDelta, 0, data.m_transferSize);
                if (data.m_sourceBuilding != 0)
                {
                    IndustryBuildingManager.ExchangeResource(data.m_transferType, amountDelta, data.m_sourceBuilding, data.m_targetBuilding);
                }
            }
            if ((data.m_flags & Vehicle.Flags.TransferToSource) != 0)
            {
                data.m_transferSize += (ushort)Mathf.Max(0, -amountDelta);
            }
            if (data.m_sourceBuilding != 0 && (instance.m_buildings.m_buffer[data.m_sourceBuilding].m_flags & Building.Flags.IncomingOutgoing) == Building.Flags.Outgoing)
            {
                BuildingInfo info2 = instance.m_buildings.m_buffer[data.m_sourceBuilding].Info;
                ushort num = instance.FindBuilding(instance.m_buildings.m_buffer[data.m_sourceBuilding].m_position, 200f, info2.m_class.m_service, ItemClass.SubService.None, Building.Flags.Incoming, Building.Flags.Outgoing);
                if (num != 0)
                {
                    instance.m_buildings.m_buffer[data.m_sourceBuilding].RemoveOwnVehicle(vehicleID, ref data);
                    data.m_sourceBuilding = num;
                    instance.m_buildings.m_buffer[data.m_sourceBuilding].AddOwnVehicle(vehicleID, ref data);
                }
            }
            if ((instance.m_buildings.m_buffer[data.m_targetBuilding].m_flags & Building.Flags.IncomingOutgoing) == Building.Flags.Incoming)
            {
                ushort num2 = instance.FindBuilding(instance.m_buildings.m_buffer[data.m_targetBuilding].m_position, 200f, info.m_class.m_service, ItemClass.SubService.None, Building.Flags.Outgoing, Building.Flags.Incoming);
                if (num2 != 0)
                {
                    data.Unspawn(vehicleID);
                    BuildingInfo info3 = instance.m_buildings.m_buffer[num2].Info;
                    Randomizer randomizer = new Randomizer(vehicleID);
                    info3.m_buildingAI.CalculateSpawnPosition(num2, ref instance.m_buildings.m_buffer[num2], ref randomizer, m_info, out var position, out var target);
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
                    SetTarget(vehicleID, ref data, 0);
                    return true;
                }
            }
            SetTarget(vehicleID, ref data, 0);
            return false;
        }

        private bool ArriveAtSource(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_sourceBuilding == 0)
            {
                Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleID);
                return true;
            }
            if ((data.m_flags & Vehicle.Flags.TransferToSource) != 0)
            {
                int num = data.m_transferSize;
                BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].Info;
                if (data.m_transferType >= 200)
                {
                    byte transferType = (byte)(data.m_transferType - 200);
                    ((IExtendedBuildingAI)info.m_buildingAI).ExtendedModifyMaterialBuffer(data.m_sourceBuilding, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding], (ExtendedTransferManager.TransferReason)transferType, ref num);
                }
                else
                {
                    info.m_buildingAI.ModifyMaterialBuffer(data.m_sourceBuilding, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding], (TransferManager.TransferReason)data.m_transferType, ref num);
                }
                data.m_transferSize = (ushort)Mathf.Clamp(data.m_transferSize - num, 0, data.m_transferSize);
            }
            RemoveSource(vehicleID, ref data);
            Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleID);
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
                    info.m_buildingAI.CalculateUnspawnPosition(leaderData.m_sourceBuilding, ref instance.m_buildings.m_buffer[leaderData.m_sourceBuilding], ref randomizer, m_info, out var position, out var _);
                    vehicleData.SetTargetPos(index++, CalculateTargetPoint(refPos, position, minSqrDistance, 4f));
                }
            }
            else if (leaderData.m_targetBuilding != 0)
            {
                BuildingManager instance2 = Singleton<BuildingManager>.instance;
                BuildingInfo info2 = instance2.m_buildings.m_buffer[leaderData.m_targetBuilding].Info;
                Randomizer randomizer2 = new Randomizer(vehicleID);
                info2.m_buildingAI.CalculateUnspawnPosition(leaderData.m_targetBuilding, ref instance2.m_buildings.m_buffer[leaderData.m_targetBuilding], ref randomizer2, m_info, out var position2, out var _);
                vehicleData.SetTargetPos(index++, CalculateTargetPoint(refPos, position2, minSqrDistance, 4f));
            }
        }

        protected override bool NeedChangeVehicleType(ushort vehicleID, ref Vehicle vehicleData, PathUnit.Position pathPos, uint laneID, VehicleInfo.VehicleType laneVehicleType, ref Vector4 targetPos)
        {
            return NeedChangeVehicleType(m_info, vehicleID, ref vehicleData, pathPos, laneID, laneVehicleType, ref targetPos);
        }

        protected override bool ChangeVehicleType(ushort vehicleID, ref Vehicle vehicleData, PathUnit.Position pathPos, uint laneID)
        {
            return ChangeVehicleType(m_info, vehicleID, ref vehicleData, pathPos, laneID);
        }

        public static bool NeedChangeVehicleType(VehicleInfo vehicleInfo, ushort vehicleID, ref Vehicle vehicleData, PathUnit.Position pathPos, uint laneID, VehicleInfo.VehicleType laneVehicleType, ref Vector4 targetPos)
        {
            if ((laneVehicleType & VehicleInfo.VehicleType.Car) == 0 && (laneVehicleType & (VehicleInfo.VehicleType.Train | VehicleInfo.VehicleType.Ship | VehicleInfo.VehicleType.Plane)) != 0)
            {
                NetManager instance = Singleton<NetManager>.instance;
                BuildingManager instance2 = Singleton<BuildingManager>.instance;
                NetInfo info = instance.m_segments.m_buffer[pathPos.m_segment].Info;
                PathUnit.CalculatePathPositionOffset(laneID, targetPos, out var offset);
                Vector3 position = instance.m_lanes.m_buffer[laneID].CalculatePosition((float)(int)offset * 0.003921569f);
                ushort num = FindCargoStation(position, info.m_class.m_service);
                if (num != 0 && num != vehicleData.m_sourceBuilding)
                {
                    BuildingInfo info2 = instance2.m_buildings.m_buffer[num].Info;
                    Randomizer randomizer = new Randomizer(vehicleID);
                    info2.m_buildingAI.CalculateUnspawnPosition(num, ref instance2.m_buildings.m_buffer[num], ref randomizer, vehicleInfo, out var position2, out var _);
                    targetPos = position2;
                    targetPos.w = 4f;
                }
                return true;
            }
            return false;
        }

        private static ushort FindCargoStation(Vector3 position, ItemClass.Service service, ItemClass.SubService subservice = ItemClass.SubService.None)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            if (subservice != ItemClass.SubService.PublicTransportPlane)
            {
                subservice = ItemClass.SubService.None;
            }
            ushort num = instance.FindBuilding(position, 100f, service, subservice, Building.Flags.None, Building.Flags.None);
            int num2 = 0;
            while (num != 0)
            {
                ushort parentBuilding = instance.m_buildings.m_buffer[num].m_parentBuilding;
                BuildingInfo info = instance.m_buildings.m_buffer[num].Info;
                if (info.m_buildingAI is CargoStationAI)
                {
                    return num;
                }
                if (info.m_buildingAI is OutsideConnectionAI)
                {
                    return num;
                }
                if (info.m_buildingAI is ExtendedWarehouseStationAI)
                {
                    return num;
                }
                if (parentBuilding == 0)
                {
                    return num;
                }
                num = parentBuilding;
                if (++num2 > 49152)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return 0;
        }

        public static bool ChangeVehicleType(VehicleInfo vehicleInfo, ushort vehicleID, ref Vehicle vehicleData, PathUnit.Position pathPos, uint laneID, bool canReturnToSource = false)
        {
            if (!canReturnToSource && (vehicleData.m_flags & (Vehicle.Flags.TransferToSource | Vehicle.Flags.GoingBack)) != 0)
            {
                return false;
            }
            VehicleManager instance = Singleton<VehicleManager>.instance;
            NetManager instance2 = Singleton<NetManager>.instance;
            BuildingManager instance3 = Singleton<BuildingManager>.instance;
            NetInfo info = instance2.m_segments.m_buffer[pathPos.m_segment].Info;
            Vector3 vector = instance2.m_lanes.m_buffer[laneID].CalculatePosition(0.5f);
            Vector3 lastPos = vector;
            if (!SkipNonCarPaths(ref vehicleData.m_path, ref vehicleData.m_pathPositionIndex, ref vehicleData.m_lastPathOffset, ref lastPos))
            {
                return false;
            }
            ushort num = FindCargoStation(vector, info.m_class.m_service, info.m_class.m_subService);
            ushort num2 = FindCargoStation(lastPos, info.m_class.m_service, info.m_class.m_subService);
            if (num2 == num)
            {
                return true;
            }
            bool flag = false;
            if (num != 0 && (instance3.m_buildings.m_buffer[num].m_flags & Building.Flags.Active) != 0)
            {
                flag = true;
            }
            bool flag2 = false;
            if (num2 != 0 && (instance3.m_buildings.m_buffer[num2].m_flags & Building.Flags.Active) != 0)
            {
                flag2 = true;
            }
            ushort vehicle;
            bool vehicle_created;
            if (vehicleData.m_transferType >= 200)
            {
                vehicle_created = ExtedndedVehicleManager.CreateVehicle(out vehicle, ref Singleton<SimulationManager>.instance.m_randomizer, vehicleInfo, vector, vehicleData.m_transferType, transferToSource: false, transferToTarget: true);
            }
            else
            {
                vehicle_created = instance.CreateVehicle(out vehicle, ref Singleton<SimulationManager>.instance.m_randomizer, vehicleInfo, vector, (TransferManager.TransferReason)vehicleData.m_transferType, transferToSource: false, transferToTarget: true);
            }
            if (flag && flag2 && vehicle_created)
            {
                if (vehicleData.m_targetBuilding != 0)
                {
                    instance.m_vehicles.m_buffer[vehicle].m_targetBuilding = vehicleData.m_targetBuilding;
                    instance.m_vehicles.m_buffer[vehicle].m_flags &= ~Vehicle.Flags.WaitingTarget;
                    instance3.m_buildings.m_buffer[vehicleData.m_targetBuilding].AddGuestVehicle(vehicle, ref instance.m_vehicles.m_buffer[vehicle]);
                }
                instance.m_vehicles.m_buffer[vehicle].m_transferSize = vehicleData.m_transferSize;
                instance.m_vehicles.m_buffer[vehicle].m_path = vehicleData.m_path;
                instance.m_vehicles.m_buffer[vehicle].m_pathPositionIndex = vehicleData.m_pathPositionIndex;
                instance.m_vehicles.m_buffer[vehicle].m_lastPathOffset = vehicleData.m_lastPathOffset;
                instance.m_vehicles.m_buffer[vehicle].m_flags |= vehicleData.m_flags & (Vehicle.Flags.Importing | Vehicle.Flags.Exporting);
                vehicleData.m_path = 0u;
                ushort vehicle2 = FindCargoParent(num, num2, info.m_class.m_service, info.m_class.m_subService);
                VehicleInfo vehicleInfo2;
                if (vehicle2 != 0)
                {
                    vehicleInfo2 = instance.m_vehicles.m_buffer[vehicle2].Info;
                }
                else
                {
                    vehicleInfo2 = instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, info.m_class.m_service, info.m_class.m_subService, ItemClass.Level.Level4);
                    if ((object)vehicleInfo2 != null && instance.CreateVehicle(out vehicle2, ref Singleton<SimulationManager>.instance.m_randomizer, vehicleInfo2, vector, TransferManager.TransferReason.None, transferToSource: false, transferToTarget: true))
                    {
                        vehicleInfo2.m_vehicleAI.SetSource(vehicle2, ref instance.m_vehicles.m_buffer[vehicle2], num);
                        vehicleInfo2.m_vehicleAI.SetTarget(vehicle2, ref instance.m_vehicles.m_buffer[vehicle2], num2);
                    }
                }
                if (vehicle2 != 0)
                {
                    vehicleInfo2.m_vehicleAI.GetSize(vehicle2, ref instance.m_vehicles.m_buffer[vehicle2], out var size, out var max);
                    instance.m_vehicles.m_buffer[vehicle].m_cargoParent = vehicle2;
                    instance.m_vehicles.m_buffer[vehicle].m_nextCargo = instance.m_vehicles.m_buffer[vehicle2].m_firstCargo;
                    instance.m_vehicles.m_buffer[vehicle2].m_firstCargo = vehicle;
                    instance.m_vehicles.m_buffer[vehicle2].m_transferSize = (ushort)(++size);
                    if (size >= max && vehicleInfo2.m_vehicleAI.CanSpawnAt(vector))
                    {
                        instance.m_vehicles.m_buffer[vehicle2].m_flags &= ~Vehicle.Flags.WaitingCargo;
                        instance.m_vehicles.m_buffer[vehicle2].m_waitCounter = 0;
                        vehicleInfo2.m_vehicleAI.SetTarget(vehicle2, ref instance.m_vehicles.m_buffer[vehicle2], num2);
                    }
                }
                else
                {
                    instance.ReleaseVehicle(vehicle);
                }
                if (vehicleData.m_sourceBuilding != 0)
                {
                    IndustryBuildingManager.ExchangeResource(vehicleData.m_transferType, vehicleData.m_transferSize, vehicleData.m_sourceBuilding, num);
                }
            }
            vehicleData.m_transferSize = 0;
            if (num != 0)
            {
                vehicleData.Unspawn(vehicleID);
                BuildingInfo info2 = instance3.m_buildings.m_buffer[num].Info;
                Randomizer randomizer = new Randomizer(vehicleID);
                info2.m_buildingAI.CalculateSpawnPosition(num, ref instance3.m_buildings.m_buffer[num], ref randomizer, vehicleInfo, out var position, out var target);
                Quaternion rotation = Quaternion.identity;
                Vector3 forward = target - position;
                if (forward.sqrMagnitude > 0.01f)
                {
                    rotation = Quaternion.LookRotation(forward);
                }
                vehicleData.m_frame0 = new Vehicle.Frame(position, rotation);
                vehicleData.m_frame1 = vehicleData.m_frame0;
                vehicleData.m_frame2 = vehicleData.m_frame0;
                vehicleData.m_frame3 = vehicleData.m_frame0;
                vehicleData.m_targetPos0 = position;
                vehicleData.m_targetPos0.w = 2f;
                vehicleData.m_targetPos1 = target;
                vehicleData.m_targetPos1.w = 2f;
                vehicleData.m_targetPos2 = vehicleData.m_targetPos1;
                vehicleData.m_targetPos3 = vehicleData.m_targetPos1;
                if (num == vehicleData.m_sourceBuilding)
                {
                    if (vehicleData.m_targetBuilding != 0)
                    {
                        Singleton<BuildingManager>.instance.m_buildings.m_buffer[vehicleData.m_targetBuilding].RemoveGuestVehicle(vehicleID, ref vehicleData);
                        vehicleData.m_targetBuilding = 0;
                    }
                    vehicleData.m_flags &= ~Vehicle.Flags.WaitingTarget;
                    vehicleData.m_flags |= Vehicle.Flags.GoingBack;
                    vehicleData.m_waitCounter = 0;
                    return true;
                }
            }
            else
            {
                vehicleData.m_targetPos1 = vehicleData.m_targetPos0;
                vehicleData.m_targetPos2 = vehicleData.m_targetPos0;
                vehicleData.m_targetPos3 = vehicleData.m_targetPos0;
            }
            vehicleInfo.m_vehicleAI.SetTarget(vehicleID, ref vehicleData, 0);
            return true;
        }

        public static ushort FindNextCargoParent(ushort sourceBuilding, ItemClass.Service service, ItemClass.SubService subService)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            VehicleManager instance2 = Singleton<VehicleManager>.instance;
            ushort num = instance.m_buildings.m_buffer[sourceBuilding].m_ownVehicles;
            ushort result = 0;
            int num2 = -1;
            int num3 = 0;
            while (num != 0)
            {
                if ((instance2.m_vehicles.m_buffer[num].m_flags & Vehicle.Flags.WaitingCargo) != 0)
                {
                    VehicleInfo info = instance2.m_vehicles.m_buffer[num].Info;
                    if (info.m_class.m_service == service && info.m_class.m_subService == subService)
                    {
                        info.m_vehicleAI.GetSize(num, ref instance2.m_vehicles.m_buffer[num], out var size, out var max);
                        int num4 = Mathf.Max(size * 255 / Mathf.Max(1, max), instance2.m_vehicles.m_buffer[num].m_waitCounter);
                        if (num4 > num2)
                        {
                            result = num;
                            num2 = num4;
                        }
                    }
                }
                num = instance2.m_vehicles.m_buffer[num].m_nextOwnVehicle;
                if (++num3 >= 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return result;
        }

        public static void SwitchCargoParent(ushort source, ushort target)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort num = instance.m_vehicles.m_buffer[source].m_firstCargo;
            instance.m_vehicles.m_buffer[source].m_firstCargo = 0;
            instance.m_vehicles.m_buffer[target].m_firstCargo = num;
            instance.m_vehicles.m_buffer[target].m_transferSize = instance.m_vehicles.m_buffer[source].m_transferSize;
            int num2 = 0;
            while (num != 0)
            {
                instance.m_vehicles.m_buffer[num].m_cargoParent = target;
                num = instance.m_vehicles.m_buffer[num].m_nextCargo;
                if (++num2 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            instance.ReleaseVehicle(source);
        }

        private static ushort FindCargoParent(ushort sourceBuilding, ushort targetBuilding, ItemClass.Service service, ItemClass.SubService subService)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            VehicleManager instance2 = Singleton<VehicleManager>.instance;
            ushort num = instance.m_buildings.m_buffer[sourceBuilding].m_ownVehicles;
            int num2 = 0;
            while (num != 0)
            {
                if (instance2.m_vehicles.m_buffer[num].m_targetBuilding == targetBuilding && (instance2.m_vehicles.m_buffer[num].m_flags & Vehicle.Flags.WaitingCargo) != 0)
                {
                    VehicleInfo info = instance2.m_vehicles.m_buffer[num].Info;
                    if (info.m_class.m_service == service && info.m_class.m_subService == subService)
                    {
                        info.m_vehicleAI.GetSize(num, ref instance2.m_vehicles.m_buffer[num], out var size, out var max);
                        if (size < max)
                        {
                            return num;
                        }
                    }
                }
                num = instance2.m_vehicles.m_buffer[num].m_nextOwnVehicle;
                if (++num2 >= 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return 0;
        }

        private static bool SkipNonCarPaths(ref uint path, ref byte pathPositionIndex, ref byte lastPathOffset, ref Vector3 lastPos)
        {
            PathManager instance = Singleton<PathManager>.instance;
            NetManager instance2 = Singleton<NetManager>.instance;
            int num = 0;
            PathUnit.Position pathPos = default(PathUnit.Position);
            while (path != 0)
            {
                if (instance.m_pathUnits.m_buffer[path].GetPosition(pathPositionIndex >> 1, out var position))
                {
                    NetInfo info = instance2.m_segments.m_buffer[position.m_segment].Info;
                    if (info.m_lanes != null && info.m_lanes.Length > position.m_lane && (info.m_lanes[position.m_lane].m_vehicleType & VehicleInfo.VehicleType.Car) != 0)
                    {
                        if (pathPos.m_segment != 0)
                        {
                            uint laneID = PathManager.GetLaneID(pathPos);
                            uint laneID2 = PathManager.GetLaneID(position);
                            if (laneID != 0 && laneID2 != 0)
                            {
                                lastPos = instance2.m_lanes.m_buffer[laneID].CalculatePosition(0.5f);
                                PathUnit.CalculatePathPositionOffset(laneID2, lastPos, out lastPathOffset);
                                return true;
                            }
                        }
                        return false;
                    }
                    pathPos = position;
                    int num2 = (pathPositionIndex >> 1) + 1;
                    if (num2 >= instance.m_pathUnits.m_buffer[path].m_positionCount)
                    {
                        num2 = 0;
                        Singleton<PathManager>.instance.ReleaseFirstUnit(ref path);
                    }
                    pathPositionIndex = (byte)(num2 << 1);
                    if (++num >= 262144)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        break;
                    }
                    continue;
                }
                return false;
            }
            if (pathPos.m_segment != 0)
            {
                uint laneID3 = PathManager.GetLaneID(pathPos);
                if (laneID3 != 0)
                {
                    lastPos = instance2.m_lanes.m_buffer[laneID3].CalculatePosition(0.5f);
                    return true;
                }
            }
            return false;
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
                    Building[] buffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
                    BuildingInfo info = buffer[vehicleData.m_sourceBuilding].Info;
                    BuildingInfo info2 = buffer[vehicleData.m_targetBuilding].Info;
                    Randomizer randomizer = new Randomizer(vehicleID);
                    info.m_buildingAI.CalculateUnspawnPosition(vehicleData.m_sourceBuilding, ref buffer[vehicleData.m_sourceBuilding], ref randomizer, m_info, out var _, out var target);
                    bool startNotUseTruck = vehicleData.m_targetBuilding != 0 && !info2.m_buildingAI.UseTruckDelivery(vehicleData.m_targetBuilding, ref buffer[vehicleData.m_targetBuilding]);
                    bool endNotUseTruck = !info.m_buildingAI.UseTruckDelivery(vehicleData.m_sourceBuilding, ref buffer[vehicleData.m_sourceBuilding]);
                    return StartPathFind(vehicleID, ref vehicleData, vehicleData.m_targetPos3, target, startBothWays: true, endBothWays: true, undergroundTarget: false, startNotUseTruck, endNotUseTruck);
                }
            }
            else if (vehicleData.m_targetBuilding != 0)
            {
                Building[] buffer2 = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
                BuildingInfo info3 = buffer2[vehicleData.m_sourceBuilding].Info;
                BuildingInfo info4 = buffer2[vehicleData.m_targetBuilding].Info;
                Randomizer randomizer2 = new Randomizer(vehicleID);
                info4.m_buildingAI.CalculateUnspawnPosition(vehicleData.m_targetBuilding, ref buffer2[vehicleData.m_targetBuilding], ref randomizer2, m_info, out var _, out var target2);
                bool startNotUseTruck2 = vehicleData.m_sourceBuilding != 0 && !info3.m_buildingAI.UseTruckDelivery(vehicleData.m_sourceBuilding, ref buffer2[vehicleData.m_sourceBuilding]);
                bool endNotUseTruck2 = !info4.m_buildingAI.UseTruckDelivery(vehicleData.m_targetBuilding, ref buffer2[vehicleData.m_targetBuilding]);
                return StartPathFind(vehicleID, ref vehicleData, vehicleData.m_targetPos3, target2, startBothWays: true, endBothWays: true, undergroundTarget: false, startNotUseTruck2, endNotUseTruck2);
            }
            return false;
        }

        protected override bool StartPathFind(ushort vehicleID, ref Vehicle vehicleData, Vector3 startPos, Vector3 endPos, bool startBothWays, bool endBothWays, bool undergroundTarget)
        {
            if ((vehicleData.m_flags & (Vehicle.Flags.TransferToSource | Vehicle.Flags.GoingBack)) != 0)
            {
                return base.StartPathFind(vehicleID, ref vehicleData, startPos, endPos, startBothWays, endBothWays, undergroundTarget);
            }
            return StartPathFind(vehicleID, ref vehicleData, startPos, endPos, startBothWays, endBothWays, undergroundTarget, startNotUseTruck: false, endNotUseTruck: false);
        }

        protected bool StartPathFind(ushort vehicleID, ref Vehicle vehicleData, Vector3 startPos, Vector3 endPos, bool startBothWays, bool endBothWays, bool undergroundTarget, bool startNotUseTruck, bool endNotUseTruck)
        {
            bool allowUnderground = (vehicleData.m_flags & (Vehicle.Flags.Underground | Vehicle.Flags.Transition)) != 0;
            PathUnit.Position pathPosA;
            PathUnit.Position pathPosB;
            float distanceSqrA;
            float distanceSqrB;
            bool flag = PathManager.FindPathPosition(startPos, ItemClass.Service.Road, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, VehicleInfo.VehicleType.Car, vehicleData.Info.vehicleCategory, allowUnderground, requireConnect: false, 32f, excludeLaneWidth: false, checkPedestrianStreet: false, out pathPosA, out pathPosB, out distanceSqrA, out distanceSqrB) && !startNotUseTruck;
            if (PathManager.FindPathPosition(startPos, ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Train | VehicleInfo.VehicleType.Ship | VehicleInfo.VehicleType.Plane, vehicleData.Info.vehicleCategory | VehicleInfo.VehicleCategory.Cargo, allowUnderground, requireConnect: false, 32f, excludeLaneWidth: false, checkPedestrianStreet: false, out var pathPosA2, out var pathPosB2, out var distanceSqrA2, out var distanceSqrB2))
            {
                if (!flag || (distanceSqrA2 < distanceSqrA && (Mathf.Abs(startPos.x) > 4800f || Mathf.Abs(startPos.z) > 4800f)))
                {
                    pathPosA = pathPosA2;
                    pathPosB = pathPosB2;
                    distanceSqrA = distanceSqrA2;
                    distanceSqrB = distanceSqrB2;
                }
                flag = true;
            }
            PathUnit.Position pathPosA3;
            PathUnit.Position pathPosB3;
            float distanceSqrA3;
            float distanceSqrB3;
            bool flag2 = PathManager.FindPathPosition(endPos, ItemClass.Service.Road, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, VehicleInfo.VehicleType.Car, vehicleData.Info.vehicleCategory, undergroundTarget, requireConnect: false, 32f, excludeLaneWidth: false, checkPedestrianStreet: false, out pathPosA3, out pathPosB3, out distanceSqrA3, out distanceSqrB3) && !endNotUseTruck;
            if (PathManager.FindPathPosition(endPos, ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Train | VehicleInfo.VehicleType.Ship | VehicleInfo.VehicleType.Plane, vehicleData.Info.vehicleCategory | VehicleInfo.VehicleCategory.Cargo, undergroundTarget, requireConnect: false, 32f, excludeLaneWidth: false, checkPedestrianStreet: false, out var pathPosA4, out var pathPosB4, out var distanceSqrA4, out var distanceSqrB4))
            {
                if (!flag2 || (distanceSqrA4 < distanceSqrA3 && (Mathf.Abs(endPos.x) > 4800f || Mathf.Abs(endPos.z) > 4800f)))
                {
                    pathPosA3 = pathPosA4;
                    pathPosB3 = pathPosB4;
                    distanceSqrA3 = distanceSqrA4;
                    distanceSqrB3 = distanceSqrB4;
                }
                flag2 = true;
            }
            if (flag && flag2)
            {
                PathManager instance = Singleton<PathManager>.instance;
                if (!startBothWays || distanceSqrA < 10f)
                {
                    pathPosB = default(PathUnit.Position);
                }
                if (!endBothWays || distanceSqrA3 < 10f)
                {
                    pathPosB3 = default(PathUnit.Position);
                }
                NetInfo.LaneType laneTypes = NetInfo.LaneType.Vehicle | NetInfo.LaneType.CargoVehicle;
                VehicleInfo.VehicleType vehicleTypes = VehicleInfo.VehicleType.Car | VehicleInfo.VehicleType.Train | VehicleInfo.VehicleType.Ship | VehicleInfo.VehicleType.Plane;
                VehicleInfo.VehicleCategory vehicleCategories = vehicleData.Info.vehicleCategory | VehicleInfo.VehicleCategory.Cargo;
                if (instance.CreatePath(out var unit, ref Singleton<SimulationManager>.instance.m_randomizer, Singleton<SimulationManager>.instance.m_currentBuildIndex, pathPosA, pathPosB, pathPosA3, pathPosB3, default(PathUnit.Position), laneTypes, vehicleTypes, vehicleCategories, 20000f, IsHeavyVehicle(), IgnoreBlocked(vehicleID, ref vehicleData), stablePath: false, skipQueue: false, randomParking: false, ignoreFlooded: false, CombustionEngine()))
                {
                    if (vehicleData.m_path != 0)
                    {
                        instance.ReleasePath(vehicleData.m_path);
                    }
                    vehicleData.m_path = unit;
                    vehicleData.m_flags |= Vehicle.Flags.WaitingPath;
                    return true;
                }
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

        public override void EnterTollRoad(ushort vehicle, ref Vehicle vehicleData, ushort buildingID, ushort segmentID, int basePrice)
        {
            if (buildingID != 0)
            {
                int num = basePrice * 2;
                BuildingManager instance = Singleton<BuildingManager>.instance;
                DistrictManager instance2 = Singleton<DistrictManager>.instance;
                byte district = instance2.GetDistrict(instance.m_buildings.m_buffer[buildingID].m_position);
                DistrictPolicies.CityPlanning cityPlanningPolicies = instance2.m_districts.m_buffer[district].m_cityPlanningPolicies;
                if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.AutomatedToll) != 0)
                {
                    num = (num * 70 + Singleton<SimulationManager>.instance.m_randomizer.Int32(100u)) / 100;
                    instance2.m_districts.m_buffer[district].m_cityPlanningPoliciesEffect |= DistrictPolicies.CityPlanning.AutomatedToll;
                }
                else
                {
                    vehicleData.m_flags2 |= Vehicle.Flags2.EndStop;
                }
                Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.PublicIncome, num, ItemClass.Service.Vehicles, ItemClass.SubService.None, ItemClass.Level.Level2);
                instance.m_buildings.m_buffer[buildingID].m_customBuffer1 = (ushort)Mathf.Min(instance.m_buildings.m_buffer[buildingID].m_customBuffer1 + 1, 65535);
            }
        }

        public override int GetNoiseLevel()
        {
            return (!IsHeavyVehicle()) ? 6 : 9;
        }

        protected override bool IsHeavyVehicle()
        {
            return m_isHeavyVehicle;
        }

        protected override bool CombustionEngine()
        {
            return true;
        }

    }
}








