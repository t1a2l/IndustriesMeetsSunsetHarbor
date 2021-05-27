using ColossalFramework;
using ColossalFramework.UI;
using System.Reflection;
using HarmonyLib;

namespace FishIndustryEnhanced
{
    public static class UniqueFactoryWorldInfoPanelAI
    {
		[HarmonyPostfix]
        public static void Postfix(UniqueFactoryWorldInfoPanel __instance)
		{
			var m_InstanceID = (InstanceID)typeof(WorldInfoPanel).GetField("m_InstanceID", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(typeof(WorldInfoPanel));
			var m_inputResourceCount = (int)typeof(UniqueFactoryWorldInfoPanel).GetField("m_inputResourceCount", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(typeof(UniqueFactoryWorldInfoPanel));
			var m_inputs = (UITemplateList<UIPanel>)typeof(UniqueFactoryWorldInfoPanel).GetField("m_inputs", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(typeof(UniqueFactoryWorldInfoPanel));
			var m_horizontalLine = (UIPanel)typeof(UniqueFactoryWorldInfoPanel).GetField("m_horizontalLine", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(typeof(UniqueFactoryWorldInfoPanel));
			var m_inputContainer = (UIPanel)typeof(UniqueFactoryWorldInfoPanel).GetField("m_inputContainer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(typeof(UniqueFactoryWorldInfoPanel));
			var m_productLabel = (UILabel)typeof(UniqueFactoryWorldInfoPanel).GetField("m_productLabel", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(typeof(UniqueFactoryWorldInfoPanel));
			var m_productBuffer = (UIProgressBar)typeof(UniqueFactoryWorldInfoPanel).GetField("m_productBuffer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(typeof(UniqueFactoryWorldInfoPanel));
			var m_productionSlider = (UISlider)typeof(UniqueFactoryWorldInfoPanel).GetField("m_productionSlider", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(typeof(UniqueFactoryWorldInfoPanel));

			var GetInputResourceCount = typeof(UniqueFactoryWorldInfoPanel).GetMethod("GetInputResourceCount", BindingFlags.NonPublic | BindingFlags.Instance);
			var GetInputResourceName = typeof(UniqueFactoryWorldInfoPanel).GetMethod("GetInputResourceName", BindingFlags.NonPublic | BindingFlags.Instance);
			var GetInputResource = typeof(UniqueFactoryWorldInfoPanel).GetMethod("GetInputResource", BindingFlags.NonPublic | BindingFlags.Instance);

			var OnSetTarget = typeof(BuildingWorldInfoPanel).GetMethod("OnSetTarget", BindingFlags.NonPublic | BindingFlags.Instance);
			OnSetTarget.Invoke(__instance, null);
			UniqueFactoryAI uniqueFactoryAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)m_InstanceID.Building].Info.m_buildingAI as UniqueFactoryAI;
			m_inputResourceCount = (int)GetInputResourceCount.Invoke(__instance, new object[] { uniqueFactoryAI });
			m_inputs.SetItemCount(m_inputResourceCount);
			m_horizontalLine.width = m_inputContainer.width;
			m_productLabel.text = uniqueFactoryAI.m_info.name;
			m_productBuffer.progressColor = IndustryWorldInfoPanel.instance.GetResourceColor(uniqueFactoryAI.m_outputResource);
			for (int i = 0; i < m_inputResourceCount; i++)
			{
				UILabel uilabel = m_inputs.items[i].Find<UILabel>("ResourceLabel");
				uilabel.text = (string)GetInputResourceName.Invoke(__instance, new object[] { i });
				UISprite uisprite = m_inputs.items[i].Find<UISprite>("ResourceIcon");
				uisprite.spriteName = IndustryWorldInfoPanel.ResourceSpriteName((TransferManager.TransferReason)GetInputResource.Invoke(__instance, new object[] { i }), false);
			}
			byte productionRate = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)m_InstanceID.Building].m_productionRate;
			if (productionRate > 0)
			{
				m_productionSlider.value = (float)productionRate;
			}
		}
    }
}
