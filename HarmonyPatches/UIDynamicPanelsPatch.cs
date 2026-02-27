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
            var extendedProcessingFacilityWorldInfoPanel = CreateDynamicPanelInfo(__instance, view, "ExtendedProcessingFacilityWorldInfoPanel", "CityServiceWorldInfoPanel");
            if(extendedProcessingFacilityWorldInfoPanel != null)
            {
                m_CachedPanels.Add(extendedProcessingFacilityWorldInfoPanel.name, extendedProcessingFacilityWorldInfoPanel);
            }
            var FarmingWorldInfoPanel = CreateDynamicPanelInfo(__instance, view, "FarmingWorldInfoPanel", "IndustryWorldInfoPanel");
            if (FarmingWorldInfoPanel != null)
            {
                m_CachedPanels.Add(FarmingWorldInfoPanel.name, FarmingWorldInfoPanel);
            }
            var restaurantDynamicPanelInfo = CreateDynamicPanelInfo(__instance, view, "RestaurantWorldInfoPanel", "UniqueFactoryWorldInfoPanel");
            if(restaurantDynamicPanelInfo != null)
            {
                m_CachedPanels.Add(restaurantDynamicPanelInfo.name, restaurantDynamicPanelInfo);
            }
            var restaurantInfoViewPanelDynamicPanelInfo = CreateDynamicPanelInfo(__instance, view, "RestaurantInfoViewPanel", "PostInfoViewPanel");
            if (restaurantInfoViewPanelDynamicPanelInfo != null)
            {
                m_CachedPanels.Add(restaurantInfoViewPanelDynamicPanelInfo.name, restaurantInfoViewPanelDynamicPanelInfo);
            }
            var extendedWarehouseWorldInfoPanelInfo = CreateDynamicPanelInfo(__instance, view, "ExtendedWarehouseWorldInfoPanel", "WarehouseWorldInfoPanel");
            if (extendedWarehouseWorldInfoPanelInfo != null)
            {
                m_CachedPanels.Add(extendedWarehouseWorldInfoPanelInfo.name, extendedWarehouseWorldInfoPanelInfo);
            }
            typeof(UIDynamicPanels).GetField("m_CachedPanels", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, m_CachedPanels);
        }

        private static DynamicPanelInfo CreateDynamicPanelInfo(UIDynamicPanels __instance, UIView view, string customWorldInfoPanelName, string customOldWorldInfoPanelName)
        {
            DynamicPanelInfo dynamicPanelInfo = new()
            {
                viewOwner = view
            };
            var customOldWorldInfoPanel = Array.Find(__instance.m_DynamicPanels, element => element.name == customOldWorldInfoPanelName);
            if(customOldWorldInfoPanel == null)
            {
                return null;
            }
            GameObject ClonedGameObject = Object.Instantiate(customOldWorldInfoPanel.panelRoot.gameObject);
            ClonedGameObject.name = "(Library) " + customWorldInfoPanelName;
            if(customWorldInfoPanelName == "ExtendedProcessingFacilityWorldInfoPanel")
            {
                var old_component = ClonedGameObject.GetComponent<CityServiceWorldInfoPanel>();
                Object.DestroyImmediate(old_component);
                var extendedProcessingFacilityComp = ClonedGameObject.AddComponent<ExtendedProcessingFacilityWorldInfoPanel>();
                PrefabUtil.TryCopyAttributes<WorldInfoPanel>(old_component, extendedProcessingFacilityComp, false);
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
                                    caption_child_bind?.dataTarget.component = extendedProcessingFacilityComp;
                                }
                            }
                        }
                        var bind = child.GetComponent<BindEvent>();
                        bind?.dataTarget.component = extendedProcessingFacilityComp;
                    }
                }
                var main_panel = extendedProcessingFacilityComp.Find<UIPanel>("(Library) ExtendedProcessingFacilityWorldInfoPanel");
                main_panel.cachedName = "(Library) ExtendedProcessingFacilityWorldInfoPanel";
                typeof(DynamicPanelInfo).GetField("m_PanelRoot", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dynamicPanelInfo, main_panel);
            }
            else if (customWorldInfoPanelName == "FarmingWorldInfoPanel")
            {
                var old_component = ClonedGameObject.GetComponent<IndustryWorldInfoPanel>();
                Object.DestroyImmediate(old_component);
                var newFarmingComp = ClonedGameObject.AddComponent<FarmingWorldInfoPanel>();
                PrefabUtil.TryCopyAttributes<WorldInfoPanel>(old_component, newFarmingComp, false);
                for (int i = 0; i < ClonedGameObject.transform.childCount; i++)
                {
                    var child = ClonedGameObject.transform.GetChild(i);
                    if (child != null)
                    {
                        if (child.name == "Caption")
                        {
                            for (int j = 0; j < child.transform.childCount; j++)
                            {
                                var caption_child = child.transform.GetChild(j);
                                if (caption_child != null)
                                {
                                    var caption_child_bind = caption_child.GetComponent<BindEvent>();
                                    caption_child_bind?.dataTarget.component = newFarmingComp;
                                }
                            }
                        }
                        var bind = child.GetComponent<BindEvent>();
                        bind?.dataTarget.component = newFarmingComp;
                    }
                }
                var main_panel = newFarmingComp.Find<UIPanel>("(Library) FarmingWorldInfoPanel");
                main_panel.height = 475;
                main_panel.cachedName = "(Library) FarmingWorldInfoPanel";
                typeof(DynamicPanelInfo).GetField("m_PanelRoot", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dynamicPanelInfo, main_panel);
            }
            else if(customWorldInfoPanelName == "RestaurantWorldInfoPanel")
            {
                var old_component = ClonedGameObject.GetComponent<UniqueFactoryWorldInfoPanel>();
                Object.DestroyImmediate(old_component);
                var restaurantComp = ClonedGameObject.AddComponent<RestaurantWorldInfoPanel>();
                var ProductionSlider = restaurantComp.Find<UISlider>("ProductionSlider");
                Object.DestroyImmediate(ProductionSlider.gameObject);
                var LabelProductionRate = restaurantComp.Find<UILabel>("LabelProductionRate");
                Object.DestroyImmediate(LabelProductionRate.gameObject);
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
                                    caption_child_bind?.dataTarget.component = restaurantComp;
                                }
                            }
                        }
                        var bind = child.GetComponent<BindEvent>();
                        bind?.dataTarget.component = restaurantComp;
                    }
                }
                var main_panel = restaurantComp.Find<UIPanel>("(Library) RestaurantWorldInfoPanel");
                main_panel.cachedName = "(Library) RestaurantWorldInfoPanel";
                typeof(DynamicPanelInfo).GetField("m_PanelRoot", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dynamicPanelInfo, main_panel);
            }
            else if(customWorldInfoPanelName == "RestaurantInfoViewPanel")
            {
                var old_component = ClonedGameObject.GetComponent<PostInfoViewPanel>();
                Object.DestroyImmediate(old_component);
                var restaurantInfoViewPanelComp = ClonedGameObject.AddComponent<RestaurantInfoViewPanel>();
                PrefabUtil.TryCopyAttributes<InfoViewPanel>(old_component, restaurantInfoViewPanelComp, false);
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
                                    caption_child_bind?.dataTarget.component = restaurantInfoViewPanelComp;
                                }
                            }
                        }
                        var bind = child.GetComponent<BindEvent>();
                        bind?.dataTarget.component = restaurantInfoViewPanelComp;
                    }
                }
                var main_panel = restaurantInfoViewPanelComp.Find<UIPanel>("(Library) RestaurantInfoViewPanel");
                main_panel.cachedName = "(Library) RestaurantInfoViewPanel";
                typeof(DynamicPanelInfo).GetField("m_PanelRoot", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dynamicPanelInfo, main_panel);
            }
            else if (customWorldInfoPanelName == "ExtendedWarehouseWorldInfoPanel")
            {
                var old_component = ClonedGameObject.GetComponent<WarehouseWorldInfoPanel>();
                Object.DestroyImmediate(old_component);
                var extendedWarehouseComp = ClonedGameObject.AddComponent<ExtendedWarehouseWorldInfoPanel>();
                PrefabUtil.TryCopyAttributes<WorldInfoPanel>(old_component, extendedWarehouseComp, false);
                for (int i = 0; i < ClonedGameObject.transform.childCount; i++)
                {
                    var child = ClonedGameObject.transform.GetChild(i);
                    if (child != null)
                    {
                        if (child.name == "Caption")
                        {
                            for (int j = 0; j < child.transform.childCount; j++)
                            {
                                var caption_child = child.transform.GetChild(j);
                                if (caption_child != null)
                                {
                                    var caption_child_bind = caption_child.GetComponent<BindEvent>();
                                    caption_child_bind?.dataTarget.component = extendedWarehouseComp;
                                }
                            }
                        }
                        var bind = child.GetComponent<BindEvent>();
                        bind?.dataTarget.component = extendedWarehouseComp;
                    }
                }
                var main_panel = extendedWarehouseComp.Find<UIPanel>("(Library) ExtendedWarehouseWorldInfoPanel");
                main_panel.cachedName = "(Library) ExtendedWarehouseWorldInfoPanel";
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
