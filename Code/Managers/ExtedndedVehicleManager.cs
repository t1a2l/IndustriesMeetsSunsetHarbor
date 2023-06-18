using ColossalFramework;
using System;
using MoreTransferReasons;
using UnityEngine;
using ColossalFramework.Math;

namespace IndustriesMeetsSunsetHarbor.Managers
{
    public static class ExtedndedVehicleManager
    {
        public static void CalculateOwnVehicles(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort num = data.m_ownVehicles;
            int num2 = 0;
            while (num != 0)
            {
                var transfer_type = instance.m_vehicles.m_buffer[num].m_transferType;
                byte material_byte = (byte)(transfer_type - 200);
                if ((ExtendedTransferManager.TransferReason)material_byte == material)
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

        public static void CalculateGuestVehicles(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort num = data.m_guestVehicles;
            int num2 = 0;
            while (num != 0)
            {
                var transfer_type = instance.m_vehicles.m_buffer[num].m_transferType;
                byte material_byte = (byte)(transfer_type - 200);
                if ((ExtendedTransferManager.TransferReason)material_byte == material)
                {
                    VehicleInfo info = instance.m_vehicles.m_buffer[num].Info;
                    info.m_vehicleAI.GetSize(num, ref instance.m_vehicles.m_buffer[num], out var size, out var max);
                    cargo += Mathf.Min(size, max);
                    capacity += max;
                    count++;
                    if ((instance.m_vehicles.m_buffer[num].m_flags & (Vehicle.Flags.Importing | Vehicle.Flags.Exporting)) != 0)
                    {
                        outside++;
                    }
                }
                num = instance.m_vehicles.m_buffer[num].m_nextGuestVehicle;
                if (++num2 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }

        public static bool CreateVehicle(out ushort vehicle, ref Randomizer r, VehicleInfo info, Vector3 position, byte type, bool transferToSource, bool transferToTarget)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            if (instance.m_vehicles.CreateItem(out var item, ref r))
            {
                vehicle = item;
                Vehicle.Frame frame = new Vehicle.Frame(position, Quaternion.identity);
                instance.m_vehicles.m_buffer[vehicle].m_flags = Vehicle.Flags.Created;
                instance.m_vehicles.m_buffer[vehicle].m_flags2 = (Vehicle.Flags2)0;
                if (transferToSource)
                {
                    instance.m_vehicles.m_buffer[vehicle].m_flags |= Vehicle.Flags.TransferToSource;
                }
                if (transferToTarget)
                {
                    instance.m_vehicles.m_buffer[vehicle].m_flags |= Vehicle.Flags.TransferToTarget;
                }
                instance.m_vehicles.m_buffer[vehicle].Info = info;
                instance.m_vehicles.m_buffer[vehicle].m_frame0 = frame;
                instance.m_vehicles.m_buffer[vehicle].m_frame1 = frame;
                instance.m_vehicles.m_buffer[vehicle].m_frame2 = frame;
                instance.m_vehicles.m_buffer[vehicle].m_frame3 = frame;
                instance.m_vehicles.m_buffer[vehicle].m_targetPos0 = Vector4.zero;
                instance.m_vehicles.m_buffer[vehicle].m_targetPos1 = Vector4.zero;
                instance.m_vehicles.m_buffer[vehicle].m_targetPos2 = Vector4.zero;
                instance.m_vehicles.m_buffer[vehicle].m_targetPos3 = Vector4.zero;
                instance.m_vehicles.m_buffer[vehicle].m_sourceBuilding = 0;
                instance.m_vehicles.m_buffer[vehicle].m_targetBuilding = 0;
                instance.m_vehicles.m_buffer[vehicle].m_transferType = type;
                instance.m_vehicles.m_buffer[vehicle].m_transferSize = 0;
                instance.m_vehicles.m_buffer[vehicle].m_waitCounter = 0;
                instance.m_vehicles.m_buffer[vehicle].m_blockCounter = 0;
                instance.m_vehicles.m_buffer[vehicle].m_nextGridVehicle = 0;
                instance.m_vehicles.m_buffer[vehicle].m_nextOwnVehicle = 0;
                instance.m_vehicles.m_buffer[vehicle].m_nextGuestVehicle = 0;
                instance.m_vehicles.m_buffer[vehicle].m_nextLineVehicle = 0;
                instance.m_vehicles.m_buffer[vehicle].m_transportLine = 0;
                instance.m_vehicles.m_buffer[vehicle].m_leadingVehicle = 0;
                instance.m_vehicles.m_buffer[vehicle].m_trailingVehicle = 0;
                instance.m_vehicles.m_buffer[vehicle].m_cargoParent = 0;
                instance.m_vehicles.m_buffer[vehicle].m_firstCargo = 0;
                instance.m_vehicles.m_buffer[vehicle].m_nextCargo = 0;
                instance.m_vehicles.m_buffer[vehicle].m_citizenUnits = 0u;
                instance.m_vehicles.m_buffer[vehicle].m_path = 0u;
                instance.m_vehicles.m_buffer[vehicle].m_lastFrame = 0;
                instance.m_vehicles.m_buffer[vehicle].m_pathPositionIndex = 0;
                instance.m_vehicles.m_buffer[vehicle].m_lastPathOffset = 0;
                instance.m_vehicles.m_buffer[vehicle].m_gateIndex = 0;
                instance.m_vehicles.m_buffer[vehicle].m_waterSource = 0;
                instance.m_vehicles.m_buffer[vehicle].m_touristCount = 0;
                instance.m_vehicles.m_buffer[vehicle].m_custom = 0;
                info.m_vehicleAI.CreateVehicle(vehicle, ref instance.m_vehicles.m_buffer[vehicle]);
                info.m_vehicleAI.FrameDataUpdated(vehicle, ref instance.m_vehicles.m_buffer[vehicle], ref instance.m_vehicles.m_buffer[vehicle].m_frame0);
                instance.m_vehicleCount = (int)(instance.m_vehicles.ItemCount() - 1);
                return true;
            }
            vehicle = 0;
            return false;
        }
    }
}
