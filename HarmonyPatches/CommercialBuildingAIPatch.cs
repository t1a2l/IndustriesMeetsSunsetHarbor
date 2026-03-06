using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;
using MoreTransferReasons;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(CommercialBuildingAI))]
    public static class CommercialBuildingAIPatch
    {
        [HarmonyPatch(typeof(CommercialBuildingAI), "ModifyMaterialBuffer")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.High)]
        public static bool ModifyMaterialBuffer(CommercialBuildingAI __instance, ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int amountDelta)
        {
            if (IsUniqueMaterialType(material))
            {
                int width = data.Width;
                int length = data.Length;
                int num = 4000;
                int num2 = __instance.CalculateVisitplaceCount((ItemClass.Level)data.m_level, new Randomizer(buildingID), width, length);
                int num3 = Mathf.Min(Mathf.Max(num2 * 500, num * 4), 65535);
                int goodsAmount = data.m_customBuffer1;
                amountDelta = Mathf.Clamp(amountDelta, 0, num3 - goodsAmount);
                data.m_customBuffer1 = (ushort)(goodsAmount + amountDelta);

                // Find delivering truck
                ushort vehicleID = data.m_guestVehicles;
                while (vehicleID != 0)
                {
                    var vehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleID];
                    if (vehicle.m_transferType == (byte)material && vehicle.m_targetBuilding == buildingID)
                    {
                        byte incomingQuality = (byte)vehicle.m_touristCount;
                        AccumulateQuality(ref data, incomingQuality, amountDelta, num3);
                        break;
                    }
                    vehicleID = vehicle.m_nextGuestVehicle;
                }
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(CommercialBuildingAI), "SimulationStepActive")]
        [HarmonyPostfix]
        public static void SimulationStepActive(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            // Only apply quality bonus if building has received unique factory goods
            if (buildingData.m_education1 == 0)
            {
                return;
            }

            if (buildingData.m_education1 > 0)
            {
                // Faster decay default — quality effect fades in ~30 days
                var decay = (Singleton<SimulationManager>.instance.m_currentFrameIndex & 7) == 0;
                if (Mod.IsRealTimeEnabled)
                {
                    // Slower decay — fades in ~1 year
                    decay = (Singleton<SimulationManager>.instance.m_currentFrameIndex & 31) == 0;
                }
                if (decay)
                {
                    // 255 steps = ~255 days of quality memory after last delivery
                    buildingData.m_education1--;
                }
            }

            float quality = buildingData.m_education2 / 2f; // 0.0 to 1.0
            float visitMultiplier = Mathf.Lerp(0.4f, 1.0f, quality); // 40%–100%

            int attractivenessBonus = Mathf.RoundToInt(50 * visitMultiplier);
            int entertainmentBonus = Mathf.RoundToInt(30 * visitMultiplier);

            Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Attractiveness, attractivenessBonus, buildingData.m_position, 150f); // radius

            Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Entertainment, entertainmentBonus, buildingData.m_position, 100f);
        }

        private static bool IsUniqueMaterialType(TransferManager.TransferReason material)
        {
            return material == ExtendedTransferManager.BakedGoods ||
                material == ExtendedTransferManager.BeverageProducts ||
                material == ExtendedTransferManager.CannedFish ||
                material == ExtendedTransferManager.Cars ||
                material == ExtendedTransferManager.ChemicalProducts ||
                material == ExtendedTransferManager.Cloths ||
                material == ExtendedTransferManager.ElectronicProducts ||
                material == ExtendedTransferManager.FoodProducts ||
                material == ExtendedTransferManager.Footwear ||
                material == ExtendedTransferManager.Furnitures ||
                material == ExtendedTransferManager.HouseParts ||
                material == ExtendedTransferManager.IndustrialSteel ||
                material == ExtendedTransferManager.PetroleumProducts ||
                material == ExtendedTransferManager.PrintedProducts ||
                material == ExtendedTransferManager.Toys ||
                material == ExtendedTransferManager.TissuePaper ||
                material == ExtendedTransferManager.Tupperware ||
                material == TransferManager.TransferReason.LuxuryProducts;
        }

        private static void AccumulateQuality(ref Building data, byte incomingQuality, int amount, int bufferCapacity)
        {
            data.m_education1 = 255; // mark as having received quality goods

            // weighted average: blend existing quality toward incoming
            // weight = amount delivered vs total buffer size (smoothing factor)
            float weight = Mathf.Clamp01(amount / (float)Mathf.Max(1, bufferCapacity));
            float current = data.m_education2 / 2f; // normalize to 0..1
            float incoming = incomingQuality / 2f; // normalize to 0..1
            float blended = Mathf.Lerp(current, incoming, weight * 0.3f); // smooth
            data.m_education2 = (byte)Mathf.RoundToInt(blended * 2f);
        }
    }
}
