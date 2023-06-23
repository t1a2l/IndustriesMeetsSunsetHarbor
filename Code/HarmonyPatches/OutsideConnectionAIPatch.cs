using ColossalFramework;
using HarmonyLib;
using MoreTransferReasons;
using UnityEngine;
using static BuildingAI;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(OutsideConnectionAI))]
    public static class OutsideConnectionAIPatch
    {
        public static ExtendedTransferManager.TransferReason m_dummyTrafficReason = ExtendedTransferManager.TransferReason.None;

        [HarmonyPatch(typeof(OutsideConnectionAI), "ReleaseBuilding")]
        [HarmonyPrefix]
        public static void ReleaseBuilding(ushort buildingID, ref Building data)
        {
            RemoveExtendedConnectionOffers(buildingID, ref data, m_dummyTrafficReason);
        }

        [HarmonyPatch(typeof(OutsideConnectionAI), "SimulationStep")]
        [HarmonyPostfix]
        public static void SimulationStep(OutsideConnectionAI __instance, ushort buildingID, ref Building data)
	{
	    if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.Game) != 0)
	    {
		int budget = Singleton<EconomyManager>.instance.GetBudget(__instance.m_info.m_class);
		int productionRate = GetProductionRate(100, budget);
		AddConnectionOffers(buildingID, ref data, productionRate, __instance.m_cargoCapacity, m_dummyTrafficReason, __instance.m_dummyTrafficFactor);
	    }
	}

        private static void RemoveExtendedConnectionOffers(ushort buildingID, ref Building data, ExtendedTransferManager.TransferReason dummyTrafficReason)
        {
            ExtendedTransferManager instance = Singleton<ExtendedTransferManager>.instance;
            if ((data.m_flags & Building.Flags.Outgoing) != 0)
            {
                ExtendedTransferManager.Offer offer = default;
                offer.Building = buildingID;
                instance.RemoveOutgoingOffer(ExtendedTransferManager.TransferReason.DrinkSupplies, offer);
                instance.RemoveOutgoingOffer(ExtendedTransferManager.TransferReason.FoodSupplies, offer);
                instance.RemoveOutgoingOffer(ExtendedTransferManager.TransferReason.Bread, offer);
                instance.RemoveOutgoingOffer(ExtendedTransferManager.TransferReason.CannedFish, offer);
                if (dummyTrafficReason != ExtendedTransferManager.TransferReason.None)
                {
                    instance.RemoveOutgoingOffer(dummyTrafficReason, offer);
                }
            }
            if ((data.m_flags & Building.Flags.Incoming) != 0)
            {
                ExtendedTransferManager.Offer offer2 = default;
                offer2.Building = buildingID;
                instance.RemoveIncomingOffer(ExtendedTransferManager.TransferReason.DrinkSupplies, offer2);
                instance.RemoveIncomingOffer(ExtendedTransferManager.TransferReason.FoodSupplies, offer2);
                instance.RemoveIncomingOffer(ExtendedTransferManager.TransferReason.Bread, offer2);
                instance.RemoveIncomingOffer(ExtendedTransferManager.TransferReason.CannedFish, offer2);
                if (dummyTrafficReason != ExtendedTransferManager.TransferReason.None)
                {
                    instance.RemoveIncomingOffer(dummyTrafficReason, offer2);
                }
            }
        }

        private static void AddConnectionOffers(ushort buildingID, ref Building data, int productionRate, int cargoCapacity, ExtendedTransferManager.TransferReason dummyTrafficReason, int dummyTrafficFactor)
        {
            SimulationManager instance = Singleton<SimulationManager>.instance;
            ExtendedTransferManager instance2 = Singleton<ExtendedTransferManager>.instance;
            cargoCapacity = (cargoCapacity * productionRate + 99) / 100;
            dummyTrafficFactor = (dummyTrafficFactor * productionRate + 99) / 100;
            dummyTrafficFactor = (dummyTrafficFactor * DummyTrafficProbability() + 99) / 100;
            int num2 = (cargoCapacity + instance.m_randomizer.Int32(16u)) / 16;
            if ((data.m_flags & Building.Flags.Outgoing) != 0)
            {
                int num6 = TickPathfindStatus(buildingID, ref data, PathFindType.LeavingDummy);
                ExtendedTransferManager.Offer offer = default;
                offer.Building = buildingID;
                offer.Position = data.m_position * ((float)instance.m_randomizer.Int32(100, 400) * 0.01f);
                offer.Active = true;
                int num9 = (dummyTrafficFactor + instance.m_randomizer.Int32(100u)) / 100;
                if (num9 > 0 && dummyTrafficReason != ExtendedTransferManager.TransferReason.None)
                {
                    num9 = num9 * num6 + instance.m_randomizer.Int32(256u) >> 8;
                    if (num9 == 0)
                    {

                        offer.Amount = 1;
                        if (instance.m_randomizer.Int32(4u) == 0)
                        {
                            instance2.AddOutgoingOffer(dummyTrafficReason, offer);
                        }
                    }
                    else
                    {
                        offer.Amount = num9;
                        instance2.AddOutgoingOffer(dummyTrafficReason, offer);
                    }
                }
            }
            if ((data.m_flags & Building.Flags.Incoming) == 0)
            {
                return;
            }
            int num17 = TickPathfindStatus(buildingID, ref data, PathFindType.EnteringCargo);
            int num18 = TickPathfindStatus(buildingID, ref data, PathFindType.EnteringDummy);
            ExtendedTransferManager.Offer offer2 = default;
            offer2.Building = buildingID;
            offer2.Position = data.m_position * ((float)instance.m_randomizer.Int32(100, 400) * 0.01f);
            offer2.Active = false;
            int num19 = num2;
            if (num19 != 0)
            {
                if (num19 * num17 + instance.m_randomizer.Int32(256u) >> 8 == 0)
                {
                    offer2.Amount = 1;
                    if (instance.m_randomizer.Int32(16u) == 0)
                    {
                        instance2.AddIncomingOffer(ExtendedTransferManager.TransferReason.DrinkSupplies, offer2);
                    }
                    if (instance.m_randomizer.Int32(16u) == 0)
                    {
                        instance2.AddIncomingOffer(ExtendedTransferManager.TransferReason.FoodSupplies, offer2);
                    }
                    if (instance.m_randomizer.Int32(16u) == 0)
                    {
                        instance2.AddIncomingOffer(ExtendedTransferManager.TransferReason.Bread, offer2);
                    }
                    if (instance.m_randomizer.Int32(16u) == 0)
                    {
                        instance2.AddIncomingOffer(ExtendedTransferManager.TransferReason.CannedFish, offer2);
                    }
                }
                else
                {
                    offer2.Amount = num2;
                    instance2.AddIncomingOffer(ExtendedTransferManager.TransferReason.DrinkSupplies, offer2);
                    instance2.AddIncomingOffer(ExtendedTransferManager.TransferReason.FoodSupplies, offer2);
                    instance2.AddIncomingOffer(ExtendedTransferManager.TransferReason.Bread, offer2);
                    instance2.AddIncomingOffer(ExtendedTransferManager.TransferReason.CannedFish, offer2);
                }
            }
            int num21 = (dummyTrafficFactor + instance.m_randomizer.Int32(100u)) / 100;
            if (num21 > 0 && dummyTrafficReason != ExtendedTransferManager.TransferReason.None)
            {
                num21 = num21 * num18 + instance.m_randomizer.Int32(256u) >> 8;
                if (num21 == 0)
                {
                    offer2.Amount = 1;
                    if (instance.m_randomizer.Int32(4u) == 0)
                    {
                        instance2.AddIncomingOffer(dummyTrafficReason, offer2);
                    }
                }
                else
                {
                    offer2.Amount = num21;
                    instance2.AddIncomingOffer(dummyTrafficReason, offer2);
                }
            }
        }

        private static int DummyTrafficProbability()
	{
	    uint vehicleCount = (uint)Singleton<VehicleManager>.instance.m_vehicleCount;
	    uint instanceCount = (uint)Singleton<CitizenManager>.instance.m_instanceCount;
	    if (vehicleCount * 65536 > instanceCount * 16384)
	    {
		    return 2048000 / (int)(16384 + vehicleCount * 4) - 25;
	    }
	    return 8192000 / (int)(65536 + instanceCount * 4) - 25;
	}

        private static int TickPathfindStatus(ushort buildingID, ref Building data, PathFindType type)
	{
	    return type switch
	    {
		PathFindType.EnteringCargo => TickPathfindStatus(ref data.m_education3, ref data.m_adults), 
		PathFindType.LeavingCargo => TickPathfindStatus(ref data.m_teens, ref data.m_serviceProblemTimer), 
		PathFindType.EnteringHuman => TickPathfindStatus(ref data.m_workerProblemTimer, ref data.m_taxProblemTimer), 
		PathFindType.LeavingHuman => TickPathfindStatus(ref data.m_incomingProblemTimer, ref data.m_seniors), 
		PathFindType.EnteringDummy => TickPathfindStatus(ref data.m_outgoingProblemTimer, ref data.m_education1), 
		PathFindType.LeavingDummy => TickPathfindStatus(ref data.m_youngs, ref data.m_education2), 
		_ => 0, 
	    };
	}

	private static int TickPathfindStatus(ref byte success, ref byte failure)
	{
	    int result = (success << 8) / Mathf.Max(1, success + failure);
	    if (success > failure)
	    {
		success = (byte)(success + 1 >> 1);
		failure >>= 1;
	    }
	    else
	    {
		success >>= 1;
		failure = (byte)(failure + 1 >> 1);
	    }
	    return result;
	}

        public static int GetProductionRate(int productionRate, int budget)
	{
	    if (budget < 100)
	    {
		    budget = (budget * budget + 99) / 100;
	    }
	    else if (budget > 150)
	    {
		    budget = 125;
	    }
	    else if (budget > 100)
	    {
		    budget -= (100 - budget) * (100 - budget) / 100;
	    }
	    return (productionRate * budget + 99) / 100;
	}

    }
}
