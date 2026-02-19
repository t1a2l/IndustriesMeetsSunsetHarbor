using MoreTransferReasons;

namespace IndustriesMeetsSunsetHarbor.Managers
{
    public static class DistrictParkManager
    {
        public static void AddConsumptionAmount(byte parkID, TransferManager.TransferReason material, int amount)
        {
            var data = FarmingParkDataManager.GetFarmingPark(parkID);

            switch (material)
            {
                case ExtendedTransferManager.Fruits:
                    data.m_fruitsData.m_tempConsumption += (uint)amount;
                    break;
                case ExtendedTransferManager.Vegetables:
                    data.m_vegetablesData.m_tempConsumption += (uint)amount;
                    break;
                case ExtendedTransferManager.Cotton:
                    data.m_cottonData.m_tempConsumption += (uint)amount;
                    break;
                case ExtendedTransferManager.Cows:
                    data.m_cowsData.m_tempConsumption += (uint)amount;
                    break;
                case ExtendedTransferManager.HighlandCows:
                    data.m_highlandCowsData.m_tempConsumption += (uint)amount;
                    break;
                case ExtendedTransferManager.Sheep:
                    data.m_sheepData.m_tempConsumption += (uint)amount;
                    break;
                case ExtendedTransferManager.Pigs:
                    data.m_pigsData.m_tempConsumption += (uint)amount;
                    break;
                case ExtendedTransferManager.BeefMeat:
                    data.m_beefMeatData.m_tempConsumption += (uint)amount;
                    break;
                case ExtendedTransferManager.CowMilk:
                    data.m_cowMilkData.m_tempConsumption += (uint)amount;
                    break;
                case ExtendedTransferManager.HighlandBeefMeat:
                    data.m_highlandBeefData.m_tempConsumption += (uint)amount;
                    break;
                case ExtendedTransferManager.HighlandCowMilk:
                    data.m_highlandMilkData.m_tempConsumption += (uint)amount;
                    break;
                case ExtendedTransferManager.LambMeat:
                    data.m_lambMeatData.m_tempConsumption += (uint)amount;
                    break;
                case ExtendedTransferManager.SheepMilk:
                    data.m_sheepMilkData.m_tempConsumption += (uint)amount;
                    break;
                case ExtendedTransferManager.Wool:
                    data.m_woolData.m_tempConsumption += (uint)amount;
                    break;
                case ExtendedTransferManager.PorkMeat:
                    data.m_porkMeatData.m_tempConsumption += (uint)amount;
                    break;
            }

            FarmingParkDataManager.SetCustomBuffer(parkID, data);
        }

        public static void AddProductionAmount(byte parkID, TransferManager.TransferReason material, int amount)
        {
            var data = FarmingParkDataManager.GetFarmingPark(parkID);

            switch (material)
            {
                case ExtendedTransferManager.Fruits:
                    data.m_fruitsData.m_tempProduction += (uint)amount;
                    break;
                case ExtendedTransferManager.Vegetables:
                    data.m_vegetablesData.m_tempProduction += (uint)amount;
                    break;
                case ExtendedTransferManager.Cotton:
                    data.m_cottonData.m_tempProduction += (uint)amount;
                    break;
                case ExtendedTransferManager.Cows:
                    data.m_cowsData.m_tempProduction += (uint)amount;
                    break;
                case ExtendedTransferManager.HighlandCows:
                    data.m_highlandCowsData.m_tempProduction += (uint)amount;
                    break;
                case ExtendedTransferManager.Sheep:
                    data.m_sheepData.m_tempProduction += (uint)amount;
                    break;
                case ExtendedTransferManager.Pigs:
                    data.m_pigsData.m_tempProduction += (uint)amount;
                    break;
                case ExtendedTransferManager.BeefMeat:
                    data.m_beefMeatData.m_tempProduction += (uint)amount;
                    break;
                case ExtendedTransferManager.CowMilk:
                    data.m_cowMilkData.m_tempProduction += (uint)amount;
                    break;
                case ExtendedTransferManager.HighlandBeefMeat:
                    data.m_highlandBeefData.m_tempProduction += (uint)amount;
                    break;
                case ExtendedTransferManager.HighlandCowMilk:
                    data.m_highlandMilkData.m_tempProduction += (uint)amount;
                    break;
                case ExtendedTransferManager.LambMeat:
                    data.m_lambMeatData.m_tempProduction += (uint)amount;
                    break;
                case ExtendedTransferManager.SheepMilk:
                    data.m_sheepMilkData.m_tempProduction += (uint)amount;
                    break;
                case ExtendedTransferManager.Wool:
                    data.m_woolData.m_tempProduction += (uint)amount;
                    break;
                case ExtendedTransferManager.PorkMeat:
                    data.m_porkMeatData.m_tempProduction += (uint)amount;
                    break;
            }

            FarmingParkDataManager.SetCustomBuffer(parkID, data);
        }

