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
                if (!b.Info.m_class.name.Equals("Algae Bioreactor") && !b.Info.m_class.name.Equals("Aquaculture Farm - Algae Tanks") 
                    && !b.Info.m_class.name.Equals("Fish Hatchery - Long") && !b.Info.m_class.name.Equals("Fish Hatchery - Wide"))
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