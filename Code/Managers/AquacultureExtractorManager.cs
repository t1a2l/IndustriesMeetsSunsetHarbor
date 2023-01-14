using IndustriesMeetsSunsetHarbor.AI;
using System.Collections.Generic;

namespace IndustriesMeetsSunsetHarbor.Managers
{
    public static class AquacultureExtractorManager
    {

        public static List<ushort> AquacultureExtractorsWithNoFarm;

        public static void Init()
        {
            if(AquacultureExtractorsWithNoFarm == null)
            {
                AquacultureExtractorsWithNoFarm = new();
            }
        }

        public static void Deinit()
        {
            AquacultureExtractorsWithNoFarm = new();
        }

        public static void ObserveBuilding(ushort buildingId)
        {
            ObserveForInfo(buildingId);
        }

        public static void ObserveForInfo(ushort buildingId)
        {
            if (!IsValidAquacultureExtractor(buildingId))
            {
                return;
            }
            if (AquacultureFarmManager.GetAquacultureFarm(buildingId) == 0)
            {
                var aquacultureFarmID = AquacultureFarmManager.GetClosestAquacultureFarm(buildingId);
                if(AquacultureFarmManager.AquacultureFarms.ContainsKey(aquacultureFarmID))
                {
                    AquacultureFarmManager.AquacultureFarms[aquacultureFarmID].Add(buildingId);
                }
                else
                {
                    AquacultureExtractorsWithNoFarm.Add(buildingId);
                }
            }
        }

        public static bool IsValidAquacultureExtractor(ushort aquacultureExtractorID)
        {
            if (aquacultureExtractorID == 0)
            {
                return false;
            }
            var building = BuildingManager.instance.m_buildings.m_buffer[aquacultureExtractorID];
            if (building.Info?.m_class == null || (building.m_flags & Building.Flags.Created) == Building.Flags.None)
            {
                return false;
            }
            var aquacultureExtractorAI = building.Info.m_buildingAI as AquacultureExtractorAI;
            if (aquacultureExtractorAI == null)
            {
                return false;
            }
            return true;
        }

    }
}
