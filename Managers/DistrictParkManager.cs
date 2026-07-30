using TransferManagerCore;

namespace IndustriesMeetsSunsetHarbor.Managers
{
    public static class DistrictParkManager
    {
        public static void AddConsumptionAmount(byte parkID, CustomTransferReason.Reason material, int amount)
        {
            var data = FarmingParkDataManager.GetFarmingPark(parkID);

            switch (material)
            {
                case CustomTransferReason.Reason.Fruits:
                    data.m_fruitsData.m_tempConsumption += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Vegetables:
                    data.m_vegetablesData.m_tempConsumption += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Cotton:
                    data.m_cottonData.m_tempConsumption += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Cows:
                    data.m_cowsData.m_tempConsumption += (uint)amount;
                    break;
                case CustomTransferReason.Reason.HighlandCows:
                    data.m_highlandCowsData.m_tempConsumption += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Sheep:
                    data.m_sheepData.m_tempConsumption += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Pigs:
                    data.m_pigsData.m_tempConsumption += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Milk:
                    data.m_milkData.m_tempConsumption += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Wool:
                    data.m_woolData.m_tempConsumption += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Pork:
                    data.m_porkData.m_tempConsumption += (uint)amount;
                    break;
            }

            FarmingParkDataManager.SetCustomBuffer(parkID, data);
        }

        public static void AddProductionAmount(byte parkID, CustomTransferReason.Reason material, int amount)
        {
            var data = FarmingParkDataManager.GetFarmingPark(parkID);

            switch (material)
            {
                case CustomTransferReason.Reason.Fruits:
                    data.m_fruitsData.m_tempProduction += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Vegetables:
                    data.m_vegetablesData.m_tempProduction += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Cotton:
                    data.m_cottonData.m_tempProduction += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Cows:
                    data.m_cowsData.m_tempProduction += (uint)amount;
                    break;
                case CustomTransferReason.Reason.HighlandCows:
                    data.m_highlandCowsData.m_tempProduction += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Sheep:
                    data.m_sheepData.m_tempProduction += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Pigs:
                    data.m_pigsData.m_tempProduction += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Milk:
                    data.m_milkData.m_tempProduction += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Wool:
                    data.m_woolData.m_tempProduction += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Pork:
                    data.m_porkData.m_tempProduction += (uint)amount;
                    break;
            }

            FarmingParkDataManager.SetCustomBuffer(parkID, data);
        }

        public static void AddBufferStatus(byte parkID, CustomTransferReason.Reason material, int amount, int incoming, int capacity)
        {
            var data = FarmingParkDataManager.GetFarmingPark(parkID);

            switch (material)
            {
                case CustomTransferReason.Reason.Fruits:
                    data.m_fruitsData.Add(amount, incoming, capacity);
                    break;
                case CustomTransferReason.Reason.Vegetables:
                    data.m_vegetablesData.Add(amount, incoming, capacity);
                    break;
                case CustomTransferReason.Reason.Cotton:
                    data.m_cottonData.Add(amount, incoming, capacity);
                    break;
                case CustomTransferReason.Reason.Cows:
                    data.m_cowsData.Add(amount, incoming, capacity);
                    break;
                case CustomTransferReason.Reason.HighlandCows:
                    data.m_highlandCowsData.Add(amount, incoming, capacity);
                    break;
                case CustomTransferReason.Reason.Sheep:
                    data.m_sheepData.Add(amount, incoming, capacity);
                    break;
                case CustomTransferReason.Reason.Pigs:
                    data.m_pigsData.Add(amount, incoming, capacity);
                    break;
                case CustomTransferReason.Reason.Milk:
                    data.m_milkData.Add(amount, incoming, capacity);
                    break;
                case CustomTransferReason.Reason.Wool:
                    data.m_woolData.Add(amount, incoming, capacity);
                    break;
                case CustomTransferReason.Reason.Pork:
                    data.m_porkData.Add(amount, incoming, capacity);
                    break;
            }

            FarmingParkDataManager.SetCustomBuffer(parkID, data);
        }

        public static void AddImportAmount(byte parkID, CustomTransferReason.Reason material, int amount)
        {
            var data = FarmingParkDataManager.GetFarmingPark(parkID);

            switch (material)
            {
                case CustomTransferReason.Reason.Fruits:
                    data.m_fruitsData.m_tempImport += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Vegetables:
                    data.m_vegetablesData.m_tempImport += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Cotton:
                    data.m_cottonData.m_tempImport += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Cows:
                    data.m_cowsData.m_tempImport += (uint)amount;
                    break;
                case CustomTransferReason.Reason.HighlandCows:
                    data.m_highlandCowsData.m_tempImport += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Sheep:
                    data.m_sheepData.m_tempImport += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Pigs:
                    data.m_pigsData.m_tempImport += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Milk:
                    data.m_milkData.m_tempImport += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Wool:
                    data.m_woolData.m_tempImport += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Pork:
                    data.m_porkData.m_tempImport += (uint)amount;
                    break;
            }

            FarmingParkDataManager.SetCustomBuffer(parkID, data);
        }

        public static void AddExportAmount(byte parkID, CustomTransferReason.Reason material, int amount)
        {
            var data = FarmingParkDataManager.GetFarmingPark(parkID);

            switch (material)
            {
                case CustomTransferReason.Reason.Fruits:
                    data.m_fruitsData.m_tempExport += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Vegetables:
                    data.m_vegetablesData.m_tempExport += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Cotton:
                    data.m_cottonData.m_tempExport += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Cows:
                    data.m_cowsData.m_tempExport += (uint)amount;
                    break;
                case CustomTransferReason.Reason.HighlandCows:
                    data.m_highlandCowsData.m_tempExport += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Sheep:
                    data.m_sheepData.m_tempExport += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Pigs:
                    data.m_pigsData.m_tempExport += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Milk:
                    data.m_milkData.m_tempExport += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Wool:
                    data.m_woolData.m_tempExport += (uint)amount;
                    break;
                case CustomTransferReason.Reason.Pork:
                    data.m_porkData.m_tempExport += (uint)amount;
                    break;
            }

            FarmingParkDataManager.SetCustomBuffer(parkID, data);
        }
    }
}
