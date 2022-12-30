using System.Collections.Generic;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.Utils
{
    public struct AquacultureExtractorData
    {
        public ushort AquacultureFarm { get; set; }

        public Vector3 Position { get; set; }

        public HashSet<string> Prefabs { get; set; }
    }
}
