using ColossalFramework;
using UnityEngine;
using ColossalFramework.UI;

namespace IndustriesSunsetHarborMerged {
    class PanelExtenderFishExtractor : MonoBehaviour {
    
        private bool _initialized;
        private CityServiceWorldInfoPanel _cityServiceWorldInfoPanel;
        private UIPanel _ishmContainer;
        private UIComponent _mainSubPanel;
        private DropDown _fishFarmDropDown;

        private void Update()
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
                var fishFarmModelSelector = _cityServiceWorldInfoPanel.Find<UIPanel>("fishFarmModelSelectorContainer");
                if (fishFarmModelSelector != null)
                {
                  fishFarmModelSelector.isVisible = false;
                }
            }
        }

        private void Init()
        {
            _cityServiceWorldInfoPanel = GameObject.Find("(Library) CityServiceWorldInfoPanel").GetComponent<CityServiceWorldInfoPanel>();
            if (!(_cityServiceWorldInfoPanel != null)) return;
            UIComponent agePanel = _cityServiceWorldInfoPanel.Find("FishPanel");
            agePanel.relativePosition = new Vector3(0.0f, 84.0f, agePanel.relativePosition.z);
            _mainSubPanel = agePanel.parent;
            UIPanel uiPanel = _mainSubPanel.AddUIComponent<UIPanel>();
            uiPanel.name = "IshmContainer";
            uiPanel.width = 301f;
            uiPanel.height = 166f;
            uiPanel.autoLayoutDirection = LayoutDirection.Vertical;
            uiPanel.autoLayoutStart = LayoutStart.TopLeft;
            uiPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 5);
            uiPanel.autoLayout = true;
            uiPanel.relativePosition = new Vector3(10f, 224.0f);
            _ishmContainer = uiPanel;
            CreateDropDownPanel(); 
            _initialized = true;
        }

        private void OnDestroy()
        {
            _initialized = false;
            if(_ishmContainer != null) Object.Destroy(_ishmContainer.gameObject);
        }

        private void CreateDropDownPanel()
        {
            UIPanel uiPanel = _ishmContainer.AddUIComponent<UIPanel>();
            uiPanel.width = uiPanel.parent.width;
            uiPanel.height = 27f;
            uiPanel.autoLayoutDirection = LayoutDirection.Horizontal;
            uiPanel.autoLayoutStart = LayoutStart.TopLeft;
            uiPanel.autoLayoutPadding = new RectOffset(0, 6, 0, 0);
            uiPanel.autoLayout = true;
            UILabel uiLabel = uiPanel.AddUIComponent<UILabel>();
            string str1 = "Fish Farm:";
            uiLabel.text = str1;
            int num1 = 0;
            uiLabel.autoSize = num1 != 0;
            double num2 = 27.0;
            uiLabel.height = (float) num2;
            double num3 = 97.0;
            uiLabel.width = (float) num3;
            int num4 = 1;
            uiLabel.verticalAlignment = (UIVerticalAlignment) num4;
            _fishFarmDropDown = new DropDown();
            _fishFarmDropDown.name = "FishFarmDropDown";
            _fishFarmDropDown.height = 27f;
            _fishFarmDropDown.width = 167f;
            _fishFarmDropDown.eventSelectedItemChanged  += OnSelectedFishFarmChanged;
        }


         private void UpdateBindings()
         {
            var FishExtractorId = GetFishExtractorID();
            if(FishExtractorId != 0)
            {
                ushort fishFarmID = CachedFishExtractorData.GetFishFarm(FishExtractorId);
                var fishFarmNotValid = false;
                if(!FishFarmManager.IsValidFishFarm(fishFarmID))
                {
                    fishFarmNotValid = true;
                }
                if (_fishFarmDropDown.Items.Length == 0)
                  _fishFarmDropDown.Text = "No Fish Farms Found";
                else
                  _fishFarmDropDown.SelectedItem = fishFarmID;

                if(fishFarmNotValid)
                {
                    PopulateFishFarmDropDown();
                }
            }
         }

        private void OnSelectedFishFarmChanged(UIComponent component, ushort selectedIndex)
        {
            ushort extractorId = GetFishExtractorID();
            if (extractorId == 0) return;
            CachedFishExtractorData.SetFishFarm(extractorId, selectedIndex);
        }
     
        private void PopulateFishFarmDropDown()
        {
            _fishFarmDropDown.ClearItems();
            _fishFarmDropDown.AddItems(BuildingExtensionManager.GetFishFarmsIds(), IDToName);
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

        public ushort GetFishExtractorID()
        {
          InstanceID currentInstanceId = WorldInfoPanel.GetCurrentInstanceID();
          if (currentInstanceId.Type == InstanceType.Building)
            return currentInstanceId.Building;
          return 0;
        }

    }
}
