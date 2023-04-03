using ColossalFramework;
using ColossalFramework.UI;
using HarmonyLib;
using IndustriesMeetsSunsetHarbor.Utils;
using System.Reflection;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(NotificationManager))]
    public static class NotificationManagerPatch
    {

        private delegate bool SimulationManagerInitializePropertiesBaseDelegate(SimulationManagerBase<NotificationManager, NotificationProperties> __instance, NotificationProperties properties);
        private static readonly SimulationManagerInitializePropertiesBaseDelegate InitializePropertiesBase = AccessTools.MethodDelegate<SimulationManagerInitializePropertiesBaseDelegate>(typeof(SimulationManagerBase<NotificationManager, NotificationProperties>).GetMethod("InitializeProperties", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        [HarmonyPatch(typeof(NotificationManager), "PopulateGroupData")]
        [HarmonyPostfix]
        public static void Awake(NotificationManager __instance)
        {
            int num = 25;
            __instance.m_groupData = new NotificationManager.GroupData[num * num * 69];
            var m_groupData2 = new NotificationManager.GroupData[num * num * 69];
            typeof(NotificationManager).GetField("m_groupData2", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_groupData2);

        }


        [HarmonyPatch(typeof(NotificationManager), "InitializeProperties")]
        [HarmonyPrefix]
        public static bool InitializeProperties(NotificationManager __instance, NotificationProperties properties)
        {
            InitializePropertiesBase(__instance, properties);
            Notification.ProblemStruct all = Notification.ProblemStruct.All;
            var DeliveryNotificationAtlas = TextureUtils.GetAtlas("DeliveryNotificationAtlas");
            for (int i = 0; i < 69; i++)
            {
                Notification.ProblemStruct problemStruct = all[i];
                string text;
                string text2;
                string text3;
                if (problemStruct.m_Problems1 != Notification.Problem1.None)
                {
                    text = problemStruct.m_Problems1.Name("Normal");
                    text2 = problemStruct.m_Problems1.Name("Major");
                    text3 = problemStruct.m_Problems1.Name("Fatal");
                }
                else
                {
                    text = problemStruct.m_Problems2.Name("Normal");
                    text2 = problemStruct.m_Problems2.Name("Major");
                    text3 = problemStruct.m_Problems2.Name("Fatal");
                }
                if (!string.IsNullOrEmpty(text))
                {
                    UITextureAtlas.SpriteInfo spriteInfo;
                    if(i == 68)
                    {
                        spriteInfo = DeliveryNotificationAtlas["BuildingNotificationNotEnoughFoodDeliveryFirst"];
                    }
                    else
                    {
                        spriteInfo = properties.m_notificationAtlas[text];
                    }
                    if (spriteInfo != null)
                    {
                        Rect region = spriteInfo.region;
                        __instance.m_spriteAtlasRegions[i] = new Vector4(region.xMin, region.yMin, region.xMax, region.yMax);
                    }
                }
                if (!string.IsNullOrEmpty(text2))
                {
                    UITextureAtlas.SpriteInfo spriteInfo2;
                    if(i == 68)
                    {
                        spriteInfo2 = DeliveryNotificationAtlas["BuildingNotificationNotEnoughFoodDelivery"];
                    }
                    else
                    {
                        spriteInfo2 = properties.m_notificationAtlas[text2];
                    }
                    if (spriteInfo2 != null)
                    {
                        Rect region2 = spriteInfo2.region;
                        __instance.m_spriteAtlasRegions[69 + i] = new Vector4(region2.xMin, region2.yMin, region2.xMax, region2.yMax);
                    }
                }
                if (!string.IsNullOrEmpty(text3))
                {
                    UITextureAtlas.SpriteInfo spriteInfo3;
                    if(i == 68)
                    {
                        spriteInfo3 = DeliveryNotificationAtlas["BuildingNotificationNotEnoughFoodDeliveryCritical"];
                    }
                    else
                    {
                        spriteInfo3 = properties.m_notificationAtlas[text3];
                    }
                    if (spriteInfo3 != null)
                    {
                        Rect region3 = spriteInfo3.region;
                        __instance.m_spriteAtlasRegions[136 + i] = new Vector4(region3.xMin, region3.yMin, region3.xMax, region3.yMax);
                    }
                }
            }
            for (int j = 0; j < 28; j++)
            {
                NotificationEvent.Type type = (NotificationEvent.Type)j;
                string text4 = type.Name<NotificationEvent.Type>();
                UITextureAtlas.SpriteInfo spriteInfo4 = properties.m_notificationAtlas[text4];
                if (spriteInfo4 != null)
                {
                    Rect region4 = spriteInfo4.region;
                    __instance.m_spriteAtlasRegions[204 + j] = new Vector4(region4.xMin, region4.yMin, region4.xMax, region4.yMax);
                }
            }
            __instance.m_notificationMaterial = new Material(__instance.m_properties.m_notificationAtlas.material);
            __instance.m_notificationMaterial.shader = __instance.m_properties.m_notificationShader;
            GameObject gameObject = GameObject.FindGameObjectWithTag("MainCamera");
            if (gameObject != null)
            {
                typeof(NotificationManager).GetField("m_cameraController", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, gameObject.GetComponent<CameraController>());
            }
            return false;
        }

        [HarmonyPatch(typeof(NotificationManager), "InitializeProperties")]
        [HarmonyPrefix]
        public static bool EndRenderingImpl(NotificationManager __instance, RenderManager.CameraInfo cameraInfo)
        {
            FastList<RenderGroup> renderedGroups = Singleton<RenderManager>.instance.m_renderedGroups;
            var m_groupData2 = (NotificationManager.GroupData[])typeof(NotificationManager).GetField("m_groupData2", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_groupDataCount = (byte[])typeof(NotificationManager).GetField("m_groupDataCount", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            for (int i = 0; i < renderedGroups.m_size; i++)
            {
                RenderGroup renderGroup = renderedGroups.m_buffer[i];
                if ((renderGroup.m_layersRendered & (renderGroup.m_instanceMask = 0) & (1 << __instance.m_notificationLayer)) != 0)
                {
                    int num = 25;
                    int num2 = 45 - num >> 1;
                    int num3 = renderGroup.m_x - num2;
                    int num4 = renderGroup.m_z - num2;
                    if (num3 >= 0 && num4 >= 0 && num3 < num && num4 < num)
                    {
                        int num5 = num4 * num + num3;
                        int num6 = (int)m_groupDataCount[num5];
                        num5 *= 69;
                        for (int j = 0; j < num6; j++)
                        {
                            NotificationManager.GroupData groupData = m_groupData2[num5 + j];
                            if (groupData.m_problems.IsNotNone)
                            {
                                Vector3 vector;
                                vector.x = (groupData.m_minPos.x + groupData.m_maxPos.x) * 0.5f;
                                vector.y = groupData.m_maxPos.y;
                                vector.z = (groupData.m_minPos.z + groupData.m_maxPos.z) * 0.5f;
                                Notification.RenderInstance(cameraInfo, groupData.m_problems, vector, groupData.m_size);
                            }
                        }
                    }
                }
            }
            typeof(NotificationManager).GetField("m_groupData2", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_groupData2);
            typeof(NotificationManager).GetField("m_groupDataCount", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_groupDataCount);


            var m_events = (FastList<NotificationEvent>)typeof(NotificationManager).GetField("m_events", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var m_cameraController = (CameraController)typeof(NotificationManager).GetField("m_cameraController", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);


            for (int k = 0; k < m_events.m_size; k++)
            {
                m_events.m_buffer[k].RenderInstance(cameraInfo);
            }
            if (m_cameraController != null)
            {
                InstanceID target = m_cameraController.GetTarget();
                if (!target.IsEmpty && target.Type != InstanceType.Disaster && target != WorldInfoPanel.GetCurrentInstanceID() && InstanceManager.GetPosition(target, out Vector3 vector2, out Quaternion quaternion, out Vector3 vector3))
                {
                    vector2.y += vector3.y * 0.85f + 2f;
                    NotificationEvent.RenderInstance(cameraInfo, NotificationEvent.Type.LocationMarker, vector2, 0.75f, 1f);
                }
            }
            return false;
        }

        [HarmonyPatch(typeof(NotificationManager), "CalculateGroupData")]
        [HarmonyPrefix]
        public static bool CalculateGroupData(NotificationManager __instance, int groupX, int groupZ, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays)
        {
            if (layer == __instance.m_notificationLayer)
            {
                int num = 25;
                int num2 = 45 - num >> 1;
                groupX -= num2;
                groupZ -= num2;
                if (groupX >= 0 && groupZ >= 0 && groupX < num && groupZ < num)
                {
                    int num3 = (groupZ * num + groupX) * 69;
                    for (int i = 0; i < 69; i++)
                    {
                        NotificationManager.GroupData groupData;
                        groupData.m_problems = Notification.ProblemStruct.None;
                        groupData.m_size = 0f;
                        groupData.m_minPos = new Vector3(100000f, 100000f, 100000f);
                        groupData.m_maxPos = new Vector3(-100000f, -100000f, -100000f);
                        __instance.m_groupData[num3 + i] = groupData;
                    }
                }
            }
            return false;
        }

        [HarmonyPatch(typeof(NotificationManager), "PopulateGroupData")]
        [HarmonyPrefix]
        public static bool PopulateGroupData(NotificationManager __instance, int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance, ref bool requireSurfaceMaps)
        {
            if (layer == __instance.m_notificationLayer)
            {
                int num = 25;
                int num2 = 45 - num >> 1;
                groupX -= num2;
                groupZ -= num2;
                if (groupX >= 0 && groupZ >= 0 && groupX < num && groupZ < num)
                {
                    int num3 = (groupZ * num + groupX) * 69;
                    int num4 = 0;
                    for (int i = 0; i < 69; i++)
                    {
                        NotificationManager.GroupData groupData = __instance.m_groupData[num3 + i];
                        if (groupData.m_problems.IsNotNone)
                        {
                            bool flag = false;
                            for (int j = 0; j < num4; j++)
                            {
                                NotificationManager.GroupData groupData2 = __instance.m_groupData[num3 + j];
                                if (groupData2.m_minPos == groupData.m_minPos && groupData2.m_maxPos == groupData.m_maxPos)
                                {
                                    groupData2.m_problems = Notification.AddProblems(groupData2.m_problems, groupData.m_problems);
                                    groupData2.m_size += groupData.m_size;
                                    __instance.m_groupData[num3 + j] = groupData2;
                                    flag = true;
                                    break;
                                }
                            }
                            if (!flag)
                            {
                                __instance.m_groupData[num3 + num4] = groupData;
                                num4++;
                            }
                        }
                    }

                    var m_groupData2 = (NotificationManager.GroupData[])typeof(NotificationManager).GetField("m_groupData2", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                    var m_groupDataCount = (byte[])typeof(NotificationManager).GetField("m_groupDataCount", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

                    for (int k = 0; k < num4; k++)
                    {
                        NotificationManager.GroupData groupData3 = __instance.m_groupData[num3 + k];
                        groupData3.m_size = Mathf.Min(2f, Mathf.Sqrt(groupData3.m_size));
                        m_groupData2[num3 + k] = groupData3;
                    }
                    for (int l = num4; l < 69; l++)
                    {
                        m_groupData2[num3 + l] = default(NotificationManager.GroupData);
                    }
                    m_groupDataCount[groupZ * num + groupX] = (byte)num4;

                    typeof(NotificationManager).GetField("m_groupData2", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_groupData2);
                    typeof(NotificationManager).GetField("m_groupDataCount", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, m_groupDataCount);

                }
            }
            return false;
        }

    }
}
