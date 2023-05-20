using HarmonyLib;
using System.Reflection;
using UnityEngine;
using IndustriesMeetsSunsetHarbor.Managers;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(ResidentialBuildingAI))]
    public static class ResidentialBuildingAIPatch
    {
        private delegate void GetHomeBehaviourCommonBuildingAIDelegate(CommonBuildingAI __instance, ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveCount, ref int totalCount, ref int homeCount, ref int aliveHomeCount, ref int emptyHomeCount);
        private static readonly GetHomeBehaviourCommonBuildingAIDelegate GetHomeBehaviour = AccessTools.MethodDelegate<GetHomeBehaviourCommonBuildingAIDelegate>(typeof(CommonBuildingAI).GetMethod("GetHomeBehaviour", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        [HarmonyPatch(typeof(ResidentialBuildingAI), "SimulationStepActive")]
        [HarmonyPrefix]
        public static void SimulationStepActive(ResidentialBuildingAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            Citizen.BehaviourData behaviourData = default(Citizen.BehaviourData);
	    int aliveCount = 0;
	    int totalCount = 0;
	    int homeCount = 0;
	    int aliveHomeCount = 0;
	    int emptyHomeCount = 0;
	    GetHomeBehaviour(__instance, buildingID, ref buildingData, ref behaviourData, ref aliveCount, ref totalCount, ref homeCount, ref aliveHomeCount, ref emptyHomeCount);
            HandleFoodDelivery(__instance, buildingID, ref buildingData, ref behaviourData, totalCount);
        }

        private static void HandleFoodDelivery(ResidentialBuildingAI __instance, ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, int citizenCount)
        {
            var custom_buffers = CustomBuffersManager.GetCustomBuffer(buildingID);
            Notification.ProblemStruct problemStruct = Notification.RemoveProblems(buildingData.m_problems, CustomBuffersManager.WaitingDelivery);
            var waiting_delivery = RestaurantDeliveriesManager.RestaurantDeliveries.FindIndex(item => item.buildingId == buildingID);
            if(waiting_delivery != -1)
            {
                custom_buffers.m_customBuffer1 = (byte)Mathf.Min(255, (int)(custom_buffers.m_customBuffer1 + 1));
                if (custom_buffers.m_customBuffer1 >= 128)
                {
                    problemStruct = Notification.AddProblems(problemStruct, CustomBuffersManager.WaitingDelivery);
                }
                else if (custom_buffers.m_customBuffer1 >= 64)
                {
                    problemStruct = Notification.AddProblems(problemStruct, CustomBuffersManager.WaitingDelivery);
                }
            }
            else
            {
                custom_buffers.m_customBuffer1 = 0;
            }
            buildingData.m_problems = problemStruct;
        }
    }
}
