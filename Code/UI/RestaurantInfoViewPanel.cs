using ColossalFramework;
using ColossalFramework.UI;
using IndustriesMeetsSunsetHarbor.Utils;

namespace IndustriesMeetsSunsetHarbor.UI
{
    public class RestaurantInfoViewPanel : InfoViewPanel
    {
        private bool m_initialized;

        protected override void UpdatePanel()
        {
            if (!m_initialized && Singleton<LoadingManager>.exists && Singleton<LoadingManager>.instance.m_loadingComplete && Singleton<InfoManager>.exists)
            {
                UISprite uISprite = Find<UISprite>("ColorActive");
                UISprite uISprite2 = Find<UISprite>("ColorInactive");
                UILabel mainLabel = Find<UILabel>("Label");
                mainLabel.text = "Restaurants";
                UISprite mainIcon = Find<UISprite>("Icon");
                mainIcon.atlas = TextureUtils.GetAtlas("InfoIconRestaurantButtonAtlas");
                mainIcon.spriteName = "InfoIconRestaurant";
                UILabel buildingLabel = Find<UILabel>("Building");
                UILabel roadCoverageLabel = Find<UILabel>("RoadCoverage");
                buildingLabel.text = "Restaurants";
                roadCoverageLabel.text = "Customers and Orders";
                uISprite.color = Singleton<InfoManager>.instance.m_properties.m_modeProperties[41].m_activeColor;
                uISprite2.color = Singleton<InfoManager>.instance.m_properties.m_modeProperties[41].m_inactiveColor;
                UITextureSprite uITextureSprite = Find<UITextureSprite>("CoverageGradient");
                uITextureSprite.renderMaterial.SetColor("_ColorB", Singleton<InfoManager>.instance.m_properties.m_modeProperties[41].m_targetColor);
                uITextureSprite.renderMaterial.SetColor("_ColorA", Singleton<InfoManager>.instance.m_properties.m_neutralColor);
                UILabel highLabel = Find<UILabel>("High");
                UILabel lowLabel = Find<UILabel>("Low");
                highLabel.text = "Deliveries & Customers";
                lowLabel.text = "None";
                m_initialized = true;
            }
        }
    }
}
