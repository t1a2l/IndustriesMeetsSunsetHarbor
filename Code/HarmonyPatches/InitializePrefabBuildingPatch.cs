using System;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.AI;
using IndustriesMeetsSunsetHarbor.Utils;
using Object = UnityEngine.Object;
using MoreTransferReasons;
using IndustriesMeetsSunsetHarbor.Code.AI;
using MoreTransferReasons.AI;

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
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.FoodProducts;
                    }
                    else if (__instance.name.Contains("Lemonade Factory 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.BeverageProducts;
                        __instance.name = "Drinks Factory 01";
                    }
                    else if (__instance.name.Contains("Bakery 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.BakedGoods;
                    }
                    else if (__instance.name.Contains("Food Factory 02") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.CannedFish;
                    }
                    else if (__instance.name.Contains("Furniture Factory 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.Furnitures;
                    }
                    else if (__instance.name.Contains("Industrial Steel Plant 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.IndustrialSteel;
                    }
                    else if (__instance.name.Contains("Household Plastic Factory 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.Tupperware;
                    }
                    else if (__instance.name.Contains("Toy Factory 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.Toys;
                    }
                    else if (__instance.name.Contains("Printing Press 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.PrintedProducts;
                    }
                    else if (__instance.name.Contains("Electronics Factory 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.ElectronicProducts;
                    }
                    else if (__instance.name.Contains("Clothing Factory 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.Cloths;
                    }
                    else if (__instance.name.Contains("Petroleum Refinery 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.PetroleumProducts;
                    }
                    else if (__instance.name.Contains("Soft Paper Factory 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.TissuePaper;
                    }
                    else if (__instance.name.Contains("Sneaker Factory 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.Footwear;
                    }
                    else if (__instance.name.Contains("Modular House Factory 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.HouseParts;
                    }
                    else if (__instance.name.Contains("Car Factory 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.Cars;
                    }
                    else if (__instance.name.Contains("Dry Dock 01") && __instance.GetAI() is not NewUniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.Ship;
                        newAI.m_outputVehicleCount2 = 0;
                    }
                    else if (__instance.name.Contains("Animal Pasture 01") && __instance.GetAI() is not NewProcessingFacilityAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewProcessingFacilityAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource1 = TransferManager.TransferReason.None;
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.Cows;
                        newAI.m_inputResource1 = TransferManager.TransferReason.Grain;
                    }
                    else if (__instance.name.Contains("Animal Pasture") && __instance.GetAI() is not NewProcessingFacilityAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewProcessingFacilityAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        if (__instance.name.Contains("Sheep"))
                        {
                            newAI.m_outputResource1 = TransferManager.TransferReason.None;
                            newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.Sheep;
                            newAI.m_inputResource1 = TransferManager.TransferReason.Grain;
                            newAI.m_inputResource5 = ExtendedTransferManager.TransferReason.Sheep;
                        }
                        else if (__instance.name.Contains("Highland Cow"))
                        {
                            newAI.m_outputResource1 = TransferManager.TransferReason.None;
                            newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.HighlandCows;
                            newAI.m_inputResource1 = TransferManager.TransferReason.Grain;
                            newAI.m_inputResource5 = ExtendedTransferManager.TransferReason.HighlandCows;
                        }
                        else if (__instance.name.Contains("Pig"))
                        {
                            newAI.m_outputResource1 = TransferManager.TransferReason.None;
                            newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.Pigs;
                            newAI.m_inputResource1 = TransferManager.TransferReason.Grain;
                            newAI.m_inputResource5 = ExtendedTransferManager.TransferReason.Pigs;
                        }
                        else 
                        {
                            newAI.m_outputResource1 = TransferManager.TransferReason.None;
                            newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.Cows;
                            newAI.m_inputResource1 = TransferManager.TransferReason.Grain;
                            newAI.m_inputResource5 = ExtendedTransferManager.TransferReason.Cows;
                        }
                    }
                    else if (__instance.name.Contains("Cattle Shed") && __instance.GetAI() is not NewProcessingFacilityAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewProcessingFacilityAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource1 = TransferManager.TransferReason.None;
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.Cows;
                        newAI.m_outputResource3 = ExtendedTransferManager.TransferReason.HighlandCows;
                        newAI.m_inputResource1 = TransferManager.TransferReason.Grain;
                        newAI.m_inputResource5 = ExtendedTransferManager.TransferReason.Cows;
                        newAI.m_inputResource6 = ExtendedTransferManager.TransferReason.HighlandCows;
                    }
                    else if (__instance.name.Contains("Slaughter House") && __instance.GetAI() is not NewProcessingFacilityAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewProcessingFacilityAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource1 = TransferManager.TransferReason.AnimalProducts;
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.None;
                        newAI.m_inputResource5 = ExtendedTransferManager.TransferReason.Cows;
                        newAI.m_inputResource6 = ExtendedTransferManager.TransferReason.HighlandCows;
                    }
                    else if (__instance.name.Contains("Milking Parlour") && __instance.GetAI() is not NewProcessingFacilityAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewProcessingFacilityAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource1 = TransferManager.TransferReason.None;
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.Cows;
                        newAI.m_outputResource3 = ExtendedTransferManager.TransferReason.HighlandCows;
                        newAI.m_outputResource4 = ExtendedTransferManager.TransferReason.Milk;
                        newAI.m_inputResource5 = ExtendedTransferManager.TransferReason.Cows;
                        newAI.m_inputResource6 = ExtendedTransferManager.TransferReason.HighlandCows;
                    }
                    else if (__instance.name.Contains("Crop Field") && __instance.GetAI() is not NewExtractingFacilityAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewExtractingFacilityAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        if (__instance.name.Contains("Corn") || __instance.name.Contains("Potato") || __instance.name.Contains("Green House"))
                        {
                            newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.Vegetables;
                        }
                        else if (__instance.name.Contains("Cotton"))
                        {
                            newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.Cotton;
                        }
                        else if (__instance.name.Contains("Wheat"))
                        {
                            newAI.m_outputResource1 = TransferManager.TransferReason.Grain;
                        }
                    }
                    else if (__instance.name.Contains("Fruit Field") && __instance.GetAI() is not NewExtractingFacilityAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<NewExtractingFacilityAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource2 = ExtendedTransferManager.TransferReason.Fruits;
                    }
                    switch (__instance.name)
                    {
                        case "Grain Silo 01":
                        case "Grain Silo 02":
                        case "Barn 01":
                        case "Barn 02":
                            if (__instance.GetAI() is not ExtendedWarehouseAI)
                            {
                                var oldAI = __instance.GetComponent<PrefabAI>();
                                Object.DestroyImmediate(oldAI);
                                var newAI = __instance.gameObject.AddComponent<ExtendedWarehouseAI>();
                                PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                            }
                            break;
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

        public static void Postfix(BuildingInfo __instance)
        {
            uint index = 0U;
            for (; PrefabCollection<BuildingInfo>.LoadedCount() > index; ++index)
            {
                BuildingInfo buildingInfo = PrefabCollection<BuildingInfo>.GetLoaded(index);

                if (buildingInfo != null && buildingInfo.GetAI() is ExtendedWarehouseAI extendedWarehouseAI && extendedWarehouseAI != null)
                {
                    switch (__instance.name)
                    {
                        case "Grain Silo 01":
                        case "Grain Silo 02":
                        case "Barn 01":
                        case "Barn 02":
                            extendedWarehouseAI.m_storageType = TransferManager.TransferReason.None;
                            extendedWarehouseAI.m_extendedStorageType = ExtendedTransferManager.TransferReason.None;
                            extendedWarehouseAI.m_isFarmIndustry = true;
                            extendedWarehouseAI.m_isFishIndustry = false;
                            break;
                    }

                }
            }
        }

    }
}