using ColossalFramework;
using UnityEngine;
using System.Reflection;
using HarmonyLib;

namespace FishIndustryEnhanced
{
	[HarmonyPatch(typeof(FishFarmAI), "ProduceGoods")]
    public static class AlgaeTanksAI
    {
		private delegate void HandleDeadDelegate(CommonBuildingAI instance, ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, int citizenCount);
		private static HandleDeadDelegate BaseHandleDead = AccessTools.MethodDelegate<HandleDeadDelegate>(typeof(CommonBuildingAI).GetMethod("ProduceGoods", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

		private delegate void CalculateOwnVehiclesDelegate(CommonBuildingAI __instance, ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside);
		private static CalculateOwnVehiclesDelegate BaseCalculateOwnVehicles = AccessTools.MethodDelegate<CalculateOwnVehiclesDelegate>(typeof(CommonBuildingAI).GetMethod("CalculateOwnVehicles", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

		private delegate void ProduceGoodsDelegate(PlayerBuildingAI instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount);
		private static ProduceGoodsDelegate BaseProduceGoods = AccessTools.MethodDelegate<ProduceGoodsDelegate>(typeof(PlayerBuildingAI).GetMethod("ProduceGoods", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

		[HarmonyPostfix]
        public static void Postfix(FishFarmAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
		{
			Debug.Log("postfix works!!!!");
			if(__instance.m_info.name == "(Fish) Farm Dock - Algae.Aquaculture Dock - Algae_Data" || __instance.m_info.name == "(Fish) Farm Dock - Seaweed.Aquaculture Dock - Seaweed_Data" 
				|| __instance.m_info.name == "(Fish) Farm Tanks - Algae.Aquaculture Farm - Algae Tanks_Data")
            {
				__instance.m_outputResource = TransferManager.TransferReason.Grain;
            } 
			else
            {
				__instance.m_outputResource = TransferManager.TransferReason.Fish;
            }
				
			DistrictManager instance = Singleton<DistrictManager>.instance;
			byte district = instance.GetDistrict(buildingData.m_position);
			DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[(int)district].m_servicePolicies;
			Notification.Problem problem = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.WaterNotConnected | Notification.Problem.NoResources | Notification.Problem.NoNaturalResources | Notification.Problem.FishFarmWaterDirty | Notification.Problem.NoPlaceForFishingGoods);
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			if (__instance.m_extractionPositions != null && __instance.m_extractionPositions.Length > 0)
			{
				for (int i = 0; i < __instance.m_extractionPositions.Length; i++)
				{
					Vector3 position = buildingData.CalculatePosition(__instance.m_extractionPositions[i]);
					int b;
					int num4;
					int value;
					Singleton<TerrainManager>.instance.CountWaterCoverage(position, 20f, out b, out num4, out value);
					num += Mathf.Clamp(value, 0, 128);
					num3 = Mathf.Max(num3, b);
				}
				if(__instance.m_info.name == "(Fish) Farm Tanks - Algae.Aquaculture Farm - Algae Tanks_Data")
                {
					num2 = 0;
					num3 = 1;
                } else
                {
					num2 = num / __instance.m_extractionPositions.Length;
                }		
			}
			else
			{
				finalProductionRate = 0;
			}
			if (num2 > 32)
			{
				GuideController properties = Singleton<GuideManager>.instance.m_properties;
				if (properties != null)
				{
					Singleton<BuildingManager>.instance.m_fishingPollutionDetected.Activate(properties.m_fishingPollutionDetected, buildingID);
				}
			}
			if (num3 == 0)
			{
				finalProductionRate = 0;
			}
			if (finalProductionRate != 0)
			{
				int num5 = finalProductionRate;
				if (num2 > 96)
				{
					problem = (Notification.Problem.FishFarmWaterDirty | Notification.Problem.FatalProblem);
				}
				else if (num2 > 64)
				{
					problem = Notification.AddProblems(problem, Notification.Problem.FishFarmWaterDirty | Notification.Problem.MajorProblem);
				}
				else if (num2 > 32)
				{
					problem = Notification.AddProblems(problem, Notification.Problem.FishFarmWaterDirty);
				}
				finalProductionRate = finalProductionRate * Mathf.Clamp(255 - num2 * 2, 0, 255) / 255;
				int num6 = finalProductionRate * __instance.m_noiseAccumulation / 100;
				if (num6 != 0)
				{
					Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, num6, buildingData.m_position, __instance.m_noiseRadius);
				}
				BaseHandleDead(__instance, buildingID, ref buildingData, ref behaviour, totalWorkerCount);
				int num7 = 0;
				int num8 = 0;
				if (__instance.m_outputResource != TransferManager.TransferReason.None)
				{
					int num9 = (__instance.m_productionRate * finalProductionRate + 99) / 100;
					if (__instance.m_info.m_class.m_level == ItemClass.Level.Level3 && (servicePolicies & DistrictPolicies.Services.AlgaeBasedWaterFiltering) != DistrictPolicies.Services.None)
					{
						District[] buffer = instance.m_districts.m_buffer;
						byte b2 = district;
						buffer[(int)b2].m_servicePoliciesEffect = (buffer[(int)b2].m_servicePoliciesEffect | DistrictPolicies.Services.AlgaeBasedWaterFiltering);
						num9 = (num9 * 50 + 49) / 100;
					}
					int cycleBufferSize = __instance.GetCycleBufferSize(buildingID, ref buildingData);
					int num10 = (int)buildingData.m_customBuffer1;
					
					num7 = __instance.GetStorageBufferSize(buildingID, ref buildingData);
					num8 = (int)(buildingData.m_customBuffer2 * 100);
					if (num9 >= cycleBufferSize - num10)
					{
						if (cycleBufferSize > num7 - num8)
						{
							num9 = cycleBufferSize - num10;
							num10 = cycleBufferSize;
							problem = Notification.AddProblems(problem, Notification.Problem.NoPlaceForFishingGoods);
						}
						else
						{
							num10 = num10 + num9 - cycleBufferSize;
							num8 += cycleBufferSize;
						}
					}
					else
					{
						num10 += num9;
					}
					Singleton<StatisticsManager>.instance.Acquire<StatisticInt64>(StatisticType.FishFarmed).Add(num9);
					buildingData.m_customBuffer1 = (ushort)num10;
					buildingData.m_customBuffer2 = (ushort)(num8 / 100);
				}
				if (__instance.m_outputResource != TransferManager.TransferReason.None)
				{
					int num11 = (num5 * __instance.m_outputVehicleCount + 99) / 100;
					int num12 = 0;
					int num13 = 0;
					int num14 = 0;
					int value2 = 0;

					BaseCalculateOwnVehicles(__instance, buildingID, ref buildingData, __instance.m_outputResource, ref num12, ref num13, ref num14, ref value2);
					buildingData.m_tempExport = (byte)Mathf.Clamp(value2, (int)buildingData.m_tempExport, 255);
					if (buildingData.m_finalExport != 0)
					{
						District[] buffer2 = instance.m_districts.m_buffer;
						byte b3 = district;
						buffer2[(int)b3].m_playerConsumption.m_finalExportAmount = buffer2[(int)b3].m_playerConsumption.m_finalExportAmount + (uint)buildingData.m_finalExport;
					}
					int num15 = num8;
					if (num15 >= 8000 && num12 < num11)
					{
						TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
						offer.Priority = Mathf.Max(1, num15 * 8 / num7);
						offer.Building = buildingID;
						offer.Position = buildingData.m_position;
						offer.Amount = 1;
						offer.Active = true;
						Singleton<TransferManager>.instance.AddOutgoingOffer(__instance.m_outputResource, offer);
					}
				}
			}
			buildingData.m_problems = problem;
			buildingData.m_education3 = (byte)Mathf.Clamp(finalProductionRate * __instance.m_productionRate / Mathf.Max(1, __instance.m_productionRate), 0, 255);
			BaseProduceGoods(__instance, buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
		}
    }
}