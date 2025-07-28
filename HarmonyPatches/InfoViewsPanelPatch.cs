using IndustriesMeetsSunsetHarbor.Utils;
using HarmonyLib;
using UnityEngine;
using ColossalFramework.UI;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(InfoViewsPanel))]
    public static class InfoViewsPanelPatch
    {
        [HarmonyPatch(typeof(InfoViewsPanel), "SpawnButtonEntry")]
        [HarmonyPrefix]
        public static bool SpawnButtonEntry(InfoViewsPanel __instance, string name, string spriteBase, string localeID, int index, bool enabled, ref UIButton __result, ref int ___m_ObjectIndex)
        {
            UIButton uIButton;
            if (__instance.m_ChildContainer.childCount > ___m_ObjectIndex)
            {
                uIButton = __instance.m_ChildContainer.components[___m_ObjectIndex] as UIButton;
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
            if (name == "Restaurant")
            {
                uIButton.atlas = TextureUtils.GetAtlas("InfoIconRestaurantButtonAtlas");
                uIButton.tooltip = "Restaurant Locations";
            }
            else
            {
                uIButton.atlas = __instance.m_Atlas;
                uIButton.tooltip = ColossalFramework.Globalization.Locale.Get(localeID, name);
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
            uIButton.group = __instance.component;
            ___m_ObjectIndex++;
            __result = uIButton;
            return false;
        }

    }
}
