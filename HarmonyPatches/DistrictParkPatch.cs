using ColossalFramework;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.Managers;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(DistrictPark))]
    public static class DistrictParkPatch
    {
        [HarmonyPatch(typeof(DistrictPark), "BaseSimulationStep")]
        [HarmonyPostfix]
        public static void BaseSimulationStep(byte parkID)
        {
            SimulationManager instance = Singleton<SimulationManager>.instance;
            if ((instance.m_currentFrameIndex & 0xFFF) >= 3840)
            {
                var data = FarmingParkDataManager.GetFarmingPark(parkID);
                data.m_fruitsData.Update();
                data.m_vegetablesData.Update();
                data.m_cottonData.Update();
                data.m_cowsData.Update();
                data.m_highlandCowsData.Update();
                data.m_sheepData.Update();
                data.m_pigsData.Update();
                data.m_beefMeatData.Update();
                data.m_cowMilkData.Update();
                data.m_highlandBeefData.Update();
                data.m_highlandMilkData.Update();
                data.m_lambMeatData.Update();
                data.m_sheepMilkData.Update();
                data.m_woolData.Update();
                data.m_porkMeatData.Update();

                data.m_fruitsData.Reset();
                data.m_vegetablesData.Reset();
                data.m_cottonData.Reset();
                data.m_cowsData.Reset();
                data.m_highlandCowsData.Reset();
                data.m_sheepData.Reset();
                data.m_pigsData.Reset();
                data.m_beefMeatData.Reset();
                data.m_cowMilkData.Reset();
                data.m_highlandBeefData.Reset();
                data.m_highlandMilkData.Reset();
                data.m_lambMeatData.Reset();
                data.m_sheepMilkData.Reset();
                data.m_woolData.Reset();
                data.m_porkMeatData.Reset();

                FarmingParkDataManager.SetCustomBuffer(parkID, data);
            }
            
        }

        [HarmonyPatch(typeof(DistrictPark), "BaseSimulationStep")]
        [HarmonyPostfix]
        public static void IndustrySimulationStep(byte parkID)
        {
            var data = FarmingParkDataManager.GetFarmingPark(parkID);

            if ((Singleton<SimulationManager>.instance.m_currentFrameIndex & 0xFFF) >= 3840)
            {
                data.m_fruitsData.Add(ref data.m_fruitsData);
                data.m_vegetablesData.Add(ref data.m_vegetablesData);
                data.m_cottonData.Add(ref data.m_cottonData);
                data.m_cowsData.Add(ref data.m_cowsData);
                data.m_highlandCowsData.Add(ref data.m_highlandCowsData);
                data.m_sheepData.Add(ref data.m_sheepData);
                data.m_pigsData.Add(ref data.m_pigsData);
                data.m_beefMeatData.Add(ref data.m_beefMeatData);
                data.m_cowMilkData.Add(ref data.m_cowMilkData);
                data.m_highlandBeefData.Add(ref data.m_highlandBeefData);
                data.m_highlandMilkData.Add(ref data.m_highlandMilkData);
                data.m_lambMeatData.Add(ref data.m_lambMeatData);
                data.m_sheepMilkData.Add(ref data.m_sheepMilkData);
                data.m_woolData.Add(ref data.m_woolData);
                data.m_porkMeatData.Add(ref data.m_porkMeatData);

                data.m_fruitsData.Update();
                data.m_vegetablesData.Update();
                data.m_cottonData.Update();
                data.m_cowsData.Update();
                data.m_highlandCowsData.Update();
                data.m_sheepData.Update();
                data.m_pigsData.Update();
                data.m_beefMeatData.Update();
                data.m_cowMilkData.Update();
                data.m_highlandBeefData.Update();
                data.m_highlandMilkData.Update();
                data.m_lambMeatData.Update();
                data.m_sheepMilkData.Update();
                data.m_woolData.Update();
                data.m_porkMeatData.Update();

                data.m_fruitsData.Reset();
                data.m_vegetablesData.Reset();
                data.m_cottonData.Reset();
                data.m_cowsData.Reset();
                data.m_highlandCowsData.Reset();
                data.m_sheepData.Reset();
                data.m_pigsData.Reset();
                data.m_beefMeatData.Reset();
                data.m_cowMilkData.Reset();
                data.m_highlandBeefData.Reset();
                data.m_highlandMilkData.Reset();
                data.m_lambMeatData.Reset();
                data.m_sheepMilkData.Reset();
                data.m_woolData.Reset();
                data.m_porkMeatData.Reset();

                FarmingParkDataManager.SetCustomBuffer(parkID, data);
            }
            else
            {
                data.m_fruitsData.ResetBuffers();
                data.m_vegetablesData.ResetBuffers();
                data.m_cottonData.ResetBuffers();
                data.m_cowsData.ResetBuffers();
                data.m_highlandCowsData.ResetBuffers();
                data.m_sheepData.ResetBuffers();
                data.m_pigsData.ResetBuffers();
                data.m_beefMeatData.ResetBuffers();
                data.m_cowMilkData.ResetBuffers();
                data.m_highlandBeefData.ResetBuffers();
                data.m_highlandMilkData.ResetBuffers();
                data.m_lambMeatData.ResetBuffers();
                data.m_sheepMilkData.ResetBuffers();
                data.m_woolData.ResetBuffers();
                data.m_porkMeatData.ResetBuffers();

                FarmingParkDataManager.SetCustomBuffer(parkID, data);
            }

        }
    }
}
