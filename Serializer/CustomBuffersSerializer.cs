using System;
using System.Collections.Generic;
using IndustriesMeetsSunsetHarbor.Managers;
using IndustriesMeetsSunsetHarbor.Utils;

namespace IndustriesMeetsSunsetHarbor.Serializer
{
    public class CustomBuffersSerializer
    {
        // Some magic values to check we are line up correctly on the tuple boundaries
        private const uint uiTUPLE_START = 0xFEFEFEFE;
        private const uint uiTUPLE_END = 0xFAFAFAFA;

        private const ushort iCUSTOM_BUFFERS_DATA_VERSION = 1;

        public static void SaveData(FastList<byte> Data)
        {
            // Write out metadata
            StorageData.WriteUInt16(iCUSTOM_BUFFERS_DATA_VERSION, Data);
            StorageData.WriteInt32(CustomBuffersManager.CustomBuffers.Count, Data);

            // Write out each buffer settings
            foreach (KeyValuePair<ushort, CustomBuffersManager.CustomBuffer> kvp in CustomBuffersManager.CustomBuffers)
            {
                // Write start tuple
                StorageData.WriteUInt32(uiTUPLE_START, Data);

                // Write actual settings
                StorageData.WriteUInt16(kvp.Key, Data);

                StorageData.WriteInt32(kvp.Value.m_volumes.Length, Data);

                StorageData.WriteFloatArrayWithoutLength(kvp.Value.m_volumes, Data);

                // Write end tuple
                StorageData.WriteUInt32(uiTUPLE_END, Data);
            }
        }

        public static void LoadData(int iGlobalVersion, byte[] Data, ref int iIndex)
        {
            if (Data != null && Data.Length > iIndex)
            {
                int iCustomBuffersVersion = StorageData.ReadUInt16(Data, ref iIndex);
                LogHelper.Information("Global: " + iGlobalVersion + " BufferVersion: " + iCustomBuffersVersion + " DataLength: " + Data.Length + " Index: " + iIndex);
                CustomBuffersManager.CustomBuffers = [];
                var CustomBuffers_Count = StorageData.ReadInt32(Data, ref iIndex);

                for (int i = 0; i < CustomBuffers_Count; i++)
                {
                    CheckStartTuple($"Buffer({i})", iCustomBuffersVersion, Data, ref iIndex);
                    ushort buildingId = StorageData.ReadUInt16(Data, ref iIndex);
                    int volumesLength = StorageData.ReadInt32(Data, ref iIndex);
                    CustomBuffersManager.CustomBuffer new_strcut = new()
                    {
                        m_volumes = StorageData.ReadFloatArrayWithoutLength(Data, ref iIndex, volumesLength)
                    };
                    CustomBuffersManager.CustomBuffers.Add(buildingId, new_strcut);
                    CheckEndTuple($"Buffer({i})", iCustomBuffersVersion, Data, ref iIndex);
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
                    throw new Exception($"CustomBuffer start tuple not found at: {sTupleLocation}");
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
                    throw new Exception($"CustomBuffer end tuple not found at: {sTupleLocation}");
                }
            }
        }

    }
}
