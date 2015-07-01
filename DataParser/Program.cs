// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Visteon">
//   
// </copyright>
// <summary>
//   The program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace DataParser
{
    using System;
    using System.IO;

    using DataParser.EEP;
    using DataParser.FE;

    /// <summary>
    ///     The program.
    /// </summary>
    internal class Program
    {
        /// <summary>
        ///     The configuration file name.
        /// </summary>
        private const string ConfigurationFileName = "DataParser.xml";

        /// <summary>
        ///     The main.
        /// </summary>
        private static void Main()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("-------------------------------------------------------");
                Console.WriteLine(" EEPROM and Front End data generator");
                Console.WriteLine("-------------------------------------------------------");
                Console.WriteLine();

                var reader = new XmlReaderClass(ConfigurationFileName);
                Console.WriteLine(">>>>> Configuration file " + ConfigurationFileName + " succesfully read.");
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(">>>>> Generate Eeprom Files (y/n)?");
                var key = Console.ReadKey();

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;

                if (key.KeyChar == 'y' || key.KeyChar == 'Y')
                {
                    Console.WriteLine();
                    Console.WriteLine("-------------------------------------------------------");
                    Console.WriteLine(" Generating EEPROM data files");
                    Console.WriteLine("-------------------------------------------------------");
                    Console.WriteLine();

                    var def = new EepDefaultData(
                        reader.Defaults, 
                        reader.EepCfgManDefaultsFile, 
                        reader.RteDefaultsFile, 
                        reader.CodingDefaultsFile, 
                        reader.DltDefaultsFile);
                    Console.WriteLine(">>>>> Eeprom data parsed from input files.");

                    var eep = new EepEeprom(reader.EepData);
                    Console.WriteLine(">>>>> Eeprom data succesfully generated.");

                    var gen = new EepDiagScriptGenerator(reader.DiagScriptFile, eep.EepImage);
                    Console.WriteLine(
                        ">>>>> Eeprom diangoser script file " + reader.DiagScriptFile + " succesfully generated.");

                    var gen1 = new EepBinFileGenerator(reader.BinFile, eep.EepImage);
                    Console.WriteLine(">>>>> Eeprom binary file " + reader.BinFile + " succesfully generated.");

                    Console.WriteLine();
                    Console.WriteLine("-------------------------------------------------------");
                    Console.WriteLine(" EEPROM data files generation complete");
                    Console.WriteLine("-------------------------------------------------------");
                    Console.WriteLine();
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(">>>>> Generate Front End Files (y/n)?");
                key = Console.ReadKey();
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;

                if (key.KeyChar == 'y' || key.KeyChar == 'Y')
                {
                    Console.WriteLine();
                    Console.WriteLine("-------------------------------------------------------");
                    Console.WriteLine(" Generating Front End data files");
                    Console.WriteLine("-------------------------------------------------------");
                    Console.WriteLine();

                    // Remove output folder
                    if (Directory.Exists(reader.FeOutputFolder))
                    {
                        RemoveReadOnly(new DirectoryInfo(reader.FeOutputFolder));
                        Directory.Delete(reader.FeOutputFolder, true);
                    }

                    Console.WriteLine(">>>>> Remove old generated files.");

                    // Create new output folder
                    Directory.CreateDirectory(reader.FeOutputFolder);
                    foreach (var item in reader.FeVariants)
                    {
                        Directory.CreateDirectory(reader.FeOutputFolder + "\\" + item);
                    }

                    Console.WriteLine(">>>>> Creating folders.");

                    foreach (var item in reader.FeVariants)
                    {
                        var a = new IndigoVariant(
                            reader.FeInputSequenceFolder, 
                            reader.FeInputParameterFolder + "\\" + item, 
                            reader.FeParameterVersion);
                        a.CopyFiles(reader.FeOutputFolder + "\\" + item);
                        a.MergeFiles(reader.FeOutputFolder + "\\" + item, item);
                        Console.WriteLine(">>>>> Varinat " + item + " generated.");
                    }

                    Console.WriteLine();
                    Console.WriteLine("-------------------------------------------------------");
                    Console.WriteLine(" Front End data files generation complete");
                    Console.WriteLine("-------------------------------------------------------");
                }

                Console.WriteLine();
                Console.WriteLine("-------------------------------------------------------");
                Console.WriteLine(">>>>> Done!");
                Console.WriteLine("-------------------------------------------------------");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(">>>>> ERROR: " + e.Message);
                Console.ReadLine();
            }
        }

        /// <summary>
        /// The remove read only.
        /// </summary>
        /// <param Name="directory">
        /// The directory.
        /// </param>
        /// <param name="directory">
        /// The directory.
        /// </param>
        private static void RemoveReadOnly(DirectoryInfo directory)
        {
            foreach (var fi in directory.GetFiles())
            {
                fi.IsReadOnly = false;
            }

            foreach (var subdir in directory.GetDirectories())
            {
                RemoveReadOnly(subdir);
            }
        }
    }
}