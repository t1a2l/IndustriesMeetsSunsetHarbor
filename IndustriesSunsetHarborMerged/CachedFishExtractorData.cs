using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework;
using IndustriesSunsetHarborMerged.Utils.FishFarmUtils;

namespace IndustriesSunsetHarborMerged.CachedFishExtractorData
{
    public static class CachedFishExtractorData
    {
        private static readonly string _dataID = "IndustriesSunsetHarborMerged";
        private static readonly string _dataVersion = "v004";

        public static bool _init = false;
        public static FishExtractorData.FishExtractorData[] _fishExtractorData;
        
        public static void Init()
        {
            if (!TryLoadData(out _fishExtractorData))
            {
                LogHelper.Information("Loading default transport line data.");
                NetManager instance1 = Singleton<NetManager>.instance;
                TransportManager instance2 = Singleton<TransportManager>.instance;
                int length = instance2.m_lines.m_buffer.Length;
                for (ushort index = 0; index < length; ++index)
                {
                    _fishExtractorData[index].FishFarm = FishFarmUtils.GetClosestFishFarm((ushort) index,
                        BuildingManager.instance.m_buildings.m_buffer[index].m_position);

                }
            }
            SerializableDataExtension.instance.EventSaveData += new SerializableDataExtension.SaveDataEventHandler(OnSaveData);

            _init = true;
        }

        public static void Deinit()
        {
            _fishExtractorData = null;
            SerializableDataExtension.instance.EventSaveData -= new SerializableDataExtension.SaveDataEventHandler(OnSaveData);
            _init = false;
        }

        public static bool TryLoadData(out FishExtractorData.FishExtractorData[] data)
        {
            data = new FishExtractorData.FishExtractorData[256];
            byte[] data1 = SerializableDataExtension.instance.SerializableData.LoadData(_dataID);
            if (data1 == null)
                return false;
            int index1 = 0;
            ushort fishExtractorID = 0;
            string empty = string.Empty;
            try
            {
                LogHelper.Information("Try to load fish extractor data.");
                string str = SerializableDataExtension.ReadString(data1, ref index1);
                if (string.IsNullOrEmpty(str) || str.Length != 4)
                {
                    LogHelper.Warning("Unknown data found.");
                    return false;
                }
                LogHelper.Information("Found fish extractor data version: " + str);
                NetManager instance1 = Singleton<NetManager>.instance;
                TransportManager instance2 = Singleton<TransportManager>.instance;
                while (index1 < data1.Length)
                {
                    index1 += 4;
                    index1 += 4;
                    bool boolean = BitConverter.ToBoolean(data1, index1);
                    ++index1;
                    ushort uint16 = BitConverter.ToUInt16(data1, index1);
                    data[(int) fishExtractorID].FishFarm = (int) uint16 != 0
                        ? uint16
                        : FishFarmUtils.GetClosestFishFarm(fishExtractorID, BuildingManager.instance.m_buildings.m_buffer[fishExtractorID].m_position);
                    index1 += 2;
                    if (str == "v003")
                        ++index1;
                    ++fishExtractorID;
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error("Could not load fish extractor data. " + ex.Message);
                data = new FishExtractorData.FishExtractorData[256];
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
        
               
        public static ushort GetFishFarm(ushort fishExtractorID)
        {
            return CachedFishExtractorData._fishExtractorData[fishExtractorID].FishFarm;
        }

        public static void SetFishFarm(ushort fishExtractorID, ushort fishFarmID)
        {
            CachedFishExtractorData._fishExtractorData[fishExtractorID].FishFarm = fishFarmID;
        }

        public static HashSet<string> GetPrefabs(ushort fishExtractorID)
        {
            return CachedFishExtractorData._fishExtractorData[fishExtractorID].Prefabs;
        }

        public static void SetPrefabs(ushort fishExtractorID, HashSet<string> prefabs)
        {
            CachedFishExtractorData._fishExtractorData[fishExtractorID].Prefabs = prefabs;
        }

        public static string GetRandomPrefab(ushort fishExtractorID)
        {
            if (CachedFishExtractorData._fishExtractorData[fishExtractorID].Prefabs != null)
            {
                string[] array = CachedFishExtractorData._fishExtractorData[fishExtractorID].Prefabs.ToArray<string>();
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