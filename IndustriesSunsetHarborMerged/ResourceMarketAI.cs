using System.Reflection;
using ColossalFramework;
using UnityEngine;
using System.Collections.Generic;

namespace IndustriesSunsetHarborMerged.IndustriesSunsetHarborMerged
{
    public class ResourceMarketAI : MarketAI
    {
		Dictionary<ushort, ushort[]> marketBuffers = new Dictionary<ushort, ushort[]>();

		public TransferManager.TransferReason[] m_incomingResources = new TransferManager.TransferReason[]
		{
			TransferManager.TransferReason.Fish,
			TransferManager.TransferReason.Grain,
			TransferManager.TransferReason.Food,
			TransferManager.TransferReason.LuxuryProducts,
			TransferManager.TransferReason.AnimalProducts,
			TransferManager.TransferReason.Flours
        };

		public override Color GetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode)
		{
			int attractivenessAccumulation = this.GetAttractivenessAccumulation(buildingID, ref data);
			switch (infoMode)
			{
			case InfoManager.InfoMode.Tours:
				return CommonBuildingAI.GetAttractivenessColor(attractivenessAccumulation * 15);
			default:
				if (infoMode == InfoManager.InfoMode.NoisePollution)
				{
					int noiseAccumulation = this.m_noiseAccumulation;
					return CommonBuildingAI.GetNoisePollutionColor((float)noiseAccumulation);
				}
				if (infoMode != InfoManager.InfoMode.Connections)
				{
					if (infoMode != InfoManager.InfoMode.Entertainment)
					{
						return base.GetColor(buildingID, ref data, infoMode);
					}
					if (Singleton<InfoManager>.instance.CurrentSubMode != InfoManager.SubInfoMode.WaterPower)
					{
						return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
					}
					if ((data.m_flags & Building.Flags.Active) != Building.Flags.None)
					{
						return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
					}
					return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_inactiveColor;
				}
				else
				{
					if (!this.ShowConsumption(buildingID, ref data))
					{
						return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
					}
					if (Singleton<InfoManager>.instance.CurrentSubMode != InfoManager.SubInfoMode.Default)
					{
						return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
					}
					for(int i = 0; i < m_incomingResources.Length; i++)
                    {
						if (m_incomingResources[i] != TransferManager.TransferReason.None && (data.m_tempImport != 0 || data.m_finalImport != 0))
						{
							return Singleton<TransferManager>.instance.m_properties.m_resourceColors[(int)m_incomingResources[i]];
						}
                    }
					return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
				}
			case InfoManager.InfoMode.Tourism:
			{
				InfoManager.SubInfoMode currentSubMode = Singleton<InfoManager>.instance.CurrentSubMode;
				if (currentSubMode == InfoManager.SubInfoMode.Default)
				{
					if (data.m_tempExport != 0 || data.m_finalExport != 0)
					{
						return CommonBuildingAI.GetTourismColor(Mathf.Max((int)data.m_tempExport, (int)data.m_finalExport));
					}
					return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
				}
				else
				{
					if (currentSubMode != InfoManager.SubInfoMode.WaterPower)
					{
						return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
					}
					if (attractivenessAccumulation != 0)
					{
						return CommonBuildingAI.GetAttractivenessColor(attractivenessAccumulation * 100);
					}
					return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
				}
			}
			case InfoManager.InfoMode.Fishing:
				if ((data.m_flags & Building.Flags.Active) != Building.Flags.None)
				{
					return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
				}
				return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_inactiveColor;
			}
		}

		public override void GetPlacementInfoMode(out InfoManager.InfoMode mode, out InfoManager.SubInfoMode subMode, float elevation)
		{
			if (m_incomingResources[0] == TransferManager.TransferReason.Fish)
			{
				mode = InfoManager.InfoMode.Fishing;
				subMode = InfoManager.SubInfoMode.Default;
			}
			else
			{
				base.GetPlacementInfoMode(out mode, out subMode, elevation);
			}
		}

