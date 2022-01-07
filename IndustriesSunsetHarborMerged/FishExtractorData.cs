using System.Collections.Generic;
using UnityEngine;

namespace IndustriesSunsetHarborMerged.FishExtractorData {
    public struct FishExtractorData
    {
        public ushort FishFarm { get; set; }

        public Vector3 Position { get; set; }

        public HashSet<string> Prefabs { get; set; }
    }
}
