using System.Collections.Generic;

namespace IndustriesMeetsSunsetHarbor.Managers
{
    public static class VehicleFuelManager
    {
        public static Dictionary<ushort, VehicleFuelCapacity> VehiclesFuel;

        public struct VehicleFuelCapacity
        {
            public int CurrentFuelCapacity;
            public int MaxFuelCapacity;
        }


        public static void Init()
        {
            VehiclesFuel ??= [];
        }

        public static void Deinit() => VehiclesFuel = [];

        public static VehicleFuelCapacity GetVehicleFuel(ushort vehicleId) => !VehiclesFuel.TryGetValue(vehicleId, out var fuelCapacity) ? default : fuelCapacity;

        public static void CreateVehicleFuel(ushort vehicleId, int currentFuelCapacity, int maxFuelCapacity)
        {
            if (!VehiclesFuel.TryGetValue(vehicleId, out _))
            {
                var vehicleFuelCapacity = new VehicleFuelCapacity
                {
                    CurrentFuelCapacity = currentFuelCapacity,
                    MaxFuelCapacity = maxFuelCapacity
                };
                VehiclesFuel.Add(vehicleId, vehicleFuelCapacity);
            }
        }

        public static void SetVehicleFuel(ushort vehicleId, ushort newFuelCapacity)
        {
            var vehicleFuelCapacity = VehiclesFuel[vehicleId];
            vehicleFuelCapacity.CurrentFuelCapacity = newFuelCapacity;
            VehiclesFuel[vehicleId] = vehicleFuelCapacity;
        }


        public static void RemoveVehicleFuel(ushort vehicleId) => VehiclesFuel.Remove(vehicleId);
    }
}
