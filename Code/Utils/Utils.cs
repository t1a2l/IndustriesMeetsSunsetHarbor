using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace IndustriesMeetsSunsetHarbor.Utils
{
    public static class Utils
    {
        private static readonly string _fileName = "IndustriesMeetsSunsetHarbor.log";
        private static readonly string _logPrefix = "IndustriesMeetsSunsetHarbor: ";

        public static string AssemblyPath => PluginInfo.modPath;

        private static PluginManager.PluginInfo PluginInfo
        {
            get
            {
                var pluginManager = PluginManager.instance;
                var plugins = pluginManager.GetPluginsInfo();

                foreach (var item in plugins)
                {
                    try
                    {
                        var instances = item.GetInstances<IUserMod>();
                        if (!(instances.FirstOrDefault() is IndustriesMeetsSunsetHarborMod))
                        {
                            continue;
                        }
                        return item;
                    }
                    catch
                    {

                    }
                }
                throw new Exception("Failed to find IndustriesMeetsSunsetHarbor assembly!");

            }
        }

        public static void ClearLogFile()
        {
            try
            {
                File.WriteAllText(Utils._fileName, string.Empty);
            }
            catch
            {
                Debug.LogWarning((object)("Error while clearing log file: " + Utils._fileName));
            }
        }

        public static void LogToTxt(object o)
        {
            try
            {
                File.AppendAllText(Utils._fileName, DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy ") + o + Environment.NewLine);
            }
            catch
            {
                Debug.LogWarning((object)("Error while writing to log file: " + Utils._fileName));
            }
        }

        public static void Log(object o)
        {
            Utils.Log(PluginManager.MessageType.Message, o);
        }

        public static void LogError(object o)
        {
            Utils.Log(PluginManager.MessageType.Error, o);
        }

        public static void LogWarning(object o)
        {
            Utils.Log(PluginManager.MessageType.Warning, o);
        }

        private static void Log(PluginManager.MessageType type, object o)
        {
            string str = Utils._logPrefix + o;
            switch (type)
            {
                case PluginManager.MessageType.Error:
                    Debug.LogError((object)str);
                    break;
                case PluginManager.MessageType.Warning:
                    Debug.LogWarning((object)str);
                    break;
                case PluginManager.MessageType.Message:
                    Debug.Log((object)str);
                    break;
            }
        }

        public static Q GetPrivate<Q>(object o, string fieldName)
        {
            FieldInfo[] fields = o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo fieldInfo1 = (FieldInfo)null;
            foreach (FieldInfo fieldInfo2 in fields)
            {
                if (fieldInfo2.Name == fieldName)
                {
                    fieldInfo1 = fieldInfo2;
                    break;
                }
            }
            return (Q)fieldInfo1.GetValue(o);
        }

        public static float ToSingle(string value)
        {
            float.TryParse(value, out float result);
            return result;
        }

        public static int ToInt32(string value)
        {
            int.TryParse(value, out int result);
            return result;
        }

        public static byte ToByte(string value)
        {
            byte.TryParse(value, out byte result);
            return result;
        }

        public static bool Truncate(UILabel label, string text, string suffix = "…")
        {
            bool flag = false;
            try
            {
                using (UIFontRenderer renderer = label.ObtainRenderer())
                {
                    float units = label.GetUIView().PixelsToUnits();
                    float[] characterWidths = renderer.GetCharacterWidths(text);
                    float num1 = 0.0f;
                    float num2 = (float)((double)label.width - (double)label.padding.horizontal - 2.0);
                    for (int index = 0; index < characterWidths.Length; ++index)
                    {
                        num1 += characterWidths[index] / units;
                        if ((double)num1 > (double)num2)
                        {
                            flag = true;
                            text = text.Substring(0, index - 3) + suffix;
                            break;
                        }
                    }
                }
                label.text = text;
            }
            catch
            {
                flag = false;
            }
            return flag;
        }

        public static string RemoveInvalidFileNameChars(string fileName)
        {
            return ((IEnumerable<char>)Path.GetInvalidFileNameChars()).Aggregate<char, string>(fileName, (Func<string, char, string>)((current, c) => current.Replace(c.ToString(), string.Empty)));
        }

        public static int RoundToNearest(float value, int nearest)
        {
            return Mathf.RoundToInt(value / (float)nearest) * nearest;
        }

        public static bool AreParametersEqual(ParameterInfo[] sourceParameters, ParameterInfo[] destinationParameters)
        {
            if (sourceParameters.Length != destinationParameters.Length)
                return false;
            for (int index = 0; index < sourceParameters.Length; ++index)
            {
                if (!sourceParameters[index].ParameterType.IsAssignableFrom(destinationParameters[index].ParameterType))
                    return false;
            }
            return true;
        }

        public static string GetModPath(string assemblyName, ulong workshopId)
        {
            foreach (PluginManager.PluginInfo pluginInfo in Singleton<PluginManager>.instance.GetPluginsInfo())
            {
                if (pluginInfo.name == assemblyName || (long)pluginInfo.publishedFileID.AsUInt64 == (long)workshopId)
                    return pluginInfo.modPath;
            }
            return (string)null;
        }

        public static bool IsModActive(ulong modId)
        {
            try
            {
                var plugins = PluginManager.instance.GetPluginsInfo();
                return plugins.Any(p => p != null && p.isEnabled && p.publishedFileID.AsUInt64 == modId);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to detect if mod {modId} is active");
                UnityEngine.Debug.LogException(e);
                return false;
            }
        }
    }
}