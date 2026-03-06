using System;
using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;
using MoreTransferReasons;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(TransferManager))]
    public static class TransferManagerPatch
    {
        private static readonly TransferManager.TransferReason[] UniqueFactoryProducts =
        [
            ExtendedTransferManager.BakedGoods,
            ExtendedTransferManager.BeverageProducts,
            ExtendedTransferManager.CannedFish,
            ExtendedTransferManager.Cloths,
            ExtendedTransferManager.ElectronicProducts,
            ExtendedTransferManager.FoodProducts,
            ExtendedTransferManager.Footwear,
            ExtendedTransferManager.Furnitures,
            ExtendedTransferManager.HouseParts,
            ExtendedTransferManager.PrintedProducts,
            ExtendedTransferManager.Toys,
            ExtendedTransferManager.Tupperware,
            ExtendedTransferManager.TissuePaper
        ];

        [HarmonyPatch(typeof(TransferManager), "AddIncomingOffer")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.High)]
        public static void Prefix(ref TransferManager.TransferReason material, ref TransferManager.TransferOffer offer)
        {
            if (offer.Building != 0)
            {
                if (material == TransferManager.TransferReason.LuxuryProducts)
                {
                    var buildingData = Singleton<BuildingManager>.instance.m_buildings.m_buffer[offer.Building];
                    if (buildingData.Info.m_class.m_service == ItemClass.Service.Commercial)
                    {
                        byte currentQuality = buildingData.m_education2;
                        int qualityBonus = (2 - currentQuality) * 2; // low stock quality = higher priority = attracts more trucks
                        offer.Priority = Mathf.Clamp(offer.Priority + qualityBonus, 1, 7);
                        uint week = (uint)(Singleton<SimulationManager>.instance.m_currentGameTime.Ticks / (TimeSpan.TicksPerDay * 7));
                        var rng = new Randomizer((uint)offer.Building ^ week);
                        material = UniqueFactoryProducts[rng.Int32((uint)UniqueFactoryProducts.Length)];
                    }
                } 
            }
        }
    }
}
