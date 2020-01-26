using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CATSettingsLib.SettingTypes
{
    public class StringSetting : Setting
    {
        public string Value { 
            get {
                return Internal_Value;
            }
            set {
                SetValue(value);
            }
        }
        private string Internal_Value;
        public StringSetting(byte[] data) : base(data)
        {
            this.IsKnownDatatype = true;
            if (this.HasValue)
            {
                int start = this.InternalDataStartIndex + 32; //skip garbage data stored in the middle of the string setting

                int length_flag_1 = this.InternalDataStartIndex + 25; //value is always length + 0x1C
                int length_flag_2 = this.InternalDataStartIndex + 30; //value is always length + 0x1C

                //--- note, this is the "value" datatype definition (should always be CATUnicodeString)
                int len = (int)data[start + 1];
                int index = start + 2;
                start = index + len + 6; //skip some more garbage
                if (data[start] == 0x34)
                {
                    len = (int)data[start + 1];
                    index = start + 2;
                    this.Internal_Value = Encoding.ASCII.GetString(data, index, len);
                }
                else
                {
                    //NOTE: SettingsDialog.CATSettings (WorkbenchName value is 0x20  (string)) (no 0x34 header)
                    index = start + 1;
                    int index_of_ending = data.ToList().IndexOf(0x9F, index);
                    this.Internal_Value = Encoding.ASCII.GetString(data, index, index_of_ending - index);
                }
            }
            else
            {
                this.Internal_Value = "";
            }
        }

        public override string GetValue()
        {
            return Value;
        }
        public override void SetValue(object obj)
        {
            if (!this.HasValue)
                throw new Exception("Cannot set the value of an object with no original value.");

            if (obj is string str)
            {
                this.Internal_Value = str;

                byte[] new_string_bytes = Encoding.ASCII.GetBytes(this.Internal_Value);
                List<byte> bytes = this.Binary.ToList();

                byte length_flag = Convert.ToByte(new_string_bytes.Length + 0x1C); //insert flag #1

                bytes[this.InternalDataStartIndex + 25] = length_flag;
                bytes[this.InternalDataStartIndex + 30] = length_flag;
                int start = this.InternalDataStartIndex + 32;
                int len = (int)bytes[start + 1];
                int index = start + 2;
                start = index + len + 6;
                if (bytes[start] == 0x34)
                {
                    len = (int)bytes[start + 1];
                    index = start + 2;
                    byte new_length_value = Convert.ToByte(new_string_bytes.Length);
                    bytes[start + 1] = new_length_value;//insert new string length
                    bytes.RemoveRange(index, len); //remove old string
                    bytes.InsertRange(index, new_string_bytes); //insert new string

                    this.Binary = bytes.ToArray(); //update raw data with something we can write
                }
                else
                {
                    throw new Exception("Writing invalid headers is not complete");
                }
            }
            else
            {
                throw new Exception("Cannot write a non-string to a string object");
            }
        }
    }
}
