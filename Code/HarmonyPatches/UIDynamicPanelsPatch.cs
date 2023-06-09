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
using ColossalFramework.DataBinding;

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
            if(newUniqueFactorydDynamicPanelInfo != null)
            {
                m_CachedPanels.Add(newUniqueFactorydDynamicPanelInfo.name, newUniqueFactorydDynamicPanelInfo);
            }
            var restaurantAIdynamicPanelInfo = CreateDynamicPanelInfo(__instance, view, "RestaurantWorldInfoPanel", "UniqueFactoryWorldInfoPanel");
            if(restaurantAIdynamicPanelInfo != null)
            {
                m_CachedPanels.Add(restaurantAIdynamicPanelInfo.name, restaurantAIdynamicPanelInfo);
            }
            typeof(UIDynamicPanels).GetField("m_CachedPanels", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, m_CachedPanels);
        }

        private static DynamicPanelInfo CreateDynamicPanelInfo(UIDynamicPanels __instance, UIView view, string customWorldInfoPanelName, string customOldWorldInfoPanelName)
        {
            DynamicPanelInfo dynamicPanelInfo = new();
            dynamicPanelInfo.viewOwner = view;
            var customOldWorldInfoPanel = Array.Find(__instance.m_DynamicPanels, element => element.name == customOldWorldInfoPanelName);
            if(customOldWorldInfoPanel == null)
            {
                return null;
            }
            GameObject ClonedGameObject = Object.Instantiate(customOldWorldInfoPanel.panelRoot.gameObject);
            ClonedGameObject.name = "(Library) " + customWorldInfoPanelName;
            if(customWorldInfoPanelName == "NewUniqueFactoryWorldInfoPanel")
            {
                var old_component = ClonedGameObject.GetComponent<UniqueFactoryWorldInfoPanel>();
                Object.DestroyImmediate(old_component);
                var newUniqueFactoryComp = ClonedGameObject.AddComponent<NewUniqueFactoryWorldInfoPanel>();
                PrefabUtil.TryCopyAttributes<WorldInfoPanel>(old_component, newUniqueFactoryComp, false);
                for (int i = 0; i < ClonedGameObject.transform.childCount; i++)
                {
                    var child = ClonedGameObject.transform.GetChild(i);
                    if(child != null)
                    {
                        if(child.name == "Caption")
                        {
                            for (int j = 0; j < child.transform.childCount; j++)
                            {
                                var caption_child = child.transform.GetChild(j);
                                if(caption_child != null)
                                {
                                    var caption_child_bind = caption_child.GetComponent<BindEvent>();
                                    if(caption_child_bind != null)
                                    {
                                        caption_child_bind.dataTarget.component = newUniqueFactoryComp;
                                    }
                                }
                            }
                        }
                        var bind = child.GetComponent<BindEvent>();
                        if(bind != null)
                        {
                            bind.dataTarget.component = newUniqueFactoryComp;
                        }
                    }
                }
                var main_panel = newUniqueFactoryComp.Find<UIPanel>("(Library) NewUniqueFactoryWorldInfoPanel");
                main_panel.cachedName = "(Library) NewUniqueFactoryWorldInfoPanel";
                typeof(DynamicPanelInfo).GetField("m_PanelRoot", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dynamicPanelInfo, main_panel);
            }
            else if(customWorldInfoPanelName == "RestaurantWorldInfoPanel")
            {
                var old_component = ClonedGameObject.GetComponent<UniqueFactoryWorldInfoPanel>();
                Object.DestroyImmediate(old_component);
                var restaurantComp = ClonedGameObject.AddComponent<RestaurantWorldInfoPanel>();
                var ProductionBar = restaurantComp.Find<UISlicedSprite>("ProductionBar");
                Object.DestroyImmediate(ProductionBar);
                PrefabUtil.TryCopyAttributes<WorldInfoPanel>(old_component, restaurantComp, false);
                for (int i = 0; i < ClonedGameObject.transform.childCount; i++)
                {
                    var child = ClonedGameObject.transform.GetChild(i);
                    if(child != null)
                    {
                        if(child.name == "Caption")
                        {
                            for (int j = 0; j < child.transform.childCount; j++)
                            {
                                var caption_child = child.transform.GetChild(j);
                                if(caption_child != null)
                                {
                                    var caption_child_bind = caption_child.GetComponent<BindEvent>();
                                    if(caption_child_bind != null)
                                    {
                                        caption_child_bind.dataTarget.component = restaurantComp;
                                    }
                                }
                            }
                        }
                        var bind = child.GetComponent<BindEvent>();
                        if(bind != null)
                        {
                            bind.dataTarget.component = restaurantComp;
                        }
                    }
                }
                var main_panel = restaurantComp.Find<UIPanel>("(Library) RestaurantWorldInfoPanel");
                main_panel.cachedName = "(Library) RestaurantWorldInfoPanel";
                typeof(DynamicPanelInfo).GetField("m_PanelRoot", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dynamicPanelInfo, main_panel);
            }
            typeof(DynamicPanelInfo).GetField("m_Name", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dynamicPanelInfo, customWorldInfoPanelName);
            typeof(DynamicPanelInfo).GetField("m_SingleInstance", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dynamicPanelInfo, true);
            typeof(DynamicPanelInfo).GetField("m_IsModal", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dynamicPanelInfo, false);
	    ClonedGameObject.hideFlags = HideFlags.DontSave;
            UIComponent uicomponent = view.AttachUIComponent(ClonedGameObject);
	    uicomponent.isVisible = false;
	    dynamicPanelInfo.AddInstance(uicomponent);
            return dynamicPanelInfo;
        }

    }
}
