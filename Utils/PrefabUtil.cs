using System;
using System.Collections.Generic;
using System.Reflection;

namespace IndustriesMeetsSunsetHarbor.Utils
{
    public static class PrefabUtil
    {
        public static void TryCopyAttributes<T>(T src, T dst, bool safe = true)
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
                // do not copy attributes marked NonSerialized
                bool copyField = !fieldInfo.IsDefined(typeof(NonSerializedAttribute), true);

                if (safe && !fieldInfo.IsDefined(typeof(CustomizablePropertyAttribute), true)) copyField = false;

                if (copyField)
                {
                    newAIFieldDic.TryGetValue(fieldInfo.Name, out FieldInfo newAIField);
                    try
                    {
                        if (fieldInfo.FieldType.DeclaringType != null && newAIField != null && newAIField.FieldType.DeclaringType != null)
                        {
                            var isTransferManager = fieldInfo.FieldType.DeclaringType.Name == "TransferManager";
                            var isExtendedTransferManager = newAIField.FieldType.DeclaringType.Name == "ExtendedTransferManager";
                            if (isTransferManager && isExtendedTransferManager)
                            {
                                continue;
                            }
                        }
                        if (newAIField != null && newAIField.GetType().Equals(fieldInfo.GetType()))
                        {
                            var obj = fieldInfo.GetValue(src);
                            if(obj != null && obj.ToString() == "LuxuryProducts") continue;
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