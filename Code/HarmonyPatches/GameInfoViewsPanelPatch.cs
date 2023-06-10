using ColossalFramework;
using HarmonyLib;
using ColossalFramework.UI;
using UnityEngine;
using System.Reflection;
using System;
using IndustriesMeetsSunsetHarbor.Utils;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(GameInfoViewsPanel))]
    public static class GameInfoViewsPanelPatch
    {
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
                if (!__instance.IgnoreInfoView(___kResources[i].enumValue))
                {
                    if (i == old_length)
                    {
                        ___kResources[i].enumName = "Restaurant";
                        ___kResources[i].enumCategory = "Game";
                        ___kResources[i].index = 38;
                    }
                    uIButton = SpawnButtonEntry(__instance, ___kResources[i].enumName, "InfoIcon", "INFOVIEWS", num, true);
                    ___m_buttonToResource[num] = i;
                    ___m_resourceToButton[i] = num;
                    num++;
                }
            }

            if (uIButton != null)
            {
                UIPanel uIPanel = (UIPanel)__instance.m_ChildContainer;
                __instance.m_RootContainer.height = (float)Mathf.CeilToInt((float)num / 2f) * (uIButton.height + (float)uIPanel.autoLayoutPadding.top) + (float)uIPanel.autoLayoutPadding.top;
            }
            return false;
        }

        public static UIButton SpawnButtonEntry(InfoViewsPanel __instance, string name, string spriteBase, string localeID, int index, bool enabled)
        {

            var m_ObjectIndex = (int)typeof(InfoViewsPanel).GetField("m_ObjectIndex", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);

            UIButton uIButton;
            if (__instance.m_ChildContainer.childCount > m_ObjectIndex)
            {
                uIButton = __instance.m_ChildContainer.components[m_ObjectIndex] as UIButton;
            }
            else
            {
                GameObject asGameObject = UITemplateManager.GetAsGameObject("InfoViewButtonTemplate");
                uIButton = __instance.m_ChildContainer.AttachUIComponent(asGameObject) as UIButton;
            }
            TutorialUITag tutorialUITag = uIButton.gameObject.GetComponent<TutorialUITag>();
            if (tutorialUITag == null)
            {
                tutorialUITag = uIButton.gameObject.AddComponent<TutorialUITag>();
            }
            tutorialUITag.tutorialTag = "Info" + name;
            tutorialUITag.m_ParentOverride = __instance.m_ParentButton;
            uIButton.pivot = UIPivotPoint.TopCenter;
            if(name == "Restaurant")
            {
                uIButton.atlas = TextureUtils.GetAtlas("RestaurantInfoIconButtonAtlas");
            }
            else
            {
                uIButton.atlas = __instance.m_Atlas;
            }
            uIButton.text = string.Empty;
            uIButton.playAudioEvents = true;
            uIButton.name = name;
            uIButton.tooltipAnchor = UITooltipAnchor.Anchored;
            uIButton.tabStrip = true;
            uIButton.horizontalAlignment = UIHorizontalAlignment.Center;
            uIButton.verticalAlignment = UIVerticalAlignment.Middle;
            uIButton.zOrder = index;
            uIButton.isEnabled = enabled;
            string text2 = (uIButton.normalFgSprite = spriteBase + name);
            uIButton.focusedFgSprite = text2 + "Focused";
            uIButton.hoveredFgSprite = text2 + "Hovered";
            uIButton.pressedFgSprite = text2 + "Pressed";
            uIButton.disabledFgSprite = text2 + "Disabled";
            uIButton.tooltip = ColossalFramework.Globalization.Locale.Get(localeID, name);
            uIButton.group = __instance.component;
            m_ObjectIndex++;
            typeof(InfoViewsPanel).GetField("m_ObjectIndex", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, m_ObjectIndex);
            return uIButton;
        }
    }
}
