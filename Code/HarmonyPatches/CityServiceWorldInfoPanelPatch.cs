using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using HarmonyLib;
using System.Reflection;
using IndustriesMeetsSunsetHarbor.Managers;
using IndustriesMeetsSunsetHarbor.UI;
using IndustriesMeetsSunsetHarbor.AI;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{

    [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "OnSetTarget")]
    public static class CityServiceWorldInfoPanelPatch
    {
        private static CityServiceWorldInfoPanel _cityServiceWorldInfoPanel;
        private static UIPanel _aquacultureExtractorPanel;
        private static UIDropDown _aquacultureFarmDropDown;

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
                UIComponent wrapper = _cityServiceWorldInfoPanel?.Find("Wrapper");
                UIComponent mainSectionPanel = wrapper?.Find("MainSectionPanel");
                UIComponent mainBottom = mainSectionPanel?.Find("MainBottom");
                UIComponent buttonPanels = mainBottom?.Find("ButtonPanels");
                if (buttonPanels != null)
                {
                    _aquacultureExtractorPanel = buttonPanels.AddUIComponent<UIPanel>();
                    _aquacultureExtractorPanel.name = "AquacultureExtractorPanel";
                    _aquacultureExtractorPanel.width = 301f;
                    _aquacultureExtractorPanel.height = 166f;
                    _aquacultureExtractorPanel.autoLayoutDirection = LayoutDirection.Vertical;
                    _aquacultureExtractorPanel.autoLayoutStart = LayoutStart.TopLeft;
                    _aquacultureExtractorPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 5);
                    _aquacultureExtractorPanel.autoLayout = true;
                    _aquacultureExtractorPanel.relativePosition = new Vector3(10f, 224.0f);
                    _aquacultureFarmDropDown = UIDropDowns.AddLabelledDropDown(_aquacultureExtractorPanel, _aquacultureExtractorPanel.width, 160f, "Aquaculture Farm:");
                    _aquacultureFarmDropDown.eventSelectedIndexChanged += (control, index) =>
                    {
                        var buildingID = GetBuildingID();
                        Building[] buildingBuffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
                        BuildingInfo buildingInfo = buildingBuffer[buildingID].Info;
                        if (buildingID != 0 && buildingInfo.GetAI() is AquacultureExtractorAI)
                        {
                            var extractorId = buildingID;
                            var aquacultureFarm = AquacultureFarmManager.GetAquacultureFarm(extractorId);
                            if(AquacultureFarmManager.AquacultureFarms.TryGetValue(aquacultureFarm, out List<ushort> _))
                            {
                                AquacultureFarmManager.AquacultureFarms[aquacultureFarm].Remove(extractorId);
                            }
                            var aquacultureFarms = AquacultureFarmManager.GetAquacultureFarmsIds();
                            var chosenAquacultureFarm = aquacultureFarms[index];
                            if(AquacultureFarmManager.AquacultureFarms.TryGetValue(chosenAquacultureFarm, out List<ushort> _))
                            {
                                 AquacultureFarmManager.AquacultureFarms[chosenAquacultureFarm].Add(extractorId);
                            }
                        }
                    };
                }
            }
            
            var buildingID = GetBuildingID();
            Building[] buildingBuffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            BuildingInfo buildingInfo = buildingBuffer[buildingID].Info;

            if (buildingID != 0 && buildingInfo.GetAI() is AquacultureExtractorAI)
            {
                ushort aquacultureFarmID = AquacultureFarmManager.GetAquacultureFarm(buildingID);
                var aquacultureFarmNotValid = false;
                if (!AquacultureFarmManager.IsValidAquacultureFarm(aquacultureFarmID))
                {
                    aquacultureFarmNotValid = true;
                }
                if (_aquacultureFarmDropDown.items.Length == 0)
                {
                    _aquacultureFarmDropDown.text = "No Aquaculture Farms Found";
                }
                else
                {
                    var aquacultureFarms = AquacultureFarmManager.GetAquacultureFarmsIds();
                    var selectedIndex = Array.FindIndex<ushort>(aquacultureFarms, item => item == aquacultureFarmID);
                    _aquacultureFarmDropDown.selectedIndex = selectedIndex;
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

        private static void PopulateAquacultureFarmDropDown()
        {
            if(_aquacultureFarmDropDown.items != null && _aquacultureFarmDropDown.items.Length > 0)
            {
                Array.Clear(_aquacultureFarmDropDown.items, 0, _aquacultureFarmDropDown.items.Length);
            }
            var aquacultureFarms = AquacultureFarmManager.GetAquacultureFarmsIds();
            if(aquacultureFarms != null && aquacultureFarms.Length > 0)
            {
                _aquacultureFarmDropDown.items = new string[aquacultureFarms.Length];
                foreach(ushort item in aquacultureFarms)
                {
                    _aquacultureFarmDropDown.AddItem(IDToName(item));
                }
            }
        }

        private static string IDToName(ushort buildingID)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            if ((instance.m_buildings.m_buffer[(int)buildingID].m_flags & Building.Flags.Untouchable) != Building.Flags.None)
            {
                buildingID = instance.FindBuilding(instance.m_buildings.m_buffer[buildingID].m_position, 100f, ItemClass.Service.None, ItemClass.SubService.None, Building.Flags.Active, Building.Flags.Untouchable);
            }
            var building_name = instance.GetBuildingName(buildingID, InstanceID.Empty) ?? "No name";
            return building_name;
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
