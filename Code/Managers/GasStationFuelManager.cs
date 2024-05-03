using System.Collections.Generic;

namespace IndustriesMeetsSunsetHarbor.Managers
{
    public static class GasStationFuelManager
    {
        public static Dictionary<ushort, ushort> GasStationsFuel;

        public static void Init()
        {
            GasStationsFuel ??= [];
        }

        public static void Deinit() => GasStationsFuel = [];

        public static ushort GetGasStationFuel(ushort buildingId) => !GasStationsFuel.TryGetValue(buildingId, out var fuelCapacity) ? default : fuelCapacity;

        public static void CreateGasStationFuel(ushort buildingId)
        {
            if (!GasStationsFuel.TryGetValue(buildingId, out _))
            {
                GasStationsFuel.Add(buildingId, 0);
            }
        }

        public static void SetGasStationFuel(ushort buildingId, ushort fuelCapacity) => GasStationsFuel[buildingId] = fuelCapacity;


        public static void RemoveGasStationFuel(ushort buildingId) => GasStationsFuel.Remove(buildingId);
    }
}
