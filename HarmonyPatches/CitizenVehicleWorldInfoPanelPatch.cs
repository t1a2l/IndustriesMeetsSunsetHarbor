using ColossalFramework.UI;
using HarmonyLib;
using ColossalFramework;
using IndustriesMeetsSunsetHarbor.Utils;
using IndustriesMeetsSunsetHarbor.AI;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{

    [HarmonyPatch(typeof(CitizenVehicleWorldInfoPanel))]
    public static class CitizenVehicleWorldInfoPanelPatch
    {
        [HarmonyPatch(typeof(CitizenVehicleWorldInfoPanel), "UpdateBindings")]
        [HarmonyPostfix]
        public static void UpdateBindings(CitizenVehicleWorldInfoPanel __instance, ref InstanceID ___m_InstanceID, ref UISprite ___m_VehicleType)
        {
            if (___m_InstanceID.Type == InstanceType.Vehicle && ___m_InstanceID.Vehicle != 0)
            {
                InstanceID instanceID = ___m_InstanceID;
                ushort vehicle = ___m_InstanceID.Vehicle;
                ushort firstVehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicle].GetFirstVehicle(vehicle);
                if (firstVehicle != 0)
                {
                    instanceID.Vehicle = firstVehicle;
                    VehicleInfo info = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[firstVehicle].Info;
                    if(info.GetAI() is RestaurantDeliveryVehicleAI)
                    {
                        ___m_VehicleType.atlas = TextureUtils.GetAtlas("InfoIconRestaurantButtonAtlas");
                        ___m_VehicleType.spriteName = "InfoIconRestaurant";
                    }
                }
            }
            else if (___m_InstanceID.Type == InstanceType.ParkedVehicle && ___m_InstanceID.ParkedVehicle != 0)
            {
                VehicleParked vehicleParked = Singleton<VehicleManager>.instance.m_parkedVehicles.m_buffer[___m_InstanceID.ParkedVehicle];
                if(vehicleParked.Info.GetAI() is RestaurantDeliveryVehicleAI)
                {
                    ___m_VehicleType.atlas = TextureUtils.GetAtlas("InfoIconRestaurantButtonAtlas");
                    ___m_VehicleType.spriteName = "InfoIconRestaurant";
                }
            }
        }

    }
}
