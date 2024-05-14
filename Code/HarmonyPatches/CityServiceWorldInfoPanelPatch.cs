using ColossalFramework.Globalization;
using ColossalFramework.UI;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.UI;
using IndustriesMeetsSunsetHarbor.AI;
using ColossalFramework;
using IndustriesMeetsSunsetHarbor.Managers;
using IndustriesMeetsSunsetHarbor.Code.AI;
using IndustriesMeetsSunsetHarbor.Utils;
using System.Reflection;
using MoreTransferReasons;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{

    [HarmonyPatch(typeof(CityServiceWorldInfoPanel))]
    public static class CityServiceWorldInfoPanelPatch
    {
        private delegate void BuildingWorldInfoPanelOnSetTargetDelegate(BuildingWorldInfoPanel instance);
        private static BuildingWorldInfoPanelOnSetTargetDelegate BaseOnSetTarget = AccessTools.MethodDelegate<BuildingWorldInfoPanelOnSetTargetDelegate>(typeof(BuildingWorldInfoPanel).GetMethod("OnSetTarget", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate void BuildingWorldInfoPanelUpdateBindingsDelegate(BuildingWorldInfoPanel instance);
        private static BuildingWorldInfoPanelUpdateBindingsDelegate BaseUpdateBindings = AccessTools.MethodDelegate<BuildingWorldInfoPanelUpdateBindingsDelegate>(typeof(BuildingWorldInfoPanel).GetMethod("UpdateBindings", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "OnSetTarget")]
        [HarmonyPrefix]
        public static bool PreSetTarget(CityServiceWorldInfoPanel __instance, ref bool ___m_needResetTarget, ref InstanceID ___m_InstanceID, ref UIProgressBar ___m_outputBuffer, ref UILabel ___m_outputLabel, ref UISprite ___m_arrow3, ref UISprite ___m_outputSprite, ref UIButton ___m_ShowIndustryInfoButton)
	{
	    ___m_needResetTarget = false;
	    BaseOnSetTarget(__instance);
	    if (___m_InstanceID.Type != InstanceType.Building || ___m_InstanceID.Building == 0)
	    {
		return false;
	    }
	    ushort building = ___m_InstanceID.Building;
	    Building data = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building];
            NewExtractingFacilityAI newExtractingFacilityAI = data.Info.GetAI() as NewExtractingFacilityAI;
            if (newExtractingFacilityAI != null)
	    {
		___m_outputBuffer.progressColor = IndustryBuildingManager.GetExtendedResourceColor(newExtractingFacilityAI.m_outputResource);
                string text = newExtractingFacilityAI.m_outputResource.ToString();
                ___m_outputLabel.text = text;
		___m_arrow3.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYBUILDING_EXTRACTINGTOOLTIP"), text);
                ___m_outputSprite.atlas = TextureUtils.GetAtlas("IndustriesAtlas");
		___m_outputSprite.spriteName = IndustryBuildingManager.ResourceSpriteName(newExtractingFacilityAI.m_outputResource);
		___m_ShowIndustryInfoButton.isVisible = true;
                return false;
	    }
            return true;
        }

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
		    var customBuffer = custom_buffers.m_customBuffer2;
		    int outputBufferSize = newExtractingFacilityAI.GetOutputBufferSize(ref building);
		    ___m_outputBuffer.value = IndustryWorldInfoPanel.SafelyNormalize(customBuffer, outputBufferSize);
		    ___m_outputSection.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYPANEL_BUFFERTOOLTIP"), IndustryBuildingManager.FormatResource((uint)customBuffer), IndustryBuildingManager.FormatExtendedResourceWithUnit((uint)outputBufferSize, newExtractingFacilityAI.m_outputResource));
                    return false;
                }
                return true;
            }
            return true;
        }

        [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "OnSetTarget")]
        [HarmonyPostfix]
        public static void PostSetTarget(CityServiceWorldInfoPanel __instance, ref InstanceID ___m_InstanceID, ref UIProgressBar ___m_outputBuffer, ref UILabel ___m_outputLabel, ref UISprite ___m_arrow3, ref UISprite ___m_outputSprite, ref UIButton ___m_ShowIndustryInfoButton, ref UIPanel ___m_outputSection, ref UIPanel ___m_inputOutputSection)
        {
            ushort building = ___m_InstanceID.Building;
	    Building data = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building];
            AquacultureFarmAI m_aquacultureFarmAI = data.Info.GetAI() as AquacultureFarmAI;
            FishFarmAI m_fishFarmAI = data.Info.GetAI() as FishFarmAI;

            if (m_aquacultureFarmAI != null)
            {
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

            if(m_fishFarmAI != null && m_fishFarmAI.m_outputResource == TransferManager.TransferReason.Grain)
            {
                ___m_outputBuffer.progressColor = IndustryWorldInfoPanel.instance.GetResourceColor(m_fishFarmAI.m_outputResource);
		string text4 = Locale.Get("WAREHOUSEPANEL_RESOURCE", m_fishFarmAI.m_outputResource.ToString());
		___m_outputLabel.text = text4;
		___m_arrow3.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYBUILDING_EXTRACTINGTOOLTIP"), text4);
		___m_outputSprite.spriteName = IndustryWorldInfoPanel.ResourceSpriteName(m_fishFarmAI.m_outputResource);
		___m_ShowIndustryInfoButton.isVisible = false;
            }

            if(AquacultureExtractorPanel._aquacultureExtractorPanel == null)
            {
                AquacultureExtractorPanel.Init();
            }

            AquacultureExtractorPanel.ExtractorDropdownCheck();
        }

    }
}
