using ColossalFramework;
using UnityEngine;
using ColossalFramework.UI;
using System.Collections.Generic;

namespace IndustriesSunsetHarborMerged {
    class FishExtractorAI : FishFarmAI {

        private UILabel _fishAmount;
        private DropDown _fishFarmDropDown;
        private UIPanel _ishmContainer;
        private bool _initialized;
        private UIComponent _mainSubPanel;
        private Dictionary<BuildingExtension.FishItemClass, bool> _updateFishFarms;
        private BuildingWorldInfoPanel _buildingWorldInfoPanel;


        public FishExtractorAI()
        {
             Dictionary<BuildingExtension.FishItemClass, bool> dictionary = new Dictionary<BuildingExtension.FishItemClass, bool>();
             dictionary.Add(new BuildingExtension.FishItemClass(ItemClass.Service.Fishing), true);
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
        if (!_initialized)
          return;
        UpdateBindings();
      }
    }

    private void Init()
    {
        _buildingWorldInfoPanel = GameObject.Find("(Library) BuildingWorldInfoPanel").GetComponent<BuildingWorldInfoPanel>();      
        UIComponent fishExtractorPanel = _buildingWorldInfoPanel.Find("FishFarmPanel");
        _fishAmount = BuildingExtension.GetPrivate<UILabel>((object) _buildingWorldInfoPanel, "m_incomingResources");
        _mainSubPanel = fishExtractorPanel.parent;
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
        BuildingExtension.OnFishFarmAdded += OnFishFarmChanged;
        BuildingExtension.OnFishFarmRemoved += OnFishFarmChanged;
        _initialized = true;
    }


        private void UpdateBindings()
        {
            bool flag1 = false;
            ushort extractorID = GetFishExtractorID();
            ushort fishFarmID = BuildingExtension.GetFishFarm(extractorID);
            BuildingInfo extractorInfo = BuildingManager.instance.m_buildings.m_buffer[extractorID].Info;
            BuildingInfo fishFarmInfo = BuildingManager.instance.m_buildings.m_buffer[fishFarmID].Info;
            ItemClass.Service service = fishFarmInfo.GetService();
            BuildingExtension.FishItemClass fishfarm = new BuildingExtension.FishItemClass(service);
            if (!FishFarmUtil.IsValidFishFarm(fishFarmID))
            {
                flag1 = true;
            }
            if (flag1 || _updateFishFarms[fishfarm])
            {
                PopulateFishFarmDropDown(extractorInfo, fishFarmInfo);
                _updateFishFarms[fishfarm] = false;
            }
            if (_fishFarmDropDown.Items.Length == 0)
                _fishFarmDropDown.Text = "No fish farm found.";
            else
                _fishFarmDropDown.SelectedItem = fishFarmID;
            
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
            UIFont font = _fishAmount.font;
            uiLabel.font = font;
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

        private void OnDestroy()
        {
            _initialized = false;
            BuildingExtension.OnFishFarmAdded -= OnFishFarmChanged;
            BuildingExtension.OnFishFarmRemoved -= OnFishFarmChanged;
            if (_updateFishFarms != null)
                _updateFishFarms.Clear();
            if ((Object) _ishmContainer != (Object) null)
            Object.Destroy((Object) _ishmContainer.gameObject);
        }

        private void OnFishFarmMarkerClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
          component.Unfocus();
          if ((int) _fishFarmDropDown.SelectedItem == 0)
            return;
          InstanceID id = new InstanceID();
          id.Building = _fishFarmDropDown.SelectedItem;
          ToolsModifierControl.cameraController.SetTarget(id, ToolsModifierControl.cameraController.transform.position, Input.GetKey(KeyCode.LeftShift) | Input.GetKey(KeyCode.RightShift));
          DefaultTool.OpenWorldInfoPanel(id, ToolsModifierControl.cameraController.transform.position);
        }

        private void OnFishFarmChanged(ItemClass.Service service)
        {
            _updateFishFarms[new BuildingExtension.FishItemClass(service)] = true;
        }

        private void PopulateFishFarmDropDown(BuildingInfo extractorInfo, BuildingInfo fishFarmInfo)
        {
            _fishFarmDropDown.ClearItems();
            if (extractorInfo == null || fishFarmInfo == null)
            {
                return;
            }
            _fishFarmDropDown.AddItems(BuildingExtension.GetFishFarms(extractorInfo, fishFarmInfo), IDToName);
        }

        private string IDToName(ushort buildingID)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            if ((instance.m_buildings.m_buffer[(int) buildingID].m_flags & Building.Flags.Untouchable) != Building.Flags.None)
            {
                buildingID = instance.FindBuilding(instance.m_buildings.m_buffer[(int) buildingID].m_position, 100f, ItemClass.Service.None, ItemClass.SubService.None, Building.Flags.Active, Building.Flags.Untouchable);
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

        private void OnSelectedFishFarmChanged(UIComponent component, ushort selectedItem)
        {
            ushort ExtractorId = GetFishExtractorID();
            if ((int) ExtractorId == 0) return;
            BuildingExtension.SetFishFarm(ExtractorId, selectedItem);
        }

    }
}
