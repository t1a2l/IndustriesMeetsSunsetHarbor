using UnityEngine;
using MoreTransferReasons.Code;

namespace IndustriesMeetsSunsetHarbor.Utils
{
    class AtlasUtils
    {
        public static string[] SpriteNames = new string[]
        {
            "Bread",
            "Drinks",
            "Food",
            "Meal",
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
        }

        public static string ResourceSpriteName(ExtendedTransferManager.TransferReason transferReason)
	{
	    return transferReason.ToString();
	}


    }
}
