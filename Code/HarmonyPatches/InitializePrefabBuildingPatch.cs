using System;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.AI;
using IndustriesMeetsSunsetHarbor.Utils;
using Object = UnityEngine.Object;
using MoreTransferReasons;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(BuildingInfo), "InitializePrefab")]
    public static class InitializePrefabBuildingPatch
    {
        public static void Prefix(BuildingInfo __instance)
        {
            try
            {
                if (__instance.m_class.m_service == ItemClass.Service.Fishing)
                {
                    if (__instance.name.Contains("Fish Market 01") && __instance.GetAI() is not ResourceMarketAI)
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = (PrefabAI)__instance.gameObject.AddComponent<ResourceMarketAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                    }
                    else if ((__instance.name.Contains("Aqua Crops Extractor") || __instance.name.Contains("Aqua Fish Extractor")) && __instance.GetAI() is not AquacultureExtractorAI)
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = (PrefabAI)__instance.gameObject.AddComponent<AquacultureExtractorAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                    }
                    else if (__instance.name.Contains("Aquaculture") &&  __instance.name.Contains("Dock") && __instance.GetAI() is not AquacultureFarmAI)
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = (PrefabAI)__instance.gameObject.AddComponent<AquacultureFarmAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                    }
                }
                else if (__instance.m_class.m_service == ItemClass.Service.PlayerIndustry)
                {
                    var component = __instance.GetComponent<PrefabAI>();
                    if (__instance.name.Contains("Food Factory 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource1 = TransferManager.TransferReason.None;
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.FoodProducts;
                    }
                    else if (__instance.name.Contains("Lemonade Factory 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource1 = TransferManager.TransferReason.None;
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.BeverageProducts;
                        __instance.name = "Drinks Factory 01";
                    }
                    else if (__instance.name.Contains("Bakery 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource1 = TransferManager.TransferReason.None;
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.BakedGoods;
                    }
                    else if (__instance.name.Contains("Food Factory 02") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource1 = TransferManager.TransferReason.None;
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.CannedFish;
                    }
                }
                else if (__instance.m_class.m_service == ItemClass.Service.Commercial)
                {
                    var component = __instance.GetComponent<PrefabAI>();
                    if (component != null && component is RestaurantAI)
                    {
                        __instance.m_class.m_service = (ItemClass.Service)28;
                        __instance.m_class.m_subService = ItemClass.SubService.None;
                        __instance.m_class.m_level = ItemClass.Level.Level3;
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e.ToString());
            }
        }

    }
}