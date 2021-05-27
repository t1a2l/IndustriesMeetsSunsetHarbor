﻿using HarmonyLib;
using ColossalFramework;
using UnityEngine;
using System.Reflection;
using FishIndustryEnhanced.FishPark;

namespace FishIndustryEnhanced
{
	[HarmonyPatch(typeof(UniqueFactoryWorldInfoPanel), "OnSetTarget")]
    public static class UniqueFactoryOutputAI
    {

		private delegate void CalculateGuestVehiclesDelegate(CommonBuildingAI __instance, ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside);
		private static CalculateGuestVehiclesDelegate BaseCalculateGuestVehicles = AccessTools.MethodDelegate<CalculateGuestVehiclesDelegate>(typeof(CommonBuildingAI).GetMethod("CalculateGuestVehicles", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

		private delegate void HandleDead2Delegate(IndustryBuildingAI __instance, ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, int citizenCount);
		private static HandleDead2Delegate BaseHandleDead2 = AccessTools.MethodDelegate<HandleDead2Delegate>(typeof(IndustryBuildingAI).GetMethod("HandleDead2", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

		private delegate void CalculateOwnVehiclesDelegate(CommonBuildingAI __instance, ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside);
		private static CalculateOwnVehiclesDelegate BaseCalculateOwnVehicles = AccessTools.MethodDelegate<CalculateOwnVehiclesDelegate>(typeof(CommonBuildingAI).GetMethod("CalculateOwnVehicles", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

		private delegate void ProduceGoodsDelegate(PlayerBuildingAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount);
		private static ProduceGoodsDelegate BaseProduceGoods = AccessTools.MethodDelegate<ProduceGoodsDelegate>(typeof(PlayerBuildingAI).GetMethod("ProduceGoods", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

		public static bool Prefix()
        {
			var Bioplastics_Plant = PrefabCollection<BuildingInfo>.FindLoaded("(Factory) Bioplastics Plant.Bioplastics Plant_Data");
			Bioplastics_Plant.m_placementMode = BuildingInfo.PlacementMode.Roadside;
			var Fishmeal_Factory = PrefabCollection<BuildingInfo>.FindLoaded("(Fish) Factory - Fishmeal.Fishmeal Factory_Data");
			Fishmeal_Factory.m_placementMode = BuildingInfo.PlacementMode.Roadside;
			return false;
		}

        public static void Postfix(UniqueFactoryAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
		{
			var IsRawMaterial = typeof(ProcessingFacilityAI).GetMethod("IsRawMaterial", BindingFlags.NonPublic | BindingFlags.Instance);
			var GetInputBufferSize1 = typeof(ProcessingFacilityAI).GetMethod("GetInputBufferSize1", BindingFlags.NonPublic | BindingFlags.Instance);
			var GetInputBufferSize2 = typeof(ProcessingFacilityAI).GetMethod("GetInputBufferSize2", BindingFlags.NonPublic | BindingFlags.Instance);
			var GetInputBufferSize3 = typeof(ProcessingFacilityAI).GetMethod("GetInputBufferSize3", BindingFlags.NonPublic | BindingFlags.Instance);
			var GetInputBufferSize4 = typeof(ProcessingFacilityAI).GetMethod("GetInputBufferSize4", BindingFlags.NonPublic | BindingFlags.Instance);
			var GetOutputBufferSize = typeof(ProcessingFacilityAI).GetMethod("GetOutputBufferSize", BindingFlags.NonPublic | BindingFlags.Instance);


			DistrictManager instance = Singleton<DistrictManager>.instance;
			byte district = instance.GetDistrict(buildingData.m_position);
			byte b = instance.GetPark(buildingData.m_position);
			if (b != 0)
			{
				if (!instance.m_parks.m_buffer[(int)b].IsIndustry)
				{
					b = 0;
				}
				else if (__instance.m_industryType == DistrictPark.ParkType.Industry || __instance.m_industryType != instance.m_parks.m_buffer[(int)b].m_parkType)
				{
					b = 0;
				}
			}
			float num = (float)buildingData.Width * -4f;
			float num2 = (float)buildingData.Width * 4f;
			float num3 = (float)buildingData.Length * -4f;
			float num4 = (float)buildingData.Length * 4f;
			if (__instance.m_info.m_subBuildings != null)
			{
				for (int i = 0; i < __instance.m_info.m_subBuildings.Length; i++)
				{
					if (__instance.m_info.m_subBuildings[i].m_buildingInfo != null)
					{
						float num5 = (float)__instance.m_info.m_subBuildings[i].m_buildingInfo.m_cellWidth;
						float num6 = (float)__instance.m_info.m_subBuildings[i].m_buildingInfo.m_cellLength;
						float x = __instance.m_info.m_subBuildings[i].m_position.x;
						float num7 = -__instance.m_info.m_subBuildings[i].m_position.z;
						num = Mathf.Min(num, x - num5 * 4f);
						num2 = Mathf.Max(num2, x + num5 * 4f);
						num3 = Mathf.Min(num3, num7 - num6 * 4f);
						num4 = Mathf.Max(num4, num7 + num6 * 4f);
					}
				}
			}
			float angle = buildingData.m_angle;
			float num8 = -(num + num2) * 0.5f;
			float num9 = -(num3 + num4) * 0.5f;
			float num10 = Mathf.Sin(angle);
			float num11 = Mathf.Cos(angle);
			Vector3 position = buildingData.m_position - new Vector3(num11 * num8 + num10 * num9, 0f, num10 * num8 - num11 * num9);
			Notification.Problem problem = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.NoResources | Notification.Problem.NoPlaceforGoods | Notification.Problem.NoInputProducts | Notification.Problem.NoFishingGoods);
			bool flag = __instance.m_info.m_class.m_service == ItemClass.Service.Fishing;
			DistrictPolicies.Park parkPolicies = instance.m_parks.m_buffer[(int)b].m_parkPolicies;
			DistrictPark[] buffer = instance.m_parks.m_buffer;
			byte b2 = b;
			buffer[(int)b2].m_parkPoliciesEffect = (buffer[(int)b2].m_parkPoliciesEffect | (parkPolicies & (DistrictPolicies.Park.ImprovedLogistics | DistrictPolicies.Park.WorkSafety | DistrictPolicies.Park.AdvancedAutomation)));
			if ((parkPolicies & DistrictPolicies.Park.ImprovedLogistics) != DistrictPolicies.Park.None)
			{
				int num12 = __instance.GetMaintenanceCost() / 100;
				num12 = finalProductionRate * num12 / 1000;
				if (num12 != 0)
				{
					Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Maintenance, num12, __instance.m_info.m_class);
				}
			}
			int num13 = __instance.m_outputRate;
			if ((parkPolicies & DistrictPolicies.Park.AdvancedAutomation) != DistrictPolicies.Park.None)
			{
				num13 = (num13 * 110 + 50) / 100;
				int num14 = __instance.GetMaintenanceCost() / 100;
				num14 = finalProductionRate * num14 / 1000;
				if (num14 != 0)
				{
					Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Maintenance, num14, __instance.m_info.m_class);
				}
			}
			if ((parkPolicies & DistrictPolicies.Park.WorkSafety) != DistrictPolicies.Park.None)
			{
				int num15 = (aliveWorkerCount + (int)(Singleton<SimulationManager>.instance.m_currentFrameIndex >> 8 & 15U)) / 16;
				if (num15 != 0)
				{
					Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, num15, __instance.m_info.m_class);
				}
			}
			if (finalProductionRate != 0)
			{
				int num16 = __instance.m_pollutionAccumulation;
				if (b != 0)
				{
					int num17;
					int num18;
					instance.m_parks.m_buffer[(int)b].GetProductionFactors(out num17, out num18);
					finalProductionRate = (finalProductionRate * num17 + 50) / 100;
					num16 = (num16 * num18 + 50) / 100;
				}
				else if (__instance.m_industryType != DistrictPark.ParkType.Industry)
				{
					finalProductionRate = 0;
				}
				int num19 = 0;
				int num20 = 0;
				if (__instance.m_inputResource1 != TransferManager.TransferReason.None)
				{
					
					num19 = (int)GetInputBufferSize1.Invoke(__instance, new object [] {parkPolicies, (int)instance.m_parks.m_buffer[(int)b].m_finalStorageDelta });
					num20 = (int)buildingData.m_customBuffer2;
					int num21 = (__instance.m_inputRate1 * finalProductionRate + 99) / 100;
					if (num20 < num21)
					{
						finalProductionRate = (num20 * 100 + __instance.m_inputRate1 - 1) / __instance.m_inputRate1;
						problem = Notification.AddProblems(problem, (!flag) ? ((!(bool)IsRawMaterial.Invoke(__instance, new object [] { __instance.m_inputResource1 })) ? Notification.Problem.NoInputProducts : Notification.Problem.NoResources) : Notification.Problem.NoFishingGoods);
					}
				}
				int num22 = 0;
				int num23 = 0;
				if (__instance.m_inputResource2 != TransferManager.TransferReason.None)
				{
					num22 = (int)GetInputBufferSize2.Invoke(__instance, new object [] {parkPolicies, (int)instance.m_parks.m_buffer[(int)b].m_finalStorageDelta });
					num23 = ((int)buildingData.m_teens << 8 | (int)buildingData.m_youngs);
					int num24 = (__instance.m_inputRate2 * finalProductionRate + 99) / 100;
					if (num23 < num24)
					{
						finalProductionRate = (num23 * 100 + __instance.m_inputRate2 - 1) / __instance.m_inputRate2;
						problem = Notification.AddProblems(problem, (!flag) ? ((!(bool)IsRawMaterial.Invoke(__instance, new object [] { __instance.m_inputResource2 })) ? Notification.Problem.NoInputProducts : Notification.Problem.NoResources) : Notification.Problem.NoFishingGoods);
					}
				}
				int num25 = 0;
				int num26 = 0;
				if (__instance.m_inputResource3 != TransferManager.TransferReason.None)
				{
					num25 = (int)GetInputBufferSize3.Invoke(__instance, new object [] {parkPolicies, (int)instance.m_parks.m_buffer[(int)b].m_finalStorageDelta });
					num26 = ((int)buildingData.m_adults << 8 | (int)buildingData.m_seniors);
					int num27 = (__instance.m_inputRate3 * finalProductionRate + 99) / 100;
					if (num26 < num27)
					{
						finalProductionRate = (num26 * 100 + __instance.m_inputRate3 - 1) / __instance.m_inputRate3;
						problem = Notification.AddProblems(problem, (!flag) ? ((!(bool)IsRawMaterial.Invoke(__instance, new object [] { __instance.m_inputResource3 })) ? Notification.Problem.NoInputProducts : Notification.Problem.NoResources) : Notification.Problem.NoFishingGoods);
					}
				}
				int num28 = 0;
				int num29 = 0;
				if (__instance.m_inputResource4 != TransferManager.TransferReason.None)
				{
					num28 = (int)GetInputBufferSize4.Invoke(__instance, new object [] {parkPolicies, (int)instance.m_parks.m_buffer[(int)b].m_finalStorageDelta });
					num29 = ((int)buildingData.m_education1 << 8 | (int)buildingData.m_education2);
					int num30 = (__instance.m_inputRate4 * finalProductionRate + 99) / 100;
					if (num29 < num30)
					{
						finalProductionRate = (num29 * 100 + __instance.m_inputRate4 - 1) / __instance.m_inputRate4;
						problem = Notification.AddProblems(problem, (!flag) ? ((!(bool)IsRawMaterial.Invoke(__instance, new object [] { __instance.m_inputResource4 })) ? Notification.Problem.NoInputProducts : Notification.Problem.NoResources) : Notification.Problem.NoFishingGoods);
					}
				}
				int num31 = 0;
				int num32 = 0;
				if (__instance.m_outputResource != TransferManager.TransferReason.None)
				{
					num31 = (int)GetOutputBufferSize.Invoke(__instance, new object [] {parkPolicies, (int)instance.m_parks.m_buffer[(int)b].m_finalStorageDelta });
					num32 = (int)buildingData.m_customBuffer1;
					int num33 = (num13 * finalProductionRate + 99) / 100;
					if (num31 - num32 < num33)
					{
						num33 = Mathf.Max(0, num31 - num32);
						finalProductionRate = (num33 * 100 + num13 - 1) / num13;
						if (__instance.m_outputVehicleCount != 0)
						{
							problem = Notification.AddProblems(problem, Notification.Problem.NoPlaceforGoods);
							if (__instance.m_info.m_class.m_service == ItemClass.Service.PlayerIndustry)
							{
								GuideController properties = Singleton<GuideManager>.instance.m_properties;
								if (properties != null)
								{
									Singleton<BuildingManager>.instance.m_warehouseNeeded.Activate(properties.m_warehouseNeeded, buildingID);
								}
							}
						}
					}
				}
				if (__instance.m_inputResource1 != TransferManager.TransferReason.None)
				{
					int num34 = (__instance.m_inputRate1 * finalProductionRate + 99) / 100;
					num20 = Mathf.Max(0, num20 - num34);
					buildingData.m_customBuffer2 = (ushort)num20;
					instance.m_parks.m_buffer[(int)b].AddConsumptionAmount(__instance.m_inputResource1, num34);
				}
				if (__instance.m_inputResource2 != TransferManager.TransferReason.None)
				{
					int num35 = (__instance.m_inputRate2 * finalProductionRate + 99) / 100;
					num23 = Mathf.Max(0, num23 - num35);
					buildingData.m_youngs = (byte)(num23 & 255);
					buildingData.m_teens = (byte)(num23 >> 8);
					instance.m_parks.m_buffer[(int)b].AddConsumptionAmount(__instance.m_inputResource2, num35);
				}
				if (__instance.m_inputResource3 != TransferManager.TransferReason.None)
				{
					int num36 = (__instance.m_inputRate3 * finalProductionRate + 99) / 100;
					num26 = Mathf.Max(0, num26 - num36);
					buildingData.m_seniors = (byte)(num26 & 255);
					buildingData.m_adults = (byte)(num26 >> 8);
					instance.m_parks.m_buffer[(int)b].AddConsumptionAmount(__instance.m_inputResource3, num36);
				}
				if (__instance.m_inputResource4 != TransferManager.TransferReason.None)
				{
					int num37 = (__instance.m_inputRate4 * finalProductionRate + 99) / 100;
					num29 = Mathf.Max(0, num29 - num37);
					buildingData.m_education2 = (byte)(num29 & 255);
					buildingData.m_education1 = (byte)(num29 >> 8);
					instance.m_parks.m_buffer[(int)b].AddConsumptionAmount(__instance.m_inputResource4, num37);
				}
				if (__instance.m_outputResource != TransferManager.TransferReason.None)
				{
					int num38 = (num13 * finalProductionRate + 99) / 100;
					num32 = Mathf.Min(num31, num32 + num38);
					buildingData.m_customBuffer1 = (ushort)num32;
					LogHelper.Information("FishHatchery production amount output resource:" + __instance.m_outputResource.ToString());
					instance.m_parks.m_buffer[(int)b].AddProductionAmountFish(__instance.m_outputResource, num38);
				}
				num16 = (finalProductionRate * num16 + 50) / 100;
				if (num16 != 0)
				{
					num16 = UniqueFacultyAI.DecreaseByBonus(UniqueFacultyAI.FacultyBonus.Science, num16);
					Singleton<NaturalResourceManager>.instance.TryDumpResource(NaturalResourceManager.Resource.Pollution, num16, num16, position, __instance.m_pollutionRadius);
				}
				BaseHandleDead2(__instance, buildingID, ref buildingData, ref behaviour, totalWorkerCount);
				if (b != 0 || __instance.m_industryType == DistrictPark.ParkType.Industry)
				{
					int num39 = 0;
					if (__instance.m_inputResource1 != TransferManager.TransferReason.None)
					{
						int num40 = 0;
						int num41 = 0;
						int num42 = 0;
						int num43 = 0;
						BaseCalculateGuestVehicles(__instance, buildingID, ref buildingData, __instance.m_inputResource1, ref num40, ref num41, ref num42, ref num43);
						if (num43 != 0)
						{
							num39 |= 1;
						}
						int num44 = num19 - num20 - num41;
						if (num44 >= 8000)
						{
							TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
							offer.Priority = Mathf.Max(1, num44 * 8 / num19);
							offer.Building = buildingID;
							offer.Position = buildingData.m_position;
							offer.Amount = 1;
							offer.Active = false;
							Singleton<TransferManager>.instance.AddIncomingOffer(__instance.m_inputResource1, offer);
						}
						instance.m_parks.m_buffer[(int)b].AddBufferStatus(__instance.m_inputResource1, num20, num41, num19);
					}
					if (__instance.m_inputResource2 != TransferManager.TransferReason.None)
					{
						int num45 = 0;
						int num46 = 0;
						int num47 = 0;
						int num48 = 0;
						BaseCalculateGuestVehicles(__instance, buildingID, ref buildingData, __instance.m_inputResource2, ref num45, ref num46, ref num47, ref num48);
						if (num48 != 0)
						{
							num39 |= 2;
						}
						int num49 = num22 - num23 - num46;
						if (num49 >= 8000)
						{
							TransferManager.TransferOffer offer2 = default(TransferManager.TransferOffer);
							offer2.Priority = Mathf.Max(1, num49 * 8 / num22);
							offer2.Building = buildingID;
							offer2.Position = buildingData.m_position;
							offer2.Amount = 1;
							offer2.Active = false;
							Singleton<TransferManager>.instance.AddIncomingOffer(__instance.m_inputResource2, offer2);
						}
						instance.m_parks.m_buffer[(int)b].AddBufferStatus(__instance.m_inputResource2, num23, num46, num22);
					}
					if (__instance.m_inputResource3 != TransferManager.TransferReason.None)
					{
						int num50 = 0;
						int num51 = 0;
						int num52 = 0;
						int num53 = 0;
						BaseCalculateGuestVehicles(__instance, buildingID, ref buildingData, __instance.m_inputResource3, ref num50, ref num51, ref num52, ref num53);
						if (num53 != 0)
						{
							num39 |= 4;
						}
						int num54 = num25 - num26 - num51;
						if (num54 >= 8000)
						{
							TransferManager.TransferOffer offer3 = default(TransferManager.TransferOffer);
							offer3.Priority = Mathf.Max(1, num54 * 8 / num25);
							offer3.Building = buildingID;
							offer3.Position = buildingData.m_position;
							offer3.Amount = 1;
							offer3.Active = false;
							Singleton<TransferManager>.instance.AddIncomingOffer(__instance.m_inputResource3, offer3);
						}
						instance.m_parks.m_buffer[(int)b].AddBufferStatus(__instance.m_inputResource3, num26, num51, num25);
					}
					if (__instance.m_inputResource4 != TransferManager.TransferReason.None)
					{
						int num55 = 0;
						int num56 = 0;
						int num57 = 0;
						int num58 = 0;
						BaseCalculateGuestVehicles(__instance, buildingID, ref buildingData, __instance.m_inputResource4, ref num55, ref num56, ref num57, ref num58);
						if (num58 != 0)
						{
							num39 |= 8;
						}
						int num59 = num28 - num29 - num56;
						if (num59 >= 8000)
						{
							TransferManager.TransferOffer offer4 = default(TransferManager.TransferOffer);
							offer4.Priority = Mathf.Max(1, num59 * 8 / num28);
							offer4.Building = buildingID;
							offer4.Position = buildingData.m_position;
							offer4.Amount = 1;
							offer4.Active = false;
							Singleton<TransferManager>.instance.AddIncomingOffer(__instance.m_inputResource4, offer4);
						}
						instance.m_parks.m_buffer[(int)b].AddBufferStatus(__instance.m_inputResource4, num29, num56, num28);
					}
					buildingData.m_tempImport |= (byte)num39;
					if (__instance.m_outputResource != TransferManager.TransferReason.None)
					{
						if (__instance.m_outputVehicleCount == 0)
						{
							if (num32 == num31)
							{
								int num60 = (num32 * IndustryBuildingAI.GetResourcePrice(__instance.m_outputResource, ItemClass.Service.None) + 50) / 100;
								if ((instance.m_districts.m_buffer[(int)district].m_cityPlanningPolicies & DistrictPolicies.CityPlanning.SustainableFishing) != DistrictPolicies.CityPlanning.None)
								{
									num60 = (num60 * 105 + 99) / 100;
								}
								num60 = UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Science, num60);
								Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.ResourcePrice, num60, __instance.m_info.m_class);
								if (b != 0)
								{
									LogHelper.Information("FishHatchery export amount output resource:" + __instance.m_outputResource.ToString());
									instance.m_parks.m_buffer[(int)b].AddExportAmountFish(__instance.m_outputResource, num32);
								}
								num32 = 0;
								buildingData.m_customBuffer1 = (ushort)num32;
								buildingData.m_tempExport = byte.MaxValue;
							}
						}
						else
						{
							int num61 = 0;
							int num62 = 0;
							int num63 = 0;
							int value = 0;
							BaseCalculateOwnVehicles(__instance, buildingID, ref buildingData, __instance.m_outputResource, ref num61, ref num62, ref num63, ref value);
							buildingData.m_tempExport = (byte)Mathf.Clamp(value, (int)buildingData.m_tempExport, 255);
							int budget = Singleton<EconomyManager>.instance.GetBudget(__instance.m_info.m_class);
							int productionRate2 = PlayerBuildingAI.GetProductionRate(100, budget);
							int num64 = (productionRate2 * __instance.m_outputVehicleCount + 99) / 100;
							int num65 = num32;
							if (num65 >= 8000 && num61 < num64)
							{
								TransferManager.TransferOffer offer5 = default(TransferManager.TransferOffer);
								offer5.Priority = Mathf.Max(1, num65 * 8 / num31);
								offer5.Building = buildingID;
								offer5.Position = buildingData.m_position;
								offer5.Amount = 1;
								offer5.Active = true;
								Singleton<TransferManager>.instance.AddOutgoingOffer(__instance.m_outputResource, offer5);
							}
						}
						LogHelper.Information("FishHatchery buffer output resource:" + __instance.m_outputResource.ToString());
						instance.m_parks.m_buffer[(int)b].AddBufferStatusFish(__instance.m_outputResource, num32, 0, num31);
					}
				}
				if (buildingData.m_finalImport != 0)
				{
					District[] buffer2 = instance.m_districts.m_buffer;
					byte b3 = district;
					buffer2[(int)b3].m_playerConsumption.m_finalImportAmount = buffer2[(int)b3].m_playerConsumption.m_finalImportAmount + (uint)buildingData.m_finalImport;
				}
				if (buildingData.m_finalExport != 0)
				{
					District[] buffer3 = instance.m_districts.m_buffer;
					byte b4 = district;
					buffer3[(int)b4].m_playerConsumption.m_finalExportAmount = buffer3[(int)b4].m_playerConsumption.m_finalExportAmount + (uint)buildingData.m_finalExport;
				}
				int num66 = finalProductionRate * __instance.m_noiseAccumulation / 100;
				if (num66 != 0)
				{
					Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, num66, position, __instance.m_noiseRadius);
				}
			}
			buildingData.m_problems = problem;
			buildingData.m_education3 = (byte)Mathf.Clamp(finalProductionRate * num13 / Mathf.Max(1, __instance.m_outputRate), 0, 255);
			buildingData.m_health = (byte)Mathf.Clamp(finalProductionRate, 0, 255);
			if (b != 0)
			{
				instance.m_parks.m_buffer[(int)b].AddWorkers(aliveWorkerCount);
			}
			else if (__instance.m_industryType != DistrictPark.ParkType.Industry)
			{
				GuideController properties2 = Singleton<GuideManager>.instance.m_properties;
				if (properties2 != null)
				{
					Singleton<BuildingManager>.instance.m_industryBuildingOutsideIndustryArea.Activate(properties2.m_industryBuildingOutsideIndustryArea, buildingID);
				}
			}
			BaseProduceGoods(__instance, buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
		}


    }
		
}
