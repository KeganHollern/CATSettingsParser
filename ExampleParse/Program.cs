using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CATSettings;

namespace ExampleParse
{
    class Program
    {
        static void Main(string[] args)
        {
            string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folder = roaming + @"\DassaultSystemes\CATSettings";
            string file = folder + @"\Knowledge.CATSettings";
            if (args.Length > 0)
            {
                file = string.Join(" ", args);
            }
            SettingsFile CATSettingsFile = new SettingsFile(file);
            Console.WriteLine("============");
            Console.WriteLine(CATSettingsFile.ToJSON());
            Console.ReadKey();
        }
    }
}
