using ColossalFramework;
using System;
using System.Collections.Generic;

namespace IndustriesSunsetHarborMerged {
    public class ResourceMarketManager {

        public class MarketData {
            public ushort[] inputAmountBuffer;
            public ushort[] outputAmountBuffer;
            public ushort[] amountSold1;
            public ushort[] amountSold2;
        }

        public byte[] Serialize() {
            LogHelper.Information("Serialize this is: ", this);
            var xml = XMLSerializerUtil.Serialize(this);
            LogHelper.Information("XML is: ", xml);
            return Convert.FromBase64String(xml);
        }
        public static MarketData Deserialize(byte[] data) {
            LogHelper.Information("Deserialize data is: ", data.ToString());
            var info = Convert.ToBase64String(data);
            LogHelper.Information("info is: ", info);
            return XMLSerializerUtil.Deserialize<MarketData>(info);
        }

        public Dictionary<ushort, MarketData> marketBuffers = new();

        protected static ResourceMarketManager sInstance;

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
