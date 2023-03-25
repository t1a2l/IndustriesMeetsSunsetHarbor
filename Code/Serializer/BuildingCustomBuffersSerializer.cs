using System;
using System.Collections.Generic;
using IndustriesMeetsSunsetHarbor.Managers;
using IndustriesMeetsSunsetHarbor.Utils;

namespace IndustriesMeetsSunsetHarbor.Serializer
{
    public class BuildingCustomBuffersSerializer
    {
        // Some magic values to check we are line up correctly on the tuple boundaries
        private const uint uiTUPLE_START = 0xFEFEFEFE;
        private const uint uiTUPLE_END = 0xFAFAFAFA;

        private const ushort iBUILDING_CUSTOM_BUFFERS_DATA_VERSION = 1;

        public static void SaveData(FastList<byte> Data)
        {
            // Write out metadata
            StorageData.WriteUInt16(iBUILDING_CUSTOM_BUFFERS_DATA_VERSION, Data);
            StorageData.WriteInt32(BuildingCustomBuffersManager.BuildingCustomBuffers.Count, Data);

            // Write out each buildings settings
            foreach (KeyValuePair<ushort, BuildingCustomBuffersManager.BuildingCustomBuffer> kvp in BuildingCustomBuffersManager.BuildingCustomBuffers)
            {
                // Write start tuple
                StorageData.WriteUInt32(uiTUPLE_START, Data);

                // Write actual settings
                StorageData.WriteUInt16(kvp.Key, Data);
                StorageData.WriteUInt16(kvp.Value.m_customBuffer1, Data);
                StorageData.WriteUInt16(kvp.Value.m_customBuffer2, Data);
                StorageData.WriteUInt16(kvp.Value.m_customBuffer3, Data);
                StorageData.WriteUInt16(kvp.Value.m_customBuffer4, Data);
                StorageData.WriteUInt16(kvp.Value.m_customBuffer5, Data);
                StorageData.WriteUInt16(kvp.Value.m_customBuffer6, Data);
                StorageData.WriteUInt16(kvp.Value.m_customBuffer7, Data);
                StorageData.WriteUInt16(kvp.Value.m_customBuffer8, Data);
                StorageData.WriteUInt16(kvp.Value.m_customBuffer9, Data);
                StorageData.WriteUInt16(kvp.Value.m_customBuffer10, Data);

                // Write end tuple
                StorageData.WriteUInt32(uiTUPLE_END, Data);
            }
        }

        public static void LoadData(int iGlobalVersion, byte[] Data, ref int iIndex)
        {
            if (Data != null && Data.Length > iIndex)
            {
                int iBuildingCustomBuffersVersion = StorageData.ReadUInt16(Data, ref iIndex);
                LogHelper.Information("Global: " + iGlobalVersion + " BuildingVersion: " + iBuildingCustomBuffersVersion + " DataLength: " + Data.Length + " Index: " + iIndex);

                if (iBuildingCustomBuffersVersion <= iBUILDING_CUSTOM_BUFFERS_DATA_VERSION)
                {
                    if(BuildingCustomBuffersManager.BuildingCustomBuffers == null)
                    {
                        BuildingCustomBuffersManager.BuildingCustomBuffers = new Dictionary<ushort, BuildingCustomBuffersManager.BuildingCustomBuffer>();
                    }
                    var BuildingCustomBuffers_Count = StorageData.ReadInt32(Data, ref iIndex);
                    for (int i = 0; i < BuildingCustomBuffers_Count; i++)
                    {
                        CheckStartTuple($"Building({i})", iBuildingCustomBuffersVersion, Data, ref iIndex);
                        ushort  buildingCustomBuffersId = StorageData.ReadUInt16(Data, ref iIndex);
                        BuildingCustomBuffersManager.BuildingCustomBuffer new_strcut = new();
                        new_strcut.m_customBuffer1 =  StorageData.ReadUInt16(Data, ref iIndex);
                        new_strcut.m_customBuffer2 =  StorageData.ReadUInt16(Data, ref iIndex);
                        new_strcut.m_customBuffer3 =  StorageData.ReadUInt16(Data, ref iIndex);
                        new_strcut.m_customBuffer4 =  StorageData.ReadUInt16(Data, ref iIndex);
                        new_strcut.m_customBuffer5 =  StorageData.ReadUInt16(Data, ref iIndex);
                        new_strcut.m_customBuffer6 =  StorageData.ReadUInt16(Data, ref iIndex);
                        new_strcut.m_customBuffer7 =  StorageData.ReadUInt16(Data, ref iIndex);
                        new_strcut.m_customBuffer8 =  StorageData.ReadUInt16(Data, ref iIndex);
                        new_strcut.m_customBuffer9 =  StorageData.ReadUInt16(Data, ref iIndex);
                        new_strcut.m_customBuffer10 =  StorageData.ReadUInt16(Data, ref iIndex);
                        BuildingCustomBuffersManager.BuildingCustomBuffers.Add(buildingCustomBuffersId, new_strcut);
                        CheckEndTuple($"Building({i})", iBuildingCustomBuffersVersion, Data, ref iIndex);
                    }
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
                    throw new Exception($"Building start tuple not found at: {sTupleLocation}");
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
                    throw new Exception($"Building end tuple not found at: {sTupleLocation}");
                }
            }
        }

    }
}
