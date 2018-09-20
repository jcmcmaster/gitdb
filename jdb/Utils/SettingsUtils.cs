using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace jdb.Utils
{
    /// <summary>
    /// Utilities for creating, accessing, and updating program settings.
    /// </summary>
    public static class SettingsUtils
    {
        /// <summary>
        /// URL of the config file.
        /// </summary>
        public static string SettingsLocation = AppDomain.CurrentDomain.BaseDirectory + "/config.json";

        /// <summary>
        /// Checks if settings file exists, creating it if not, and loads them into memory.
        /// </summary>
        /// <returns></returns>
        public static dynamic InitSettings()
        {
            if (!File.Exists(SettingsLocation))
            {
                InitSettingsFile();
            }

            return GetSettings();
        }

        /// <summary>
        /// Accepts a dynamic object and writes it as JSON to the settings file.
        /// </summary>
        /// <param name="jsonObj"></param>
        public static void WriteSettings(dynamic jsonObj)
        {
            string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
            File.WriteAllText(SettingsLocation, output);
        }

        /// <summary>
        /// Overwrites the settings file with an empty JSON object.
        /// </summary>
        private static void InitSettingsFile()
        {
            File.WriteAllText(SettingsLocation, "{}");
        }

        /// <summary>
        /// Retrieves a dynamic representation of the settings file.
        /// </summary>
        /// <returns></returns>
        private static dynamic GetSettings()
        {
            string json = File.ReadAllText(SettingsLocation);
            return JsonConvert.DeserializeObject(json);
        }
    }
}
