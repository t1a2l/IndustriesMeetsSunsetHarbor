using System.Collections.Generic;

namespace IndustriesMeetsSunsetHarbor.Managers
{
    public static class CustomBuffersManager
    {
        public static Dictionary<ushort, CustomBuffer> CustomBuffers;

        public const int RESOURCE_COUNT = 53;

        public struct CustomBuffer
        {
            public float[] m_volumes;

            // Meal counters — not transfer reasons, just internal production tracking
            public float[] m_mealsSitDown;
            public float[] m_mealsDelivery;

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
                    m_mealsDelivery = new float[4]
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
                159 => 25, // Trout
                160 => 26, // Milk
                161 => 27, // RawHides
                162 => 28, // Pork
                163 => 29, // Fruits
                164 => 30, // Vegetables
                165 => 31, // Wool
                166 => 32, // Cotton
                171 => 33, // ProcessedVegetableOil
                172 => 34, // LiquidConcentrates
                173 => 35, // ChemicalProducts
                174 => 36, // Leather
                175 => 37, // FoodProducts
                176 => 38, // BeverageProducts
                177 => 39, // BakedGoods
                178 => 40, // CannedFish
                179 => 41, // Furnitures
                180 => 42, // ElectronicProducts
                181 => 43, // IndustrialSteel
                182 => 44, // Tupperware
                183 => 45, // Toys
                184 => 46, // PrintedProducts
                185 => 47, // TissuePaper
                186 => 48, // Cloths
                187 => 49, // PetroleumProducts
                188 => 50, // Cars
                189 => 51, // Footwear
                190 => 52, // HouseParts
                _ => -1,
            };
        }
    }
    
}
