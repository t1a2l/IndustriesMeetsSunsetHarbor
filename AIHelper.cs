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
                if (b.name.Equals("(Fish) Algae Bioreactor.Algae Bioreactor_Data"))
                {
                    ChangeBuildingAI(b, typeof(AlgaeBioreactorAI));
                    return;
                }
                else if (b.name.Equals("(Fish) Farm Tanks - Algae.Aquaculture Farm - Algae Tanks_Data") || b.name.Equals("(Fish) Farm Dock - Algae.Aquaculture Dock - Algae_Data") || b.name.Equals("(Fish) Farm Dock - Seaweed.Aquaculture Dock - Seaweed_Data"))
                {
                    ChangeBuildingAI(b, typeof(AlgaeTanksAI));
                    return;
                }
                else if (b.name.Equals("(Factory) Bioplastics Plant.Bioplastics Plant_Data") || b.name.Equals("(Fish) Factory - Fishmeal.Fishmeal Factory_Data"))
                {
                    ChangeBuildingAI(b, typeof(UniqueFactoryOutputAI));
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