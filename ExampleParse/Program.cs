using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CATSettingsLib;
using YamlDotNet;
using Newtonsoft.Json;
using CATSettingsLib.SettingTypes;
using YamlDotNet.Serialization;

namespace ExampleParse
{
    class Program
    {
        static void Main(string[] args)
        {



            //Write_In_Session(args);
            //Write_Test(args); //--- passed (double, string)
            DumpYaml_Test(args); //--- passed
            Console.ReadKey();
        }
        static void Write_In_Session(string[] args)
        {
            string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string file = roaming + @"\DassaultSystemes\CATSettings\Knowledge.CATSettings";
            SettingsFile CATSettingsFile = new SettingsFile(file);

            StringSetting setting = (StringSetting)CATSettingsFile.FindSetting("CATKnowledgeBuildPath");
            setting.Value = @"T:\TEST";
            CATSettingsFile.Save();
        }
        static void Write_Test(string[] args)
        {
            string file = string.Join(" ", args);
            SettingsFile CATSettingsFile = new SettingsFile(file);
            Console.WriteLine("=== ORIGINAL YAML ===");
            Console.WriteLine(CATSettingsFile.ToYAML());

            //write our settings values
            foreach(Setting setting in CATSettingsFile.Settings)
            {
                //--- CkeTolerance tests
                if(setting.FieldName == "minAngleTolerance")
                {
                    DoubleSetting dbl_set = (DoubleSetting)setting;
                    dbl_set.Value = 5;
                }
                if (setting.FieldName == "maxAngleTolerance")
                {
                    DoubleSetting dbl_set = (DoubleSetting)setting;
                    dbl_set.Value = 30;
                }
                if (setting.FieldName == "minLengthTolerance")
                {
                    DoubleSetting dbl_set = (DoubleSetting)setting;
                    dbl_set.Value = 2;
                }
                if (setting.FieldName == "maxLengthTolerance")
                {
                    DoubleSetting dbl_set = (DoubleSetting)setting;
                    dbl_set.Value = 6;
                }

                //---Knowledge tests
                if (setting.FieldName == "CATKnowledgeBuildPath")
                {
                    StringSetting str_set = (StringSetting)setting;
                    str_set.Value = @"T:\Catalog\3 - Knowledgeware\knowledgeTypesCustom";
                }
                
            }
            Console.WriteLine("=== NEW YAML ===");
            Console.WriteLine(CATSettingsFile.ToYAML());

            CATSettingsFile.Save();
        }
        static void DumpYaml_Test(string[] args)
        {
            if (args.Length > 0)
            {
                string file = string.Join(" ", args);
                SettingsFile CATSettingsFile = new SettingsFile(file);
                Console.WriteLine(CATSettingsFile.ToYAML());
                
            }
            else
            {
                string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string folder = roaming + @"\DassaultSystemes\CATSettings";

                foreach (string file in Directory.EnumerateFiles(folder, "*.CATSettings"))
                {
                    SettingsFile CATSettingsFile = new SettingsFile(file);
                    Console.WriteLine(CATSettingsFile.ToYAML());
                }

            }
        }
    }
}
