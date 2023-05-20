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
                if (__instance.m_class.m_service == ItemClass.Service.Industrial && __instance.name.Contains("Delivery Vehicle"))
                {
                    __instance.m_class.m_service = ItemClass.Service.Commercial;
                    __instance.m_class.m_subService = ItemClass.SubService.None;
                    __instance.m_class.m_level = ItemClass.Level.Level3;
                }
                if (__instance.m_class.m_service == ItemClass.Service.PlayerIndustry)
                {
                    var oldAI = __instance.GetComponent<PrefabAI>();
                    UnityEngine.Object.DestroyImmediate(oldAI);
                    var newAI = (PrefabAI)__instance.gameObject.AddComponent<ExtendedCargoTruckAI>();
                    PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

    }
}