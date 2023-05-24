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
            int num = (amount * GetResourcePrice(material, info.m_class.m_service) + 50) / 100;
            if (material == (byte)TransferManager.TransferReason.Fish && ((cityPlanningPolicies & DistrictPolicies.CityPlanning.SustainableFishing) != 0 || (cityPlanningPolicies2 & DistrictPolicies.CityPlanning.SustainableFishing) != 0))
            {
                num = (num * 105 + 99) / 100;
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

        public static int GetResourcePrice(byte material, ItemClass.Service sourceService = ItemClass.Service.None)
        {
            return UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Science, material switch
            {
                (byte)TransferManager.TransferReason.Grain => 200,
                (byte)TransferManager.TransferReason.Logs => 200,
                (byte)TransferManager.TransferReason.Oil => 400,
                (byte)TransferManager.TransferReason.Ore => 300,
                (byte)TransferManager.TransferReason.Food => 0,
                (byte)TransferManager.TransferReason.Lumber => 0,
                (byte)TransferManager.TransferReason.Petrol => 0,
                (byte)TransferManager.TransferReason.Coal => 0,
                (byte)TransferManager.TransferReason.Goods => (sourceService == ItemClass.Service.Fishing) ? 1500 : 0,
                (byte)TransferManager.TransferReason.AnimalProducts => 1500,
                (byte)TransferManager.TransferReason.Flours => 1500,
                (byte)TransferManager.TransferReason.Paper => 1500,
                (byte)TransferManager.TransferReason.PlanedTimber => 1500,
                (byte)TransferManager.TransferReason.Petroleum => 3000,
                (byte)TransferManager.TransferReason.Plastics => 3000,
                (byte)TransferManager.TransferReason.Glass => 2250,
                (byte)TransferManager.TransferReason.Metals => 2250,
                (byte)TransferManager.TransferReason.LuxuryProducts => 10000,
                (byte)TransferManager.TransferReason.Fish => 600,
                (byte)ExtendedTransferManager.TransferReason.Bread => 1500,
                (byte)ExtendedTransferManager.TransferReason.FoodSupplies => 1500,
                (byte)ExtendedTransferManager.TransferReason.DrinkSupplies => 1500,
                _ => 0,
            });
        }

    }
}
