using CitiesHarmony.API;
using ICities;
using System;
using UnityEngine;
using ColossalFramework.UI;

namespace IndustriesSunsetHarborMerged {
    public class Mod : LoadingExtensionBase, IUserMod {

        public static bool inGame = false;
        private GameObject _ishmGameObject;
        private GameObject _worldInfoPanel;
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
                var loadedBuildingInfoCount = PrefabCollection<BuildingInfo>.LoadedCount();
                for (uint i = 0; i < loadedBuildingInfoCount; i++) {
                    var bi = PrefabCollection<BuildingInfo>.GetLoaded(i);
                    if (bi is null) continue;
                    if (bi.name.Equals("Fish Market 01")) {
                        LogHelper.Information(bi.name);
                        AIHelper.ApplyNewAIToBuilding(bi);
                    }
                    else if (bi.name.Contains("FishExtractor_Data")) {
                        LogHelper.Information(bi.name);
                        AIHelper.ApplyNewAIToBuilding(bi);
                    }
                }

                UIView objectOfType = UnityEngine.Object.FindObjectOfType<UIView>();
                if (objectOfType != null)
                {
                    _ishmGameObject = new GameObject("IsmhGameObject");
                    _ishmGameObject.transform.parent = objectOfType.transform;
                    _worldInfoPanel = new GameObject("CityServiceWorldInfoPanel");
                    _worldInfoPanel.transform.parent = objectOfType.transform;
                    _worldInfoPanel.AddComponent<CityServiceWorldInfoPanel>();
                    BuildingExtensionManager.Init();
                    _ishmGameObject.AddComponent<PanelExtenderFishExtractor>();
                }
                else
                {
                    LogHelper.Error("UIView not found, aborting!");
                }
                        
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
          BuildingExtensionManager.Deinit();
        }

    }

}
