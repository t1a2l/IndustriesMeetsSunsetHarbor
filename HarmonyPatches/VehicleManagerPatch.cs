using HarmonyLib;
using System.Reflection;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(VehicleManager), "RefreshTransferVehicles")]
    public static class VehicleManagerPatch
    {
        public static void Prefix(VehicleManager __instance)
        {
            var m_transferVehicles = (FastList<ushort>[])typeof(VehicleManager).GetField("m_transferVehicles", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            int num2 = PrefabCollection<VehicleInfo>.PrefabCount();
            for (int j = 0; j < num2; j++)
            {
                VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab((uint)j);
                if (prefab != null && prefab.m_class.m_service != 0 && prefab.m_placementStyle == ItemClass.Placement.Automatic)
                {
                    int transferIndex = GetTransferIndex(prefab.m_class.m_service, prefab.m_class.m_subService, prefab.m_class.m_level);
                    if(transferIndex >= m_transferVehicles.Length)
                    {
                        prefab.m_class.m_service = ItemClass.Service.Commercial;
                        prefab.m_class.m_subService = ItemClass.SubService.None;
                        prefab.m_class.m_level = ItemClass.Level.Level1;
                    }

                }
            }
        }

        private static int GetTransferIndex(ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level)
        {
	        int num = ((subService == ItemClass.SubService.None) ? ((int)(service - 1)) : ((int)(26 + subService - 1)));
	        return (int)(num * 5 + level);
        }
    }
}
