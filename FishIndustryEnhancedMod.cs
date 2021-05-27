using ICities;
using System;
using CitiesHarmony.API;

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
            try
            {
                var loadedBuildingInfoCount = PrefabCollection<BuildingInfo>.LoadedCount();
                for (uint i = 0; i < loadedBuildingInfoCount; i++)
                {
                    var bi = PrefabCollection<BuildingInfo>.GetLoaded(i);
                    if (bi is null) continue;
                    if (bi.name.Equals("(Fish) Algae Bioreactor.Algae Bioreactor_Data") || bi.name.Equals("(Fish) Farm Tanks - Algae.Aquaculture Farm - Algae Tanks_Data") || bi.name.Equals("(Fish) Farm Dock - Algae.Aquaculture Dock - Algae_Data") || bi.name.Equals("(Fish) Farm Dock - Seaweed.Aquaculture Dock - Seaweed_Data") || bi.name.Equals("(Factory) Bioplastics Plant.Bioplastics Plant_Data") || bi.name.Equals("(Fish) Factory - Fishmeal.Fishmeal Factory_Data"))
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
