using IndustriesMeetsSunsetHarbor.Managers;
using System;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.Serializer
{
    public class GasStationFuelManagerSerializer
    {
        // Some magic values to check we are line up correctly on the tuple boundaries
        private const uint uiTUPLE_START = 0xFEFEFEFE;
        private const uint uiTUPLE_END = 0xFAFAFAFA;

        private const ushort iGAS_STATION_FUEL_MANAGER_DATA_VERSION = 1;

        public static void SaveData(FastList<byte> Data)
        {
            // Write out metadata
            StorageData.WriteUInt16(iGAS_STATION_FUEL_MANAGER_DATA_VERSION, Data);
            StorageData.WriteInt32(GasStationFuelManager.GasStationsFuel.Count, Data);

            // Write out each buffer settings
            foreach (var kvp in GasStationFuelManager.GasStationsFuel)
            {
                // Write start tuple
                StorageData.WriteUInt32(uiTUPLE_START, Data);

                // Write actual settings
                StorageData.WriteUInt16(kvp.Key, Data);
                StorageData.WriteUInt16(kvp.Value, Data);

                // Write end tuple
                StorageData.WriteUInt32(uiTUPLE_END, Data);
            }
        }

        public static void LoadData(int iGlobalVersion, byte[] Data, ref int iIndex)
        {
            if (Data != null && Data.Length > iIndex)
            {
                int iGasStationManagerVersion = StorageData.ReadUInt16(Data, ref iIndex);
                Debug.Log("Global: " + iGlobalVersion + " BufferVersion: " + iGasStationManagerVersion + " DataLength: " + Data.Length + " Index: " + iIndex);
                if (GasStationFuelManager.GasStationsFuel == null)
                {
                    GasStationFuelManager.GasStationsFuel = [];
                }
                int GasStationsFuel_Count = StorageData.ReadInt32(Data, ref iIndex);
                for (int i = 0; i < GasStationsFuel_Count; i++)
                {
                    CheckStartTuple($"Buffer({i})", iGasStationManagerVersion, Data, ref iIndex);

                    ushort buildingId = StorageData.ReadUInt16(Data, ref iIndex);

                    ushort fuelCapacity = StorageData.ReadUInt16(Data, ref iIndex);

                    GasStationFuelManager.GasStationsFuel.Add(buildingId, fuelCapacity);

                    CheckEndTuple($"Buffer({i})", iGasStationManagerVersion, Data, ref iIndex);
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
                    throw new Exception($"GasStationsFuel Buffer start tuple not found at: {sTupleLocation}");
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
                    throw new Exception($"GasStationsFuel Buffer end tuple not found at: {sTupleLocation}");
                }
            }
        }

    }
}
