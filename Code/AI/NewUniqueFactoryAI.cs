using ColossalFramework;

namespace IndustriesMeetsSunsetHarbor.AI
{
    class NewUniqueFactoryAI : NewProcessingFacilityAI
    {
        public override void PlacementSucceeded()
	{
	    BuildingTypeGuide uniqueFactoryNotUsed = Singleton<BuildingManager>.instance.m_uniqueFactoryNotUsed;
	    if (uniqueFactoryNotUsed != null)
	    {
		uniqueFactoryNotUsed.Disable();
	    }
	}

        public override void UpdateGuide(GuideController guideController)
	{
	    BuildingTypeGuide uniqueFactoryNotUsed = Singleton<BuildingManager>.instance.m_uniqueFactoryNotUsed;
	    if (uniqueFactoryNotUsed != null)
	    {
		uniqueFactoryNotUsed.Activate(guideController.m_uniqueFactoryNotUsed, this.m_info);
	    }
	    base.UpdateGuide(guideController);
	}

        public override bool CanBeBuiltOnlyOnce()
	{
	    return true;
	}
    }
}
