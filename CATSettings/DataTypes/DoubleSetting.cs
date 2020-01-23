using System;
using System.Collections.Generic;
using System.Text;

namespace CATSettings.DataTypes
{
    public class DoubleSetting : CATSetting
    {
        public double Value { get; }

        public DoubleSetting(byte[] data) : base(data)
        {
            if (this.HasValue)
            {

                int start = this.NameEndIndex + 33; //skip garbage data stored in the middle of the string setting

                this.Value = BitConverter.ToDouble(data, start);
            }
        }
        public override string GetValue()
        {
            return Value.ToString();
        }
    }
}
