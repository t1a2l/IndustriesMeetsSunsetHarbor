using System.Collections.Generic;
using ColossalFramework;
using ICities;

namespace IndustriesSunsetHarborMerged {

    public class BuildingExtensionManager : BuildingExtensionBase
    {
        public static event FishFarmAdded OnFishFarmAdded;
        public static event FishFarmRemoved OnFishFarmRemoved;

        private static Dictionary<ushort, HashSet<ushort>> FishFarms;

        public static void Init()
        {
            FishFarms = new Dictionary<ushort, HashSet<ushort>>();
            for (ushort index = 0; index < BuildingManager.instance.m_buildings.m_buffer.Length; ++index)
            {
                ObserveBuilding(index);
            }
        }

        public static void Deinit()
        {
            FishFarms = new Dictionary<ushort, HashSet<ushort>>();
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
            foreach (var fishFarm in FishFarms)
            {
                if (!fishFarm.Value.Remove(id))
                {
                    continue;
                }
                FishFarmManager.GetStats(ref BuildingManager.instance.m_buildings.m_buffer[id], out BuildingInfo primaryInfo);
                OnReleasedForInfo(id, primaryInfo);
            }
        }

        private void OnReleasedForInfo(ushort id, BuildingInfo buildingInfo)
        {
            if (buildingInfo == null)
            {
                return;
            }
            OnFishFarmRemoved?.Invoke(buildingInfo.GetService());
        }

        private static void ObserveBuilding(ushort buildingId)
        {
            FishFarmManager.GetStats(ref BuildingManager.instance.m_buildings.m_buffer[buildingId], out BuildingInfo primaryInfo);
            ObserveForInfo(buildingId, primaryInfo);
        }

        private static void ObserveForInfo(ushort buildingId, BuildingInfo buildingInfo)
        {
            if (buildingInfo == null || !FishFarmManager.IsValidFishFarm(buildingId))
            {
                return;
            }
            if (!FishFarms.TryGetValue(buildingId, out HashSet<ushort> fishExtractors))
            {
                fishExtractors = new HashSet<ushort>();
                FishFarms.Add(buildingId, fishExtractors);
            }
            if (FishFarms.ContainsKey(buildingId))
            {
                return;
            }
            fishExtractors.Add(buildingId);
            OnFishFarmAdded?.Invoke(buildingInfo.GetService());
        }


        public static ushort[] GetFishFarmsIds()
        {
            List<ushort> fishFarms = new List<ushort>();
            BuildingManager instance2 = Singleton<BuildingManager>.instance;
            int length = instance2.m_buildings.m_buffer.Length;
            for (ushort index = 0; index < length; ++index)
            {
                var buildingInfo = BuildingManager.instance.m_buildings.m_buffer[index].Info;
                if(buildingInfo.GetAI() is FishFarmAI) {
                    if(FishFarmManager.IsValidFishFarm(index)) {
                        fishFarms.Add(index);
                    }
                }
            }

            return fishFarms.ToArray();
        }

        public delegate void FishFarmAdded(ItemClass.Service service);

        public delegate void FishFarmRemoved(ItemClass.Service service);
    }
}
