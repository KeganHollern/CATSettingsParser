using CATSettings.Data;
using CATSettings.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using YamlDotNet.Serialization;

namespace CATSettings
{
    public class SettingsFile
    {
        public string FullName { get; }
        public string ShortName { get; }
        private SettingsParser Parser;
        public SettingsFile(string path)
        {
            this.Parser = new SettingsParser(path);
            this.FullName = path;
            this.ShortName = Path.GetFileName(path);
        }

        public IEnumerable<CATSetting> GetSettings()
        {
            return this.Parser.GetSettings();
        }
        public string ToYAML()
        {
            //ya this somehow converts my json string into a valid object we can serialize??
            object as_object = ConvertJTokenToObject(JsonConvert.DeserializeObject<JToken>(ToJSON()));

            //serializer constructor
            var serializer = new YamlDotNet.Serialization.Serializer();

            //serialize our object to a string and return it
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, as_object);
                var yaml = writer.ToString();

                return yaml;
            }
        }
        




        public string ToJSON()
        {
            return GetJsonObject().ToString();
        }
        private JObject GetJsonObject()
        {
            List<CATSetting> Settings = this.GetSettings().ToList();



            JObject result = new JObject(
                new JProperty("File", this.ShortName),
                new JProperty("Count", Settings.Count),
                new JProperty("Settings",
                    new JArray(
                        from setting in Settings
                        select
                        new JObject(
                           new JProperty("Name", setting.FieldName),
                           new JProperty("Type", setting.DataType),
                           (setting is CATSettingRepository ? ParseRepository(setting) : new JProperty("Value", (setting.HasValue ? setting.GetValue() : "DEFAULT")))

                        )
                    )
                )
            );
            return result;
        }
        private JProperty ParseRepository(CATSetting repo_setting)
        {
            List<CATSetting> Settings = ((CATSettingRepository)repo_setting).GetSettings().ToList();

            JProperty result = new JProperty("Value", 
                new JObject(
                    new JProperty("Count", Settings.Count),
                    new JProperty("Settings", 
                        new JArray(
                            from setting in Settings
                            select
                            new JObject(
                                new JProperty("Name", setting.FieldName),
                                new JProperty("Type", setting.DataType),
                               (setting is CATSettingRepository ? ParseRepository(setting) : new JProperty("Value", (setting.HasValue ? setting.GetValue() : "DEFAULT")))
                            )
                        )
                    )
                )
            );
            return result;
        }
        // this is posted on here: https://stackoverflow.com/questions/36061049/how-to-convert-json-to-yaml-using-yamldotnet
        private object ConvertJTokenToObject(JToken token)
        {
            if (token is JValue)
                return ((JValue)token).Value;
            if (token is JArray)
                return token.AsEnumerable().Select(ConvertJTokenToObject).ToList();
            if (token is JObject)
                return token.AsEnumerable().Cast<JProperty>().ToDictionary(x => x.Name, x => ConvertJTokenToObject(x.Value));
            throw new InvalidOperationException("Unexpected token: " + token);
        }
    }
}
