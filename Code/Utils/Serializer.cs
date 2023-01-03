using System;
using System.Collections.Generic;
using ICities;
using IndustriesMeetsSunsetHarbor.Managers;

namespace IndustriesMeetsSunsetHarbor.Utils
{
    public class Serializer : ISerializableDataExtension
    {
        // Some magic values to check we are line up correctly on the tuple boundaries
        private const uint uiTUPLE_START = 0xFEFEFEFE;
        private const uint uiTUPLE_END = 0xFAFAFAFA;

        public const ushort DataVersion = 1;
        public const string DataID = "IndustriesMeetsSunsetHarbor";

        public static Serializer instance = null;
        private ISerializableData m_serializableData = null;

        public void OnCreated(ISerializableData serializedData)
        {
            instance = this;
            m_serializableData = serializedData;
        }

        public void OnSaveData()
        {
            FastList<byte> Data = new();

            StorageData.WriteUInt32(uiTUPLE_START, Data);
            // Write out metadata
            StorageData.WriteUInt16(DataVersion, Data);
            StorageData.WriteUInt16(AquacultureFarmManager.AquacultureFarms.Count, Data);

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

            StorageData.WriteUInt32(uiTUPLE_END, Data);

            StorageData.WriteUInt32(uiTUPLE_START, Data);

            StorageData.WriteUInt16(ResourceMarketManager.MarketBuffers.Count, Data);

            foreach (KeyValuePair<ushort, ResourceMarketManager.MarketData> kvp in ResourceMarketManager.MarketBuffers)
            {
                // Write start tuple
                StorageData.WriteUInt32(uiTUPLE_START, Data);

                // Write actual settings
                StorageData.WriteUInt16(kvp.Key, Data);
                StorageData.WriteUInt16(kvp.Value.inputAmountBuffer.Length, Data);
                StorageData.WriteUInt16ArrayWithoutLength(kvp.Value.inputAmountBuffer, Data);
                StorageData.WriteUInt16(kvp.Value.outputAmountBuffer.Length, Data);
                StorageData.WriteUInt16ArrayWithoutLength(kvp.Value.outputAmountBuffer, Data);
                StorageData.WriteUInt16(kvp.Value.amountSold1.Length, Data);
                StorageData.WriteUInt16ArrayWithoutLength(kvp.Value.amountSold1, Data);
                StorageData.WriteUInt16(kvp.Value.amountSold2.Length, Data);
                StorageData.WriteUInt16ArrayWithoutLength(kvp.Value.amountSold2, Data);

                // Write end tuple
                StorageData.WriteUInt32(uiTUPLE_END, Data);
            }

             StorageData.WriteUInt32(uiTUPLE_END, Data);
        }

        public void OnLoadData()
        {
            byte[] Data = m_serializableData.LoadData(DataID);
            if (Data != null && Data.Length > 0)
            {
                 ushort SaveGameFileVersion;
                int Index = 0;

                CheckStartTuple(DataID, DataVersion, Data, ref Index);

                SaveGameFileVersion = StorageData.ReadUInt16(Data, ref Index);
                if(SaveGameFileVersion != DataVersion)
                {
                    return;
                }

                var AquacultureFarms_Count = StorageData.ReadUInt16(Data, ref Index);

                for (int i = 0; i < AquacultureFarms_Count; i++)
                {
                    CheckStartTuple(DataID, DataVersion, Data, ref Index);
                    ushort aquaculturerFarmId = StorageData.ReadUInt16(Data, ref Index);
                    List<ushort> aquaculturerFarmExtractors = StorageData.ReadList(Data, ref Index);
                    AquacultureFarmManager.AquacultureFarms[aquaculturerFarmId] = aquaculturerFarmExtractors;
                    CheckEndTuple(DataID, DataVersion, Data, ref Index);
                }

                CheckEndTuple(DataID, DataVersion, Data, ref Index);

                CheckStartTuple(DataID, DataVersion, Data, ref Index);

                var MarketBuffers_Count = StorageData.ReadUInt16(Data, ref Index);

                for (int i = 0; i < MarketBuffers_Count; i++)
                {
                    CheckStartTuple(DataID, DataVersion, Data, ref Index);
                    ushort marketId = StorageData.ReadUInt16(Data, ref Index);
                    int inputAmountBuffer_length = StorageData.ReadUInt16(Data, ref Index);
                    ushort[] inputAmountBuffer = StorageData.ReadUInt16ArrayWithoutLength(Data, ref Index, inputAmountBuffer_length);
                    int outputAmountBuffer_length = StorageData.ReadUInt16(Data, ref Index);
                    ushort[] outputAmountBuffer = StorageData.ReadUInt16ArrayWithoutLength(Data, ref Index, outputAmountBuffer_length);
                    int amountSold1_length = StorageData.ReadUInt16(Data, ref Index);
                    ushort[] amountSold1 = StorageData.ReadUInt16ArrayWithoutLength(Data, ref Index, amountSold1_length);
                    int amountSold2_length = StorageData.ReadUInt16(Data, ref Index);
                    ushort[] amountSold2 = StorageData.ReadUInt16ArrayWithoutLength(Data, ref Index, amountSold2_length);
                    var marketData = new ResourceMarketManager.MarketData
                    {
                        inputAmountBuffer = inputAmountBuffer,
                        outputAmountBuffer = outputAmountBuffer,
                        amountSold1 = amountSold1,
                        amountSold2 = amountSold2
                    };
                    ResourceMarketManager.MarketBuffers[marketId] = marketData;
                    CheckEndTuple(DataID, DataVersion, Data, ref Index);
                }

                CheckEndTuple(DataID, DataVersion, Data, ref Index);
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


        public void OnReleased()
        {
            Serializer.instance = (Serializer)null;
        }
    }
}