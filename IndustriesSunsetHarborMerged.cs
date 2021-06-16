using CitiesHarmony.API;
using ICities;
using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace IndustriesSunsetHarborMerged {
    public class Mod : LoadingExtensionBase, IUserMod {

        public static bool inGame = false;

        private GameObject _ishmGameObject;
        string IUserMod.Name => "Industries Sunset Harbor Merged Mod";

        string IUserMod.Description => "Mix Industries and Sunset Harbor together";

        public void OnEnabled() {
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
        }

        public void OnDisabled() {
            if (HarmonyHelper.IsHarmonyInstalled) Patcher.UnpatchAll();
        }


        public override void OnLevelLoaded(LoadMode mode) {
            if (mode != LoadMode.NewGame && mode != LoadMode.LoadGame) {
                return;
            }
            try {
                inGame = true;
                _ishmGameObject = new GameObject("IshmGameObject");
                _ishmGameObject.AddComponent<FishExtractorAI>();
                var loadedBuildingInfoCount = PrefabCollection<BuildingInfo>.LoadedCount();
                for (uint i = 0; i < loadedBuildingInfoCount; i++) {
                    var bi = PrefabCollection<BuildingInfo>.GetLoaded(i);
                    if (bi is null) continue;
                    if (bi.name.Equals("Fish Market 01")) {
                        LogHelper.Information(bi.name);
                        AIHelper.ApplyNewAIToBuilding(bi);
                    }
                }
                LogHelper.Information("Reloaded Mod");
            }
            catch (Exception e) {
                LogHelper.Information(e.ToString());
                Deinit();
            }
        }

        public override void OnLevelUnloading() {
            base.OnLevelUnloading();
            if (!inGame)
                return;
            inGame = false;
            Deinit();
            LogHelper.Information("Unloading done!" + Environment.NewLine);
        }

        private void Deinit()
        {
          BuildingExtension.Deinit();
          if (_ishmGameObject != null)
            UnityEngine.Object.Destroy(_ishmGameObject);
        }

    }

    public class SerializableDataExtension : SerializableDataExtensionBase {
        private const string DATA_ID = "MarketData";

        public override void OnLoadData() {
            try {
                byte[] data = serializableDataManager.LoadData(DATA_ID);
                ResourceMarketManager.Deserialize(data); //check for null
            }
            catch (Exception ex) {
                LogHelper.Error(ex.ToString());
            }
        }

        public override void OnSaveData() {
            try {
                byte[] data = ResourceMarketManager.Instance.Serialize();
                serializableDataManager.SaveData(DATA_ID, data);
            }
            catch (Exception ex) {
                LogHelper.Error(ex.ToString());
            }
        }
    }

    internal static class XMLSerializerUtil {
        static XmlSerializer Serilizer<T>() => new XmlSerializer(typeof(T));
        static void Serialize<T>(TextWriter writer, T value) => Serilizer<T>().Serialize(writer, value);
        static T Deserialize<T>(TextReader reader) => (T)Serilizer<T>().Deserialize(reader);

        public static string Serialize<T>(T value) {
            try {
                using (TextWriter writer = new StringWriter()) {
                    Serialize<T>(writer, value);
                    return writer.ToString();
                }
            }
            catch (Exception ex) {
                LogHelper.Error(ex.ToString());
                return null;
            }
        }

        public static T Deserialize<T>(string data) {
            try {
                using (TextReader reader = new StringReader(data)) {
                    return Deserialize<T>(reader);
                }
            }
            catch (Exception ex) {
                LogHelper.Information("data=" + data);
                LogHelper.Error(ex.ToString());
                return default;
            }
        }
    }

}
