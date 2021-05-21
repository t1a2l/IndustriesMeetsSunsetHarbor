
namespace FishIndustryEnhanced.FishPark
{
    static class FishDistrictPark
    {
		public static DistrictAreaResourceData m_grainData;
		public static DistrictAreaResourceData m_fishData;
		public static DistrictAreaResourceData m_logsData;
		public static DistrictAreaResourceData m_oreData;
		public static DistrictAreaResourceData m_oilData;
		public static DistrictAreaResourceData m_animalProductsData;
		public static DistrictAreaResourceData m_floursData;
		public static DistrictAreaResourceData m_paperData;
		public static DistrictAreaResourceData m_planedTimberData;
		public static DistrictAreaResourceData m_petroleumData;
		public static DistrictAreaResourceData m_plasticsData;
		public static DistrictAreaResourceData m_glassData;
		public static DistrictAreaResourceData m_metalsData;
		public static DistrictAreaResourceData m_luxuryProductsData;
		public static void AddProductionAmountFish(this DistrictPark districtPark, TransferManager.TransferReason material, int amount)
		{
			switch (material)
			{
			case TransferManager.TransferReason.AnimalProducts:
				m_animalProductsData.m_tempConsumption = m_animalProductsData.m_tempConsumption + (uint)amount;
				break;
			case TransferManager.TransferReason.Flours:
				m_floursData.m_tempConsumption = m_floursData.m_tempConsumption + (uint)amount;
				break;
			case TransferManager.TransferReason.Paper:
				m_paperData.m_tempConsumption = m_paperData.m_tempConsumption + (uint)amount;
				break;
			case TransferManager.TransferReason.PlanedTimber:
				m_planedTimberData.m_tempConsumption = m_planedTimberData.m_tempConsumption + (uint)amount;
				break;
			case TransferManager.TransferReason.Petroleum:
				m_petroleumData.m_tempConsumption = m_petroleumData.m_tempConsumption + (uint)amount;
				break;
			case TransferManager.TransferReason.Plastics:
				m_plasticsData.m_tempConsumption = m_plasticsData.m_tempConsumption + (uint)amount;
				break;
			case TransferManager.TransferReason.Glass:
				m_glassData.m_tempConsumption = m_glassData.m_tempConsumption + (uint)amount;
				break;
			case TransferManager.TransferReason.Metals:
				m_metalsData.m_tempConsumption = m_metalsData.m_tempConsumption + (uint)amount;
				break;
			case TransferManager.TransferReason.LuxuryProducts:
				m_luxuryProductsData.m_tempConsumption = m_luxuryProductsData.m_tempConsumption + (uint)amount;
				break;
			case TransferManager.TransferReason.Fish:
				m_fishData.m_tempConsumption = m_fishData.m_tempConsumption + (uint)amount;
				break;
			default:
				switch (material)
				{
				case TransferManager.TransferReason.Oil:
					m_oilData.m_tempConsumption = m_oilData.m_tempConsumption + (uint)amount;
					break;
				case TransferManager.TransferReason.Ore:
					m_oreData.m_tempConsumption = m_oreData.m_tempConsumption + (uint)amount;
					break;
				case TransferManager.TransferReason.Logs:
					m_logsData.m_tempConsumption = m_logsData.m_tempConsumption + (uint)amount;
					break;
				case TransferManager.TransferReason.Grain:
					m_grainData.m_tempConsumption = m_grainData.m_tempConsumption + (uint)amount;
					break;
				}
				break;
			}
		}

