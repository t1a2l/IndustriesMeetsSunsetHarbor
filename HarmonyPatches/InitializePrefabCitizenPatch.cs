using System;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.AI;
using IndustriesMeetsSunsetHarbor.Utils;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(CitizenInfo), "InitializePrefab")]
    public static class InitializePrefabCitizenPatch
    {
        public static void Prefix(CitizenInfo __instance)
        {
            try
            {
                var component = __instance.GetComponent<PrefabAI>();
                if (component != null && component is RestaurantDeliveryPersonAI)
                {
                    __instance.m_class.m_service = (ItemClass.Service)28;
                    __instance.m_class.m_subService = ItemClass.SubService.None;
                    __instance.m_class.m_level = ItemClass.Level.Level3;
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e.ToString());
            }
        }

    }
}