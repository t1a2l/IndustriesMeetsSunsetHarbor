using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IndustriesSunsetHarborMerged {
    public class ResourceMarketManager {

        public class MarketData {
            public ushort[] inputAmountBuffer;
            public ushort[] outputAmountBuffer;
            public ushort[] amountSold1;
            public ushort[] amountSold2;
        }

        public class TempKeyValue {
            public ushort key;
            public MarketData value;

            public TempKeyValue(ushort buildingId, MarketData marketdata) {
                key = buildingId;
                value = marketdata;
            }
}

        protected static ResourceMarketManager sInstance;

        public Dictionary<ushort, MarketData> marketBuffers = new();

        public byte[] Serialize() {
            var result = ResourceMarketManager.Instance.marketBuffers.Select(kv => new TempKeyValue(kv.Key, kv.Value)).ToArray();
            var xml = XMLSerializerUtil.Serialize(result);
            return Convert.FromBase64String(xml);
        }
        public static void Deserialize(byte[] data) {
            var str = Convert.ToBase64String(data);
            var result = XMLSerializerUtil.Deserialize<TempKeyValue[]>(str);
            result.ForEach(item => ResourceMarketManager.Instance.marketBuffers[item.key]=item.value); 
        }

        

        

        public static ResourceMarketManager Instance {
            get {
                if (sInstance == null) {
                    sInstance = new ResourceMarketManager();
                    CODebugBase<InternalLogChannel>.VerboseLog(InternalLogChannel.System, "Creating singleton of type " + typeof(ResourceMarketManager).Name);
                }
                return sInstance;
            }
        }

        public static bool exists => sInstance != null;

        public static void Ensure() {
            _ = Instance;
        }
    }
}
