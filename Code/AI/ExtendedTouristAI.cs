using MoreTransferReasons;

namespace IndustriesMeetsSunsetHarbor.AI
{
    public class ExtendedTouristAI : TouristAI, IExtendedCitizenAI
    {
        void IExtendedCitizenAI.ExtendedStartTransfer(uint citizenID, ref Citizen data, ExtendedTransferManager.TransferReason reason, ExtendedTransferManager.Offer offer)
        {
            if(reason == ExtendedTransferManager.TransferReason.MealsLow || reason == ExtendedTransferManager.TransferReason.MealsMedium || reason == ExtendedTransferManager.TransferReason.MealsHigh)
            {
                if (data.m_flags == Citizen.Flags.None || data.Sick || data.Dead)
                {
                    return;
                }
                data.m_flags &= ~Citizen.Flags.Evacuating;
                if (StartMoving(citizenID, ref data, 0, offer.Building))
                {
                    data.SetVisitplace(citizenID, offer.Building, 0u);
                }
            }
        }
    }
}
