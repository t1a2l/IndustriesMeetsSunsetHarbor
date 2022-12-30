using ColossalFramework;
using UnityEngine;
using ColossalFramework.UI;
using IndustriesMeetsSunsetHarbor.AI;
using IndustriesMeetsSunsetHarbor.Managers;
using IndustriesMeetsSunsetHarbor.Utils;

namespace IndustriesMeetsSunsetHarbor
{
    class PanelExtenderAquacultureExtractor : MonoBehaviour
    {
    
        private bool _initialized;
        private CityServiceWorldInfoPanel _cityServiceWorldInfoPanel;
        private UIPanel _aquacultureExtractorPanel;
        private DropDown _aquacultureFarmDropDown;

        public void Update()
        {
            if (!_initialized)
            {
                Init();
            }
            else
            {
                if (!_initialized || !_cityServiceWorldInfoPanel.component.isVisible)
                    return;
                UpdateBindings();
            }
        }

        private void Init()
        {
            _cityServiceWorldInfoPanel = GameObject.Find("(Library) CityServiceWorldInfoPanel").GetComponent<CityServiceWorldInfoPanel>();
            if (!(_cityServiceWorldInfoPanel != null)) return;
            UIComponent Wrapper = _cityServiceWorldInfoPanel?.Find("Wrapper");
            UIComponent MainSectionPanel = Wrapper?.Find("MainSectionPanel");
            UIComponent MainTop = MainSectionPanel?.Find("MainTop");
            UIComponent Right = MainTop?.Find("Right");
            UIComponent Info = Right?.Find("Info");
            if(Info != null) {
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
                _initialized = true;
            }
        }

        public void OnDestroy()
        {
            _initialized = false;
            if(_aquacultureExtractorPanel != null) Destroy(_aquacultureExtractorPanel.gameObject);
        }

        private void CreateDropDownPanel()
        {
            UIPanel uiPanel = _aquacultureExtractorPanel.AddUIComponent<UIPanel>();
            uiPanel.width = uiPanel.parent.width;
            uiPanel.height = 27f;
            uiPanel.autoLayoutDirection = LayoutDirection.Horizontal;
            uiPanel.autoLayoutStart = LayoutStart.TopLeft;
            uiPanel.autoLayoutPadding = new RectOffset(0, 6, 0, 0);
            uiPanel.autoLayout = true;
            UILabel uiLabel = uiPanel.AddUIComponent<UILabel>();
            uiLabel.text = "Fish Farm:";
            int num1 = 0;
            uiLabel.autoSize = num1 != 0;
            double num2 = 27.0;
            uiLabel.height = (float) num2;
            double num3 = 97.0;
            uiLabel.width = (float) num3;
            int num4 = 1;
            uiLabel.verticalAlignment = (UIVerticalAlignment) num4;
            _aquacultureFarmDropDown = DropDown.Create(uiPanel);
            _aquacultureFarmDropDown.name = "AquacultureFarmDropDown";
            _aquacultureFarmDropDown.height = 27f;
            _aquacultureFarmDropDown.width = 167f;
            _aquacultureFarmDropDown.DropDownPanelAlignParent = _cityServiceWorldInfoPanel.component;
            _aquacultureFarmDropDown.eventSelectedItemChanged  += OnSelectedAquacultureFarmChanged;

            // Local references.
            var buildingID = GetBuildingID();
            Building[] buildingBuffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            BuildingInfo buildingInfo = buildingBuffer[buildingID].Info;

            // Is this a auqaculture extractor building ?
            AquacultureExtractorAI aquacultureExtractorAI = buildingInfo.GetAI() as AquacultureExtractorAI;
            if (aquacultureExtractorAI != null)
            {
                // school building - show the label.
                _aquacultureExtractorPanel.Show();
            }
            else
            {
                // Not a school building - hide the label.
                _aquacultureExtractorPanel.Hide();
            }
        }

        private void UpdateBindings()
        { 
            var buildingID = GetBuildingID();
            Building[] buildingBuffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            BuildingInfo buildingInfo = buildingBuffer[buildingID].Info;
            
            if(buildingID != 0 && buildingInfo.GetAI() is AquacultureExtractorAI)
            {
                ushort aquacultureFarmID = CachedAquacultureExtractorData.GetAquacultureFarm(buildingID);
                var aquacultureFarmNotValid = false;
                if(!AquacultureFarmManager.IsValidAquacultureFarm(aquacultureFarmID))
                {
                    aquacultureFarmNotValid = true;
                }
                if (_aquacultureFarmDropDown.Items.Length == 0)
                    _aquacultureFarmDropDown.Text = "No Aquaculture Farms Found";
                else
                    _aquacultureFarmDropDown.SelectedItem = aquacultureFarmID;

                if(aquacultureFarmNotValid)
                {
                    PopulateAquacultureFarmDropDown();
                }
            }
        }

        private void OnSelectedAquacultureFarmChanged(UIComponent component, ushort selectedIndex)
        {
            var buildingID = GetBuildingID();
            Building[] buildingBuffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            BuildingInfo buildingInfo = buildingBuffer[buildingID].Info;
            if (buildingID != 0 && buildingInfo.GetAI() is AquacultureExtractorAI)
            {
                CachedAquacultureExtractorData.SetAquacultureFarm(buildingID, selectedIndex);
            } 
        }
     
        private void PopulateAquacultureFarmDropDown()
        {
            _aquacultureFarmDropDown.ClearItems();
            _aquacultureFarmDropDown.AddItems(BuildingExtensionManager.GetAquacultureFarmsIds(), IDToName);
        }

        private string IDToName(ushort buildingID)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            if ((instance.m_buildings.m_buffer[(int) buildingID].m_flags & Building.Flags.Untouchable) != Building.Flags.None)
            {
                buildingID = instance.FindBuilding(instance.m_buildings.m_buffer[buildingID].m_position, 100f, ItemClass.Service.None, ItemClass.SubService.None, Building.Flags.Active, Building.Flags.Untouchable);
            }
            return instance.GetBuildingName(buildingID, InstanceID.Empty) ?? "";
        }

        public ushort GetBuildingID()
        {
          InstanceID currentInstanceId = WorldInfoPanel.GetCurrentInstanceID();
          if (currentInstanceId.Type == InstanceType.Building)
            return currentInstanceId.Building;
          return 0;
        }

    }
}
