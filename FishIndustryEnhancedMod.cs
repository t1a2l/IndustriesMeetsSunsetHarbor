using ICities;
using System.Reflection;
using CitiesHarmony.API;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace FishIndustryEnhanced
{
    public class FishIndustryEnhanced : IUserMod
    {

        string IUserMod.Name => "Fish Industry Enhanced Mod";

        string IUserMod.Description => "Enhance the fishing Industry";
        
        public void OnEnabled() {
             HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
        }

        public void OnDisabled() {
            if (HarmonyHelper.IsHarmonyInstalled) Patcher.UnpatchAll();
        }
    }

    public class LoadingExtension : LoadingExtensionBase
    {

        public override void OnCreated(ILoading loading)
        {
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
            //base.OnLevelLoaded(mode);
            if (mode != LoadMode.NewGame && mode != LoadMode.LoadGame)
            {
                return;
            }
            LogHelper.Information("Loaded Mod");
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
}
