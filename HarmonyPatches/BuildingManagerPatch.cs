using HarmonyLib;
using System;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(BuildingManager))]
    public static class BuildingManagerPatch
    {
        [HarmonyPatch(typeof(BuildingManager), "Awake")]
        [HarmonyPostfix]
        public static void Awake(BuildingManager __instance, ref FastList<ushort>[] ___m_areaBuildings, ref FastList<ushort>[] ___m_serviceBuildings)
        {
            Array.Resize(ref ___m_areaBuildings, 4640);
            Array.Resize(ref ___m_serviceBuildings, 20);
            ___m_serviceBuildings[19] = new FastList<ushort>();
        }
    }


    [HarmonyPatch(typeof(UnlockManager))]
    public static class UnlockManagerPatch
    {
        [HarmonyPatch(typeof(UnlockManager), "Unlocked", new Type[] { typeof(ItemClass.Service) },
            new ArgumentType[] { ArgumentType.Normal })]
        [HarmonyPrefix]
        public static bool Unlocked(UnlockManager __instance, ItemClass.Service service, ref bool __result)
	{
            if(service == (ItemClass.Service)28)
            {
                __result = __instance.Unlocked(null);
                return false;
            }
	    return true;
	}
    }
    
}
