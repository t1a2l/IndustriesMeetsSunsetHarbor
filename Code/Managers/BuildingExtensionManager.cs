using ICities;

namespace IndustriesMeetsSunsetHarbor.Managers
{

    public class BuildingExtensionManager : BuildingExtensionBase
    {

        public override void OnBuildingCreated(ushort id)
        {
            base.OnBuildingCreated(id);
            if (!Mod.inGame)
            {
                return;
            }
            AquacultureFarmManager.ObserveBuilding(id);
        }

        public override void OnBuildingReleased(ushort id)
        {
            base.OnBuildingReleased(id);
            if (!Mod.inGame)
            {
                return;
            }
            foreach (var aquacultureFarm in AquacultureFarmManager.AquacultureFarms)
            {
                if (!aquacultureFarm.Value.Remove(id))
                {
                    continue;
                }
                AquacultureFarmManager.GetStats(ref BuildingManager.instance.m_buildings.m_buffer[id], out BuildingInfo primaryInfo);
                AquacultureFarmManager.OnReleasedForInfo(id, primaryInfo);
            }
        }
    }
}
