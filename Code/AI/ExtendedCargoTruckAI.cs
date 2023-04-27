using MoreTransferReasons;

namespace IndustriesMeetsSunsetHarbor.AI
{
    class ExtendedCargoTruckAI : CargoTruckAI, IExtendedVehicleAI
    {
        void IExtendedVehicleAI.ExtendedStartTransfer(ushort vehicleID, ref Vehicle data, ExtendedTransferManager.TransferReason material, ExtendedTransferManager.Offer offer)
        {
            if (material == (ExtendedTransferManager.TransferReason)data.m_transferType)
            {
                if ((data.m_flags & Vehicle.Flags.WaitingTarget) != 0)
                {
                    SetTarget(vehicleID, ref data, offer.Building);
                }
            }
        }


      
    }
}
