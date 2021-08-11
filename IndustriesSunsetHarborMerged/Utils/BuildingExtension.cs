using System.Collections.Generic;
using System.Linq;
using ICities;
using System.Reflection;
using UnityEngine;
using IndustriesSunsetHarborMerged.Utils.FishFarmUtils;

namespace IndustriesSunsetHarborMerged.Utils.BuildingExtension {

    public class BuildingExtension : BuildingExtensionBase
    {
        public static event FishFarmAdded OnFishFarmAdded;
        public static event FishFarmRemoved OnFishFarmRemoved;

        public static ExtractorData[] _extractorData;
        public struct ExtractorData
        {
            public ushort FishFarm { get; set; }

            public Vector3 Position { get; set; }
        }

        public struct FishItemClass
        {

            public FishItemClass(ItemClass.Service service)
            {
                Service = service;
            }


            public ItemClass.Service Service { get; }

            public bool IsValid()
            {
                return Service != ItemClass.Service.None;
            }
        }

        private static Dictionary<FishItemClass, HashSet<ushort>> _fishFarmMap;

        public static void Init()
        {
            _fishFarmMap = new Dictionary<FishItemClass, HashSet<ushort>>();
            for (ushort index = 0; index < BuildingManager.instance.m_buildings.m_buffer.Length; ++index)
            {
                ObserveBuilding(index);
            }
        }

        public static void Deinit()
        {
            _fishFarmMap = new Dictionary<FishItemClass, HashSet<ushort>>();
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

        private void OnReleasedForInfo(ushort id, BuildingInfo fishFarmInfo)
        {
            if (fishFarmInfo == null)
            {
                return;
            }
            OnFishFarmRemoved?.Invoke(fishFarmInfo.GetService());
        }

        private static void ObserveBuilding(ushort buildingId)
        {
            FishFarmUtils.FishFarmUtils.GetStats(ref BuildingManager.instance.m_buildings.m_buffer[buildingId], out BuildingInfo primaryInfo);
            ObserveForInfo(buildingId, primaryInfo);
        }

        private static void ObserveForInfo(ushort buildingId, BuildingInfo fishFarmInfo)
        {
            if (fishFarmInfo == null || !FishFarmUtils.FishFarmUtils.IsValidFishFarm(buildingId))
            {
                return;
            }
            var fishItemClass = new FishItemClass(fishFarmInfo.GetService());
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

        public static ushort[] GetFishFarms(BuildingInfo extractorInfo, BuildingInfo fishFarmInfo)
        {
            if (fishFarmInfo == null)
            {
                return new ushort[0];
            }
            var FarmNames = fishFarmInfo.name.Split('_');
            var FarmType = FarmNames[0];
            var ExtractorNames = extractorInfo.name.Split('_');
            var ExtractorType = ExtractorNames[0];
            return _fishFarmMap.TryGetValue(new FishItemClass(fishFarmInfo.GetService()),out HashSet<ushort> source)
                ? source.Where(d => FishFarmUtils.FishFarmUtils.IsValidFishFarm(d)).Where(d => FarmType == ExtractorType).ToArray()
                : new ushort [0];
        }

        public static ushort GetFishFarm(ushort extractorId)
        {
            return _extractorData[(int) extractorId].FishFarm;
        }

        public static void SetFishFarm(ushort extractorId, ushort fishFarmId)
        {
            var extractorPosition = BuildingManager.instance.m_buildings.m_buffer[extractorId].m_position;
            _extractorData[(int) extractorId].FishFarm = fishFarmId;
            _extractorData[(int) extractorId].Position = extractorPosition;
        }

        public delegate void FishFarmAdded(ItemClass.Service service);

        public delegate void FishFarmRemoved(ItemClass.Service service);

        public static Q GetPrivate<Q>(object o, string fieldName)
        {
            FieldInfo[] fields = o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo fieldInfo1 = null;
            foreach (FieldInfo fieldInfo2 in fields)
            {
                if (fieldInfo2.Name == fieldName)
                {
                    fieldInfo1 = fieldInfo2;
                    break;
                }
            }

            return (Q) fieldInfo1.GetValue(o);
        }
    }
}
