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
            "OrderedMeals"
        };

        public static string[] NotificationSpriteNames = new string[]
        {
            "BuildingNotificationWaitingDeliveryCritical",
            "BuildingNotificationWaitingDelivery",
            "BuildingNotificationWaitingDeliveryFirst"
        };

        public static string[] InfoIconRestaurantButton = new string[]
        {
            "InfoIconRestaurantBase",
	    "InfoIconRestaurantDisabled",
	    "InfoIconRestaurantFocused",
	    "InfoIconRestaurantHovered",
	    "InfoIconRestaurantPressed",
        };

        public static void CreateAtlas()
        {
            if (TextureUtils.GetAtlas("RestaurantAtlas") == null)
            {
                TextureUtils.InitialiseAtlas("RestaurantAtlas");
                for (int i = 0; i < SpriteNames.Length; i++)
                {
                    TextureUtils.AddSpriteToAtlas(new Rect(32 * i + 2, 2, 30, 30), SpriteNames[i], "RestaurantAtlas");
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
                    TextureUtils.AddSpriteToAtlas(new Rect(34 * i + 6, 2, 32, 32), InfoIconRestaurantButton[i], "InfoIconRestaurantButtonAtlas");
                }
            }
        }

    }
}
