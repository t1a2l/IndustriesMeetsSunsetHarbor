using ColossalFramework;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.Managers;
using MoreTransferReasons.AI;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(CarAI))]
    public static class CarAIPatch
    {
        [HarmonyPatch(typeof(CarAI), "CreateVehicle")]
        [HarmonyPostfix]
        public static void CreateVehicle(CarAI __instance, ushort vehicleID, ref Vehicle data)
        {
            if(__instance is PassengerCarAI)
            {
                int randomFuelCapacity = Singleton<SimulationManager>.instance.m_randomizer.Int32(30, 60);
                VehicleFuelManager.CreateVehicleFuel(vehicleID, randomFuelCapacity, 60);
            }
            if (__instance is ExtendedCargoTruckAI)
            {
                int randomFuelCapacity = Singleton<SimulationManager>.instance.m_randomizer.Int32(50, 80);
                VehicleFuelManager.CreateVehicleFuel(vehicleID, randomFuelCapacity, 80);
            }
        }

        [HarmonyPatch(typeof(CarAI), "ReleaseVehicle")]
        [HarmonyPostfix]
        public static void ReleaseVehicle(CarAI __instance, ushort vehicleID, ref Vehicle data)
        {
            if (__instance is PassengerCarAI || __instance is ExtendedCargoTruckAI)
            {
                VehicleFuelManager.RemoveVehicleFuel(vehicleID);
            }
        }
    }
}
