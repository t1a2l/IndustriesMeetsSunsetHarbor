using System.Collections.Generic;

namespace IndustriesMeetsSunsetHarbor.Managers
{
    public static class ResourceMarketManager
    {
        public struct MarketData
        {
            public ushort[] inputAmountBuffer;
            public ushort[] outputAmountBuffer;
            public ushort[] amountSold1;
            public ushort[] amountSold2;
        }

        public static Dictionary<ushort, MarketData> MarketBuffers = new();
    }
}
