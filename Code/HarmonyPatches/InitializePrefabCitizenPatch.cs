using System;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.AI;
using IndustriesMeetsSunsetHarbor.Utils;
using Object = UnityEngine.Object;

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
                if(component != null && component is ResidentAI)
                {
                    var oldAI = __instance.GetComponent<PrefabAI>();
                    Object.DestroyImmediate(oldAI);
                    var newAI = __instance.gameObject.AddComponent<ExtendedResidentAI>();
                    PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                }
                else if(component != null && component is TouristAI)
                {
                    var oldAI = __instance.GetComponent<PrefabAI>();
                    Object.DestroyImmediate(oldAI);
                    var newAI = __instance.gameObject.AddComponent<ExtendedTouristAI>();
                    PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                }
                else if (component != null && component is RestaurantDeliveryPersonAI)
                {
                    __instance.m_class.m_service = ItemClass.Service.Commercial;
                    __instance.m_class.m_subService = (ItemClass.SubService)28;
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