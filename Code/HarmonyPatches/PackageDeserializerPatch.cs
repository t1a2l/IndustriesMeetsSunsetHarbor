using ColossalFramework;
using ColossalFramework.Packaging;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using static ColossalFramework.Packaging.PackageDeserializer;

namespace IndustriesMeetsSunsetHarbor.HarmonyPatches
{
    [HarmonyPatch(typeof(PackageDeserializer))]
    public static class PackageDeserializerPatch
    {
        [HarmonyPatch(typeof(PackageDeserializer), "DeserializeMonoBehaviour")]
        [HarmonyPrefix]
        public static bool DeserializeMonoBehaviour(Package package, MonoBehaviour behaviour, PackageReader reader, ref ResolveLegacyTypeHandler ___m_ResolveLegacyTypeHandler, ref UnknownTypeHandler ___m_UnknownTypeHandler)
        {
            int num = reader.ReadInt32();
            for (int i = 0; i < num; i++)
            {
                if (!DeserializeHeader(out var type, out var name, reader, behaviour, ref ___m_ResolveLegacyTypeHandler, ref ___m_UnknownTypeHandler))
                {
                    continue;
                }
                FieldInfo field = behaviour.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                Type expectedType = field?.FieldType;
                try
                {                 
                    if (type.IsArray)
                    {
                        int num2 = reader.ReadInt32();
                        Array array = Array.CreateInstance(type.GetElementType(), num2);
                        for (int j = 0; j < num2; j++)
                        {
                            array.SetValue(DeserializeSingleObject(package, type.GetElementType(), reader, expectedType), j);
                        }
                        field?.SetValue(behaviour, array);
                    }
                    else
                    {
                        object value = DeserializeSingleObject(package, type, reader, expectedType);
                        if(type.Name == "String" && expectedType.Name == "String[]")
                        {
                            Array array = Array.CreateInstance(expectedType.GetElementType(), 1);
                            array.SetValue(value, 0);
                            field?.SetValue(behaviour, array);
                        }
                        else
                        {
                            field?.SetValue(behaviour, value);
                        }
                    }                   
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error while deserialization Field '{field.Name}' of type '{behaviour.GetType().Name}'");
                    throw ex;
                }
            }
            return false;
        }

        public static bool DeserializeHeader(out Type type, out string name, PackageReader reader, MonoBehaviour behaviour, ref ResolveLegacyTypeHandler m_ResolveLegacyTypeHandler, ref UnknownTypeHandler m_UnknownTypeHandler)
        {
            type = null;
            name = null;
            if (reader.ReadBoolean())
            {
                return false;
            }
            string text = reader.ReadString();
            type = Type.GetType(text);
            name = reader.ReadString();
            if(behaviour.GetType().Name == "RestaurantAI")
            {
                return true;
            }
            if (type == null)
            {
                type = Type.GetType(ResolveLegacyType(text, ref m_ResolveLegacyTypeHandler));
                if (type == null)
                {
                    if (HandleUnknownType(text, reader, ref m_UnknownTypeHandler) < 0)
                    {
                        throw new InvalidDataException("Unknown type to deserialize " + text);
                    }
                    return false;
                }
            }
            return true;
        }

        internal static object DeserializeSingleObject(Package package, Type type, PackageReader reader, Type expectedType)
        {
            if (hasCustomDeserializer)
            {
                object obj = customDeserializer(package, type, reader);
                if (obj != null)
                {
                    return obj;
                }
            }
            if (typeof(ScriptableObject).IsAssignableFrom(type))
            {
                return reader.ReadAsset(package).Instantiate();
            }
            if (typeof(GameObject).IsAssignableFrom(type))
            {
                return reader.ReadAsset(package).Instantiate();
            }
            if (IsUnityType(type))
            {
                if (package.version < 3 && expectedType != null && expectedType == typeof(Package.Asset))
                {
                    return reader.ReadUnityType(expectedType);
                }
                return reader.ReadUnityType(type, package);
            }
            Debug.Log("Unsupported type for deserialization: [" + type.Name + "]");
            return null;
        }

        internal static string ResolveLegacyType(string type, ref ResolveLegacyTypeHandler m_ResolveLegacyTypeHandler)
        {
            if (m_ResolveLegacyTypeHandler != null)
            {
                string text = m_ResolveLegacyTypeHandler(type);
                CODebugBase<InternalLogChannel>.Warn(InternalLogChannel.Packer, "Unkown type detected. Attempting to resolve from '" + type + "' to '" + text + "'");
                return text;
            }
            return type;
        }

        private static int HandleUnknownType(string type, PackageReader reader, ref UnknownTypeHandler m_UnknownTypeHandler)
        {
            int num = HandleUnknownType(type, ref m_UnknownTypeHandler);
            if (num > 0)
            {
                reader.ReadBytes(num);
                return num;
            }
            return -1;
        }

        internal static int HandleUnknownType(string type, ref UnknownTypeHandler m_UnknownTypeHandler)
        {
            if (m_UnknownTypeHandler != null)
            {
                int num = m_UnknownTypeHandler(type);
                CODebugBase<InternalLogChannel>.Warn(InternalLogChannel.Packer, "Unexpected type '" + type + "' detected. No resolver handled this type. Skipping " + num + " bytes.");
                return num;
            }
            return -1;
        }

        internal static bool IsUnityType(Type type)
        {
            if (type.IsEnum || type == typeof(bool) || type == typeof(byte) || type == typeof(int) || type == typeof(uint) || type == typeof(ulong) || type == typeof(float) || type == typeof(string) || type == typeof(bool[]) || type == typeof(byte[]) || type == typeof(int[]) || type == typeof(float[]) || type == typeof(string[]) || type == typeof(DateTime) || type == typeof(Package.Asset) || type == typeof(GameObject) || type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4) || type == typeof(Color) || type == typeof(Matrix4x4) || type == typeof(Quaternion) || type == typeof(Vector2[]) || type == typeof(Vector3[]) || type == typeof(Vector4[]) || type == typeof(Color[]) || type == typeof(Matrix4x4[]) || type == typeof(Quaternion[]))
            {
                return true;
            }
            return false;
        }

    }

}

