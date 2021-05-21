using System;
using System.Collections;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace FishIndustryEnhanced
{

    public class WarehousePanelExtender : WarehouseWorldInfoPanel
    {
        private bool _initialized;
		private UIDropDown m_dropdownResource;
        private UIDropDown m_dropdownMode;

		protected override void Start(){
			  base.Start(); // big base start method.
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


        private void RefreshDropdownLists()
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

		

        public void Update()
        {
            if (!_initialized)
            {
                RefreshDropdownLists();
                this.m_dropdownResource.eventSelectedIndexChanged += OnDropdownResourceChanged;
                this.m_dropdownMode.eventSelectedIndexChanged += OnDropdownModeChanged;
                _initialized = true;
            }
        }

        private void OnDropdownResourceChanged(UIComponent component, int index)
	    {
		    WarehouseAI ai = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)this.m_InstanceID.Building].Info.m_buildingAI as WarehouseAI;
		    Singleton<SimulationManager>.instance.AddAction(delegate()
		    {
			    ai.SetTransferReason(this.m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)this.m_InstanceID.Building], this.m_transferReasons[index]);
		    });
	    }

        private void OnDropdownModeChanged(UIComponent component, int index)
	    {
		    this.warehouseMode = this.m_warehouseModes[index];
	    }

        private WarehousePanelExtender.WarehouseMode warehouseMode
		{
			get
			{
				if ((Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)this.m_InstanceID.Building].m_flags & Building.Flags.Filling) == Building.Flags.None && (Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)this.m_InstanceID.Building].m_flags & Building.Flags.Downgrading) == Building.Flags.None)
				{
					return WarehousePanelExtender.WarehouseMode.Balanced;
				}
				if ((Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)this.m_InstanceID.Building].m_flags & Building.Flags.Downgrading) != Building.Flags.None)
				{
					return WarehousePanelExtender.WarehouseMode.Export;
				}
				return WarehousePanelExtender.WarehouseMode.Import;
			}
			set
			{
				if (value != WarehousePanelExtender.WarehouseMode.Balanced)
				{
					if (value != WarehousePanelExtender.WarehouseMode.Export)
					{
						if (value == WarehousePanelExtender.WarehouseMode.Import)
						{
							Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)this.m_InstanceID.Building].Info.m_buildingAI.SetEmptying(this.m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)this.m_InstanceID.Building], false);
							Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)this.m_InstanceID.Building].Info.m_buildingAI.SetFilling(this.m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)this.m_InstanceID.Building], true);
						}
					}
					else
					{
						Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)this.m_InstanceID.Building].Info.m_buildingAI.SetEmptying(this.m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)this.m_InstanceID.Building], true);
						Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)this.m_InstanceID.Building].Info.m_buildingAI.SetFilling(this.m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)this.m_InstanceID.Building], false);
					}
				}
				else
				{
					Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)this.m_InstanceID.Building].Info.m_buildingAI.SetEmptying(this.m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)this.m_InstanceID.Building], false);
					Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)this.m_InstanceID.Building].Info.m_buildingAI.SetFilling(this.m_InstanceID.Building, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)this.m_InstanceID.Building], false);
				}
			}
		}
    }
    
}
