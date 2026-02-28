using ColossalFramework;

namespace IndustriesMeetsSunsetHarbor.AI
{
    public class ExtendedUniqueFactoryAI : ExtendedProcessingFacilityAI
    {
        public override void PlacementSucceeded()
        {
            Singleton<BuildingManager>.instance.m_uniqueFactoryNotUsed?.Disable();
        }

        public override void UpdateGuide(GuideController guideController)
        {
            Singleton<BuildingManager>.instance.m_uniqueFactoryNotUsed?.Activate(guideController.m_uniqueFactoryNotUsed, m_info);
            base.UpdateGuide(guideController);
        }

        public override bool CanBeBuiltOnlyOnce()
        {
            return true;
        }
    }

}
