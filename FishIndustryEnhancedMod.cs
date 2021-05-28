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
            LogHelper.Information("Loaded Mod");
            var Algae_Tanks = PrefabCollection<BuildingInfo>.FindLoaded("(Fish) Farm Tanks - Algae.Aquaculture Farm - Algae Tanks_Data");
			Algae_Tanks.m_placementMode = BuildingInfo.PlacementMode.Roadside;
            var Algae_Bioreactor = PrefabCollection<BuildingInfo>.FindLoaded("(Fish) Algae Bioreactor.Algae Bioreactor_Data");
			Algae_Bioreactor.m_placementMode = BuildingInfo.PlacementMode.Roadside;
            var Bioplastics_Plant = PrefabCollection<BuildingInfo>.FindLoaded("(Factory) Bioplastics Plant.Bioplastics Plant_Data");
			Bioplastics_Plant.m_placementMode = BuildingInfo.PlacementMode.Roadside;
			var Fishmeal_Factory = PrefabCollection<BuildingInfo>.FindLoaded("(Fish) Factory - Fishmeal.Fishmeal Factory_Data");
			Fishmeal_Factory.m_placementMode = BuildingInfo.PlacementMode.Roadside;
            LogHelper.Information("set all to road side");
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
