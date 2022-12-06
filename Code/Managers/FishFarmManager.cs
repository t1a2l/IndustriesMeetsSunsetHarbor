using ColossalFramework;
using UnityEngine;

namespace IndustriesSunsetHarborMerged { 
    public static class FishFarmManager
    {
        public static void GetStats(ref Building building, out BuildingInfo primatyInfo)
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
            if (fishFarmID == 0)
            {
                return false;
            }
            var building = BuildingManager.instance.m_buildings.m_buffer[fishFarmID];
            if (building.Info?.m_class == null || (building.m_flags & Building.Flags.Created) == Building.Flags.None)
            {
                return false;
            }   
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

        public static bool ValidateFishFarmAndFindNewIfNeeded(ushort fishExtractorID, ref ushort fishFarmID, BuildingInfo buildingInfo)
        {
            if (buildingInfo == null)
            {
                return false;
            }
            if (fishFarmID != 0 && IsValidFishFarm(fishFarmID))
            {
                return true;
            }
            fishFarmID = AutoAssignFishExtractorToFishFarm(fishExtractorID, fishFarmID, out _);
            return fishFarmID != 0;
        }
        
        public static ushort AutoAssignFishExtractorToFishFarm(ushort fishExtractorID, ushort fishFarmID, out Vector3 fishExtractoPosition)
        {
            fishExtractoPosition = BuildingManager.instance.m_buildings.m_buffer[fishExtractorID].m_position;
            ushort closestFishFarm = GetClosestFishFarm(fishExtractoPosition);
            if (closestFishFarm != 0)
            {
                CachedFishExtractorData.SetFishFarm(fishExtractorID, closestFishFarm);
                LogHelper.Information($"auto assigned fish farm {closestFishFarm} to fish extractor {fishExtractorID}");
            }
            return closestFishFarm;
        }

        public static ushort GetClosestFishFarm(Vector3 fishExtractoPosition)
        {
            ushort result = 0;
            var previousDistance = float.MaxValue;
            var instance = Singleton<BuildingManager>.instance;
            var fishFarmIds = BuildingExtensionManager.GetFishFarmsIds();
            foreach (var fishFarmId in fishFarmIds)
            {
                var distance = Vector3.Distance(fishExtractoPosition, instance.m_buildings.m_buffer[fishFarmId].m_position);
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
