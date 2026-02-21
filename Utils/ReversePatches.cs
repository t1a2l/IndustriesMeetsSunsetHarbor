using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.Utils
{
    [HarmonyPatch]
    public static class ReversePatches
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(PlayerBuildingAI), "ProduceGoods")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void BaseProduceGoods(PlayerBuildingAI instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            string message = "ProduceGoods reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ProcessingFacilityAI), "GetInputBufferSize1", [typeof(DistrictPolicies.Park), typeof(int)], [ArgumentType.Normal, ArgumentType.Normal])]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int GetInputBufferSize1(ProcessingFacilityAI instance, DistrictPolicies.Park policies, int storageDelta)
        {
            string message = "GetInputBufferSize1 reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ProcessingFacilityAI), "GetInputBufferSize2", [typeof(DistrictPolicies.Park), typeof(int)], [ArgumentType.Normal, ArgumentType.Normal])]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int GetInputBufferSize2(ProcessingFacilityAI instance, DistrictPolicies.Park policies, int storageDelta)
        {
            string message = "GetInputBufferSize2 reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ProcessingFacilityAI), "GetInputBufferSize3", [typeof(DistrictPolicies.Park), typeof(int)], [ArgumentType.Normal, ArgumentType.Normal])]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int GetInputBufferSize3(ProcessingFacilityAI instance, DistrictPolicies.Park policies, int storageDelta)
        {
            string message = "GetInputBufferSize3 reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ProcessingFacilityAI), "GetInputBufferSize4", [typeof(DistrictPolicies.Park), typeof(int)], [ArgumentType.Normal, ArgumentType.Normal])]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int GetInputBufferSize4(ProcessingFacilityAI instance, DistrictPolicies.Park policies, int storageDelta)
        {
            string message = "GetInputBufferSize4 reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ProcessingFacilityAI), "GetOutputBufferSize", [typeof(DistrictPolicies.Park), typeof(int)], [ArgumentType.Normal, ArgumentType.Normal])]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int GetOutputBufferSize1(ProcessingFacilityAI instance, DistrictPolicies.Park policies, int storageDelta)
        {
            string message = "GetOutputBufferSize reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ExtractingFacilityAI), "GetOutputBufferSize", [typeof(DistrictPolicies.Park), typeof(int)], [ArgumentType.Normal, ArgumentType.Normal])]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int GetOutputBufferSize2(ExtractingFacilityAI instance, DistrictPolicies.Park policies, int storageDelta)
        {
            string message = "GetOutputBufferSize reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ExtractingFacilityAI), "GetResourceBufferSize", [typeof(DistrictPolicies.Park), typeof(int)], [ArgumentType.Normal, ArgumentType.Normal])]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int GetResourceBufferSize(ExtractingFacilityAI instance, DistrictPolicies.Park policies, int storageDelta)
        {
            string message = "GetResourceBufferSize reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(CommonBuildingAI), "HandleDead")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void HandleDead(CommonBuildingAI instance, ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, int citizenCount)
        {
            string message = "HandleDead reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ProcessingFacilityAI), "IsRawMaterial")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool IsRawMaterial(ProcessingFacilityAI instance, TransferManager.TransferReason material)
        {
            string message = "IsRawMaterial reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(WarehouseAI), "GetMaxLoadSize")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int GetMaxLoadSize(WarehouseAI instance)
        {
            string message = "GetMaxLoadSize reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(WarehouseAI), "SetContentFlags", MethodType.Normal)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SetContentFlags(WarehouseAI instance, ushort buildingID, ref Building data, TransferManager.TransferReason material)
        {
            string message = "SetContentFlags reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(IndustryBuildingAI), "GetIndustryArea")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static byte GetIndustryArea(IndustryBuildingAI instance, ushort buildingID)
        {
            string message = "GetIndustryArea reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(IndustryBuildingAI), "HandleDead2", MethodType.Normal)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void HandleDead2(IndustryBuildingAI instance, ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, int citizenCount)
        {
            string message = "HandleDead2 reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }
    }
}
