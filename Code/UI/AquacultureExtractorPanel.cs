using ColossalFramework;
using ColossalFramework.UI;
using IndustriesMeetsSunsetHarbor.Managers;
using IndustriesMeetsSunsetHarbor.AI;
using UnityEngine;
using System;
using System.Linq;

namespace IndustriesMeetsSunsetHarbor.UI
{
    public static class AquacultureExtractorPanel
    {
        private static CityServiceWorldInfoPanel _cityServiceWorldInfoPanel;
        public static UIPanel _aquacultureExtractorPanel;
        private static UIDropDown _aquacultureFarmDropDown;

        public static void Init()
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
                _aquacultureFarmDropDown.eventSelectedIndexChanged += ChangeSelectedFarm;
                buttonPanels.AttachUIComponent(_aquacultureExtractorPanel.gameObject);
                PopulateAquacultureFarmDropDown();
            }
        }

        private static void ChangeSelectedFarm(UIComponent c, int index)
        {
            var extractorId = GetBuildingID();
            var aquacultureFarm = AquacultureFarmManager.GetAquacultureFarm(extractorId);
            if(AquacultureFarmManager.AquacultureFarms.ContainsKey(aquacultureFarm))
            {
                AquacultureFarmManager.AquacultureFarms[aquacultureFarm].Remove(extractorId);
            }
            var aquacultureFarms = AquacultureFarmManager.GetAquacultureFarmsIds();
            var chosenAquacultureFarm = aquacultureFarms[index];
            if(AquacultureFarmManager.AquacultureFarms.ContainsKey(chosenAquacultureFarm))
            {
                AquacultureFarmManager.AquacultureFarms[chosenAquacultureFarm].Add(extractorId);
            }
            
        } 

        public static void ExtractorDropdownCheck()
        {
            var buildingID = GetBuildingID();
            Building[] buildingBuffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            BuildingInfo buildingInfo = buildingBuffer[buildingID].Info;

            if (buildingID != 0 && buildingInfo.GetAI() is AquacultureExtractorAI)
            {
                if (_aquacultureFarmDropDown.items.Length == 0)
                {
                    _aquacultureFarmDropDown.text = "No Aquaculture Farms Found";
                }
                else
                {
                    ushort aquacultureFarmID = AquacultureFarmManager.GetAquacultureFarm(buildingID);
                    var aquacultureFarms = AquacultureFarmManager.GetAquacultureFarmsIds();
                    var selectedIndex = Array.FindIndex<ushort>(aquacultureFarms, item => item == aquacultureFarmID);
                    _aquacultureFarmDropDown.selectedIndex = selectedIndex;
                     // aquaculture building - show the label.
                    _aquacultureExtractorPanel.Show();
                }
            }
            else
            {
                // Not a aquaculture building - hide the dropdown.
                _aquacultureExtractorPanel.Hide();
            }
        }

        public static void PopulateAquacultureFarmDropDown()
        {
            if(_aquacultureFarmDropDown.items != null && _aquacultureFarmDropDown.items.Length > 0)
            {
                Array.Clear(_aquacultureFarmDropDown.items, 0, _aquacultureFarmDropDown.items.Length);
            }
            var aquacultureFarms = AquacultureFarmManager.GetAquacultureFarmsIds();
            if(aquacultureFarms != null && aquacultureFarms.Length > 0)
            {
                _aquacultureFarmDropDown.items = aquacultureFarms.Select(x => IDToName(x)).ToArray();
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
