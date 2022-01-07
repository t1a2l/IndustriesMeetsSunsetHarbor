using ColossalFramework;
using UnityEngine;
using ColossalFramework.UI;
using System.Collections.Generic;
using IndustriesSunsetHarborMerged.Utils.BuildingExtension;
using IndustriesSunsetHarborMerged.Utils.FishFarmUtils;

namespace IndustriesSunsetHarborMerged.PanelExtenderFishExtractor {
    class PanelExtenderFishExtractor : MonoBehaviour {
    
        private bool _initialized;
        private int _cachedFishExtractorID;
        private Dictionary<ItemClassFishFarm.ItemClassFishFarm, bool> _updateFishFarms;
        private CityServiceWorldInfoPanel _cityServiceWorldInfoPanel;
        private UIPanel _ishmContainer;
        private DropDown _fishFarmDropDown;
        private UIPanel _prefabPanel;
        private UILabel _fishAmount;

        public PanelExtenderFishExtractor()
        {
             Dictionary<ItemClassFishFarm.ItemClassFishFarm, bool> dictionary = new Dictionary<ItemClassFishFarm.ItemClassFishFarm, bool>();
             dictionary.Add(new ItemClassFishFarm.ItemClassFishFarm(ItemClass.Service.Fishing), true);
             _updateFishFarms = dictionary;
        }

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
            if (!(_cityServiceWorldInfoPanel != null))
                return;        
            CreateDropDownPanel(); 
            BuildingExtension.OnFishFarmAdded += OnFishFarmChanged;
            BuildingExtension.OnFishFarmRemoved += OnFishFarmChanged;
            _initialized = true;
        }

        private void UpdateBindings()
        {
            ushort extractorID = GetFishExtractorID();
            if(extractorID != 0) {
                bool flag1 = false;
                ushort fishFarmID = CachedFishExtractorData.CachedFishExtractorData.GetFishFarm(extractorID);
                BuildingInfo extractorInfo = BuildingManager.instance.m_buildings.m_buffer[extractorID].Info;
                if (!FishFarmUtils.IsValidFishFarm(fishFarmID, extractorInfo))
                {
                    flag1 = true;
                }
                ItemClass.Service service = extractorInfo.GetService();
                ItemClassFishFarm.ItemClassFishFarm triplet = new ItemClassFishFarm.ItemClassFishFarm(service);
                if (flag1 || _updateFishFarms[triplet])
                {
                    PopulateFishFarmDropDown(extractorInfo);
                    _updateFishFarms[triplet] = false;
                }
                if (_fishFarmDropDown.Items.Length == 0) {
                    _fishFarmDropDown.Text = "No Fish Farms Found";
                } 
                else
                {
                    _fishFarmDropDown.SelectedItem = fishFarmID;
                }
                if (extractorID != _cachedFishExtractorID)
                {
                  _prefabPanel.Hide();
                }
            }
            else {
                 _cityServiceWorldInfoPanel.Hide();
            } 
        }

        private void OnDestroy()
        {
            _initialized = false;
            BuildingExtension.OnFishFarmAdded -= OnFishFarmChanged;
            BuildingExtension.OnFishFarmRemoved -= OnFishFarmChanged;
            if (_updateFishFarms != null)
                _updateFishFarms.Clear();
            if (_ishmContainer != null)
                Object.Destroy((Object) _ishmContainer.gameObject);
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
            Color32 textColor = _fishAmount.textColor;
            uiLabel.textColor = textColor;
            double textScale = (double) _fishAmount.textScale;
            uiLabel.textScale = (float) textScale;
            int num1 = 0;
            uiLabel.autoSize = num1 != 0;
            double num2 = 27.0;
            uiLabel.height = (float) num2;
            double num3 = 97.0;
            uiLabel.width = (float) num3;
            int num4 = 1;
            uiLabel.verticalAlignment = (UIVerticalAlignment) num4;
            _fishFarmDropDown = DropDown.Create((UIComponent) uiPanel);
            _fishFarmDropDown.name = "FishFarmDropDown";
            _fishFarmDropDown.Font = _fishAmount.font;
            _fishFarmDropDown.height = 27f;
            _fishFarmDropDown.width = 167f;
            _fishFarmDropDown.eventSelectedItemChanged += new PropertyChangedEventHandler<ushort>(OnSelectedFishFarmChanged);
            UIButton uiButton = uiPanel.AddUIComponent<UIButton>();
            string str2 = "FishFarmMarker";
            uiButton.name = str2;
            string str3 = "LocationMarkerNormal";
            uiButton.normalBgSprite = str3;
            string str4 = "LocationMarkerDisabled";
            uiButton.disabledBgSprite = str4;
            string str5 = "LocationMarkerHovered";
            uiButton.hoveredBgSprite = str5;
            string str6 = "LocationMarkerFocused";
            uiButton.focusedBgSprite = str6;
            string str7 = "LocationMarkerPressed";
            uiButton.pressedBgSprite = str7;
            Vector2 vector2 = new Vector2(27f, 27f);
            uiButton.size = vector2;
            string str8 = "Jump towards the selected fish farm.\nHolding down a shift key when clicking will also zoom in.";
            uiButton.tooltip = str8;
            MouseEventHandler mouseEventHandler = new MouseEventHandler(OnFishFarmMarkerClicked);
            uiButton.eventClick += mouseEventHandler;
        }

        private void OnSelectedFishFarmChanged(UIComponent component, ushort selectedItem)
        {
            ushort extractorId = GetFishExtractorID();
            if (extractorId == 0) return;
            CachedFishExtractorData.CachedFishExtractorData.SetFishFarm(extractorId, selectedItem);
        }

        private void OnFishFarmMarkerClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
          component.Unfocus();
          if (_fishFarmDropDown.SelectedItem == 0)
            return;
          InstanceID id = new InstanceID();
          id.Building = _fishFarmDropDown.SelectedItem;
          ToolsModifierControl.cameraController.SetTarget(id, ToolsModifierControl.cameraController.transform.position, Input.GetKey(KeyCode.LeftShift) | Input.GetKey(KeyCode.RightShift));
          DefaultTool.OpenWorldInfoPanel(id, ToolsModifierControl.cameraController.transform.position);
        }

        private void OnFishFarmChanged(ItemClass.Service service)
        {
            _updateFishFarms[new ItemClassFishFarm.ItemClassFishFarm(service)] = true;
        }
     
        private void PopulateFishFarmDropDown(BuildingInfo buildingInfo)
        {
            _fishFarmDropDown.ClearItems();
            if (buildingInfo == null)
            {
                return;
            }
            _fishFarmDropDown.AddItems(BuildingExtension.GetFishFarms(buildingInfo), IDToName);
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
