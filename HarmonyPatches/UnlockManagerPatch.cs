using HarmonyLib;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(UnlockManager))]
    public static class UnlockManagerPatch
    {
        [HarmonyPatch(typeof(UnlockManager), "Unlocked", [typeof(ItemClass.Service)], [ArgumentType.Normal])]
        [HarmonyPrefix]
        public static bool Unlocked(UnlockManager __instance, ItemClass.Service service, ref bool __result)
        {
            if (service == (ItemClass.Service)28)
            {
                __result = __instance.Unlocked(null);
                return false;
            }
            return true;
        }
    }
}
