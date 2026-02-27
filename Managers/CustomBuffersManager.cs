using System.Collections.Generic;

namespace IndustriesMeetsSunsetHarbor.Managers
{
    public static class CustomBuffersManager
    {
        public static Dictionary<ushort, CustomBuffer> CustomBuffers;

        public struct CustomBuffer
        {
            public float[] m_volumes;

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

        public static CustomBuffer GetCustomBuffer(ushort buildingID)
        {
            if(!CustomBuffers.TryGetValue(buildingID, out CustomBuffer buffer_struct))
            {
                buffer_struct = new CustomBuffer
                {
                    m_volumes = new float[52] // 52 materials
                };
                CustomBuffers.Add(buildingID, buffer_struct);
            }
            return buffer_struct;
        }

        public static void SetCustomBuffer(ushort buildingID, CustomBuffer buffer_struct)
        {
            CustomBuffers[buildingID] = buffer_struct;
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
                172 => 34, // ChemicalProducts
                173 => 35, // Leather
                174 => 36, // FoodProducts
                175 => 37, // BeverageProducts
                176 => 38, // BakedGoods
                177 => 39, // CannedFish
                178 => 40, // Furnitures
                179 => 41, // ElectronicProducts
                180 => 42, // IndustrialSteel
                181 => 43, // Tupperware
                182 => 44, // Toys
                183 => 45, // PrintedProducts
                184 => 46, // TissuePaper
                185 => 47, // Cloths
                186 => 48, // PetroleumProducts
                187 => 49, // Cars
                188 => 50, // Footwear
                189 => 51, // HouseParts
                _ => -1,
            };
        }
    }
    
}
