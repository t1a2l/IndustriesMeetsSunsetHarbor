using HarmonyLib;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(UniqueFactoryWorldInfoPanel))]
    public static class UniqueFactoryWorldInfoPanelPatch
    {
        [HarmonyPatch(typeof(UniqueFactoryWorldInfoPanel), "Start")]
        [HarmonyPrefix]
        public static bool Start(UniqueFactoryWorldInfoPanel __instance)
        {
            if(__instance.GetType().Name == "RestaurantWorldInfoPanel")
            {
                return false;
            }
            return true;
        }

    }
}
