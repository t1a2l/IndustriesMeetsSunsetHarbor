using ICities;
using CitiesHarmony.API;
using IndustriesSunsetHarborMerged.IndustriesSunsetHarborMerged;
using ColossalFramework;
using System;

namespace IndustriesSunsetHarborMerged
{
    public class Mod : IUserMod
    {

        string IUserMod.Name => "Industries Sunset Harbor Merged Mod";

        string IUserMod.Description => "Mix Industries and Sunset Harbor together";
        
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
}
