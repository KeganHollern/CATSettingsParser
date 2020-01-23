using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CATSettings.DataTypes;

namespace CATSettings.Data
{
    class SettingsParser
    {
        
        
        private byte[] RawData;
        public SettingsParser(string settings_path)
        {
            if(!settings_path.ToLower().EndsWith(".catsettings"))
            {
                throw new Exception("SettingsParser can only be used on files with the .CATSettings extension!");
            }
            byte[] source = File.ReadAllBytes(settings_path);
            source = strip_file_header(source);
            source = strip_file_footer(source);
            RawData = source;

        }

        public IEnumerable<CATSetting> GetSettings()
        {
            List<byte> bytes = RawData.ToList();
            List<int> end_locations = new List<int>();
            int index = 0;
            int result = 0;
            do
            {
                result = bytes.FindPattern(Patterns.TERMINATOR, index);
                index = result + Patterns.TERMINATOR_LENGTH;//get the index that every terminator ends at

                if (result != -1)
                    end_locations.Add(index);

            } while (result != -1);

            List<byte[]> entries = new List<byte[]>();
            for (int i = 0; i <= end_locations.Count; i++)
            {
                int start = 0;
                if (i != 0)
                    start = end_locations[i - 1];
                int end = bytes.Count;
                if (i != end_locations.Count)
                    end = end_locations[i];

                entries.Add(bytes.GetRange(start, end - start).ToArray());
            }

            foreach (byte[] data in entries)
            {
                if(data.Length > 0)
                    yield return CATSetting.ParseEntry(data);
            }
        }

        private byte[] strip_file_footer(byte[] bytes)
        {
            

            List<byte> values = bytes.ToList();
            int last_index = -1;
            int index = -1;
            do
            {
                index = values.FindPattern(Patterns.FOOTER, index + 1);
                if (index != -1)
                    last_index = index;
            } while (index != -1);
            values.RemoveRange(last_index, values.Count - last_index);

            return values.ToArray();
        }
        private byte[] strip_file_header(byte[] bytes)
        {
            List<byte> values = bytes.ToList();
            values.RemoveRange(0, (16 * 8) - 2); //remove file header

            values.RemoveRange(0, 28); //remove "CATSettingRepository" entry 

            if (values[0] == 0x34)
            {
                values.RemoveRange(0, 1); //remove 0x34
                int length = (int)values[0]; // get the length of our repository name
                values.RemoveRange(0, length + 1); //strip our repository name
            }
            else
            {
                //NOTE: check DLNames.CATSettings, the "DLNames" header entry does not have a 34 + length flag
                int end_name_index = values.IndexOf(0x22);
                values.RemoveRange(0, end_name_index);
            }
            values.RemoveRange(0, 5); //remove garbage
            values.RemoveRange(0, 18); //remove garbage
            byte some_unk_flag = values[0]; //this flag is unique between files (unsure the use)
            values.RemoveRange(0, 1); //remove flag
            values.RemoveRange(0, 3);
            byte some_unk_flag_2 = values[0]; //this flag is unique between files (unsure the use)
            values.RemoveRange(0, 1); //remove flag
            values.RemoveRange(0, 1); //remove sperator between header & content
            //values.RemoveRange(0, 56);//remove remainder of header "CATSettingAttribute"

            //determine if the header contains "CATSettingAttribute" field (if so strip it, if not, we have an empty settings file)
            
            int len = (int)values[1];
            string field_name = Encoding.ASCII.GetString(values.ToArray(), 2, len);
            if(field_name == "CATSettingAttribute")
            {
                values.RemoveRange(0, 2); //strip attribute header
                values.RemoveRange(0, len); //strip the attribute name
                values.RemoveRange(0, 6); //strip garbage after name
            }
            return values.ToArray();
        }


        
        
    }
}
