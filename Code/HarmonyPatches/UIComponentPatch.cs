using ColossalFramework.UI;
using HarmonyLib;
using System;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(UIComponent))]
    public static class UIComponentPatch
    {
        [HarmonyPatch(typeof(UIComponent), "Find", new Type[] { typeof(string), typeof(Type) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal })]
        [HarmonyPrefix]
        public static bool Find(UIComponent __instance, string searchName, Type type, ref UIComponent __result)
        {
            if (__instance.tooltipLocaleID == searchName && type.IsAssignableFrom(__instance.GetType()))
            {
                __result = __instance;
                return false;
            }
            return true;
        }

    }
}