        public static void AddBufferStatus(byte parkID, TransferManager.TransferReason material, int amount, int incoming, int capacity)
        {
            var data = FarmingParkDataManager.GetFarmingPark(parkID);

            switch (material)
            {
                case ExtendedTransferManager.Fruits:
                    data.m_fruitsData.Add(amount, incoming, capacity);
                    break;
                case ExtendedTransferManager.Vegetables:
                    data.m_vegetablesData.Add(amount, incoming, capacity);
                    break;
                case ExtendedTransferManager.Cotton:
                    data.m_cottonData.Add(amount, incoming, capacity);
                    break;
                case ExtendedTransferManager.Cows:
                    data.m_cowsData.Add(amount, incoming, capacity);
                    break;
                case ExtendedTransferManager.HighlandCows:
                    data.m_highlandCowsData.Add(amount, incoming, capacity);
                    break;
                case ExtendedTransferManager.Sheep:
                    data.m_sheepData.Add(amount, incoming, capacity);
                    break;
                case ExtendedTransferManager.Pigs:
                    data.m_pigsData.Add(amount, incoming, capacity);
                    break;
                case ExtendedTransferManager.BeefMeat:
                    data.m_beefMeatData.Add(amount, incoming, capacity);
                    break;
                case ExtendedTransferManager.CowMilk:
                    data.m_cowMilkData.Add(amount, incoming, capacity);
                    break;
                case ExtendedTransferManager.HighlandBeefMeat:
                    data.m_highlandBeefData.Add(amount, incoming, capacity);
                    break;
                case ExtendedTransferManager.HighlandCowMilk:
                    data.m_highlandMilkData.Add(amount, incoming, capacity);
                    break;
                case ExtendedTransferManager.LambMeat:
                    data.m_lambMeatData.Add(amount, incoming, capacity);
                    break;
                case ExtendedTransferManager.SheepMilk:
                    data.m_sheepMilkData.Add(amount, incoming, capacity);
                    break;
                case ExtendedTransferManager.Wool:
                    data.m_woolData.Add(amount, incoming, capacity);
                    break;
                case ExtendedTransferManager.PorkMeat:
                    data.m_porkMeatData.Add(amount, incoming, capacity);
                    break;
            }

            FarmingParkDataManager.SetCustomBuffer(parkID, data);
        }

