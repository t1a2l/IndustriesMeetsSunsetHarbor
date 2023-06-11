using System;
using HarmonyLib;
using UnityEngine;
using IndustriesMeetsSunsetHarbor.AI;
using IndustriesMeetsSunsetHarbor.Utils;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(VehicleInfo), "InitializePrefab")]
    public static class InitializePrefabVehiclePatch
    {
        public static void Prefix(VehicleInfo __instance)
        {
            try
            {
                if ((__instance.m_class.m_service == ItemClass.Service.PlayerIndustry || __instance.m_class.m_service == ItemClass.Service.Industrial) && !__instance.name.Contains("Trailer"))
                {
                    var oldAI = __instance.GetComponent<PrefabAI>();
                    UnityEngine.Object.DestroyImmediate(oldAI);
                    var newAI = (PrefabAI)__instance.gameObject.AddComponent<ExtendedCargoTruckAI>();
                    PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                }
                else if (__instance.m_class.m_service == ItemClass.Service.Commercial)
                {
                    var component = __instance.GetComponent<PrefabAI>();
                    if(component != null && component is RestaurantDeliveryVehicleAI)
                    {
                        __instance.m_class.m_subService = (ItemClass.SubService)28;
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