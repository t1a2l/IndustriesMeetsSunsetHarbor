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
            var warehouseAIdynamicPanelInfo = CreateDynamicPanelInfo(__instance, view, "ExtendedWarehouseWorldInfoPanel", "WarehouseWorldInfoPanel");
            if(warehouseAIdynamicPanelInfo != null)
            {
                m_CachedPanels.Add(warehouseAIdynamicPanelInfo.name, warehouseAIdynamicPanelInfo);
            }
            typeof(UIDynamicPanels).GetField("m_CachedPanels", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, m_CachedPanels);
        }

        private static DynamicPanelInfo CreateDynamicPanelInfo(UIDynamicPanels __instance, UIView view, string customWorldInfoPanelName, string customOldWorldInfoPanelName)
        {
            DynamicPanelInfo dynamicPanelInfo = new();
            var customOldWorldInfoPanel = Array.Find(__instance.m_DynamicPanels, element => element.name == customOldWorldInfoPanelName);
            if(customOldWorldInfoPanel == null)
            {
                return null;
            }
            var customOldUIClone = Object.Instantiate(customOldWorldInfoPanel.instance);
            customOldUIClone.gameObject.name = customWorldInfoPanelName;
            if(customWorldInfoPanelName == "RestaurantWorldInfoPanel")
            {
                var ProductionBar = customOldUIClone.Find<UISlicedSprite>("ProductionBar");
                Object.DestroyImmediate(ProductionBar);
            }
            typeof(DynamicPanelInfo).GetField("m_Name", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dynamicPanelInfo, customWorldInfoPanelName);
            typeof(DynamicPanelInfo).GetField("m_PanelRoot", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dynamicPanelInfo, customOldUIClone);
            typeof(DynamicPanelInfo).GetField("m_SingleInstance", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dynamicPanelInfo, true);
            typeof(DynamicPanelInfo).GetField("m_IsModal", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dynamicPanelInfo, false);
            dynamicPanelInfo.viewOwner = view;
	    GameObject gameObject = Object.Instantiate(dynamicPanelInfo.panelRoot.gameObject);
            if(customWorldInfoPanelName == "NewUniqueFactoryWorldInfoPanel")
            {
                var old_component = gameObject.GetComponent<UniqueFactoryWorldInfoPanel>();
                Object.DestroyImmediate(old_component);
                var newUniqueFactoryComp = gameObject.AddComponent<NewUniqueFactoryWorldInfoPanel>();
                PrefabUtil.TryCopyAttributes<WorldInfoPanel>(old_component, newUniqueFactoryComp, false);
                for (int i = 0; i < gameObject.transform.childCount; i++)
                {
                    var child = gameObject.transform.GetChild(i);
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
            }
            else if(customWorldInfoPanelName == "RestaurantWorldInfoPanel")
            {
                var old_component = gameObject.GetComponent<UniqueFactoryWorldInfoPanel>();
                Object.DestroyImmediate(old_component);
                var restaurantComp = gameObject.AddComponent<RestaurantWorldInfoPanel>();
                PrefabUtil.TryCopyAttributes<WorldInfoPanel>(old_component, restaurantComp, false);
                for (int i = 0; i < gameObject.transform.childCount; i++)
                {
                    var child = gameObject.transform.GetChild(i);
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
            }
            else if(customWorldInfoPanelName == "ExtendedWarehouseWorldInfoPanel")
            {
                var old_component = gameObject.GetComponent<WarehouseWorldInfoPanel>();
                Object.DestroyImmediate(old_component);
                var extendedWarehouseComp = gameObject.AddComponent<ExtendedWarehouseWorldInfoPanel>();
                PrefabUtil.TryCopyAttributes<WorldInfoPanel>(old_component, extendedWarehouseComp, false);
                for (int i = 0; i < gameObject.transform.childCount; i++)
                {
                    var child = gameObject.transform.GetChild(i);
                    if(child != null)
                    {
                        if(child.name == "Caption")
                        {
                            for (int j = 0; j < child.transform.childCount; j++)
                            {
                                var caption_child = child.transform.GetChild(j);
                                if(caption_child != null)
                                {
                                    if(caption_child.name == "Panel")
                                    {
                                        for (int k = 0; k < caption_child.transform.childCount; k++)
                                        {
                                            var panel_child = caption_child.transform.GetChild(k);
                                            if(panel_child != null)
                                            {
                                                var panel_child_bind = panel_child.GetComponent<BindEvent>();
                                                if(panel_child_bind != null)
                                                {
                                                    panel_child_bind.dataTarget.component = extendedWarehouseComp;
                                                }
                                            }
                                        }
                                    }
                                    var caption_child_bind = caption_child.GetComponent<BindEvent>();
                                    if(caption_child_bind != null)
                                    {
                                        caption_child_bind.dataTarget.component = extendedWarehouseComp;
                                    }
                                }
                            }
                            
                        }
                        var bind = child.GetComponent<BindEvent>();
                        if(bind != null)
                        {
                            bind.dataTarget.component = extendedWarehouseComp;
                        }
                    }
                }
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
