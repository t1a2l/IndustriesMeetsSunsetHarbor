using System.Collections.Generic;
using static Notification;

namespace IndustriesMeetsSunsetHarbor.Managers
{
    public static class CustomBuffersManager
    {
        public static Dictionary<ushort, CustomBuffer> CustomBuffers;

        public struct CustomBuffer
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

        public static Problem2 WaitingDelivery = (Problem2)64UL;

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
