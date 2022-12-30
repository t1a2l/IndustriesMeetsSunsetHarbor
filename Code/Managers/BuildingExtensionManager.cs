using System.Collections.Generic;
using ColossalFramework;
using ICities;

namespace IndustriesMeetsSunsetHarbor.Managers {

    public class BuildingExtensionManager : BuildingExtensionBase
    {
        public static event AquacultureFarmAdded OnAquacultureFarmAdded;
        public static event AquacultureFarmRemoved OnAquacultureFarmRemoved;

        private static Dictionary<ushort, HashSet<ushort>> AquacultureFarms;

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

        public override void OnBuildingCreated(ushort id)
        {
            base.OnBuildingCreated(id);
            if (!Mod.inGame)
            {
                return;
            }
            ObserveBuilding(id);
        }

        public override void OnBuildingReleased(ushort id)
        {
            base.OnBuildingReleased(id);
            if (!Mod.inGame)
            {
                return;
            }
            foreach (var fishFarm in AquacultureFarms)
            {
                if (!fishFarm.Value.Remove(id))
                {
                    continue;
                }
                AquacultureFarmManager.GetStats(ref BuildingManager.instance.m_buildings.m_buffer[id], out BuildingInfo primaryInfo);
                OnReleasedForInfo(id, primaryInfo);
            }
        }

        private void OnReleasedForInfo(ushort _, BuildingInfo buildingInfo)
        {
            if (buildingInfo == null)
            {
                return;
            }
            OnAquacultureFarmRemoved?.Invoke(buildingInfo.GetService());
        }

        private static void ObserveBuilding(ushort buildingId)
        {
            AquacultureFarmManager.GetStats(ref BuildingManager.instance.m_buildings.m_buffer[buildingId], out BuildingInfo primaryInfo);
            ObserveForInfo(buildingId, primaryInfo);
        }

        private static void ObserveForInfo(ushort buildingId, BuildingInfo buildingInfo)
        {
            if (buildingInfo == null || !AquacultureFarmManager.IsValidAquacultureFarm(buildingId))
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
            aquacultureExtractors.Add(buildingId);
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
                if(buildingInfo.GetAI() is FishFarmAI) {
                    if(AquacultureFarmManager.IsValidAquacultureFarm(index)) {
                        AquacultureFarms.Add(index);
                    }
                }
            }

            return AquacultureFarms.ToArray();
        }

        public delegate void AquacultureFarmAdded(ItemClass.Service service);

        public delegate void AquacultureFarmRemoved(ItemClass.Service service);
    }
}
