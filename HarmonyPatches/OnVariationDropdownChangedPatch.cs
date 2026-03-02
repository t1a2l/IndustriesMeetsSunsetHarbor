using ColossalFramework;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.AI;
using IndustriesMeetsSunsetHarbor.Managers;
using System;
using System.Reflection;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    public static class OnVariationDropdownChangedPatch
    {
        public static MethodBase TargetMethod()
        {
            var AnonStorey2 = AccessTools.TypeByName("<OnVariationDropdownChanged>c__AnonStorey2");

            return AccessTools.FirstMethod(AnonStorey2, method => method.Name.Contains("<>m__0"));
        }

        public static void Postfix(CityServiceWorldInfoPanel __instance, ref InstanceID ___m_InstanceID)
        {
            ushort buildingId = ___m_InstanceID.Building;
            BuildingManager instance = Singleton<BuildingManager>.instance;
            ref Building building = ref instance.m_buildings.m_buffer[buildingId];
            BuildingInfo info = building.Info;
            BuildingAI buildingAI = info.m_buildingAI;
            if (buildingAI is ExtractingFacilityAI)
            {
                building.m_customBuffer1 = 0;
                building.m_customBuffer2 = 0;
            }
            else if (buildingAI is ExtendedProcessingFacilityAI)
            {
                var custom_buffers = CustomBuffersManager.GetCustomBuffer(buildingId);
                Array.Clear(custom_buffers.m_volumes, 0, CustomBuffersManager.RESOURCE_COUNT);
                Array.Clear(custom_buffers.m_mealsSitDown, 0, 4);
                Array.Clear(custom_buffers.m_mealsDelivery, 0, 4);
                CustomBuffersManager.SetCustomBuffer(buildingId, custom_buffers);
            }
        }
    }
}
