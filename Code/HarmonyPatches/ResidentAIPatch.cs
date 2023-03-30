using ColossalFramework;
using HarmonyLib;
using System;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(ResidentAI))]
    public static class ResidentAIPatch
    {
        [HarmonyPatch(typeof(ResidentAI), "SimulationStep", new Type[] { typeof(ushort), typeof(CitizenInstance), typeof(CitizenInstance.Frame), typeof(bool) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal })]
        [HarmonyPrefix]
        public static bool SimulationStep(ResidentAI __instance, ushort instanceID, ref CitizenInstance citizenData, ref CitizenInstance.Frame frameData, bool lodPhysics)
        {
            uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
            if ((ulong)((currentFrameIndex >> 4) & 63U) == (ulong)((long)(instanceID & 63)))
            {
                CitizenManager instance = Singleton<CitizenManager>.instance;
                uint citizen = citizenData.m_citizen;
                // if citizen is waiting for delivery do nothing - the goods transfer will happen when the delivery vehicle will arrive at the house
                if (citizen != 0U && ((instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].m_flags & Citizen.Flags.NeedGoods) != Citizen.Flags.None) && ((instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].m_flags & HumanAIPatch.waitingDelivery) != Citizen.Flags.None))
                {
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(ResidentAI), "StartTransfer")]
        [HarmonyPrefix]
        public static bool StartTransfer(uint citizenID, ref Citizen data, TransferManager.TransferReason reason, TransferManager.TransferOffer offer)
        {
            switch (reason)
            {
                case TransferManager.TransferReason.Shopping:
                case TransferManager.TransferReason.ShoppingB:
		case TransferManager.TransferReason.ShoppingC:
		case TransferManager.TransferReason.ShoppingD:
		case TransferManager.TransferReason.ShoppingE:
		case TransferManager.TransferReason.ShoppingF:
		case TransferManager.TransferReason.ShoppingG:
		case TransferManager.TransferReason.ShoppingH:
                    {
                        if((data.m_flags & HumanAIPatch.waitingDelivery) != Citizen.Flags.None) // don't start moving if waiting for delivery
                        {
                             return false;
                        }
                        return true;
                    }
                    
                default:
                     return true;
                   
            }
        }


    }
}
