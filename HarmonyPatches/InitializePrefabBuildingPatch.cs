using System;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.AI;
using IndustriesMeetsSunsetHarbor.Utils;
using Object = UnityEngine.Object;
using MoreTransferReasons;
using System.Linq;

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
                            fishingHarborAI.m_outputResource = ExtendedTransferManager.Salmon;
                        }
                        else if (__instance.name.Contains("Fishing Boat Harbor 03") || __instance.name.Contains("Shellfish"))
                        {
                            fishingHarborAI.m_outputResource = ExtendedTransferManager.Shellfish;
                        }
                        else if (__instance.name.Contains("Fishing Boat Harbor 04") || __instance.name.Contains("Tuna"))
                        {
                            fishingHarborAI.m_outputResource = ExtendedTransferManager.Tuna;
                        }
                        else if (__instance.name.Contains("Fishing Boat Harbor 05") || __instance.name.Contains("Anchovies"))
                        {
                            fishingHarborAI.m_outputResource = ExtendedTransferManager.Anchovy;
                        }
                    }
                    if (oldAI is FishFarmAI fishFarmAI && !__instance.name.Contains("Sub"))
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
                                extendedUniqueFactoryAI.m_inputResource1 = [TransferManager.TransferReason.AnimalProducts, ExtendedTransferManager.Pork];
                                extendedUniqueFactoryAI.m_inputResource2 = [TransferManager.TransferReason.Flours];
                                extendedUniqueFactoryAI.m_inputResource3 = [ExtendedTransferManager.Milk];
                                extendedUniqueFactoryAI.m_inputResource4 = [ExtendedTransferManager.ProcessedVegetableOil];
                                extendedUniqueFactoryAI.m_inputResource5 = [ExtendedTransferManager.Algae, ExtendedTransferManager.Seaweed];
                                extendedUniqueFactoryAI.m_inputResource6 = [TransferManager.TransferReason.Plastics];
                                extendedUniqueFactoryAI.m_inputResource7 = [TransferManager.TransferReason.Metals];
                                extendedUniqueFactoryAI.m_outputResource1 = ExtendedTransferManager.FoodProducts;
                            }
                            else if (__instance.name.Contains("Lemonade Factory 01"))
                            {
                                __instance.name = "Drinks Factory 01";
                                extendedUniqueFactoryAI.m_inputResource1 = [ExtendedTransferManager.LiquidConcentrates, ExtendedTransferManager.Milk];
                                extendedUniqueFactoryAI.m_inputResource2 = [TransferManager.TransferReason.Grain];
                                extendedUniqueFactoryAI.m_inputResource3 = [TransferManager.TransferReason.Glass];
                                extendedUniqueFactoryAI.m_inputResource4 = [TransferManager.TransferReason.Plastics];
                                extendedUniqueFactoryAI.m_outputResource1 = ExtendedTransferManager.BeverageProducts;
                            }
                            else if (__instance.name.Contains("Bakery 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [TransferManager.TransferReason.Flours];
                                extendedUniqueFactoryAI.m_inputResource2 = [ExtendedTransferManager.Milk];
                                extendedUniqueFactoryAI.m_inputResource3 = [ExtendedTransferManager.Fruits];
                                extendedUniqueFactoryAI.m_outputResource1 = ExtendedTransferManager.BakedGoods;
                            }
                            else if (__instance.name.Contains("Food Factory 02"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [ExtendedTransferManager.Salmon, ExtendedTransferManager.Tuna, ExtendedTransferManager.Trout];
                                extendedUniqueFactoryAI.m_inputResource2 = [ExtendedTransferManager.ProcessedVegetableOil];
                                extendedUniqueFactoryAI.m_inputResource3 = [ExtendedTransferManager.Algae, ExtendedTransferManager.Seaweed];
                                extendedUniqueFactoryAI.m_inputResource4 = [TransferManager.TransferReason.Plastics];
                                extendedUniqueFactoryAI.m_inputResource5 = [TransferManager.TransferReason.Metals];
                                extendedUniqueFactoryAI.m_outputResource1 = ExtendedTransferManager.CannedFish;
                            }
                            else if (__instance.name.Contains("Furniture Factory 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [TransferManager.TransferReason.PlanedTimber];
                                extendedUniqueFactoryAI.m_inputResource2 = [ExtendedTransferManager.Leather, ExtendedTransferManager.Cotton];
                                extendedUniqueFactoryAI.m_inputResource3 = [ExtendedTransferManager.Algae, ExtendedTransferManager.Seaweed];
                                extendedUniqueFactoryAI.m_inputResource4 = [ExtendedTransferManager.ChemicalProducts];
                                extendedUniqueFactoryAI.m_inputResource5 = [TransferManager.TransferReason.Paper];
                                extendedUniqueFactoryAI.m_outputResource1 = ExtendedTransferManager.Furnitures;
                            }
                            else if (__instance.name.Contains("Electronics Factory 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [TransferManager.TransferReason.Metals];
                                extendedUniqueFactoryAI.m_inputResource2 = [TransferManager.TransferReason.Glass];
                                extendedUniqueFactoryAI.m_inputResource3 = [TransferManager.TransferReason.Plastics];
                                extendedUniqueFactoryAI.m_outputResource1 = ExtendedTransferManager.ElectronicProducts;
                            }
                            else if (__instance.name.Contains("Industrial Steel Plant 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [TransferManager.TransferReason.Metals];
                                extendedUniqueFactoryAI.m_outputResource1 = ExtendedTransferManager.IndustrialSteel;
                            }
                            else if (__instance.name.Contains("Household Plastic Factory 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [ExtendedTransferManager.ChemicalProducts];
                                extendedUniqueFactoryAI.m_inputResource2 = [ExtendedTransferManager.ProcessedVegetableOil];
                                extendedUniqueFactoryAI.m_inputResource3 = [TransferManager.TransferReason.Plastics];
                                extendedUniqueFactoryAI.m_outputResource1 = ExtendedTransferManager.Tupperware;
                            }
                            else if (__instance.name.Contains("Toy Factory 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [TransferManager.TransferReason.PlanedTimber];
                                extendedUniqueFactoryAI.m_inputResource2 = [ExtendedTransferManager.Cotton, ExtendedTransferManager.Wool];
                                extendedUniqueFactoryAI.m_inputResource3 = [ExtendedTransferManager.ChemicalProducts];
                                extendedUniqueFactoryAI.m_inputResource4 = [TransferManager.TransferReason.Plastics];
                                extendedUniqueFactoryAI.m_outputResource1 = ExtendedTransferManager.Toys;
                            }
                            else if (__instance.name.Contains("Printing Press 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [TransferManager.TransferReason.Paper];
                                extendedUniqueFactoryAI.m_inputResource2 = [ExtendedTransferManager.ChemicalProducts];
                                extendedUniqueFactoryAI.m_inputResource3 = [ExtendedTransferManager.ProcessedVegetableOil];
                                extendedUniqueFactoryAI.m_inputResource4 = [TransferManager.TransferReason.Plastics];
                                extendedUniqueFactoryAI.m_outputResource1 = ExtendedTransferManager.PrintedProducts;
                            }
                            else if (__instance.name.Contains("Soft Paper Factory 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [ExtendedTransferManager.Cotton];
                                extendedUniqueFactoryAI.m_inputResource2 = [TransferManager.TransferReason.Paper];
                                extendedUniqueFactoryAI.m_inputResource3 = [ExtendedTransferManager.ChemicalProducts];
                                extendedUniqueFactoryAI.m_inputResource4 = [TransferManager.TransferReason.Plastics];
                                extendedUniqueFactoryAI.m_outputResource1 = ExtendedTransferManager.TissuePaper;
                            }
                            else if (__instance.name.Contains("Clothing Factory 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [ExtendedTransferManager.Cotton, ExtendedTransferManager.Wool];
                                extendedUniqueFactoryAI.m_inputResource2 = [ExtendedTransferManager.Leather];
                                extendedUniqueFactoryAI.m_inputResource3 = [TransferManager.TransferReason.Plastics, TransferManager.TransferReason.Paper];
                                extendedUniqueFactoryAI.m_outputResource1 = ExtendedTransferManager.Cloths;
                            }
                            else if (__instance.name.Contains("Petroleum Refinery 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [TransferManager.TransferReason.Metals];
                                extendedUniqueFactoryAI.m_inputResource2 = [TransferManager.TransferReason.Petroleum];
                                extendedUniqueFactoryAI.m_inputResource3 = [TransferManager.TransferReason.Plastics];
                                extendedUniqueFactoryAI.m_outputResource1 = ExtendedTransferManager.PetroleumProducts;
                            }
                            else if (__instance.name.Contains("Car Factory 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [TransferManager.TransferReason.Metals];
                                extendedUniqueFactoryAI.m_inputResource2 = [ExtendedTransferManager.Leather];
                                extendedUniqueFactoryAI.m_inputResource3 = [TransferManager.TransferReason.Plastics];
                                extendedUniqueFactoryAI.m_inputResource4 = [ExtendedTransferManager.ChemicalProducts];
                                extendedUniqueFactoryAI.m_inputResource5 = [TransferManager.TransferReason.Glass];
                                extendedUniqueFactoryAI.m_outputResource1 = ExtendedTransferManager.Cars;
                            }
                            else if (__instance.name.Contains("Sneaker Factory 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [TransferManager.TransferReason.PlanedTimber];
                                extendedUniqueFactoryAI.m_inputResource2 = [ExtendedTransferManager.Cotton, ExtendedTransferManager.Leather];
                                extendedUniqueFactoryAI.m_inputResource3 = [TransferManager.TransferReason.Plastics];
                                extendedUniqueFactoryAI.m_inputResource4 = [ExtendedTransferManager.ChemicalProducts];
                                extendedUniqueFactoryAI.m_outputResource1 = ExtendedTransferManager.Footwear;
                            }
                            else if (__instance.name.Contains("Modular House Factory 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [ExtendedTransferManager.ChemicalProducts];
                                extendedUniqueFactoryAI.m_inputResource2 = [TransferManager.TransferReason.Metals, TransferManager.TransferReason.PlanedTimber];
                                extendedUniqueFactoryAI.m_inputResource3 = [TransferManager.TransferReason.Paper, TransferManager.TransferReason.Plastics];
                                extendedUniqueFactoryAI.m_inputResource4 = [TransferManager.TransferReason.Glass];
                                extendedUniqueFactoryAI.m_outputResource1 = ExtendedTransferManager.HouseParts;
                            }
                            else if (__instance.name.Contains("Dry Dock 01"))
                            {
                                extendedUniqueFactoryAI.m_inputResource1 = [TransferManager.TransferReason.PlanedTimber, TransferManager.TransferReason.Metals];
                                extendedUniqueFactoryAI.m_inputResource2 = [TransferManager.TransferReason.Plastics, TransferManager.TransferReason.Glass];
                                extendedUniqueFactoryAI.m_inputResource3 = [ExtendedTransferManager.ChemicalProducts];
                                extendedUniqueFactoryAI.m_inputResource4 = [ExtendedTransferManager.Leather, ExtendedTransferManager.Cotton];
                                extendedUniqueFactoryAI.m_outputResource1 = ExtendedTransferManager.Ship;
                                extendedUniqueFactoryAI.m_outputVehicleCount1 = 0;
                            }
                        }
                    }
                    else if (oldAI is ExtractingFacilityAI extractingFacilityAI && !__instance.name.Contains("Sub"))
                    {
                        if (__instance.name.Contains("Fruit Field"))
                        {
                            extractingFacilityAI.m_outputResource = ExtendedTransferManager.Fruits;
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
                                    extendedProcessingFacilityAI.m_inputResource1 = [TransferManager.TransferReason.Grain];
                                    extendedProcessingFacilityAI.m_inputResource2 = [ExtendedTransferManager.Vegetables];
                                    extendedProcessingFacilityAI.m_inputRate1 = 1000;
                                    extendedProcessingFacilityAI.m_inputRate2 = 1000;
                                    extendedProcessingFacilityAI.m_outputResource1 = ExtendedTransferManager.Cows;
                                    extendedProcessingFacilityAI.m_outputResource2 = TransferManager.TransferReason.None;
                                    extendedProcessingFacilityAI.m_outputRate1 = 1000;
                                    extendedProcessingFacilityAI.m_outputRate2 = 0;
                                }
                                if (__instance.name.Contains("Slaughter House") || __instance.name.Contains("Milking Parlour"))
                                {
                                    extendedProcessingFacilityAI.m_inputResource1 = [ExtendedTransferManager.Cows];
                                    extendedProcessingFacilityAI.m_inputRate1 = 1000;
                                    if (__instance.name.Contains("Slaughter House"))
                                    {
                                        extendedProcessingFacilityAI.m_outputResource1 = TransferManager.TransferReason.AnimalProducts;
                                        extendedProcessingFacilityAI.m_outputResource2 = ExtendedTransferManager.RawHides;
                                        extendedProcessingFacilityAI.m_outputRate1 = 1000;
                                        extendedProcessingFacilityAI.m_outputRate2 = 1000;
                                    }
                                    else if (__instance.name.Contains("Milking Parlour"))
                                    {
                                        extendedProcessingFacilityAI.m_outputResource1 = ExtendedTransferManager.Milk;
                                        extendedProcessingFacilityAI.m_outputResource2 = TransferManager.TransferReason.None;
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
                        __instance.m_class.m_service = (ItemClass.Service)28;
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