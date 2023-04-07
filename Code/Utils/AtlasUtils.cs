using UnityEngine;
using MoreTransferReasons;

namespace IndustriesMeetsSunsetHarbor.Utils
{
    class AtlasUtils
    {
        public static string[] SpriteNames = new string[]
        {
            "Bread",
            "DrinkSupplies",
            "FoodSupplies",
            "Meals"
        };

        public static string[] NotificationSpriteNames = new string[]
        {
            "BuildingNotificationNotEnoughFoodDeliveryCritical",
            "BuildingNotificationNotEnoughFoodDelivery",
            "BuildingNotificationNotEnoughFoodDeliveryFirst"
        };

        public static void CreateAtlas()
        {
            if (TextureUtils.GetAtlas("RestaurantAtlas") == null)
            {
                TextureUtils.InitialiseAtlas("RestaurantAtlas");
                for (int i = 0; i < SpriteNames.Length; i++)
                {
                    TextureUtils.AddSpriteToAtlas(new Rect(32 * i, 0, 32, 32), SpriteNames[i], "RestaurantAtlas");
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
        }

        public static string ResourceSpriteName(ExtendedTransferManager.TransferReason transferReason)
	{
	    return transferReason.ToString();
	}


    }
}
