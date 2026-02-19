using System;
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
            public int mealType;
        }

        public struct RestaurantSitDownData
        {
            public uint citizenId;
            public int mealType;
            public DateTime enterTime;
        }

        public static Dictionary<ushort, List<RestaurantDeliveryData>> RestaurantsDeliveries {get; private set;}

        public static Dictionary<ushort, List<RestaurantSitDownData>> RestaurantsSitDowns {get; private set;}

        public static void Init()
        {
            RestaurantsDeliveries ??= [];
            RestaurantsSitDowns ??= [];
        }

        public static void Deinit()
        {
            RestaurantsDeliveries = [];
            RestaurantsSitDowns = [];
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

        public static List<RestaurantSitDownData> GetRestaurantSitDownsList(ushort buildingID)
        {
            if(!RestaurantsSitDowns.TryGetValue(buildingID, out List<RestaurantSitDownData> SitDownsList))
            {
                SitDownsList = [];
                RestaurantsSitDowns[buildingID] = SitDownsList;
            }
            return SitDownsList;
        }

        public static void SetRestaurantDeliveriesList(ushort buildingID, List<RestaurantDeliveryData> DeliveriesList)
        {
            RestaurantsDeliveries[buildingID] = DeliveriesList;
        }

        public static void SetRestaurantSitDownsList(ushort buildingID, List<RestaurantSitDownData> SitDownsList)
        {
            RestaurantsSitDowns[buildingID] = SitDownsList;
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
