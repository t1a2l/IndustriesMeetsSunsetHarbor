using ColossalFramework.UI;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.UI
{
    public class FarmingWorldInfoPanel : IndustryWorldInfoPanel
    {
        // ════════════════════════════════════════════════════════════════
        // SECTION 1 — Constants (ModTools measurements cited)
        // ════════════════════════════════════════════════════════════════

        // Columns  (930px wide panel, 122px boxes, 80px gaps)
        private const float C1 = 100f;    // Sheep / Grain
        private const float C2 = 300f;    // HC / Vegetables
        private const float C3 = 500f;    // Cows / Fruits
        private const float C4 = 700f;    // Pigs
        private const float COTTON_X = 820f;

        // Processing centres
        private const float WOOL_X = C1;
        private const float MILK_X = 280f;
        private const float MEAT_X = 460f;
        private const float PORK_X = C4;

        private const float PANEL_W = 930f;

        // Box dimensions [ModTools: StorageOil size x122 y64]
        private const float BW = 122f;
        private const float BH = 64f;

        // Row Y positions  (derived from original spacing rules)
        private const float INPUT_Y = 10f;
        private const float FEED_BUS_Y = 101f;
        private const float ANIMAL_Y = 150f;
        private const float PROC_BUS_Y = 240f;
        private const float PROC_Y = 290f;
        private const float OUTPUT_Y = 380f;

        // Connector dimensions
        private const float BUS_H = 27f;   // tiled bus line height
        private const float ARR_W = 9f;    // vertical arrow width

        // ════════════════════════════════════════════════════════════════
        // SECTION 2 — UI references
        // ════════════════════════════════════════════════════════════════

        private UIPanel m_origDiagram;
        private UIPanel m_farmDiagram;
        private UITextureAtlas m_atlas;

        // Labels to update in RefreshData()
        private UILabel m_woolTons, m_milkTons, m_meatTons, m_porkTons, m_cottonTons;
        private UILabel m_sheepExp, m_hcExp, m_cowsExp, m_pigsExp;

        // Buffer bars
        private UIProgressBar m_grainBuf, m_vegBuf, m_fruitsBuf, m_cottonBuf;
        private UIProgressBar m_sheepBuf, m_hcBuf, m_cowsBuf, m_pigsBuf;

        // ════════════════════════════════════════════════════════════════
        // Lifecycle
        // ════════════════════════════════════════════════════════════════

        protected override void Start()
        {
            base.Start();  // builds acquisition, tabs, stats

            m_atlas = GetIngameAtlas();

            m_origDiagram = Find<UIPanel>("Diagram");
            m_origDiagram?.isVisible = false;

            component.width = PANEL_W;
            component.height = 94f + OUTPUT_Y + BH + 20f;
            var tab = Find<UIPanel>("TabContainer");
            if (tab != null)
            {
                tab.width = PANEL_W;
                tab.height = component.height;
            }

            var tabStrip = Find<UITabstrip>("TabStrip");
            if (tabStrip != null)
                tabStrip.eventSelectedIndexChanged += OnTabChanged;

            BuildFarmingDiagram();
        }

        private void OnTabChanged(UIComponent c, int index)
        {
            // index 0 = Info tab, index 1 = Policies tab
            m_farmDiagram?.isVisible = (index == 0);
        }

        protected override void OnSetTarget()
        {
            base.OnSetTarget();
            m_origDiagram?.isVisible = false;
            m_farmDiagram?.isVisible = true;

            var tabStrip = Find<UITabstrip>("TabStrip");
            tabStrip?.selectedIndex = 0;
        }

        protected override void UpdateBindings()
        {
            base.UpdateBindings();
            RefreshData();
        }

        // ════════════════════════════════════════════════════════════════
        // Helpers
        // ════════════════════════════════════════════════════════════════

        private static UITextureAtlas GetIngameAtlas()
        {
            foreach (var a in Resources.FindObjectsOfTypeAll<UITextureAtlas>())
                if (a.name == "Ingame") return a;
            return UIView.GetAView().defaultAtlas;
        }

        // ════════════════════════════════════════════════════════════════
        // UI Construction — STEP 1: Inputs + Animals + Exports only
        // ════════════════════════════════════════════════════════════════

        private void BuildFarmingDiagram()
        {
            m_farmDiagram = component.AddUIComponent<UIPanel>();
            m_farmDiagram.autoLayout = false;
            m_farmDiagram.name = "FarmingDiagram";
            m_farmDiagram.relativePosition = new Vector3(0f, 94f);  // [ModTools: Diagram relPos]
            m_farmDiagram.size = new Vector2(PANEL_W, OUTPUT_Y + BH + 20f);

            BuildInputRow();
            BuildFeedBus();
            BuildAnimalRow();
            BuildProcBus();
            BuildProcessingRow();
            BuildOutputRow();
        }

        private void BuildInputRow()
        {
            var mtrAtlas = MoreTransferReasons.Utils.TextureUtils.GetAtlas("MoreTransferReasonsAtlas");
            MakeStorageBox(new GameObject(), "Grain", "Grain", UITextures.InGameAtlas, C1, INPUT_Y, out m_grainBuf);
            MakeStorageBox(new GameObject(), "Vegetables", "Vegetables", mtrAtlas, C2, INPUT_Y, out m_vegBuf);
            MakeStorageBox(new GameObject(), "Fruits", "Fruits", mtrAtlas, C3, INPUT_Y, out m_fruitsBuf);
            MakeStorageBox(new GameObject(), "Cotton", "Cotton", mtrAtlas, COTTON_X, INPUT_Y, out m_cottonBuf);

            // Arrows from Grain/Veg/Fruits down to feed bus
            float arrTop = INPUT_Y + BH;
            foreach (float x in new[] { C1, C2, C3 })
                MakeArrow(x, arrTop, FEED_BUS_Y - arrTop);

            // Cotton bypass — long arrow skipping everything, goes straight to OUTPUT_Y
            MakeArrow(COTTON_X, INPUT_Y + BH, OUTPUT_Y - (INPUT_Y + BH));
        }

        private void BuildFeedBus()
        {
            // Bus spans C1 left-edge to C4 right-edge
            float busLeft = C1 - BW / 2f;
            float busRight = C4 + BW / 2f;
            MakeBusLine(busLeft, FEED_BUS_Y, busRight - busLeft);  // [ModTools: ProcessRect]

            // Arrows from bus to animals
            float arrTop = FEED_BUS_Y + BUS_H;
            foreach (float x in new[] { C1, C2, C3, C4 })
                MakeArrow(x, arrTop, ANIMAL_Y - arrTop);  // [ModTools: WhiteRect]
        }

        private void BuildAnimalRow()
        {
            var atlas = MoreTransferReasons.Utils.TextureUtils.GetAtlas("MoreTransferReasonsAtlas");
            GameObject gameObjectSheep = new();
            GameObject gameObjectHighlandCows = new();
            GameObject gameObjectCows = new();
            GameObject gameObjectPigs = new();
            MakeStorageBox(gameObjectSheep, "Sheep", "Sheep", atlas, C1, ANIMAL_Y, out m_sheepBuf);
            MakeStorageBox(gameObjectHighlandCows, "Highland\nCows", "HighlandCows", atlas, C2, ANIMAL_Y, out m_hcBuf);
            MakeStorageBox(gameObjectCows, "Cows", "Cows", atlas, C3, ANIMAL_Y, out m_cowsBuf);
            MakeStorageBox(gameObjectPigs, "Pigs", "Pigs", atlas, C4, ANIMAL_Y, out m_pigsBuf);

            // Export branches — one for each animal
            GameObject gameObjectSheepExport = new();
            GameObject gameObjectHighlandCowsExport = new();
            GameObject gameObjectCowsExport = new();
            GameObject gameObjectPigsExport = new();
            MakeExportBranch(gameObjectSheepExport, C1, C1 + BW / 2f + 10f, "Sheep", out m_sheepExp);
            MakeExportBranch(gameObjectHighlandCowsExport, C2, C2 + BW / 2f + 10f, "HighlandCows", out m_hcExp);
            MakeExportBranch(gameObjectCowsExport, C3, C3 + BW / 2f + 10f, "Cows", out m_cowsExp);
            MakeExportBranch(gameObjectPigsExport, C4, C4 + BW / 2f + 10f, "Pigs", out m_pigsExp);
        }

        private void BuildProcessingRow()
        {
            var mtrAtlas = MoreTransferReasons.Utils.TextureUtils.GetAtlas("MoreTransferReasonsAtlas");
            MakeStorageBox(new GameObject(), "Wool", "Wool", mtrAtlas, WOOL_X, PROC_Y, out _);
            MakeStorageBox(new GameObject(), "Milk", "Milk", mtrAtlas, MILK_X, PROC_Y, out _);
            MakeStorageBox(new GameObject(), "Red Meat", "AnimalProducts", mtrAtlas, MEAT_X, PROC_Y, out _);
            MakeStorageBox(new GameObject(), "Pork", "Pork", mtrAtlas, PORK_X, PROC_Y, out _);

            // Arrows from processing boxes down to output row
            float procBottom = PROC_Y + BH;
            foreach (float x in new[] { WOOL_X, MILK_X, MEAT_X, PORK_X })
                MakeArrow(x, procBottom, OUTPUT_Y - procBottom);
        }

        private void BuildOutputRow()
        {
            MakeOutputBox(new GameObject(), WOOL_X, OUTPUT_Y, "Wool", "tons", out m_woolTons);
            MakeOutputBox(new GameObject(), MILK_X, OUTPUT_Y, "Milk", "tons", out m_milkTons);
            MakeOutputBox(new GameObject(), MEAT_X, OUTPUT_Y, "RedMeat", "tons", out m_meatTons);
            MakeOutputBox(new GameObject(), PORK_X, OUTPUT_Y, "Pork", "tons", out m_porkTons);
            MakeOutputBox(new GameObject(), COTTON_X, OUTPUT_Y, "Cotton", "tons", out m_cottonTons);
        }

        // ════════════════════════════════════════════════════════════════
        // Factories — every position from ModTools
        // ════════════════════════════════════════════════════════════════

        private UIPanel MakeStorageBox(GameObject gameObject, string label, string iconSprite, UITextureAtlas atlas, float cx, float topY, out UIProgressBar buffer)
        {
            gameObject = Instantiate(Find<UIPanel>("StorageOil").gameObject, m_farmDiagram.transform, false);
            var box = gameObject.GetComponent<UIPanel>();
            box.name = "Storage" + label.Replace("\n", "");
            box.anchor = UIAnchorStyle.None;
            box.relativePosition = new Vector3(cx - BW / 2f, topY);
            box.autoLayout = false;
            box.size = new Vector2(BW, BH);  // [ModTools: StorageOil size]
            box.backgroundSprite = "";

            // Icon [ModTools: ResourceIconOil relPos x46 y3, size x30 y30]
            var icon = gameObject.transform.Find("ResourceIconOil").GetComponent<UISprite>();
            icon.atlas = atlas;
            icon.spriteName = iconSprite;
            icon.name = "ResourceIcon" + label.Replace("\n", "");
            icon.relativePosition = new Vector3(46f, 3f);
            icon.size = new Vector2(30f, 30f);

            // Buffer bar [ModTools: OilBuffer relPos x29 y-29, size x64 y122]
            buffer = gameObject.transform.Find("OilBuffer").GetComponent<UIProgressBar>();
            buffer.name = label.Replace("\n", "") + "Buffer";
            buffer.relativePosition = new Vector3(29f, -29f);
            buffer.size = new Vector2(64f, 122f);

            // Label [ModTools: StorageOilLabel relPos x0 y35, textScale 0.8125]
            var lbl = gameObject.transform.Find("StorageOilLabel").GetComponent<UILabel>();
            lbl.text = label;
            lbl.name = "Storage" + label.Replace("\n", "") + "Label";
            lbl.relativePosition = new Vector3(0f, 35f);
            lbl.textScale = 0.8125f;
            lbl.textAlignment = UIHorizontalAlignment.Center;

            return box;
        }

        private void MakeArrow(float cx, float topY, float height)
        {
            var s = m_farmDiagram.AddUIComponent<UISprite>();
            s.atlas = UITextures.InGameAtlas;
            s.name = "Arrow";
            s.spriteName = "WhiteRect";   // [ModTools: Arrow spriteName]
            s.relativePosition = new Vector3(cx - ARR_W / 2f, topY);
            s.size = new Vector2(ARR_W, Mathf.Max(1f, height)); // [ModTools: Arrow size]
            s.color = new Color32(218, 255, 161, 255);

            var t = s.AddUIComponent<UISprite>();
            t.atlas = UITextures.InGameAtlas;
            t.name = "ArrowEnd";
            t.spriteName = "IconDownArrowProduction";   // [ModTools: IconDownArrowProduction spriteName]
            t.relativePosition = new Vector3(-4f, 14f);
            t.size = new Vector2(17f, 17f); // [ModTools: Arrow End size]
            t.color = new Color32(218, 255, 161, 255);
        }

        private void MakeBusLine(float leftX, float topY, float width)
        {
            var s = m_farmDiagram.AddUIComponent<UITiledSprite>();
            s.atlas = UITextures.InGameAtlas;
            s.spriteName = "Processrect";  // [ModTools: ArrowHorizontalLeft spriteName]
            s.name = "ArrowHorizontal";
            s.relativePosition = new Vector3(leftX, topY);
            s.size = new Vector2(Mathf.Max(1f, width), BUS_H); // [ModTools: size y]
            s.color = new Color32(218, 255, 161, 255);
        }

        private void MakeExportBranch(GameObject gameObject, float animalCX, float exportCX, string label, out UILabel headsLabel)
        {
            float stemMidY = ANIMAL_Y + 60f;
            float stemLeft = animalCX + BW / 2f;
            float stemTop = stemMidY - 4.5f;   // unified top Y for both lines

            // Horizontal stem — extends all the way to exportCX (center of vertical)
            var h = m_farmDiagram.AddUIComponent<UISprite>();
            h.atlas = UITextures.InGameAtlas;
            h.name = "ArrowSmallHorizontal";
            h.spriteName = "WhiteRect";
            h.relativePosition = new Vector3(stemLeft, stemTop);
            h.size = new Vector2(Mathf.Max(1f, exportCX - stemLeft), ARR_W); // 9px tall = same as vertical width
            h.color = new Color32(218, 255, 161, 255);

            // Vertical drop — starts at SAME Y as horizontal top → perfect L-corner
            var v = m_farmDiagram.AddUIComponent<UISprite>();
            v.atlas = UITextures.InGameAtlas;
            v.name = "ArrowSmallVertical";
            v.spriteName = "WhiteRect";
            v.relativePosition = new Vector3(exportCX - ARR_W / 2f, stemTop);  // same stemTop
            v.size = new Vector2(ARR_W, Mathf.Max(1f, OUTPUT_Y - stemTop));
            v.color = new Color32(218, 255, 161, 255);

            // Arrow tip at bottom of vertical
            var t = v.AddUIComponent<UISprite>();
            t.atlas = UITextures.InGameAtlas;
            t.name = "ArrowEnd";
            t.spriteName = "IconDownArrowProduction";
            t.relativePosition = new Vector3(-4f, OUTPUT_Y - stemTop - 17f);
            t.size = new Vector2(17f, 17f);
            t.color = new Color32(218, 255, 161, 255);

            MakeOutputBox(gameObject, exportCX, OUTPUT_Y, label, "heads", out headsLabel);
            headsLabel.parent.Find<UILabel>("Label").text = "Export";
        }

        private UIPanel MakeOutputBox(GameObject gameObject, float cx, float topY, string label, string unit, out UILabel valueLabel)
        {
            gameObject = Instantiate(Find<UIPanel>("Panel OilOutput").gameObject, m_farmDiagram.transform, false);
            var box = gameObject.GetComponent<UIPanel>();
            box.name = "Panel " + label.Replace("\n", "") + "Output";
            box.anchor = UIAnchorStyle.None;
            box.relativePosition = new Vector3(cx - BW / 2f, topY);
            box.size = new Vector2(BW, BH);
            box.backgroundSprite = "GenericPanelLight"; // [ModTools: Panel OilOutput bg]

            var title = box.transform.Find("Label").GetComponent<UILabel>();
            title.name = "Label";
            title.text = "Output";
            title.relativePosition = new Vector3(36f, 4f);  // [ModTools: Label relPos]
            title.textScale = 0.8125f;

            valueLabel = box.transform.Find("OilOutputPerWeek").GetComponent<UILabel>();
            valueLabel.name = label.Replace("\n", "") + "OutputPerWeek";
            valueLabel.text = $"0 {unit}";
            valueLabel.relativePosition = new Vector3(40f, 24f); // [ModTools: OilOutputPerWeek]
            valueLabel.textScale = 0.8125f;

            var income = box.transform.Find("OilIncomePerWeek").GetComponent<UILabel>();
            income.name = label.Replace("\n", "") + "IncomePerWeek";
            income.text = "₡0";
            income.relativePosition = new Vector3(3f, 44f); // [ModTools: OilIncomePerWeek]
            income.textScale = 0.8125f;
            income.textColor = new Color32(185, 221, 74, 255);

            return box;
        }

        private void BuildProcBus()
        {
            float animalBottom = ANIMAL_Y + BH;

            // Sheep / HC / Cows → 3-animal bus
            foreach (float x in new[] { C1, C2, C3 })
                MakeArrow(x, animalBottom, PROC_BUS_Y - animalBottom);

            // Pigs bypass the bus — direct longer arrow to PROC_Y
            MakeArrow(C4, animalBottom, PROC_Y - animalBottom);

            // 3-animal bus spans Sheep left-edge → Cows right-edge only (NOT Pigs)
            float busLeft = C1 - BW / 2f;
            float busRight = C3 + BW / 2f;
            MakeBusLine(busLeft, PROC_BUS_Y, busRight - busLeft);

            // Arrows from bus down to Wool / Milk / Meat processing boxes
            float busBottom = PROC_BUS_Y + BUS_H;
            foreach (float x in new[] { WOOL_X, MILK_X, MEAT_X })
                MakeArrow(x, busBottom, PROC_Y - busBottom);
        }

        

        // ════════════════════════════════════════════════════════════════
        // Data binding  (stub — replace with real data)
        // ════════════════════════════════════════════════════════════════

        private void RefreshData()
        {
            m_sheepExp.text = "0 heads";
            m_hcExp.text = "0 heads";
            m_cowsExp.text = "0 heads";
            m_pigsExp.text = "0 heads";

            m_woolTons.text = "0 tons";
            m_milkTons.text = "0 tons";
            m_meatTons.text = "0 tons";
            m_porkTons.text = "0 tons";
            m_cottonTons.text = "0 tons";
        }
    }
}

