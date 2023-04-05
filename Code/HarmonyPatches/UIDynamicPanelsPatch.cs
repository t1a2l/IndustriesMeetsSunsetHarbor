using HarmonyLib;
using ColossalFramework.UI;
using static ColossalFramework.UI.UIDynamicPanels;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System;
using IndustriesMeetsSunsetHarbor.UI;

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
            var m_PanelRoot = UnityEngine.Object.Instantiate(customOldWorldInfoPanel.instance);
            typeof(DynamicPanelInfo).GetField("m_Name", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField).SetValue(dynamicPanelInfo, "NewUniqueFactoryWorldInfoPanel");
            typeof(DynamicPanelInfo).GetField("m_PanelRoot", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField).SetValue(dynamicPanelInfo, m_PanelRoot);
            typeof(DynamicPanelInfo).GetField("m_SingleInstance", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField).SetValue(dynamicPanelInfo, true);
            typeof(DynamicPanelInfo).GetField("m_IsModal", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField).SetValue(dynamicPanelInfo, false);
            dynamicPanelInfo.viewOwner = view;
	    GameObject gameObject = UnityEngine.Object.Instantiate(dynamicPanelInfo.panelRoot.gameObject);
            // Type old_type = Type.GetType(customOldWorldInfoPanelName);
            var old_component = gameObject.GetComponent(typeof(UniqueFactoryWorldInfoPanel));
            GameObject.Destroy(old_component);
            //Type new_type = Type.GetType(customWorldInfoPanelName);
            if(customWorldInfoPanelName == "NewUniqueFactoryWorldInfoPanel")
            {
                gameObject.AddComponent(typeof(NewUniqueFactoryWorldInfoPanel));
            }
            else if(customWorldInfoPanelName == "RestaurantAIWorldInfoPanel")
            {
                gameObject.AddComponent(typeof(RestaurantAIWorldInfoPanel));
            }
	    gameObject.hideFlags = HideFlags.DontSave;
	    gameObject.name = "(Library) " + dynamicPanelInfo.panelRoot.name;
            UIComponent uicomponent = view.AttachUIComponent(gameObject);
	    uicomponent.isVisible = false;
	    dynamicPanelInfo.AddInstance(uicomponent);
            return dynamicPanelInfo;
        }
    }
}
