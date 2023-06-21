using System;
using System.Collections.Generic;
using IndustriesMeetsSunsetHarbor.Managers;
using IndustriesMeetsSunsetHarbor.Utils;

namespace IndustriesMeetsSunsetHarbor.Serializer
{
    public class RestaurantSerializer
    {
        // Some magic values to check we are line up correctly on the tuple boundaries
        private const uint uiTUPLE_START = 0xFEFEFEFE;
        private const uint uiTUPLE_END = 0xFAFAFAFA;

        private const ushort iRESTAURANT_DELIVERIES_DATA_VERSION = 1;

        public static void SaveData(FastList<byte> Data)
        {
            // Write out metadata
            StorageData.WriteUInt16(iRESTAURANT_DELIVERIES_DATA_VERSION, Data);

            StorageData.WriteUInt32(uiTUPLE_START, Data);
            StorageData.WriteInt32(RestaurantManager.RestaurantsDeliveries.Count, Data);

            // Write out each building deliveries settings
            foreach (KeyValuePair<ushort, List<RestaurantManager.RestaurantDeliveryData>> kvp in RestaurantManager.RestaurantsDeliveries)
            {
                // Write start tuple
                StorageData.WriteUInt32(uiTUPLE_START, Data);

                // Write actual settings
                StorageData.WriteUInt16(kvp.Key, Data);
                StorageData.WriteInt32(kvp.Value.Count, Data);
                foreach(var item in kvp.Value)
                {
                    StorageData.WriteUInt16(item.buildingId, Data);
                    StorageData.WriteUInt32(item.citizenId, Data);
                    StorageData.WriteUInt16(item.deliveryVehicleId, Data);
                    StorageData.WriteUInt16(item.restaurantId, Data);
                    StorageData.WriteBool(item.mealCooked, Data);
                    StorageData.WriteInt32(item.mealType, Data);
                }

                // Write end tuple
                StorageData.WriteUInt32(uiTUPLE_END, Data);
            }

            StorageData.WriteUInt32(uiTUPLE_END, Data);

            StorageData.WriteUInt32(uiTUPLE_START, Data);
            StorageData.WriteInt32(RestaurantManager.RestaurantsSitDowns.Count, Data);

            // Write out each building customers line settings
            foreach (KeyValuePair<ushort, List<RestaurantManager.RestaurantSitDownData>> kvp in RestaurantManager.RestaurantsSitDowns)
            {
                // Write start tuple
                StorageData.WriteUInt32(uiTUPLE_START, Data);

                // Write actual settings
                StorageData.WriteUInt16(kvp.Key, Data);
                StorageData.WriteInt32(kvp.Value.Count, Data);
                foreach(var item in kvp.Value)
                {
                    StorageData.WriteUInt32(item.citizenId, Data);
                    StorageData.WriteInt32(item.mealType, Data);
                    StorageData.WriteDateTime(item.enterTime, Data);
                }

                // Write end tuple
                StorageData.WriteUInt32(uiTUPLE_END, Data);
            }

            StorageData.WriteUInt32(uiTUPLE_END, Data);
        }

