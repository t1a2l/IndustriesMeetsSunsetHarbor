using ColossalFramework.UI;
using HarmonyLib;
using ColossalFramework;
using UnityEngine;
using IndustriesMeetsSunsetHarbor.Utils;
using System.Reflection;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{

    [HarmonyPatch(typeof(CityServiceVehicleWorldInfoPanel))]
    public static class CityServiceVehicleWorldInfoPanelPatch
    {
        private delegate string VehicleWorldInfoPanelUpdateBindingsDelegate(VehicleWorldInfoPanel instance);
        private static VehicleWorldInfoPanelUpdateBindingsDelegate BaseUpdateBindings = AccessTools.MethodDelegate<VehicleWorldInfoPanelUpdateBindingsDelegate>(typeof(VehicleWorldInfoPanel).GetMethod("UpdateBindings", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate string GetVehicleIconDelegate(CityServiceVehicleWorldInfoPanel instance, ItemClass.Service service, ItemClass.SubService subService, VehicleInfo.VehicleType type);
        private static GetVehicleIconDelegate GetVehicleIcon = AccessTools.MethodDelegate<GetVehicleIconDelegate>(typeof(CityServiceVehicleWorldInfoPanel).GetMethod("GetVehicleIcon", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        [HarmonyPatch(typeof(CityServiceVehicleWorldInfoPanel), "UpdateBindings")]
        [HarmonyPrefix]
        public static bool UpdateBindings(CityServiceVehicleWorldInfoPanel __instance, ref InstanceID ___m_InstanceID, ref UIButton ___m_Owner, ref UILabel ___m_Load, ref UILabel ___m_LoadInfo, ref UIProgressBar ___m_LoadBar, ref UISprite ___m_VehicleType)
        {
            BaseUpdateBindings(__instance);
            if (___m_InstanceID.Type != InstanceType.Vehicle || ___m_InstanceID.Vehicle == 0)
            {
                return false;
            }
            InstanceID instanceID = ___m_InstanceID;
            ushort vehicle = ___m_InstanceID.Vehicle;
            ushort firstVehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicle].GetFirstVehicle(vehicle);
            if (firstVehicle == 0)
            {
                return false;
            }
            instanceID.Vehicle = firstVehicle;
            VehicleInfo info = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[firstVehicle].Info;
            if (info.GetService() == (ItemClass.Service)28)
            {
                ___m_VehicleType.atlas = TextureUtils.GetAtlas("RestaurantInfoIconButtonAtlas");
                ___m_VehicleType.spriteName = "InfoIconRestaurant";
            }
            else
            {
                ___m_VehicleType.atlas = info.m_Atlas;
                ___m_VehicleType.spriteName = GetVehicleIcon(__instance, info.GetService(), info.GetSubService(), info.m_vehicleType);
            }
            InstanceID ownerID = info.m_vehicleAI.GetOwnerID(firstVehicle, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[firstVehicle]);
            if (ownerID.Building != 0)
            {
                ___m_Owner.objectUserData = ownerID;
                ___m_Owner.text = Singleton<BuildingManager>.instance.GetBuildingName(ownerID.Building, instanceID);
                ___m_Owner.isEnabled = (Singleton<BuildingManager>.instance.m_buildings.m_buffer[ownerID.Building].m_flags & Building.Flags.IncomingOutgoing) == 0;
            }
            else
            {
                ___m_Owner.objectUserData = ownerID;
                ___m_Owner.text = string.Empty;
                ___m_Owner.isEnabled = false;
            }
            ShortenTextToFitParent(___m_Owner);
            if (!(___m_Load != null) && !(___m_LoadInfo != null) && !(___m_LoadBar != null))
            {
                return false;
            }
            info.m_vehicleAI.GetBufferStatus(firstVehicle, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[firstVehicle], out var localeKey, out var current, out var max);
            if (max > 0)
            {
                int percentage = GetPercentage(current, max);
                if (___m_Load != null)
                {
                    ___m_Load.parent.isVisible = true;
                    ___m_Load.text = ColossalFramework.Globalization.Locale.Get("VEHICLE_BUFFER", localeKey);
                }
                if (___m_LoadInfo != null)
                {
                    ___m_LoadInfo.parent.isVisible = true;
                    ___m_LoadInfo.text = StringUtils.SafeFormat(ColossalFramework.Globalization.Locale.Get("VALUE_PERCENTAGE"), percentage);
                }
                if (___m_LoadBar != null)
                {
                    ___m_LoadBar.parent.isVisible = true;
                    ___m_LoadBar.maxValue = max;
                    ___m_LoadBar.value = current;
                }
            }
            else
            {
                if (___m_Load != null)
                {
                    ___m_Load.parent.isVisible = false;
                }
                if (___m_LoadInfo != null)
                {
                    ___m_LoadInfo.parent.isVisible = false;
                }
                if (___m_LoadBar != null)
                {
                    ___m_LoadBar.parent.isVisible = false;
                }
            }
            return false;
        }

        private static void ShortenTextToFitParent(UIButton button)
        {
            float num = button.parent.width - button.relativePosition.x;
            if (button.width > num)
            {
                button.tooltip = button.text;
                string text = button.text;
                while (button.width > num && text.Length > 5)
                {
                    text = text.Substring(0, text.Length - 4);
                    text = text.Trim();
                    text = (button.text = text + "...");
                }
            }
            else
            {
                button.tooltip = string.Empty;
            }
        }

        private static int GetPercentage(int current, int max)
        {
            float num = (float)current / (float)max * 100f;
            int result = Mathf.RoundToInt(num);
            if (num < 1f && !Mathf.Approximately(num, 0f))
            {
                result = 1;
            }
            if (num > 99f && !Mathf.Approximately(num, 100f))
            {
                result = 99;
            }
            return result;
        }
    }

}

