﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace gitdb.Utils
{
    public static class SettingsUtils
    {
        public static string SettingsLocation = AppDomain.CurrentDomain.BaseDirectory + "/config.json";

        public static dynamic InitSettings()
        {
            if (!File.Exists(SettingsLocation))
            {
                InitSettingsFile();
            }

            return GetSettings();
        }

        public static dynamic GetSettings()
        {
            string json = File.ReadAllText(SettingsLocation);
            return JsonConvert.DeserializeObject(json);
        }

        public static bool SettingExists(dynamic settings, string name)
        {
            if (settings is ExpandoObject)
                return ((IDictionary<string, object>)settings).ContainsKey(name);

            return settings.GetType().GetProperty(name) != null;
        }

        public static void InitSettingsFile()
        {
            File.WriteAllText(SettingsLocation, "{}");
        }

        public static void WriteSettings(dynamic jsonObj)
        {
            string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
            File.WriteAllText(SettingsLocation, output);
        }
    }
}