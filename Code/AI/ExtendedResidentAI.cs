using ColossalFramework;
using MoreTransferReasons;

namespace IndustriesMeetsSunsetHarbor.AI
{
    public class ExtendedResidentAI : ResidentAI, IExtendedCitizenAI
    {
        void IExtendedCitizenAI.ExtendedStartTransfer(uint citizenID, ref Citizen data, ExtendedTransferManager.TransferReason reason, ExtendedTransferManager.Offer offer)
        {
            if(reason == ExtendedTransferManager.TransferReason.MealsLow || reason == ExtendedTransferManager.TransferReason.MealsMedium || reason == ExtendedTransferManager.TransferReason.MealsHigh)
            {
                if (data.m_homeBuilding == 0 || data.Sick)
                {
                    return;
                }
                data.m_flags &= ~Citizen.Flags.Evacuating;
                if (StartMoving(citizenID, ref data, 0, offer.Building))
                {
                    data.SetVisitplace(citizenID, offer.Building, 0u);
                    CitizenManager instance = Singleton<CitizenManager>.instance;
                    BuildingManager instance2 = Singleton<BuildingManager>.instance;
                    uint containingUnit = data.GetContainingUnit(citizenID, instance2.m_buildings.m_buffer[data.m_homeBuilding].m_citizenUnits, CitizenUnit.Flags.Home);
                    if (containingUnit != 0)
                    {
                        instance.m_units.m_buffer[containingUnit].m_goods += 100;
                    }
                }
            }
        }
    }
}
