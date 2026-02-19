using HarmonyLib;
using MoreTransferReasons;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch]
    public static class TouristAIPatch
    {
        [HarmonyPatch(typeof(TouristAI), "StartTransfer")]
        [HarmonyPostfix]
        public static void StartTransfer(TouristAI __instance, uint citizenID, ref Citizen data, TransferManager.TransferReason material, TransferManager.TransferOffer offer)
        {
            if (data.m_flags == Citizen.Flags.None || data.Dead || data.Sick)
            {
                return;
            }
            ushort source_building = 0;
            switch (data.CurrentLocation)
            {
                case Citizen.Location.Home:
                    source_building = data.m_homeBuilding;
                    break;

                case Citizen.Location.Visit:
                    source_building = data.m_visitBuilding;
                    break;
            }
            switch (material)
            {
                case ExtendedTransferManager.MealsLow:
                case ExtendedTransferManager.MealsMedium:
                case ExtendedTransferManager.MealsHigh:
                    data.m_flags &= ~Citizen.Flags.Evacuating;
                    if (__instance.StartMoving(citizenID, ref data, source_building, offer.Building))
                    {
                        data.SetVisitplace(citizenID, offer.Building, 0u);
                    }
                    break;
            }
        }


    }
}
