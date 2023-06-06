using HarmonyLib;
using IndustriesMeetsSunsetHarbor.AI;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(PlayerBuildingAI))]
    public static class PlayerBuildingAIPatch
    {
        [HarmonyPatch(typeof(PlayerBuildingAI), "ProduceGoods")]
        [HarmonyPrefix]
        public static bool ProduceGoods(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            if(buildingData.Info.GetAI() is RestaurantAI && (buildingData.m_flags & Building.Flags.Active) != 0)
            {
                return false;
            }
            return true;
        }
    }
}
