using ColossalFramework;
using HarmonyLib;
using System.Reflection;
using MoreTransferReasons.Code;
using IndustriesMeetsSunsetHarbor.Utils;
using System;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(HumanAI))]
    public static class HumanAIPatch
    {
        private delegate bool GetHomeBehaviourCommonBuildingAIDelegate(CommonBuildingAI __instance, ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveCount, ref int totalCount, ref int homeCount, ref int aliveHomeCount, ref int emptyHomeCount);
        private static readonly GetHomeBehaviourCommonBuildingAIDelegate GetHomeBehaviour = AccessTools.MethodDelegate<GetHomeBehaviourCommonBuildingAIDelegate>(typeof(CommonBuildingAI).GetMethod("GetHomeBehaviour", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        [HarmonyPatch(typeof(HumanAI), "FindVisitPlace")]
        [HarmonyPrefix]
        public static bool FindVisitPlace(HumanAI __instance, uint citizenID, ushort sourceBuilding, TransferManager.TransferReason reason)
        {
            bool get_delivery = false;
            var homeBuildingData = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)sourceBuilding];
            var citizen = Singleton<CitizenManager>.instance.m_citizens.m_buffer[citizenID];
            if((citizen.m_flags & (Citizen.Flags)1048576) != Citizen.Flags.None) // already waiting for delivery do nothing
            {
                return false;
            }
            if((citizen.m_flags & (Citizen.Flags)1048576) == Citizen.Flags.None) // not waiting for delivery 50% chance ordering a delivery
            {
                Random rand = new Random();
                if (rand.Next(0, 2) != 0)
                {
                    get_delivery = true;
                    citizen.m_flags |= (Citizen.Flags)1048576; // raise flag as getting delivery for citizen
                    // if building not already waiting for delivery
                    if((homeBuildingData.m_flags & Building.Flags.Incoming) == Building.Flags.None)
                    {
                        homeBuildingData.m_flags |= Building.Flags.Incoming; // raise flag as building waitng for delivery
                    }
                    
                }
            }
            if(get_delivery) 
            {
                
                if(citizen.WealthLevel == Citizen.Wealth.Low)
                {
                    int count1 = 0;
                    int cargo1 = 0;
                    int capacity1 = 0;
                    int outside1 = 0;
                    CalculateVehicles.CalculateGuestVehicles(sourceBuilding, ref homeBuildingData, ExtendedTransferManager.TransferReason.MealsDeliveryLow, ref count1, ref cargo1, ref capacity1, ref outside1);
                    ExtendedTransferManager.Offer transferOffer1 = default(ExtendedTransferManager.Offer);
                    transferOffer1.Building = sourceBuilding;
                    transferOffer1.Position = homeBuildingData.m_position;
                    transferOffer1.Amount = 1;
                    transferOffer1.Active = false;
                    Singleton<ExtendedTransferManager>.instance.AddOutgoingOffer(ExtendedTransferManager.TransferReason.MealsDeliveryLow, transferOffer1);
                }
                else if(citizen.WealthLevel == Citizen.Wealth.Medium)
                {
                    int count2 = 0;
                    int cargo2 = 0;
                    int capacity2 = 0;
                    int outside2 = 0;
                    CalculateVehicles.CalculateGuestVehicles(sourceBuilding, ref homeBuildingData, ExtendedTransferManager.TransferReason.MealsDeliveryMedium, ref count2, ref cargo2, ref capacity2, ref outside2);
                    ExtendedTransferManager.Offer transferOffer2 = default(ExtendedTransferManager.Offer);
                    transferOffer2.Building = sourceBuilding;
                    transferOffer2.Position = homeBuildingData.m_position;
                    transferOffer2.Amount = 1;
                    transferOffer2.Active = false;
                    Singleton<ExtendedTransferManager>.instance.AddOutgoingOffer(ExtendedTransferManager.TransferReason.MealsDeliveryMedium, transferOffer2);
                }
                else if(citizen.WealthLevel == Citizen.Wealth.High)
                {
                    int count3 = 0;
                    int cargo3 = 0;
                    int capacity3 = 0;
                    int outside3 = 0;
                    CalculateVehicles.CalculateGuestVehicles(sourceBuilding, ref homeBuildingData, ExtendedTransferManager.TransferReason.MealsDeliveryHigh, ref count3, ref cargo3, ref capacity3, ref outside3);
                    ExtendedTransferManager.Offer transferOffer3 = default(ExtendedTransferManager.Offer);
                    transferOffer3.Building = sourceBuilding;
                    transferOffer3.Position = homeBuildingData.m_position;
                    transferOffer3.Amount = 1;
                    transferOffer3.Active = false;
                    Singleton<ExtendedTransferManager>.instance.AddOutgoingOffer(ExtendedTransferManager.TransferReason.MealsDeliveryHigh, transferOffer3);
                }
                return false;
            }
            return true;
        }

        
    }
}
