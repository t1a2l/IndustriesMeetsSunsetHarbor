using System;
using System.Collections.Generic;
using IndustriesMeetsSunsetHarbor.Managers;
using IndustriesMeetsSunsetHarbor.Utils;

namespace IndustriesMeetsSunsetHarbor.Serializer
{
    public class FarmingParkDataSerializer
    {
        // Some magic values to check we are line up correctly on the tuple boundaries
        private const uint uiTUPLE_START = 0xFEFEFEFE;
        private const uint uiTUPLE_END = 0xFAFAFAFA;

        private const ushort iFARMING_PARK_DATA_VERSION = 1;

        public static void SaveData(FastList<byte> Data)
        {
            // Write out metadata
            StorageData.WriteUInt16(iFARMING_PARK_DATA_VERSION, Data);
            StorageData.WriteInt32(FarmingParkDataManager.FarmingParks.Count, Data);

            // Write out each buffer settings
            foreach (KeyValuePair<byte, FarmingParkDataManager.FarmingParkData> kvp in FarmingParkDataManager.FarmingParks)
            {
                // Write start tuple
                StorageData.WriteUInt32(uiTUPLE_START, Data);

                // Write actual settings
                StorageData.WriteByte(kvp.Key, Data);

                WriteResourceData(kvp.Value.m_fruitsData, Data);
                WriteResourceData(kvp.Value.m_vegetablesData, Data);
                WriteResourceData(kvp.Value.m_cottonData, Data);
                WriteResourceData(kvp.Value.m_cowsData, Data);
                WriteResourceData(kvp.Value.m_highlandCowsData, Data);
                WriteResourceData(kvp.Value.m_sheepData, Data);
                WriteResourceData(kvp.Value.m_pigsData, Data);
                WriteResourceData(kvp.Value.m_milkData, Data);
                WriteResourceData(kvp.Value.m_woolData, Data);
                WriteResourceData(kvp.Value.m_porkData, Data);

                // Write end tuple
                StorageData.WriteUInt32(uiTUPLE_END, Data);
            }
        }

        public static void LoadData(int iGlobalVersion, byte[] Data, ref int iIndex)
        {
            if (Data != null && Data.Length > iIndex)
            {
                int iFarmingParkDataVersion = StorageData.ReadUInt16(Data, ref iIndex);
                LogHelper.Information("Global: " + iGlobalVersion + " BufferVersion: " + iFarmingParkDataVersion + " DataLength: " + Data.Length + " Index: " + iIndex);
                FarmingParkDataManager.FarmingParks = [];
                var FarmingParkData_Count = StorageData.ReadInt32(Data, ref iIndex);
                for (int i = 0; i < FarmingParkData_Count; i++)
                {
                    CheckStartTuple($"Buffer({i})", iFarmingParkDataVersion, Data, ref iIndex);
                    byte parkId = StorageData.ReadByte(Data, ref iIndex);
                    FarmingParkDataManager.FarmingParkData new_strcut = new()
                    {
                        m_fruitsData = ReadResourceData(Data, ref iIndex),
                        m_vegetablesData = ReadResourceData(Data, ref iIndex),
                        m_cottonData = ReadResourceData(Data, ref iIndex),
                        m_cowsData = ReadResourceData(Data, ref iIndex),
                        m_highlandCowsData = ReadResourceData(Data, ref iIndex),
                        m_sheepData = ReadResourceData(Data, ref iIndex),
                        m_pigsData = ReadResourceData(Data, ref iIndex),
                        m_milkData = ReadResourceData(Data, ref iIndex),
                        m_woolData = ReadResourceData(Data, ref iIndex),
                        m_porkData = ReadResourceData(Data, ref iIndex)
                    };
                    FarmingParkDataManager.FarmingParks.Add(parkId, new_strcut);
                    CheckEndTuple($"Buffer({i})", iFarmingParkDataVersion, Data, ref iIndex);
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
                    throw new Exception($"FarmingParkData Buffer start tuple not found at: {sTupleLocation}");
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
                    throw new Exception($"FarmingParkData Buffer end tuple not found at: {sTupleLocation}");
                }
            }
        }

        private static void WriteResourceData(DistrictAreaResourceData d, FastList<byte> Data)
        {
            StorageData.WriteUInt32(d.m_tempConsumption, Data);
            StorageData.WriteUInt32(d.m_tempProduction, Data);
            StorageData.WriteUInt32(d.m_tempBufferAmount, Data);
            StorageData.WriteUInt32(d.m_tempIncomingTransfer, Data);
            StorageData.WriteUInt32(d.m_tempBufferCapacity, Data);
            StorageData.WriteUInt32(d.m_tempImport, Data);
            StorageData.WriteUInt32(d.m_tempExport, Data);
            StorageData.WriteUInt32(d.m_finalConsumption, Data);
            StorageData.WriteUInt32(d.m_finalProduction, Data);
            StorageData.WriteUInt32(d.m_finalBufferAmount, Data);
            StorageData.WriteUInt32(d.m_finalIncomingTransfer, Data);
            StorageData.WriteUInt32(d.m_finalBufferCapacity, Data);
            StorageData.WriteUInt32(d.m_finalImport, Data);
            StorageData.WriteUInt32(d.m_finalExport, Data);
        }

        private static DistrictAreaResourceData ReadResourceData(byte[] Data, ref int iIndex)
        {
            return new()
            {
                m_tempConsumption = StorageData.ReadUInt32(Data, ref iIndex),
                m_tempProduction = StorageData.ReadUInt32(Data, ref iIndex),
                m_tempBufferAmount = StorageData.ReadUInt32(Data, ref iIndex),
                m_tempIncomingTransfer = StorageData.ReadUInt32(Data, ref iIndex),
                m_tempBufferCapacity = StorageData.ReadUInt32(Data, ref iIndex),
                m_tempImport = StorageData.ReadUInt32(Data, ref iIndex),
                m_tempExport = StorageData.ReadUInt32(Data, ref iIndex),
                m_finalConsumption = StorageData.ReadUInt32(Data, ref iIndex),
                m_finalProduction = StorageData.ReadUInt32(Data, ref iIndex),
                m_finalBufferAmount = StorageData.ReadUInt32(Data, ref iIndex),
                m_finalIncomingTransfer = StorageData.ReadUInt32(Data, ref iIndex),
                m_finalBufferCapacity = StorageData.ReadUInt32(Data, ref iIndex),
                m_finalImport = StorageData.ReadUInt32(Data, ref iIndex),
                m_finalExport = StorageData.ReadUInt32(Data, ref iIndex)
            };

        }

    }
}
