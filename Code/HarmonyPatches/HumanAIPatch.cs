using ColossalFramework;
using HarmonyLib;
using MoreTransferReasons;
using IndustriesMeetsSunsetHarbor.Managers;
using System;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(HumanAI))]
    public static class HumanAIPatch
    {
        [HarmonyPatch(typeof(HumanAI), "FindVisitPlace")]
        [HarmonyPrefix]
        public static bool FindVisitPlace(HumanAI __instance, uint citizenID, ushort sourceBuilding, TransferManager.TransferReason reason)
        {
            if(reason == GetShoppingReason() || reason == GetEntertainmentReason())
            {
                bool get_delivery = false;
                var homeBuildingData = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)sourceBuilding];
                var citizen = Singleton<CitizenManager>.instance.m_citizens.m_buffer[citizenID];
                var waiting_delivery = RestaurantDeliveriesManager.RestaurantDeliveries.FindIndex(item => item.citizenId == citizenID);
                if (waiting_delivery != -1) // already waiting for delivery do nothing
                {
                    return false;
                }
                else // not waiting for delivery 50% chance ordering a delivery by default
                {
                    if (Singleton<SimulationManager>.instance.m_randomizer.Int32(100U) < Mod.DeliveryChance)
                    {
                        get_delivery = true;
                    }
                }
                var level = GetRestaurantQuality(citizen.WealthLevel);
                if (get_delivery)
                {
                    if (level == 1)
                    {
                        int count1 = 0;
                        int cargo1 = 0;
                        int capacity1 = 0;
                        int outside1 = 0;
                        ExtedndedVehicleManager.CalculateGuestVehicles(sourceBuilding, ref homeBuildingData, ExtendedTransferManager.TransferReason.MealsDeliveryLow, ref count1, ref cargo1, ref capacity1, ref outside1);
                        ExtendedTransferManager.Offer transferOffer1 = default;
                        transferOffer1.Citizen = citizenID;
                        transferOffer1.Position = homeBuildingData.m_position;
                        transferOffer1.Amount = 1;
                        transferOffer1.Active = false;
                        Singleton<ExtendedTransferManager>.instance.AddOutgoingOffer(ExtendedTransferManager.TransferReason.MealsDeliveryLow, transferOffer1);
                    }
                    else if (level == 2)
                    {
                        int count2 = 0;
                        int cargo2 = 0;
                        int capacity2 = 0;
                        int outside2 = 0;
                        ExtedndedVehicleManager.CalculateGuestVehicles(sourceBuilding, ref homeBuildingData, ExtendedTransferManager.TransferReason.MealsDeliveryMedium, ref count2, ref cargo2, ref capacity2, ref outside2);
                        ExtendedTransferManager.Offer transferOffer2 = default;
                        transferOffer2.Citizen = citizenID;
                        transferOffer2.Position = homeBuildingData.m_position;
                        transferOffer2.Amount = 1;
                        transferOffer2.Active = false;
                        Singleton<ExtendedTransferManager>.instance.AddOutgoingOffer(ExtendedTransferManager.TransferReason.MealsDeliveryMedium, transferOffer2);
                    }
                    else if (level == 3)
                    {
                        int count3 = 0;
                        int cargo3 = 0;
                        int capacity3 = 0;
                        int outside3 = 0;
                        ExtedndedVehicleManager.CalculateGuestVehicles(sourceBuilding, ref homeBuildingData, ExtendedTransferManager.TransferReason.MealsDeliveryHigh, ref count3, ref cargo3, ref capacity3, ref outside3);
                        ExtendedTransferManager.Offer transferOffer3 = default;
                        transferOffer3.Citizen = citizenID;
                        transferOffer3.Position = homeBuildingData.m_position;
                        transferOffer3.Amount = 1;
                        transferOffer3.Active = false;
                        Singleton<ExtendedTransferManager>.instance.AddOutgoingOffer(ExtendedTransferManager.TransferReason.MealsDeliveryHigh, transferOffer3);
                    }
                    return false;
                }
                if(Singleton<SimulationManager>.instance.m_randomizer.Int32(100U) < Mod.VisitChance)
                {
                    if (level == 1)
                    {
                        ExtendedTransferManager.Offer transferOffer4 = default;
                        transferOffer4.Citizen = citizenID;
                        transferOffer4.Position = homeBuildingData.m_position;
                        transferOffer4.Amount = 1;
                        transferOffer4.Active = false;
                        Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(ExtendedTransferManager.TransferReason.MealsLow, transferOffer4);
                    }
                    else if (level == 2)
                    {
                        ExtendedTransferManager.Offer transferOffer5 = default;
                        transferOffer5.Citizen = citizenID;
                        transferOffer5.Position = homeBuildingData.m_position;
                        transferOffer5.Amount = 1;
                        transferOffer5.Active = false;
                        Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(ExtendedTransferManager.TransferReason.MealsMedium, transferOffer5);
                    }
                    else if (level == 3)
                    {
                        ExtendedTransferManager.Offer transferOffer6 = default;
                        transferOffer6.Citizen = citizenID;
                        transferOffer6.Position = homeBuildingData.m_position;
                        transferOffer6.Amount = 1;
                        transferOffer6.Active = false;
                        Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(ExtendedTransferManager.TransferReason.MealsHigh, transferOffer6);
                    }
                    return false;
                }  
            }
            return true;
        }


        [HarmonyPatch(typeof(HumanAI), "StartMoving", new Type[] { typeof(uint), typeof(Citizen), typeof(ushort), typeof(TransferManager.TransferOffer) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal })]
        [HarmonyPrefix]
        public static bool StartMoving(HumanAI __instance, uint citizenID, ref Citizen data, ushort sourceBuilding, TransferManager.TransferOffer offer, ref bool __result)
        {
            var waiting_delivery = RestaurantDeliveriesManager.RestaurantDeliveries.FindIndex(item => item.citizenId == citizenID);
            if(waiting_delivery != -1) // don't start moving if waiting for delivery
            {
                __result = false;
                return false;
            }
            return true;
        }

        private static int GetRestaurantQuality(Citizen.Wealth level)
        {
            var random = Singleton<SimulationManager>.instance.m_randomizer.Int32(100U);
            switch (level)
            {
                case Citizen.Wealth.Low:
                    // Quality 1's should be mainly for Low Wealth citizens, but not impossible for medium
                    if (random <= 80)
                    {
                        return 1;
                    }
                    else
                    {
                        return 2;
                    }
                case Citizen.Wealth.Medium:
                    // Quality 2 are ideal for medium wealth citizens, but possible for all
                    if (random > 50 && random <= 90)
                    {
                        return 1;
                    }
                    else if (random > 90)
                    {
                        return 3;
                    }
                    else
                    {
                        return 2;
                    }
                case Citizen.Wealth.High:
                    // Quality 3's are best suited for high wealth citizens, but some medium wealth citizens can afford it
                    if (random > 70 && random <= 90)
                    {
                        return 2;
                    }
                    else if (random > 90)
                    {
                        return 1;
                    }
                    else
                    {
                        return 3;
                    }
            }
            return 2;
        }

        private static TransferManager.TransferReason GetShoppingReason()
        {
	    return Singleton<SimulationManager>.instance.m_randomizer.Int32(8u) switch
	    {
		0 => TransferManager.TransferReason.Shopping, 
		1 => TransferManager.TransferReason.ShoppingB, 
		2 => TransferManager.TransferReason.ShoppingC, 
		3 => TransferManager.TransferReason.ShoppingD, 
		4 => TransferManager.TransferReason.ShoppingE, 
		5 => TransferManager.TransferReason.ShoppingF, 
		6 => TransferManager.TransferReason.ShoppingG, 
		7 => TransferManager.TransferReason.ShoppingH, 
		_ => TransferManager.TransferReason.Shopping, 
	    };
        }

        private static TransferManager.TransferReason GetEntertainmentReason()
        {
	    return Singleton<SimulationManager>.instance.m_randomizer.Int32(4u) switch
	    {
		0 => TransferManager.TransferReason.Entertainment, 
		1 => TransferManager.TransferReason.EntertainmentB, 
		2 => TransferManager.TransferReason.EntertainmentC, 
		3 => TransferManager.TransferReason.EntertainmentD, 
		_ => TransferManager.TransferReason.Entertainment, 
	    };
        }
    }
}
