using HarmonyLib;
using System;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(InfoProperties))]
    public static class InfoPropertiesPatch
    {
        [HarmonyPatch(typeof(InfoProperties), "InitializeShaderProperties")]
        [HarmonyPrefix]
        public static bool InitializeShaderProperties(InfoProperties __instance)
        {
            if (__instance.m_neutralColor.a != 0f)
            {
                __instance.m_neutralColor.a = 0f;
            }
            Shader.SetGlobalColor("_InfoObjectColor", __instance.m_neutralColor.linear);
            string[] names = Enum.GetNames(typeof(InfoManager.InfoMode));
            Array.Resize(ref __instance.m_modeProperties, __instance.m_modeProperties.Length + 1);
            Array.Resize(ref names, names.Length + 1);
            int num = Mathf.Min(__instance.m_modeProperties.Length, names.Length);
            names[names.Length - 1] = "Restaurant";
            __instance.m_modeProperties[names.Length - 1] = __instance.m_modeProperties[35];
            for (int i = 0; i < num; i++)
            {
                Shader.SetGlobalColor("_InfoColor" + names[i], __instance.m_modeProperties[i].m_activeColor.linear);
                Shader.SetGlobalColor("_InfoColorB" + names[i], __instance.m_modeProperties[i].m_activeColorB.linear);
                Shader.SetGlobalColor("_InfoColorInactive" + names[i], __instance.m_modeProperties[i].m_inactiveColor.linear);
            }
            return false;
        }



    }
}
