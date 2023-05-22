using ColossalFramework.Globalization;
using ColossalFramework.UI;
using HarmonyLib;
using System.Reflection;
using IndustriesMeetsSunsetHarbor.AI;
using ColossalFramework;
using System;
using MoreTransferReasons;
using ICities;
using UnityEngine;
using IndustriesMeetsSunsetHarbor.Utils;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{

    [HarmonyPatch(typeof(WarehouseWorldInfoPanel))]
    public static class WarehouseWorldInfoPanelPatch
    {
        private delegate void BuildingWorldInfoPanelOnSetTargetDelegate(BuildingWorldInfoPanel instance);
        private static BuildingWorldInfoPanelOnSetTargetDelegate BaseOnSetTarget = AccessTools.MethodDelegate<BuildingWorldInfoPanelOnSetTargetDelegate>(typeof(BuildingWorldInfoPanel).GetMethod("OnSetTarget", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate void BuildingWorldInfoPanelUpdateBindingsDelegate(BuildingWorldInfoPanel instance);
        private static BuildingWorldInfoPanelUpdateBindingsDelegate BaseUpdateBindings = AccessTools.MethodDelegate<BuildingWorldInfoPanelUpdateBindingsDelegate>(typeof(BuildingWorldInfoPanel).GetMethod("UpdateBindings", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private enum WarehouseMode
        {
            Balanced,
            Import,
            Export
        }

        private static TransferManager.TransferReason[] m_transferReasons = new TransferManager.TransferReason[16]
        {
            TransferManager.TransferReason.None,
            TransferManager.TransferReason.Fish,
            TransferManager.TransferReason.AnimalProducts,
            TransferManager.TransferReason.Flours,
            TransferManager.TransferReason.Paper,
            TransferManager.TransferReason.PlanedTimber,
            TransferManager.TransferReason.Petroleum,
            TransferManager.TransferReason.Plastics,
            TransferManager.TransferReason.Glass,
            TransferManager.TransferReason.Metals,
            TransferManager.TransferReason.LuxuryProducts,
            TransferManager.TransferReason.Lumber,
            TransferManager.TransferReason.Food,
            TransferManager.TransferReason.Coal,
            TransferManager.TransferReason.Petrol,
            TransferManager.TransferReason.Goods
        };

        private static ExtendedTransferManager.TransferReason[] m_extendedTransferReasons = new ExtendedTransferManager.TransferReason[3]
        {
            ExtendedTransferManager.TransferReason.Bread, // 16 - 0
            ExtendedTransferManager.TransferReason.DrinkSupplies, // 17 - 1
            ExtendedTransferManager.TransferReason.FoodSupplies // 18 - 2
        };

        [HarmonyPatch(typeof(WarehouseWorldInfoPanel), "OnDropdownResourceChanged")]
        [HarmonyPrefix]
        public static bool OnDropdownResourceChanged(WarehouseWorldInfoPanel __instance, UIComponent component, int index, InstanceID ___m_InstanceID)
        {
            ExtendedWarehouseAI ai = Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building].Info.m_buildingAI as ExtendedWarehouseAI;
            Singleton<SimulationManager>.instance.AddAction(delegate
            {
                if (index < m_transferReasons.Length)
                {
                    ai.SetTransferReason(___m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building], m_transferReasons[index]);
                }
                else
                {
                    ai.SetExtendedTransferReason(___m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building], m_extendedTransferReasons[index - m_transferReasons.Length]);
                }
            });
            return false;
        }

        [HarmonyPatch(typeof(WarehouseWorldInfoPanel), "RefreshDropdownLists")]
        [HarmonyPrefix]
        public static bool RefreshDropdownLists(WarehouseWorldInfoPanel __instance)
        {
            var m_dropdownResource = (UIDropDown)typeof(WarehouseWorldInfoPanel).GetField("m_dropdownResource", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_dropdownMode = (UIDropDown)typeof(WarehouseWorldInfoPanel).GetField("m_dropdownMode", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_warehouseModes = (WarehouseMode[])typeof(WarehouseWorldInfoPanel).GetField("m_dropdownMode", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            string[] array = new string[m_transferReasons.Length + m_extendedTransferReasons.Length];
            for (int i = 0; i < m_transferReasons.Length; i++)
            {
                string text = (array[i] = Locale.Get("WAREHOUSEPANEL_RESOURCE", m_transferReasons[i].ToString()));
            }
            for (int j = m_transferReasons.Length; j < m_extendedTransferReasons.Length; j++)
            {
                string text = (array[j] = m_extendedTransferReasons[j].ToString());
            }
            m_dropdownResource.items = array;
            array = new string[m_warehouseModes.Length];
            for (int j = 0; j < m_warehouseModes.Length; j++)
            {
                string text2 = (array[j] = Locale.Get("WAREHOUSEPANEL_MODE", m_warehouseModes[j].ToString()));
            }
            m_dropdownMode.items = array;

            typeof(WarehouseWorldInfoPanel).GetField("m_dropdownResource", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_dropdownResource);
            typeof(WarehouseWorldInfoPanel).GetField("m_dropdownMode", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_dropdownMode);

            return false;
        }

        [HarmonyPatch(typeof(WarehouseWorldInfoPanel), "OnSetTarget")]
        [HarmonyPrefix]
        public static bool OnSetTarget(WarehouseWorldInfoPanel __instance, InstanceID ___m_InstanceID)
        {
            var m_resourcePanel = (UIPanel)typeof(WarehouseWorldInfoPanel).GetField("m_resourcePanel", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_originalHeight = (float)typeof(WarehouseWorldInfoPanel).GetField("m_originalHeight", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_dropdownResource = (UIDropDown)typeof(WarehouseWorldInfoPanel).GetField("m_dropdownResource", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_dropdownMode = (UIDropDown)typeof(WarehouseWorldInfoPanel).GetField("m_dropdownMode", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var warehouseMode = (WarehouseMode)typeof(WarehouseWorldInfoPanel).GetField("warehouseMode", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            BaseOnSetTarget(__instance);
            ExtendedWarehouseAI extendedWarehouseAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building].Info.m_buildingAI as ExtendedWarehouseAI;
            m_resourcePanel.isVisible = (extendedWarehouseAI.m_storageType == TransferManager.TransferReason.None && extendedWarehouseAI.m_extendedStorageType == ExtendedTransferManager.TransferReason.None);
            __instance.component.height = ((!m_resourcePanel.isVisible) ? (m_originalHeight - m_resourcePanel.height) : m_originalHeight);
            if (m_resourcePanel.isVisible)
            {
                int num = 0;
                TransferManager.TransferReason[] transferReasons = m_transferReasons;
                ExtendedTransferManager.TransferReason[] extendedTransferReasons2 = m_extendedTransferReasons;
                foreach (TransferManager.TransferReason transferReason in transferReasons)
                {
                    if (transferReason == extendedWarehouseAI.GetTransferReason(___m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building]))
                    {
                        m_dropdownResource.selectedIndex = num;
                        break;
                    }
                    num++;
                }
                foreach (ExtendedTransferManager.TransferReason extendedTransferReason in extendedTransferReasons2)
                {
                    if (extendedTransferReason == extendedWarehouseAI.GetExtendedTransferReason(___m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building]))
                    {
                        m_dropdownResource.selectedIndex = num;
                        break;
                    }
                    num++;
                }
            }
            m_dropdownMode.selectedIndex = (int)warehouseMode;

            typeof(WarehouseWorldInfoPanel).GetField("m_dropdownResource", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_dropdownResource);
            typeof(WarehouseWorldInfoPanel).GetField("m_dropdownMode", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_dropdownMode);

            return false;
        }

        [HarmonyPatch(typeof(WarehouseWorldInfoPanel), "UpdateBindings")]
        [HarmonyPrefix]
        public static bool UpdateBindings(WarehouseWorldInfoPanel __instance, InstanceID ___m_InstanceID)
        {
            var m_Type = (UILabel)typeof(WarehouseWorldInfoPanel).GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_Status = (UILabel)typeof(WarehouseWorldInfoPanel).GetField("m_Status", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_Upkeep = (UILabel)typeof(WarehouseWorldInfoPanel).GetField("m_Upkeep", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_Thumbnail = (UISprite)typeof(WarehouseWorldInfoPanel).GetField("m_Thumbnail", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_BuildingDesc = (UILabel)typeof(WarehouseWorldInfoPanel).GetField("m_BuildingDesc", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_RebuildButton = (UIButton)typeof(WarehouseWorldInfoPanel).GetField("m_RebuildButton", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_ActionPanel = (UIPanel)typeof(WarehouseWorldInfoPanel).GetField("m_ActionPanel", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_resourceProgressBar = (UIProgressBar)typeof(WarehouseWorldInfoPanel).GetField("m_resourceProgressBar", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_resourceLabel = (UILabel)typeof(WarehouseWorldInfoPanel).GetField("m_resourceLabel", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_emptyingOldResource = (UIPanel)typeof(WarehouseWorldInfoPanel).GetField("m_emptyingOldResource", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_resourceDescription = (UILabel)typeof(WarehouseWorldInfoPanel).GetField("m_resourceDescription", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_resourceSprite = (UISprite)typeof(WarehouseWorldInfoPanel).GetField("m_resourceSprite", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_buffer = (UIPanel)typeof(WarehouseWorldInfoPanel).GetField("m_buffer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_capacityLabel = (UILabel)typeof(WarehouseWorldInfoPanel).GetField("m_capacityLabel", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_Info = (UILabel)typeof(WarehouseWorldInfoPanel).GetField("m_Info", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);


            BaseUpdateBindings(__instance);
            ushort building = ___m_InstanceID.Building;
            BuildingManager instance = Singleton<BuildingManager>.instance;
            Building building2 = instance.m_buildings.m_buffer[building];
            BuildingInfo info = building2.Info;
            BuildingAI buildingAI = info.m_buildingAI;
            m_Type.text = Singleton<BuildingManager>.instance.GetDefaultBuildingName(building, InstanceID.Empty);
            m_Status.text = buildingAI.GetLocalizedStatus(building, ref instance.m_buildings.m_buffer[___m_InstanceID.Building]);
            m_Upkeep.text = LocaleFormatter.FormatUpkeep(buildingAI.GetResourceRate(building, ref instance.m_buildings.m_buffer[building], EconomyManager.Resource.Maintenance), isDistanceBased: false);
            m_Thumbnail.atlas = info.m_Atlas;
            m_Thumbnail.spriteName = info.m_Thumbnail;
            if (m_Thumbnail.atlas != null && !string.IsNullOrEmpty(m_Thumbnail.spriteName))
            {
                UITextureAtlas.SpriteInfo spriteInfo = m_Thumbnail.atlas[m_Thumbnail.spriteName];
                if (spriteInfo != null)
                {
                    m_Thumbnail.size = spriteInfo.pixelSize;
                }
            }
            m_BuildingDesc.text = info.GetLocalizedDescriptionShort();
            if ((building2.m_flags & Building.Flags.Collapsed) != 0)
            {
                m_RebuildButton.tooltip = ((!IsDisasterServiceRequired(___m_InstanceID)) ? LocaleFormatter.FormatCost(buildingAI.GetRelocationCost(), isDistanceBased: false) : Locale.Get("CITYSERVICE_TOOLTIP_DISASTERSERVICEREQUIRED"));
                m_RebuildButton.isVisible = Singleton<LoadingManager>.instance.SupportsExpansion(Expansion.NaturalDisasters);
                m_RebuildButton.isEnabled = __instance.CanRebuild();
                m_ActionPanel.isVisible = false;
            }
            else
            {
                m_RebuildButton.isVisible = false;
                m_ActionPanel.isVisible = true;
            }
            ExtendedWarehouseAI extendedWarehouseAI = buildingAI as ExtendedWarehouseAI;
            int num = building2.m_customBuffer1 * 100;
            m_resourceProgressBar.value = num / extendedWarehouseAI.m_storageCapacity;

            TransferManager.TransferReason transferReason = extendedWarehouseAI.GetTransferReason(___m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building]);
            ExtendedTransferManager.TransferReason extendedTransferReason = extendedWarehouseAI.GetExtendedTransferReason(___m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building]);

            TransferManager.TransferReason actualTransferReason = extendedWarehouseAI.GetActualTransferReason(___m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building]);
            ExtendedTransferManager.TransferReason actualExtendedTransferReason = extendedWarehouseAI.GetActualExtendedTransferReason(___m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building]);

            if (actualTransferReason != TransferManager.TransferReason.None)
            {
                m_resourceProgressBar.progressColor = IndustryWorldInfoPanel.instance.GetResourceColor(actualTransferReason);
                m_resourceLabel.text = Locale.Get("WAREHOUSEPANEL_RESOURCE", actualTransferReason.ToString());
                m_emptyingOldResource.isVisible = transferReason != actualTransferReason;
                m_resourceDescription.isVisible = transferReason != TransferManager.TransferReason.None;
                m_resourceDescription.text = GenerateResourceDescription(transferReason, isForWarehousePanel: true);
                m_resourceSprite.spriteName = IndustryWorldInfoPanel.ResourceSpriteName(actualTransferReason);
                string text = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), IndustryWorldInfoPanel.FormatResource((uint)num), IndustryWorldInfoPanel.FormatResourceWithUnit((uint)extendedWarehouseAI.m_storageCapacity, actualTransferReason));
                m_buffer.tooltip = text;
                m_capacityLabel.text = text;
            }
            else if (actualExtendedTransferReason != ExtendedTransferManager.TransferReason.None)
            {
                m_resourceProgressBar.progressColor = Color.Lerp(Color.grey, Color.black, 0.2f);
                m_resourceLabel.text = actualExtendedTransferReason.ToString();
                m_emptyingOldResource.isVisible = extendedTransferReason != actualExtendedTransferReason;
                m_resourceDescription.isVisible = extendedTransferReason != ExtendedTransferManager.TransferReason.None;
                m_resourceDescription.text = GenerateExtendedResourceDescription(extendedTransferReason, isForWarehousePanel: true);
                m_resourceSprite.atlas = TextureUtils.GetAtlas("RestaurantAtlas");
                m_resourceSprite.spriteName = extendedWarehouseAI.m_extendedStorageType.ToString();
                var FormatResource = IndustryWorldInfoPanel.FormatResource((uint)num);
                var formatResourceWithUnit = FormatResourceWithUnit((uint)extendedWarehouseAI.m_storageCapacity);
                string text = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), FormatResource, formatResourceWithUnit);
                m_buffer.tooltip = text;
                m_capacityLabel.text = text;
            }
            m_Info.text = buildingAI.GetLocalizedStats(building, ref instance.m_buildings.m_buffer[building]);
            if (extendedWarehouseAI != null)
            {
                UpdateWorkers(__instance, building, extendedWarehouseAI, ref instance.m_buildings.m_buffer[building]);
            }

            typeof(WarehouseWorldInfoPanel).GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_Type);
            typeof(WarehouseWorldInfoPanel).GetField("m_Status", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_Status);
            typeof(WarehouseWorldInfoPanel).GetField("m_Upkeep", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_Upkeep);
            typeof(WarehouseWorldInfoPanel).GetField("m_Thumbnail", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_Thumbnail);
            typeof(WarehouseWorldInfoPanel).GetField("m_BuildingDesc", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_BuildingDesc);
            typeof(WarehouseWorldInfoPanel).GetField("m_RebuildButton", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_RebuildButton);
            typeof(WarehouseWorldInfoPanel).GetField("m_ActionPanel", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_ActionPanel);
            typeof(WarehouseWorldInfoPanel).GetField("m_resourceProgressBar", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_resourceProgressBar);
            typeof(WarehouseWorldInfoPanel).GetField("m_resourceLabel", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_resourceLabel);
            typeof(WarehouseWorldInfoPanel).GetField("m_emptyingOldResource", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_emptyingOldResource);
            typeof(WarehouseWorldInfoPanel).GetField("m_resourceDescription", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_resourceDescription);
            typeof(WarehouseWorldInfoPanel).GetField("m_resourceSprite", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_resourceSprite);
            typeof(WarehouseWorldInfoPanel).GetField("m_buffer", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_buffer);
            typeof(WarehouseWorldInfoPanel).GetField("m_capacityLabel", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_capacityLabel);
            typeof(WarehouseWorldInfoPanel).GetField("m_Info", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_Info);

            return false;
        }

        public static string GenerateResourceDescription(TransferManager.TransferReason resource, bool isForWarehousePanel = false)
        {
            string text = Locale.Get("RESOURCEDESCRIPTION", resource.ToString());
            if (resource == TransferManager.TransferReason.None)
            {
                return text;
            }
            text += Environment.NewLine;
            text += Environment.NewLine;
            switch (resource)
            {
                case TransferManager.TransferReason.Oil:
                case TransferManager.TransferReason.Ore:
                case TransferManager.TransferReason.Logs:
                case TransferManager.TransferReason.Grain:
                    text = text + "- " + Locale.Get("RESOURCE_CANBEIMPORTED_COST");
                    break;
                case TransferManager.TransferReason.Goods:
                case TransferManager.TransferReason.Coal:
                case TransferManager.TransferReason.Petrol:
                case TransferManager.TransferReason.Food:
                case TransferManager.TransferReason.Lumber:
                    text = text + "- " + Locale.Get("RESOURCE_CANBEIMPORTED");
                    break;
                default:
                    text = text + "- " + Locale.Get("RESOURCE_CANNOTBEIMPORTED");
                    break;
            }
            text += Environment.NewLine;
            text = ((resource != TransferManager.TransferReason.Food && resource != TransferManager.TransferReason.Coal && resource != TransferManager.TransferReason.Petrol && resource != TransferManager.TransferReason.Lumber && resource != TransferManager.TransferReason.Goods) ? (text + "- " + ColossalFramework.Globalization.Locale.Get("RESOURCE_CANBEEXPORTED_COST")) : (text + "- " + ColossalFramework.Globalization.Locale.Get("RESOURCE_CANBEEXPORTED")));
            if (isForWarehousePanel)
            {
                return text;
            }
            text += Environment.NewLine;
            text += Environment.NewLine;
            if (resource == TransferManager.TransferReason.Ore || resource == TransferManager.TransferReason.Oil || resource == TransferManager.TransferReason.Grain || resource == TransferManager.TransferReason.Logs)
            {
                return text + "- " + LocaleFormatter.FormatGeneric("RESOURCE_STOREINSTORAGEBUILDING", Locale.Get("WAREHOUSEPANEL_RESOURCE", resource.ToString()));
            }
            return text + "- " + Locale.Get("RESOURCE_STOREINWAREHOUSE");
        }

        public static string GenerateExtendedResourceDescription(ExtendedTransferManager.TransferReason resource, bool isForWarehousePanel = false)
        {
            string text = resource.ToString();
            if (resource == ExtendedTransferManager.TransferReason.None)
            {
                return text;
            }
            text += Environment.NewLine;
            text += Environment.NewLine;
            text = text + "- " + Locale.Get("RESOURCE_CANNOTBEIMPORTED");
            text += Environment.NewLine;
            text += Environment.NewLine;
            text = "Cannot be exported";
            if (isForWarehousePanel)
            {
                return text;
            }
            text += Environment.NewLine;
            text += Environment.NewLine;
            return text + "- " + Locale.Get("RESOURCE_STOREINWAREHOUSE");
        }

        private static bool IsDisasterServiceRequired(InstanceID m_InstanceID)
        {
            ushort building = m_InstanceID.Building;
            if (building != 0)
            {
                return Singleton<BuildingManager>.instance.m_buildings.m_buffer[building].m_levelUpProgress != byte.MaxValue;
            }
            return false;
        }

        private static void UpdateWorkers(WarehouseWorldInfoPanel __instance, ushort buildingID, ExtendedWarehouseAI extendedWarehouseAI, ref Building building)
        {
            if (!Singleton<CitizenManager>.exists)
            {
                return;
            }
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint num = building.m_citizenUnits;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            int num6 = 0;
            int num7 = 0;
            int num8 = 0;
            int num9 = 0;
            int num10 = 0;
            int num11 = 0;
            int num12 = 0;
            num5 = extendedWarehouseAI.m_workPlaceCount0;
            num6 = extendedWarehouseAI.m_workPlaceCount1;
            num7 = extendedWarehouseAI.m_workPlaceCount2;
            num8 = extendedWarehouseAI.m_workPlaceCount3;
            num4 = num5 + num6 + num7 + num8;
            while (num != 0)
            {
                uint nextUnit = instance.m_units.m_buffer[num].m_nextUnit;
                if ((instance.m_units.m_buffer[num].m_flags & CitizenUnit.Flags.Work) != 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        uint citizen = instance.m_units.m_buffer[num].GetCitizen(i);
                        if (citizen != 0 && !instance.m_citizens.m_buffer[citizen].Dead && (instance.m_citizens.m_buffer[citizen].m_flags & Citizen.Flags.MovingIn) == 0)
                        {
                            num3++;
                            switch (instance.m_citizens.m_buffer[citizen].EducationLevel)
                            {
                                case Citizen.Education.Uneducated:
                                    num9++;
                                    break;
                                case Citizen.Education.OneSchool:
                                    num10++;
                                    break;
                                case Citizen.Education.TwoSchools:
                                    num11++;
                                    break;
                                case Citizen.Education.ThreeSchools:
                                    num12++;
                                    break;
                            }
                        }
                    }
                }
                num = nextUnit;
                if (++num2 > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            int num13 = 0;
            int num14 = num5 - num9;
            if (num10 > num6)
            {
                num13 += Mathf.Max(0, Mathf.Min(num14, num10 - num6));
            }
            num14 += num6 - num10;
            if (num11 > num7)
            {
                num13 += Mathf.Max(0, Mathf.Min(num14, num11 - num7));
            }
            num14 += num7 - num11;
            if (num12 > num8)
            {
                num13 += Mathf.Max(0, Mathf.Min(num14, num12 - num8));
            }
            string format = Locale.Get((num13 != 1) ? "ZONEDBUILDING_OVEREDUCATEDWORKERS" : "ZONEDBUILDING_OVEREDUCATEDWORKER");

            var m_OverWorkSituation = (UILabel)typeof(WarehouseWorldInfoPanel).GetField("m_OverWorkSituation", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_UneducatedPlaces = (UILabel)typeof(WarehouseWorldInfoPanel).GetField("m_UneducatedPlaces", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_EducatedPlaces = (UILabel)typeof(WarehouseWorldInfoPanel).GetField("m_EducatedPlaces", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_WellEducatedPlaces = (UILabel)typeof(WarehouseWorldInfoPanel).GetField("m_WellEducatedPlaces", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_HighlyEducatedPlaces = (UILabel)typeof(WarehouseWorldInfoPanel).GetField("m_HighlyEducatedPlaces", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_UneducatedWorkers = (UILabel)typeof(WarehouseWorldInfoPanel).GetField("m_UneducatedWorkers", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_EducatedWorkers = (UILabel)typeof(WarehouseWorldInfoPanel).GetField("m_EducatedWorkers", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_WellEducatedWorkers = (UILabel)typeof(WarehouseWorldInfoPanel).GetField("m_WellEducatedWorkers", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_HighlyEducatedWorkers = (UILabel)typeof(WarehouseWorldInfoPanel).GetField("m_HighlyEducatedWorkers", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_JobsAvailLegend = (UILabel)typeof(WarehouseWorldInfoPanel).GetField("m_JobsAvailLegend", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_WorkPlacesEducationChart = (UIRadialChart)typeof(WarehouseWorldInfoPanel).GetField("m_WorkPlacesEducationChart", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_WorkersEducationChart = (UIRadialChart)typeof(WarehouseWorldInfoPanel).GetField("m_WorkersEducationChart", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_workersInfoLabel = (UILabel)typeof(WarehouseWorldInfoPanel).GetField("m_workersInfoLabel", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            m_OverWorkSituation.text = StringUtils.SafeFormat(format, num13);
            m_OverWorkSituation.isVisible = num13 > 0;
            m_UneducatedPlaces.text = num5.ToString();
            m_EducatedPlaces.text = num6.ToString();
            m_WellEducatedPlaces.text = num7.ToString();
            m_HighlyEducatedPlaces.text = num8.ToString();
            m_UneducatedWorkers.text = num9.ToString();
            m_EducatedWorkers.text = num10.ToString();
            m_WellEducatedWorkers.text = num11.ToString();
            m_HighlyEducatedWorkers.text = num12.ToString();
            m_JobsAvailLegend.text = (num4 - (num9 + num10 + num11 + num12)).ToString();
            int num15 = GetValue(num5, num4);
            int value = GetValue(num6, num4);
            int value2 = GetValue(num7, num4);
            int value3 = GetValue(num8, num4);
            int num16 = num15 + value + value2 + value3;
            if (num16 != 0 && num16 != 100)
            {
                num15 = 100 - (value + value2 + value3);
            }
            m_WorkPlacesEducationChart.SetValues(num15, value, value2, value3);
            int value4 = GetValue(num9, num4);
            int value5 = GetValue(num10, num4);
            int value6 = GetValue(num11, num4);
            int value7 = GetValue(num12, num4);
            int num17 = 0;
            int num18 = value4 + value5 + value6 + value7;
            num17 = 100 - num18;
            m_WorkersEducationChart.SetValues(value4, value5, value6, value7, num17);
            m_workersInfoLabel.text = StringUtils.SafeFormat(Locale.Get("ZONEDBUILDING_WORKERS"), num3, num4);

            typeof(WarehouseWorldInfoPanel).GetField("m_OverWorkSituation", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_OverWorkSituation);
            typeof(WarehouseWorldInfoPanel).GetField("m_UneducatedPlaces", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_UneducatedPlaces);
            typeof(WarehouseWorldInfoPanel).GetField("m_EducatedPlaces", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_EducatedPlaces);
            typeof(WarehouseWorldInfoPanel).GetField("m_WellEducatedPlaces", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_WellEducatedPlaces);
            typeof(WarehouseWorldInfoPanel).GetField("m_HighlyEducatedPlaces", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_HighlyEducatedPlaces);
            typeof(WarehouseWorldInfoPanel).GetField("m_UneducatedWorkers", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_UneducatedWorkers);
            typeof(WarehouseWorldInfoPanel).GetField("m_EducatedWorkers", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_EducatedWorkers);
            typeof(WarehouseWorldInfoPanel).GetField("m_WellEducatedWorkers", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_WellEducatedWorkers);
            typeof(WarehouseWorldInfoPanel).GetField("m_HighlyEducatedWorkers", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_HighlyEducatedWorkers);
            typeof(WarehouseWorldInfoPanel).GetField("m_JobsAvailLegend", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_JobsAvailLegend);
            typeof(WarehouseWorldInfoPanel).GetField("m_WorkPlacesEducationChart", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_WorkPlacesEducationChart);
            typeof(WarehouseWorldInfoPanel).GetField("m_WorkersEducationChart", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_WorkersEducationChart);
            typeof(WarehouseWorldInfoPanel).GetField("m_workersInfoLabel", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_workersInfoLabel);

        }

        private static int GetValue(int value, int total)
	{
	    float num = (float)value / (float)total;
	    return Mathf.Clamp(Mathf.FloorToInt(num * 100f), 0, 100);
	}

        private static string FormatResourceWithUnit(uint amount)
        {
	    return string.Concat(str2: Locale.Get("RESOURCEUNIT_TONS"), str0: IndustryWorldInfoPanel.FormatResource(amount), str1: " ");
        }

    }
}
