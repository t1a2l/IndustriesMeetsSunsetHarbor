namespace IndustriesSunsetHarborMerged.ItemClassFishFarm {
    public struct ItemClassFishFarm : System.IEquatable<ItemClassFishFarm> {

            public ItemClassFishFarm(ItemClass.Service service) 
            {
                Service = service;
            }

            public ItemClass.Service Service { get; } 

            public override bool Equals(object obj) {
                return obj is ItemClassFishFarm @class && Equals(@class);
            }

            public bool Equals(ItemClassFishFarm other) {
                return Service == other.Service;
            }

            public override int GetHashCode() {
                return 1514353572 + Service.GetHashCode();
            }

            public bool IsValid() 
            {
                return Service != ItemClass.Service.None;
            }

            public static bool operator ==(ItemClassFishFarm left, ItemClassFishFarm right) {
                return left.Equals(right);
            }

            public static bool operator !=(ItemClassFishFarm left, ItemClassFishFarm right) {
                return !(left == right);
            }
        }
}
