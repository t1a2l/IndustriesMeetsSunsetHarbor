using ColossalFramework;
using ColossalFramework.UI;
using IndustriesMeetsSunsetHarbor.Managers;
using IndustriesMeetsSunsetHarbor.AI;
using UnityEngine;
using System;
using System.Linq;
using IndustriesMeetsSunsetHarbor.Utils;

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
                _aquacultureExtractorPanel.width = 230f;
                _aquacultureExtractorPanel.height = 40f;
                _aquacultureExtractorPanel.autoLayoutDirection = LayoutDirection.Vertical;
                _aquacultureExtractorPanel.autoLayoutStart = LayoutStart.TopLeft;
                _aquacultureExtractorPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 5);
                _aquacultureExtractorPanel.autoLayout = true;
                _aquacultureExtractorPanel.relativePosition = new Vector3(50f, 10f);
                _aquacultureFarmDropDown = UIDropDowns.AddLabelledDropDown(_aquacultureExtractorPanel, _aquacultureExtractorPanel.width, 160f, "Aquaculture Farm:");
                _aquacultureFarmDropDown.eventSelectedIndexChanged += ChangeSelectedFarm;
                buttonPanels.AttachUIComponent(_aquacultureExtractorPanel.gameObject);
            }
        }

        private static void ChangeSelectedFarm(UIComponent c, int index)
        {
            var extractorId = GetBuildingID();
            var aquacultureFarmId = AquacultureFarmManager.GetAquacultureFarm(extractorId);
            LogHelper.Information("the aquacultureFarm id is: {0}, name is: {1}", aquacultureFarmId, IDToName(aquacultureFarmId));
            var aquacultureFarmsIds = AquacultureFarmManager.GetAquacultureFarmsIds(extractorId);
            var chosenAquacultureFarmId = aquacultureFarmsIds[index];
            if(aquacultureFarmId != 0) // if extractor already belong to other farm - remove it 
            {
                if(AquacultureFarmManager.AquacultureFarms.ContainsKey(aquacultureFarmId))
                {
                    AquacultureFarmManager.AquacultureFarms[aquacultureFarmId].Remove(extractorId);
                    LogHelper.Information("extractor removed from list farm id - {0}, farm name: {1}", aquacultureFarmId, IDToName(aquacultureFarmId));
                }
            }
            if(AquacultureFarmManager.AquacultureFarms.ContainsKey(chosenAquacultureFarmId)) // add extractor to new farm
            {
                AquacultureFarmManager.AquacultureFarms[chosenAquacultureFarmId].Add(extractorId);
                LogHelper.Information("extractor add to list farm id - {0}, farm name: {1}", chosenAquacultureFarmId, IDToName(chosenAquacultureFarmId));
            }         
        } 

        public static void ExtractorDropdownCheck()
        {
            var buildingID = GetBuildingID();
            Building[] buildingBuffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            BuildingInfo buildingInfo = buildingBuffer[buildingID].Info;
            if (buildingID != 0 && buildingInfo.GetAI() is AquacultureExtractorAI)
            {
                var aquacultureFarmsIds = AquacultureFarmManager.GetAquacultureFarmsIds(buildingID);
                PopulateAquacultureFarmDropDown(aquacultureFarmsIds);
                if (_aquacultureFarmDropDown.items.Length == 0)
                {
                    _aquacultureFarmDropDown.text = "No Aquaculture Farms Found";
                }
                else
                {
                    ushort aquacultureFarmID = AquacultureFarmManager.GetAquacultureFarm(buildingID);
                    if(aquacultureFarmID == 0)
                    {
                        aquacultureFarmID = AquacultureFarmManager.GetClosestAquacultureFarm(buildingID);
                        if(AquacultureFarmManager.AquacultureFarms.ContainsKey(aquacultureFarmID))
                        {
                            AquacultureFarmManager.AquacultureFarms[aquacultureFarmID].Add(buildingID);
                        }
                    }
                    var selectedIndex = Array.FindIndex<ushort>(aquacultureFarmsIds, item => item == aquacultureFarmID);
                    if(selectedIndex != -1)
                    {
                        _aquacultureFarmDropDown.selectedIndex = selectedIndex;
                    }
                }
                _aquacultureExtractorPanel.Show();
            }
            else
            {
                // Not a aquaculture extractor - hide the dropdown.
                _aquacultureExtractorPanel.Hide();
            }
        }

        public static void PopulateAquacultureFarmDropDown(ushort[] aquacultureFarmsIds)
        {
            LogHelper.Information("aquacultureFarmsIds length: {0}", aquacultureFarmsIds.Length);
            LogHelper.Information("_aquacultureFarmDropDown length befor clear: {0}", _aquacultureFarmDropDown.items.Length);
            if(_aquacultureFarmDropDown.items != null && _aquacultureFarmDropDown.items.Length > 0)
            {
                _aquacultureFarmDropDown.items = new string[0];
            }
            LogHelper.Information("_aquacultureFarmDropDown length after clear: {0}", _aquacultureFarmDropDown.items.Length);
            if(aquacultureFarmsIds != null && aquacultureFarmsIds.Length > 0)
            {
                _aquacultureFarmDropDown.items = aquacultureFarmsIds.Select(x => IDToName(x)).ToArray();
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
