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
        public static void Postfix(CityServiceWorldInfoPanel __instance, Building data)
        {
            var m_outputBuffer = (UIProgressBar)typeof(CityServiceWorldInfoPanel).GetField("m_outputBuffer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_outputLabel = (UILabel)typeof(CityServiceWorldInfoPanel).GetField("m_outputLabel", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_arrow3 = (UISprite)typeof(CityServiceWorldInfoPanel).GetField("m_arrow3", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_outputSprite = (UISprite)typeof(CityServiceWorldInfoPanel).GetField("m_outputSprite", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_ShowIndustryInfoButton = (UIButton)typeof(CityServiceWorldInfoPanel).GetField("m_ShowIndustryInfoButton", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            AquacultureFarmAI m_aquacultureFarmAI = data.Info.GetAI() as AquacultureFarmAI;

            if (m_aquacultureFarmAI != null)
            {
                m_outputBuffer.progressColor = IndustryWorldInfoPanel.instance.GetResourceColor(m_aquacultureFarmAI.m_outputResource);
                string text = Locale.Get("WAREHOUSEPANEL_RESOURCE", m_aquacultureFarmAI.m_outputResource.ToString());
                m_outputLabel.text = text;
                m_arrow3.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYBUILDING_EXTRACTINGTOOLTIP"), text);
                m_outputSprite.spriteName = IndustryWorldInfoPanel.ResourceSpriteName(m_aquacultureFarmAI.m_outputResource, false);
                m_ShowIndustryInfoButton.isVisible = false;
            }

            if(AquacultureExtractorPanel._aquacultureExtractorPanel == null)
            {
                AquacultureExtractorPanel.Init();
            }

            AquacultureExtractorPanel.ExtractorDropdownCheck();

        }

    }
}
