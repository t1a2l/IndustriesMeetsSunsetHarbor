using ColossalFramework;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.Managers;
using System;
using IndustriesMeetsSunsetHarbor.AI;

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
                BuildingManager instance2 = Singleton<BuildingManager>.instance;
                var building_info = instance2.m_buildings.m_buffer[citizenData.m_targetBuilding].Info;
                uint citizen = citizenData.m_citizen;
                var waiting_delivery = RestaurantManager.IsCitizenWaitingForDelivery(citizen);
                // if citizen is waiting for delivery do nothing - the goods transfer will happen when the delivery vehicle will arrive at the house
                if (citizen != 0U && ((instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].m_flags & Citizen.Flags.NeedGoods) != Citizen.Flags.None))
                {
                    if(waiting_delivery)
                    {
                         return false;
                    }
                    else if(building_info.m_buildingAI is RestaurantAI)
                    {
                         return false;
                    }
                }
            }
            return true;
        }

    }
}


