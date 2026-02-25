using HarmonyLib;
using IndustriesMeetsSunsetHarbor.Managers;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(DistrictManager))]
    public static class DistrictManagerPatch
    {
        [HarmonyPatch(typeof(DistrictManager), "CreatePark")]
        [HarmonyPostfix]
        public static void CreatePark(ref byte park, DistrictPark.ParkType type, DistrictPark.ParkLevel level, ref bool __result)
        {
            if(__result && type == DistrictPark.ParkType.Farming)
            {
                var data = FarmingParkDataManager.GetFarmingPark(park);
                data.m_fruitsData = default;
                data.m_vegetablesData = default;
                data.m_cottonData = default;
                data.m_cowsData = default;
                data.m_highlandCowsData = default;
                data.m_sheepData = default;
                data.m_pigsData = default;
                data.m_milkData = default;
                data.m_woolData = default;
                data.m_porkData = default;
                FarmingParkDataManager.SetCustomBuffer(park, data);
            }
        }
    }
}
