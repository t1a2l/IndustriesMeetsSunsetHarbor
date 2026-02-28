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
        [HarmonyPrefix]
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
                    else if (__instance.name.Contains("Aquaculture") && __instance.name.Contains("Dock") && __instance.GetAI() is not AquacultureFarmAI)
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<AquacultureFarmAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_outputResource = TransferManager.TransferReason.Fish;
                        if (__instance.name.Contains("Algae"))
                        {
                            newAI.m_outputResource = ExtendedTransferManager.Algae;
                        }
                        else if (__instance.name.Contains("Seaweed"))
                        {
                            newAI.m_outputResource = ExtendedTransferManager.Seaweed;
                        }
                        else if (__instance.name.Contains("Mussels"))
                        {
                            newAI.m_outputResource = ExtendedTransferManager.Trout;
                        }
                    }
                    else if (__instance.GetAI() is FishingHarborAI fishingHarborAI)
                    {
                        if (__instance.name.Contains("Fishing Boat Harbor 02"))
                        {
                            fishingHarborAI.m_outputResource = ExtendedTransferManager.Salmon;
                        }
                        else if (__instance.name.Contains("Fishing Boat Harbor 03"))
                        {
                            fishingHarborAI.m_outputResource = ExtendedTransferManager.Shellfish;
                        }
                        else if (__instance.name.Contains("Fishing Boat Harbor 04"))
                        {
                            fishingHarborAI.m_outputResource = ExtendedTransferManager.Tuna;
                        }
                        else if (__instance.name.Contains("Fishing Boat Harbor 05"))
                        {
                            fishingHarborAI.m_outputResource = ExtendedTransferManager.Anchovy;
                        }
                    }
                    else if (__instance.GetAI() is FishFarmAI fishFarmAI && !__instance.name.Contains("Sub"))
                    {
                        if (__instance.name.Contains("Fish Farm 01"))
                        {
                            fishFarmAI.m_outputResource = ExtendedTransferManager.Trout;
                        }
                        else if (__instance.name.Contains("Fish Farm 02"))
                        {
                            fishFarmAI.m_outputResource = ExtendedTransferManager.Algae;
                        }
                        else if (__instance.name.Contains("Fish Farm 03"))
                        {
                            fishFarmAI.m_outputResource = ExtendedTransferManager.Seaweed;
                        }
                    }
                }
                else if (__instance.m_class.m_service == ItemClass.Service.PlayerIndustry)
                {
                    var component = __instance.GetComponent<PrefabAI>();
                    if (__instance.GetAI() is UniqueFactoryAI uniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        if (__instance.name.Contains("Food Factory 01"))
                        {
                            uniqueFactoryAI.m_outputResource = ExtendedTransferManager.FoodProducts;
                        }
                        else if (__instance.name.Contains("Lemonade Factory 01"))
                        {
                            uniqueFactoryAI.m_outputResource = ExtendedTransferManager.BeverageProducts;
                            __instance.name = "Drinks Factory 01";
                        }
                        else if (__instance.name.Contains("Food Factory 02"))
                        {
                            uniqueFactoryAI.m_outputResource = ExtendedTransferManager.CannedFish;
                        }
                        else if (__instance.name.Contains("Furniture Factory 01"))
                        {
                            uniqueFactoryAI.m_outputResource = ExtendedTransferManager.Furnitures;
                        }
                        else if (__instance.name.Contains("Industrial Steel Plant 01"))
                        {
                            uniqueFactoryAI.m_outputResource = ExtendedTransferManager.IndustrialSteel;
                        }
                        else if (__instance.name.Contains("Household Plastic Factory 01"))
                        {
                            uniqueFactoryAI.m_outputResource = ExtendedTransferManager.Tupperware;
                        }
                        else if (__instance.name.Contains("Toy Factory 01"))
                        {
                            uniqueFactoryAI.m_outputResource = ExtendedTransferManager.Toys;
                        }
                        else if (__instance.name.Contains("Printing Press 01"))
                        {
                            uniqueFactoryAI.m_outputResource = ExtendedTransferManager.PrintedProducts;
                        }
                        else if (__instance.name.Contains("Electronics Factory 01"))
                        {
                            uniqueFactoryAI.m_outputResource = ExtendedTransferManager.ElectronicProducts;
                        }
                        else if (__instance.name.Contains("Clothing Factory 01"))
                        {
                            uniqueFactoryAI.m_outputResource = ExtendedTransferManager.Cloths;
                        }
                        else if (__instance.name.Contains("Petroleum Refinery 01"))
                        {
                            uniqueFactoryAI.m_outputResource = ExtendedTransferManager.PetroleumProducts;
                        }
                        else if (__instance.name.Contains("Soft Paper Factory 01"))
                        {
                            uniqueFactoryAI.m_outputResource = ExtendedTransferManager.TissuePaper;
                        }
                        else if (__instance.name.Contains("Sneaker Factory 01"))
                        {
                            uniqueFactoryAI.m_outputResource = ExtendedTransferManager.Footwear;
                        }
                        else if (__instance.name.Contains("Modular House Factory 01"))
                        {
                            uniqueFactoryAI.m_outputResource = ExtendedTransferManager.HouseParts;
                        }
                        else if (__instance.name.Contains("Car Factory 01"))
                        {
                            uniqueFactoryAI.m_outputResource = ExtendedTransferManager.Cars;
                        }
                        else if (__instance.name.Contains("Dry Dock 01"))
                        {
                            uniqueFactoryAI.m_outputResource = ExtendedTransferManager.Ship;
                            uniqueFactoryAI.m_outputVehicleCount = 0;
                        }
                    }
                    else if (__instance.name.Contains("Animal Pasture") && __instance.GetAI() is not ExtendedProcessingFacilityAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<ExtendedProcessingFacilityAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_inputResource1 = TransferManager.TransferReason.Grain;
                        newAI.m_inputResource2 = ExtendedTransferManager.Vegetables;
                        newAI.m_inputRate1 = 1000;
                        newAI.m_inputRate2 = 1000;
                        if (__instance.name.Contains("Sheep"))
                        {
                            newAI.m_outputResource1 = ExtendedTransferManager.Sheep;
                            newAI.m_outputResource2 = ExtendedTransferManager.Wool;
                        }
                        else if (__instance.name.Contains("Highland Cow"))
                        {
                            newAI.m_outputResource1 = ExtendedTransferManager.HighlandCows;
                        }
                        else if (__instance.name.Contains("Pig"))
                        {
                            newAI.m_outputResource1 = ExtendedTransferManager.Pigs;
                        }
                        else
                        {
                            newAI.m_outputResource1 = ExtendedTransferManager.Cows;
                        }
                    }
                    else if (__instance.GetAI() is ExtractingFacilityAI extractingFacilityAI && !__instance.name.Contains("Sub"))
                    {
                        if (__instance.name.Contains("Crop Field"))
                        {
                            if (__instance.name.Contains("Corn") || __instance.name.Contains("Potato") || __instance.name.Contains("Green House"))
                            {
                                extractingFacilityAI.m_outputResource = ExtendedTransferManager.Vegetables;
                            }
                            else if (__instance.name.Contains("Cotton"))
                            {
                                extractingFacilityAI.m_outputResource = ExtendedTransferManager.Cotton;
                            }
                            else if (__instance.name.Contains("Wheat"))
                            {
                                extractingFacilityAI.m_outputResource = TransferManager.TransferReason.Grain;
                            }
                        }
                        else if (__instance.name.Contains("Fruit Field"))
                        {
                            extractingFacilityAI.m_outputResource = ExtendedTransferManager.Fruits;
                        }
                    }
                    else if (__instance.name.Contains("Cattle Shed") && __instance.GetAI() is not ExtendedProcessingFacilityAI && !__instance.name.Contains("Sub"))
                    {
                        var oldAI = __instance.GetComponent<PrefabAI>();
                        Object.DestroyImmediate(oldAI);
                        var newAI = __instance.gameObject.AddComponent<ExtendedProcessingFacilityAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        newAI.m_inputResource1 = TransferManager.TransferReason.Grain;
                        newAI.m_inputResource2 = ExtendedTransferManager.Vegetables;
                        newAI.m_inputRate1 = 1000;
                        newAI.m_inputRate2 = 1000;
                        newAI.m_outputResource1 = ExtendedTransferManager.Cows;
                    }
                    else if (__instance.name.Contains("Slaughter House") && __instance.GetAI() is ProcessingFacilityAI processingFacilityAI && !__instance.name.Contains("Sub"))
                    {
                        processingFacilityAI.m_inputResource1 = ExtendedTransferManager.Cows;
                        processingFacilityAI.m_inputRate1 = 1000;

                        if (__instance.name.Contains("Slaughter House"))
                        {
                            processingFacilityAI.m_outputResource = TransferManager.TransferReason.AnimalProducts;
                        }
                        else if (__instance.name.Contains("Milking Parlour"))
                        {
                            processingFacilityAI.m_outputResource = ExtendedTransferManager.Milk;
                        }
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

        [HarmonyPostfix]
        public static void Postfix(BuildingInfo __instance)
        {
            uint index = 0U;
            for (; PrefabCollection<BuildingInfo>.LoadedCount() > index; ++index)
            {
                BuildingInfo buildingInfo = PrefabCollection<BuildingInfo>.GetLoaded(index);

                if (buildingInfo != null && buildingInfo.GetAI() is WarehouseAI warehouseAI && warehouseAI != null)
                {
                    switch (__instance.name)
                    {
                        case "Grain Silo 01":
                        case "Grain Silo 02":
                        case "Barn 01":
                        case "Barn 02":
                            warehouseAI.m_storageType = TransferManager.TransferReason.None;
                            break;
                    }

                }
            }
        }

    }
}