using System;
using System.Collections.Generic;
using System.Reflection;

namespace FishIndustryEnhanced
{
    public static class AIHelper
    {

        public static void ApplyNewAIToBuilding(BuildingInfo b)
        {
            try
            {
                if (b.name.Equals("Algae Bioreactor"))
                {
                    ChangeBuildingAI(b, typeof(AlgaeBioreactorAI));
                    return;
                }
                else if (b.name.Equals("Aquaculture Farm - Algae Tanks"))
                {
                    ChangeBuildingAI(b, typeof(AlgaeTank));
                    return;
                }
                else if (b.name.Equals("Fish Hatchery - Long") || b.name.Equals("Fish Hatchery - Wide"))
                {
                    ChangeBuildingAI(b, typeof(FishHatchery));
                    return;
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e.ToString());
            }
        }

        private static void ChangeBuildingAI(BuildingInfo b, Type AIType)
        {
            //Delete old AI
            var oldAI = b.gameObject.GetComponent<PrefabAI>();
            UnityEngine.Object.DestroyImmediate(oldAI);

            //Add new AI
            var newAI = (PrefabAI)b.gameObject.AddComponent(AIType);
            TryCopyAttributes(oldAI, newAI, false);
            b.InitializePrefab();
        }

        private static void TryCopyAttributes(PrefabAI src, PrefabAI dst, bool safe = true)
        {
            var oldAIFields = src.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            var newAIFields = dst.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

            var newAIFieldDic = new Dictionary<string, FieldInfo>(newAIFields.Length);
            foreach (var field in newAIFields)
            {
                newAIFieldDic.Add(field.Name, field);
            }

            foreach (var fieldInfo in oldAIFields)
            {
                bool copyField = !fieldInfo.IsDefined(typeof(NonSerializedAttribute), true);

                if (safe && !fieldInfo.IsDefined(typeof(CustomizablePropertyAttribute), true)) copyField = false;

                if (copyField)
                {
                    FieldInfo newAIField;
                    newAIFieldDic.TryGetValue(fieldInfo.Name, out newAIField);
                    try
                    {
                        if (newAIField != null && newAIField.GetType().Equals(fieldInfo.GetType()))
                        {
                            newAIField.SetValue(dst, fieldInfo.GetValue(src));
                        }
                    }
                    catch (NullReferenceException)
                    {
                    }
                }
            }
        }
    }
}