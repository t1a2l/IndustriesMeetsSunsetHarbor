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
                StorageData.WriteFloat(kvp.Value.m_customBuffer1, Data);
                StorageData.WriteFloat(kvp.Value.m_customBuffer2, Data);
                StorageData.WriteFloat(kvp.Value.m_customBuffer3, Data);
                StorageData.WriteFloat(kvp.Value.m_customBuffer4, Data);
                StorageData.WriteFloat(kvp.Value.m_customBuffer5, Data);
                StorageData.WriteFloat(kvp.Value.m_customBuffer6, Data);
                StorageData.WriteFloat(kvp.Value.m_customBuffer7, Data);
                StorageData.WriteFloat(kvp.Value.m_customBuffer8, Data);
                StorageData.WriteFloat(kvp.Value.m_customBuffer9, Data);
                StorageData.WriteFloat(kvp.Value.m_customBuffer10, Data);
                StorageData.WriteFloat(kvp.Value.m_customBuffer11, Data);
                StorageData.WriteFloat(kvp.Value.m_customBuffer12, Data);
                StorageData.WriteFloat(kvp.Value.m_customBuffer13, Data);
                StorageData.WriteFloat(kvp.Value.m_customBuffer14, Data);
                StorageData.WriteFloat(kvp.Value.m_customBuffer15, Data);
                StorageData.WriteFloat(kvp.Value.m_customBuffer16, Data);
                StorageData.WriteFloat(kvp.Value.m_customBuffer17, Data);
                StorageData.WriteFloat(kvp.Value.m_customBuffer18, Data);
                StorageData.WriteFloat(kvp.Value.m_customBuffer19, Data);
                StorageData.WriteFloat(kvp.Value.m_customBuffer20, Data);

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

                if (iCustomBuffersVersion <= iCUSTOM_BUFFERS_DATA_VERSION)
                {
                    if(CustomBuffersManager.CustomBuffers == null)
                    {
                        CustomBuffersManager.CustomBuffers = new Dictionary<ushort, CustomBuffersManager.CustomBuffer>();
                    }
                    var CustomBuffers_Count = StorageData.ReadInt32(Data, ref iIndex);
                    for (int i = 0; i < CustomBuffers_Count; i++)
                    {
                        CheckStartTuple($"Buffer({i})", iCustomBuffersVersion, Data, ref iIndex);
                        ushort customBuffersId = StorageData.ReadUInt16(Data, ref iIndex);
                        CustomBuffersManager.CustomBuffer new_strcut = new();
                        new_strcut.m_customBuffer1 = StorageData.ReadFloat(Data, ref iIndex);
                        new_strcut.m_customBuffer2 = StorageData.ReadFloat(Data, ref iIndex);
                        new_strcut.m_customBuffer3 = StorageData.ReadFloat(Data, ref iIndex);
                        new_strcut.m_customBuffer4 = StorageData.ReadFloat(Data, ref iIndex);
                        new_strcut.m_customBuffer5 = StorageData.ReadFloat(Data, ref iIndex);
                        new_strcut.m_customBuffer6 = StorageData.ReadFloat(Data, ref iIndex);
                        new_strcut.m_customBuffer7 = StorageData.ReadFloat(Data, ref iIndex);
                        new_strcut.m_customBuffer8 = StorageData.ReadFloat(Data, ref iIndex);
                        new_strcut.m_customBuffer9 = StorageData.ReadFloat(Data, ref iIndex);
                        new_strcut.m_customBuffer10 = StorageData.ReadFloat(Data, ref iIndex);
                        new_strcut.m_customBuffer11 = StorageData.ReadFloat(Data, ref iIndex);
                        new_strcut.m_customBuffer12 = StorageData.ReadFloat(Data, ref iIndex);
                        new_strcut.m_customBuffer13 = StorageData.ReadFloat(Data, ref iIndex);
                        new_strcut.m_customBuffer14 = StorageData.ReadFloat(Data, ref iIndex);
                        new_strcut.m_customBuffer15 = StorageData.ReadFloat(Data, ref iIndex);
                        new_strcut.m_customBuffer16 = StorageData.ReadFloat(Data, ref iIndex);
                        new_strcut.m_customBuffer17 = StorageData.ReadFloat(Data, ref iIndex);
                        new_strcut.m_customBuffer18 = StorageData.ReadFloat(Data, ref iIndex);
                        new_strcut.m_customBuffer19 = StorageData.ReadFloat(Data, ref iIndex);
                        new_strcut.m_customBuffer20 = StorageData.ReadFloat(Data, ref iIndex);
                        CustomBuffersManager.CustomBuffers.Add(customBuffersId, new_strcut);
                        CheckEndTuple($"Buffer({i})", iCustomBuffersVersion, Data, ref iIndex);
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
                    throw new Exception($"Buffer start tuple not found at: {sTupleLocation}");
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
                    throw new Exception($"Buffer end tuple not found at: {sTupleLocation}");
                }
            }
        }

    }
}
