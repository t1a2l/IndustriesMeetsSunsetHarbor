﻿using ColossalFramework;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FishIndustryEnhanced
{
    public class FishIndustryEnhanced : IUserMod
    {

        string IUserMod.Name => "Fish Industry Enhanced Mod";

        string IUserMod.Description => "Enhance the fishing Industry";

    }

    public class LoadingExtension : LoadingExtensionBase
    {

        public override void OnLevelLoaded(LoadMode mode)
        {
            try
            {
                Building[] buffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
                for (ushort i = 0; i < buffer.Length; i++)
                {

                    if (buffer[i].Info == null) continue;

                    if (buffer[i].Info.m_class.name.Equals("Algae Bioreactor"))
                    {
                        AIHelper.ApplyNewAIToBuilding(Singleton<BuildingManager>.instance.m_buildings.m_buffer[i]);
                    }
                    else if (buffer[i].Info.m_class.name.Equals("Aquaculture Farm - Algae Tanks"))
                    {
                        AIHelper.ApplyNewAIToBuilding(Singleton<BuildingManager>.instance.m_buildings.m_buffer[i]);
                    }
                    else if (buffer[i].Info.m_class.name.Equals("Fish Hatchery - Long") || buffer[i].Info.m_class.name.Equals("Fish Hatchery - Wide"))
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

            LogHelper.Information("Loaded Mod");
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
        } 
    }
}
