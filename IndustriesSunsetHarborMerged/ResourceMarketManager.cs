using ColossalFramework;
using System.Collections.Generic;
using System;

namespace IndustriesSunsetHarborMerged
{
    public class ResourceMarketManager { 
    
        public class MarketData
		{
			public ushort[] inputAmountBuffer;
			public ushort[] outputAmountBuffer;
			public ushort[] amountSold1;
			public ushort[] amountSold2;
		}

		public byte[] Serialize()
		{
			return Convert.FromBase64String(XMLSerializerUtil.Serialize(this));
		}
        public static MarketData Deserialize(byte [] data) {
			return XMLSerializerUtil.Deserialize<MarketData>(Convert.ToBase64String(data));
		}

		public Dictionary<ushort, MarketData> marketBuffers = new Dictionary<ushort, MarketData>();

		protected  static ResourceMarketManager sInstance;

		public static ResourceMarketManager instance
		{
			get
			{
				if (sInstance == null)
				{
					sInstance = new ResourceMarketManager();
					CODebugBase<InternalLogChannel>.VerboseLog(InternalLogChannel.System, "Creating singleton of type " + typeof(ResourceMarketManager).Name);
				}
				return sInstance;
			}
		}

		public static bool exists => sInstance != null;

		public static void Ensure()
		{
			_ = instance;
		}
	}
}
