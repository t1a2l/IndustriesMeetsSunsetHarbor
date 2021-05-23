using ColossalFramework;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FishIndustryEnhanced
{
    public class BuildingExtension : BuildingExtensionBase
    {
        public override void OnBuildingCreated(ushort id)
        {
            base.OnBuildingCreated(id);

            try
            {
                var b = Singleton<BuildingManager>.instance.m_buildings.m_buffer[id];
                var asset_name = b.Info.name;
                if (!asset_name.Equals("Algae Bioreactor") && !asset_name.Equals("Aquaculture Farm - Algae Tanks") 
                    && !asset_name.Equals("Fish Hatchery - Long") && !asset_name.Equals("Fish Hatchery - Wide"))
                return;

                AIHelper.ApplyNewAIToBuilding(b);
            }
            catch (Exception e)
            {
                LogHelper.Information(e.ToString());
            }
        }
    }
}