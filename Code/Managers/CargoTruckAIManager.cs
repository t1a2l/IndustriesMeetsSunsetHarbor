using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;
using MoreTransferReasons;
using IndustriesMeetsSunsetHarbor.AI;
using IndustriesMeetsSunsetHarbor.HarmonyPatches;
using System;

namespace IndustriesMeetsSunsetHarbor.Managers
{
    public static class CargoTruckAIManager
    {
        public static void ReleaseVehicle(CargoTruckAI cargo_instance, ushort vehicleID, ref Vehicle data)
        {
            if ((data.m_flags & Vehicle.Flags.TransferToTarget) != 0 && data.m_sourceBuilding != 0 && data.m_transferSize != 0)
            {
                int transferSize = data.m_transferSize;
                ModifyMaterialBufferSource(ref data, ref transferSize);
            }
            RemoveOffers(vehicleID, ref data);
            RemoveSource(vehicleID, ref data);
            RemoveTarget(vehicleID, ref data);
            cargo_instance.ReleaseVehicle(vehicleID, ref data);
        }

        public static void SetSource(CargoTruckAI cargo_instance, ushort vehicleID, ref Vehicle data, ushort sourceBuilding)
        {
            RemoveSource(vehicleID, ref data);
            data.m_sourceBuilding = sourceBuilding;
            if (sourceBuilding != 0)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                BuildingInfo info = instance.m_buildings.m_buffer[(int)sourceBuilding].Info;
                data.Unspawn(vehicleID);
                Randomizer randomizer = new Randomizer((int)vehicleID);
                Vector3 vector;
                Vector3 vector2;
                info.m_buildingAI.CalculateSpawnPosition(sourceBuilding, ref instance.m_buildings.m_buffer[(int)sourceBuilding], ref randomizer, cargo_instance.m_info, out vector, out vector2);
                Quaternion quaternion = Quaternion.identity;
                Vector3 vector3 = vector2 - vector;
                if (vector3.sqrMagnitude > 0.01f)
                {
                    quaternion = Quaternion.LookRotation(vector3);
                }
                data.m_frame0 = new Vehicle.Frame(vector, quaternion);
                data.m_frame1 = data.m_frame0;
                data.m_frame2 = data.m_frame0;
                data.m_frame3 = data.m_frame0;
                data.m_targetPos0 = vector;
                data.m_targetPos0.w = 2f;
                data.m_targetPos1 = vector2;
                data.m_targetPos1.w = 2f;
                data.m_targetPos2 = data.m_targetPos1;
                data.m_targetPos3 = data.m_targetPos1;
                if ((data.m_flags & Vehicle.Flags.TransferToTarget) != 0)
                {
                    int num = Mathf.Min(0, data.m_transferSize - cargo_instance.m_cargoCapacity);
                    ModifyMaterialBufferSource(ref data, ref num);
                    num = Mathf.Max(0, -num);
                    data.m_transferSize += (ushort)num;
                }
                cargo_instance.FrameDataUpdated(vehicleID, ref data, ref data.m_frame0);
                instance.m_buildings.m_buffer[(int)data.m_sourceBuilding].AddOwnVehicle(vehicleID, ref data);
                if ((instance.m_buildings.m_buffer[(int)data.m_sourceBuilding].m_flags & Building.Flags.IncomingOutgoing) != Building.Flags.None)
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
        }

