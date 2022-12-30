using System;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.AI;
using IndustriesMeetsSunsetHarbor.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(BuildingInfo), "InitializePrefab")]
    public static class InitializePrefabBuildingPatch
    {
        public static void Prefix(BuildingInfo __instance)
        {
            try
            {
                if (__instance.m_class.m_service == ItemClass.Service.HealthCare && __instance.name.Contains("Fish Market 01") && __instance.GetAI() is not ResourceMarketAI)
                {
                    var oldAI = __instance.GetComponent<PrefabAI>();
                    Object.DestroyImmediate(oldAI);
                    var newAI = (PrefabAI)__instance.gameObject.AddComponent<ResourceMarketAI>();
                    PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                }
                else if (__instance.m_class.m_service == ItemClass.Service.HealthCare && (__instance.name.Contains("Aqua Crop Extractor") || __instance.name.Contains("Aqua Fish Extractor")) && __instance.GetAI() is not AquacultureExtractorAI)
                {
                    var oldAI = __instance.GetComponent<PrefabAI>();
                    Object.DestroyImmediate(oldAI);
                    var newAI = (PrefabAI)__instance.gameObject.AddComponent<AquacultureExtractorAI>();
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