using HarmonyLib;
using System.Collections.Generic;
using static Notification;

namespace IndustriesMeetsSunsetHarbor.Code.HarmonyPatches
{
    [HarmonyPatch(typeof(ProblemStruct))]
    public static class ProblemStructPatch
    {
        [HarmonyPatch(typeof(ProblemStruct), "get_Count", MethodType.Getter)]
        [HarmonyPrefix]
        public static bool get_Count(ProblemStruct __instance, ref int __result)
        {
            if (__instance.IsNone)
            {
                __result = 0;
                return false;
            }
            int num = 0;
            for (int i = 0; i < 62; i++)
            {
                if ((__instance.m_Problems1 & (Notification.Problem1)(1L << i)) != Notification.Problem1.None)
                {
                    num++;
                }
            }
            for (int j = 0; j < 7; j++)
            {
                if ((__instance.m_Problems2 & (Notification.Problem2)(1L << j)) != Notification.Problem2.None)
                {
                    num++;
                }
            }
            __result = num;
            return false;
        }

        [HarmonyPatch(typeof(ProblemStruct), "GetEnumerator")]
        [HarmonyPrefix]
        public static bool Prefix(ProblemStruct __instance, ref IEnumerator<ProblemStruct> __result)
        {
            __result = GetEnumerator(__instance);
            return false;
        }

        private static IEnumerator<ProblemStruct> GetEnumerator(ProblemStruct __instance)
        {
            for (int i = 0; i < 62; i++)
            {
                if ((__instance.m_Problems1 & (Notification.Problem1)(1L << i)) != Notification.Problem1.None)
                {
                    yield return (Notification.Problem1)(1L << i);
                }
            }
            for (int j = 0; j < 7; j++)
            {
                if ((__instance.m_Problems2 & (Notification.Problem2)(1L << j)) != Notification.Problem2.None)
                {
                    yield return (Notification.Problem2)(1L << j);
                }
            }
            yield break;
        }

        [HarmonyPatch(typeof(ProblemStruct), "get_Item", MethodType.Getter)]
        [HarmonyPrefix]
        public static bool get_Item(ProblemStruct __instance, int index, ref ProblemStruct __result)
        {
            if (index < 0 || index >= 69)
            {
                __result = Notification.ProblemStruct.None;
                 return false;
            }
            if (index < 62)
            {
                __result = new Notification.ProblemStruct((Notification.Problem1)((1L << index) & (long)__instance.m_Problems1));
                return false;
            }
            __result = new Notification.ProblemStruct((Notification.Problem2)((1L << index - 62) & (long)__instance.m_Problems2));
            return false;
        }

        

    }
}
