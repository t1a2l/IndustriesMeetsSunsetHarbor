using System.Collections.Generic;

namespace IndustriesMeetsSunsetHarbor.Managers
{
    public static class CustomBuffersManager
    {
        public static Dictionary<ushort, CustomBuffer> CustomBuffers;

        public struct CustomBuffer
        {
            public float m_customBuffer1;
            public float m_customBuffer2;
            public float m_customBuffer3;
            public float m_customBuffer4;
            public float m_customBuffer5;
            public float m_customBuffer6;
            public float m_customBuffer7;
            public float m_customBuffer8;
            public float m_customBuffer9;
            public float m_customBuffer10;
            public float m_customBuffer11;
            public float m_customBuffer12;
            public float m_customBuffer13;
            public float m_customBuffer14;
            public float m_customBuffer15;
            public float m_customBuffer16;
            public float m_customBuffer17;
            public float m_customBuffer18;
            public float m_customBuffer19;
            public float m_customBuffer20;
        }

        public static void Init()
        {
            CustomBuffers = [];
        }

        public static void Deinit()
        {
            CustomBuffers = [];
        }

        public static CustomBuffer GetCustomBuffer(ushort bufferID)
        {
            if(!CustomBuffers.TryGetValue(bufferID, out CustomBuffer buffer_struct))
            {
                CustomBuffers.Add(bufferID, buffer_struct);
            }
            return buffer_struct;
        }

        public static void SetCustomBuffer(ushort bufferID, CustomBuffer buffer_struct)
        {
            CustomBuffers[bufferID] = buffer_struct;
        }
    }
    
}
