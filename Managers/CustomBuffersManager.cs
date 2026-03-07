using System.Collections.Generic;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.Managers
{
    public static class CustomBuffersManager
    {
        public static Dictionary<ushort, CustomBuffer> CustomBuffers;

        public const int RESOURCE_COUNT = 56;

        public struct CustomBuffer
        {
            public float[] m_volumes;

            // Meal counters — not transfer reasons, just internal production tracking
            public float[] m_mealsSitDown;
            public float[] m_mealsDelivery;

            // new — keyed by (int)TransferReason → [low, medium, high]
            public Dictionary<int, int[]> m_qualityBuckets;

            public int m_animalVariationIndex;

            public readonly void Add(int materialId, float amount)
            {
                int idx = GetIndex(materialId);
                if (idx != -1) m_volumes[idx] += amount;
            }

            public readonly void Set(int materialId, float amount)
            {
                int idx = GetIndex(materialId);
                if (idx != -1) m_volumes[idx] = amount;
            }

            public readonly float Get(int materialId)
            {
                int idx = GetIndex(materialId);
                if (idx != -1) return m_volumes[idx];
                return 0;
            }

            public int[] GetQualityBuckets(TransferManager.TransferReason reason)
            {
                m_qualityBuckets ??= [];

                if (!m_qualityBuckets.TryGetValue((int)reason, out var buckets))
                {
                    buckets = new int[3];
                    m_qualityBuckets[(int)reason] = buckets;
                }
                return buckets;
            }

            public void AddQuality(TransferManager.TransferReason reason, byte quality, int amount)
            {
                var buckets = GetQualityBuckets(reason);
                buckets[quality] = Mathf.Max(0, buckets[quality] + amount);
            }

            public void RemoveQuality(TransferManager.TransferReason reason, byte quality, int amount)
            {
                var buckets = GetQualityBuckets(reason);
                buckets[quality] = Mathf.Max(0, buckets[quality] - amount);
            }

            public byte GetBestAvailableQuality(TransferManager.TransferReason reason, int threshold = 8000)
            {
                var buckets = GetQualityBuckets(reason);
                for (byte q = 2; q >= 0; q--)
                    if (buckets[q] >= threshold) return q;
                return 0;
            }
        }

        public static void Init()
        {
            CustomBuffers = [];
        }

        public static void Deinit()
        {
            CustomBuffers = [];
        }

        public static bool CustomBufferExist(ushort buildingID)
        {
            return CustomBuffers.ContainsKey(buildingID);
        }

        public static CustomBuffer GetCustomBuffer(ushort buildingID)
        {
            if(!CustomBuffers.TryGetValue(buildingID, out CustomBuffer buffer_struct))
            {
                buffer_struct = new CustomBuffer
                {
                    m_volumes = new float[RESOURCE_COUNT],
                    m_mealsSitDown = new float[4],
                    m_mealsDelivery = new float[4],
                    m_qualityBuckets = [],
                    m_animalVariationIndex = 0
                };
                CustomBuffers.Add(buildingID, buffer_struct);
            }
            return buffer_struct;
        }

        public static void SetCustomBuffer(ushort buildingID, CustomBuffer buffer_struct)
        {
            CustomBuffers[buildingID] = buffer_struct;
        }

        public static void RemoveCustomBuffer(ushort buildingID)
        {
            CustomBuffers.Remove(buildingID);
        }

        public static int GetIndex(int materialId)
        {
            return materialId switch
            {
                13 => 0, // Oil
                14 => 1, // Ore
                15 => 2, // Logs
                16 => 3, // Grain
                17 => 4, // Goods
                19 => 5, // Coal
                31 => 6, // Petrol
                32 => 7, // Food
                37 => 8, // Lumber
                97 => 9, // AnimalProducts
                98 => 10, // Flours
                99 => 11, // Paper
                100 => 12, // PlanedTimber
                101 => 13, // Petroleum
                102 => 14, // Plastics
                103 => 15, // Glass
                104 => 16, // Metals
                105 => 17, // LuxuryProducts (Jewelry)
                108 => 18, // Fish
                153 => 19, // Anchovy
                154 => 20, // Salmon
                155 => 21, // Shellfish
                156 => 22, // Tuna
                157 => 23, // Algae
                158 => 24, // Seaweed
                159 => 25, // Mussels
                160 => 26, // Trout
                161 => 27, // Milk
                162 => 28, // RawHides
                163 => 29, // Pork
                164 => 30, // Fruits
                165 => 31, // Vegetables
                166 => 32, // Wool
                167 => 33, // Cotton
                172 => 34, // ProcessedVegetableOil
                173 => 35, // LiquidConcentrates
                174 => 36, // FishMeal
                175 => 37, // FishOil
                176 => 38, // ChemicalProducts
                177 => 39, // Leather
                178 => 40, // FoodProducts
                179 => 41, // BeverageProducts
                180 => 42, // BakedGoods
                181 => 43, // CannedFish
                182 => 44, // Furnitures
                183 => 45, // ElectronicProducts
                184 => 46, // IndustrialSteel
                185 => 47, // Tupperware
                186 => 48, // Toys
                187 => 49, // PrintedProducts
                188 => 50, // TissuePaper
                189 => 51, // Cloths
                190 => 52, // PetroleumProducts
                191 => 53, // Cars
                192 => 54, // Footwear
                193 => 55, // HouseParts
                _ => -1,
            };
        }
    }
    
}
