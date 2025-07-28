using ColossalFramework.UI;
using HarmonyLib;
using System;
using System.Reflection;

namespace IndustriesMeetsSunsetHarbor.Utils
{
    public static class UILabelUtils
    {
        private delegate void UpdateComponentHierarchyDelegate(UIComponent instance, bool force = false);
        private static UpdateComponentHierarchyDelegate UpdateComponentHierarchy = AccessTools.MethodDelegate<UpdateComponentHierarchyDelegate>(typeof(UIComponent).GetMethod("UpdateComponentHierarchy", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        public static UIComponent FindByLocaleID(UIComponent component, string searchName, Type type)
        {
            UpdateComponentHierarchy(component, force: true);
            for (int i = 0; i < component.childCount; i++)
            {
                UIComponent uIComponent = component.components[i];
                if (uIComponent is UILabel uILabel && uILabel.localeID == searchName && type.IsAssignableFrom(uIComponent.GetType()))
                {
                    return uIComponent;
                }
            }
            for (int j = 0; j < component.childCount; j++)
            {
                UIComponent uIComponent2 = FindByLocaleID(component.components[j], searchName, type);
                if (uIComponent2 != null)
                {
                    return uIComponent2;
                }
            }
            return null;
        }
    }
}


