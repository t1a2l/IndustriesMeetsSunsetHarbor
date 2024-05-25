using ColossalFramework.Globalization;
using ColossalFramework.UI;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.UI;
using IndustriesMeetsSunsetHarbor.AI;
using ColossalFramework;
using IndustriesMeetsSunsetHarbor.Managers;
using System.Reflection;
using MoreTransferReasons;
using System.Collections.Generic;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{

    [HarmonyPatch(typeof(CityServiceWorldInfoPanel))]
    public static class CityServiceWorldInfoPanelPatch
    {
        private delegate void BuildingWorldInfoPanelUpdateBindingsDelegate(BuildingWorldInfoPanel instance);
        private static BuildingWorldInfoPanelUpdateBindingsDelegate BaseUpdateBindings = AccessTools.MethodDelegate<BuildingWorldInfoPanelUpdateBindingsDelegate>(typeof(BuildingWorldInfoPanel).GetMethod("UpdateBindings", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "UpdateBindings")]
        [HarmonyPrefix]
        public static bool PreUpdateBindings(CityServiceWorldInfoPanel __instance, ref InstanceID ___m_InstanceID, ref UISprite ___m_BuildingService, ref UIProgressBar ___m_outputBuffer, ref UIPanel ___m_outputSection)
	{
            BaseUpdateBindings(__instance);
	    if (Singleton<BuildingManager>.exists && ___m_InstanceID.Type == InstanceType.Building && ___m_InstanceID.Building != 0)
	    {
		ushort buildingId = ___m_InstanceID.Building;
		BuildingManager instance = Singleton<BuildingManager>.instance;
		Building building = instance.m_buildings.m_buffer[buildingId];
		BuildingInfo info = building.Info;
		BuildingAI buildingAI = info.m_buildingAI;
                if (buildingAI is NewExtractingFacilityAI newExtractingFacilityAI)
	        {
                    ItemClass.Service service = info.GetService();
                    string text = ColossalFramework.Utils.GetNameByValue(service, "Game");
                    ___m_BuildingService.spriteName = "UIFilterExtractorBuildings";
		    ___m_BuildingService.tooltip = Locale.Get("MAIN_TOOL", text);
                    var custom_buffers = CustomBuffersManager.GetCustomBuffer(buildingId);

                    if(newExtractingFacilityAI.m_outputResource1 != TransferManager.TransferReason.None)
                    {
                        var customBuffer = custom_buffers.m_customBuffer2;
                        int outputBufferSize = newExtractingFacilityAI.GetOutputBufferSize(ref building);
                        ___m_outputBuffer.value = IndustryWorldInfoPanel.SafelyNormalize(customBuffer, outputBufferSize);
                        ___m_outputSection.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), IndustryWorldInfoPanel.FormatResource((uint)customBuffer), IndustryWorldInfoPanel.FormatResourceWithUnit((uint)outputBufferSize, newExtractingFacilityAI.m_outputResource1));
                    }
                    if (newExtractingFacilityAI.m_outputResource2 != ExtendedTransferManager.TransferReason.None)
                    {
                        var customBuffer = custom_buffers.m_customBuffer2;
                        int outputBufferSize = newExtractingFacilityAI.GetOutputBufferSize(ref building);
                        ___m_outputBuffer.value = IndustryWorldInfoPanel.SafelyNormalize(customBuffer, outputBufferSize);
                        ___m_outputSection.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), IndustryBuildingManager.FormatResource((uint)customBuffer), IndustryBuildingManager.FormatExtendedResourceWithUnit((uint)outputBufferSize, newExtractingFacilityAI.m_outputResource2));
                    }

                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "OnSetTarget")]
        [HarmonyPostfix]
        public static void PostSetTarget(CityServiceWorldInfoPanel __instance, ref InstanceID ___m_InstanceID, ref UIProgressBar ___m_outputBuffer, ref UILabel ___m_outputLabel, ref UISprite ___m_arrow3, ref UISprite ___m_outputSprite, ref UIButton ___m_ShowIndustryInfoButton, ref UIPanel ___m_outputSection, ref UIPanel ___m_inputOutputSection, ref UIPanel ___m_inputSection, ref UIPanel ___m_VariationPanel, ref UIDropDown ___m_VariationDropdown)
        {
            ushort building = ___m_InstanceID.Building;
	    Building data = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building];
            AquacultureFarmAI m_aquacultureFarmAI = data.Info.GetAI() as AquacultureFarmAI;
            ExtendedFishingHarborAI m_extendedFishingHarborAI = data.Info.GetAI() as ExtendedFishingHarborAI;
            ExtendedFishFarmAI m_extendedFishFarmAI = data.Info.GetAI() as ExtendedFishFarmAI;
            FishFarmAI m_fishFarmAI = data.Info.GetAI() as FishFarmAI;
            NewExtractingFacilityAI newExtractingFacilityAI = data.Info.GetAI() as NewExtractingFacilityAI;
            if (m_aquacultureFarmAI != null)
            {
                ___m_inputSection.isVisible = false;
                ___m_outputSection.isVisible = true;
                ___m_inputOutputSection.isVisible = true;
                ___m_outputBuffer.progressColor = IndustryBuildingManager.GetExtendedResourceColor(m_aquacultureFarmAI.m_outputResource);
                string text = Locale.Get("WAREHOUSEPANEL_RESOURCE", m_aquacultureFarmAI.m_outputResource.ToString());
                ___m_outputLabel.text = text;
                ___m_arrow3.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYBUILDING_EXTRACTINGTOOLTIP"), text);
                ___m_outputSprite.spriteName = IndustryBuildingManager.ResourceSpriteName(m_aquacultureFarmAI.m_outputResource);
                ___m_ShowIndustryInfoButton.isVisible = false;
                int num = Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building].m_customBuffer2 * 100;
		int storageBufferSize = m_aquacultureFarmAI.GetStorageBufferSize(___m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building]);
		___m_outputBuffer.value = IndustryWorldInfoPanel.SafelyNormalize(num, storageBufferSize);
		___m_outputSection.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), IndustryBuildingManager.FormatResource((uint)num), IndustryBuildingManager.FormatExtendedResourceWithUnit((uint)storageBufferSize, m_aquacultureFarmAI.m_outputResource));
            }
            if (m_extendedFishingHarborAI != null)
            {
                ___m_inputSection.isVisible = false;
                ___m_outputSection.isVisible = true;
                ___m_inputOutputSection.isVisible = true;
                ___m_outputBuffer.progressColor = IndustryBuildingManager.GetExtendedResourceColor(m_extendedFishingHarborAI.m_outputResource);
                string text = m_extendedFishingHarborAI.m_outputResource.ToString();
                ___m_outputLabel.text = text;
                ___m_arrow3.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYBUILDING_EXTRACTINGTOOLTIP"), text);
                ___m_outputSprite.atlas = MoreTransferReasons.Utils.TextureUtils.GetAtlas("MoreTransferReasonsAtlas");
                ___m_outputSprite.spriteName = IndustryBuildingManager.ResourceSpriteName(m_extendedFishingHarborAI.m_outputResource);
                ___m_ShowIndustryInfoButton.isVisible = false;
                int num = Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building].m_customBuffer2 * 100;
                int storageBufferSize = m_extendedFishingHarborAI.m_storageBufferSize;
                ___m_outputBuffer.value = IndustryWorldInfoPanel.SafelyNormalize(num, storageBufferSize);
                ___m_outputSection.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), IndustryBuildingManager.FormatResource((uint)num), IndustryBuildingManager.FormatExtendedResourceWithUnit((uint)storageBufferSize, m_extendedFishingHarborAI.m_outputResource));
            }

            if (m_extendedFishFarmAI != null)
            {
                ___m_inputSection.isVisible = false;
                ___m_outputSection.isVisible = true;
                ___m_inputOutputSection.isVisible = true;
                ___m_outputBuffer.progressColor = IndustryBuildingManager.GetExtendedResourceColor(m_extendedFishFarmAI.m_outputResource);
                string text = m_extendedFishFarmAI.m_outputResource.ToString();
                ___m_outputLabel.text = text;
		___m_arrow3.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYBUILDING_EXTRACTINGTOOLTIP"), text);
                ___m_outputSprite.atlas = MoreTransferReasons.Utils.TextureUtils.GetAtlas("MoreTransferReasonsAtlas");
                ___m_outputSprite.spriteName = IndustryBuildingManager.ResourceSpriteName(m_extendedFishFarmAI.m_outputResource);
		___m_ShowIndustryInfoButton.isVisible = false;
            }

            if (newExtractingFacilityAI != null)
            {
                ___m_inputSection.isVisible = false;
                ___m_outputSection.isVisible = true;
                ___m_inputOutputSection.isVisible = true;
                if (newExtractingFacilityAI.m_outputResource1 != TransferManager.TransferReason.None)
                {
                    ___m_outputBuffer.progressColor = IndustryWorldInfoPanel.instance.GetResourceColor(newExtractingFacilityAI.m_outputResource1);
                    string text = Locale.Get("WAREHOUSEPANEL_RESOURCE", newExtractingFacilityAI.m_outputResource1.ToString());
                    ___m_outputLabel.text = text;
                    ___m_arrow3.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYBUILDING_EXTRACTINGTOOLTIP"), text);
                    ___m_outputSprite.atlas = UITextures.InGameAtlas;
                    ___m_outputSprite.spriteName = IndustryWorldInfoPanel.ResourceSpriteName(newExtractingFacilityAI.m_outputResource1);
                    ___m_ShowIndustryInfoButton.isVisible = true;
                }
                if (newExtractingFacilityAI.m_outputResource2 != ExtendedTransferManager.TransferReason.None)
                {
                    ___m_outputBuffer.progressColor = IndustryBuildingManager.GetExtendedResourceColor(newExtractingFacilityAI.m_outputResource2);
                    string text = newExtractingFacilityAI.m_outputResource2.ToString();
                    ___m_outputLabel.text = text;
                    ___m_arrow3.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYBUILDING_EXTRACTINGTOOLTIP"), text);
                    ___m_outputSprite.atlas = MoreTransferReasons.Utils.TextureUtils.GetAtlas("MoreTransferReasonsAtlas");
                    ___m_outputSprite.spriteName = IndustryBuildingManager.ResourceSpriteName(newExtractingFacilityAI.m_outputResource2);
                    ___m_ShowIndustryInfoButton.isVisible = true;
                }

                if (newExtractingFacilityAI.GetVariations(out var variations) && variations.m_size > 1)
                {
                    ___m_VariationPanel.isVisible = true;
                    List<string> list = [];
                    int selectedIndex = -1;
                    for (int i = 0; i < variations.m_size; i++)
                    {
                        string id = "FIELDVARIATION" + "_" + Singleton<SimulationManager>.instance.m_metaData.m_environment.ToUpper();
                        string empty = (Locale.Exists(id, variations.m_buffer[i].m_info.name) ? Locale.Get(id, variations.m_buffer[i].m_info.name) : ((!Locale.Exists("FIELDVARIATION", variations.m_buffer[i].m_info.name)) ? variations.m_buffer[i].m_info.GetUncheckedLocalizedTitle() : Locale.Get("FIELDVARIATION", variations.m_buffer[i].m_info.name)));
                        list.Add(empty);
                        if (newExtractingFacilityAI.m_info.name == variations.m_buffer[i].m_info.name)
                        {
                            selectedIndex = i;
                        }
                    }
                    ___m_VariationDropdown.items = list.ToArray();
                    ___m_VariationDropdown.selectedIndex = selectedIndex;
                }
            }

            if (m_fishFarmAI != null && m_fishFarmAI.m_outputResource == TransferManager.TransferReason.Grain)
            {
                ___m_inputSection.isVisible = false;
                ___m_outputBuffer.progressColor = IndustryWorldInfoPanel.instance.GetResourceColor(m_fishFarmAI.m_outputResource);
                string text4 = Locale.Get("WAREHOUSEPANEL_RESOURCE", m_fishFarmAI.m_outputResource.ToString());
                ___m_outputLabel.text = text4;
                ___m_arrow3.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYBUILDING_EXTRACTINGTOOLTIP"), text4);
                ___m_outputSprite.spriteName = IndustryWorldInfoPanel.ResourceSpriteName(m_fishFarmAI.m_outputResource);
                ___m_ShowIndustryInfoButton.isVisible = false;
            }
            if (data.Info.GetAI() is FishingHarborAI)
            {
                ___m_outputSprite.atlas = UITextures.InGameAtlas;
            }
            if (AquacultureExtractorPanel._aquacultureExtractorPanel == null)
            {
                AquacultureExtractorPanel.Init();
            }

            AquacultureExtractorPanel.ExtractorDropdownCheck();
        }

        [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "OnVariationDropdownChanged")]
        [HarmonyPrefix]
        public static bool OnVariationDropdownChanged(CityServiceWorldInfoPanel __instance, UIComponent component, int value, ref IndustryBuildingAI ___m_IndustryBuildingAI)
        {
            if(___m_IndustryBuildingAI == null)
            {
                return false;
            }
            return true;
        }
    }
}
