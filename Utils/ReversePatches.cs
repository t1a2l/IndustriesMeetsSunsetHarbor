using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.Utils
{
    public static class ReversePatches
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(PlayerBuildingAI), "ProduceGoods")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void BaseProduceGoods(object instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            string message = "ProduceGoods reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ProcessingFacilityAI), "GetInputBufferSize1", [typeof(DistrictPolicies.Park), typeof(int)], [ArgumentType.Normal, ArgumentType.Normal])]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int GetInputBufferSize1(object instance, DistrictPolicies.Park policies, int storageDelta)
        {
            string message = "GetInputBufferSize1 reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ProcessingFacilityAI), "GetInputBufferSize2", [typeof(DistrictPolicies.Park), typeof(int)], [ArgumentType.Normal, ArgumentType.Normal])]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int GetInputBufferSize2(object instance, DistrictPolicies.Park policies, int storageDelta)
        {
            string message = "GetInputBufferSize2 reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ProcessingFacilityAI), "GetInputBufferSize3", [typeof(DistrictPolicies.Park), typeof(int)], [ArgumentType.Normal, ArgumentType.Normal])]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int GetInputBufferSize3(object instance, DistrictPolicies.Park policies, int storageDelta)
        {
            string message = "GetInputBufferSize3 reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ProcessingFacilityAI), "GetInputBufferSize4", [typeof(DistrictPolicies.Park), typeof(int)], [ArgumentType.Normal, ArgumentType.Normal])]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int GetInputBufferSize4(object instance, DistrictPolicies.Park policies, int storageDelta)
        {
            string message = "GetInputBufferSize4 reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ExtractingFacilityAI), "GetResourceBufferSize", [typeof(DistrictPolicies.Park), typeof(int)], [ArgumentType.Normal, ArgumentType.Normal])]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int GetResourceBufferSize(object instance, DistrictPolicies.Park policies, int storageDelta)
        {
            string message = "GetResourceBufferSize reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ProcessingFacilityAI), "GetOutputBufferSize", [typeof(DistrictPolicies.Park), typeof(int)], [ArgumentType.Normal, ArgumentType.Normal])]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int GetOutputBufferSize(object instance, DistrictPolicies.Park policies, int storageDelta)
        {
            string message = "GetOutputBufferSize reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(CommonBuildingAI), "HandleDead")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void HandleDead(object instance, ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, int citizenCount)
        {
            string message = "HandleDead reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(IndustryBuildingAI), "HandleDead2")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void HandleDead2(object instance, ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, int citizenCount)
        {
            string message = "HandleDead2 reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(CommonBuildingAI), "IsRawMaterial")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool IsRawMaterial(object instance, TransferManager.TransferReason material)
        {
            string message = "IsRawMaterial reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(WarehouseAI), "GetMaxLoadSize")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int GetMaxLoadSize(object instance)
        {
            string message = "GetMaxLoadSize reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(WarehouseAI), "SetContentFlags")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int SetContentFlags(object instance, ushort buildingID, ref Building data, TransferManager.TransferReason material)
        {
            string message = "SetContentFlags reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }
    }
}
