using CATSettingsLib.SettingTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CATSettingsLib.Internal
{
    class SettingData
    {
        public Setting Object { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }

    }
}
