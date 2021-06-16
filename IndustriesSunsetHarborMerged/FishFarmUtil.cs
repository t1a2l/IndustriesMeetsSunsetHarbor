using ColossalFramework;
using UnityEngine;

namespace IndustriesSunsetHarborMerged
{ 
    public static class FishFarmUtil
    {
        public static void GetStats(ref Building building,out BuildingInfo primatyInfo)
        {
            var fishFarmAi = building.Info?.m_buildingAI as FishFarmAI;
            if (fishFarmAi == null)
            {
                primatyInfo = null;
            }
            else
            {
                primatyInfo = fishFarmAi.m_info;
            }
        }


        public static bool IsValidFishFarm(ushort fishFarmID)
        {
            var fishFarmInfo = Singleton<BuildingManager>.instance.m_buildings.m_buffer[fishFarmID].Info;
            if (fishFarmInfo == null || fishFarmID == 0)
            {
                return false;
            }
            var building = BuildingManager.instance.m_buildings.m_buffer[fishFarmID];
            if (building.Info?.m_class == null || (building.m_flags & Building.Flags.Created) == Building.Flags.None)
                return false;
            GetStats(ref building, out BuildingInfo primaryInfo);
            if (primaryInfo == null)
            {
                return false;
            }
            var fishFarmAi = building.Info.m_buildingAI as FishFarmAI;
            if (fishFarmAi != null)
            {
                return true;
            }
            return false;
        }

        public static bool ValidateDepotAndFindNewIfNeeded(ushort extractorID, ref ushort fishFarmID, BuildingInfo buildingInfo)
        {
            if (buildingInfo == null)
            {
                return false;
            }
            if (extractorID != 0 && IsValidFishFarm(fishFarmID))
            {
                return true;
            }
            fishFarmID = AutoAssignExtractorFishFarm(extractorID, fishFarmID, out _);
            return fishFarmID != 0;
        }
        
        public static ushort AutoAssignExtractorFishFarm(ushort extractorID, ushort fishFarmID, out Vector3 position)
        {
            position = BuildingManager.instance.m_buildings.m_buffer[extractorID].m_position;
            ushort closestFishFarm = GetClosestFishFarm(extractorID, fishFarmID, position);
            if ((int) closestFishFarm != 0)
            {
                BuildingExtension.SetFishFarm(extractorID, closestFishFarm);
                LogHelper.Information($"auto assigned fish farm {closestFishFarm} to extractor {extractorID}");
            }
            return closestFishFarm;
        }

        public static ushort GetClosestFishFarm(ushort extractorID, ushort fishFarmID, Vector3 position)
        {
            ushort result = 0;
            var previousDistance = float.MaxValue;
            var instance = Singleton<BuildingManager>.instance;
            var extractorInfo = Singleton<BuildingManager>.instance.m_buildings.m_buffer[extractorID].Info;
            var fishFarmInfo = Singleton<BuildingManager>.instance.m_buildings.m_buffer[fishFarmID].Info;
            var fishFarmsIds = BuildingExtension.GetFishFarms(extractorInfo, fishFarmInfo);
            foreach (var fishFarmId in fishFarmsIds)
            {
                var distance = Vector3.Distance(position, instance.m_buildings.m_buffer[fishFarmId].m_position);
                if (!(distance < (double) previousDistance))
                {
                    continue;
                }
                result = fishFarmId;
                previousDistance = distance;
            }
            return result;
        }
    }
}
