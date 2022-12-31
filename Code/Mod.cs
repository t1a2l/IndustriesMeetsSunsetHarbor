using CitiesHarmony.API;
using ICities;
using System;
using IndustriesMeetsSunsetHarbor.Managers;

namespace IndustriesMeetsSunsetHarbor
{
    public class Mod : LoadingExtensionBase, IUserMod
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
            }
            catch (Exception e)
            {
                LogHelper.Information(e.ToString());
                AquacultureFarmManager.Deinit();
            }
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            if (!inGame)
                return;
            inGame = false;
            AquacultureFarmManager.Deinit();
            LogHelper.Information("Unloading done!" + Environment.NewLine);
        }


    }

}
