using System;
using IndustriesMeetsSunsetHarbor.Managers;
using IndustriesMeetsSunsetHarbor.Utils;

namespace IndustriesMeetsSunsetHarbor.Serializer
{
    public class RestaurantDeliveriesSerializer
    {
        // Some magic values to check we are line up correctly on the tuple boundaries
        private const uint uiTUPLE_START = 0xFEFEFEFE;
        private const uint uiTUPLE_END = 0xFAFAFAFA;

        private const ushort iRESTAURANT_DELIVERIES_DATA_VERSION = 1;

        public static void SaveData(FastList<byte> Data)
        {
            // Write out metadata
            StorageData.WriteUInt16(iRESTAURANT_DELIVERIES_DATA_VERSION, Data);
            StorageData.WriteInt32(RestaurantDeliveriesManager.RestaurantDeliveries.Count, Data);

            // Write out each buildings settings
            foreach (var item in RestaurantDeliveriesManager.RestaurantDeliveries)
            {
                // Write start tuple
                StorageData.WriteUInt32(uiTUPLE_START, Data);

                // Write actual settings
                StorageData.WriteUInt16(item.deliveryVehicleId, Data);
                StorageData.WriteUInt16(item.buildingId, Data);
                StorageData.WriteUInt32(item.citizenId, Data);

                // Write end tuple
                StorageData.WriteUInt32(uiTUPLE_END, Data);
            }
        }

        public static void LoadData(int iGlobalVersion, byte[] Data, ref int iIndex)
        {
            if (Data != null && Data.Length > iIndex)
            {
                int iRestaurantDeliveriesVersion = StorageData.ReadUInt16(Data, ref iIndex);
                LogHelper.Information("Global: " + iGlobalVersion + " BuildingVersion: " + iRestaurantDeliveriesVersion + " DataLength: " + Data.Length + " Index: " + iIndex);

                if (iRestaurantDeliveriesVersion <= iRESTAURANT_DELIVERIES_DATA_VERSION)
                {
                    if(RestaurantDeliveriesManager.RestaurantDeliveries == null)
                    {
                        RestaurantDeliveriesManager.RestaurantDeliveries = new();
                    }
                    var RestaurantDeliveries_Count = StorageData.ReadInt32(Data, ref iIndex);
                    for (int i = 0; i < RestaurantDeliveries_Count; i++)
                    {
                        CheckStartTuple($"Building({i})", iRestaurantDeliveriesVersion, Data, ref iIndex);
                        ushort deliveryVehicleId = StorageData.ReadUInt16(Data, ref iIndex);
                        ushort buildingId = StorageData.ReadUInt16(Data, ref iIndex);
                        uint citizenId = StorageData.ReadUInt32(Data, ref iIndex);
                        var restaurantDeliveryData = new RestaurantDeliveriesManager.RestaurantDeliveryData
                        {
                            deliveryVehicleId = deliveryVehicleId,
                            buildingId = buildingId,
                            citizenId = citizenId
                        };
                        RestaurantDeliveriesManager.RestaurantDeliveries.Add(restaurantDeliveryData);
                        CheckEndTuple($"Building({i})", iRestaurantDeliveriesVersion, Data, ref iIndex);
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
