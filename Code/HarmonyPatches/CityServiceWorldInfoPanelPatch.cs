using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using HarmonyLib;
using System.Reflection;
using IndustriesMeetsSunsetHarbor.Managers;
using IndustriesMeetsSunsetHarbor.UI;
using IndustriesMeetsSunsetHarbor.AI;
using IndustriesMeetsSunsetHarbor.Utils;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{

    [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "OnSetTarget")]
    public static class CityServiceWorldInfoPanelPatch
    {
        private static CityServiceWorldInfoPanel _cityServiceWorldInfoPanel;
        private static UIPanel _aquacultureExtractorPanel;
        private static DropDown _aquacultureFarmDropDown;

        [HarmonyPostfix]
        public static void Postfix(CityServiceWorldInfoPanel __instance)
        {
            var m_fishFarmAI = (FishFarmAI)typeof(CityServiceWorldInfoPanel).GetField("m_fishFarmAI", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_outputBuffer = (UIProgressBar)typeof(CityServiceWorldInfoPanel).GetField("m_outputBuffer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_outputLabel = (UILabel)typeof(CityServiceWorldInfoPanel).GetField("m_outputLabel", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_arrow3 = (UISprite)typeof(CityServiceWorldInfoPanel).GetField("m_arrow3", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_outputSprite = (UISprite)typeof(CityServiceWorldInfoPanel).GetField("m_outputSprite", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_ShowIndustryInfoButton = (UIButton)typeof(CityServiceWorldInfoPanel).GetField("m_ShowIndustryInfoButton", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            if (m_fishFarmAI != null)
            {
                m_outputBuffer.progressColor = IndustryWorldInfoPanel.instance.GetResourceColor(m_fishFarmAI.m_outputResource);
                string text = Locale.Get("WAREHOUSEPANEL_RESOURCE", m_fishFarmAI.m_outputResource.ToString());
                m_outputLabel.text = text;
                m_arrow3.tooltip = StringUtils.SafeFormat(Locale.Get("INDUSTRYBUILDING_EXTRACTINGTOOLTIP"), text);
                m_outputSprite.spriteName = IndustryWorldInfoPanel.ResourceSpriteName(m_fishFarmAI.m_outputResource, false);
                m_ShowIndustryInfoButton.isVisible = false;
            }
        }

        [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "UpdateBindings")]
        [HarmonyPostfix]
        public static void UpdateBindings()
	{
            if(_aquacultureExtractorPanel == null || _aquacultureFarmDropDown == null)
            {
                _cityServiceWorldInfoPanel = UIView.library.Get<CityServiceWorldInfoPanel>(typeof(CityServiceWorldInfoPanel).Name);
                if (!(_cityServiceWorldInfoPanel != null)) return;
                UIComponent Wrapper = _cityServiceWorldInfoPanel?.Find("Wrapper");
                UIComponent MainSectionPanel = Wrapper?.Find("MainSectionPanel");
                UIComponent MainTop = MainSectionPanel?.Find("MainTop");
                UIComponent Right = MainTop?.Find("Right");
                UIComponent Info = Right?.Find("Info");
                if (Info != null)
                {
                    UIPanel uiPanel = Info.AddUIComponent<UIPanel>();
                    uiPanel.name = "AquacultureExtractorPanel";
                    uiPanel.width = 301f;
                    uiPanel.height = 166f;
                    uiPanel.autoLayoutDirection = LayoutDirection.Vertical;
                    uiPanel.autoLayoutStart = LayoutStart.TopLeft;
                    uiPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 5);
                    uiPanel.autoLayout = true;
                    uiPanel.relativePosition = new Vector3(10f, 224.0f);
                    _aquacultureExtractorPanel = uiPanel;
                    CreateDropDownPanel();
                }
            }
            
            var buildingID = GetBuildingID();
            Building[] buildingBuffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            BuildingInfo buildingInfo = buildingBuffer[buildingID].Info;

            if (buildingID != 0 && buildingInfo.GetAI() is AquacultureExtractorAI)
            {
                ushort aquacultureFarmID = CachedAquacultureExtractorData.GetAquacultureFarm(buildingID);
                var aquacultureFarmNotValid = false;
                if (!AquacultureFarmManager.IsValidAquacultureFarm(aquacultureFarmID))
                {
                    aquacultureFarmNotValid = true;
                }
                if (_aquacultureFarmDropDown.Items.Length == 0)
                    _aquacultureFarmDropDown.Text = "No Aquaculture Farms Found";
                else
                {
                    _aquacultureFarmDropDown.SelectedItem = aquacultureFarmID;
                     // aquaculture building - show the label.
                    _aquacultureExtractorPanel.Show();
            
                }
                if (aquacultureFarmNotValid)
                {
                    PopulateAquacultureFarmDropDown();
                }
            }
            else
            {
                // Not a aquaculture building - hide the dropdown.
                _aquacultureExtractorPanel.Hide();
            }
        }

        private static void CreateDropDownPanel()
        {
            UIPanel uiPanel = _aquacultureExtractorPanel.AddUIComponent<UIPanel>();
            uiPanel.width = uiPanel.parent.width;
            uiPanel.height = 27f;
            uiPanel.autoLayoutDirection = LayoutDirection.Horizontal;
            uiPanel.autoLayoutStart = LayoutStart.TopLeft;
            uiPanel.autoLayoutPadding = new RectOffset(0, 6, 0, 0);
            uiPanel.autoLayout = true;
            UILabel uiLabel = uiPanel.AddUIComponent<UILabel>();
            uiLabel.text = "Aquaculture Farm:";
            int num1 = 0;
            uiLabel.autoSize = num1 != 0;
            double num2 = 27.0;
            uiLabel.height = (float)num2;
            double num3 = 97.0;
            uiLabel.width = (float)num3;
            int num4 = 1;
            uiLabel.verticalAlignment = (UIVerticalAlignment)num4;
            _aquacultureFarmDropDown = DropDown.Create(uiPanel);
            _aquacultureFarmDropDown.name = "AquacultureFarmDropDown";
            _aquacultureFarmDropDown.height = 27f;
            _aquacultureFarmDropDown.width = 167f;
            _aquacultureFarmDropDown.DropDownPanelAlignParent = _cityServiceWorldInfoPanel.component;
            _aquacultureFarmDropDown.eventSelectedItemChanged += OnSelectedAquacultureFarmChanged;
        }

        private static void OnSelectedAquacultureFarmChanged(UIComponent component, ushort selectedIndex)
        {
            var buildingID = GetBuildingID();
            Building[] buildingBuffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            BuildingInfo buildingInfo = buildingBuffer[buildingID].Info;
            if (buildingID != 0 && buildingInfo.GetAI() is AquacultureExtractorAI)
            {
                var currentAquacultureFarmId = CachedAquacultureExtractorData.GetAquacultureFarm(buildingID);
                if(currentAquacultureFarmId != 0)
                {
                    AquacultureFarmManager.AquacultureFarms[currentAquacultureFarmId].Remove(buildingID);
                }
                AquacultureFarmManager.AquacultureFarms[selectedIndex].Add(buildingID);
                CachedAquacultureExtractorData.SetAquacultureFarm(buildingID, selectedIndex);
            }
        }

        private static void PopulateAquacultureFarmDropDown()
        {
            _aquacultureFarmDropDown.ClearItems();
            _aquacultureFarmDropDown.AddItems(AquacultureFarmManager.GetAquacultureFarmsIds(), IDToName);
        }

        private static string IDToName(ushort buildingID)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            if ((instance.m_buildings.m_buffer[(int)buildingID].m_flags & Building.Flags.Untouchable) != Building.Flags.None)
            {
                buildingID = instance.FindBuilding(instance.m_buildings.m_buffer[buildingID].m_position, 100f, ItemClass.Service.None, ItemClass.SubService.None, Building.Flags.Active, Building.Flags.Untouchable);
            }
            return instance.GetBuildingName(buildingID, InstanceID.Empty) ?? "";
        }

        public static ushort GetBuildingID()
        {
            InstanceID currentInstanceId = WorldInfoPanel.GetCurrentInstanceID();
            if (currentInstanceId.Type == InstanceType.Building)
                return currentInstanceId.Building;
            return 0;
        }
    }
}
