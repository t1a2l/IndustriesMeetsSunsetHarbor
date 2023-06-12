using ColossalFramework;
using HarmonyLib;
using static InfoManager;
using ColossalFramework.UI;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(InfoManager))]
    class InfoManagerPatch
    {
        [HarmonyPatch(typeof(InfoManager), "SetMode")]
        [HarmonyPrefix]
        public static bool SetMode(InfoMode mode, SubInfoMode subMode, ref InfoMode ___m_currentMode, ref SubInfoMode ___m_currentSubMode)
        {
            if (mode == (InfoMode)41 && subMode == SubInfoMode.Default)
            {
                ___m_currentMode = mode;
                ___m_currentSubMode = subMode;
                Singleton<CoverageManager>.instance.SetMode((ItemClass.Service)28, ItemClass.SubService.None, ItemClass.SubService.None, ItemClass.Level.Level3, 500f, invertDirection: false);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(InfoManager), "ToggleModeUI")]
        [HarmonyPrefix]
        public static bool ToggleModeUI(InfoMode mode, ref InfoMode ___m_currentMode)
        {
            if (!(UIView.library != null))
            {
                return false;
            }
            if (___m_currentMode != mode)
            {
                if (___m_currentMode == (InfoMode)41)
                {
                    UIView.library.Hide("(Library) RestaurantInfoViewPanel");
                }
                if(mode == (InfoMode)41)
                {
                    UIView.library.Show("(Library) RestaurantInfoViewPanel", bringToFront: true, onlyWhenInvisible: true);
                    return false;
                } 
            }
            return true;
        }

        [HarmonyPatch(typeof(InfoManager), "IsInfoModeAvailable")]
        [HarmonyPrefix]
        public static bool IsInfoModeAvailable(InfoMode mode, ref bool __result)
        {
            if(mode == (InfoMode)41)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
