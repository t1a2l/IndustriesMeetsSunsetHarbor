using HarmonyLib;
using UnityEngine;
using IndustriesMeetsSunsetHarbor.AI;
using IndustriesMeetsSunsetHarbor.UI;
using ColossalFramework;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(DefaultTool))]
    public static class DefaultToolPatch
    {
        [HarmonyPatch(typeof(DefaultTool), "OpenWorldInfoPanel")]
        [HarmonyPrefix]
        public static bool OpenWorldInfoPanel(DefaultTool __instance, InstanceID id, Vector3 position)
        {
            if (id.Building != 0)
            {
                BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[id.Building].Info;
                ExtendedProcessingFacilityAI extendedProcessingFacilityAI = info.m_buildingAI as ExtendedProcessingFacilityAI;
                WarehouseAI warehouseAI = info.m_buildingAI as WarehouseAI;
                WarehouseStationAI warehouseStationAI = info.m_buildingAI as WarehouseStationAI;
                RestaurantAI restaurantAI = info.m_buildingAI as RestaurantAI;
                if (Singleton<InstanceManager>.instance.SelectInstance(id))
                {
                    if(extendedProcessingFacilityAI != null)
		    {
		        WorldInfoPanel.Show<ExtendedProcessingFacilityWorldInfoPanel>(position, id);
                        WorldInfoPanel.Hide<ExtendedWarehouseWorldInfoPanel>();
                        WorldInfoPanel.Hide<RestaurantWorldInfoPanel>();
                        return false;
		    }
                    else if(restaurantAI != null)
		    {
		        WorldInfoPanel.Show<RestaurantWorldInfoPanel>(position, id);
                        WorldInfoPanel.Hide<ExtendedProcessingFacilityWorldInfoPanel>();
                        WorldInfoPanel.Hide<ExtendedWarehouseWorldInfoPanel>();
                        return false;
		    }
                    else if (warehouseAI != null || warehouseStationAI != null)
                    {
                        WorldInfoPanel.Show<ExtendedWarehouseWorldInfoPanel>(position, id);
                        WorldInfoPanel.Hide<ExtendedProcessingFacilityWorldInfoPanel>();
                        WorldInfoPanel.Hide<RestaurantWorldInfoPanel>();
                        return false;
                    }
                    else
                    {
                        WorldInfoPanel.Hide<ExtendedProcessingFacilityWorldInfoPanel>();
                        WorldInfoPanel.Hide<ExtendedWarehouseWorldInfoPanel>();
                        WorldInfoPanel.Hide<RestaurantWorldInfoPanel>();
                    }
                }
                else
                {
                    WorldInfoPanel.Hide<ExtendedProcessingFacilityWorldInfoPanel>();
                    WorldInfoPanel.Hide<RestaurantWorldInfoPanel>();
                }
            }
            else if (id.Park > 0)
            {
                if (Singleton<InstanceManager>.instance.SelectInstance(id))
                {
                    if (Singleton<DistrictManager>.instance.m_parks.m_buffer[id.Park].m_parkType == DistrictPark.ParkType.Farming)
                    {
                        WorldInfoPanel.Show<FarmingWorldInfoPanel>(position, id);
                        WorldInfoPanel.Hide<IndustryWorldInfoPanel>();
                        return false;
                    }
                    else
                    {
                        WorldInfoPanel.Hide<FarmingWorldInfoPanel>();
                    }
                }
                else
                {
                    WorldInfoPanel.Hide<FarmingWorldInfoPanel>();
                }
            }
            return true;
        }
    }
}
