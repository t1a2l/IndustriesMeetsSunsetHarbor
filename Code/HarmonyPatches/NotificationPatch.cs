using ColossalFramework;
using HarmonyLib;
using UnityEngine;
using static Notification;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(Notification))]
    public static class NotificationPatch
    {
        
        [HarmonyPatch(typeof(Notification), "PopulateGroupData")]
        [HarmonyPrefix]
        public static bool Prefix(ProblemStruct problems, Vector3 position, float scale, int groupX, int groupZ, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance)
        {
            min = Vector3.Min(min, position - new Vector3(10f, 10f, 10f));
            max = Vector3.Max(max, position + new Vector3(10f, 10f, 10f));
            maxRenderDistance = Mathf.Max(maxRenderDistance, 100000f);
            maxInstanceDistance = Mathf.Max(maxInstanceDistance, 1000f);
            if (Singleton<NotificationManager>.instance.FadeAwayNotifications)
            {
                return false;
            }
            int num = 25;
            int num2 = 45 - num >> 1;
            groupX -= num2;
            groupZ -= num2;
            if (groupX < 0 || groupZ < 0 || groupX >= num || groupZ >= num)
            {
                return false;
            }
            int num3 = (groupZ * num + groupX) * NotificationManagerPatch.NotificationArrayLength;
            for (int i = 0; i < NotificationManagerPatch.NotificationArrayLength; i++)
            {
                ProblemStruct problemStruct = problems[i];
                if (problemStruct.IsNotNone)
                {
                    problemStruct |= problems & (Problem1.MajorProblem | Problem1.FatalProblem);
                    NotificationManager.GroupData groupData = Singleton<NotificationManager>.instance.m_groupData[num3 + i];
                    groupData.m_problems = AddProblems(groupData.m_problems, problemStruct);
                    groupData.m_size += scale;
                    groupData.m_minPos = Vector3.Min(groupData.m_minPos, position);
                    groupData.m_maxPos = Vector3.Max(groupData.m_maxPos, position);
                    Singleton<NotificationManager>.instance.m_groupData[num3 + i] = groupData;
                }
            }
            return false;
        }

        [HarmonyPatch(typeof(Notification), "RenderInstance")]
        [HarmonyPrefix]
        public static bool RenderInstance(RenderManager.CameraInfo cameraInfo, ProblemStruct problems, Vector3 position, float scale)
        {
            NotificationManager instance = Singleton<NotificationManager>.instance;
            int num = problems.Count;
            if (num != 0)
            {
                Vector4 vector = new Vector4(0.1f, 1f, 5f, 0f);
                int num2 = 0;
                if ((problems & ProblemStruct.Fatal).IsNotNone)
                {
                    num2 = 136;
                    vector.x = 0.2f;
                    vector.z = 6f;
                }
                else if ((problems & ProblemStruct.Major).IsNotNone)
                {
                    num2 = NotificationManagerPatch.NotificationArrayLength;
                    vector.x = 0.2f;
                    vector.z = 6f;
                }
                if (instance.FadeAwayNotifications)
                {
                    float num3 = Vector3.SqrMagnitude(cameraInfo.m_position - position) * 1E-06f;
                    vector.y = Mathf.Max(0f, 1f - num3 * num3);
                }
                num = (int)Time.realtimeSinceStartup % num;
                for (int i = 0; i < NotificationManagerPatch.NotificationArrayLength; i++)
                {
                    if (problems[i].IsNotNone && num-- == 0)
                    {
                        NotificationManager.BufferedItem bufferedItem;
                        bufferedItem.m_position = new Vector4(position.x, position.y, position.z, scale);
                        bufferedItem.m_params = vector;
                        bufferedItem.m_distanceSqr = Vector3.SqrMagnitude(position - cameraInfo.m_position);
                        bufferedItem.m_regionIndex = num2 + i;
                        instance.m_bufferedItems.Add(bufferedItem);
                        break;
                    }
                }
            }
            return false;
        }

    }

}
