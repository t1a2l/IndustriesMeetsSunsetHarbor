using ColossalFramework;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.AI;
using IndustriesMeetsSunsetHarbor.Managers;
using MoreTransferReasons;
using MoreTransferReasons.AI;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(CarAI))]
    public static class CarAIPatch
    {
        [HarmonyPatch(typeof(CarAI), "CreateVehicle")]
        [HarmonyPostfix]
        public static void CreateVehicle(CarAI __instance, ushort vehicleID, ref Vehicle data)
        {
            if(__instance is ExtendedPassengerCarAI)
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
            if (__instance is ExtendedPassengerCarAI || __instance is ExtendedCargoTruckAI)
            {
                VehicleFuelManager.RemoveVehicleFuel(vehicleID);
            }
        }

        [HarmonyPatch(typeof(CarAI), "CalculateTargetSpeed")]
        [HarmonyPostfix]
        public static void CalculateTargetSpeed(CarAI __instance, ushort vehicleID, ref Vehicle data, float speedLimit, float curve, ref float __result)
        {
            if (__instance is ExtendedPassengerCarAI || __instance is ExtendedCargoTruckAI)
            {
                var vehicleFuel = VehicleFuelManager.GetVehicleFuel(vehicleID);
                if(vehicleFuel.CurrentFuelCapacity < 10)
                {
                    __result = 0.5f;
                }
            }
        }

        [HarmonyPatch(typeof(CarAI), "SimulationStep",
            [typeof(ushort), typeof(Vehicle), typeof(Vehicle.Frame), typeof(ushort), typeof(Vehicle), typeof(int)],
            [ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal])]
        [HarmonyPrefix]
        public static void SimulationStep(CarAI __instance, ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            if (__instance is ExtendedPassengerCarAI || __instance is ExtendedCargoTruckAI)
            {
                var vehicleFuel = VehicleFuelManager.GetVehicleFuel(vehicleID);
                float percent = vehicleFuel.CurrentFuelCapacity / vehicleFuel.MaxFuelCapacity;
                bool shouldFuel = Singleton<SimulationManager>.instance.m_randomizer.Int32(16) == 0;
                if (percent > 0.1 && shouldFuel || percent <= 0.1)
                {
                    ExtendedTransferManager.Offer offer = default;
                    offer.Vehicle = vehicleID;
                    offer.Position = vehicleData.GetLastFramePosition();
                    offer.Active = true;
                    Singleton<ExtendedTransferManager>.instance.AddOutgoingOffer(ExtendedTransferManager.TransferReason.FuelVehicle, offer);
                }
            }
        }

        [HarmonyPatch(typeof(PassengerCarAI), "ArriveAtTarget")]
        [HarmonyPrefix]
        public static bool PassengerCarAIPrefix(PassengerCarAI __instance, ushort vehicleID, ref Vehicle data, ref bool __result)
        {
            var building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding];
            var distance = Vector3.Distance(data.GetLastFramePosition(), building.m_position);
            if (building.Info.GetAI() is GasStationAI gasStationAI && distance < 80f)
            {
                FuelVehicle(vehicleID, ref data, gasStationAI, ref building);
                __instance.SetTarget(vehicleID, ref data, 0);
                __result = true;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(CargoTruckAI), "ArriveAtTarget")]
        [HarmonyPrefix]
        public static bool CargoTruckAIPrefix(CargoTruckAI __instance, ushort vehicleID, ref Vehicle data, ref bool __result)
        {
            if (data.m_transferType >= 200)
            {
                var transferType = (byte)(data.m_transferType - 200);
                if ((ExtendedTransferManager.TransferReason)transferType == ExtendedTransferManager.TransferReason.FuelVehicle)
                {
                    var building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding];
                    var distance = Vector3.Distance(data.GetLastFramePosition(), building.m_position);
                    if (building.Info.GetAI() is GasStationAI gasStationAI && distance < 80f)
                    {
                        FuelVehicle(vehicleID, ref data, gasStationAI, ref building);
                        __instance.SetTarget(vehicleID, ref data, 0);
                        __result = true;
                        return false;
                    }
                }
                else if ((ExtendedTransferManager.TransferReason)transferType == ExtendedTransferManager.TransferReason.PetroleumProducts)
                {
                    var building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding];
                    var distance = Vector3.Distance(data.GetLastFramePosition(), building.m_position);
                    if (building.Info.GetAI() is GasStationAI gasStationAI && distance < 80f)
                    {
                        int transferSize = (int)data.m_transferSize;
                        gasStationAI.ExtendedModifyMaterialBuffer(data.m_targetBuilding, ref building, ExtendedTransferManager.TransferReason.PetroleumProducts, ref transferSize);
                        __instance.SetTarget(vehicleID, ref data, 0);
                        __result = true;
                        return false;
                    }
                }
            }
            return true;
        }

        private static void FuelVehicle(ushort vehicleID, ref Vehicle data, GasStationAI gasStationAI, ref Building building)
        {
            data.m_flags |= Vehicle.Flags.Stopped;
            var vehicleFuel = VehicleFuelManager.GetVehicleFuel(vehicleID);
            var neededFuel = vehicleFuel.MaxFuelCapacity - vehicleFuel.CurrentFuelCapacity;
            gasStationAI.ExtendedModifyMaterialBuffer(data.m_targetBuilding, ref building, ExtendedTransferManager.TransferReason.FuelVehicle, ref neededFuel);
            VehicleFuelManager.SetVehicleFuel(vehicleID, neededFuel);
            
        }
    }
}
