using ColossalFramework;
using UnityEngine;
using MoreTransferReasons;

namespace IndustriesMeetsSunsetHarbor.Managers
{
    public class IndustryBuildingManager
    {
        public static void ExchangeResource(byte material, int amount, ushort sourceBuilding, ushort targetBuilding)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            BuildingManager instance2 = Singleton<BuildingManager>.instance;
            BuildingInfo info = instance2.m_buildings.m_buffer[sourceBuilding].Info;
            BuildingInfo info2 = instance2.m_buildings.m_buffer[targetBuilding].Info;
            byte industryArea = GetIndustryArea(sourceBuilding);
            byte industryArea2 = GetIndustryArea(targetBuilding);
            Vector3 position = instance2.m_buildings.m_buffer[sourceBuilding].m_position;
            Vector3 position2 = instance2.m_buildings.m_buffer[targetBuilding].m_position;
            byte district = instance.GetDistrict(position);
            byte district2 = instance.GetDistrict(position2);
            DistrictPolicies.CityPlanning cityPlanningPolicies = instance.m_districts.m_buffer[district].m_cityPlanningPolicies;
            DistrictPolicies.CityPlanning cityPlanningPolicies2 = instance.m_districts.m_buffer[district2].m_cityPlanningPolicies;
            if (industryArea != industryArea2)
            {
                if (material < 200)
                {
                    if (industryArea != 0)
                    {
                        instance.m_parks.m_buffer[industryArea].AddExportAmount((TransferManager.TransferReason)material, amount);
                    }
                    if (industryArea2 != 0)
                    {
                        instance.m_parks.m_buffer[industryArea2].AddImportAmount((TransferManager.TransferReason)material, amount);
                    }
                }

            }
            int num;
            if (material >= 200)
            {
                byte transferType = (byte)(material - 200);
                num = (amount * GetExtendedResourcePrice((ExtendedTransferManager.TransferReason)transferType, info.m_class.m_service) + 50) / 100;
            }
            else
            {
                num = (amount * GetResourcePrice((TransferManager.TransferReason)material, info.m_class.m_service) + 50) / 100;
                if (material == (byte)TransferManager.TransferReason.Fish && ((cityPlanningPolicies & DistrictPolicies.CityPlanning.SustainableFishing) != 0 || (cityPlanningPolicies2 & DistrictPolicies.CityPlanning.SustainableFishing) != 0))
                {
                    num = (num * 105 + 99) / 100;
                }
            }
            if (num == 0)
            {
                return;
            }
            Building.Flags flags = instance2.m_buildings.m_buffer[sourceBuilding].m_flags;
            Building.Flags flags2 = instance2.m_buildings.m_buffer[targetBuilding].m_flags;
            ItemClass.Service service = info.m_class.m_service;
            ItemClass.Service service2 = info2.m_class.m_service;
            if (ItemClass.GetPublicServiceIndex(info.m_class.m_service) != -1 && (flags & Building.Flags.IncomingOutgoing) == 0)
            {
                if (ItemClass.GetPublicServiceIndex(info2.m_class.m_service) == -1 || info2.m_class.m_service == ItemClass.Service.ServicePoint || (flags2 & Building.Flags.IncomingOutgoing) != 0)
                {
                    if (service != ItemClass.Service.PublicTransport)
                    {
                        Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.ResourcePrice, num, info.m_class);
                    }
                }
                else if (service == ItemClass.Service.PublicTransport)
                {
                    if (service2 != ItemClass.Service.PublicTransport)
                    {
                        Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.ResourcePrice, num, info2.m_class);
                    }
                }
                else if (service2 == ItemClass.Service.PublicTransport)
                {
                    Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.ResourcePrice, num, info.m_class);
                }
                else if (industryArea != industryArea2)
                {
                    Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.ResourcePrice, num, info.m_class);
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.ResourcePrice, num, info2.m_class);
                }
            }
            else if ((ItemClass.GetPublicServiceIndex(info2.m_class.m_service) != -1 || info2.m_class.m_service == ItemClass.Service.ServicePoint) && (flags2 & Building.Flags.IncomingOutgoing) == 0 && service2 != ItemClass.Service.PublicTransport)
            {
                Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.ResourcePrice, num, info2.m_class);
            }
        }

        private static byte GetIndustryArea(ushort buildingID)
        {
            if (buildingID == 0)
            {
                return 0;
            }
            BuildingManager instance = Singleton<BuildingManager>.instance;
            BuildingInfo info = instance.m_buildings.m_buffer[buildingID].Info;
            if ((object)info == null)
            {
                return 0;
            }
            IndustryBuildingAI industryBuildingAI = info.m_buildingAI as IndustryBuildingAI;
            if ((object)industryBuildingAI == null)
            {
                WarehouseAI warehouseAI = info.m_buildingAI as WarehouseAI;
                if ((object)warehouseAI == null)
                {
                    return 0;
                }
            }
            DistrictManager instance2 = Singleton<DistrictManager>.instance;
            byte park = instance2.GetPark(instance.m_buildings.m_buffer[buildingID].m_position);
            if (park == 0 || !instance2.m_parks.m_buffer[park].IsIndustry)
            {
                return 0;
            }
            if ((object)industryBuildingAI != null && (industryBuildingAI.m_industryType == DistrictPark.ParkType.Industry || industryBuildingAI.m_industryType != instance2.m_parks.m_buffer[park].m_parkType))
            {
                return 0;
            }
            return park;
        }

        public static int GetResourcePrice(TransferManager.TransferReason material, ItemClass.Service sourceService = ItemClass.Service.None)
        {
            return UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Science, material switch
            {
                TransferManager.TransferReason.Grain => 200,
                TransferManager.TransferReason.Logs => 200,
                TransferManager.TransferReason.Oil => 400,
                TransferManager.TransferReason.Ore => 300,
                TransferManager.TransferReason.Food => 0,
                TransferManager.TransferReason.Lumber => 0,
                TransferManager.TransferReason.Petrol => 0,
                TransferManager.TransferReason.Coal => 0,
                TransferManager.TransferReason.Goods => (sourceService == ItemClass.Service.Fishing) ? 1500 : 0,
                TransferManager.TransferReason.AnimalProducts => 1500,
                TransferManager.TransferReason.Flours => 1500,
                TransferManager.TransferReason.Paper => 1500,
                TransferManager.TransferReason.PlanedTimber => 1500,
                TransferManager.TransferReason.Petroleum => 3000,
                TransferManager.TransferReason.Plastics => 3000,
                TransferManager.TransferReason.Glass => 2250,
                TransferManager.TransferReason.Metals => 2250,
                TransferManager.TransferReason.LuxuryProducts => 10000,
                TransferManager.TransferReason.Fish => 600,
                _ => 0,
            });
        }

        public static int GetExtendedResourcePrice(ExtendedTransferManager.TransferReason material, ItemClass.Service sourceService = ItemClass.Service.None)
        {
            return UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Science, material switch
            {
                ExtendedTransferManager.TransferReason.Bread => 1500,
                ExtendedTransferManager.TransferReason.FoodSupplies => 1500,
                ExtendedTransferManager.TransferReason.DrinkSupplies => 1500,
                _ => 0,
            });
        }

        public static InfoManager.SubInfoMode ResourceToInfoMode(byte material)
        {
            switch (material)
            {
                case (byte)TransferManager.TransferReason.Grain:
                case (byte)TransferManager.TransferReason.Food:
                case (byte)TransferManager.TransferReason.AnimalProducts:
                case (byte)TransferManager.TransferReason.Flours:
                    return InfoManager.SubInfoMode.WaterPower;
                case (byte)TransferManager.TransferReason.Logs:
                case (byte)TransferManager.TransferReason.Lumber:
                case (byte)TransferManager.TransferReason.Paper:
                case (byte)TransferManager.TransferReason.PlanedTimber:
                    return InfoManager.SubInfoMode.Default;
                case (byte)TransferManager.TransferReason.Oil:
                case (byte)TransferManager.TransferReason.Petrol:
                case (byte)TransferManager.TransferReason.Petroleum:
                case (byte)TransferManager.TransferReason.Plastics:
                    return InfoManager.SubInfoMode.PipeWater;
                case (byte)TransferManager.TransferReason.Ore:
                case (byte)TransferManager.TransferReason.Coal:
                case (byte)TransferManager.TransferReason.Glass:
                case (byte)TransferManager.TransferReason.Metals:
                    return InfoManager.SubInfoMode.WindPower;
                default:
                    return InfoManager.SubInfoMode.None;
            }
        }

        public static string FormatResourceWithUnit(uint amount, TransferManager.TransferReason type)
        {
	    return string.Concat(str2: (type != TransferManager.TransferReason.Oil && type != TransferManager.TransferReason.Petroleum && type != TransferManager.TransferReason.Petrol) ? ColossalFramework.Globalization.Locale.Get("RESOURCEUNIT_TONS") : ColossalFramework.Globalization.Locale.Get("RESOURCEUNIT_BARRELS"), str0: FormatResource(amount), str1: " ");
        }

        public static string FormatExtendedResourceWithUnit(uint amount, ExtendedTransferManager.TransferReason type)
        {
	    return string.Concat(str2: ColossalFramework.Globalization.Locale.Get("RESOURCEUNIT_TONS"), str0: FormatResource(amount), str1: " ");
        }

        public static string FormatResource(ulong amount)
        {
	    float num = amount;
	    num /= 1000f;
	    return Mathf.Round(num).ToString();
        }

        public static string ResourceSpriteName(ExtendedTransferManager.TransferReason transferReason)
	{
	    return transferReason.ToString();
	}

        public static Color GetExtendedResourceColor(ExtendedTransferManager.TransferReason resource)
        {
	    switch (resource)
	    {
		case ExtendedTransferManager.TransferReason.MealsDeliveryLow:
                case ExtendedTransferManager.TransferReason.MealsDeliveryMedium:
                case ExtendedTransferManager.TransferReason.MealsDeliveryHigh:
                    return Color.Lerp(Color.magenta, Color.black, 0.2f);
                case ExtendedTransferManager.TransferReason.MealsLow:
                case ExtendedTransferManager.TransferReason.MealsMedium:
                case ExtendedTransferManager.TransferReason.MealsHigh:
                    return Color.Lerp(Color.cyan, Color.black, 0.2f);
                case ExtendedTransferManager.TransferReason.DrinkSupplies:
                    return Color.Lerp(Color.white, Color.black, 0.2f);
                case ExtendedTransferManager.TransferReason.FoodSupplies:
                    return Color.Lerp(Color.green, Color.red, 0.5f);
                case ExtendedTransferManager.TransferReason.Bread:
                    return Color.Lerp(Color.red, Color.yellow, 0.5f);
                case ExtendedTransferManager.TransferReason.CannedFish:
                    return Color.Lerp(Color.cyan, Color.blue, 0.5f);
	    };
            return Color.Lerp(Color.grey, Color.black, 0.2f);;
        }
    }
}
