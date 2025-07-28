using ColossalFramework;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.Managers;
using IndustriesMeetsSunsetHarbor.AI;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(CommonBuildingAI))]
    public static class CommonBuildingAIPatch
    {
        [HarmonyPatch(typeof(CommonBuildingAI), "GetColor")]
        [HarmonyPrefix]
        public static bool GetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode, ref Color __result)
        {
            if (infoMode == (InfoManager.InfoMode)41 && data.Info.GetAI() is not RestaurantAI)
            {
                var waiting_delivery = RestaurantManager.IsBuildingWaitingForDelivery(buildingID);
                if (waiting_delivery)
                {
                    __result = Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_targetColor;
                }
                else
                {
                    __result = Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                }
                return false;
            }
            return true;
        }
    }
}
