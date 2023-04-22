using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;
using MoreTransferReasons;
using IndustriesMeetsSunsetHarbor.AI;

namespace IndustriesMeetsSunsetHarbor.Managers
{
    public static class CargoTruckAIManager
    {
        public static void ReleaseVehicle(CargoTruckAI cargo_instance, ushort vehicleID, ref Vehicle data)
        {
            if ((data.m_flags & Vehicle.Flags.TransferToTarget) != 0 && data.m_sourceBuilding != 0 && data.m_transferSize != 0)
            {
                int transferSize = data.m_transferSize;
                ModifyMaterialBufferCall(ref data, ref transferSize);
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
                    ModifyMaterialBufferCall(ref data, ref num);
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
            ModifyMaterialBufferCall(ref data, ref num);
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
                ModifyMaterialBufferCall(ref data, ref num);
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

        private static void ModifyMaterialBufferCall(ref Vehicle data, ref int num)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            BuildingInfo info2 = instance.m_buildings.m_buffer[data.m_targetBuilding].Info;
            if (info2 != null)
            {
                if (info2.m_buildingAI is RestaurantAI restaurantAI)
                {
                    if (data.m_transferType == (byte)ExtendedTransferManager.TransferReason.Bread || data.m_transferType == (byte)ExtendedTransferManager.TransferReason.FoodSupplies || data.m_transferType == (byte)ExtendedTransferManager.TransferReason.DrinkSupplies)
                    { 
                        ((IExtendedBuildingAI)restaurantAI).ExtendedModifyMaterialBuffer(data.m_targetBuilding, ref instance.m_buildings.m_buffer[(int)data.m_targetBuilding], (ExtendedTransferManager.TransferReason)data.m_transferType, ref num);
                    }
                    else
                    {
                        restaurantAI.ModifyMaterialBuffer(data.m_targetBuilding, ref instance.m_buildings.m_buffer[(int)data.m_targetBuilding], (TransferManager.TransferReason)data.m_transferType, ref num);
                    }

                }
                else if (info2.m_buildingAI is NewUniqueFactoryAI newUniqueFactoryAI)
                {
                    if (data.m_transferType == (byte)ExtendedTransferManager.TransferReason.Bread || data.m_transferType == (byte)ExtendedTransferManager.TransferReason.FoodSupplies || data.m_transferType == (byte)ExtendedTransferManager.TransferReason.DrinkSupplies)
                    {
                        ((IExtendedBuildingAI)newUniqueFactoryAI).ExtendedModifyMaterialBuffer(data.m_targetBuilding, ref instance.m_buildings.m_buffer[(int)data.m_targetBuilding], (ExtendedTransferManager.TransferReason)data.m_transferType, ref num);
                    }
                    else
                    {
                        newUniqueFactoryAI.ModifyMaterialBuffer(data.m_targetBuilding, ref instance.m_buildings.m_buffer[(int)data.m_targetBuilding], (TransferManager.TransferReason)data.m_transferType, ref num);
                    }
                }
                else if (info2.m_buildingAI is NewProcessingFacilityAI newProcessingFacilityAI)
                {
                    if (data.m_transferType == (byte)ExtendedTransferManager.TransferReason.Bread || data.m_transferType == (byte)ExtendedTransferManager.TransferReason.FoodSupplies || data.m_transferType == (byte)ExtendedTransferManager.TransferReason.DrinkSupplies)
                    {
                        ((IExtendedBuildingAI)newProcessingFacilityAI).ExtendedModifyMaterialBuffer(data.m_targetBuilding, ref instance.m_buildings.m_buffer[(int)data.m_targetBuilding], (ExtendedTransferManager.TransferReason)data.m_transferType, ref num);
                    }
                    else
                    {
                        newProcessingFacilityAI.ModifyMaterialBuffer(data.m_targetBuilding, ref instance.m_buildings.m_buffer[(int)data.m_targetBuilding], (TransferManager.TransferReason)data.m_transferType, ref num);
                    }
                }
            }

        }

    }
}
