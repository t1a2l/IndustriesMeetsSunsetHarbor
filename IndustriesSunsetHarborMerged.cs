using ICities;
using CitiesHarmony.API;
using IndustriesSunsetHarborMerged.IndustriesSunsetHarborMerged;
using System;
using System.Xml.Serialization;
using System.IO;

namespace IndustriesSunsetHarborMerged
{
    public class Mod : LoadingExtensionBase, IUserMod
    {
        private static Mod instance;

        string IUserMod.Name => "Industries Sunset Harbor Merged Mod";

        string IUserMod.Description => "Mix Industries and Sunset Harbor together";
        
        public void OnEnabled() {
             HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
        }

        public void OnDisabled() {
            if (HarmonyHelper.IsHarmonyInstalled) Patcher.UnpatchAll();
        }

        public static Mod getInstance() {
            return instance;
        }

        public override void OnCreated(ILoading loading)
        {
            instance = this;
            base.OnCreated(loading);
            if (loading.currentMode != AppMode.Game)
            {
                return;
            }
            if (!HarmonyHelper.IsHarmonyInstalled)
            {
                return;
            }
        }
        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode != LoadMode.NewGame && mode != LoadMode.LoadGame)
            {
                return;
            }
            try
            {
                var loadedBuildingInfoCount = PrefabCollection<BuildingInfo>.LoadedCount();
                for (uint i = 0; i < loadedBuildingInfoCount; i++)
                {
                    var bi = PrefabCollection<BuildingInfo>.GetLoaded(i);
                    if (bi is null) continue;
                    LogHelper.Information(bi.name);
                    if (bi.name.Equals("Fish Market 01"))
                    {
                        AIHelper.ApplyNewAIToBuilding(bi);
                    }
                }
                LogHelper.Information("Reloaded Mod");
            }
            catch (Exception e)
            {
                LogHelper.Information(e.ToString());
            }
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
        } 

        public override void OnReleased()
        {
            base.OnReleased();
            if (!HarmonyHelper.IsHarmonyInstalled)
            {
                return;
            }
        }

    }

    public class SerializableDataExtension : SerializableDataExtensionBase
    {
        private const string DATA_ID = "MarketBuffer";

        ResourceMarketAI.MarketBuffer marketBuffer = ResourceMarketAI.MarketBuffer.getInstance();

        public override void OnLoadData()
        { 
            try {
                byte[] data = serializableDataManager.LoadData(DATA_ID);
                ResourceMarketAI.MarketBuffer.Deserialize(data); //check for null
            } catch (Exception ex) {
                LogHelper.Error(ex.Message);
            }
        }

        public override void OnSaveData()
        {
            try {
                byte[] data = marketBuffer.Serialize();
                serializableDataManager.SaveData(DATA_ID, data);
            } catch (Exception ex) {
                LogHelper.Error(ex.Message);
            }
        }
    }

     internal static class XMLSerializerUtil 
     {
        static XmlSerializer Serilizer<T>() => new XmlSerializer(typeof(T));
        static void Serialize<T>(TextWriter writer, T value) => Serilizer<T>().Serialize(writer, value);
        static T Deserialize<T>(TextReader reader) => (T)Serilizer<T>().Deserialize(reader);

        public static string Serialize<T>(T value) {
            try {
                using (TextWriter writer = new StringWriter()) {
                    Serialize<T>(writer, value);
                    return writer.ToString();
                }
            } catch (Exception ex) {
                LogHelper.Error(ex.Message);
                return null;
            }
        }

        public static T Deserialize<T>(string data) {
            try {
                using (TextReader reader = new StringReader(data)) {
                    return Deserialize<T>(reader);
                }
            } catch (Exception ex) {
                LogHelper.Information("data=" + data);
                LogHelper.Error(ex.Message);
                return default;
            }
        }
     }
}
