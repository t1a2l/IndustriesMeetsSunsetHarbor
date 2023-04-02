using ColossalFramework;
using System;
using MoreTransferReasons;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.Utils
{
    public static class CalculateVehicles
    {
        public static void CalculateOwnVehicles(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside)
        {
	    VehicleManager instance = Singleton<VehicleManager>.instance;
	    ushort num = data.m_ownVehicles;
	    int num2 = 0;
	    while (num != 0)
	    {
		if ((ExtendedTransferManager.TransferReason)instance.m_vehicles.m_buffer[num].m_transferType == material)
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
		if ((ExtendedTransferManager.TransferReason)instance.m_vehicles.m_buffer[num].m_transferType == material)
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


    }
}
