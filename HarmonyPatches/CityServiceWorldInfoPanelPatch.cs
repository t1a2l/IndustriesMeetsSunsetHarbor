using ColossalFramework.Globalization;
using ColossalFramework.UI;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.UI;
using IndustriesMeetsSunsetHarbor.AI;
using ColossalFramework;

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
            if (m_fishFarmAI != null)
            {
                ___m_inputSection.isVisible = false;
                ___m_outputBuffer.progressColor = IndustryWorldInfoPanel.instance.GetResourceColor(m_fishFarmAI.m_outputResource);
                string text4 = Locale.Get("WAREHOUSEPANEL_RESOURCE", m_fishFarmAI.m_outputResource.ToString());
                ___m_outputLabel.text = text4;
                ___m_arrow3.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYBUILDING_EXTRACTINGTOOLTIP"), text4);
                ___m_outputSprite.atlas = m_fishFarmAI.m_outputResource == TransferManager.TransferReason.Grain ? UITextures.InGameAtlas : MoreTransferReasons.Utils.TextureUtils.GetAtlas("MoreTransferReasonsAtlas");
                ___m_outputSprite.spriteName = IndustryWorldInfoPanel.ResourceSpriteName(m_fishFarmAI.m_outputResource);
                ___m_ShowIndustryInfoButton.isVisible = false;
            }
            if (AquacultureExtractorPanel._aquacultureExtractorPanel == null)
            {
                AquacultureExtractorPanel.Init();
            }

            AquacultureExtractorPanel.ExtractorDropdownCheck();
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
