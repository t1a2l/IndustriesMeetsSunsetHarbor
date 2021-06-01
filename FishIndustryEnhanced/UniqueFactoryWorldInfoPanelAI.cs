using ColossalFramework;
using ColossalFramework.UI;
using System.Reflection;
using HarmonyLib;

namespace FishIndustryEnhanced
{
	[HarmonyPatch(typeof(UniqueFactoryWorldInfoPanel), "OnSetTarget")]
    public static class UniqueFactoryWorldInfoPanelNameAI
    {
        public static UISprite luxuryProductIcon;

 		[HarmonyPostfix]
        public static void Postfix(UniqueFactoryWorldInfoPanel __instance, UIPanel ___m_productStorage)
		{
			var m_InstanceID = (InstanceID)typeof(WorldInfoPanel).GetField("m_InstanceID", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_productLabel = (UILabel)typeof(UniqueFactoryWorldInfoPanel).GetField("m_productLabel", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            UniqueFactoryAI uniqueFactoryAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)m_InstanceID.Building].Info.m_buildingAI as UniqueFactoryAI;           
            m_productLabel.text = uniqueFactoryAI.m_outputResource.ToString();
            if (!luxuryProductIcon) 
            {
                luxuryProductIcon = ___m_productStorage.Find<UISprite>("LuxuryProductIcon");
            }
            luxuryProductIcon.spriteName = IndustryWorldInfoPanel.ResourceSpriteName(uniqueFactoryAI.m_outputResource, false);
        }
    } 
}
