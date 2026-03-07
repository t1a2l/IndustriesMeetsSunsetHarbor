using ColossalFramework.Globalization;
using ColossalFramework.UI;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.UI;
using IndustriesMeetsSunsetHarbor.AI;
using ColossalFramework;
using UnityEngine;
using MoreTransferReasons.Utils;
using MoreTransferReasons;
using ColossalFramework.Threading;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(CityServiceWorldInfoPanel))]
    public static class CityServiceWorldInfoPanelPatch
    {
        [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "OnSetTarget")]
        [HarmonyPostfix]
        public static void PostSetTarget(CityServiceWorldInfoPanel __instance, ref InstanceID ___m_InstanceID, ref UIProgressBar ___m_outputBuffer, ref UILabel ___m_outputLabel, ref UISprite ___m_arrow3, ref UISprite ___m_outputSprite, ref UIButton ___m_ShowIndustryInfoButton, ref UIPanel ___m_outputSection, ref UIPanel ___m_inputOutputSection, ref UIPanel ___m_inputSection, ref UIPanel ___m_VariationPanel, ref UIDropDown ___m_VariationDropdown)
        {
            ushort building = ___m_InstanceID.Building;
	    Building data = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building];
            AquacultureFarmAI m_aquacultureFarmAI = data.Info.GetAI() as AquacultureFarmAI;
            FishingHarborAI m_fishingHarborAI = data.Info.GetAI() as FishingHarborAI;
            FishFarmAI m_fishFarmAI = data.Info.GetAI() as FishFarmAI;
            ExtractingFacilityAI m_extractingFacilityAI = data.Info.GetAI() as ExtractingFacilityAI;
            if (m_aquacultureFarmAI != null)
            {
                ___m_inputSection.isVisible = false;
                ___m_outputSection.isVisible = true;
                ___m_inputOutputSection.isVisible = true;
                ___m_outputBuffer.progressColor = IndustryWorldInfoPanel.instance.GetResourceColor(m_aquacultureFarmAI.m_outputResource);
                string text = AtlasUtils.GetSpriteName(m_aquacultureFarmAI.m_outputResource);
                ___m_outputLabel.text = text;
                ___m_arrow3.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYBUILDING_EXTRACTINGTOOLTIP"), text);
                ___m_outputSprite.atlas = AtlasUtils.GetResourceAtlas(m_aquacultureFarmAI.m_outputResource);
                ___m_outputSprite.spriteName = AtlasUtils.GetSpriteName(m_aquacultureFarmAI.m_outputResource);
                ___m_ShowIndustryInfoButton.isVisible = false;
                int num = Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building].m_customBuffer2 * 100;
		int storageBufferSize = m_aquacultureFarmAI.GetStorageBufferSize(___m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building]);
		___m_outputBuffer.value = IndustryWorldInfoPanel.SafelyNormalize(num, storageBufferSize);
		___m_outputSection.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), IndustryWorldInfoPanel.FormatResource((uint)num), IndustryWorldInfoPanel.FormatResourceWithUnit((uint)storageBufferSize, m_aquacultureFarmAI.m_outputResource));
            }
            if (m_fishingHarborAI != null)
            {
                ___m_inputSection.isVisible = false;
                ___m_outputBuffer.progressColor = Color.white;
                string text4 = Locale.Get("WAREHOUSEPANEL_RESOURCE", m_fishingHarborAI.m_outputResource.ToString());
                ___m_outputLabel.text = text4;
                ___m_arrow3.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYBUILDING_EXTRACTINGTOOLTIP"), text4);
                ___m_outputSprite.atlas = AtlasUtils.GetResourceAtlas(m_fishingHarborAI.m_outputResource);
                ___m_outputSprite.spriteName = AtlasUtils.GetSpriteName(m_fishingHarborAI.m_outputResource);
                ___m_ShowIndustryInfoButton.isVisible = false;
            }
            if (m_fishFarmAI != null)
            {
                ___m_inputSection.isVisible = false;
                ___m_outputBuffer.progressColor = Color.white;
                string text4 = Locale.Get("WAREHOUSEPANEL_RESOURCE", m_fishFarmAI.m_outputResource.ToString());
                ___m_outputLabel.text = text4;
                ___m_arrow3.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYBUILDING_EXTRACTINGTOOLTIP"), text4);
                ___m_outputSprite.atlas = AtlasUtils.GetResourceAtlas(m_fishFarmAI.m_outputResource);
                ___m_outputSprite.spriteName = AtlasUtils.GetSpriteName(m_fishFarmAI.m_outputResource);
                ___m_ShowIndustryInfoButton.isVisible = false;
            }
            if (m_extractingFacilityAI != null)
            {
                if(data.Info.m_class.m_subService == ItemClass.SubService.PlayerIndustryFarming)
                {
                    ___m_inputSection.isVisible = false;
                    ___m_outputBuffer.progressColor = Color.white;
                    string text4 = Locale.Get("WAREHOUSEPANEL_RESOURCE", m_extractingFacilityAI.m_outputResource.ToString());
                    ___m_outputLabel.text = text4;
                    ___m_arrow3.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYBUILDING_EXTRACTINGTOOLTIP"), text4);
                    ___m_outputSprite.atlas = AtlasUtils.GetResourceAtlas(m_extractingFacilityAI.m_outputResource);
                    ___m_outputSprite.spriteName = AtlasUtils.GetSpriteName(m_extractingFacilityAI.m_outputResource);
                    ___m_ShowIndustryInfoButton.isVisible = true;
                }
                else
                {
                    ___m_outputSprite.atlas = AtlasUtils.GetResourceAtlas(m_extractingFacilityAI.m_outputResource);
                }
            }
            if (AquacultureExtractorPanel._aquacultureExtractorPanel == null)
            {
                AquacultureExtractorPanel.Init();
            }

            AquacultureExtractorPanel.ExtractorDropdownCheck();
        }

        [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "UpdateBindings")]
        [HarmonyPostfix]
        public static void UpdateBindings(CityServiceWorldInfoPanel __instance, ref InstanceID ___m_InstanceID, ref UISprite ___m_BuildingService, ref UIProgressBar ___m_outputBuffer, ref UIPanel ___m_outputSection)
        {
            if (Singleton<BuildingManager>.exists && ___m_InstanceID.Type == InstanceType.Building && ___m_InstanceID.Building != 0)
            {
                ushort building = ___m_InstanceID.Building;
                BuildingManager instance = Singleton<BuildingManager>.instance;
                Building building2 = instance.m_buildings.m_buffer[building];
                BuildingInfo info = building2.Info;
                BuildingAI buildingAI = info.m_buildingAI;
                AquacultureFarmAI m_aquacultureFarmAI = buildingAI as AquacultureFarmAI;
                FishingHarborAI m_fishingHarborAI = buildingAI as FishingHarborAI;
                FishFarmAI m_fishFarmAI = buildingAI as FishFarmAI;
                ExtractingFacilityAI m_extractingFacilityAI = buildingAI as ExtractingFacilityAI;
                ItemClass.Service service = info.GetService();
                ItemClass.SubService sub_service = info.GetSubService();
                if (service != ItemClass.Service.None)
                {
                    string text = ColossalFramework.Utils.GetNameByValue(service, "Game");
                    string spriteName = "ToolbarIcon" + text;
                    string id = "MAIN_TOOL";
                    if (service == ItemClass.Service.Fishing)
                    {
                        spriteName = ((!(m_fishFarmAI != null || m_aquacultureFarmAI != null) && !(m_fishingHarborAI != null)) ? "InfoIconFishing" : "UIFilterFishingExtractorBuildings");
                    }
                    ___m_BuildingService.spriteName = spriteName;
                    ___m_BuildingService.tooltip = Locale.Get(id, text);
                }
                if (m_aquacultureFarmAI != null)
                {
                    int num = Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building].m_customBuffer2 * 100;
                    int storageBufferSize = m_aquacultureFarmAI.GetStorageBufferSize(___m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building]);
                    ___m_outputBuffer.value = IndustryWorldInfoPanel.SafelyNormalize(num, storageBufferSize);
                    ___m_outputSection.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), IndustryWorldInfoPanel.FormatResource((uint)num), IndustryWorldInfoPanel.FormatResourceWithUnit((uint)storageBufferSize, m_aquacultureFarmAI.m_outputResource));
                }
                else if (m_fishFarmAI != null)
                {
                    int num2 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building].m_customBuffer2 * 100;
                    int storageBufferSize2 = m_fishFarmAI.GetStorageBufferSize(___m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building]);
                    ___m_outputBuffer.value = IndustryWorldInfoPanel.SafelyNormalize(num2, storageBufferSize2);
                    ___m_outputSection.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), IndustryWorldInfoPanel.FormatResource((uint)num2), IndustryWorldInfoPanel.FormatResourceWithUnit((uint)storageBufferSize2, m_fishFarmAI.m_outputResource));
                }
                else if (m_fishingHarborAI != null)
                {
                    int num3 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building].m_customBuffer2 * 100;
                    int storageBufferSize3 = m_fishingHarborAI.m_storageBufferSize;
                    ___m_outputBuffer.value = IndustryWorldInfoPanel.SafelyNormalize(num3, storageBufferSize3);
                    ___m_outputSection.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), IndustryWorldInfoPanel.FormatResource((uint)num3), IndustryWorldInfoPanel.FormatResourceWithUnit((uint)storageBufferSize3, m_fishingHarborAI.m_outputResource));
                }
                else if (m_extractingFacilityAI != null && sub_service == ItemClass.SubService.PlayerIndustryFarming)
                {
                    int num4 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building].m_customBuffer2 * 100;
                    int storageBufferSize4 = m_extractingFacilityAI.GetOutputBufferSize(___m_InstanceID.Building, ref building2);
                    ___m_outputBuffer.value = IndustryWorldInfoPanel.SafelyNormalize(num4, storageBufferSize4);
                    ___m_outputSection.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), IndustryWorldInfoPanel.FormatResource((uint)num4), IndustryWorldInfoPanel.FormatResourceWithUnit((uint)storageBufferSize4, m_extractingFacilityAI.m_outputResource));
                }
            }
        }

        [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "OnVariationDropdownChanged")]
        [HarmonyPrefix]
        public static bool OnVariationDropdownChangedPre(CityServiceWorldInfoPanel __instance, UIComponent component, int value, ref InstanceID ___m_InstanceID, ref IndustryBuildingAI ___m_IndustryBuildingAI, ref UIDropDown ___m_VariationDropdown, ref UITextField ___m_NameField)
        {
            var m_InstanceID = ___m_InstanceID;
            int index = ___m_VariationDropdown.selectedIndex;
            string nameField = ___m_NameField.text;
            var selectedValue = ___m_VariationDropdown.items[value];
            BuildingManager instance = Singleton<BuildingManager>.instance;
            BuildingInfo info = instance.m_buildings.m_buffer[m_InstanceID.Building].Info;
            BuildingAI buildingAI = instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI;
            if (buildingAI is ExtractingFacilityAI && info.m_class.m_subService == ItemClass.SubService.PlayerIndustryFarming)
            {
                if (!___m_IndustryBuildingAI.GetVariations(out var variations))
                {
                    return false;
                }
                Singleton<SimulationManager>.instance.AddAction(delegate
                {
                    if (Singleton<BuildingManager>.exists)
                    {
                        ref Building building = ref instance.m_buildings.m_buffer[m_InstanceID.Building];
                        ExtractingFacilityAI m_extractingFacilityAI = building.Info.m_buildingAI as ExtractingFacilityAI;
                        if (m_extractingFacilityAI != null && building.Info.m_class.m_subService == ItemClass.SubService.PlayerIndustryFarming)
                        {
                            CropFieldVariationChanged(ref building, m_extractingFacilityAI, selectedValue);
                        }
                        Singleton<BuildingManager>.instance.UpdateBuildingInfo(m_InstanceID.Building, variations[index].m_info);
                        ThreadHelper.dispatcher.Dispatch(delegate
                        {
                            nameField = GetName(m_InstanceID);
                        });
                        IndustryBuildingAI industryBuildingAI = (IndustryBuildingAI)Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info.m_buildingAI;
                        industryBuildingAI.SetLastVariationIndex(value);
                    }
                });
                return false;
            }
            return true;
        }

        private static void CropFieldVariationChanged(ref Building building, ExtractingFacilityAI m_extractingFacilityAI, string selectedValue)
        {
            var oldOutputResource = m_extractingFacilityAI.m_outputResource;
            var outputResource = TransferManager.TransferReason.None; 
            if (selectedValue.Contains("Corn") || selectedValue.Contains("Potato") || selectedValue.Contains("Green House"))
            {
                outputResource = ExtendedTransferManager.Vegetables;
            }
            else if (selectedValue.Contains("Cotton"))
            {
                outputResource = ExtendedTransferManager.Cotton;
            }
            else if (selectedValue.Contains("Wheat"))
            {
                outputResource = TransferManager.TransferReason.Grain;
            }
            if (outputResource != oldOutputResource)
            {
                building.m_customBuffer1 = 0;
            }
            m_extractingFacilityAI.m_outputResource = outputResource;
        }

        private static string GetName(InstanceID m_InstanceID)
        {
            if (m_InstanceID.Type == InstanceType.Building && m_InstanceID.Building != 0)
            {
                return Singleton<BuildingManager>.instance.GetBuildingName(m_InstanceID.Building, InstanceID.Empty);
            }
            return string.Empty;
        }

    }
}
