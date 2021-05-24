using ColossalFramework;
using ColossalFramework.UI;
using ColossalFramework.Globalization;

namespace FishIndustryEnhanced
{
    class UniqueFactoryWorldInfoPanelExtended : UniqueFactoryWorldInfoPanel
    {
		private UISlider m_productionSlider;
		private UIPanel m_horizontalLine;
		private UIPanel m_inputContainer;
		private UITemplateList<UIPanel> m_inputs;
		private int m_inputResourceCount;
		private UILabel m_productLabel;
		private UIProgressBar m_productBuffer;

        protected override void OnSetTarget()
		{
			base.OnSetTarget();
			UniqueFactoryAI uniqueFactoryAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)this.m_InstanceID.Building].Info.m_buildingAI as UniqueFactoryAI;
			this.m_inputResourceCount = this.GetInputResourceCount(uniqueFactoryAI);
			this.m_inputs.SetItemCount(this.m_inputResourceCount);
			this.m_horizontalLine.width = this.m_inputContainer.width;
			this.m_productLabel.text = uniqueFactoryAI.m_info.name;
			this.m_productBuffer.progressColor = IndustryWorldInfoPanel.instance.GetResourceColor(uniqueFactoryAI.m_outputResource);
			for (int i = 0; i < this.m_inputResourceCount; i++)
			{
				UILabel uilabel = this.m_inputs.items[i].Find<UILabel>("ResourceLabel");
				uilabel.text = this.GetInputResourceName(i);
				UISprite uisprite = this.m_inputs.items[i].Find<UISprite>("ResourceIcon");
				uisprite.spriteName = IndustryWorldInfoPanel.ResourceSpriteName(this.GetInputResource(i), false);
			}
			byte productionRate = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)this.m_InstanceID.Building].m_productionRate;
			if (productionRate > 0)
			{
				this.m_productionSlider.value = (float)productionRate;
			}
		}

		private int GetInputResourceCount(UniqueFactoryAI ai)
		{
			if (ai.m_inputResource1 == TransferManager.TransferReason.None)
			{
				return 0;
			}
			if (ai.m_inputResource2 == TransferManager.TransferReason.None)
			{
				return 1;
			}
			if (ai.m_inputResource3 == TransferManager.TransferReason.None)
			{
				return 2;
			}
			if (ai.m_inputResource4 == TransferManager.TransferReason.None)
			{
				return 3;
			}
			return 4;
		}

		private string GetInputResourceName(int resourceIndex)
		{
			UniqueFactoryAI uniqueFactoryAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)this.m_InstanceID.Building].Info.m_buildingAI as UniqueFactoryAI;
			string key = "N/A";
			switch (resourceIndex)
			{
			case 0:
				key = uniqueFactoryAI.m_inputResource1.ToString();
				break;
			case 1:
				key = uniqueFactoryAI.m_inputResource2.ToString();
				break;
			case 2:
				key = uniqueFactoryAI.m_inputResource3.ToString();
				break;
			case 3:
				key = uniqueFactoryAI.m_inputResource4.ToString();
				break;
			}
			return Locale.Get("WAREHOUSEPANEL_RESOURCE", key);
		}

		private TransferManager.TransferReason GetInputResource(int resourceIndex)
		{
			UniqueFactoryAI uniqueFactoryAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)this.m_InstanceID.Building].Info.m_buildingAI as UniqueFactoryAI;
			switch (resourceIndex)
			{
			case 0:
				return uniqueFactoryAI.m_inputResource1;
			case 1:
				return uniqueFactoryAI.m_inputResource2;
			case 2:
				return uniqueFactoryAI.m_inputResource3;
			case 3:
				return uniqueFactoryAI.m_inputResource4;
			default:
				return TransferManager.TransferReason.None;
			}
		}
    }
}
