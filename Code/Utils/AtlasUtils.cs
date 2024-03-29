using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.Utils
{
    class AtlasUtils
    {
        public static string[] SpriteNames = new string[]
        {
            "Bread",
            "DrinkSupplies",
            "FoodSupplies",
            "Meals",
            "CannedFish",
            "OrderedMeals",
            "Anchovy",
	    "Salmon",
	    "Shellfish",
	    "Tuna",
	    "Algae",
	    "Seaweed",	
	    "Trout",
	    "Fruits",
	    "Vegetables",
	    "Cows",
	    "HighlandCows", 
	    "Sheep",
	    "Pigs",
	    "Furnitures",
	    "ElectronicProducts",
	    "IndustrialSteel",
	    "Tupperware",
	    "Toys",
	    "PrintedProducts",
	    "TissuePaper",
	    "Cloths",
	    "PetroleumProducts",
	    "Cars",
	    "Footwear",
	    "Houses",
        };

        public static string[] NotificationSpriteNames = new string[]
        {
            "BuildingNotificationWaitingDeliveryCritical",
            "BuildingNotificationWaitingDelivery",
            "BuildingNotificationWaitingDeliveryFirst"
        };

        public static string[] InfoIconRestaurantButton = new string[]
        {
            "InfoIconRestaurant",
	    "InfoIconRestaurantDisabled",
            "InfoIconRestaurantFocused",
            "InfoIconRestaurantHovered",
	    "InfoIconRestaurantPressed",
        };

        public static void CreateAtlas()
        {
            if (TextureUtils.GetAtlas("IndustriesAtlas") == null)
            {
                TextureUtils.InitialiseAtlas("IndustriesAtlas");
                for (int i = 0; i < SpriteNames.Length; i++)
                {
                    TextureUtils.AddSpriteToAtlas(new Rect(32 * i + 2, 2, 30, 30), SpriteNames[i], "IndustriesAtlas");
                }
            }
            if (TextureUtils.GetAtlas("DeliveryNotificationAtlas") == null)
            {
                TextureUtils.InitialiseAtlas("DeliveryNotificationAtlas");
                for (int i = 0; i < NotificationSpriteNames.Length; i++)
                {
                    TextureUtils.AddSpriteToAtlas(new Rect(82 * i, 0, 82, 82), NotificationSpriteNames[i], "DeliveryNotificationAtlas");
                }
            }
            if (TextureUtils.GetAtlas("RestaurantInfoIconButtonAtlas") == null)
            {
                TextureUtils.InitialiseAtlas("InfoIconRestaurantButtonAtlas");
                for (int i = 0; i < InfoIconRestaurantButton.Length; i++)
                {
                    TextureUtils.AddSpriteToAtlas(new Rect(2 * (-17 + (18 * (i + 1))), 2, 34, 34), InfoIconRestaurantButton[i], "InfoIconRestaurantButtonAtlas");
                }
            }
        }

    }
}