        public static void AddImportAmount(byte parkID, TransferManager.TransferReason material, int amount)
        {
            var data = FarmingParkDataManager.GetFarmingPark(parkID);

            switch (material)
            {
                case ExtendedTransferManager.Fruits:
                    data.m_fruitsData.m_tempImport += (uint)amount;
                    break;
                case ExtendedTransferManager.Vegetables:
                    data.m_vegetablesData.m_tempImport += (uint)amount;
                    break;
                case ExtendedTransferManager.Cotton:
                    data.m_cottonData.m_tempImport += (uint)amount;
                    break;
                case ExtendedTransferManager.Cows:
                    data.m_cowsData.m_tempImport += (uint)amount;
                    break;
                case ExtendedTransferManager.HighlandCows:
                    data.m_highlandCowsData.m_tempImport += (uint)amount;
                    break;
                case ExtendedTransferManager.Sheep:
                    data.m_sheepData.m_tempImport += (uint)amount;
                    break;
                case ExtendedTransferManager.Pigs:
                    data.m_pigsData.m_tempImport += (uint)amount;
                    break;
                case ExtendedTransferManager.BeefMeat:
                    data.m_beefMeatData.m_tempImport += (uint)amount;
                    break;
                case ExtendedTransferManager.CowMilk:
                    data.m_cowMilkData.m_tempImport += (uint)amount;
                    break;
                case ExtendedTransferManager.HighlandBeefMeat:
                    data.m_highlandBeefData.m_tempImport += (uint)amount;
                    break;
                case ExtendedTransferManager.HighlandCowMilk:
                    data.m_highlandMilkData.m_tempImport += (uint)amount;
                    break;
                case ExtendedTransferManager.LambMeat:
                    data.m_lambMeatData.m_tempImport += (uint)amount;
                    break;
                case ExtendedTransferManager.SheepMilk:
                    data.m_sheepMilkData.m_tempImport += (uint)amount;
                    break;
                case ExtendedTransferManager.Wool:
                    data.m_woolData.m_tempImport += (uint)amount;
                    break;
                case ExtendedTransferManager.PorkMeat:
                    data.m_porkMeatData.m_tempImport += (uint)amount;
                    break;
            }

            FarmingParkDataManager.SetCustomBuffer(parkID, data);
        }

        public static void AddExportAmount(byte parkID, TransferManager.TransferReason material, int amount)
        {
            var data = FarmingParkDataManager.GetFarmingPark(parkID);

            switch (material)
            {
                case ExtendedTransferManager.Fruits:
                    data.m_fruitsData.m_tempExport += (uint)amount;
                    break;
                case ExtendedTransferManager.Vegetables:
                    data.m_vegetablesData.m_tempExport += (uint)amount;
                    break;
                case ExtendedTransferManager.Cotton:
                    data.m_cottonData.m_tempExport += (uint)amount;
                    break;
                case ExtendedTransferManager.Cows:
                    data.m_cowsData.m_tempExport += (uint)amount;
                    break;
                case ExtendedTransferManager.HighlandCows:
                    data.m_highlandCowsData.m_tempExport += (uint)amount;
                    break;
                case ExtendedTransferManager.Sheep:
                    data.m_sheepData.m_tempExport += (uint)amount;
                    break;
                case ExtendedTransferManager.Pigs:
                    data.m_pigsData.m_tempExport += (uint)amount;
                    break;
                case ExtendedTransferManager.BeefMeat:
                    data.m_beefMeatData.m_tempExport += (uint)amount;
                    break;
                case ExtendedTransferManager.CowMilk:
                    data.m_cowMilkData.m_tempExport += (uint)amount;
                    break;
                case ExtendedTransferManager.HighlandBeefMeat:
                    data.m_highlandBeefData.m_tempExport += (uint)amount;
                    break;
                case ExtendedTransferManager.HighlandCowMilk:
                    data.m_highlandMilkData.m_tempExport += (uint)amount;
                    break;
                case ExtendedTransferManager.LambMeat:
                    data.m_lambMeatData.m_tempExport += (uint)amount;
                    break;
                case ExtendedTransferManager.SheepMilk:
                    data.m_sheepMilkData.m_tempExport += (uint)amount;
                    break;
                case ExtendedTransferManager.Wool:
                    data.m_woolData.m_tempExport += (uint)amount;
                    break;
                case ExtendedTransferManager.PorkMeat:
                    data.m_porkMeatData.m_tempExport += (uint)amount;
                    break;
            }

            FarmingParkDataManager.SetCustomBuffer(parkID, data);
        }
    }
}
