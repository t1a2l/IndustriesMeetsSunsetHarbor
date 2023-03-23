using ColossalFramework;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.Managers;
using IndustriesMeetsSunsetHarbor.AI;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(CargoTruckAI))]
    public static class CargoTruckAIPatch
    {
        [HarmonyPatch(typeof(CargoTruckAI), "ReleaseVehicle")]
        [HarmonyPrefix]
        public static bool ReleaseVehicle(CargoTruckAI __instance, ushort vehicleID, ref Vehicle data) {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            BuildingInfo info = instance.m_buildings.m_buffer[data.m_sourceBuilding].Info;
            if(info.m_buildingAI is RestaurantAI || info.m_buildingAI is NewUniqueFactoryAI || info.m_buildingAI is NewProcessingFacilityAI)
            {
                CargoTruckAIManager.ReleaseVehicle(__instance, vehicleID, ref data);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(CargoTruckAI), "SetSource")]
        [HarmonyPrefix]
        public static bool SetSource(CargoTruckAI __instance, ushort vehicleID, ref Vehicle data, ushort sourceBuilding) {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            BuildingInfo info = instance.m_buildings.m_buffer[(int)sourceBuilding].Info;
            if(info.m_buildingAI is RestaurantAI || info.m_buildingAI is NewUniqueFactoryAI || info.m_buildingAI is NewProcessingFacilityAI)
            {
                CargoTruckAIManager.SetSource(__instance, vehicleID, ref data, sourceBuilding);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(CargoTruckAI), "ArriveAtTarget")]
        [HarmonyPrefix]
        public static bool ArriveAtTarget(CargoTruckAI __instance, ushort vehicleID, ref Vehicle data) {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            BuildingInfo info = instance.m_buildings.m_buffer[data.m_sourceBuilding].Info;
            if(info.m_buildingAI is RestaurantAI || info.m_buildingAI is NewUniqueFactoryAI || info.m_buildingAI is NewProcessingFacilityAI)
            {
                CargoTruckAIManager.ArriveAtTarget(__instance, vehicleID, ref data);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(CargoTruckAI), "ArriveAtSource")]
        [HarmonyPrefix]
        public static bool ArriveAtSource(CargoTruckAI __instance, ushort vehicleID, ref Vehicle data) {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            BuildingInfo info = instance.m_buildings.m_buffer[data.m_sourceBuilding].Info;
            if(info.m_buildingAI is RestaurantAI || info.m_buildingAI is NewUniqueFactoryAI || info.m_buildingAI is NewProcessingFacilityAI)
            {
                CargoTruckAIManager.ArriveAtSource(__instance, vehicleID, ref data);
                return false;
            }
            return true;
        }
    }
}
