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
                MultiProcessingFacilityAI multiProcessingFacilityAI = info.m_buildingAI as MultiProcessingFacilityAI;
                NewProcessingFacilityAI newProcessingFacilityAI = info.m_buildingAI as NewProcessingFacilityAI;
                NewUniqueFactoryAI newUniqueFactoryAI = info.m_buildingAI as NewUniqueFactoryAI;
                RestaurantAI restaurantAI = info.m_buildingAI as RestaurantAI;
                if (Singleton<InstanceManager>.instance.SelectInstance(id))
                {
                    if(newUniqueFactoryAI != null || newProcessingFacilityAI != null)
		    {
		        WorldInfoPanel.Show<NewProcessingFacilityWorldInfoPanel>(position, id);
                        WorldInfoPanel.Hide<NewMultiProcessingFacilityWorldInfoPanel>();
                        WorldInfoPanel.Hide<RestaurantWorldInfoPanel>();
                        return false;
		    }
                    else if (multiProcessingFacilityAI != null)
                    {
                        WorldInfoPanel.Show<NewMultiProcessingFacilityWorldInfoPanel>(position, id);
                        WorldInfoPanel.Hide<NewProcessingFacilityWorldInfoPanel>();
                        WorldInfoPanel.Hide<RestaurantWorldInfoPanel>();
                        return false;
                    }
                    else if(restaurantAI != null)
		    {
		        WorldInfoPanel.Show<RestaurantWorldInfoPanel>(position, id);
                        WorldInfoPanel.Hide<NewProcessingFacilityWorldInfoPanel>();
                        WorldInfoPanel.Hide<NewMultiProcessingFacilityWorldInfoPanel>();
                        return false;
		    }
                    else
                    {
                        WorldInfoPanel.Hide<NewProcessingFacilityWorldInfoPanel>();
                        WorldInfoPanel.Hide<NewMultiProcessingFacilityWorldInfoPanel>();
                        WorldInfoPanel.Hide<RestaurantWorldInfoPanel>();
                    }
                }
            }
            else
            {
                WorldInfoPanel.Hide<NewProcessingFacilityWorldInfoPanel>();
                WorldInfoPanel.Hide<NewMultiProcessingFacilityWorldInfoPanel>();
                WorldInfoPanel.Hide<RestaurantWorldInfoPanel>();
            }
            return true;
        }
    }
}
