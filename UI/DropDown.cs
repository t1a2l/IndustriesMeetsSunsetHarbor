using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace IndustriesSunsetHarborMerged
{
  public class DropDown : UIPanel
  {
    protected int _itemHeight = 27;
    protected int _maxVisibleItems = 8;
    protected bool _showPanel = true;
    protected Vector3 _listPosition = Vector3.zero;
    protected DropDownRow[] _items = new DropDownRow[0];
    protected UIButton _triggerButton;
    protected UIPanel _dropDownPanel;
    protected UIComponent _dropDownPanelAlignParent;
    protected UIScrollablePanel _scrollablePanel;
    protected UIPanel _scrollbarPanel;
    protected UIScrollbar _scrollbar;

    public UIComponent DropDownPanelAlignParent
    {
      set
      {
        _dropDownPanelAlignParent = value;
      }
    }

    public UIFont Font { get; set; }

    public float ListWidth { get; set; }

    public Vector3 ListPosition
    {
      get
      {
        return _listPosition;
      }
      set
      {
        _listPosition = value;
      }
    }

    public bool ShowPanel
    {
      get
      {
        return _showPanel;
      }
      set
      {
        _showPanel = value;
      }
    }

    public int MaxVisibleItems
    {
      get
      {
        return _maxVisibleItems;
      }
      set
      {
        _maxVisibleItems = Math.Max(1, value);
      }
    }

    public int SelectedIndex
    {
      get
      {
        int num = -1;
        if (_items == null || _items.Length == 0)
          return num;
        for (int index = 0; index < _items.Length; ++index)
        {
          if (_items[index].IsSelected)
          {
            num = index;
            break;
          }
        }
        return num;
      }
      set
      {
        if (_items == null || _items.Length == 0)
          return;
        _items[value].IsSelected = true;
      }
    }

    public ushort SelectedItem
    {
      get
      {
        ushort num = 0;
        if (_items == null || _items.Length == 0)
          return num;
        for (int index = 0; index < _items.Length; ++index)
        {
          DropDownRow dropDownRow = _items[index];
          if (dropDownRow.IsSelected)
            num = dropDownRow.ID;
        }
        return num;
      }
      set
      {
        if (_items == null || _items.Length == 0 || (int) SelectedItem == (int) value)
          return;
        Text = "Default";
        for (int index = 0; index < _items.Length; ++index)
        {
          DropDownRow dropDownRow = _items[index];
          if ((int) dropDownRow.ID == (int) value)
          {
            dropDownRow.IsSelected = true;
            Text = dropDownRow.Text;
          }
          else
            dropDownRow.IsSelected = false;
        }
        OnSelectedItemChanged();
      }
    }

    public string Text
    {
      get
      {
        return _triggerButton.text;
      }
      set
      {
        if (!_showPanel)
        {
          _triggerButton.text = "";
        }
        else
        {
          if (!(value != _triggerButton.text))
            return;
          _triggerButton.text = value;
        }
      }
    }

    public DropDownRow[] Items
    {
      get
      {
        return _items;
      }
    }

    public event PropertyChangedEventHandler<ushort> eventSelectedItemChanged;

    public void ClearItems()
    {
      if (_items == null)
        return;
      for (int index = 0; index < _items.Length; ++index)
        UnityEngine.Object.Destroy((UnityEngine.Object) _items[index].gameObject);
      _items = (DropDownRow[]) null;
      Text = "Default";
      _scrollablePanel.scrollPosition = new Vector2(0.0f, 0.0f);
    }

    public void AddItem(ushort ID, Func<ushort, string> func)
    {
      if (_items == null)
        _items = new DropDownRow[0];
      DropDownRow[] dropDownRowArray = new DropDownRow[_items.Length + 1];
      Array.Copy((Array) _items, (Array) dropDownRowArray, _items.Length);
      DropDownRow dropDownRow = _scrollablePanel.AddUIComponent<DropDownRow>();
      if ((UnityEngine.Object) Font != (UnityEngine.Object) null)
        dropDownRow.Font = Font;
      dropDownRow.ID = ID;
      dropDownRow.IDToNameFunc = func;
      dropDownRow.eventClick += new MouseEventHandler(OnRowClick);
      dropDownRowArray[_items.Length] = dropDownRow;
      _items = dropDownRowArray;
    }

    public void AddItems(ushort[] IDs, Func<ushort, string> func)
    {
      List<DropDownRow> dropDownRowList = new List<DropDownRow>();
      foreach (ushort id in IDs)
      {
        DropDownRow dropDownRow = _scrollablePanel.AddUIComponent<DropDownRow>();
        if ((UnityEngine.Object) Font != (UnityEngine.Object) null)
          dropDownRow.Font = Font;
        dropDownRow.ID = id;
        dropDownRow.IDToNameFunc = func;
        dropDownRow.eventClick += new MouseEventHandler(OnRowClick);
        dropDownRowList.Add(dropDownRow);
      }
      _items = dropDownRowList.ToArray();
    }

    private void OnRowClick(UIComponent component, UIMouseEventParameter eventParam)
    {
      for (int index = 0; index < _items.Length; ++index)
        _items[index].IsSelected = false;
      DropDownRow dropDownRow = component as DropDownRow;
      dropDownRow.IsSelected = !dropDownRow.IsSelected;
      Text = dropDownRow.Text;
      _dropDownPanel.isVisible = false;
      OnSelectedItemChanged();
    }

    private void OnButtonClick(UIComponent component, UIMouseEventParameter eventParam)
    {
      if (_items == null || _items.Length == 0)
        return;
      if (_dropDownPanel.isVisible)
      {
        _dropDownPanel.Hide();
      }
      else
      {
        _dropDownPanel.Show();
        int num1;
        float num2 = (float) (num1 = Math.Min(_items.Length, _maxVisibleItems)) * (float) _itemHeight;
        _dropDownPanel.height = num2;
        _scrollablePanel.height = num2;
        int maxVisibleItems = _maxVisibleItems;
        if (num1 < maxVisibleItems)
        {
          _scrollablePanel.width = (double) ListWidth > 0.0 ? ListWidth : width;
          UpdateRowWidth();
          _scrollbarPanel.Hide();
        }
        else
        {
          _scrollablePanel.width = (float) (((double) ListWidth > 0.0 ? (double) ListWidth : (double) width) - 10.0);
          UpdateRowWidth();
          _scrollbarPanel.Show();
          EnsureVisible(SelectedIndex);
        }
      }
    }

    private void UpdateRowWidth()
    {
      for (int index = 0; index < _items.Length; ++index)
        _items[index].width = _scrollablePanel.width;
    }

    public void EnsureVisible(int index)
    {
      int num = index * _itemHeight;
      if ((double) _scrollbar.value > (double) num)
        _scrollbar.value = (float) num;
      if ((double) _scrollbar.value + (double) _itemHeight >= (double) (num + _itemHeight))
        return;
      _scrollbar.value = (float) num - (float) _itemHeight + (float) _itemHeight;
    }

    private void CheckForPopupClose()
    {
      if ((UnityEngine.Object) _dropDownPanel == (UnityEngine.Object) null || !Input.GetMouseButtonDown(0))
        return;
      Ray ray = GetCamera().ScreenPointToRay(Input.mousePosition);
      if (_dropDownPanel.Raycast(ray) || _triggerButton.Raycast(ray))
        return;
      _dropDownPanel.Hide();
    }

    protected internal virtual void OnSelectedItemChanged()
    {
      // ISSUE: reference to a compiler-generated field
      if (eventSelectedItemChanged != null)
      {
        // ISSUE: reference to a compiler-generated field
        eventSelectedItemChanged((UIComponent) this, SelectedItem);
      }
      InvokeUpward("OnSelectedItemChanged", (object) SelectedItem);
    }

    public static DropDown Create(UIComponent parent)
    {
      return parent.AddUIComponent<DropDown>();
    }

    public override void Update()
    {
      base.Update();
      if (isVisible)
      {
        if (SelectedIndex > -1)
          Text = _items[SelectedIndex].Text;
        if (_listPosition == Vector3.zero)
          _dropDownPanel.absolutePosition = absolutePosition + new Vector3(0.0f, height);
        else
          _dropDownPanel.absolutePosition = _listPosition;
      }
      CheckForPopupClose();
    }

    public override void Start()
    {
      base.Start();
      if (_showPanel)
      {
        backgroundSprite = "ButtonMenu";
        eventMouseEnter += (MouseEventHandler) ((component, param) => (component as DropDown).backgroundSprite = "ButtonMenuHovered");
        eventMouseLeave += (MouseEventHandler) ((component, param) => (component as DropDown).backgroundSprite = "ButtonMenu");
      }
      zOrder = 1;
      UIButton uiButton = AddUIComponent<UIButton>();
      uiButton.width = width;
      uiButton.height = height;
      uiButton.relativePosition = new Vector3(0.0f, 0.0f);
      if ((UnityEngine.Object) Font != (UnityEngine.Object) null)
        uiButton.font = Font;
      uiButton.textScale = 0.8f;
      uiButton.textVerticalAlignment = UIVerticalAlignment.Middle;
      uiButton.textHorizontalAlignment = UIHorizontalAlignment.Left;
      uiButton.textPadding = new RectOffset(6, 0, 4, 0);
      uiButton.normalFgSprite = "IconDownArrow";
      uiButton.hoveredFgSprite = "IconDownArrowHovered";
      uiButton.pressedFgSprite = "IconDownArrowPressed";
      uiButton.focusedFgSprite = "IconDownArrow";
      uiButton.disabledFgSprite = "IconDownArrowDisabled";
      uiButton.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
      uiButton.horizontalAlignment = UIHorizontalAlignment.Right;
      uiButton.verticalAlignment = UIVerticalAlignment.Middle;
      uiButton.zOrder = 0;
      uiButton.eventClick += new MouseEventHandler(OnButtonClick);
      _triggerButton = uiButton;
      Text = "Default";
      UIPanel uiPanel1 = AddUIComponent(typeof (UIPanel)) as UIPanel;
      uiPanel1.name = "PopUpPanel";
      uiPanel1.isVisible = false;
      uiPanel1.width = (double) ListWidth > 0.0 ? ListWidth : width;
      uiPanel1.height = (float) (_itemHeight * _maxVisibleItems);
      uiPanel1.autoLayoutDirection = LayoutDirection.Horizontal;
      uiPanel1.autoLayoutStart = LayoutStart.TopLeft;
      uiPanel1.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
      uiPanel1.autoLayout = true;
      uiPanel1.backgroundSprite = "GenericPanelWhite";
      uiPanel1.color = (Color32) Color.black;
      uiPanel1.AlignTo(_dropDownPanelAlignParent, UIAlignAnchor.TopLeft);
      _dropDownPanel = uiPanel1;
      UIScrollablePanel uiScrollablePanel = uiPanel1.AddUIComponent<UIScrollablePanel>();
      uiScrollablePanel.width = uiPanel1.width - 10f;
      uiScrollablePanel.height = uiPanel1.height;
      uiScrollablePanel.autoLayoutDirection = LayoutDirection.Vertical;
      uiScrollablePanel.autoLayoutStart = LayoutStart.TopLeft;
      uiScrollablePanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
      uiScrollablePanel.autoLayout = true;
      uiScrollablePanel.clipChildren = true;
      uiScrollablePanel.backgroundSprite = "GenericPanelWhite";
      uiScrollablePanel.color = (Color32) Color.black;
      _scrollablePanel = uiScrollablePanel;
      UIPanel uiPanel2 = uiPanel1.AddUIComponent<UIPanel>();
      uiPanel2.width = 10f;
      uiPanel2.height = uiPanel2.parent.height;
      uiPanel2.autoLayoutDirection = LayoutDirection.Horizontal;
      uiPanel2.autoLayoutStart = LayoutStart.TopLeft;
      uiPanel2.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
      uiPanel2.autoLayout = true;
      _scrollbarPanel = uiPanel2;
      UIScrollbar scrollbar = uiPanel2.AddUIComponent<UIScrollbar>();
      scrollbar.width = 10f;
      scrollbar.height = uiPanel2.height;
      scrollbar.orientation = UIOrientation.Vertical;
      scrollbar.pivot = UIPivotPoint.BottomLeft;
      scrollbar.AlignTo((UIComponent) uiPanel2, UIAlignAnchor.TopRight);
      scrollbar.minValue = 0.0f;
      scrollbar.value = 0.0f;
      scrollbar.incrementAmount = (float) _itemHeight;
      _scrollbar = scrollbar;
      UISlicedSprite uiSlicedSprite1 = scrollbar.AddUIComponent<UISlicedSprite>();
      uiSlicedSprite1.relativePosition = (Vector3) Vector2.zero;
      uiSlicedSprite1.autoSize = true;
      uiSlicedSprite1.size = uiSlicedSprite1.parent.size;
      uiSlicedSprite1.fillDirection = UIFillDirection.Vertical;
      uiSlicedSprite1.spriteName = "ScrollbarTrack";
      scrollbar.trackObject = (UIComponent) uiSlicedSprite1;
      UISlicedSprite uiSlicedSprite2 = uiSlicedSprite1.AddUIComponent<UISlicedSprite>();
      uiSlicedSprite2.relativePosition = (Vector3) Vector2.zero;
      uiSlicedSprite2.fillDirection = UIFillDirection.Vertical;
      uiSlicedSprite2.autoSize = true;
      uiSlicedSprite2.width = uiSlicedSprite2.parent.width - 4f;
      uiSlicedSprite2.spriteName = "ScrollbarThumb";
      scrollbar.thumbObject = (UIComponent) uiSlicedSprite2;
      _scrollablePanel.verticalScrollbar = scrollbar;
      _scrollablePanel.eventMouseWheel += (MouseEventHandler) ((component, param) => _scrollablePanel.scrollPosition += new Vector2(0.0f, Mathf.Sign(param.wheelDelta) * -1f * scrollbar.incrementAmount));
    }

    public override void OnDestroy()
    {
      if ((UnityEngine.Object) _triggerButton != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) _triggerButton.gameObject);
      if ((UnityEngine.Object) _dropDownPanel != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) _dropDownPanel.gameObject);
      if ((UnityEngine.Object) _scrollablePanel != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) _scrollablePanel.gameObject);
      if ((UnityEngine.Object) _scrollbarPanel != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) _scrollbarPanel.gameObject);
      if ((UnityEngine.Object) _scrollbar != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) _scrollbar.gameObject);
      base.OnDestroy();
    }
  }
}
