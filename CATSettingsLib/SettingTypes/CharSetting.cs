using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CATSettingsLib.SettingTypes
{
    public class CharSetting : Setting
    {
        public char Value {
            get {
                return Internal_Value;
            }
            set {
                SetValue(value);
            }
        }
        private char Internal_Value;
        public CharSetting(byte[] data) : base(data)
        {
            this.IsKnownDatatype = true;
            if (this.HasValue)
            {
                int start = this.InternalDataStartIndex + 20; //skip garbage data stored in the middle of the string setting

                this.Internal_Value = BitConverter.ToChar(data, start);
            }
            else
            {
                this.Internal_Value = (char)0x0;
            }
        }

        public override string GetValue()
        {
            return $"0x{BitConverter.ToString(new byte[] { Convert.ToByte(this.Value) })}";
        }
        public override void SetValue(object obj)
        {
            //---TODO: figure out how to convert default objects to valued objects
            if (!this.HasValue)
                throw new Exception("Cannot set the value of an object with no original value.");

            if (obj is char chr)
            {
                this.Internal_Value = chr;

                //write our new value to the binary (no size change pretty easy)
                byte new_value = (byte)chr;
                int start = this.InternalDataStartIndex + 20;
                this.Binary[start] = new_value;
            }
            else
            {
                throw new Exception("Cannot write a non-char to a char object");
            }
        }
    }
}
