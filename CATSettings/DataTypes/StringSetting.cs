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

                int length_flag_1 = this.NameEndIndex + 25; //value is always length + 0x1C
                int length_flag_2 = this.NameEndIndex + 30; //value is always length + 0x1C
                
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
        public override void WriteToRaw(object new_data)
        {
            if(new_data is string new_string)
            {
                byte[] new_string_bytes = Encoding.ASCII.GetBytes(new_string);
                List<byte> bytes = this.Raw.ToList();
                bytes[this.NameEndIndex + 25] = (byte)((new_string_bytes.Length + 0x1C) >> 8); //insert flag #1
                bytes[this.NameEndIndex + 30] = (byte)((new_string_bytes.Length + 0x1C) >> 8); //insert flag #2
                int start = this.NameEndIndex + 32;
                int len = (int)bytes[start + 1];
                int index = start + 2;
                start = index + len + 6;
                if(bytes[start] == 0x34)
                {
                    len = (int)bytes[start + 1];
                    index = start + 2;
                    bytes[start + 1] = (byte)(new_string_bytes.Length >> 8);//insert new string length
                    bytes.RemoveRange(index, len); //remove old string
                    bytes.InsertRange(index, new_string_bytes); //insert new string

                    this.Raw = bytes.ToArray(); //update raw data with something we can write
                    this.Value = new_string; //update value with new value
                } 
                else
                {
                    throw new Exception("Writing invalid headers is not complete");
                }
            } else
            {
                throw new Exception("StringSetting can only write a 'string' object. The object passed to WriteToRaw is not a string object.");
            }
        }
    }
}
