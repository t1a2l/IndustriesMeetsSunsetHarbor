using ColossalFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

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

    internal static class XMLSerializerUtil {
        static XmlSerializer Serilizer<T>() => new XmlSerializer(typeof(T));
        static void Serialize<T>(TextWriter writer, T value) => Serilizer<T>().Serialize(writer, value);
        static T Deserialize<T>(TextReader reader) => (T)Serilizer<T>().Deserialize(reader);

        public static string Serialize<T>(T value) {
            try {
                using (TextWriter writer = new StringWriter()) {
                    Serialize<T>(writer, value);
                    return writer.ToString();
                }
            }
            catch (Exception ex) {
                LogHelper.Error(ex.ToString());
                return null;
            }
        }

        public static T Deserialize<T>(string data) {
            try {
                using (TextReader reader = new StringReader(data)) {
                    return Deserialize<T>(reader);
                }
            }
            catch (Exception ex) {
                LogHelper.Information("data=" + data);
                LogHelper.Error(ex.ToString());
                return default;
            }
        }
    }
}