		public override string GetDebugString(ushort buildingID, ref Building data)
		{
			string text = base.GetDebugString(buildingID, ref data);
			TransferManager.TransferReason[] incomingResources = m_incomingResources;
			TransferManager.TransferReason transferReason = TransferManager.TransferReason.Shopping;
			for(int i = 0; i < incomingResources.Length; i++)
            {
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				if(incomingResources[i] != TransferManager.TransferReason.None)
                {
					base.CalculateGuestVehicles(buildingID, ref data, incomingResources[i], ref num, ref num2, ref num3, ref num4);
					text = StringUtils.SafeFormat("{0}\n{1}: {2} (+{3})", new object[]
					{
						text,
						incomingResources[i].ToString(),
						data.m_customBuffer1,
						num2
					});
				}
            }
			if (transferReason != TransferManager.TransferReason.None)
			{
				text = StringUtils.SafeFormat("{0}\n{1}: {2}", new object[]
				{
					text,
					transferReason.ToString(),
					data.m_customBuffer2
				});
			}
			return text;
		}

		public override void ModifyMaterialBuffer(ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int amountDelta)
		{
			switch (material)
			{
			case TransferManager.TransferReason.ShoppingB:
			case TransferManager.TransferReason.ShoppingC:
			case TransferManager.TransferReason.ShoppingD:
			case TransferManager.TransferReason.ShoppingE:
			case TransferManager.TransferReason.ShoppingF:
			case TransferManager.TransferReason.ShoppingG:
			case TransferManager.TransferReason.ShoppingH:
				break;
			default:
				if (material != TransferManager.TransferReason.Shopping)
				{
					for(int i = 0; i < m_incomingResources.Length; i++)
                    {
						if (material == m_incomingResources[i])
						{
                            if (!marketBuffers.ContainsKey(buildingID))
                            {
									ushort[] customBuffer = new ushort[m_incomingResources.Length];
									for(int j = 0; j < customBuffer.Length; j++)
                                    {
										customBuffer[j] = data.m_customBuffer1;
                                    }
									marketBuffers.Add(buildingID, customBuffer);
                            }
							var m_customBuffer = marketBuffers[buildingID];
							int goodsCapacity = this.m_goodsCapacity;
							amountDelta = Mathf.Clamp(amountDelta, 0, goodsCapacity - (int)m_customBuffer[i]);
							m_customBuffer[i] = (ushort)((int)m_customBuffer[i] + amountDelta);
							marketBuffers[buildingID] = m_customBuffer;
							return;
						}
                    }
					base.ModifyMaterialBuffer(buildingID, ref data, material, ref amountDelta);	
				}
				break;
			}
			int customBuffer2 = (int)data.m_customBuffer2;
			amountDelta = Mathf.Clamp(amountDelta, -customBuffer2, 0);
			data.m_customBuffer2 = (ushort)(customBuffer2 + amountDelta);
			data.m_outgoingProblemTimer = 0;
			data.m_education1 = (byte)Mathf.Clamp((int)data.m_education1 + (-amountDelta + 99) / 100, 0, 255);
			int num = (-amountDelta * this.m_goodsSellPrice + 50) / 100;
			if (num != 0)
			{
				Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.ResourcePrice, num, this.m_info.m_class);
			}
		}

		public override void BuildingDeactivated(ushort buildingID, ref Building data)
		{
			TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
			offer.Building = buildingID;
			for(int i = 0; i < m_incomingResources.Length; i++)
            {
				if (m_incomingResources[i] != TransferManager.TransferReason.None)
				{
					Singleton<TransferManager>.instance.RemoveIncomingOffer(m_incomingResources[i], offer);
				}
            }
			Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.Entertainment, offer);
			Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.EntertainmentB, offer);
			Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.EntertainmentC, offer);
			Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.EntertainmentD, offer);
			Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.TouristA, offer);
			Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.TouristB, offer);
			Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.TouristC, offer);
			Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.TouristD, offer);
			Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.Shopping, offer);
			Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.ShoppingB, offer);
			Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.ShoppingC, offer);
			Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.ShoppingD, offer);
			Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.ShoppingE, offer);
			Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.ShoppingF, offer);
			Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.ShoppingG, offer);
			Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.ShoppingH, offer);
			base.BuildingDeactivated(buildingID, ref data);
		}
	
		protected override void ProduceGoods(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
		{
			var GetOutgoingTransferReason = this.GetType().GetMethod("GetOutgoingTransferReason", BindingFlags.NonPublic | BindingFlags.Instance);
			base.ProduceGoods(buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
			float num = (float)buildingData.Width * -4f;
			float num2 = (float)buildingData.Width * 4f;
			float num3 = (float)buildingData.Length * -4f;
			float num4 = (float)buildingData.Length * 4f;
			if (this.m_info.m_subBuildings != null)
			{
				for (int i = 0; i < this.m_info.m_subBuildings.Length; i++)
				{
					if (this.m_info.m_subBuildings[i].m_buildingInfo != null)
					{
						float num5 = (float)this.m_info.m_subBuildings[i].m_buildingInfo.m_cellWidth;
						float num6 = (float)this.m_info.m_subBuildings[i].m_buildingInfo.m_cellLength;
						float x = this.m_info.m_subBuildings[i].m_position.x;
						float num7 = -this.m_info.m_subBuildings[i].m_position.z;
						num = Mathf.Min(num, x - num5 * 4f);
						num2 = Mathf.Max(num2, x + num5 * 4f);
						num3 = Mathf.Min(num3, num7 - num6 * 4f);
						num4 = Mathf.Max(num4, num7 + num6 * 4f);
					}
				}
			}
			float num8 = num4 - num3;
			float angle = buildingData.m_angle;
			float num9 = -(num + num2) * 0.5f;
			float num10 = -(num3 + num4) * 0.5f;
			float num11 = num8 * 0.25f - (num3 + num4) * 0.5f;
			float num12 = Mathf.Sin(angle);
			float num13 = Mathf.Cos(angle);
			Vector3 position = buildingData.m_position - new Vector3(num13 * num9 + num12 * num10, 0f, num12 * num9 - num13 * num10);
			Vector3 position2 = buildingData.m_position - new Vector3(num13 * num9 + num12 * num11, 0f, num12 * num9 - num13 * num11);
			float currentRange = this.GetCurrentRange(buildingID, ref buildingData);
			Notification.Problem problem = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.NoCustomers | Notification.Problem.NoGoods | Notification.Problem.NoFishingGoods);
			int num14 = productionRate * this.m_healthCareAccumulation / 100;
			if (num14 != 0)
			{
				Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.HealthCare, num14, buildingData.m_position, this.m_healthCareRadius);
			}
			if (finalProductionRate != 0)
			{
				if (this.m_entertainmentAccumulation != 0)
				{
					int rate = productionRate * this.GetEntertainmentAccumulation(buildingID, ref buildingData) / 100;
					Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Entertainment, rate, position, currentRange);
				}
				if (this.m_attractivenessAccumulation != 0)
				{
					int num15 = this.GetAttractivenessAccumulation(buildingID, ref buildingData);
					Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Attractiveness, num15);
					num15 = productionRate * num15 * 15 / 100;
					Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Attractiveness, num15, position2, num8);
				}
				if (this.m_noiseAccumulation != 0)
				{
					Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, this.m_noiseAccumulation, buildingData.m_position, this.m_noiseRadius);
				}
				base.HandleDead(buildingID, ref buildingData, ref behaviour, totalWorkerCount + totalVisitorCount);
				int goodsCapacity = this.m_goodsCapacity;
				TransferManager.TransferReason outgoingTransferReason =  (TransferManager.TransferReason)GetOutgoingTransferReason.Invoke(this, new object[] { buildingID });
				if (productionRate != 0)
				{
					int num16 = goodsCapacity;
					var m_customBuffer1 = marketBuffers[buildingID];
					for(int i = 0; i < m_customBuffer1.Length; i++)
                    {
						if (m_incomingResources[i] != TransferManager.TransferReason.None)
						{
							num16 = Mathf.Min(num16, (int)m_customBuffer1[i]);
						}
						if (outgoingTransferReason != TransferManager.TransferReason.None)
						{
							num16 = Mathf.Min(num16, goodsCapacity - (int)buildingData.m_customBuffer2);
						}
						productionRate = Mathf.Max(0, Mathf.Min(productionRate, (num16 * 200 + goodsCapacity - 1) / goodsCapacity));
						int num17 = (visitPlaceCount * productionRate + 9) / 10;
						if (Singleton<SimulationManager>.instance.m_isNightTime)
						{
							num17 = num17 + 1 >> 1;
						}
						num17 = Mathf.Max(0, Mathf.Min(num17, num16));
						if (m_incomingResources[i] != TransferManager.TransferReason.None)
						{
							m_customBuffer1[i] -= (ushort)num17;
						}
						if (outgoingTransferReason != TransferManager.TransferReason.None)
						{
							buildingData.m_customBuffer2 += (ushort)num17;
						}
						productionRate += (num17 + 9) / 10;
						marketBuffers[buildingID] = m_customBuffer1;
					}
				}
				int num18 = 0;
				int num19 = 0;
				int num20 = 0;
				int value = 0;
				for(int i = 0; i < m_incomingResources.Length; i++)
                {
					if (m_incomingResources[i] != TransferManager.TransferReason.None)
					{
						base.CalculateGuestVehicles(buildingID, ref buildingData, m_incomingResources[i], ref num18, ref num19, ref num20, ref value);
						buildingData.m_tempImport = (byte)Mathf.Clamp(value, (int)buildingData.m_tempImport, 255);
					}
                }
				
				buildingData.m_tempExport = (byte)Mathf.Clamp(behaviour.m_touristCount, (int)buildingData.m_tempExport, 255);
				buildingData.m_adults = (byte)productionRate;
				int num21 = visitPlaceCount * 500;
				if ((int)buildingData.m_customBuffer2 > goodsCapacity - (num21 >> 1) && aliveVisitorCount <= visitPlaceCount >> 1)
				{
					buildingData.m_outgoingProblemTimer = (byte)Mathf.Min(255, (int)(buildingData.m_outgoingProblemTimer + 1));
					if (buildingData.m_outgoingProblemTimer >= 192)
					{
						problem = Notification.AddProblems(problem, Notification.Problem.NoCustomers | Notification.Problem.MajorProblem);
					}
					else if (buildingData.m_outgoingProblemTimer >= 128)
					{
						problem = Notification.AddProblems(problem, Notification.Problem.NoCustomers);
					}
				}
				else
				{
					buildingData.m_outgoingProblemTimer = 0;
				}
				var m_customBuffer = marketBuffers[buildingID];
				for(int i = 0; i < m_incomingResources.Length; i++)
                {
					if (m_customBuffer[i] == 0 && m_incomingResources[i] != TransferManager.TransferReason.None)
					{
						buildingData.m_incomingProblemTimer = (byte)Mathf.Min(255, (int)(buildingData.m_incomingProblemTimer + 1));
						Notification.Problem problem2 = (m_incomingResources[i] != TransferManager.TransferReason.Fish) ? Notification.Problem.NoGoods : Notification.Problem.NoFishingGoods;
						if (buildingData.m_incomingProblemTimer < 64)
						{
							problem = Notification.AddProblems(problem, problem2);
						}
						else
						{
							problem = Notification.AddProblems(problem, Notification.Problem.MajorProblem | problem2);
						}
					}
					else
					{
						buildingData.m_incomingProblemTimer = 0;
					}
                }
				for(int i = 0; i < m_incomingResources.Length; i++)
                {
					if (buildingData.m_fireIntensity == 0 && m_incomingResources[i] != TransferManager.TransferReason.None)
					{
						int num22 = goodsCapacity - (int)m_customBuffer[i] - num20;
						int num23 = this.m_goodsCapacity / 4;
						num22 -= num23 >> 1;
						if (num22 >= 0)
						{
							TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
							offer.Priority = num22 * 8 / num23;
							offer.Building = buildingID;
							offer.Position = buildingData.m_position;
							offer.Amount = 1;
							offer.Active = false;
							Singleton<TransferManager>.instance.AddIncomingOffer(m_incomingResources[i], offer);
						}
					}
                }
				
				if (buildingData.m_fireIntensity == 0 && outgoingTransferReason != TransferManager.TransferReason.None)
				{
					int num24 = (int)buildingData.m_customBuffer2 - aliveVisitorCount * 100;
					int num25 = Mathf.Max(0, visitPlaceCount - totalVisitorCount);
					if (num24 >= 100 && num25 > 0)
					{
						TransferManager.TransferOffer offer2 = default(TransferManager.TransferOffer);
						offer2.Priority = Mathf.Max(1, num24 * 8 / goodsCapacity);
						offer2.Building = buildingID;
						offer2.Position = buildingData.m_position;
						offer2.Amount = Mathf.Min(num24 / 100, num25);
						offer2.Active = false;
						Singleton<TransferManager>.instance.AddOutgoingOffer(outgoingTransferReason, offer2);
					}
				}
			}
			buildingData.m_problems = problem;
		}
	}
}
