using System;
using System.Collections.Generic;
using IndustriesMeetsSunsetHarbor.Managers;

namespace IndustriesMeetsSunsetHarbor.Serializer
{
    public class AquacultureFarmSerializer
    {
        // Some magic values to check we are line up correctly on the tuple boundaries
        private const uint uiTUPLE_START = 0xFEFEFEFE;
        private const uint uiTUPLE_END = 0xFAFAFAFA;

        private const ushort iAQUACULTURE_FARM_DATA_VERSION = 14;

        public static void SaveData(FastList<byte> Data)
        {
            // Write out metadata
            StorageData.WriteUInt16(iAQUACULTURE_FARM_DATA_VERSION, Data);
            StorageData.WriteInt32(AquacultureFarmManager.AquacultureFarms.Count, Data);

            // Write out each buildings settings
            foreach (KeyValuePair<ushort, List<ushort>> kvp in AquacultureFarmManager.AquacultureFarms)
            {
                // Write start tuple
                StorageData.WriteUInt32(uiTUPLE_START, Data);

                // Write actual settings
                StorageData.WriteUInt16(kvp.Key, Data);
                StorageData.WriteList(kvp.Value, Data);

                // Write end tuple
                StorageData.WriteUInt32(uiTUPLE_END, Data);
            }
        }

        public static void LoadData(int iGlobalVersion, byte[] Data, ref int iIndex)
        {
            if (Data != null && Data.Length > iIndex)
            {
                int iAquacultureFarmVersion = StorageData.ReadUInt16(Data, ref iIndex);
                LogHelper.Information("Global: " + iGlobalVersion + " BuildingVersion: " + iAquacultureFarmVersion + " DataLength: " + Data.Length + " Index: " + iIndex);

                if (iAquacultureFarmVersion <= iAQUACULTURE_FARM_DATA_VERSION)
                {
                    var AquacultureFarms_Count = StorageData.ReadInt32(Data, ref iIndex);
                    for (int i = 0; i < AquacultureFarms_Count; i++)
                    {
                        CheckStartTuple($"Building({i})", iAquacultureFarmVersion, Data, ref iIndex);
                        ushort aquaculturerFarmId = StorageData.ReadUInt16(Data, ref iIndex);
                        List<ushort> aquaculturerFarmExtractors = StorageData.ReadList(Data, ref iIndex);
                        AquacultureFarmManager.AquacultureFarms.Add(aquaculturerFarmId, aquaculturerFarmExtractors);
                        CheckEndTuple($"Building({i})", iAquacultureFarmVersion, Data, ref iIndex);
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
                uint iTupleStart = StorageData.ReadUInt32(Data, ref iIndex);
                if (iTupleStart != uiTUPLE_END)
                {
                    throw new Exception($"Building end tuple not found at: {sTupleLocation}");
                }
            }
        }

    }
}
