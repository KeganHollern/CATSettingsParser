using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CATSettingsLib.SettingTypes
{
    public class Setting
    {
        protected int InternalDataStartIndex;
        public byte[] Binary { get; protected set; }
        public string DataType { get; }
        public string FieldName { get; }

        public bool HasValue { get; }

        public bool IsKnownDatatype { get; protected set; }

        public Setting(byte[] data)
        {
            this.IsKnownDatatype = false;
            this.Binary = data;

            if (data.Length == 0)
            {
                HasValue = false;
                DataType = "NULL";
                FieldName = "NULL";
                return;
            }

            //--- extract datatype string
            int start = 0;
            int len = (int)data[start + 1];
            int index = start + 2;
            this.DataType = Encoding.ASCII.GetString(data, index, len).Trim('\0');

            //--- extract the field name (some CATSettings objects are invalid in their object structure so this gets a bit complex)
            start = index + len;
            if (data[start] == 0x34)
            {
                len = (int)data[start + 1];
                index = start + 2;
                this.FieldName = Encoding.ASCII.GetString(data, index, len).Trim('\0');
            }
            else
            {
                //NOTE: Assembly.CATSettings / UdpAuto (last entry) does not contain a valid fieldname header
                //find next 0x34 (header)
                int next;
                for (next = start + 1; next < data.Length; next++)
                {
                    if (data[next] == 0x34)
                    {
                        break;
                    }
                }
                if (next == data.Length) //some how this entire field is corrupt, assume false
                {
                    throw new Exception("Field data invalid");
                }
                index = start + 1;
                len = next - index;
                this.FieldName = Encoding.ASCII.GetString(data, index, len).Trim('\0');
            }

            //--- save the index where we start our internal data structure
            this.InternalDataStartIndex = index + len;

            //--- check the "has value" byte
            int value_index = index + len + 18;
            byte value_flag = data[value_index];
            this.HasValue = value_flag == 0x1;
        }

        public virtual string GetValue()
        {
            throw new Exception("Cannot get the value of an unknown datatype");
        }
        public virtual void SetValue(object obj)
        {
            throw new Exception("Cannot set the value of an unknown datatype");
        }



        public static Setting ParseEntry(byte[] data)
        {
            if (data.Length == 0)
                return new Setting(data);

            int start = 0;
            int len = (int)data[start + 1];
            int index = start + 2;
            string datatype = Encoding.ASCII.GetString(data, index, len).Trim('\0');

            Setting entry;
            switch (datatype)
            {

                case "CATString":
                case "CATUnicodeString":
                    {
                        entry = new StringSetting(data);
                        break;
                    }
                case "int":
                    {
                        entry = new IntSetting(data);
                        break;
                    }
                case "double":
                    {
                        entry = new DoubleSetting(data);
                        break;
                    }
                case "char":
                    {
                        entry = new CharSetting(data);
                        break;
                    }
                case "unsigned int":
                    {
                        entry = new UIntSetting(data);
                        break;
                    }
                case "float":
                    {
                        entry = new FloatSetting(data);
                        break;
                    }
                case "short":
                    {
                        entry = new ShortSetting(data);
                        break;
                    }
                case "CATSettingRepository":
                    {
                        entry = new SettingRepository(data);
                        break;
                    }
                    //--- these datatypes we do not have the information to handle
                case "long":
                case "CATHeaderAttributes":
                default:
                    {
                        entry = new Setting(data);
                        break;
                    }
            }
            return entry;
        }
    }
}
