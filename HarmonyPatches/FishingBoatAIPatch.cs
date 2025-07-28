using ColossalFramework;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.AI;
using MoreTransferReasons;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(FishingBoatAI))]
    public static class FishingBoatAIPatch
    {
        [HarmonyPatch(typeof(FishingBoatAI), "ArriveAtTarget")]
        [HarmonyPrefix]
        public static bool ArriveAtTarget(FishingBoatAI __instance, ushort vehicleID, ref Vehicle data, ref bool __result)
        {
            if (data.m_targetBuilding == 0)
            {
                return true;
            }
            int amountDelta = data.m_transferSize;
            BuildingManager instance = Singleton<BuildingManager>.instance;
            Notification.ProblemStruct problems = instance.m_buildings.m_buffer[data.m_targetBuilding].m_problems;
            int capacity = GetCapacity(__instance, vehicleID, ref data);
            Notification.ProblemStruct problemStruct = amountDelta >= capacity * 6 / 10 ? Notification.RemoveProblems(problems, Notification.Problem1.FishingRouteInefficient) : Notification.AddProblems(problems, Notification.Problem1.FishingRouteInefficient);
            if (problems != problemStruct)
            {
                instance.m_buildings.m_buffer[data.m_targetBuilding].m_problems = problemStruct;
                instance.UpdateNotifications(data.m_targetBuilding, problems, problemStruct);
            }
            int num = 0;
            if (capacity > 0 && amountDelta > 0)
            {
                num = capacity * 100 / amountDelta;
            }
            if (num > 0)
            {
                num = 10000 / num;
            }
            instance.m_buildings.m_buffer[data.m_targetBuilding].m_education1 = (byte)num;
            BuildingInfo info = instance.m_buildings.m_buffer[data.m_targetBuilding].Info;
            if(info.GetAI() is ExtendedFishingHarborAI extendedFishingHarborAI)
            {
                ((IExtendedBuildingAI)extendedFishingHarborAI).ExtendedModifyMaterialBuffer(data.m_targetBuilding, ref instance.m_buildings.m_buffer[data.m_targetBuilding], extendedFishingHarborAI.m_outputResource, ref amountDelta);
            }
            else
            {
                info.m_buildingAI.ModifyMaterialBuffer(data.m_targetBuilding, ref instance.m_buildings.m_buffer[data.m_targetBuilding], TransferManager.TransferReason.Fish, ref amountDelta);
            }
            data.m_transferSize = 0;
            __result = true;
            return false;
        }

        private static int GetCapacity(FishingBoatAI __instance, ushort vehicleID, ref Vehicle data)
        {
            int num = __instance.m_capacity;
            if (data.m_sourceBuilding != 0)
            {
                DistrictManager instance = Singleton<DistrictManager>.instance;
                byte district = instance.GetDistrict(Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].m_position);
                DistrictPolicies.CityPlanning cityPlanningPolicies = instance.m_districts.m_buffer[district].m_cityPlanningPolicies;
                int num2 = 0;
                if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.SustainableFishing) != 0)
                {
                    num2 -= 10;
                }
                if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.DolphinSafeTuna) != 0)
                {
                    num2 -= 25;
                }
                num = (num * (100 + num2) + 99) / 100;
            }
            return num;
        }

    }
}
