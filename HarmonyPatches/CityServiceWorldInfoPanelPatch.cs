using ColossalFramework.Globalization;
using ColossalFramework.UI;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.UI;
using IndustriesMeetsSunsetHarbor.AI;
using ColossalFramework;
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
        public static void PostSetTarget(CityServiceWorldInfoPanel __instance, ref InstanceID ___m_InstanceID, ref UIProgressBar ___m_outputBuffer, ref UILabel ___m_outputLabel, ref UISprite ___m_arrow3, ref UISprite ___m_outputSprite, ref UIButton ___m_ShowIndustryInfoButton, ref UIPanel ___m_outputSection, ref UIPanel ___m_inputOutputSection, ref UIPanel ___m_inputSection)
        {
            ushort building = ___m_InstanceID.Building;
	    Building data = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building];
            AquacultureFarmAI m_aquacultureFarmAI = data.Info.GetAI() as AquacultureFarmAI;
            FishingHarborAI m_fishingHarborAI = data.Info.GetAI() as FishingHarborAI;
            FishFarmAI m_fishFarmAI = data.Info.GetAI() as FishFarmAI;
            ExtractingFacilityAI m_extractingFacilityAI = data.Info.GetAI() as ExtractingFacilityAI;
            TransferManager.TransferReason outputResource = TransferManager.TransferReason.None;
            ___m_ShowIndustryInfoButton.isVisible = false;
            if (m_aquacultureFarmAI != null || m_fishingHarborAI != null || m_fishFarmAI != null)
            {
                ___m_inputSection.isVisible = false;
                ___m_outputSection.isVisible = true;
                ___m_inputOutputSection.isVisible = true;
                if (m_aquacultureFarmAI != null)
                {
                    outputResource = m_aquacultureFarmAI.m_outputResource;
                }
                if (m_fishingHarborAI != null)
                {
                    outputResource = m_fishingHarborAI.m_outputResource;
                }
                if (m_fishFarmAI != null)
                {
                    outputResource = m_fishFarmAI.m_outputResource;
                }
                ___m_outputBuffer.progressColor = IndustryWorldInfoPanel.instance.GetResourceColor(TransferManager.TransferReason.Fish);
                string text = Locale.Get("WAREHOUSEPANEL_RESOURCE", outputResource.ToString());
                ___m_outputLabel.text = text;
                ___m_arrow3.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYBUILDING_EXTRACTINGTOOLTIP"), text);
                ___m_outputSprite.atlas = AtlasUtils.GetResourceAtlas(outputResource);
                ___m_outputSprite.spriteName = AtlasUtils.GetSpriteName(outputResource);
            }
            if (m_extractingFacilityAI != null)
            {
                ___m_outputBuffer.progressColor = IndustryWorldInfoPanel.instance.GetResourceColor(TransferManager.TransferReason.Grain);
                ___m_inputSection.isVisible = false;
                ___m_outputSection.isVisible = true;
                ___m_ShowIndustryInfoButton.isVisible = true;
            }
            if (AquacultureExtractorPanel._aquacultureExtractorPanel == null)
            {
                AquacultureExtractorPanel.Init();
            }
            AquacultureExtractorPanel.ExtractorDropdownCheck();
        }

        [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "UpdateBindings")]
        [HarmonyPostfix]
        public static void UpdateBindings(CityServiceWorldInfoPanel __instance, ref InstanceID ___m_InstanceID, ref UISprite ___m_BuildingService, ref UIProgressBar ___m_outputBuffer, ref UIPanel ___m_outputSection, ref UILabel ___m_outputLabel, ref UISprite ___m_arrow3, ref UISprite ___m_outputSprite)
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
                if (m_aquacultureFarmAI != null || m_fishingHarborAI != null || m_fishFarmAI != null || m_extractingFacilityAI != null)
                {
                    int storageBufferSize = 0;
                    int num = building2.m_customBuffer2 * 100;
                    TransferManager.TransferReason outputResource = TransferManager.TransferReason.None;
                    if (m_aquacultureFarmAI != null)
                    {
                        outputResource = m_aquacultureFarmAI.m_outputResource;
                        storageBufferSize = m_aquacultureFarmAI.GetStorageBufferSize(___m_InstanceID.Building, ref building2);
                    }
                    if (m_fishingHarborAI != null)
                    {
                        outputResource = m_fishingHarborAI.m_outputResource;
                        storageBufferSize = m_fishingHarborAI.m_storageBufferSize;
                    }
                    if (m_fishFarmAI != null)
                    {
                        outputResource = m_fishFarmAI.m_outputResource;
                        storageBufferSize = m_fishFarmAI.GetStorageBufferSize(___m_InstanceID.Building, ref building2);
                    }
                    if (m_extractingFacilityAI != null)
                    {
                        outputResource = m_extractingFacilityAI.m_outputResource;
                        storageBufferSize = m_extractingFacilityAI.GetOutputBufferSize(___m_InstanceID.Building, ref building2);
                        if (sub_service == ItemClass.SubService.PlayerIndustryFarming)
                        {
                            string text4 = Locale.Get("WAREHOUSEPANEL_RESOURCE", m_extractingFacilityAI.m_outputResource.ToString());
                            ___m_outputLabel.text = text4;
                            ___m_arrow3.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYBUILDING_EXTRACTINGTOOLTIP"), text4);
                            ___m_outputSprite.atlas = AtlasUtils.GetResourceAtlas(m_extractingFacilityAI.m_outputResource);
                            ___m_outputSprite.spriteName = AtlasUtils.GetSpriteName(m_extractingFacilityAI.m_outputResource);
                        }
                        else
                        {
                            ___m_outputSprite.atlas = AtlasUtils.GetResourceAtlas(m_extractingFacilityAI.m_outputResource);
                        }
                    }
                    ___m_outputBuffer.value = IndustryWorldInfoPanel.SafelyNormalize(num, storageBufferSize);
                    ___m_outputSection.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), IndustryWorldInfoPanel.FormatResource((uint)num), IndustryWorldInfoPanel.FormatResourceWithUnit((uint)storageBufferSize, outputResource));
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
