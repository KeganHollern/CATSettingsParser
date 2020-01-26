using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CATSettingsLib.SettingTypes
{
    class SettingRepository : Setting
    {
        public SettingRepository(byte[] data) : base(data)
        {
            this.IsKnownDatatype = false; //--- this is TODO as writing to repositories is kinda tricky
        }
    }
}
