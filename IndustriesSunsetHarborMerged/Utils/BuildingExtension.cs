using System.Collections.Generic;
using System.Linq;
using ICities;

namespace IndustriesSunsetHarborMerged.Utils.BuildingExtension {

    public class BuildingExtension : BuildingExtensionBase
    {
        public static event FishFarmAdded OnFishFarmAdded;
        public static event FishFarmRemoved OnFishFarmRemoved;

        private static Dictionary<ItemClassFishFarm.ItemClassFishFarm, HashSet<ushort>> _fishFarmMap;

        public static void Init()
        {
            _fishFarmMap = new Dictionary<ItemClassFishFarm.ItemClassFishFarm, HashSet<ushort>>();
            for (ushort index = 0; index < BuildingManager.instance.m_buildings.m_buffer.Length; ++index)
            {
                ObserveBuilding(index);
            }
        }

        public static void Deinit()
        {
            _fishFarmMap = new Dictionary<ItemClassFishFarm.ItemClassFishFarm, HashSet<ushort>>();
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
            foreach (var fishFarms in _fishFarmMap)
            {
                if (!fishFarms.Value.Remove(id))
                {
                    continue;
                }
                FishFarmUtils.FishFarmUtils.GetStats(ref BuildingManager.instance.m_buildings.m_buffer[id], out BuildingInfo primaryInfo);
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
            FishFarmUtils.FishFarmUtils.GetStats(ref BuildingManager.instance.m_buildings.m_buffer[buildingId], out BuildingInfo primaryInfo);
            ObserveForInfo(buildingId, primaryInfo);
        }

        private static void ObserveForInfo(ushort buildingId, BuildingInfo buildingInfo)
        {
            if (buildingInfo == null || !FishFarmUtils.FishFarmUtils.IsValidFishFarm(buildingId, buildingInfo))
            {
                return;
            }
            var fishItemClass = new ItemClassFishFarm.ItemClassFishFarm(buildingInfo.GetService());
            if (!_fishFarmMap.TryGetValue(fishItemClass, out HashSet<ushort> fishFarms))
            {
                fishFarms = new HashSet<ushort>();
                _fishFarmMap.Add(fishItemClass, fishFarms);
            }
            if (fishFarms.Contains(buildingId))
            {
                return;
            }
            fishFarms.Add(buildingId);
            OnFishFarmAdded?.Invoke(fishItemClass.Service);
        }


        public static ushort[] GetFishFarms(BuildingInfo buildingInfo)
        {
            if (buildingInfo == null)
            {
                return new ushort[0];
            }

            return _fishFarmMap.TryGetValue(
                new ItemClassFishFarm.ItemClassFishFarm(buildingInfo.GetService()),
                out HashSet<ushort> source)
                ? source.Where(d => FishFarmUtils.FishFarmUtils.IsValidFishFarm(d, buildingInfo))
                    .ToArray()
                : new ushort [0];
        }



        public delegate void FishFarmAdded(ItemClass.Service service);

        public delegate void FishFarmRemoved(ItemClass.Service service);
    }
}
