using ColossalFramework.UI;
using HarmonyLib;
using ColossalFramework;
using IndustriesMeetsSunsetHarbor.Utils;
using IndustriesMeetsSunsetHarbor.AI;
using System.Reflection;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{

    [HarmonyPatch(typeof(CitizenVehicleWorldInfoPanel))]
    public static class CitizenVehicleWorldInfoPanelPatch
    {
        private delegate string VehicleWorldInfoPanelUpdateBindingsDelegate(VehicleWorldInfoPanel instance);
        private static VehicleWorldInfoPanelUpdateBindingsDelegate BaseUpdateBindings = AccessTools.MethodDelegate<VehicleWorldInfoPanelUpdateBindingsDelegate>(typeof(VehicleWorldInfoPanel).GetMethod("UpdateBindings", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate string GetVehicleTypeIconDelegate(CitizenVehicleWorldInfoPanel instance, VehicleInfo.VehicleType type);
        private static GetVehicleTypeIconDelegate GetVehicleTypeIcon = AccessTools.MethodDelegate<GetVehicleTypeIconDelegate>(typeof(CitizenVehicleWorldInfoPanel).GetMethod("GetVehicleTypeIcon", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        [HarmonyPatch(typeof(CitizenVehicleWorldInfoPanel), "GetVehicleTypeIcon")]
        [HarmonyPrefix]
        public static bool UpdateBindings(CitizenVehicleWorldInfoPanel __instance, ref InstanceID ___m_InstanceID, ref UISprite ___m_VehicleType)
        {
            BaseUpdateBindings(__instance);
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
                        ___m_VehicleType.atlas = TextureUtils.GetAtlas("RestaurantInfoIconButtonAtlas");
                        ___m_VehicleType.spriteName = "InfoIconRestaurant";
                    }
                    else
                    {
                        ___m_VehicleType.atlas = info.m_Atlas;
                        ___m_VehicleType.spriteName = GetVehicleTypeIcon(__instance, info.m_vehicleType);
                    }
                }
            }
            else if (___m_InstanceID.Type == InstanceType.ParkedVehicle && ___m_InstanceID.ParkedVehicle != 0)
            {
                VehicleParked vehicleParked = Singleton<VehicleManager>.instance.m_parkedVehicles.m_buffer[___m_InstanceID.ParkedVehicle];
                if(vehicleParked.Info.GetAI() is RestaurantDeliveryVehicleAI)
                {
                    ___m_VehicleType.atlas = TextureUtils.GetAtlas("RestaurantInfoIconButtonAtlas");
                    ___m_VehicleType.spriteName = "InfoIconRestaurant";
                }
                else
                {
                    ___m_VehicleType.atlas = vehicleParked.Info.m_Atlas;
                    ___m_VehicleType.spriteName = GetVehicleTypeIcon(__instance, vehicleParked.Info.m_vehicleType);
                }
            }
            return false;
        }

    }
}