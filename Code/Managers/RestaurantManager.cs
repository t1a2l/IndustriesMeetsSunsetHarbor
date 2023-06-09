using System.Collections.Generic;

namespace IndustriesMeetsSunsetHarbor.Managers
{
    public static class RestaurantManager
    {

        public struct RestaurantDeliveryData
        {
            public ushort deliveryVehicleId;
            public ushort buildingId;
            public uint citizenId;
            public ushort restaurantId;
            public bool mealCooked;
        }

        public static Dictionary<ushort, List<RestaurantDeliveryData>> RestaurantsDeliveries;

        public static Dictionary<ushort, List<uint>> RestaurantsLines;

        public static void Init()
        {
            if(RestaurantsDeliveries == null)
            {
                RestaurantsDeliveries = new();
            }
            if(RestaurantsLines == null)
            {
                RestaurantsLines = new();
            }
        }

        public static void Deinit()
        {
            RestaurantsDeliveries = new();
            RestaurantsLines = new();
        }

        public static List<RestaurantDeliveryData> GetRestaurantDeliveriesList(ushort buildingID)
        {
            if(!RestaurantsDeliveries.TryGetValue(buildingID, out List<RestaurantDeliveryData> DeliveriesList))
            {
                DeliveriesList = new List<RestaurantDeliveryData>();
                RestaurantsDeliveries[buildingID] = DeliveriesList;
            }
            return DeliveriesList;
        }

        public static List<uint> GetRestaurantLineList(ushort buildingID)
        {
            if(!RestaurantsLines.TryGetValue(buildingID, out List<uint> LineList))
            {
                LineList = new List<uint>();
                RestaurantsLines[buildingID] = LineList;
            }
            return LineList;
        }

        public static void SetRestaurantDeliveriesList(ushort buildingID, List<RestaurantDeliveryData> DeliveriesList)
        {
            RestaurantsDeliveries[buildingID] = DeliveriesList;
        }

        public static void SetRestaurantLineList(ushort buildingID, List<uint> LineList)
        {
            RestaurantsLines[buildingID] = LineList;
        }

        public static bool IsCitizenWaitingForDelivery(uint citizenId)
        {
            bool found = false;
            foreach(var restaurantDelivery in RestaurantsDeliveries)
            {
                var restaurantDeliveryList = restaurantDelivery.Value;
                found = restaurantDeliveryList.Exists(item => item.citizenId == citizenId);
            }
            return found;
        }

        public static bool IsBuildingWaitingForDelivery(ushort buildingId)
        {
            bool found = false;
            foreach(var restaurantDelivery in RestaurantsDeliveries)
            {
                var restaurantDeliveryList = restaurantDelivery.Value;
                found = restaurantDeliveryList.Exists(item => item.buildingId == buildingId);
            }
            return found;
        }
    }
}
