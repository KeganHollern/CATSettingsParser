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
        public string ToJSON()
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
            return result.ToString();
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
    }
}
