using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CATSettingsLib.SettingTypes
{
    //CATCmdWorkshop
    class DataOnlySetting : Setting
    {
        public byte[] Value {
            get {
                return Internal_Value;
            }
        }
        private byte[] Internal_Value;
        public DataOnlySetting(byte[] data) : base(data)
        {
            this.IsKnownDatatype = true;
            if (this.HasValue)
            {
                int start = this.InternalDataStartIndex; //skip garbage data stored in the middle of the string setting
                List<byte> temp = data.ToList();
                temp.RemoveRange(0, start);
                this.Internal_Value = temp.ToArray();
            }
            else
            {
                this.Internal_Value = new byte[] { };
            }
        }

        public override string GetValue()
        {
            return $"{BitConverter.ToString(this.Value)}";
        }
        public override void SetValue(object obj)
        {
            throw new Exception("Cannot set the value of a read-only setting.");
        }
    }
}
