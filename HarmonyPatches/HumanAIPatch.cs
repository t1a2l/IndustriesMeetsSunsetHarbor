using ColossalFramework;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.Managers;
using IndustriesMeetsSunsetHarbor.AI;
using System;
using System.Collections.Generic;
using UnityEngine;
using MoreTransferReasons;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(HumanAI))]
    public static class HumanAIPatch
    {
        [HarmonyPatch(typeof(HumanAI), "FindVisitPlace")]
        [HarmonyPrefix]
        public static bool FindVisitPlace(HumanAI __instance, uint citizenID, ushort sourceBuilding, TransferManager.TransferReason reason)
        {
            var citizen = Singleton<CitizenManager>.instance.m_citizens.m_buffer[citizenID];
            if(IsShoppingReason(reason) && Citizen.GetAgeGroup(citizen.Age) == Citizen.AgeGroup.Senior || Citizen.GetAgeGroup(citizen.Age) == Citizen.AgeGroup.Adult || Citizen.GetAgeGroup(citizen.Age) == Citizen.AgeGroup.Young)
            {
                bool get_delivery = false;
                var homeBuildingData = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)sourceBuilding];
                
                var waiting_delivery = RestaurantManager.IsCitizenWaitingForDelivery(citizenID);
                if (waiting_delivery) // already waiting for delivery do nothing
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
                    var material = ExtendedTransferManager.TransferReason.None;
                    // Quality 1's should be mainly for Low Wealth citizens, but not impossible for medium and high
                    if (level == 1)
                    {
                        material = ExtendedTransferManager.TransferReason.MealsDeliveryLow;
                    }
                    // Quality 2 are ideal for medium wealth citizens, but possible for all
                    else if (level == 2)
                    {
                        material = ExtendedTransferManager.TransferReason.MealsDeliveryMedium;
                    }
                    // Quality 3's are best suited for high wealth citizens, but some medium wealth citizens can afford it
                    else if (level == 3)
                    {
                        material = ExtendedTransferManager.TransferReason.MealsDeliveryHigh;
                    }
                    if(material != ExtendedTransferManager.TransferReason.None)
                    {
                        ExtendedTransferManager.Offer transferOffer = default;
                        transferOffer.Citizen = citizenID;
                        transferOffer.Position = homeBuildingData.m_position;
                        transferOffer.Amount = 1;
                        transferOffer.Active = false;
                        Singleton<ExtendedTransferManager>.instance.AddOutgoingOffer(material, transferOffer);
                    }
                    
                    return false;
                }
                if(Singleton<SimulationManager>.instance.m_randomizer.Int32(100U) < Mod.VisitChance)
                {
                    var material = ExtendedTransferManager.TransferReason.None;
                    // Quality 1's should be mainly for Low Wealth citizens, but not impossible for medium and high
                    if (level == 1)
                    {
                        material = ExtendedTransferManager.TransferReason.MealsLow;
                    }
                    // Quality 2 are ideal for medium wealth citizens, but possible for all
                    else if (level == 2)
                    {
                        material = ExtendedTransferManager.TransferReason.MealsMedium;
                    }
                    // Quality 3's are best suited for high wealth citizens, but some medium wealth citizens can afford it
                    else if (level == 3)
                    {
                        material = ExtendedTransferManager.TransferReason.MealsHigh;
                    }
                    if(material != ExtendedTransferManager.TransferReason.None)
                    {
                        ExtendedTransferManager.Offer transferOffer = default;
                        transferOffer.Citizen = citizenID;
                        transferOffer.Position = homeBuildingData.m_position;
                        transferOffer.Amount = 1;
                        transferOffer.Active = true;
                        Singleton<ExtendedTransferManager>.instance.AddIncomingOffer(material, transferOffer);
                    }
                    return false;
                }  
            }
            return true;
        }


        [HarmonyPatch(typeof(HumanAI), "StartMoving", new Type[] { typeof(uint), typeof(Citizen), typeof(ushort), typeof(TransferManager.TransferOffer) },
            [ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal])]
        [HarmonyPrefix]
        public static bool StartMoving(HumanAI __instance, uint citizenID, ref Citizen data, ushort sourceBuilding, TransferManager.TransferOffer offer, ref bool __result)
        {
            var waiting_delivery = RestaurantManager.IsCitizenWaitingForDelivery(citizenID);
            if(waiting_delivery) // don't start moving if waiting for delivery
            {
                __result = false;
                return false;
            }
            return true;
        }


        [HarmonyPatch(typeof(HumanAI), "GetColor")]
        [HarmonyPrefix]
        public static bool GetColor(ushort instanceID, ref CitizenInstance data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode, ref Color __result)
        {
            if (infoMode == (InfoManager.InfoMode)41)
            {
                BuildingManager instance2 = Singleton<BuildingManager>.instance;
                uint citizen = data.m_citizen;
                var building_info = instance2.m_buildings.m_buffer[data.m_targetBuilding].Info;
                var waiting_delivery = RestaurantManager.IsCitizenWaitingForDelivery(citizen);
                if (waiting_delivery || building_info.GetAI() is RestaurantAI)
		{
		    __result = Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_targetColor;
		}
                else
                {
                    __result = Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                }
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
                    // 80% chance of choosing a low end restaurant, 20% chance of choosing a medium end restaurant
                    if (random <= 80)
                    {
                        return 1; 
                    }
                    else
                    {
                        return 2;
                    }
                case Citizen.Wealth.Medium:
                    // 40% chance of choosing a low end restaurant, 50% chance of choosing a medium end restaurant
                    // 10% chance of choosing a high end restaurant
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
                    // 10% chance of choosing a low end restaurant, 20% chance of choosing a medium end restaurant
                    // 70% chance of choosing a high end restaurant
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

        private static bool IsShoppingReason(TransferManager.TransferReason reason)
        {
            List<TransferManager.TransferReason> ShoppingList = new()
            {
                TransferManager.TransferReason.Shopping, 
		TransferManager.TransferReason.ShoppingB, 
		TransferManager.TransferReason.ShoppingC, 
		TransferManager.TransferReason.ShoppingD, 
		TransferManager.TransferReason.ShoppingE, 
		TransferManager.TransferReason.ShoppingF, 
		TransferManager.TransferReason.ShoppingG, 
		TransferManager.TransferReason.ShoppingH
            };

            if(ShoppingList.Contains(reason)) return true;
            return false;
        }
    }
}
