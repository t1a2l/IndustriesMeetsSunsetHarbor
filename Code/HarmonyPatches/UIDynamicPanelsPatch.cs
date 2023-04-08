using HarmonyLib;
using ColossalFramework.UI;
using static ColossalFramework.UI.UIDynamicPanels;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System;
using IndustriesMeetsSunsetHarbor.UI;
using Object = UnityEngine.Object;
using IndustriesMeetsSunsetHarbor.Utils;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(UIDynamicPanels))]
    public static class UIDynamicPanelsPatch
    {
        [HarmonyPatch(typeof(UIDynamicPanels), "Init")]
        [HarmonyPostfix]
        public static void Init(UIDynamicPanels __instance, UIView view)
        {
            var m_CachedPanels = (Dictionary<string, DynamicPanelInfo>)typeof(UIDynamicPanels).GetField("m_CachedPanels", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
            var newUniqueFactorydDynamicPanelInfo = CreateDynamicPanelInfo(__instance, view, "NewUniqueFactoryWorldInfoPanel", "UniqueFactoryWorldInfoPanel");
            m_CachedPanels.Add(newUniqueFactorydDynamicPanelInfo.name, newUniqueFactorydDynamicPanelInfo);
            var restaurantAIdynamicPanelInfo = CreateDynamicPanelInfo(__instance, view, "RestaurantAIWorldInfoPanel", "UniqueFactoryWorldInfoPanel");
            m_CachedPanels.Add(restaurantAIdynamicPanelInfo.name, restaurantAIdynamicPanelInfo);
            typeof(UIDynamicPanels).GetField("m_CachedPanels", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, m_CachedPanels);
        }

        private static DynamicPanelInfo CreateDynamicPanelInfo(UIDynamicPanels __instance, UIView view, string customWorldInfoPanelName, string customOldWorldInfoPanelName)
        {
            DynamicPanelInfo dynamicPanelInfo = new();
            var customOldWorldInfoPanel = Array.Find(__instance.m_DynamicPanels, element => element.name == customOldWorldInfoPanelName);
            var customOldUIClone = UnityEngine.Object.Instantiate(customOldWorldInfoPanel.instance);
            customOldUIClone.name = customWorldInfoPanelName;
            typeof(DynamicPanelInfo).GetField("m_Name", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dynamicPanelInfo, customWorldInfoPanelName);
            typeof(DynamicPanelInfo).GetField("m_PanelRoot", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dynamicPanelInfo, customOldUIClone);
            typeof(DynamicPanelInfo).GetField("m_SingleInstance", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dynamicPanelInfo, true);
            typeof(DynamicPanelInfo).GetField("m_IsModal", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dynamicPanelInfo, false);
            dynamicPanelInfo.viewOwner = view;
	    GameObject gameObject = UnityEngine.Object.Instantiate(dynamicPanelInfo.panelRoot.gameObject);
            var old_component = gameObject.GetComponent<UniqueFactoryWorldInfoPanel>();
            Object.DestroyImmediate(old_component);
            if(customWorldInfoPanelName == "NewUniqueFactoryWorldInfoPanel")
            {
                var new_component = gameObject.AddComponent<NewUniqueFactoryWorldInfoPanel>();
                PrefabUtil.TryCopyAttributes<BuildingWorldInfoPanel>(old_component, new_component, false);
            }
            else if(customWorldInfoPanelName == "RestaurantAIWorldInfoPanel")
            {
                var new_component = gameObject.AddComponent<RestaurantAIWorldInfoPanel>();
                PrefabUtil.TryCopyAttributes<BuildingWorldInfoPanel>(old_component, new_component, false);
            }
	    gameObject.hideFlags = HideFlags.DontSave;
	    gameObject.name = "(Library) " + customWorldInfoPanelName;
            UIComponent uicomponent = view.AttachUIComponent(gameObject);
	    uicomponent.isVisible = false;
	    dynamicPanelInfo.AddInstance(uicomponent);
            return dynamicPanelInfo;
        }
    }
}
