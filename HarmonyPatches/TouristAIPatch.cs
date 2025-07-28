using HarmonyLib;
using MoreTransferReasons;
using MoreTransferReasons.AI;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch]
    public static class TouristAIPatch
    {
        [HarmonyPatch(typeof(ExtenedTouristAI), "MoreTransferReasons.AI.ExtenedTouristAI.ExtendedStartTransfer")]
        [HarmonyPrefix]
        public static bool ExtendedStartTransfer(ExtenedTouristAI __instance, uint citizenID, ref Citizen data, ExtendedTransferManager.TransferReason material, ExtendedTransferManager.Offer offer)
        {
            if (data.m_flags == Citizen.Flags.None || data.Dead || data.Sick)
            {
                return true;
            }
            ushort source_building = 0;
            switch (data.CurrentLocation)
            {
                case Citizen.Location.Home:
                    source_building = data.m_homeBuilding;
                    break;

                case Citizen.Location.Work:
                    source_building = data.m_workBuilding;
                    break;

                case Citizen.Location.Visit:
                    source_building = data.m_visitBuilding;
                    break;
            }
            switch (material)
            {
                case ExtendedTransferManager.TransferReason.MealsLow:
                case ExtendedTransferManager.TransferReason.MealsMedium:
                case ExtendedTransferManager.TransferReason.MealsHigh:
                    data.m_flags &= ~Citizen.Flags.Evacuating;
                    if (__instance.StartMoving(citizenID, ref data, source_building, offer.Building))
                    {
                        data.SetVisitplace(citizenID, offer.Building, 0u);
                    }
                    return false;
            }
            return true;
        }


    }
}
