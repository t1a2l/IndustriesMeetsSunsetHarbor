using System;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.AI;
using IndustriesMeetsSunsetHarbor.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(VehicleInfo), "InitializePrefab")]
    public static class InitializePrefabVehiclePatch
    {
        public static void Prefix(VehicleInfo __instance)
        {
            try
            {
                if (__instance.m_class.m_service == ItemClass.Service.Commercial && __instance.name.Contains("Pizza") && __instance.GetAI() is not RestaurantDeliveryVehicleAI)
                {
                    var oldAI = __instance.GetComponent<PrefabAI>();
                    Object.DestroyImmediate(oldAI);
                    var newAI = (PrefabAI)__instance.gameObject.AddComponent<RestaurantDeliveryVehicleAI>();
                    PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static void Postfix()
        {
            try
            {
                VehicleInfo cargoTruckVehicleInfo = PrefabCollection<VehicleInfo>.FindLoaded("Lorry");

                uint index = 0U;
                for (; PrefabCollection<VehicleInfo>.LoadedCount() > index; ++index)
                {
                    VehicleInfo vehicleInfo = PrefabCollection<VehicleInfo>.GetLoaded(index);

                    if (vehicleInfo != null && vehicleInfo.GetAI() is FoodDeliveryVehicleAI foodDeliveryVehicleAI)
                    {
                        vehicleInfo.m_class = cargoTruckVehicleInfo.m_class;
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