        public static void LoadData(int iGlobalVersion, byte[] Data, ref int iIndex)
        {
            if (Data != null && Data.Length > iIndex)
            {
                int iRestaurantVersion = StorageData.ReadUInt16(Data, ref iIndex);
                LogHelper.Information("Global: " + iGlobalVersion + " BuildingVersion: " + iRestaurantVersion + " DataLength: " + Data.Length + " Index: " + iIndex);

                if (iRestaurantVersion <= iRESTAURANT_DELIVERIES_DATA_VERSION)
                {
                    CheckStartTuple($"RestaurantDeliveries Start", iRestaurantVersion, Data, ref iIndex);
                    var RestaurantDeliveriesCount = StorageData.ReadInt32(Data, ref iIndex);
                    for (int i = 0; i < RestaurantDeliveriesCount; i++)
                    {
                        CheckStartTuple($"Building({i})", iRestaurantVersion, Data, ref iIndex);
                        ushort restaurantId = StorageData.ReadUInt16(Data, ref iIndex);
                        List<RestaurantManager.RestaurantDeliveryData> RestaurantDeliveryDataList = new();
                        if (Data.Length > iIndex + 4)
                        {
                            int iArrayCount = StorageData.ReadInt32(Data, ref iIndex);
                            if (Data.Length >= iIndex + (iArrayCount * 2))
                            {
                                for (int j = 0; j < iArrayCount; j++)
                                {
                                    ushort buildingId = StorageData.ReadUInt16(Data, ref iIndex);
                                    uint citizenId = StorageData.ReadUInt32(Data, ref iIndex);
                                    ushort deliveryVehicleId = StorageData.ReadUInt16(Data, ref iIndex);
                                    ushort restaurantItemId = StorageData.ReadUInt16(Data, ref iIndex);
                                    bool mealCooked = StorageData.ReadBool(Data, ref iIndex);
                                    int mealType = StorageData.ReadInt32(Data, ref iIndex);
                        
                                    var restaurantDeliveryData = new RestaurantManager.RestaurantDeliveryData
                                    {
                                        deliveryVehicleId = deliveryVehicleId,
                                        buildingId = buildingId,
                                        citizenId = citizenId,
                                        restaurantId = restaurantItemId,
                                        mealCooked = mealCooked,
                                        mealType = mealType
                                    };
                                    RestaurantDeliveryDataList.Add(restaurantDeliveryData);
                                }
                            } 
                            else
                            {
                                LogHelper.Error("Data size not large enough aborting read. ArraySize: " + iArrayCount + " DataSize: " + Data.Length + " Index: " + iIndex);
                            }
                        }
                        RestaurantManager.RestaurantsDeliveries.Add(restaurantId, RestaurantDeliveryDataList);
                        CheckEndTuple($"Building({i})", iRestaurantVersion, Data, ref iIndex);
                    }
                    CheckEndTuple($"RestaurantDeliveries End", iRestaurantVersion, Data, ref iIndex);

                    // ---------------------------------------------------------------------------------------------------------

                    CheckStartTuple($"RestaurantsSitDowns Start", iRestaurantVersion, Data, ref iIndex);
                    var RestaurantsSitDownsCount = StorageData.ReadInt32(Data, ref iIndex);
                    for (int i = 0; i < RestaurantsSitDownsCount; i++)
                    {
                        CheckStartTuple($"Building({i})", iRestaurantVersion, Data, ref iIndex);
                        ushort restaurantId = StorageData.ReadUInt16(Data, ref iIndex);
                        List<RestaurantManager.RestaurantSitDownData> RestaurantSitDownDataList = new();
                        if (Data.Length > iIndex + 4)
                        {
                            int iArrayCount = StorageData.ReadInt32(Data, ref iIndex);
                            if (Data.Length >= iIndex + (iArrayCount * 2))
                            {
                                for (int j = 0; j < iArrayCount; j++)
                                {
                                    uint citizenId = StorageData.ReadUInt32(Data, ref iIndex);
                                    int mealType = StorageData.ReadInt32(Data, ref iIndex);
                                    DateTime enterTime = StorageData.ReadDateTime(Data, ref iIndex);
                        
                                    var restaurantSitDownData = new RestaurantManager.RestaurantSitDownData
                                    {
                                        citizenId = citizenId,
                                        mealType = mealType,
                                        enterTime = enterTime
                                    };
                                    RestaurantSitDownDataList.Add(restaurantSitDownData);
                                }
                            } 
                            else
                            {
                                LogHelper.Error("Data size not large enough aborting read. ArraySize: " + iArrayCount + " DataSize: " + Data.Length + " Index: " + iIndex);
                            }
                        }
                        RestaurantManager.RestaurantsSitDowns.Add(restaurantId, RestaurantSitDownDataList);
                        CheckEndTuple($"Building({i})", iRestaurantVersion, Data, ref iIndex);
                    }
                    CheckEndTuple($"RestaurantsSitDowns End", iRestaurantVersion, Data, ref iIndex);
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
