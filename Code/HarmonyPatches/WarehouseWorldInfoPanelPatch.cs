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
        public static bool RefreshDropdownLists(WarehouseWorldInfoPanel __instance, ref UIDropDown ___m_dropdownResource, ref UIDropDown ___m_dropdownMode)
        {
            var m_warehouseModes = (int[])typeof(WarehouseWorldInfoPanel).GetField("m_warehouseModes", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            string[] array = new string[m_transferReasons.Length + m_extendedTransferReasons.Length];
            for (int i = 0; i < m_transferReasons.Length; i++)
            {
                string text = (array[i] = Locale.Get("WAREHOUSEPANEL_RESOURCE", m_transferReasons[i].ToString()));
            }
            for (int j = m_transferReasons.Length; j < array.Length; j++)
            {
                string text = (array[j] = m_extendedTransferReasons[j - m_transferReasons.Length].ToString());
            }
            ___m_dropdownResource.items = array;
            array = new string[m_warehouseModes.Length];
            for (int k = 0; k < m_warehouseModes.Length; k++)
            {
                string text2 = (array[k] = m_warehouseModes[k].ToString());
            }
            ___m_dropdownMode.items = array;
            return false;
        }

        [HarmonyPatch(typeof(WarehouseWorldInfoPanel), "OnSetTarget")]
        [HarmonyPrefix]
        public static bool OnSetTarget(WarehouseWorldInfoPanel __instance, InstanceID ___m_InstanceID, ref UIPanel ___m_resourcePanel, ref float ___m_originalHeight, ref UIDropDown ___m_dropdownResource, ref UIDropDown ___m_dropdownMode)
        {
            BaseOnSetTarget(__instance);
            ExtendedWarehouseAI extendedWarehouseAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building].Info.m_buildingAI as ExtendedWarehouseAI;
            ___m_resourcePanel.isVisible = (extendedWarehouseAI.m_storageType == TransferManager.TransferReason.None && extendedWarehouseAI.m_extendedStorageType == ExtendedTransferManager.TransferReason.None);
            __instance.component.height = ((!___m_resourcePanel.isVisible) ? (___m_originalHeight - ___m_resourcePanel.height) : ___m_originalHeight);
            if (___m_resourcePanel.isVisible)
            {
                int num = 0;
                TransferManager.TransferReason[] transferReasons = m_transferReasons;
                ExtendedTransferManager.TransferReason[] extendedTransferReasons2 = m_extendedTransferReasons;
                var material_byte = extendedWarehouseAI.GetTransferReason(___m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building]);
                if(material_byte != 255)
                {
                    if(material_byte < 200)
                    {
                        foreach (TransferManager.TransferReason transferReason in transferReasons)
                        {
                            if ((byte)transferReason == material_byte)
                            {
                                ___m_dropdownResource.selectedIndex = num;
                                break;
                            }
                            num++;
                        }
                    }
                    if(material_byte >= 200)
                    {
                        num = 16;
                        byte extended_material_byte = (byte)(material_byte - 200);
                        foreach (ExtendedTransferManager.TransferReason extendedTransferReason in extendedTransferReasons2)
                        {
                            if ((byte)extendedTransferReason == extended_material_byte)
                            {
                                ___m_dropdownResource.selectedIndex = num;
                                break;
                            }
                            num++;
                        }
                    }
                }
            }
            var warehouseMode = (int)typeof(WarehouseWorldInfoPanel).GetProperty("warehouseMode", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance, null);
            ___m_dropdownMode.selectedIndex = warehouseMode;
            return false;
        }

        [HarmonyPatch(typeof(WarehouseWorldInfoPanel), "UpdateBindings")]
        [HarmonyPrefix]
        public static bool UpdateBindings(WarehouseWorldInfoPanel __instance, InstanceID ___m_InstanceID, ref UILabel ___m_Type, ref UILabel ___m_Status, ref UILabel ___m_Upkeep, ref UISprite ___m_Thumbnail, ref UILabel ___m_BuildingDesc, ref UIButton ___m_RebuildButton, ref UIPanel ___m_ActionPanel, ref UIProgressBar ___m_resourceProgressBar, ref UILabel ___m_resourceLabel, ref UIPanel ___m_emptyingOldResource, ref UILabel ___m_resourceDescription, ref UISprite ___m_resourceSprite, ref UIPanel ___m_buffer, ref UILabel ___m_capacityLabel, ref UILabel ___m_Info, ref UILabel ___m_OverWorkSituation, ref UILabel ___m_UneducatedPlaces, ref UILabel ___m_EducatedPlaces, ref UILabel ___m_WellEducatedPlaces, ref UILabel ___m_HighlyEducatedPlaces, ref UILabel ___m_UneducatedWorkers, ref UILabel ___m_EducatedWorkers, ref UILabel ___m_WellEducatedWorkers, ref UILabel ___m_HighlyEducatedWorkers, ref UILabel ___m_JobsAvailLegend, ref UIRadialChart ___m_WorkPlacesEducationChart, ref UIRadialChart ___m_WorkersEducationChart, ref UILabel ___m_workersInfoLabel)
        {
            BaseUpdateBindings(__instance);
            ushort building = ___m_InstanceID.Building;
            BuildingManager instance = Singleton<BuildingManager>.instance;
            Building building2 = instance.m_buildings.m_buffer[building];
            BuildingInfo info = building2.Info;
            BuildingAI buildingAI = info.m_buildingAI;
            ExtendedWarehouseAI extendedWarehouseAI = buildingAI as ExtendedWarehouseAI;
            ___m_Type.text = Singleton<BuildingManager>.instance.GetDefaultBuildingName(building, InstanceID.Empty);
            ___m_Status.text = extendedWarehouseAI.GetLocalizedStatus(building, ref instance.m_buildings.m_buffer[___m_InstanceID.Building]);
            ___m_Upkeep.text = LocaleFormatter.FormatUpkeep(extendedWarehouseAI.GetResourceRate(building, ref instance.m_buildings.m_buffer[building], EconomyManager.Resource.Maintenance), isDistanceBased: false);
            ___m_Thumbnail.atlas = info.m_Atlas;
            ___m_Thumbnail.spriteName = info.m_Thumbnail;
            if (___m_Thumbnail.atlas != null && !string.IsNullOrEmpty(___m_Thumbnail.spriteName))
            {
                UITextureAtlas.SpriteInfo spriteInfo = ___m_Thumbnail.atlas[___m_Thumbnail.spriteName];
                if (spriteInfo != null)
                {
                    ___m_Thumbnail.size = spriteInfo.pixelSize;
                }
            }
            ___m_BuildingDesc.text = info.GetLocalizedDescriptionShort();
            if ((building2.m_flags & Building.Flags.Collapsed) != 0)
            {
                ___m_RebuildButton.tooltip = ((!IsDisasterServiceRequired(___m_InstanceID)) ? LocaleFormatter.FormatCost(extendedWarehouseAI.GetRelocationCost(), isDistanceBased: false) : Locale.Get("CITYSERVICE_TOOLTIP_DISASTERSERVICEREQUIRED"));
                ___m_RebuildButton.isVisible = Singleton<LoadingManager>.instance.SupportsExpansion(Expansion.NaturalDisasters);
                ___m_RebuildButton.isEnabled = __instance.CanRebuild();
                ___m_ActionPanel.isVisible = false;
            }
            else
            {
                ___m_RebuildButton.isVisible = false;
                ___m_ActionPanel.isVisible = true;
            }
           
            int num = building2.m_customBuffer1 * 100;
            ___m_resourceProgressBar.value = num / extendedWarehouseAI.m_storageCapacity;

            byte transferReason = extendedWarehouseAI.GetTransferReason(___m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building]);
            byte actualTransferReason = extendedWarehouseAI.GetActualTransferReason(___m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building]);

            if (actualTransferReason != 255 && actualTransferReason < 200)
            {
                ___m_resourceProgressBar.progressColor = IndustryWorldInfoPanel.instance.GetResourceColor((TransferManager.TransferReason)actualTransferReason);
                ___m_resourceLabel.text = Locale.Get("WAREHOUSEPANEL_RESOURCE", actualTransferReason.ToString());
                ___m_emptyingOldResource.isVisible = transferReason != actualTransferReason;
                ___m_resourceDescription.isVisible = transferReason != 255;
                ___m_resourceDescription.text = GenerateResourceDescription((TransferManager.TransferReason)transferReason, isForWarehousePanel: true);
                ___m_resourceSprite.spriteName = IndustryWorldInfoPanel.ResourceSpriteName((TransferManager.TransferReason)actualTransferReason);
                string text = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), IndustryWorldInfoPanel.FormatResource((uint)num), IndustryWorldInfoPanel.FormatResourceWithUnit((uint)extendedWarehouseAI.m_storageCapacity, (TransferManager.TransferReason)actualTransferReason));
                ___m_buffer.tooltip = text;
                ___m_capacityLabel.text = text;
            }
            else if (actualTransferReason != 255 && actualTransferReason >= 200)
            {
                byte actual_material_byte = (byte)(actualTransferReason - 200);
                byte material_byte = (byte)(transferReason - 200);
                ___m_resourceProgressBar.progressColor = Color.Lerp(Color.grey, Color.black, 0.2f);
                var extendedTransferReason = (ExtendedTransferManager.TransferReason)actual_material_byte;
                ___m_resourceLabel.text = extendedTransferReason.ToString();
                ___m_emptyingOldResource.isVisible = material_byte != actual_material_byte;
                ___m_resourceDescription.isVisible = material_byte != 255;
                ___m_resourceDescription.text = GenerateExtendedResourceDescription((ExtendedTransferManager.TransferReason)actual_material_byte, isForWarehousePanel: true);
                ___m_resourceSprite.atlas = TextureUtils.GetAtlas("RestaurantAtlas");
                ___m_resourceSprite.spriteName = extendedTransferReason.ToString();
                var FormatResource = IndustryWorldInfoPanel.FormatResource((uint)num);
                var formatResourceWithUnit = FormatResourceWithUnit((uint)extendedWarehouseAI.m_storageCapacity);
                string text = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), FormatResource, formatResourceWithUnit);
                ___m_buffer.tooltip = text;
                ___m_capacityLabel.text = text;
            }
            ___m_Info.text = extendedWarehouseAI.GetLocalizedStats(building, ref instance.m_buildings.m_buffer[building]);
            if (extendedWarehouseAI != null)
            {
                UpdateWorkers(__instance, building, extendedWarehouseAI, ref instance.m_buildings.m_buffer[building], ref ___m_OverWorkSituation, ref ___m_UneducatedPlaces, ref ___m_EducatedPlaces, ref ___m_WellEducatedPlaces, ref ___m_HighlyEducatedPlaces, ref ___m_UneducatedWorkers, ref ___m_EducatedWorkers, ref ___m_WellEducatedWorkers, ref ___m_HighlyEducatedWorkers, ref ___m_JobsAvailLegend, ref ___m_WorkPlacesEducationChart, ref ___m_WorkersEducationChart, ref ___m_workersInfoLabel);
            }

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

        private static void UpdateWorkers(WarehouseWorldInfoPanel __instance, ushort buildingID, ExtendedWarehouseAI extendedWarehouseAI, ref Building building, ref UILabel m_OverWorkSituation, ref UILabel m_UneducatedPlaces, ref UILabel m_EducatedPlaces, ref UILabel m_WellEducatedPlaces, ref UILabel m_HighlyEducatedPlaces, ref UILabel m_UneducatedWorkers, ref UILabel m_EducatedWorkers, ref UILabel m_WellEducatedWorkers, ref UILabel m_HighlyEducatedWorkers, ref UILabel m_JobsAvailLegend, ref UIRadialChart m_WorkPlacesEducationChart, ref UIRadialChart m_WorkersEducationChart, ref UILabel m_workersInfoLabel)
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