		public static void AddExportAmountFish(this DistrictPark districtPark, TransferManager.TransferReason material, int amount)
		{
			switch (material)
			{
			case TransferManager.TransferReason.AnimalProducts:
				m_animalProductsData.m_tempExport = m_animalProductsData.m_tempExport + (uint)amount;
				break;
			case TransferManager.TransferReason.Flours:
				m_floursData.m_tempExport = m_floursData.m_tempExport + (uint)amount;
				break;
			case TransferManager.TransferReason.Paper:
				m_paperData.m_tempExport = m_paperData.m_tempExport + (uint)amount;
				break;
			case TransferManager.TransferReason.PlanedTimber:
				m_planedTimberData.m_tempExport = m_planedTimberData.m_tempExport + (uint)amount;
				break;
			case TransferManager.TransferReason.Petroleum:
				m_petroleumData.m_tempExport = m_petroleumData.m_tempExport + (uint)amount;
				break;
			case TransferManager.TransferReason.Plastics:
				m_plasticsData.m_tempExport = m_plasticsData.m_tempExport + (uint)amount;
				break;
			case TransferManager.TransferReason.Glass:
				m_glassData.m_tempExport = m_glassData.m_tempExport + (uint)amount;
				break;
			case TransferManager.TransferReason.Metals:
				m_metalsData.m_tempExport = m_metalsData.m_tempExport + (uint)amount;
				break;
			case TransferManager.TransferReason.LuxuryProducts:
				m_luxuryProductsData.m_tempExport = m_luxuryProductsData.m_tempExport + (uint)amount;
				break;
			case TransferManager.TransferReason.Fish:
				m_fishData.m_tempExport = m_fishData.m_tempExport + (uint)amount;
				break;
			default:
				switch (material)
				{
				case TransferManager.TransferReason.Oil:
					m_oilData.m_tempExport = m_oilData.m_tempExport + (uint)amount;
					break;
				case TransferManager.TransferReason.Ore:
					m_oreData.m_tempExport = m_oreData.m_tempExport + (uint)amount;
					break;
				case TransferManager.TransferReason.Logs:
					m_logsData.m_tempExport = m_logsData.m_tempExport + (uint)amount;
					break;
				case TransferManager.TransferReason.Grain:
					m_grainData.m_tempExport = m_grainData.m_tempExport + (uint)amount;
					break;
				}
				break;
			}
		}

		public static void AddBufferStatusFish(this DistrictPark districtPark, TransferManager.TransferReason material, int amount, int incoming, int capacity)
		{
			switch (material)
			{
			case TransferManager.TransferReason.AnimalProducts:
				m_animalProductsData.Add(amount, incoming, capacity);
				break;
			case TransferManager.TransferReason.Flours:
				m_floursData.Add(amount, incoming, capacity);
				break;
			case TransferManager.TransferReason.Paper:
				m_paperData.Add(amount, incoming, capacity);
				break;
			case TransferManager.TransferReason.PlanedTimber:
				m_planedTimberData.Add(amount, incoming, capacity);
				break;
			case TransferManager.TransferReason.Petroleum:
				m_petroleumData.Add(amount, incoming, capacity);
				break;
			case TransferManager.TransferReason.Plastics:
				m_plasticsData.Add(amount, incoming, capacity);
				break;
			case TransferManager.TransferReason.Glass:
				m_glassData.Add(amount, incoming, capacity);
				break;
			case TransferManager.TransferReason.Metals:
				m_metalsData.Add(amount, incoming, capacity);
				break;
			case TransferManager.TransferReason.LuxuryProducts:
				m_luxuryProductsData.Add(amount, incoming, capacity);
				break;
			case TransferManager.TransferReason.Fish:
				m_fishData.Add(amount, incoming, capacity);
				break;
			default:
				switch (material)
				{
				case TransferManager.TransferReason.Oil:
					m_oilData.Add(amount, incoming, capacity);
					break;
				case TransferManager.TransferReason.Ore:
					m_oreData.Add(amount, incoming, capacity);
					break;
				case TransferManager.TransferReason.Logs:
					m_logsData.Add(amount, incoming, capacity);
					break;
				case TransferManager.TransferReason.Grain:
					m_grainData.Add(amount, incoming, capacity);
					break;
				}
				break;
			}
		}
	}
}
