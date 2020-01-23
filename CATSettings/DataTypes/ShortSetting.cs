using System;
using System.Collections.Generic;
using System.Text;

namespace CATSettings.DataTypes
{
    public class ShortSetting : CATSetting
    {
        public short Value { get; }

        public ShortSetting(byte[] data) : base(data)
        {
            if (this.HasValue)
            {
                int start = this.NameEndIndex + 32; //skip garbage data stored in the middle of the string setting

                this.Value = BitConverter.ToInt16(data, start);
            }
        }
        public override string GetValue()
        {
            return Value.ToString();
        }
    }
}
