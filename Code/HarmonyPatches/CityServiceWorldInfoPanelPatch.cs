using ColossalFramework.Globalization;
using ColossalFramework.UI;
using HarmonyLib;
using System.Reflection;
using IndustriesMeetsSunsetHarbor.UI;
using IndustriesMeetsSunsetHarbor.AI;
using ColossalFramework;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{

    [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "OnSetTarget")]
    public static class CityServiceWorldInfoPanelPatch
    {
        [HarmonyPostfix]
        public static void Postfix(CityServiceWorldInfoPanel __instance, InstanceID ___m_InstanceID)
        {
            var m_outputBuffer = (UIProgressBar)typeof(CityServiceWorldInfoPanel).GetField("m_outputBuffer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_outputLabel = (UILabel)typeof(CityServiceWorldInfoPanel).GetField("m_outputLabel", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_arrow3 = (UISprite)typeof(CityServiceWorldInfoPanel).GetField("m_arrow3", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_outputSprite = (UISprite)typeof(CityServiceWorldInfoPanel).GetField("m_outputSprite", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_ShowIndustryInfoButton = (UIButton)typeof(CityServiceWorldInfoPanel).GetField("m_ShowIndustryInfoButton", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_outputSection = (UIPanel)typeof(CityServiceWorldInfoPanel).GetField("m_outputSection", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_inputOutputSection = (UIPanel)typeof(CityServiceWorldInfoPanel).GetField("m_inputOutputSection", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            ushort building = ___m_InstanceID.Building;
	    Building data = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building];
            AquacultureFarmAI m_aquacultureFarmAI = data.Info.GetAI() as AquacultureFarmAI;
            FishFarmAI m_fishFarmAI = data.Info.GetAI() as FishFarmAI;

            if (m_aquacultureFarmAI != null)
            {
                m_outputSection.isVisible = true;
                m_inputOutputSection.isVisible = true;
                m_outputBuffer.progressColor = IndustryWorldInfoPanel.instance.GetResourceColor(m_aquacultureFarmAI.m_outputResource);
                string text = Locale.Get("WAREHOUSEPANEL_RESOURCE", m_aquacultureFarmAI.m_outputResource.ToString());
                m_outputLabel.text = text;
                m_arrow3.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYBUILDING_EXTRACTINGTOOLTIP"), text);
                m_outputSprite.spriteName = IndustryWorldInfoPanel.ResourceSpriteName(m_aquacultureFarmAI.m_outputResource, false);
                m_ShowIndustryInfoButton.isVisible = false;
                int num = Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building].m_customBuffer2 * 100;
		int storageBufferSize = m_aquacultureFarmAI.GetStorageBufferSize(___m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building]);
		m_outputBuffer.value = IndustryWorldInfoPanel.SafelyNormalize(num, storageBufferSize);
		m_outputSection.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), IndustryWorldInfoPanel.FormatResource((uint)num), IndustryWorldInfoPanel.FormatResourceWithUnit((uint)storageBufferSize, m_aquacultureFarmAI.m_outputResource));

                typeof(CityServiceWorldInfoPanel).GetField("m_outputSection", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_outputSection);
                typeof(CityServiceWorldInfoPanel).GetField("m_outputBuffer", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_outputBuffer);
                typeof(CityServiceWorldInfoPanel).GetField("m_outputLabel", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_outputLabel);
                typeof(CityServiceWorldInfoPanel).GetField("m_arrow3", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_arrow3);
                typeof(CityServiceWorldInfoPanel).GetField("m_outputSprite", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_outputSprite);
                typeof(CityServiceWorldInfoPanel).GetField("m_ShowIndustryInfoButton", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_ShowIndustryInfoButton);
                typeof(CityServiceWorldInfoPanel).GetField("m_inputOutputSection", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_inputOutputSection);
            }

            if(m_fishFarmAI != null && m_fishFarmAI.m_outputResource == TransferManager.TransferReason.Grain)
            {
                m_outputBuffer.progressColor = IndustryWorldInfoPanel.instance.GetResourceColor(m_fishFarmAI.m_outputResource);
		string text4 = Locale.Get("WAREHOUSEPANEL_RESOURCE", m_fishFarmAI.m_outputResource.ToString());
		m_outputLabel.text = text4;
		m_arrow3.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYBUILDING_EXTRACTINGTOOLTIP"), text4);
		m_outputSprite.spriteName = IndustryWorldInfoPanel.ResourceSpriteName(m_fishFarmAI.m_outputResource);
		m_ShowIndustryInfoButton.isVisible = false;
                typeof(CityServiceWorldInfoPanel).GetField("m_outputBuffer", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_outputBuffer);
                typeof(CityServiceWorldInfoPanel).GetField("m_outputLabel", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_outputLabel);
                typeof(CityServiceWorldInfoPanel).GetField("m_arrow3", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_arrow3);
                typeof(CityServiceWorldInfoPanel).GetField("m_outputSprite", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_outputSprite);
                typeof(CityServiceWorldInfoPanel).GetField("m_ShowIndustryInfoButton", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_ShowIndustryInfoButton);
            }

            if(AquacultureExtractorPanel._aquacultureExtractorPanel == null)
            {
                AquacultureExtractorPanel.Init();
            }

            AquacultureExtractorPanel.ExtractorDropdownCheck();
        }

    }
}
