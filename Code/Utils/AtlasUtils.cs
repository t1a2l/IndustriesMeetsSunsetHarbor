using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.Utils
{
    class AtlasUtils
    {
        public static string[] NotificationSpriteNames =
        [
            "BuildingNotificationWaitingDeliveryCritical",
            "BuildingNotificationWaitingDelivery",
            "BuildingNotificationWaitingDeliveryFirst"
        ];

        public static string[] InfoIconRestaurantButton =
        [
            "InfoIconRestaurant",
	    "InfoIconRestaurantDisabled",
            "InfoIconRestaurantFocused",
            "InfoIconRestaurantHovered",
	    "InfoIconRestaurantPressed"
        ];

        public static void CreateAtlas()
        {
            if (TextureUtils.GetAtlas("DeliveryNotificationAtlas") == null)
            {
                TextureUtils.InitialiseAtlas("DeliveryNotificationAtlas");
                TextureUtils.AddSpriteToAtlas(new Rect(2, 2, 77, 78), NotificationSpriteNames[0], "DeliveryNotificationAtlas");
                TextureUtils.AddSpriteToAtlas(new Rect(83, 1, 80, 79), NotificationSpriteNames[1], "DeliveryNotificationAtlas");
                TextureUtils.AddSpriteToAtlas(new Rect(169, 3, 71, 77), NotificationSpriteNames[2], "DeliveryNotificationAtlas");

            }
            if (TextureUtils.GetAtlas("RestaurantInfoIconButtonAtlas") == null)
            {
                TextureUtils.InitialiseAtlas("InfoIconRestaurantButtonAtlas");
                for (int i = 0; i < InfoIconRestaurantButton.Length; i++)
                {
                    TextureUtils.AddSpriteToAtlas(new Rect(36 * i, 2, 34, 34), InfoIconRestaurantButton[i], "InfoIconRestaurantButtonAtlas");
                }
            }
        }

    }
}
