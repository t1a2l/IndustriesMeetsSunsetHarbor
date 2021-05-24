using ICities;
using System;
using UnityEngine;

namespace FishIndustryEnhanced
{
    public class FishIndustryEnhanced : IUserMod
    {

        string IUserMod.Name => "Fish Industry Enhanced Mod";

        string IUserMod.Description => "Enhance the fishing Industry";
    }

    public class LoadingExtension : LoadingExtensionBase
    {
        private static GameObject _gameObject;
        public override void OnLevelLoaded(LoadMode mode)
        {
            try
            {
                _gameObject = new GameObject("UniqueFactoryFish");
                _gameObject.AddComponent<UniqueFactoryWorldInfoPanelExtended>();
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
    }
}
