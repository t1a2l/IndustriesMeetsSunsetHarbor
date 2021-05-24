using System;

namespace FishIndustryEnhanced
{
    class AlgaeBioreactorAI : PowerPlantAI
    {
		void Start()
		{
			var Algae_Bioreactor = PrefabCollection<BuildingInfo>.FindLoaded("(Fish) Algae Bioreactor.Algae Bioreactor_Data");
			Algae_Bioreactor.m_placementMode = BuildingInfo.PlacementMode.Roadside;
		}

        public override string GetLocalizedStats(ushort buildingID, ref Building data)
		{
			int electricityRate = this.GetElectricityRate(buildingID, ref data);
			string text = LocaleFormatter.FormatGeneric("AIINFO_ELECTRICITY_PRODUCTION", new object[]
			{
				(electricityRate * 16 + 500) / 1000
			});
			if (this.m_resourceType == TransferManager.TransferReason.Coal)
			{
				text += Environment.NewLine;
				int resourceDuration = this.GetResourceDuration(buildingID, ref data);
				text += LocaleFormatter.FormatGeneric("AIINFO_COAL_STORED", new object[]
				{
					resourceDuration
				});
			}
			else if (this.m_resourceType == TransferManager.TransferReason.Petrol)
			{
				text += Environment.NewLine;
				int resourceDuration2 = this.GetResourceDuration(buildingID, ref data);
				text += LocaleFormatter.FormatGeneric("AIINFO_OIL_STORED", new object[]
				{
					resourceDuration2
				});
			}
			else if (this.m_resourceType == TransferManager.TransferReason.Grain)
			{
				text += Environment.NewLine;
				int resourceDuration3 = this.GetResourceDuration(buildingID, ref data);
				text += "Crops stored for " + resourceDuration3 + "weeks";
			}
			return text;
		}
    }
}
