using System;
using System.Collections.Generic;
using IndustriesMeetsSunsetHarbor.Managers;

namespace IndustriesMeetsSunsetHarbor.Serializer
{
    public class MarketBuffersSerializer
    {
        // Some magic values to check we are line up correctly on the tuple boundaries
        private const uint uiTUPLE_START = 0xFEFEFEFE;
        private const uint uiTUPLE_END = 0xFAFAFAFA;

        public static void SaveData(FastList<byte> Data)
        {
            // Write out metadata
            StorageData.WriteInt32(IndustriesMeetsSunsetHarborSerializer.DataVersion, Data);
            StorageData.WriteInt32(ResourceMarketManager.MarketBuffers.Count, Data);

            // Write out each buildings settings
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
        }

        public static void LoadData(int iGlobalVersion, byte[] Data, ref int iIndex)
        {
            if (Data != null && Data.Length > iIndex)
            {
                int iMarketBuffersVersion = StorageData.ReadInt32(Data, ref iIndex);
                LogHelper.Information("Global: " + iGlobalVersion + " BuildingVersion: " + iMarketBuffersVersion + " DataLength: " + Data.Length + " Index: " + iIndex);

                if (iMarketBuffersVersion <= IndustriesMeetsSunsetHarborSerializer.DataVersion)
                {
                    var MarketBuffers_Count = StorageData.ReadUInt16(Data, ref iIndex);
                    for (int i = 0; i < MarketBuffers_Count; i++)
                    {
                        CheckStartTuple($"Building({i})", iMarketBuffersVersion, Data, ref iIndex);
                        ushort marketId = StorageData.ReadUInt16(Data, ref iIndex);
                        int inputAmountBuffer_length = StorageData.ReadUInt16(Data, ref iIndex);
                        ushort[] inputAmountBuffer = StorageData.ReadUInt16ArrayWithoutLength(Data, ref iIndex, inputAmountBuffer_length);
                        int outputAmountBuffer_length = StorageData.ReadUInt16(Data, ref iIndex);
                        ushort[] outputAmountBuffer = StorageData.ReadUInt16ArrayWithoutLength(Data, ref iIndex, outputAmountBuffer_length);
                        int amountSold1_length = StorageData.ReadUInt16(Data, ref iIndex);
                        ushort[] amountSold1 = StorageData.ReadUInt16ArrayWithoutLength(Data, ref iIndex, amountSold1_length);
                        int amountSold2_length = StorageData.ReadUInt16(Data, ref iIndex);
                        ushort[] amountSold2 = StorageData.ReadUInt16ArrayWithoutLength(Data, ref iIndex, amountSold2_length);
                        var marketData = new ResourceMarketManager.MarketData
                        {
                            inputAmountBuffer = inputAmountBuffer,
                            outputAmountBuffer = outputAmountBuffer,
                            amountSold1 = amountSold1,
                            amountSold2 = amountSold2
                        };
                        ResourceMarketManager.MarketBuffers.Add(marketId, marketData);
                        CheckEndTuple($"Building({i})", iMarketBuffersVersion, Data, ref iIndex);
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
