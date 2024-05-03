using System.Collections.Generic;

namespace IndustriesMeetsSunsetHarbor.Managers
{
    public static class VehicleFuelManager
    {
        public static Dictionary<ushort, ushort> VehiclesFuel;

        public static void Init()
        {
            VehiclesFuel ??= [];
        }

        public static void Deinit() => VehiclesFuel = [];

        public static ushort GetVehicleFuel(ushort vehicleId) => !VehiclesFuel.TryGetValue(vehicleId, out var fuelCapacity) ? default : fuelCapacity;

        public static void CreateVehicleFuel(ushort vehicleId)
        {
            if (!VehiclesFuel.TryGetValue(vehicleId, out _))
            {
                VehiclesFuel.Add(vehicleId, 0);
            }
        }

        public static void SetVehicleFuel(ushort vehicleId, ushort fuelCapacity) => VehiclesFuel[vehicleId] = fuelCapacity;


        public static void RemoveVehicleFuel(ushort vehicleId) => VehiclesFuel.Remove(vehicleId);
    }
}
