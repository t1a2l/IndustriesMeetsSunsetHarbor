using ColossalFramework;
using UnityEngine;
using IndustriesMeetsSunsetHarbor.Utils;

namespace IndustriesMeetsSunsetHarbor.Managers
{ 
    public static class AquacultureFarmManager
    {
        public static void GetStats(ref Building building, out BuildingInfo primatyInfo)
        {
            var aquacultureFarmAI = building.Info?.m_buildingAI as FishFarmAI;
            if (aquacultureFarmAI == null)
            {
                primatyInfo = null;
            }
            else
            {
                primatyInfo = aquacultureFarmAI.m_info;
            }
        }

        public static bool IsValidAquacultureFarm(ushort aquacultureFarmID)
        {
            if (aquacultureFarmID == 0)
            {
                return false;
            }
            var building = BuildingManager.instance.m_buildings.m_buffer[aquacultureFarmID];
            if (building.Info?.m_class == null || (building.m_flags & Building.Flags.Created) == Building.Flags.None)
            {
                return false;
            }
            if(!building.Info.name.Contains("Aquaculture Dock"))
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

        public static bool ValidateAquaCultureFarmAndFindNewIfNeeded(ushort aquacultureExtractorID, ref ushort aquacultureFarmID, BuildingInfo buildingInfo)
        {
            if (buildingInfo == null)
            {
                return false;
            }
            if (aquacultureExtractorID != 0 && IsValidAquacultureFarm(aquacultureExtractorID))
            {
                return true;
            }
            aquacultureExtractorID = AutoAssignAquacultureExtractorToAquacultureFarm(aquacultureExtractorID, aquacultureFarmID, out _);
            return aquacultureExtractorID != 0;
        }
        
        public static ushort AutoAssignAquacultureExtractorToAquacultureFarm(ushort aquacultureExtractorID, ushort aquacultureFarmID, out Vector3 aquacultureExtractoPosition)
        {
            aquacultureExtractoPosition = BuildingManager.instance.m_buildings.m_buffer[aquacultureExtractorID].m_position;
            ushort closestAquacultureFarm = GetClosestAquacultureFarm(aquacultureExtractoPosition);
            if (closestAquacultureFarm != 0)
            {
                CachedAquacultureExtractorData.SetAquacultureFarm(aquacultureExtractorID, closestAquacultureFarm);
                LogHelper.Information($"auto assigned aquaculture farm {closestAquacultureFarm} to aquaculture extractor {aquacultureExtractorID}");
            }
            return closestAquacultureFarm;
        }

        public static ushort GetClosestAquacultureFarm(Vector3 aquacultureExtractoPosition)
        {
            ushort result = 0;
            var previousDistance = float.MaxValue;
            var instance = Singleton<BuildingManager>.instance;
            var aquacultureFarmIds = BuildingExtensionManager.GetAquacultureFarmsIds();
            foreach (var aquacultureFarmId in aquacultureFarmIds)
            {
                var distance = Vector3.Distance(aquacultureExtractoPosition, instance.m_buildings.m_buffer[aquacultureFarmId].m_position);
                if (!(distance < (double) previousDistance))
                {
                    continue;
                }
                result = aquacultureFarmId;
                previousDistance = distance;
            }
            return result;
        }
    }
}