        public static bool ArriveAtTarget(CargoTruckAI cargo_instance, ushort vehicleID, ref Vehicle data)
        {
            if (data.m_targetBuilding == 0)
            {
                return true;
            }
            int num = 0;
            if ((data.m_flags & Vehicle.Flags.TransferToTarget) != 0)
            {
                num = (int)data.m_transferSize;
            }
            if ((data.m_flags & Vehicle.Flags.TransferToSource) != 0)
            {
                num = Mathf.Min(0, (int)data.m_transferSize - cargo_instance.m_cargoCapacity);
            }
            BuildingManager instance = Singleton<BuildingManager>.instance;
            BuildingInfo info = instance.m_buildings.m_buffer[(int)data.m_targetBuilding].Info;
            ModifyMaterialBufferTarget(ref data, ref num);
            if ((data.m_flags & Vehicle.Flags.TransferToTarget) != 0)
            {
                data.m_transferSize = (ushort)Mathf.Clamp((int)data.m_transferSize - num, 0, (int)data.m_transferSize);
                if (data.m_sourceBuilding != 0 && info.m_buildingAI is not RestaurantAI && info.m_buildingAI is not NewProcessingFacilityAI && info.m_buildingAI is not NewUniqueFactoryAI)
                {
                    IndustryBuildingAI.ExchangeResource((TransferManager.TransferReason)data.m_transferType, num, data.m_sourceBuilding, data.m_targetBuilding);
                }
            }
            if ((data.m_flags & Vehicle.Flags.TransferToSource) != 0)
            {
                data.m_transferSize += (ushort)Mathf.Max(0, -num);
            }
            if (data.m_sourceBuilding != 0 && (instance.m_buildings.m_buffer[(int)data.m_sourceBuilding].m_flags & Building.Flags.IncomingOutgoing) == Building.Flags.Outgoing)
            {
                BuildingInfo info2 = instance.m_buildings.m_buffer[(int)data.m_sourceBuilding].Info;
                ushort num2 = instance.FindBuilding(instance.m_buildings.m_buffer[(int)data.m_sourceBuilding].m_position, 200f, info2.m_class.m_service, ItemClass.SubService.None, Building.Flags.Incoming, Building.Flags.Outgoing);
                if (num2 != 0)
                {
                    instance.m_buildings.m_buffer[(int)data.m_sourceBuilding].RemoveOwnVehicle(vehicleID, ref data);
                    data.m_sourceBuilding = num2;
                    instance.m_buildings.m_buffer[(int)data.m_sourceBuilding].AddOwnVehicle(vehicleID, ref data);
                }
            }
            if ((instance.m_buildings.m_buffer[(int)data.m_targetBuilding].m_flags & Building.Flags.IncomingOutgoing) == Building.Flags.Incoming)
            {
                ushort num3 = instance.FindBuilding(instance.m_buildings.m_buffer[(int)data.m_targetBuilding].m_position, 200f, info.m_class.m_service, ItemClass.SubService.None, Building.Flags.Outgoing, Building.Flags.Incoming);
                if (num3 != 0)
                {
                    data.Unspawn(vehicleID);
                    BuildingInfo info3 = instance.m_buildings.m_buffer[(int)num3].Info;
                    Randomizer randomizer = new Randomizer((int)vehicleID);
                    info3.m_buildingAI.CalculateSpawnPosition(num3, ref instance.m_buildings.m_buffer[(int)num3], ref randomizer, cargo_instance.m_info, out Vector3 vector, out Vector3 vector2);
                    Quaternion quaternion = Quaternion.identity;
                    Vector3 vector3 = vector2 - vector;
                    if (vector3.sqrMagnitude > 0.01f)
                    {
                        quaternion = Quaternion.LookRotation(vector3);
                    }
                    data.m_frame0 = new Vehicle.Frame(vector, quaternion);
                    data.m_frame1 = data.m_frame0;
                    data.m_frame2 = data.m_frame0;
                    data.m_frame3 = data.m_frame0;
                    data.m_targetPos0 = vector;
                    data.m_targetPos0.w = 2f;
                    data.m_targetPos1 = vector2;
                    data.m_targetPos1.w = 2f;
                    data.m_targetPos2 = data.m_targetPos1;
                    data.m_targetPos3 = data.m_targetPos1;
                    cargo_instance.FrameDataUpdated(vehicleID, ref data, ref data.m_frame0);
                    cargo_instance.SetTarget(vehicleID, ref data, 0);
                    return true;
                }
            }
            cargo_instance.SetTarget(vehicleID, ref data, 0);
            return false;
        }

