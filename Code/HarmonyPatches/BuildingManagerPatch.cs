//using HarmonyLib;

//namespace IndustriesMeetsSunsetHarbor.Code.HarmonyPatches
//{
//    [HarmonyPatch(typeof(BuildingManager), "ReleaseBuilding")]
//    public static class BuildingManagerPatch
//    {
//        [HarmonyPrefix]
//        public static void ReleaseBuilding(BuildingManager __instance, ushort building)
//        {
//            ref Building data = ref __instance.m_buildings.m_buffer[(int)building];
//	    if((data.m_flags & Building.Flags.Deleted) != Building.Flags.None)
//            {
//                data.m_flags &= ~Building.Flags.Deleted;
//            }
//        }
//    }
//}
