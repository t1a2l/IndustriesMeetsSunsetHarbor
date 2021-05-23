using ColossalFramework;
using ICities;
using System;
using System.Reflection;

namespace FishIndustryEnhanced
{
    public class ThreadingExtension : ThreadingExtensionBase
    {
        int b = Singleton<SimulationManager>.instance.m_randomizer.Int32(10000u);
        int a = 0;

        public override void OnAfterSimulationTick()
        {
            ReloadIfRequired();
        }

        private void ReloadIfRequired()
        {
            if (a == b) return;
            a = b;

            try
            {
                Building[] buffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
                for (ushort i = 0; i < buffer.Length; i++)
                {

                    if (buffer[i].Info == null) continue;

                    if (buffer[i].Info.name.Equals("Algae Bioreactor"))
                    {
                        AIHelper.ApplyNewAIToBuilding(Singleton<BuildingManager>.instance.m_buildings.m_buffer[i]);
                    }
                    else if (buffer[i].Info.name.Equals("Aquaculture Farm - Algae Tanks"))
                    {
                        AIHelper.ApplyNewAIToBuilding(Singleton<BuildingManager>.instance.m_buildings.m_buffer[i]);
                    }
                    else if (buffer[i].Info.name.Equals("Fish Hatchery - Long") || buffer[i].Info.name.Equals("Fish Hatchery - Wide"))
                    {
                        AIHelper.ApplyNewAIToBuilding(Singleton<BuildingManager>.instance.m_buildings.m_buffer[i]);
                    }
                }

                LogHelper.Information("Reloaded Mod");
            }
            catch (Exception e)
            {
                LogHelper.Information(e.ToString());
            }
        }
    } 
}