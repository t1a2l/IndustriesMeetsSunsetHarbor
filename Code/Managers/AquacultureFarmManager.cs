using ColossalFramework;
using System.Collections.Generic;
using UnityEngine;
using IndustriesMeetsSunsetHarbor.AI;

namespace IndustriesMeetsSunsetHarbor.Managers
{
    public static class AquacultureFarmManager
    {
        public static Dictionary<ushort, List<ushort>> AquacultureFarms;

        public static void Init()
        {
            if(AquacultureFarms == null)
            {
                AquacultureFarms = new Dictionary<ushort, List<ushort>>();
            }
            for (ushort index = 0; index < BuildingManager.instance.m_buildings.m_buffer.Length; ++index)
            {
                ObserveBuilding(index);
            }
        }

        public static void Deinit()
        {
            AquacultureFarms = new Dictionary<ushort, List<ushort>>();
        }

        public static void ObserveBuilding(ushort buildingId)
        {
            ObserveForInfo(buildingId);
        }

        public static void ObserveForInfo(ushort buildingId)
        {
            if (!IsValidAquacultureFarm(buildingId))
            {
                return;
            }
            if (!AquacultureFarms.TryGetValue(buildingId, out List<ushort> _))
            {
                var aquacultureFarmExtractors = new List<ushort>();
                var farmInfo = BuildingManager.instance.m_buildings.m_buffer[buildingId].Info;
                // if there are extractors with no farm attached and we add a new farm, we need to check and match the extractors to the new farm
                foreach(var extractorId in AquacultureExtractorManager.AquacultureExtractorsWithNoFarm)
                {
                    var extractorInfo = BuildingManager.instance.m_buildings.m_buffer[extractorId].Info;
                    if(CheckIfSameAquacultureType(farmInfo, extractorInfo))
                    {
                        aquacultureFarmExtractors.Add(extractorId);
                    }
                }
                AquacultureFarms.Add(buildingId, aquacultureFarmExtractors);
            }
            if (AquacultureFarms.ContainsKey(buildingId))
            {
                return;
            }
        }

        public static ushort[] GetAquacultureFarmsIds(ushort extractorId) // get all farms of the same type of the extractor
        {
            List<ushort> AquacultureFarmsIds = new();
            var extractorInfo = BuildingManager.instance.m_buildings.m_buffer[extractorId].Info;
            for (ushort buildingId = 0; buildingId < BuildingManager.instance.m_buildings.m_buffer.Length; ++buildingId)
            {
                if (IsValidAquacultureFarm(buildingId))
                {
                    var farmInfo = BuildingManager.instance.m_buildings.m_buffer[buildingId].Info;
                    if(CheckIfSameAquacultureType(farmInfo, extractorInfo))
                    {
                        AquacultureFarmsIds.Add(buildingId);
                    }
                }
                
            }
            return AquacultureFarmsIds.ToArray();
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
            var aquacultureFarmAI = building.Info?.m_buildingAI as AquacultureFarmAI;
            if (aquacultureFarmAI == null)
            {
                return false;
            }
            return true;
        }

        public static ushort GetClosestAquacultureFarm(ushort aquacultureExtractorId)
        {
            ushort result = 0;
            var previousDistance = float.MaxValue;
            var instance = Singleton<BuildingManager>.instance;
            var aquacultureFarmIds = GetAquacultureFarmsIds(aquacultureExtractorId);
            var aquacultureExtractor = BuildingManager.instance.m_buildings.m_buffer[aquacultureExtractorId];
            foreach (var aquacultureFarmId in aquacultureFarmIds)
            {
                Building aquacultureFarm = instance.m_buildings.m_buffer[aquacultureFarmId];
                var distance = Vector3.Distance(aquacultureExtractor.m_position, aquacultureFarm.m_position);
                if (!(distance < (double)previousDistance))
                {
                    continue;
                }
                result = aquacultureFarmId;
                previousDistance = distance;
                
            }
            return result;
        }

        public static bool CheckIfSameAquacultureType(BuildingInfo aquacultureFarm, BuildingInfo aquacultureExtractor)
        {
            if(aquacultureFarm.name.Contains("Algae") && aquacultureExtractor.name.Contains("Algae"))
            {
                return true;
            }
            else if(aquacultureFarm.name.Contains("Fish") && aquacultureExtractor.name.Contains("Mixed"))
            {
                return true;
            }
            else if(aquacultureFarm.name.Contains("Mussels") && aquacultureExtractor.name.Contains("Mussels"))
            {
                return true;
            }
            else if(aquacultureFarm.name.Contains("Seaweed") && aquacultureExtractor.name.Contains("Seaweed"))
            {
                return true;
            }
            return false;
        }

        public static ushort GetAquacultureFarm(ushort extractorId)
        {
            ushort farmId = 0;
            foreach(var aquacultureFarm in AquacultureFarms)
            {
                var extractorsList = aquacultureFarm.Value;
                farmId = extractorsList.Find(item => item == extractorId);
                if(farmId != 0)
                {
                   return aquacultureFarm.Key;
                }
            }
            return 0;
        }
    }
}
