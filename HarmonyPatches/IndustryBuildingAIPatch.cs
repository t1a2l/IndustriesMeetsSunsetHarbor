using ColossalFramework;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.Managers;
using IndustriesMeetsSunsetHarbor.Utils;
using MoreTransferReasons;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(IndustryBuildingAI))]
    public static class IndustryBuildingAIPatch
    {
        [HarmonyPatch(typeof(IndustryBuildingAI), "ExchangeResource")]
        [HarmonyPrefix]
        public static bool ExchangeResource(IndustryBuildingAI __instance, TransferManager.TransferReason material, int amount, ushort sourceBuilding, ushort targetBuilding)
        {
            if(material < ExtendedTransferManager.MealsDeliveryLow)
            {
                return true;
            }

            DistrictManager instance = Singleton<DistrictManager>.instance;
            BuildingManager instance2 = Singleton<BuildingManager>.instance;
            BuildingInfo info = instance2.m_buildings.m_buffer[sourceBuilding].Info;
            BuildingInfo info2 = instance2.m_buildings.m_buffer[targetBuilding].Info;
            byte industryArea = ReversePatches.GetIndustryArea(__instance, sourceBuilding);
            byte industryArea2 = ReversePatches.GetIndustryArea(__instance, targetBuilding);
            Vector3 position = instance2.m_buildings.m_buffer[sourceBuilding].m_position;
            Vector3 position2 = instance2.m_buildings.m_buffer[targetBuilding].m_position;
            byte district = instance.GetDistrict(position);
            byte district2 = instance.GetDistrict(position2);
            DistrictPolicies.CityPlanning cityPlanningPolicies = instance.m_districts.m_buffer[district].m_cityPlanningPolicies;
            DistrictPolicies.CityPlanning cityPlanningPolicies2 = instance.m_districts.m_buffer[district2].m_cityPlanningPolicies;
            if (industryArea != industryArea2)
            {
                if (industryArea != 0)
                {
                    DistrictParkManager.AddExportAmount(industryArea, material, amount);
                }
                if (industryArea2 != 0)
                {
                    DistrictParkManager.AddImportAmount(industryArea, material, amount);
                }
            }
            int num = (amount * IndustryBuildingAI.GetResourcePrice(material, info.m_class.m_service) + 50) / 100;
            if (material == TransferManager.TransferReason.Fish && ((cityPlanningPolicies & DistrictPolicies.CityPlanning.SustainableFishing) != DistrictPolicies.CityPlanning.None || (cityPlanningPolicies2 & DistrictPolicies.CityPlanning.SustainableFishing) != DistrictPolicies.CityPlanning.None))
            {
                num = (num * 105 + 99) / 100;
            }
            if (num == 0)
            {
                return false;
            }
            Building.Flags flags = instance2.m_buildings.m_buffer[sourceBuilding].m_flags;
            Building.Flags flags2 = instance2.m_buildings.m_buffer[targetBuilding].m_flags;
            ItemClass.Service service = info.m_class.m_service;
            ItemClass.Service service2 = info2.m_class.m_service;
            if (ItemClass.GetPublicServiceIndex(info.m_class.m_service) != -1 && (flags & Building.Flags.IncomingOutgoing) == 0)
            {
                if (ItemClass.GetPublicServiceIndex(info2.m_class.m_service) == -1 || info2.m_class.m_service == ItemClass.Service.ServicePoint || (flags2 & Building.Flags.IncomingOutgoing) != Building.Flags.None)
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
            return false;
        }
    }
}
