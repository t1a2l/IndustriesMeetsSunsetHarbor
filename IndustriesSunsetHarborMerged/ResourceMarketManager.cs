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
            var xml = XMLSerializerUtil.Serialize(this);
            LogHelper.Information(xml);
            return Convert.FromBase64String(xml);
        }
        public static MarketData Deserialize(byte[] data) {
            var info = Convert.ToBase64String(data);
            LogHelper.Information(info);
            return XMLSerializerUtil.Deserialize<MarketData>(info);
        }

        public Dictionary<ushort, MarketData> marketBuffers = new Dictionary<ushort, MarketData>();

        protected static ResourceMarketManager sInstance;

        public static ResourceMarketManager instance {
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
            _ = instance;
        }
    }
}
