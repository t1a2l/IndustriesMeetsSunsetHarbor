using System;
using HarmonyLib;
using UnityEngine;
using IndustriesMeetsSunsetHarbor.AI;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(VehicleInfo), "InitializePrefab")]
    public static class InitializePrefabVehiclePatch
    {
        public static void Prefix(VehicleInfo __instance)
        {
            try
            {
                if (__instance.m_class.m_service == ItemClass.Service.Commercial)
                {
                    var component = __instance.GetComponent<PrefabAI>();
                    if (component != null && component is RestaurantDeliveryVehicleAI)
                    {
                        __instance.m_class.m_service = (ItemClass.Service)28;
                        __instance.m_class.m_subService = ItemClass.SubService.None;
                        __instance.m_class.m_level = ItemClass.Level.Level3;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

    }
}