        public static bool ArriveAtSource(CargoTruckAI cargo_instance, ushort vehicleID, ref Vehicle data)
        {
            if (data.m_sourceBuilding == 0)
            {
                Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleID);
                return true;
            }
            if ((data.m_flags & Vehicle.Flags.TransferToSource) != 0)
            {
                int num = data.m_transferSize;
                ModifyMaterialBufferSource(ref data, ref num);
                data.m_transferSize = (ushort)Mathf.Clamp(data.m_transferSize - num, 0, data.m_transferSize);
            }
            RemoveSource(vehicleID, ref data);
            Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleID);
            return true;
        }

        private static void RemoveOffers(ushort vehicleID, ref Vehicle data)
        {
            if ((data.m_flags & Vehicle.Flags.WaitingTarget) != (Vehicle.Flags)0)
            {
                ExtendedTransferManager.Offer transferOffer = default(ExtendedTransferManager.Offer);
                transferOffer.Vehicle = vehicleID;
                if ((data.m_flags & Vehicle.Flags.TransferToSource) != 0)
                {
                    Singleton<ExtendedTransferManager>.instance.RemoveIncomingOffer((ExtendedTransferManager.TransferReason)data.m_transferType, transferOffer);
                }
                else if ((data.m_flags & Vehicle.Flags.TransferToTarget) != 0)
                {
                    Singleton<ExtendedTransferManager>.instance.RemoveOutgoingOffer((ExtendedTransferManager.TransferReason)data.m_transferType, transferOffer);
                }
            }
        }

        private static void RemoveSource(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_sourceBuilding != 0)
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)data.m_sourceBuilding].RemoveOwnVehicle(vehicleID, ref data);
                data.m_sourceBuilding = 0;
            }
        }

        private static void RemoveTarget(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_targetBuilding != 0)
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)data.m_targetBuilding].RemoveGuestVehicle(vehicleID, ref data);
                data.m_targetBuilding = 0;
            }
        }

        public static void SetTarget(CargoTruckAI cargo_instance, ushort vehicleID, ref Vehicle data, ushort targetBuilding)
        {
            if (targetBuilding == data.m_targetBuilding)
            {
                if (data.m_path == 0)
                {
                    if (!CargoTruckAIPatch.StartPathFind(cargo_instance, vehicleID, ref data))
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
            }
            else
            {
                if ((data.m_flags & Vehicle.Flags.TransferToTarget) != 0)
                {
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
                        Singleton<ExtendedTransferManager>.instance.AddOutgoingOffer((ExtendedTransferManager.TransferReason)data.m_transferType, offer);
                        data.m_flags |= Vehicle.Flags.WaitingTarget;
                    }
                    else
                    {
                        data.m_flags |= Vehicle.Flags.GoingBack;
                    }
                }
                if ((data.m_flags & Vehicle.Flags.TransferToSource) != 0)
                {
                    if (data.m_transferSize < cargo_instance.m_cargoCapacity)
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
                        Singleton<ExtendedTransferManager>.instance.AddIncomingOffer((ExtendedTransferManager.TransferReason)data.m_transferType, offer2);
                        data.m_flags |= Vehicle.Flags.WaitingTarget;
                    }
                    else
                    {
                        data.m_flags |= Vehicle.Flags.GoingBack;
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
            else if (!CargoTruckAIPatch.StartPathFind(cargo_instance, vehicleID, ref data))
            {
                data.Unspawn(vehicleID);
            }
        }

        private static void ModifyMaterialBufferSource(ref Vehicle data, ref int num)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;

            BuildingInfo info_source = instance.m_buildings.m_buffer[data.m_sourceBuilding].Info;
            if (info_source != null)
            {
                if(info_source.m_buildingAI is RestaurantAI || info_source.m_buildingAI is NewUniqueFactoryAI || info_source.m_buildingAI is NewProcessingFacilityAI)
                {
                    ((IExtendedBuildingAI)info_source.m_buildingAI).ExtendedModifyMaterialBuffer(data.m_sourceBuilding, ref instance.m_buildings.m_buffer[(int)data.m_sourceBuilding], (ExtendedTransferManager.TransferReason)data.m_transferType, ref num);
                }
                else
                {
                    info_source.m_buildingAI.ModifyMaterialBuffer(data.m_sourceBuilding, ref instance.m_buildings.m_buffer[(int)data.m_sourceBuilding], (TransferManager.TransferReason)data.m_transferType, ref num);
                }
            }

        }

        private static void ModifyMaterialBufferTarget(ref Vehicle data, ref int num)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;

            BuildingInfo info_source = instance.m_buildings.m_buffer[data.m_sourceBuilding].Info;
            BuildingInfo info_target = instance.m_buildings.m_buffer[data.m_targetBuilding].Info;
 
            if (info_source != null && info_target != null)
            {
                if ((info_source.m_buildingAI is NewUniqueFactoryAI || info_source.m_buildingAI is NewProcessingFacilityAI) && info_target.m_buildingAI is RestaurantAI)
                {
                    ((IExtendedBuildingAI)info_target.m_buildingAI).ExtendedModifyMaterialBuffer(data.m_targetBuilding, ref instance.m_buildings.m_buffer[(int)data.m_targetBuilding], (ExtendedTransferManager.TransferReason)data.m_transferType, ref num);

                }
                else
                {
                    info_target.m_buildingAI.ModifyMaterialBuffer(data.m_targetBuilding, ref instance.m_buildings.m_buffer[(int)data.m_targetBuilding], (TransferManager.TransferReason)data.m_transferType, ref num);
                }
            }

        }

        public static bool TrySpawn(ushort vehicleID, ref Vehicle vehicleData)
        {
            if ((vehicleData.m_flags & Vehicle.Flags.Spawned) != 0)
            {
                return true;
            }
            if (CheckOverlap(vehicleData.m_segment, 0, 1000f))
            {
                vehicleData.m_flags |= Vehicle.Flags.WaitingSpace;
                return false;
            }
            vehicleData.Spawn(vehicleID);
            vehicleData.m_flags &= ~Vehicle.Flags.WaitingSpace;
            return true;
        }

        private static bool CheckOverlap(Segment3 segment, ushort ignoreVehicle, float maxVelocity)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            Vector3 vector = segment.Min();
            Vector3 vector2 = segment.Max();
            int num = Mathf.Max((int)((vector.x - 10f) / 32f + 270f), 0);
            int num2 = Mathf.Max((int)((vector.z - 10f) / 32f + 270f), 0);
            int num3 = Mathf.Min((int)((vector2.x + 10f) / 32f + 270f), 539);
            int num4 = Mathf.Min((int)((vector2.z + 10f) / 32f + 270f), 539);
            bool overlap = false;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort num5 = instance.m_vehicleGrid[i * 540 + j];
                    int num6 = 0;
                    while (num5 != 0)
                    {
                        num5 = CheckOverlap(segment, ignoreVehicle, maxVelocity, num5, ref instance.m_vehicles.m_buffer[num5], ref overlap);
                        if (++num6 > 16384)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return overlap;
        }

        private static ushort CheckOverlap(Segment3 segment, ushort ignoreVehicle, float maxVelocity, ushort otherID, ref Vehicle otherData, ref bool overlap)
        {
            if ((ignoreVehicle == 0 || (otherID != ignoreVehicle && otherData.m_leadingVehicle != ignoreVehicle && otherData.m_trailingVehicle != ignoreVehicle)) && segment.DistanceSqr(otherData.m_segment, out var _, out var _) < 4f)
            {
                VehicleInfo info = otherData.Info;
                if (info.m_vehicleType == VehicleInfo.VehicleType.Bicycle)
                {
                    return otherData.m_nextGridVehicle;
                }
                if (otherData.GetLastFrameData().m_velocity.sqrMagnitude < maxVelocity * maxVelocity)
                {
                    overlap = true;
                }
            }
            return otherData.m_nextGridVehicle;

        }
    }
}