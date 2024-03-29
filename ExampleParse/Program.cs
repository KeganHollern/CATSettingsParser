﻿using System;
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
            Error_Check(args);
            //Read_In_Session(args);  //--- passed
            //Write_In_Session(args); //--- failed, catia cache's our settings (note that the CATSettings file is written to, but catia does not use the new value)
            //Write_Test(args); //--- passed (double, string)
            //DumpYaml_Test(args); //--- passed
            Console.ReadKey();
        }
        static void Error_Check(string[] args)
        {
            Console.WriteLine("=== Scanning for errors ===");
            int errors = 0;
            string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folder = roaming + @"\DassaultSystemes\CATSettings";
            foreach(string file in Directory.EnumerateFiles(folder,"*.catsettings"))
            {
                string simple_name = Path.GetFileName(file);
                try
                {
                    SettingsFile CATSettingsFile = new SettingsFile(file);
                    string YAML = CATSettingsFile.ToYAML();
                    Console.WriteLine($"No Error in '{simple_name}'.");
                } catch
                {
                    Console.WriteLine($"ERROR READING '{simple_name}'.");
                    errors++;
                }
            }
            Console.WriteLine($"==== Scan Complete {errors} error{ (errors > 1 || errors == 0 ? "s" : "")} found ====");
        }
        static void Read_In_Session(string[] args)
        {
            string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string file = roaming + @"\DassaultSystemes\CATSettings\Knowledge.CATSettings";
            SettingsFile CATSettingsFile = new SettingsFile(file);

            StringSetting setting = (StringSetting)CATSettingsFile.FindSetting("CATKnowledgeBuildPath");
            Console.WriteLine(setting.Value);
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
