using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CATSettingsLib.Extensibles;

namespace CATSettingsLib.Internal
{
    public class SettingsBinary
    {
        public byte[] Binary { get; set; }

        public SettingsBinary() { Binary = new byte[] { }; }
        public SettingsBinary(byte[] data) { Binary = data; }


        public int DataLengthFlag {
            get {
                return 16 * 3 + 5;
            }
        }
        public int UnknownFlag_1 {
            get {
                List<byte> values = Binary.ToList();
                int index = DataLengthFlag + 11 + (16 * 4) - 2 + 28;
                if(values[index] == 0x34)
                {
                    index++;
                    int length = (int)values[index];
                    index += length + 1;
                }
                else
                {
                    index = values.IndexOf(0x22, index);
                }
                index += 5 + 18;
                return index;
            }
        }
        public int UnknownFlag_2 {
            get {
                int index = UnknownFlag_1;
                index += 4;
                return index;
            }
        }
        public int DataStartIndex {
            get {
                List<byte> values = Binary.ToList();
                int index = UnknownFlag_2;
                index += 2;
                int len = values[index + 1];
                string field_name = Encoding.ASCII.GetString(values.ToArray(), index + 2, len);
                if (field_name == "CATSettingAttribute")
                {
                    index += 2;
                    index += len;
                    index += 6;
                }
                return index;
            }
        }
        public int DataEndIndex {
            get {
                List<byte> values = Binary.ToList();
                int last_index = -1;
                int index = -1;
                do
                {
                    index = values.FindPattern(Patterns.FOOTER, index + 1);
                    if (index != -1)
                        last_index = index;
                } while (index != -1);
                return last_index;
            }
        }
    }
}
