using System;
using System.Collections.Generic;
using System.Text;

namespace CATSettings.DataTypes
{
    public class CATSetting
    {
        protected int NameEndIndex = -1;
        public byte[] Raw { get; }
        public string DataType { get; }
        public string FieldName { get; }
        public bool HasValue { get; }

        public virtual string GetValue()
        {
            return "Unknown Value";
        }

        public CATSetting(byte[] data)
        {
            this.Raw = data;

            int start = 0;
            int len = (int)data[start + 1];
            int index = start + 2;
            this.DataType = Encoding.ASCII.GetString(data, index, len).Trim('\0');

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
                for(next = start + 1; next < Raw.Length; next++)
                {
                    if(data[next] == 0x34)
                    {
                        break;
                    }
                }
                if(next == Raw.Length) //some how this entire field is corrupt, assume false
                {
                    throw new Exception("Field data invalid");
                }
                //next = index + len
                index = start + 1;
                len = next - index;
                this.FieldName = Encoding.ASCII.GetString(data, index, len).Trim('\0');
            }
            this.NameEndIndex = index + len;

            int value_index = index + len + 18;
            byte value_flag = data[value_index];
            this.HasValue = value_flag == 0x1;
        }

        public static CATSetting ParseEntry(byte[] data)
        {
            int start = 0;
            int len = (int)data[start + 1];
            int index = start + 2;
            string datatype = Encoding.ASCII.GetString(data, index, len).Trim('\0');

            CATSetting entry;
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
                        entry = new CATSettingRepository(data);
                        break;
                    }
                case "long":
                //TODO: catia datatypes
                case "CATHeaderAttributes":
                default:
                    {
                        entry = new CATSetting(data);
                        break;
                    }
            }
            return entry;
        }
    }
}
