using IndustriesMeetsSunsetHarbor.Managers;
using IndustriesMeetsSunsetHarbor.Utils;

namespace IndustriesMeetsSunsetHarbor.Serializer
{
    public class AquacultureExtractorSerializer
    {
        private const ushort iAQUACULTURE_EXTRACTOR_DATA_VERSION = 14;

        public static void SaveData(FastList<byte> Data)
        {
            // Write out metadata
            StorageData.WriteUInt16(iAQUACULTURE_EXTRACTOR_DATA_VERSION, Data);
            StorageData.WriteList(AquacultureExtractorManager.AquacultureExtractorsWithNoFarm, Data);
        }

        public static void LoadData(int iGlobalVersion, byte[] Data, ref int iIndex)
        {
            if (Data != null && Data.Length > iIndex)
            {
                int iAquacultureExtractorVersion = StorageData.ReadUInt16(Data, ref iIndex);
                LogHelper.Information("Global: " + iGlobalVersion + " BuildingVersion: " + iAquacultureExtractorVersion + " DataLength: " + Data.Length + " Index: " + iIndex);

                if (iAquacultureExtractorVersion <= iAQUACULTURE_EXTRACTOR_DATA_VERSION)
                {
                    if(AquacultureExtractorManager.AquacultureExtractorsWithNoFarm == null)
                    {
                        AquacultureExtractorManager.AquacultureExtractorsWithNoFarm = new();
                    }
                    AquacultureExtractorManager.AquacultureExtractorsWithNoFarm = StorageData.ReadList(Data, ref iIndex);
                }
            }
        }

    }
}
