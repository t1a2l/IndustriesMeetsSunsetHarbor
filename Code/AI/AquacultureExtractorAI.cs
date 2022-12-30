using ColossalFramework;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.AI
{
    class AquacultureExtractorAI :  PlayerBuildingAI
    {

        public override Color GetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode)
	{
	    switch (infoMode)
	    {
	        case InfoManager.InfoMode.Fishing:
		    if ((data.m_flags & Building.Flags.Active) != 0)
		    {
			    return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
		    }
		    return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_inactiveColor;
	        default:
		        return base.GetColor(buildingID, ref data, infoMode, subInfoMode);
	    }
	}

        public override void GetPlacementInfoMode(out InfoManager.InfoMode mode, out InfoManager.SubInfoMode subMode, float elevation)
	{
	    mode = InfoManager.InfoMode.Fishing;
	    subMode = InfoManager.SubInfoMode.WaterPower;
	}

        protected override string GetLocalizedStatusActive(ushort buildingID, ref Building data)
	{
	    if ((data.m_flags & Building.Flags.RateReduced) != 0)
	    {
		return ColossalFramework.Globalization.Locale.Get("BUILDING_STATUS_REDUCED");
	    }
	    return ColossalFramework.Globalization.Locale.Get("BUILDING_STATUS_DEFAULT");
	}

        public override void ReleaseBuilding(ushort buildingID, ref Building data)
	{
	    base.ReleaseBuilding(buildingID, ref data);
	    if (!Singleton<UnlockManager>.instance.m_properties.m_ServicePolicyMilestones[28].IsPassed() && m_info.m_class.m_service == ItemClass.Service.Fishing && m_info.m_class.m_subService == ItemClass.SubService.None && m_info.m_class.m_level == ItemClass.Level.Level3)
	    {
		DistrictManager instance = Singleton<DistrictManager>.instance;
		for (int i = 0; i < instance.m_districts.m_size; i++)
		{
		    instance.m_districts.m_buffer[i].m_servicePolicies &= ~DistrictPolicies.Services.AlgaeBasedWaterFiltering;
		}
		instance.NamesModified();
	    }
	}

        public override bool RequireRoadAccess()
	{
	    return false;
	}
    }
}
