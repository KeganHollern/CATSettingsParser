using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CATSettings.DataTypes
{
    public class StringSetting : CATSetting
    {
        public string Value { get; set; }
        public StringSetting(byte[] data) : base(data)
        {
            if (this.HasValue)
            {
                int start = this.NameEndIndex + 32; //skip garbage data stored in the middle of the string setting
                                               //--- note, this is the "value" datatype definition (should always be CATUnicodeString)
                int len = (int)data[start + 1];
                int index = start + 2;
                start = index + len + 6; //skip some more garbage
                if (data[start] == 0x34)
                {
                    len = (int)data[start + 1];
                    index = start + 2;
                    this.Value = Encoding.ASCII.GetString(data, index, len);
                } 
                else
                {
                    //NOTE: SettingsDialog.CATSettings (WorkbenchName value is 0x20  (string)) (no 0x34 header)
                    index = start + 1;
                    int index_of_ending = data.ToList().IndexOf(0x9F, index);
                    this.Value = Encoding.ASCII.GetString(data, index, index_of_ending - index);
                }
            }
        }
        public override string GetValue()
        {
            return Value;
        }
    }
}
