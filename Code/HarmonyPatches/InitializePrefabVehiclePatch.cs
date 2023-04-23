using System;
using HarmonyLib;
using UnityEngine;

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
                    __instance.m_class.m_level = ItemClass.Level.Level5;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}