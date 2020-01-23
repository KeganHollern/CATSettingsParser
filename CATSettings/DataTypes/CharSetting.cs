using System;
using System.Collections.Generic;
using System.Text;

namespace CATSettings.DataTypes
{
    public class CharSetting : CATSetting
    {
        public char Value { get; }

        public CharSetting(byte[] data) : base(data)
        {
            if (this.HasValue)
            {
                int start = this.NameEndIndex + 20; //skip garbage data stored in the middle of the string setting

                this.Value = BitConverter.ToChar(data, start);
            }
        }
        public override string GetValue()
        {
            return $"0x{BitConverter.ToString(new byte[] { Convert.ToByte(this.Value) })}";
        }
    }
}
