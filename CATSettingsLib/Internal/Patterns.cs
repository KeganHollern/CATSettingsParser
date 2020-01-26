using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CATSettingsLib.Extensibles
{
    static class Patterns
    {
        public static byte[] TERMINATOR = { 0x22, 0x10, 0x00, 0x00, 0x00, 0xE0, 0x02, 0x00, 0x00, 0x00, 0x00 };
        public static byte[] TERMINATOR_INTERNAL = { 0x22, 0x00, 0x00, 0x00, 0x00, 0xE0, 0x02, 0x00, 0x00, 0x00, 0x00 };
        public static byte[] FOOTER = { 0x34, 0x0E, 0x5F, 0x5F, 0x4E, 0x55, 0x4C, 0x4C, 0x5F, 0x50, 0x4F, 0x49, 0x4E };
        public static int TERMINATOR_LENGTH { get { return TERMINATOR.Length; } }

        public static int FindPattern(this List<byte> bytes, byte[] pattern, int start = 0)
        {
            for (int i = start; i < (bytes.Count - pattern.Length); i++) //don't iterate over the max length of terminator
            {
                for (int j = 0; j < pattern.Length; j++)

                {
                    byte source_byte = bytes[i + j];
                    byte pattern_byte = pattern[j];

                    if (source_byte == pattern_byte)
                    {
                        if (j == pattern.Length - 1)
                        {
                            return i;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return -1;
        }
    }
}
