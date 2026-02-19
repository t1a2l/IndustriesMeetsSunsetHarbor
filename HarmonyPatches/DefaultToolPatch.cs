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
                RestaurantAI restaurantAI = info.m_buildingAI as RestaurantAI;
                if (Singleton<InstanceManager>.instance.SelectInstance(id))
                {
                    if(extendedProcessingFacilityAI != null)
		    {
		        WorldInfoPanel.Show<ExtendedProcessingFacilityWorldInfoPanel>(position, id);
                        WorldInfoPanel.Hide<RestaurantWorldInfoPanel>();
                        return false;
		    }
                    else if(restaurantAI != null)
		    {
		        WorldInfoPanel.Show<RestaurantWorldInfoPanel>(position, id);
                        WorldInfoPanel.Hide<ExtendedProcessingFacilityWorldInfoPanel>();
                        return false;
		    }
                    else
                    {
                        WorldInfoPanel.Hide<ExtendedProcessingFacilityWorldInfoPanel>();
                        WorldInfoPanel.Hide<RestaurantWorldInfoPanel>();
                    }
                }
            }
            else
            {
                WorldInfoPanel.Hide<ExtendedProcessingFacilityWorldInfoPanel>();
                WorldInfoPanel.Hide<RestaurantWorldInfoPanel>();
            }
            return true;
        }
    }
}
