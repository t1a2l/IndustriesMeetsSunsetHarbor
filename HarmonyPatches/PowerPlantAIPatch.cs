using HarmonyLib;
using System;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{

    [HarmonyPatch(typeof(PowerPlantAI), "GetLocalizedStats")]
    public static class PowerPlantAIPatch
    {

        [HarmonyPrefix]
        public static bool Prefix()
        {
            return false;
        }

        [HarmonyPostfix]
        public static void Postfix(PowerPlantAI __instance, ushort buildingID, ref Building data, ref string __result)
        {
            int electricityRate = __instance.GetElectricityRate(buildingID, ref data);
            string text = LocaleFormatter.FormatGeneric("AIINFO_ELECTRICITY_PRODUCTION",
            [
                (electricityRate * 16 + 500) / 1000
            ]);
            text += Environment.NewLine;
            if (__instance.m_resourceType != TransferManager.TransferReason.None)
            {
                string name = __instance.m_resourceType.ToString();
                name = name.Replace("Grain", "Crops");
                name = name.Replace("Flours", "Flour");
                name = name.Replace("Metals", "Metal");
                name = name.Replace("Petroleum", "Petroleum");
                name = name.Replace("Logs", "Raw Forest Products");
                name = name.Replace("Lumber", "Zoned Forest Goods");
                int resourceDuration = __instance.GetResourceDuration(buildingID, ref data);
                text += name + " stored for " + resourceDuration + " weeks";
            }
            __result = text;
        }
    }
}
