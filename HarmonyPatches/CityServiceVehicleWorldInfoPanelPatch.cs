using ColossalFramework.UI;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.Utils;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(CityServiceVehicleWorldInfoPanel))]
    public static class CityServiceVehicleWorldInfoPanelPatch
    {
        [HarmonyPatch(typeof(CityServiceVehicleWorldInfoPanel), "GetVehicleIcon")]
        [HarmonyPrefix]
        public static bool GetVehicleIcon(ItemClass.Service service, ItemClass.SubService subService, VehicleInfo.VehicleType type, ref string __result)
        {
            if (service == (ItemClass.Service)28)
            {
                __result = "InfoIconRestaurant";
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(CityServiceVehicleWorldInfoPanel), "UpdateBindings")]
        [HarmonyPostfix]
        public static void UpdateBindings(CityServiceVehicleWorldInfoPanel __instance, ref InstanceID ___m_InstanceID, ref UISprite ___m_VehicleType)
        {
            if(___m_VehicleType.spriteName == "InfoIconRestaurant")
            {
                ___m_VehicleType.atlas = TextureUtils.GetAtlas("InfoIconRestaurantButtonAtlas");
            }
        }
    }

}

