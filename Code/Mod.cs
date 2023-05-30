using CitiesHarmony.API;
using ICities;
using System;
using IndustriesMeetsSunsetHarbor.Managers;
using IndustriesMeetsSunsetHarbor.Utils;
using IndustriesMeetsSunsetHarbor.Serializer;

namespace IndustriesMeetsSunsetHarbor
{
    public class Mod : LoadingExtensionBase, IUserMod
    {

        public static bool inGame = false;

        public static float DeliveryChance = 50f;

        public static float VisitChance = 50f;

        string IUserMod.Name => "Industries meets Sunset Harbor Mod";

        string IUserMod.Description => "Mix Industries and Sunset Harbor together";

        public void OnEnabled()
        {
            ModSettings.Load();
            AtlasUtils.CreateAtlas();
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
        }

        public void OnDisabled()
        {
            if (HarmonyHelper.IsHarmonyInstalled) Patcher.UnpatchAll();
        }


        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode != LoadMode.NewGame && mode != LoadMode.LoadGame)
            {
                return;
            }
            try
            {
                inGame = true;
                AquacultureFarmManager.Init();
                AquacultureExtractorManager.Init();
                ResourceMarketManager.Init();
                CustomBuffersManager.Init();
                RestaurantDeliveriesManager.Init();
            }
            catch (Exception e)
            {
                LogHelper.Error(e.ToString());
                AquacultureFarmManager.Deinit();
                AquacultureExtractorManager.Deinit();
                ResourceMarketManager.Deinit();
                CustomBuffersManager.Deinit();
                RestaurantDeliveriesManager.Deinit();
            }
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            if (!inGame)
                return;
            inGame = false;
            AquacultureFarmManager.Deinit();
            AquacultureExtractorManager.Deinit();
            ResourceMarketManager.Deinit();
            CustomBuffersManager.Deinit();
            RestaurantDeliveriesManager.Deinit();
            LogHelper.Information("Unloading done!" + Environment.NewLine);
        }


        public void OnSettingsUI(UIHelperBase helper)
        {
            if (IsInGame())
            {
                var group = helper.AddGroup("Industries meets Sunset Harbor Mod");

                group.AddSlider("Delivery Chance", 0.0f, 100.0f, 5.0f, 50.0f, sel =>
                {
                    DeliveryChance = sel;
                    ModSettings.Save();
                });

                group.AddSlider("Visit Chance", 0.0f, 100.0f, 5.0f, 50.0f, sel =>
                {
                    VisitChance = sel;
                    ModSettings.Save();
                });

            }
            
        }

        private bool IsInGame()
        {
            return !SimulationManager.exists
                   || SimulationManager.instance.m_metaData is {m_updateMode: SimulationManager.UpdateMode.LoadGame or SimulationManager.UpdateMode.NewGameFromMap or SimulationManager.UpdateMode.NewGameFromScenario};
        }

    }

}
