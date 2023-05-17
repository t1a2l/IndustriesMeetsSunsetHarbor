using System.Collections.Generic;

namespace IndustriesMeetsSunsetHarbor.Managers
{
    public static class RestaurantDeliveriesManager
    {

        public struct RestaurantDeliveryData
        {
            public ushort deliveryVehicleId;
            public ushort buildingId;
            public uint citizenId;
        }

        public static List<RestaurantDeliveryData> RestaurantDeliveries;


        public static void Init()
        {
            if(RestaurantDeliveries == null)
            {
                RestaurantDeliveries = new();
            }
        }

        public static void Deinit()
        {
            RestaurantDeliveries = new();
        }

    }
}
