using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CATSettingsLib.Extensibles;
using CATSettingsLib.Internal;

namespace CATSettingsLib.SettingTypes
{
    public class SettingRepository : Setting
    {
        private SettingData[] InternalData { get; }
        public Setting[] Settings { get; }

        public SettingRepository(byte[] data) : base(data)
        {
            this.IsKnownDatatype = true;
            if (this.HasValue)
            {
                int start = this.InternalDataStartIndex;
                int index = data.ToList().IndexOf(0x34, start + 18);
                int binary_end = data.ToList().FindPattern(Patterns.FOOTER, index);
                index++;// 0x34
                int len = (int)data[index];
                index++;
                index += len;
                index += 6;
                index++;
                len = (int)data[index];
                index++;
                index += len;
                index += 29;
                if (index < binary_end)
                {
                    len = (int)data[index + 1];
                    string str = Encoding.ASCII.GetString(data.ToArray(), index + 2, len);
                    if (str == "CATSettingAttribute")
                    {
                        index += 27;
                    }
                }
                else
                {
                    this.InternalData = new SettingData[] { }; //initilize our internal data as a null array
                    this.Settings = new Setting[] { };//no settings in here
                    return; //no internal
                }
                int binary_start = index;

                List<SettingData> extracted_settings = new List<SettingData>();
                List<byte> bytes = data.ToList().GetRange(binary_start, binary_end - binary_start);
                List<int> end_locations = new List<int>();
                index = 0;
                int result = 0;
                do
                {
                    result = bytes.FindPattern(Patterns.TERMINATOR_INTERNAL, index);
                    index = result + Patterns.TERMINATOR_LENGTH;//get the index that every terminator ends at

                    if (result != -1)
                        end_locations.Add(index);

                } while (result != -1);
                for (int i = 0; i <= end_locations.Count; i++)
                {
                    start = 0;
                    if (i != 0)
                        start = end_locations[i - 1];
                    int end = bytes.Count;
                    if (i != end_locations.Count)
                        end = end_locations[i];

                    byte[] setting_data = bytes.GetRange(start, end - start).ToArray();
                    SettingData setting_object = new SettingData();
                    setting_object.StartIndex = start + binary_start;
                    setting_object.EndIndex = end + binary_start;
                    setting_object.Object = Setting.ParseEntry(setting_data);
                    extracted_settings.Add(setting_object);
                }

                this.InternalData = extracted_settings.ToArray();
                this.Settings = extracted_settings.Select((d) => Setting.ParseEntry((d.Object == null ? new byte[] { } : d.Object.Binary))).ToArray();//create deep copies of our objects so editing them does not effect our raw data
            }
            else
            {
                //no value - empty settings datas
                Settings = new Setting[] { };
                InternalData = new SettingData[] { };
            }
        }
        public void Save()
        {
            List<byte> New_Binary = this.Binary.ToList();
            for (int i = 0; i < Settings.Length; i++)
            {
                SettingData original_data = InternalData[i];
                Setting new_data = Settings[i];

                int start = original_data.StartIndex;
                int end = original_data.EndIndex;

                //---correction for data size change
                int index_delta = new_data.Binary.Length - (end - start);
                if (index_delta != 0)
                {
                    for (int j = i + 1; j < Settings.Length; j++)
                    {
                        InternalData[j].StartIndex += index_delta;
                        InternalData[j].EndIndex += index_delta;
                    }
                }

                New_Binary.RemoveRange(start, end - start); //strip old data out of our binary
                New_Binary.InsertRange(start, new_data.Binary);
                original_data.EndIndex = start + new_data.Binary.Length; //if size changed, change our end index
                original_data.Object = new_data;
            }
            this.Binary = New_Binary.ToArray();
        }
        public override string GetValue()
        {
            return "";
        }
        public override void SetValue(object obj)
        {
            throw new Exception("You cannot set the value of a Repository. Perhaps you intended to set the value of one of the repositories children?");
        }
    }
}
