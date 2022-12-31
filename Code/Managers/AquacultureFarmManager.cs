using ColossalFramework;
using System.Collections.Generic;
using UnityEngine;
using IndustriesMeetsSunsetHarbor.AI;

namespace IndustriesMeetsSunsetHarbor.Managers
{
    public static class AquacultureFarmManager
    {
        public static event AquacultureFarmAdded OnAquacultureFarmAdded;
        public static event AquacultureFarmRemoved OnAquacultureFarmRemoved;

        public static Dictionary<ushort, HashSet<ushort>> AquacultureFarms;

        public static void Init()
        {
            AquacultureFarms = new Dictionary<ushort, HashSet<ushort>>();
            for (ushort index = 0; index < BuildingManager.instance.m_buildings.m_buffer.Length; ++index)
            {
                ObserveBuilding(index);
            }
        }

        public static void Deinit()
        {
            AquacultureFarms = new Dictionary<ushort, HashSet<ushort>>();
        }

        public static void ObserveBuilding(ushort buildingId)
        {
            GetStats(ref BuildingManager.instance.m_buildings.m_buffer[buildingId], out BuildingInfo primaryInfo);
            ObserveForInfo(buildingId, primaryInfo);
        }

        public static void ObserveForInfo(ushort buildingId, BuildingInfo buildingInfo)
        {
            if (buildingInfo == null || !IsValidAquacultureFarm(buildingId))
            {
                return;
            }
            if (!AquacultureFarms.TryGetValue(buildingId, out HashSet<ushort> aquacultureExtractors))
            {
                aquacultureExtractors = new HashSet<ushort>();
                AquacultureFarms.Add(buildingId, aquacultureExtractors);
            }
            if (AquacultureFarms.ContainsKey(buildingId))
            {
                return;
            }
            OnAquacultureFarmAdded?.Invoke(buildingInfo.GetService());
        }


        public static ushort[] GetAquacultureFarmsIds()
        {
            List<ushort> AquacultureFarms = new();
            BuildingManager instance2 = Singleton<BuildingManager>.instance;
            int length = instance2.m_buildings.m_buffer.Length;
            for (ushort index = 0; index < length; ++index)
            {
                var buildingInfo = BuildingManager.instance.m_buildings.m_buffer[index].Info;
                if (buildingInfo.GetAI() is AquacultureFarmAI)
                {
                    if (IsValidAquacultureFarm(index))
                    {
                        AquacultureFarms.Add(index);
                    }
                }
            }

            return AquacultureFarms.ToArray();
        }

        public static void GetStats(ref Building building, out BuildingInfo primatyInfo)
        {
            var aquacultureFarmAI = building.Info?.m_buildingAI as AquacultureFarmAI;
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
            GetStats(ref building, out BuildingInfo primaryInfo);
            if (primaryInfo == null)
            {
                return false;
            }
            var aquacultureFarmAI = building.Info.m_buildingAI as AquacultureFarmAI;
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
            var aquacultureFarmIds = GetAquacultureFarmsIds();
            Building aquacultureExtractorBuilding = instance.m_buildings.m_buffer[aquacultureExtractorId];
            foreach (var aquacultureFarmId in aquacultureFarmIds)
            {
                Building aquacultureFarmBuilding = instance.m_buildings.m_buffer[aquacultureFarmId];
                if(checkIfSameAquacultureType(aquacultureFarmBuilding.Info, aquacultureExtractorBuilding.Info))
                {
                    var distance = Vector3.Distance(aquacultureExtractorBuilding.m_position, aquacultureFarmBuilding.m_position);
                    if (!(distance < (double)previousDistance))
                    {
                        continue;
                    }
                    result = aquacultureFarmId;
                    previousDistance = distance;
                }
            }
            return result;
        }

        public static bool checkIfSameAquacultureType(BuildingInfo aquacultureFarm, BuildingInfo aquacultureExtractor)
        {
            if(aquacultureFarm.name.Contains("Algae") && aquacultureExtractor.name.Contains("Algae"))
            {
                return true;
            }
            else if(aquacultureFarm.name.Contains("Fish") && aquacultureExtractor.name.Contains("Fish"))
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

        public static void OnReleasedForInfo(ushort _, BuildingInfo buildingInfo)
        {
            if (buildingInfo == null)
            {
                return;
            }
            OnAquacultureFarmRemoved?.Invoke(buildingInfo.GetService());
        }

        public delegate void AquacultureFarmAdded(ItemClass.Service service);

        public delegate void AquacultureFarmRemoved(ItemClass.Service service);
    }
}
