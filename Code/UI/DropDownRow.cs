using ColossalFramework.UI;
using System;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.UI
{
    public class DropDownRow : UIPanel
    {
        private bool _init;
        private UILabel _label;
        private ushort _ID;
        private bool _isSelected;
        private Func<ushort, string> _idToNameFunc;

        public UIFont Font { get; set; }

        public Func<ushort, string> IDToNameFunc
        {
            set
            {
                _idToNameFunc = value;
            }
        }

        public ushort ID
        {
            get
            {
                return _ID;
            }
            set
            {
                _ID = value;
            }
        }

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                if (_isSelected)
                    backgroundSprite = "ListItemHighlight";
                else
                    backgroundSprite = "";
            }
        }

        public string Text
        {
            get
            {
                if (!_init)
                    Start();
                string text = _idToNameFunc(_ID);
                if (Truncate(_label, text, "…"))
                    tooltip = text;
                else
                    tooltip = "";
                return _label.text;
            }
        }

        protected override void OnMouseEnter(UIMouseEventParameter p)
        {
            if (!IsSelected)
                backgroundSprite = "ListItemHover";
            base.OnMouseEnter(p);
        }

        protected override void OnMouseLeave(UIMouseEventParameter p)
        {
            if (!IsSelected)
                backgroundSprite = "";
            base.OnMouseLeave(p);
        }

        public override void Update()
        {
            if (isVisible && !UIUtils.IsFullyClippedFromParent((UIComponent)this))
                _label.text = Text;
            base.Update();
        }

        public override void Start()
        {
            if (_init)
                return;
            _init = true;
            base.Start();
            width = parent.width - 10f;
            height = 27f;
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayoutStart = LayoutStart.TopLeft;
            autoLayoutPadding = new RectOffset(4, 0, 0, 0);
            autoLayout = true;
            _label = AddUIComponent<UILabel>();
            _label.text = "";
            _label.textScale = 0.8f;
            _label.font = Font;
            _label.autoSize = false;
            _label.height = height;
            _label.width = width - (float)autoLayoutPadding.left;
            _label.verticalAlignment = UIVerticalAlignment.Middle;
        }

        public override void OnDestroy()
        {
            if (_label != null)
                Destroy(_label.gameObject);
            base.OnDestroy();
        }

        public static bool Truncate(UILabel label, string text, string suffix = "…")
        {
            bool flag = false;
            try
            {
                using (UIFontRenderer renderer = label.ObtainRenderer())
                {
                    float units = label.GetUIView().PixelsToUnits();
                    float[] characterWidths = renderer.GetCharacterWidths(text);
                    float num1 = 0.0f;
                    float num2 = (float)((double)label.width - (double)label.padding.horizontal - 2.0);
                    for (int index = 0; index < characterWidths.Length; ++index)
                    {
                        num1 += characterWidths[index] / units;
                        if ((double)num1 > (double)num2)
                        {
                            flag = true;
                            text = text.Substring(0, index - 3) + suffix;
                            break;
                        }
                    }
                }
                label.text = text;
            }
            catch
            {
                flag = false;
            }
            return flag;
        }
    }
}
