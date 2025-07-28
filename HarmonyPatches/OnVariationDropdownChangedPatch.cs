using ColossalFramework;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.AI;
using IndustriesMeetsSunsetHarbor.Managers;
using System.Reflection;

namespace IndustriesMeetsSunsetHarbor.Code.HarmonyPatches
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
            if (buildingAI is NewExtractingFacilityAI)
            {
                building.m_customBuffer1 = 0;
            }
            if (buildingAI is NewProcessingFacilityAI)
            {
                var custom_buffers = CustomBuffersManager.GetCustomBuffer(buildingId);
                custom_buffers.m_customBuffer10 = 0;
                CustomBuffersManager.SetCustomBuffer(buildingId, custom_buffers);
            }
        }
    }
}
