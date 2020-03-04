using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CATSettingsLib.SettingTypes;
using System.IO;
using CATSettingsLib.Extensibles;
using CATSettingsLib.Internal;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace CATSettingsLib
{
    public class SettingsFile
    {
        public string FileName { get; }
        public string ShortName { get; }
        public Setting[] Settings { get; }
        private SettingsBinary FileData { get; }
        private SettingData[] SettingsData { get; }
        

        public SettingsFile(string File_Path)
        {
            byte[] raw = File.ReadAllBytes(File_Path);
            this.FileName = File_Path;
            this.ShortName = Path.GetFileName(File_Path);
            FileData = new SettingsBinary(raw);

            int binary_start = FileData.DataStartIndex;
            int binary_end = FileData.DataEndIndex;

            List<SettingData> extracted_settings = new List<SettingData>();

            List<byte> bytes = FileData.Binary.ToList().GetRange(binary_start, binary_end - binary_start);
            List<int> end_locations = new List<int>();
            int index = 0;
            int result = 0;
            do
            {
                result = bytes.FindPattern(Patterns.TERMINATOR, Patterns.TERMINATOR_MASK, index);
                index = result + Patterns.TERMINATOR_LENGTH;//get the index that every terminator ends at

                if (result != -1)
                    end_locations.Add(index);

            } while (result != -1);
            for (int i = 0; i <= end_locations.Count; i++)
            {
                int start = 0;
                if (i != 0)
                    start = end_locations[i - 1];
                int end = bytes.Count;
                if (i != end_locations.Count)
                    end = end_locations[i];

                if (start != (end - start))
                {
                    byte[] setting_data = bytes.GetRange(start, end - start).ToArray();
                    SettingData setting_object = new SettingData();
                    setting_object.StartIndex = start + binary_start;
                    setting_object.EndIndex = end + binary_start;
                    setting_object.Object = Setting.ParseEntry(setting_data);
                    extracted_settings.Add(setting_object);
                }
            }

            this.SettingsData = extracted_settings.ToArray();
            this.Settings = extracted_settings.Select((d) => Setting.ParseEntry(d.Object.Binary)).ToArray();//create deep copies of our objects so editing them does not effect our raw data
        }

        public void Save()
        {
            List<byte> New_Binary = FileData.Binary.ToList();
            for (int i = 0; i < Settings.Length; i++)
            {
                SettingData original_data = SettingsData[i];
                Setting new_data = Settings[i];
                if(new_data is SettingRepository sr)
                {
                    sr.Save(); //flush our repository settings into their binary
                }

                int start = original_data.StartIndex;
                int end = original_data.EndIndex;

                //---correction for data size change
                int index_delta = new_data.Binary.Length - (end - start);
                if(index_delta !=  0)
                {
                    for(int j = i+1;j < Settings.Length;j++)
                    {
                        SettingsData[j].StartIndex += index_delta;
                        SettingsData[j].EndIndex += index_delta;
                    }
                }

                New_Binary.RemoveRange(start, end - start); //strip old data out of our binary
                New_Binary.InsertRange(start,new_data.Binary);
                original_data.EndIndex = start + new_data.Binary.Length; //if size changed, change our end index
                original_data.Object = new_data;
            }
            if(New_Binary.Count != this.FileData.Binary.Length)
            {
                //--- I have no idea if this works
                int length_index = FileData.DataLengthFlag;
                byte value = FileData.Binary[length_index];
                int delta = value - (byte)this.FileData.Binary.Length; //difference between the file length & our flag value
                byte new_flag = (byte)(New_Binary.Count + delta);
                New_Binary[length_index] = new_flag;
            }
            this.FileData.Binary = New_Binary.ToArray();

            File.WriteAllBytes(this.FileName, this.FileData.Binary);
        }
        public Setting FindSetting(string field_name)
        {
            foreach (Setting s in Settings)
            {
                if (s.FieldName == field_name)
                    return s;

                if (s is SettingRepository sr)
                {
                    Setting result = sr.FindSetting(field_name);
                    if (result != null)
                        return result;
                }
            }
            return null;
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
            List<Setting> Settings = this.Settings.ToList();



            JObject result = new JObject(
                new JProperty("File", this.ShortName),
                new JProperty("Count", Settings.Count),
                new JProperty("Settings",
                    new JArray(
                        from setting in Settings where setting != null
                        select
                        new JObject(
                           new JProperty("Name", setting.FieldName),
                           new JProperty("Type", setting.DataType),
                           (setting is SettingRepository ? ParseRepository(setting) : new JProperty("Value", (setting.HasValue ? setting.GetValue() : "DEFAULT")))

                        )
                    )
                )
            );
            return result;
        }
        private JProperty ParseRepository(Setting repo_setting)
        {
            List<Setting> Settings = ((SettingRepository)repo_setting).Settings.ToList();

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
                               (setting is SettingRepository ? ParseRepository(setting) : new JProperty("Value", (setting.HasValue ? setting.GetValue() : "DEFAULT")))
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
