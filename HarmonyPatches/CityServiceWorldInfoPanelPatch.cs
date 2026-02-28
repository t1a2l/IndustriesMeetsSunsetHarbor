using ColossalFramework.Globalization;
using ColossalFramework.UI;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.UI;
using IndustriesMeetsSunsetHarbor.AI;
using ColossalFramework;
using UnityEngine;

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
            if (m_aquacultureFarmAI != null)
            {
                ___m_inputSection.isVisible = false;
                ___m_outputSection.isVisible = true;
                ___m_inputOutputSection.isVisible = true;
                ___m_outputBuffer.progressColor = IndustryWorldInfoPanel.instance.GetResourceColor(m_aquacultureFarmAI.m_outputResource);
                string text = MoreTransferReasons.Utils.AtlasUtils.GetSpriteName(m_aquacultureFarmAI.m_outputResource);
                ___m_outputLabel.text = text;
                ___m_arrow3.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYBUILDING_EXTRACTINGTOOLTIP"), text);
                ___m_outputSprite.atlas = MoreTransferReasons.Utils.TextureUtils.GetAtlas("MoreTransferReasonsAtlas");
                ___m_outputSprite.spriteName = MoreTransferReasons.Utils.AtlasUtils.GetSpriteName(m_aquacultureFarmAI.m_outputResource);
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
                ___m_outputSprite.atlas = MoreTransferReasons.Utils.TextureUtils.GetAtlas("MoreTransferReasonsAtlas");
                ___m_outputSprite.spriteName = MoreTransferReasons.Utils.AtlasUtils.GetSpriteName(m_fishingHarborAI.m_outputResource);
                ___m_ShowIndustryInfoButton.isVisible = false;
            }
            if (m_fishFarmAI != null)
            {
                ___m_inputSection.isVisible = false;
                ___m_outputBuffer.progressColor = Color.white;
                string text4 = Locale.Get("WAREHOUSEPANEL_RESOURCE", m_fishFarmAI.m_outputResource.ToString());
                ___m_outputLabel.text = text4;
                ___m_arrow3.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYBUILDING_EXTRACTINGTOOLTIP"), text4);
                ___m_outputSprite.atlas = MoreTransferReasons.Utils.TextureUtils.GetAtlas("MoreTransferReasonsAtlas");
                ___m_outputSprite.spriteName = MoreTransferReasons.Utils.AtlasUtils.GetSpriteName(m_fishFarmAI.m_outputResource);
                ___m_ShowIndustryInfoButton.isVisible = false;
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
                ItemClass.Service service = info.GetService();
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
            }
        }

        [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "OnVariationDropdownChanged")]
        [HarmonyPrefix]
        public static bool OnVariationDropdownChangedPre(CityServiceWorldInfoPanel __instance, UIComponent component, int value, ref IndustryBuildingAI ___m_IndustryBuildingAI)
        {
            if(___m_IndustryBuildingAI == null)
            {
                return false;
            }
            return true;
        }

    }
}
