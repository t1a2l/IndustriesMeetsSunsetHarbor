using ICities;
using IndustriesMeetsSunsetHarbor.AI;

namespace IndustriesMeetsSunsetHarbor.Managers
{

    public class BuildingExtensionManager : BuildingExtensionBase
    {

        public override void OnBuildingCreated(ushort buildingId)
        {
            base.OnBuildingCreated(buildingId);
            if (!IndustriesMeetsSunsetHarborMod.inGame)
            {
                return;
            }
            AquacultureFarmManager.ObserveBuilding(buildingId);
            AquacultureExtractorManager.ObserveBuilding(buildingId);
        }

        public override void OnBuildingReleased(ushort buildingId)
        {
            base.OnBuildingReleased(buildingId);
            if (!IndustriesMeetsSunsetHarborMod.inGame)
            {
                return;
            }
            var building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            // if building is an extractor just remove from building list tied to farm
            if(buildingId != 0 && building.Info.GetAI() is AquacultureExtractorAI)
            {
                var aquacultureFarmId = AquacultureFarmManager.GetAquacultureFarm(buildingId);
                if(AquacultureFarmManager.AquacultureFarms.ContainsKey(aquacultureFarmId))
                {
                    AquacultureFarmManager.AquacultureFarms[aquacultureFarmId].Remove(buildingId);
                }
            }
            // if building is a farm remove all extractors from it and try to find them a new farm, then remove the current farm
            if(buildingId != 0 && building.Info.GetAI() is AquacultureFarmAI)
            {
                if(AquacultureFarmManager.AquacultureFarms.ContainsKey(buildingId))
                {
                    foreach (var aquacultureExtractorId in AquacultureFarmManager.AquacultureFarms[buildingId])
                    {
                        AquacultureFarmManager.AquacultureFarms[buildingId].Remove(aquacultureExtractorId);
                        var aquacultureFarmId = AquacultureFarmManager.GetAquacultureFarm(aquacultureExtractorId);
                        if(aquacultureFarmId == 0)
                        {
                            var newAquacultureFarmId = AquacultureFarmManager.GetClosestAquacultureFarm(aquacultureExtractorId);
                            if(newAquacultureFarmId != 0)
                            {
                                AquacultureFarmManager.AquacultureFarms[newAquacultureFarmId].Add(aquacultureExtractorId);
                            }
                        }
                    }
                    AquacultureFarmManager.AquacultureFarms.Remove(buildingId);
                }
                
            }

            
        }
    }
}
