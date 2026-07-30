using HarmonyLib;
using ICities;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(ItemClass))]
    public static class ItemClassPatch
    {
        [HarmonyPatch(typeof(ItemClass), "GetPublicServiceIndex")]
        [HarmonyPrefix]
        public static bool GetPublicServiceIndex(Service service, ref int __result)
        {
	    if(service == (Service)29)
            {
                __result = 20;
                return false;
            }
            return true;
        }
    }
}
