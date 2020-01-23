using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using CATSettings.Data;

namespace CATSettings.DataTypes
{
    public class CATSettingRepository : CATSetting
    {
        public List<byte> RawData { get; }
        public CATSettingRepository(byte[] data) : base(data)
        {
            RawData = new List<byte>();
            if (this.HasValue)
            {
                int start = this.NameEndIndex;

                int next_index = data.ToList().IndexOf(0x34, start + 18);
                int end_index = data.ToList().FindPattern(Patterns.FOOTER,next_index);
                RawData = data.ToList().GetRange(next_index, end_index - next_index); //extract internal raw data excluding footer
                //remove header data
                RawData.RemoveRange(0, 1); //remove 0x34
                int len = (int)RawData[0]; //grab the length of the repo datatype
                RawData.RemoveRange(0, 1);//remove len entry
                RawData.RemoveRange(0, len); //remove datatype name
                RawData.RemoveRange(0, 6); //remove some garbage
                RawData.RemoveRange(0, 1); //remove 0x34
                len = (int)RawData[0];//extract repository name length
                RawData.RemoveRange(0, 1);  //remove len entry
                RawData.RemoveRange(0, len);
                RawData.RemoveRange(0, 29);

                if(RawData.Count > 0)
                {
                    //--- if this contains fields, CATSettingAttribute needs stripped as well
                    len = (int)RawData[1];
                    string str = Encoding.ASCII.GetString(RawData.ToArray(), 2, len);
                    if(str == "CATSettingAttribute")
                    {
                        RawData.RemoveRange(0, 27);
                    }
                }
            }
        }
        public override string GetValue()
        {
            throw new Exception("CATSettingsRepositorys do not contain values. Instead use GetSettings() to extract their internal settings values");
        }
        public IEnumerable<CATSetting> GetSettings()
        {
            if (this.HasValue)
            {
                List<byte> bytes = RawData;
                List<int> end_locations = new List<int>();
                int index = 0;
                int result = 0;
                do
                {
                    result = bytes.FindPattern(Patterns.TERMINATOR_INTERNAL, index);
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
                    if (data.Length > 0)
                        yield return CATSetting.ParseEntry(data);
                }
            }
        }
    }
}
