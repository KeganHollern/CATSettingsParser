using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CATSettingsLib.SettingTypes
{
    public class FloatSetting : Setting
    {
        public float Value {
            get {
                return Internal_Value;
            }
            set {
                SetValue(value);
            }
        }
        private float Internal_Value;
        public FloatSetting(byte[] data) : base(data)
        {
            this.IsKnownDatatype = true;
            if (this.HasValue)
            {
                int start = this.InternalDataStartIndex + 32; //skip garbage data stored in the middle of the string setting

                this.Internal_Value = BitConverter.ToSingle(data, start);
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

            if (obj is float flt)
            {
                this.Internal_Value = flt;


                int start = this.InternalDataStartIndex + 32;
                List<byte> new_data = this.Binary.ToList();
                new_data.RemoveRange(start, 4);
                new_data.InsertRange(start, BitConverter.GetBytes(this.Internal_Value));
                this.Binary = new_data.ToArray();
            }
            else
            {
                throw new Exception("Cannot write a non-float to a float object");
            }
        }
    }
}
