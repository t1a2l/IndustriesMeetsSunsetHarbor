using System.Collections.Generic;
using static Notification;

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
        }

        public static void Init()
        {
            if(CustomBuffers == null)
            {
                CustomBuffers = new();
            }
        }

        public static void Deinit()
        {
            CustomBuffers = new();
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
