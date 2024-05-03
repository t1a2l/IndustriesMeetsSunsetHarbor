using IndustriesMeetsSunsetHarbor.Managers;
using System;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.Serializer
{
    public class VehicleFuelManagerSerializer
    {
        // Some magic values to check we are line up correctly on the tuple boundaries
        private const uint uiTUPLE_START = 0xFEFEFEFE;
        private const uint uiTUPLE_END = 0xFAFAFAFA;

        private const ushort iVEHICLE_FUEL_MANAGER_DATA_VERSION = 1;

        public static void SaveData(FastList<byte> Data)
        {
            // Write out metadata
            StorageData.WriteUInt16(iVEHICLE_FUEL_MANAGER_DATA_VERSION, Data);
            StorageData.WriteInt32(VehicleFuelManager.VehiclesFuel.Count, Data);

            // Write out each buffer settings
            foreach (var kvp in VehicleFuelManager.VehiclesFuel)
            {
                // Write start tuple
                StorageData.WriteUInt32(uiTUPLE_START, Data);

                // Write actual settings
                StorageData.WriteUInt16(kvp.Key, Data);
                StorageData.WriteInt32(kvp.Value.CurrentFuelCapacity, Data);
                StorageData.WriteInt32(kvp.Value.MaxFuelCapacity, Data);

                // Write end tuple
                StorageData.WriteUInt32(uiTUPLE_END, Data);
            }
        }

        public static void LoadData(int iGlobalVersion, byte[] Data, ref int iIndex)
        {
            if (Data != null && Data.Length > iIndex)
            {
                int iVehicleFuelManagerVersion = StorageData.ReadUInt16(Data, ref iIndex);
                Debug.Log("Global: " + iGlobalVersion + " BufferVersion: " + iVehicleFuelManagerVersion + " DataLength: " + Data.Length + " Index: " + iIndex);
                if (VehicleFuelManager.VehiclesFuel == null)
                {
                    VehicleFuelManager.VehiclesFuel = [];
                }
                int VehiclesFuel_Count = StorageData.ReadInt32(Data, ref iIndex);
                for (int i = 0; i < VehiclesFuel_Count; i++)
                {
                    CheckStartTuple($"Buffer({i})", iVehicleFuelManagerVersion, Data, ref iIndex);

                    ushort vehicleId = StorageData.ReadUInt16(Data, ref iIndex);

                    int currentFuelCapacity = StorageData.ReadInt32(Data, ref iIndex);

                    int maxFuelCapacity = StorageData.ReadInt32(Data, ref iIndex);

                    var vehicleFuelCapacity = new VehicleFuelManager.VehicleFuelCapacity
                    {
                        CurrentFuelCapacity = currentFuelCapacity,
                        MaxFuelCapacity = maxFuelCapacity
                    };

                    VehicleFuelManager.VehiclesFuel.Add(vehicleId, vehicleFuelCapacity);

                    CheckEndTuple($"Buffer({i})", iVehicleFuelManagerVersion, Data, ref iIndex);
                }
            }
        }

        private static void CheckStartTuple(string sTupleLocation, int iDataVersion, byte[] Data, ref int iIndex)
        {
            if (iDataVersion >= 1)
            {
                uint iTupleStart = StorageData.ReadUInt32(Data, ref iIndex);
                if (iTupleStart != uiTUPLE_START)
                {
                    throw new Exception($"VehiclesFuel Buffer start tuple not found at: {sTupleLocation}");
                }
            }
        }

        private static void CheckEndTuple(string sTupleLocation, int iDataVersion, byte[] Data, ref int iIndex)
        {
            if (iDataVersion >= 1)
            {
                uint iTupleEnd = StorageData.ReadUInt32(Data, ref iIndex);
                if (iTupleEnd != uiTUPLE_END)
                {
                    throw new Exception($"VehiclesFuel Buffer end tuple not found at: {sTupleLocation}");
                }
            }
        }

    }
}
