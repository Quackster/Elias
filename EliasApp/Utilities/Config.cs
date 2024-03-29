﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace EliasApp.Utilities
{
    class Config
    {
        private static string FFDEC_PATH = @"C:\Program Files (x86)\FFDec\ffdec.exe";
        private static string DIRECTOR_PATH = @"C:\Users\Alex\Documents\GitHub\Elias\EliasDirector\";
        private static string CCT_PATH = @"C:\Users\Alex\Documents\GitHub\Elias\CCTs";

        public static Config Instance = new Config();

        private Dictionary<string, string> _configValues;
        private string _configFileName = "EliasConfig.xml";

        /// <summary>
        /// Attempt to read configuration file
        /// </summary>
        public Config()
        {
            if (_configValues == null)
                _configValues = new Dictionary<string, string>();

            if (!File.Exists(_configFileName))
            {
                WriteConfig();
            }

            _configValues.Clear();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(_configFileName);

            _configValues["ffdec.path"] = xmlDoc.SelectSingleNode("//configuration/ffdec").InnerText;
            _configValues["elias.cct.converter.app"] = xmlDoc.SelectSingleNode("//configuration/converter_app").InnerText;
            _configValues["output.path"] = xmlDoc.SelectSingleNode("//configuration/output_path").InnerText;
            _configValues["furnidata.path"] = xmlDoc.SelectSingleNode("//configuration/furnidata_path").InnerText;
            _configValues["generate.small.modern.furni"] = xmlDoc.SelectSingleNode("//configuration/small_furni/generate_modern").InnerText;
            _configValues["generate.small.furni"] = xmlDoc.SelectSingleNode("//configuration/small_furni/generate").InnerText;
            _configValues["save.as.cst"] = xmlDoc.SelectSingleNode("//configuration/options/save_as_cst").InnerText;
            _configValues["emergency.fix"] = xmlDoc.SelectSingleNode("//configuration/options/emergency_fix").InnerText;
            _configValues["close.when.finished"] = xmlDoc.SelectSingleNode("//configuration/options/close_when_finished").InnerText;
        }

        /// <summary>
        /// Attempts to write configuration file
        /// </summary>
        private void WriteConfig()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("   ");
            settings.OmitXmlDeclaration = true;

            XmlWriter xmlWriter = XmlWriter.Create(_configFileName, settings);

            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("configuration");

            xmlWriter.WriteStartElement("ffdec");
            xmlWriter.WriteString(FFDEC_PATH);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("converter_app");
            xmlWriter.WriteString(DIRECTOR_PATH + "elias_app.exe");
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("output_path");
            xmlWriter.WriteString(CCT_PATH);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("furnidata_path");
            xmlWriter.WriteString("furnidata.txt");
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("small_furni");

            xmlWriter.WriteStartElement("generate");
            xmlWriter.WriteString("true");
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("generate_modern");
            xmlWriter.WriteString("false");
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("options");

            xmlWriter.WriteStartElement("save_as_cst");
            xmlWriter.WriteString("true");
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("emergency_fix");
            xmlWriter.WriteString("false");
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("close_when_finished");
            xmlWriter.WriteString("false");
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
        }

        /// <summary>
        /// Get string by key
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns>the string</returns>
        public string GetString(string key)
        {
            string value;
            _configValues.TryGetValue(key, out value);
            return value;
        }

        /// <summary>
        /// Get integer by key
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns>the integer</returns>
        public int GetInt(string key)
        {
            int number = 0;
            int.TryParse(GetString(key), out number);
            return number;
        }

        /// <summary>
        /// Get boolean by key
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns>the boolean</returns>
        internal bool GetBoolean(string key)
        {
            return GetString(key).ToLower() == "true";
        }
    }
}
