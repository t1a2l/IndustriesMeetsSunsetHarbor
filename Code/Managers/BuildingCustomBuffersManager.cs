using System.Collections.Generic;

namespace IndustriesMeetsSunsetHarbor.Managers
{
    public static class BuildingCustomBuffersManager
    {
        public static Dictionary<ushort, BuildingCustomBuffer> BuildingCustomBuffers;

        public struct BuildingCustomBuffer
        {
            public ushort m_customBuffer1;
            public ushort m_customBuffer2;
            public ushort m_customBuffer3;
            public ushort m_customBuffer4;
            public ushort m_customBuffer5;
            public ushort m_customBuffer6;
            public ushort m_customBuffer7;
            public ushort m_customBuffer8;
            public ushort m_customBuffer9;
            public ushort m_customBuffer10;
        }

        public static void Init()
        {
            if(BuildingCustomBuffers == null)
            {
                BuildingCustomBuffers = new();
            }
        }

        public static void Deinit()
        {
            BuildingCustomBuffers = new();
        }

        public static BuildingCustomBuffer GetCustomBuffer(ushort buildingID)
        {
            if(!BuildingCustomBuffers.TryGetValue(buildingID, out BuildingCustomBuffer buffer_struct))
            {
                BuildingCustomBuffers.Add(buildingID, buffer_struct);
            }
            return buffer_struct;
        }
    }
    
}
