using ColossalFramework;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IndustriesSunsetHarborMerged.Utils.ResourceMarketManager {
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

            internal TempKeyValue() {}

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
            return Encoding.UTF8.GetBytes(xml);
        }
        public static void Deserialize(byte[] data) {
            if(data == null)
            {
                LogHelper.Information("No data to load!");
                return;
            }
            var xml = Encoding.UTF8.GetString(data);
            var result = XMLSerializerUtil.Deserialize<TempKeyValue[]>(xml);
            result.ToList().ForEach(item => ResourceMarketManager.Instance.marketBuffers[item.key]=item.value); 
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

        public static bool Exists => sInstance != null;

        public static void Ensure() {
            _ = Instance;
        }
    }
}
