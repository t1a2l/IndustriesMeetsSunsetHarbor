using System;
using ICities;
using IndustriesMeetsSunsetHarbor.Utils;

namespace IndustriesMeetsSunsetHarbor.Serializer
{
    public class IndustriesMeetsSunsetHarborSerializer : ISerializableDataExtension
    {
        // Some magic values to check we are line up correctly on the tuple boundaries
        private const uint uiTUPLE_START = 0xFEFEFEFE;
        private const uint uiTUPLE_END = 0xFAFAFAFA;

        public const ushort DataVersion = 1;
        public const string DataID = "IndustriesMeetsSunsetHarbor";

        public static IndustriesMeetsSunsetHarborSerializer instance = null;
        private ISerializableData m_serializableData = null;

        public void OnCreated(ISerializableData serializedData)
        {
            instance = this;
            m_serializableData = serializedData;
        }

        public void OnLoadData()
        {
            try
            {
                if (m_serializableData != null)
                {
                    byte[] Data = m_serializableData.LoadData(DataID);
                    if (Data != null && Data.Length > 0)
                    {
                        ushort SaveGameFileVersion;
                        int Index = 0;

                        SaveGameFileVersion = StorageData.ReadUInt16(Data, ref Index);

                        LogHelper.Information("Data length: " + Data.Length.ToString() + "; Data Version: " + SaveGameFileVersion);

                        if (SaveGameFileVersion <= DataVersion)
                        {
                            CheckStartTuple("AquacultureFarmSerializer", SaveGameFileVersion, Data, ref Index);
                            AquacultureFarmSerializer.LoadData(SaveGameFileVersion, Data, ref Index);
                            CheckEndTuple("AquacultureFarmSerializer", SaveGameFileVersion, Data, ref Index);

                            CheckStartTuple("MarketBuffersSerializer", SaveGameFileVersion, Data, ref Index);
                            MarketBuffersSerializer.LoadData(SaveGameFileVersion, Data, ref Index);
                            CheckEndTuple("MarketBuffersSerializer", SaveGameFileVersion, Data, ref Index);

                            CheckStartTuple("MarketBuffersSerializer", SaveGameFileVersion, Data, ref Index);
                            AquacultureExtractorSerializer.LoadData(SaveGameFileVersion, Data, ref Index);
                            CheckEndTuple("MarketBuffersSerializer", SaveGameFileVersion, Data, ref Index);

                            CheckStartTuple("BuildingCustomBuffersSerializer", SaveGameFileVersion, Data, ref Index);
                            BuildingCustomBuffersSerializer.LoadData(SaveGameFileVersion, Data, ref Index);
                            CheckEndTuple("BuildingCustomBuffersSerializer", SaveGameFileVersion, Data, ref Index);
                        }
                        else
                        {
                            string sMessage = "This saved game was saved with a newer version of Industries Meets Sunset Harbor.\r\n";
                            sMessage += "\r\n";
                            sMessage += "Unable to load settings.\r\n";
                            sMessage += "\r\n";
                            sMessage += "Saved game data version: " + SaveGameFileVersion + "\r\n";
                            sMessage += "MOD data version: " + DataVersion + "\r\n";
                            LogHelper.Information(sMessage);
                        }
                    }
                    else
                    {
                         LogHelper.Information("Data is null");
                    }
                }
                else
                {
                     LogHelper.Information("m_serializableData is null");
                }
            }
            catch (Exception ex)
            {
                string sErrorMessage = "Loading of Industries Meets Sunset Harbor save game settings failed with the following error:\r\n";
                sErrorMessage += "\r\n";
                sErrorMessage += ex.Message;
                LogHelper.Error(sErrorMessage);
            }
        }

        public void OnSaveData()
        {
            LogHelper.Information("OnSaveData - Start");
            try
            {
                if (m_serializableData != null)
                {
                    FastList<byte> Data = new();
                    // Always write out data version first
                    StorageData.WriteUInt16(DataVersion, Data);

                    // AquacultureFarm settings
                    StorageData.WriteUInt32(uiTUPLE_START, Data);
                    AquacultureFarmSerializer.SaveData(Data);
                    StorageData.WriteUInt32(uiTUPLE_END, Data);

                    // MarketBuffers settings
                    StorageData.WriteUInt32(uiTUPLE_START, Data);
                    MarketBuffersSerializer.SaveData(Data);
                    StorageData.WriteUInt32(uiTUPLE_END, Data);

                    // AquacultureExtractorsWithNoFarm settings
                    StorageData.WriteUInt32(uiTUPLE_START, Data);
                    AquacultureExtractorSerializer.SaveData(Data);
                    StorageData.WriteUInt32(uiTUPLE_END, Data);

                    // BuildingCustomBuffers settings
                    StorageData.WriteUInt32(uiTUPLE_START, Data);
                    BuildingCustomBuffersSerializer.SaveData(Data);
                    StorageData.WriteUInt32(uiTUPLE_END, Data);

                    m_serializableData.SaveData(DataID, Data.ToArray());
                }
            }
            catch (Exception ex)
            {
                LogHelper.Information("Could not save data. " + ex.Message);
            }
            LogHelper.Information("OnSaveData - Finish");
        }

        private void CheckStartTuple(string sTupleLocation, int iDataVersion, byte[] Data, ref int iIndex)
        {
            if (iDataVersion >= 1)
            {
                uint iTupleStart = StorageData.ReadUInt32(Data, ref iIndex);
                if (iTupleStart != uiTUPLE_START)
                {
                    throw new Exception($"Start tuple not found at: {sTupleLocation}");
                }
            }
        }

        private void CheckEndTuple(string sTupleLocation, int iDataVersion, byte[] Data, ref int iIndex)
        {
            if (iDataVersion >= 1)
            {
                uint iTupleEnd = StorageData.ReadUInt32(Data, ref iIndex);
                if (iTupleEnd != uiTUPLE_END)
                {
                    throw new Exception($"End tuple not found at: {sTupleLocation}");
                }
            }
        }

        public void OnReleased()
        {
            instance = null;
        }

    }
}