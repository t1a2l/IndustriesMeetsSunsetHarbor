using System;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.AI;
using IndustriesMeetsSunsetHarbor.Utils;
using Object = UnityEngine.Object;
using System.Linq;
using TransferManagerCore;

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
                var oldAI = __instance.GetComponent<PrefabAI>();
                if (__instance.m_class.m_service == ItemClass.Service.Fishing)
                {
                    if (__instance.name.Contains("Fish Market 01") && oldAI is not ResourceMarketAI)
                    {
                        Object.DestroyImmediate(oldAI);
                        var newAI = (PrefabAI)__instance.gameObject.AddComponent<ResourceMarketAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                    }
                    if (oldAI is FishingHarborAI fishingHarborAI)
                    {
                        if (__instance.name.Contains("Fishing Boat Harbor 02") || __instance.name.Contains("Salmon"))
                        {
                            fishingHarborAI.m_outputResource = (TransferManager.TransferReason)CustomTransferReason.Reason.Salmon;
                        }
                        else if (__instance.name.Contains("Fishing Boat Harbor 03") || __instance.name.Contains("Shellfish"))
                        {
                            fishingHarborAI.m_outputResource = (TransferManager.TransferReason)CustomTransferReason.Reason.Shellfish;
                        }
                        else if (__instance.name.Contains("Fishing Boat Harbor 04") || __instance.name.Contains("Tuna"))
                        {
                            fishingHarborAI.m_outputResource = (TransferManager.TransferReason)CustomTransferReason.Reason.Tuna;
                        }
                        else if (__instance.name.Contains("Fishing Boat Harbor 05") || __instance.name.Contains("Anchovies"))
                        {
                            fishingHarborAI.m_outputResource = (TransferManager.TransferReason)CustomTransferReason.Reason.Anchovy;
                        }
                    }
                    if (oldAI is FishFarmAI fishFarmAI && !__instance.name.Contains("Sub"))
                    {
                        if (__instance.name.Contains("Fish Farm 01"))
                        {
                            fishFarmAI.m_outputResource = (TransferManager.TransferReason)CustomTransferReason.Reason.Trout;
                        }
                        else if (__instance.name.Contains("Fish Farm 02"))
                        {
                            fishFarmAI.m_outputResource = (TransferManager.TransferReason)CustomTransferReason.Reason.Algae;
                        }
                        else if (__instance.name.Contains("Fish Farm 03"))
                        {
                            fishFarmAI.m_outputResource = (TransferManager.TransferReason)CustomTransferReason.Reason.Seaweed;
                        }
                    }
                }
                if (__instance.m_class.m_service == ItemClass.Service.PlayerIndustry)
                {
                    if (oldAI is UniqueFactoryAI && !__instance.name.Contains("Sub"))
                    {
                        Object.DestroyImmediate(oldAI);
                        var newAI = (PrefabAI)__instance.gameObject.AddComponent<ExtendedUniqueFactoryAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);

                        if (newAI is ExtendedUniqueFactoryAI extendedUniqueFactoryAI)
                        {
                            if (__instance.name.Contains("Food Factory 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [CustomTransferReason.Reason.AnimalProducts, CustomTransferReason.Reason.Pork, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource2 = [CustomTransferReason.Reason.Flours, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource3 = [CustomTransferReason.Reason.Milk, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource4 = [CustomTransferReason.Reason.ProcessedVegetableOil, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource5 = [CustomTransferReason.Reason.Algae, CustomTransferReason.Reason.Seaweed, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource6 = [CustomTransferReason.Reason.Plastics];
                                extendedUniqueFactoryAI.m_inputResource7 = [CustomTransferReason.Reason.Metals];
                                extendedUniqueFactoryAI.m_outputResource1 = CustomTransferReason.Reason.FoodProducts;
                            }
                            else if (__instance.name.Contains("Lemonade Factory 01"))
                            {
                                __instance.name = "Drinks Factory 01";
                                extendedUniqueFactoryAI.m_inputResource1 = [CustomTransferReason.Reason.LiquidConcentrates, CustomTransferReason.Reason.Milk, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource2 = [CustomTransferReason.Reason.Crops, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource3 = [CustomTransferReason.Reason.Glass, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource4 = [CustomTransferReason.Reason.Plastics, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_outputResource1 = CustomTransferReason.Reason.BeverageProducts;
                            }
                            else if (__instance.name.Contains("Bakery 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [CustomTransferReason.Reason.Flours, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource2 = [CustomTransferReason.Reason.Milk, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource3 = [CustomTransferReason.Reason.Fruits, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_outputResource1 = CustomTransferReason.Reason.BakedGoods;
                            }
                            else if (__instance.name.Contains("Food Factory 02"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [CustomTransferReason.Reason.Salmon, CustomTransferReason.Reason.Tuna, CustomTransferReason.Reason.Trout];
                                extendedUniqueFactoryAI.m_inputResource2 = [CustomTransferReason.Reason.ProcessedVegetableOil, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource3 = [CustomTransferReason.Reason.Algae, CustomTransferReason.Reason.Seaweed, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource4 = [CustomTransferReason.Reason.Plastics, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource5 = [CustomTransferReason.Reason.Metals, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_outputResource1 = CustomTransferReason.Reason.CannedFish;
                            }
                            else if (__instance.name.Contains("Furniture Factory 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [CustomTransferReason.Reason.PlanedTimber, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource2 = [CustomTransferReason.Reason.Leather, CustomTransferReason.Reason.Cotton, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource3 = [CustomTransferReason.Reason.Algae, CustomTransferReason.Reason.Seaweed, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource4 = [CustomTransferReason.Reason.ChemicalProducts, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource5 = [CustomTransferReason.Reason.Paper, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_outputResource1 = CustomTransferReason.Reason.Furnitures;
                            }
                            else if (__instance.name.Contains("Electronics Factory 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [CustomTransferReason.Reason.Metals, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource2 = [CustomTransferReason.Reason.Glass, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource3 = [CustomTransferReason.Reason.Plastics, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_outputResource1 = CustomTransferReason.Reason.ElectronicProducts;
                            }
                            else if (__instance.name.Contains("Industrial Steel Plant 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [CustomTransferReason.Reason.Metals, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_outputResource1 = CustomTransferReason.Reason.IndustrialSteel;
                            }
                            else if (__instance.name.Contains("Household Plastic Factory 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [CustomTransferReason.Reason.ChemicalProducts, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource2 = [CustomTransferReason.Reason.ProcessedVegetableOil, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource3 = [CustomTransferReason.Reason.Plastics, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_outputResource1 = CustomTransferReason.Reason.Tupperware;
                            }
                            else if (__instance.name.Contains("Toy Factory 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [CustomTransferReason.Reason.PlanedTimber, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource2 = [CustomTransferReason.Reason.Cotton, CustomTransferReason.Reason.Wool, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource3 = [CustomTransferReason.Reason.ChemicalProducts, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource4 = [CustomTransferReason.Reason.Plastics, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_outputResource1 = CustomTransferReason.Reason.Toys;
                            }
                            else if (__instance.name.Contains("Printing Press 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [CustomTransferReason.Reason.Paper, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource2 = [CustomTransferReason.Reason.ChemicalProducts, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource3 = [CustomTransferReason.Reason.ProcessedVegetableOil, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource4 = [CustomTransferReason.Reason.Plastics, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_outputResource1 = CustomTransferReason.Reason.PrintedProducts;
                            }
                            else if (__instance.name.Contains("Soft Paper Factory 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [CustomTransferReason.Reason.Cotton, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource2 = [CustomTransferReason.Reason.Paper, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource3 = [CustomTransferReason.Reason.ChemicalProducts, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource4 = [CustomTransferReason.Reason.Plastics, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_outputResource1 = CustomTransferReason.Reason.TissuePaper;
                            }
                            else if (__instance.name.Contains("Clothing Factory 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [CustomTransferReason.Reason.Cotton, CustomTransferReason.Reason.Wool, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource2 = [CustomTransferReason.Reason.Leather, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource3 = [CustomTransferReason.Reason.Plastics, CustomTransferReason.Reason.Paper, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_outputResource1 = CustomTransferReason.Reason.Cloths;
                            }
                            else if (__instance.name.Contains("Petroleum Refinery 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [CustomTransferReason.Reason.Metals, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource2 = [CustomTransferReason.Reason.Petroleum, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource3 = [CustomTransferReason.Reason.Plastics, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_outputResource1 = CustomTransferReason.Reason.PetroleumProducts;
                            }
                            else if (__instance.name.Contains("Car Factory 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [CustomTransferReason.Reason.Metals, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource2 = [CustomTransferReason.Reason.Leather, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource3 = [CustomTransferReason.Reason.Plastics, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource4 = [CustomTransferReason.Reason.ChemicalProducts, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource5 = [CustomTransferReason.Reason.Glass, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_outputResource1 = CustomTransferReason.Reason.Cars;
                            }
                            else if (__instance.name.Contains("Sneaker Factory 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [CustomTransferReason.Reason.PlanedTimber, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource2 = [CustomTransferReason.Reason.Cotton, CustomTransferReason.Reason.Leather, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource3 = [CustomTransferReason.Reason.Plastics, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource4 = [CustomTransferReason.Reason.ChemicalProducts, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_outputResource1 = CustomTransferReason.Reason.Footwear;
                            }
                            else if (__instance.name.Contains("Modular House Factory 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [CustomTransferReason.Reason.ChemicalProducts, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource2 = [CustomTransferReason.Reason.Metals, CustomTransferReason.Reason.PlanedTimber, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource3 = [CustomTransferReason.Reason.Paper, CustomTransferReason.Reason.Plastics, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource4 = [CustomTransferReason.Reason.Glass, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_outputResource1 = CustomTransferReason.Reason.HouseParts;
                            }
                            else if (__instance.name.Contains("Dry Dock 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [CustomTransferReason.Reason.PlanedTimber, CustomTransferReason.Reason.Metals, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource2 = [CustomTransferReason.Reason.Plastics, CustomTransferReason.Reason.Glass, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource3 = [CustomTransferReason.Reason.ChemicalProducts, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_inputResource4 = [CustomTransferReason.Reason.Leather, CustomTransferReason.Reason.Cotton, CustomTransferReason.Reason.None];
                                extendedUniqueFactoryAI.m_outputResource1 = CustomTransferReason.Reason.Ship;
                                extendedUniqueFactoryAI.m_outputVehicleCount1 = 0;
                            }
                        }
                    }
                    else if (oldAI is ExtractingFacilityAI extractingFacilityAI && !__instance.name.Contains("Sub"))
                    {
                        if (__instance.name.Contains("Fruit Field"))
                        {
                            extractingFacilityAI.m_outputResource = (TransferManager.TransferReason)CustomTransferReason.Reason.Fruits;
                            extractingFacilityAI.m_outputRate = 1000;
                        }
                    }
                    else if (oldAI is ProcessingFacilityAI processingFacilityAI && !__instance.name.Contains("Sub"))
                    {
                        string[] names = ["Animal Pasture", "Cattle Shed", "Slaughter House", "Milking Parlour"];
                        if (names.Any(s => __instance.name.Contains(s)))
                        {
                            Object.DestroyImmediate(oldAI);
                            var newAI = (PrefabAI)__instance.gameObject.AddComponent<ExtendedProcessingFacilityAI>();
                            PrefabUtil.TryCopyAttributes(oldAI, newAI, false);

                            if (newAI is ExtendedProcessingFacilityAI extendedProcessingFacilityAI)
                            {
                                if (__instance.name.Contains("Animal Pasture") || __instance.name.Contains("Cattle Shed"))
                                {
                                    extendedProcessingFacilityAI.m_inputResource1 = [CustomTransferReason.Reason.Crops, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                    extendedProcessingFacilityAI.m_inputResource2 = [CustomTransferReason.Reason.Vegetables, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                    extendedProcessingFacilityAI.m_inputRate1 = 1000;
                                    extendedProcessingFacilityAI.m_inputRate2 = 1000;
                                    extendedProcessingFacilityAI.m_outputResource1 = CustomTransferReason.Reason.Cows;
                                    extendedProcessingFacilityAI.m_outputResource2 = CustomTransferReason.Reason.None;
                                    extendedProcessingFacilityAI.m_outputRate1 = 1000;
                                    extendedProcessingFacilityAI.m_outputRate2 = 0;
                                }
                                if (__instance.name.Contains("Slaughter House") || __instance.name.Contains("Milking Parlour"))
                                {
                                    extendedProcessingFacilityAI.m_inputResource1 = [CustomTransferReason.Reason.Cows, CustomTransferReason.Reason.None, CustomTransferReason.Reason.None];
                                    extendedProcessingFacilityAI.m_inputRate1 = 1000;
                                    if (__instance.name.Contains("Slaughter House"))
                                    {
                                        extendedProcessingFacilityAI.m_outputResource1 = CustomTransferReason.Reason.AnimalProducts;
                                        extendedProcessingFacilityAI.m_outputResource2 = CustomTransferReason.Reason.RawHides;
                                        extendedProcessingFacilityAI.m_outputRate1 = 1000;
                                        extendedProcessingFacilityAI.m_outputRate2 = 1000;
                                    }
                                    else if (__instance.name.Contains("Milking Parlour"))
                                    {
                                        extendedProcessingFacilityAI.m_outputResource1 = CustomTransferReason.Reason.Milk;
                                        extendedProcessingFacilityAI.m_outputResource2 = CustomTransferReason.Reason.None;
                                        extendedProcessingFacilityAI.m_outputRate1 = 1000;
                                        extendedProcessingFacilityAI.m_outputRate2 = 0;
                                    }
                                }
                            }
                        }
                    }
                }
                if (__instance.m_class.m_service == ItemClass.Service.Commercial)
                {
                    var component = __instance.GetComponent<PrefabAI>();
                    if (component != null && component is RestaurantAI)
                    {
                        __instance.m_class.m_service = (ItemClass.Service)29;
                        __instance.m_class.m_subService = ItemClass.SubService.None;
                        __instance.m_class.m_level = ItemClass.Level.Level3;
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("InitializePrefabBuildingPatch Error: " + e.ToString());
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