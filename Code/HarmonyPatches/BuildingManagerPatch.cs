using HarmonyLib;
using System;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(BuildingManager))]
    public static class BuildingManagerPatch
    {
        [HarmonyPatch(typeof(BuildingManager), "Awake")]
        [HarmonyPostfix]
        public static void Awake(BuildingManager __instance, ref FastList<ushort>[] ___m_areaBuildings)
        {
            Array.Resize(ref ___m_areaBuildings, 4640);
        }
    }
}
