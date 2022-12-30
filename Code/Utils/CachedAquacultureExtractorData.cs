using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework;
using IndustriesMeetsSunsetHarbor.AI;

namespace IndustriesMeetsSunsetHarbor
{
    public static class CachedAquacultureExtractorData
    {
        private static readonly string _dataID = "IndustriesMeetsSunsetHarbor";
        private static readonly string _dataVersion = "v001";

        public static bool _init = false;
        public static AquacultureExtractorData[] _aquacultureExtractorData;
        
        public static void Init()
        {
            if (!TryLoadData(out _aquacultureExtractorData))
            {
                LogHelper.Information("Loading default aquaculture extractor data.");
                BuildingManager instance2 = Singleton<BuildingManager>.instance;
                int length = instance2.m_buildings.m_buffer.Length;
                for (ushort index = 0; index < length; ++index)
                {
                    var buildingInfo = BuildingManager.instance.m_buildings.m_buffer[index].Info;
                    var buildingPosition = BuildingManager.instance.m_buildings.m_buffer[index].m_position;
                    if(buildingInfo.GetAI() is AquaExtractorAI)
                    {
                        _aquacultureExtractorData[index].AquacultureFarm = AquacultureFarmManager.GetClosestAquacultureFarm(buildingPosition);
                    }
                }
            }
            SerializableDataExtension.instance.EventSaveData += new SerializableDataExtension.SaveDataEventHandler(OnSaveData);

            _init = true;
        }

        public static void Deinit()
        {
            _aquacultureExtractorData = null;
            SerializableDataExtension.instance.EventSaveData -= new SerializableDataExtension.SaveDataEventHandler(OnSaveData);
            _init = false;
        }

        public static bool TryLoadData(out AquacultureExtractorData[] data)
        {
            data = new AquacultureExtractorData[256];
            byte[] data1 = SerializableDataExtension.instance.SerializableData.LoadData(_dataID);
            if (data1 == null)
                return false;
            int index1 = 0;
            ushort aquacultureExtractorID = 0;
            try
            {
                LogHelper.Information("Try to load aquaculture extractor data.");
                string str = SerializableDataExtension.ReadString(data1, ref index1);
                if (string.IsNullOrEmpty(str) || str.Length != 4)
                {
                    LogHelper.Warning("Unknown data found.");
                    return false;
                }
                LogHelper.Information("Found aquaculture extractor data version: " + str);
                while (index1 < data1.Length)
                {
                    index1 += 4;
                    index1 += 4;
                    bool boolean = BitConverter.ToBoolean(data1, index1);
                    ++index1;
                    ushort uint16 = BitConverter.ToUInt16(data1, index1);
                    data[(int) aquacultureExtractorID].AquacultureFarm = (int) uint16 != 0
                        ? uint16
                        : AquacultureFarmManager.GetClosestAquacultureFarm(BuildingManager.instance.m_buildings.m_buffer[aquacultureExtractorID].m_position);
                    index1 += 2;
                    if (str == "v003")
                        ++index1;
                    ++aquacultureExtractorID;
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error("Could not load aquaculture extractor data. " + ex.Message);
                data = new AquacultureExtractorData[256];
                return false;
            }
        }

        private static void OnSaveData()
        {
            FastList<byte> data = new FastList<byte>();
            try
            {
                SerializableDataExtension.WriteString(_dataVersion, data);
                for (ushort fishExtractorID = 0; fishExtractorID < 256; ++fishExtractorID)
                {
                    SerializableDataExtension.AddToData(BitConverter.GetBytes(GetFishFarm(fishExtractorID)), data);
                    int num = 0;
                    HashSet<string> prefabs = GetPrefabs(fishExtractorID);
                    if (prefabs != null)
                        num = prefabs.Count;
                    SerializableDataExtension.AddToData(BitConverter.GetBytes(num), data);
                    if (num > 0)
                    {
                        foreach (string s in prefabs)
                            SerializableDataExtension.WriteString(s, data);
                    }
                }
                SerializableDataExtension.instance.SerializableData.SaveData(_dataID, data.ToArray());
            }
            catch (Exception ex)
            {
                string msg = "Error while saving fish extractor data! " + ex.Message + " " + (object) ex.InnerException;
                LogHelper.Error(msg);
                CODebugBase<LogChannel>.Log(LogChannel.Modding, msg, ErrorLevel.Error);
            }
        }
            
        public static ushort GetAquacultureFarm(ushort aquacultureExtractorID)
        {
            return _aquacultureExtractorData[aquacultureExtractorID].AquacultureFarm;
        }

        public static void SetAquacultureFarm(ushort aquacultureExtractorID, ushort aquacultureFarmID)
        {
            _aquacultureExtractorData[aquacultureExtractorID].AquacultureFarm = aquacultureFarmID;
        }

        public static HashSet<string> GetPrefabs(ushort fishExtractorID)
        {
            return _aquacultureExtractorData[fishExtractorID].Prefabs;
        }

        public static void SetPrefabs(ushort aquacultureExtractorID, HashSet<string> prefabs)
        {
            _aquacultureExtractorData[aquacultureExtractorID].Prefabs = prefabs;
        }

        public static string GetRandomPrefab(ushort aquacultureExtractorID)
        {
            if (_aquacultureExtractorData[aquacultureExtractorID].Prefabs != null)
            {
                string[] array = _aquacultureExtractorData[aquacultureExtractorID].Prefabs.ToArray<string>();
                if (array.Length != 0)
                {
                    int index = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint) array.Length);
                    return array[index];
                }
            }
            return "";
        }

    }
}