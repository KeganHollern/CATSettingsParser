using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CATSettingsLib.SettingTypes
{
    public class DoubleSetting : Setting
    {
        public double Value {
            get {
                return Internal_Value;
            }
            set {
                SetValue(value);
            }
        }
        private double Internal_Value;
        public DoubleSetting(byte[] data) : base(data)
        {
            this.IsKnownDatatype = true;
            if(this.HasValue)
            {
                int start = this.InternalDataStartIndex + 33; //skip garbage data stored in the middle of the string setting

                this.Internal_Value = BitConverter.ToDouble(data, start);
            }
            else
            {
                this.Internal_Value = 0;
            }
        }
        public override string GetValue()
        {
            return Value.ToString();
        }
        public override void SetValue(object obj)
        {
            if (!this.HasValue)
                throw new Exception("Cannot set the value of an object with no original value.");

            if (obj is double dbl)
            {
                this.Internal_Value = dbl;


                int start = this.InternalDataStartIndex + 33;
                List<byte> new_data = this.Binary.ToList();
                new_data.RemoveRange(start, 8);
                new_data.InsertRange(start, BitConverter.GetBytes(this.Internal_Value));
                this.Binary = new_data.ToArray();
            }
            else
            {
                throw new Exception("Cannot write a non-double to a double object");
            }
        }
    }
}
