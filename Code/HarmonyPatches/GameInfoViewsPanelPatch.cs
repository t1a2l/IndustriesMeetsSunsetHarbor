using ColossalFramework;
using HarmonyLib;
using ColossalFramework.UI;
using UnityEngine;
using System.Reflection;
using System;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(GameInfoViewsPanel))]
    public static class GameInfoViewsPanelPatch
    {
        private delegate void SelectByIndexInfoViewsPanelDelegate(InfoViewsPanel instance, int value, bool focusAdvisor);
        private static SelectByIndexInfoViewsPanelDelegate InfoViewsPanelSelectByIndex = AccessTools.MethodDelegate<SelectByIndexInfoViewsPanelDelegate>(typeof(InfoViewsPanel).GetMethod("SelectByIndex", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate void ShowSelectedIndexInfoViewsPanelDelegate(InfoViewsPanel instance);
        private static ShowSelectedIndexInfoViewsPanelDelegate InfoViewsPanelShowSelectedIndex = AccessTools.MethodDelegate<ShowSelectedIndexInfoViewsPanelDelegate>(typeof(InfoViewsPanel).GetMethod("ShowSelectedIndex", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate UIButton SpawnButtonEntryInfoViewsPanelDelegate(InfoViewsPanel instance, string name, string spriteBase, string localeID, int index, bool enabled);
        private static SpawnButtonEntryInfoViewsPanelDelegate InfoViewsPanelSpawnButtonEntry = AccessTools.MethodDelegate<SpawnButtonEntryInfoViewsPanelDelegate>(typeof(InfoViewsPanel).GetMethod("SpawnButtonEntry", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        [HarmonyPatch(typeof(GameInfoViewsPanel), "RefreshPanel")]
        [HarmonyPrefix]
        public static bool RefreshPanel(GameInfoViewsPanel __instance, ref PositionData<InfoManager.InfoMode>[] ___kResources, ref int[] ___m_buttonToResource, ref int[] ___m_resourceToButton)
        {
            var old_length = ___kResources.Length;
            ___m_buttonToResource = new int[old_length + 1];
            ___m_resourceToButton = new int[old_length + 1];
            Array.Resize(ref ___kResources, old_length + 1);
            __instance.CleanPanel();
            UIButton uIButton = null;
            int num = 0;
            for (int i = 0; i < ___kResources.Length; i++)
            {
                if (i == old_length)
                {
                    ___kResources[i].enumName = "Restaurant";
                    ___kResources[i].enumCategory = "Game";
                    ___kResources[i].index = 38;
                    ___kResources[i].enumValue = (InfoManager.InfoMode)41;
                    uIButton = InfoViewsPanelSpawnButtonEntry(__instance, "Restaurant", "InfoIcon", "INFOVIEWS", num, true);
                    ___m_buttonToResource[num] = i;
                    ___m_resourceToButton[i] = num;
                    num++;
                }
                else
                {
                    if (!__instance.IgnoreInfoView(___kResources[i].enumValue))
                    {
                        uIButton = InfoViewsPanelSpawnButtonEntry(__instance, ___kResources[i].enumName, "InfoIcon", "INFOVIEWS", num, true);
                        ___m_buttonToResource[num] = i;
                        ___m_resourceToButton[i] = num;
                        num++;
                    }
                }
            }

            if (uIButton != null)
            {
                UIPanel uIPanel = (UIPanel)__instance.m_ChildContainer;
                __instance.m_RootContainer.height = (float)Mathf.CeilToInt((float)num / 2f) * (uIButton.height + (float)uIPanel.autoLayoutPadding.top) + (float)uIPanel.autoLayoutPadding.top;
            }
            return false;
        }

        [HarmonyPatch(typeof(GameInfoViewsPanel), "ShowSelectedIndex")]
        [HarmonyPrefix]
        public static void ShowSelectedIndex(GameInfoViewsPanel __instance, ref InfoManager.InfoMode ___m_cachedMode, ref int ___m_cachedIndex, ref int[] ___m_resourceToButton)
        {
            if (Singleton<InfoManager>.exists)
            {
                if (Singleton<InfoManager>.instance.NextMode != ___m_cachedMode)
                {
                    ___m_cachedMode = Singleton<InfoManager>.instance.NextMode;
                    if (___m_cachedMode == (InfoManager.InfoMode)41)
                    {
                        ___m_cachedIndex = 38;
                    }
                    else
                    {
                        ___m_cachedIndex = ColossalFramework.Utils.GetEnumIndexByValue(___m_cachedMode, "Game");
                    }
                }
                int num = ___m_cachedIndex;
                if (num >= 0 && num < ___m_resourceToButton.Length)
                {
                    num = ___m_resourceToButton[num];
                }
                if (num != __instance.selectedIndex)
                {
                    InfoViewsPanelSelectByIndex(__instance, num, focusAdvisor: false);
                }
            }
            InfoViewsPanelShowSelectedIndex(__instance);
        }
    }
}
