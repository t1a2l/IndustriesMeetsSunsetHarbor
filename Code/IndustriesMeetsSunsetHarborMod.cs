using CitiesHarmony.API;
using ICities;
using System;
using IndustriesMeetsSunsetHarbor.Managers;
using IndustriesMeetsSunsetHarbor.Utils;

namespace IndustriesMeetsSunsetHarbor
{
    public class IndustriesMeetsSunsetHarborMod : LoadingExtensionBase, IUserMod
    {

        public static bool inGame = false;

        string IUserMod.Name => "Industries meets Sunset Harbor Mod";

        string IUserMod.Description => "Mix Industries and Sunset Harbor together";

        public void OnEnabled()
        {
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
            }
            catch (Exception e)
            {
                LogHelper.Error(e.ToString());
                AquacultureFarmManager.Deinit();
                AquacultureExtractorManager.Deinit();
                ResourceMarketManager.Deinit();
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
            LogHelper.Information("Unloading done!" + Environment.NewLine);
        }


    }

}
