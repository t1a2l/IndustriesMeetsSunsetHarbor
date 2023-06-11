using HarmonyLib;
using ICities;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(ItemClass))]
    public static class ItemClassPatch
    {

        [HarmonyPatch(typeof(ItemClass), "GetPrivateSubServiceIndex")]
        [HarmonyPrefix]
        public static bool GetPrivateSubServiceIndex(SubService subService, ref int __result)
        {
	    if(subService == (SubService)28)
            {
                __result = 20;
                return false;
            }
            return true;
        }
    }
}
