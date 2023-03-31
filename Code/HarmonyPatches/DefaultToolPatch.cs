using HarmonyLib;
using UnityEngine;
using IndustriesMeetsSunsetHarbor.AI;
using IndustriesMeetsSunsetHarbor.UI;
using ColossalFramework;

namespace IndustriesMeetsSunsetHarbor.Code.HarmonyPatches
{
    [HarmonyPatch(typeof(DefaultTool))]
    public static class DefaultToolpatch
    {
        [HarmonyPatch(typeof(DefaultTool), "OpenWorldInfoPanel")]
        [HarmonyPrefix]
        public static bool OpenWorldInfoPanel(DefaultTool __instance, InstanceID id, Vector3 position)
        {
            if (id.Building != 0)
            {
                BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[id.Building].Info;
                NewUniqueFactoryAI newUniqueFactoryAI = info.m_buildingAI as NewUniqueFactoryAI;
                RestaurantAI restaurantAI = info.m_buildingAI as RestaurantAI;
                if(newUniqueFactoryAI != null)
		{
		    WorldInfoPanel.Show<NewUniqueFactoryWorldInfoPanel>(position, id);
                    return false;
		}
                else if(restaurantAI != null)
		{
		    WorldInfoPanel.Show<RestaurantAIWorldInfoPanel>(position, id);
                    return false;
		}
            }
            return true;
        }
    }
}
