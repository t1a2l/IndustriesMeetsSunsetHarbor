using ColossalFramework.Globalization;
using ColossalFramework.UI;
using System.Reflection;

namespace FishIndustryEnhanced
{

    public class WarehousePanelExtender : WarehouseWorldInfoPanel
    {
		internal UIDropDown m_dropdownResource  => (UIDropDown)typeof(WarehouseWorldInfoPanel).GetField("m_dropdownResource", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
		internal UIDropDown m_dropdownMode  => (UIDropDown)typeof(WarehouseWorldInfoPanel).GetField("m_dropdownMode", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
   
		protected override void Start()
        {
			base.Start();
			this.MyRefreshDropdownLists();
			LocaleManager.eventLocaleChanged += this.MyRefreshDropdownLists;
        }


        private TransferManager.TransferReason[] m_transferReasons = new TransferManager.TransferReason[16]
        {
            TransferManager.TransferReason.None,
            TransferManager.TransferReason.Fish,
            TransferManager.TransferReason.AnimalProducts,
            TransferManager.TransferReason.Flours,
            TransferManager.TransferReason.Paper,
            TransferManager.TransferReason.PlanedTimber,
            TransferManager.TransferReason.Petroleum,
            TransferManager.TransferReason.Plastics,
            TransferManager.TransferReason.Glass,
            TransferManager.TransferReason.Metals,
            TransferManager.TransferReason.LuxuryProducts,
            TransferManager.TransferReason.Lumber,
            TransferManager.TransferReason.Food,
            TransferManager.TransferReason.Coal,
            TransferManager.TransferReason.Petrol,
            TransferManager.TransferReason.Goods
        };

        private enum WarehouseMode
		{
			Balanced,
			Import,
			Export
		}

        private WarehousePanelExtender.WarehouseMode[] m_warehouseModes = new WarehousePanelExtender.WarehouseMode[]
	    {
		    WarehousePanelExtender.WarehouseMode.Balanced,
		    WarehousePanelExtender.WarehouseMode.Import,
		    WarehousePanelExtender.WarehouseMode.Export
	    };


        private void MyRefreshDropdownLists()
	    {
			
		    string[] array = new string[m_transferReasons.Length];
		    for (int i = 0; i < m_transferReasons.Length; i++)
		    {
			    string text = Locale.Get("WAREHOUSEPANEL_RESOURCE", m_transferReasons[i].ToString());
			    array[i] = text;
		    }
		    this.m_dropdownResource.items = array;
		    array = new string[this.m_warehouseModes.Length];
		    for (int j = 0; j < this.m_warehouseModes.Length; j++)
		    {
			    string text2 = Locale.Get("WAREHOUSEPANEL_MODE", this.m_warehouseModes[j].ToString());
			    array[j] = text2;
		    }
		    this.m_dropdownMode.items = array;
	    }

    }
    
}
