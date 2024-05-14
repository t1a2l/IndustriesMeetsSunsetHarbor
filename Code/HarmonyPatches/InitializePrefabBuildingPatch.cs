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
                    else if (__instance.name.Contains("Fishing Boat Harbor 05") && __instance.GetAI() is not ExtendedFishingHarborAI)
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<ExtendedFishingHarborAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource = ExtendedTransferManager.TransferReason.Anchovy;
                    }
                    else if (__instance.name.Contains("Fishing Boat Harbor 02") && __instance.GetAI() is not ExtendedFishingHarborAI)
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<ExtendedFishingHarborAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource = ExtendedTransferManager.TransferReason.Salmon;
                    }
                    else if (__instance.name.Contains("Fishing Boat Harbor 03") && __instance.GetAI() is not ExtendedFishingHarborAI)
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<ExtendedFishingHarborAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource = ExtendedTransferManager.TransferReason.Shellfish;
                    }
                    else if (__instance.name.Contains("Fishing Boat Harbor 04") && __instance.GetAI() is not ExtendedFishingHarborAI)
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<ExtendedFishingHarborAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource = ExtendedTransferManager.TransferReason.Tuna;
                    }
                    else if (__instance.name.Contains("Fish Farm 01") && __instance.GetAI() is not ExtendedFishFarmAI)
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<ExtendedFishFarmAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource = ExtendedTransferManager.TransferReason.Trout;
                    }
                    else if (__instance.name.Contains("Fish Farm 02") && __instance.GetAI() is not ExtendedFishFarmAI)
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<ExtendedFishFarmAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource = ExtendedTransferManager.TransferReason.Algae;
                    }
                    else if (__instance.name.Contains("Fish Farm 03") && __instance.GetAI() is not ExtendedFishFarmAI)
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<ExtendedFishFarmAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource = ExtendedTransferManager.TransferReason.Seaweed;
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
                    else if (__instance.name.Contains("Furniture Factory 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource1 = TransferManager.TransferReason.None;
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.Furnitures;
                    }
                    else if (__instance.name.Contains("Industrial Steel Plant 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource1 = TransferManager.TransferReason.None;
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.IndustrialSteel;
                    }
                    else if (__instance.name.Contains("Household Plastic Factory 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource1 = TransferManager.TransferReason.None;
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.Tupperware;
                    }
                    else if (__instance.name.Contains("Toy Factory 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource1 = TransferManager.TransferReason.None;
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.Toys;
                    }
                    else if (__instance.name.Contains("Printing Press 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource1 = TransferManager.TransferReason.None;
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.PrintedProducts;
                    }
                    else if (__instance.name.Contains("Electronics Factory 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource1 = TransferManager.TransferReason.None;
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.ElectronicProducts;
                    }
                    else if (__instance.name.Contains("Clothing Factory 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource1 = TransferManager.TransferReason.None;
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.Cloths;
                    }
                    else if (__instance.name.Contains("Petroleum Refinery 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource1 = TransferManager.TransferReason.None;
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.PetroleumProducts;
                    }
                    else if (__instance.name.Contains("Soft Paper Factory 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource1 = TransferManager.TransferReason.None;
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.TissuePaper;
                    }
                    else if (__instance.name.Contains("Sneaker Factory 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource1 = TransferManager.TransferReason.None;
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.Footwear;
                    }
                    else if (__instance.name.Contains("Modular House Factory 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource1 = TransferManager.TransferReason.None;
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.HouseParts;
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