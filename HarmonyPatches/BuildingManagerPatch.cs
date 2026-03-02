using System;
using ColossalFramework.Math;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.AI;
using IndustriesMeetsSunsetHarbor.Managers;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(BuildingManager))]
    public static class BuildingManagerPatch
    {
        [HarmonyPatch(typeof(BuildingManager), "Awake")]
        [HarmonyPostfix]
        public static void Awake(BuildingManager __instance, ref FastList<ushort>[] ___m_areaBuildings, ref FastList<ushort>[] ___m_serviceBuildings)
        {
            Array.Resize(ref ___m_areaBuildings, 4640);
            Array.Resize(ref ___m_serviceBuildings, 20);
            ___m_serviceBuildings[19] = new FastList<ushort>();
        }

        [HarmonyPatch(typeof(BuildingManager), "CreateBuilding")]
        [HarmonyPostfix]
        public static void CreateBuilding(BuildingManager __instance, ref ushort building, ref Randomizer randomizer, BuildingInfo info, Vector3 position, float angle, int length, uint buildIndex)
        {
            AquacultureFarmManager.ObserveBuilding(building);
            AquacultureExtractorManager.ObserveBuilding(building);
        }

        [HarmonyPatch(typeof(BuildingManager), "ReleaseBuilding")]
        [HarmonyPostfix]
        public static void ReleaseBuilding(BuildingManager __instance, ushort building)
        {
            var data = BuildingManager.instance.m_buildings.m_buffer[building];
            // if building is an extractor just remove from building list tied to farm
            if (building != 0 && data.Info.GetAI() is AquacultureExtractorAI)
            {
                var aquacultureFarmId = AquacultureFarmManager.GetAquacultureFarm(building);
                if (AquacultureFarmManager.AquacultureFarms.ContainsKey(aquacultureFarmId))
                {
                    AquacultureFarmManager.AquacultureFarms[aquacultureFarmId].Remove(building);
                }
                if (AquacultureExtractorManager.AquacultureExtractorsWithNoFarm.Contains(building))
                {
                    AquacultureExtractorManager.AquacultureExtractorsWithNoFarm.Remove(building);
                }
            }
            // if building is a farm remove all extractors from it and try to find them a new farm, then remove the current farm
            if (building != 0 && data.Info.GetAI() is AquacultureFarmAI)
            {
                if (AquacultureFarmManager.AquacultureFarms.ContainsKey(building))
                {
                    foreach (var aquacultureExtractorId in AquacultureFarmManager.AquacultureFarms[building])
                    {
                        var aquacultureFarmId = AquacultureFarmManager.GetAquacultureFarm(aquacultureExtractorId);
                        // check that the closest farm is not the old farm you want to remove
                        if (aquacultureFarmId == 0 || aquacultureFarmId == building)
                        {
                            var newAquacultureFarmId = AquacultureFarmManager.GetClosestAquacultureFarm(aquacultureExtractorId);
                            if (newAquacultureFarmId != 0)
                            {
                                AquacultureFarmManager.AquacultureFarms[newAquacultureFarmId].Add(aquacultureExtractorId);
                            }
                            else
                            {
                                AquacultureExtractorManager.AquacultureExtractorsWithNoFarm.Add(aquacultureExtractorId);
                            }
                        }
                    }
                    AquacultureFarmManager.AquacultureFarms.Remove(building);
                }
            }

            if (CustomBuffersManager.CustomBufferExist(building))
            {
                CustomBuffersManager.RemoveCustomBuffer(building);
            }
        }
    }
}
