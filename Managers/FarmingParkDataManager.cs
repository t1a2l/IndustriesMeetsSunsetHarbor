using System.Collections.Generic;

namespace IndustriesMeetsSunsetHarbor.Managers
{
    public static class FarmingParkDataManager
    {
        public static Dictionary<byte, FarmingParkData> FarmingParks;

        public struct FarmingParkData
        {
            // Raw materials
            public DistrictAreaResourceData m_fruitsData;
            public DistrictAreaResourceData m_vegetablesData;
            public DistrictAreaResourceData m_cottonData;

            // Animals
            public DistrictAreaResourceData m_cowsData;
            public DistrictAreaResourceData m_highlandCowsData;
            public DistrictAreaResourceData m_sheepData;
            public DistrictAreaResourceData m_pigsData;

            // Products
            public DistrictAreaResourceData m_beefMeatData;
            public DistrictAreaResourceData m_cowMilkData;
            public DistrictAreaResourceData m_highlandBeefData;
            public DistrictAreaResourceData m_highlandMilkData;
            public DistrictAreaResourceData m_lambMeatData;
            public DistrictAreaResourceData m_sheepMilkData;
            public DistrictAreaResourceData m_woolData;
            public DistrictAreaResourceData m_porkMeatData;
        }

        public static void Init()
        {
            FarmingParks = [];
        }

        public static void Deinit()
        {
            FarmingParks = [];
        }

        public static FarmingParkData GetFarmingPark(byte parkID)
        {
            if (!FarmingParks.TryGetValue(parkID, out FarmingParkData farmingPark_struct))
            {
                FarmingParks.Add(parkID, farmingPark_struct);
            }
            return farmingPark_struct;
        }

        public static void SetCustomBuffer(byte parkID, FarmingParkData farmingPark_struct)
        {
            FarmingParks[parkID] = farmingPark_struct;
        }
    }
}
