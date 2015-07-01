// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EeProm.cs" company="Visteon">
//   
// </copyright>
// <summary>
//   The ee prom.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace DataParser.EEP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DataParser.Lib;

    /// <summary>
    ///     The ee prom.
    /// </summary>
    internal class EepEeprom
    {
        /// <summary>
        ///     The eep default data byte.
        /// </summary>
        private const byte EepDefaultDataByte = 0x00;

        /// <summary>
        ///     The eep ep wrap byte.
        /// </summary>
        private const byte EepEpWrapByte = 0x5A;

        /// <summary>
        ///     The eep gap byte.
        /// </summary>
        private const byte EepGapByte = 0xFF;

        /// <summary>
        ///     The eep size.
        /// </summary>
        private static int eepSize = 16384;

        /// <summary>
        ///     The eep data.
        /// </summary>
        private readonly List<EepEaDataStructure> eepData;

        /// <summary>
        ///     The temp.
        /// </summary>
        private List<EepEaDataStructure> temp = new List<EepEaDataStructure>();

        /// <summary>
        /// Initializes a new instance of the <see cref="EepEeprom"/> class.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        public EepEeprom(List<EepEaDataStructure> data)
        {
            this.EepImage = new List<byte>();
            this.eepData = data;

            this.ValidateAndAddDefaultData();

            var ddd = new List<byte>();
            var blocks = string.Empty;
            foreach (var a in this.eepData.OrderBy(x => x.BlockId))
            {
                for (var i = 0; i < a.DataSets; i++)
                {
                    ddd.AddRange(AddWrapBytes(a.DefaultData));
                    blocks += a.Name + ' ';
                }
            }

            var ddd1 = new List<string>();
            foreach (var a in ddd)
            {
                ddd1.Add(a.ToString("X2"));
            }

            var aaa = string.Join("|0x", ddd1);

            // var counter = 0;
            // string line, output;

            // output = string.Empty;

            // Read the file and display it line by line.
            // var file = new StreamReader("export.txt");
            // while ((line = file.ReadLine()) != null)
            // {
            // if (line.StartsWith("Received: 0x63, "))
            // {
            // output = output + line.Substring(17) + ",";
            // }

            // // Console.WriteLine(line);
            // counter++;
            // }

            // output = output.Replace("0x", "")
            // file.Close();
            this.EepImage.AddRange(Enumerable.Repeat((byte)0xFF, 64).ToList());
            foreach (var el in this.eepData.OrderBy(x => x.BlockId))
            {
                for (var i = 0; i < el.DataSets; i++)
                {
                    this.EepImage.AddRange(AddWrapBytes(el.DefaultData));
                }
            }

            this.EepImage.AddRange(Enumerable.Repeat((byte)0xFF, eepSize - this.EepImage.Count).ToList());
        }

        /// <summary>
        ///     Gets or sets the eep image.
        /// </summary>
        public List<byte> EepImage { get; set; }

        /// <summary>
        /// The add wrap bytes.
        /// </summary>
        /// <param name="list">
        /// The list.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        private static IEnumerable<byte> AddWrapBytes(IEnumerable<byte> list)
        {
            var temp = new List<byte> { EepEpWrapByte };
            temp.AddRange(list);
            temp.Add(EepEpWrapByte);
            return temp;
        }

        /// <summary>
        ///     The validate and add default data.
        /// </summary>
        /// <exception cref="Exception">
        /// </exception>
        private void ValidateAndAddDefaultData()
        {
            foreach (var eepElement in this.eepData.OrderBy(x => x.BlockId))
            {
                if (eepElement.NvmRef == null)
                {
                    // No default data available, fill in with 0x00
                    eepElement.DefaultData =
                        Enumerable.Repeat(EepGapByte, eepElement.BlockSize / eepElement.DataSets).ToList();
                }
                else if ((eepElement.NvmRef.InitCallback == "true" && eepElement.NvmRef.InitCallbackValue == "NULL_PTR")
                         || (eepElement.NvmRef.InitCallback == "false" && eepElement.NvmRef.RomBlock == "NULL_PTR"))
                {
                    if (eepElement.NvmRef.CrcUsage == "false")
                    {
                        for (var i = 0; i < eepElement.DataSets; i++)
                        {
                            // No default data available, fill in with 0x00
                            eepElement.DefaultData =
                                Enumerable.Repeat(EepDefaultDataByte, eepElement.BlockSize / eepElement.DataSets)
                                    .ToList();
                        }
                    }
                    else
                    {
                        var dataToWrite = Enumerable.Repeat(EepDefaultDataByte, eepElement.BlockSize - 2).ToArray();
                        var crc = Crc16.CalculateCrc16(dataToWrite);
                        var dataWithCrc = new List<byte>();
                        dataWithCrc.AddRange(dataToWrite);
                        dataWithCrc.AddRange(crc);

                        eepElement.DefaultData = dataWithCrc;
                    }
                }
                else if (eepElement.NvmRef.CrcUsage == "false")
                {
                    // No Crc available for the block
                    var block = eepElement.NvmRef.InitCallback == "true"
                                    ? EepDefaultData.Defaults.SingleOrDefault(
                                        x => x.Name == eepElement.NvmRef.InitCallbackValue)
                                    : EepDefaultData.Defaults.SingleOrDefault(
                                        x => x.Name == eepElement.NvmRef.RomBlock);

                    if (block == null)
                    {
                        throw new Exception("No default data found for block " + eepElement.Name);
                    }

                    if (block.Data.Count != eepElement.BlockSize * eepElement.DataSets)
                    {
                        throw new Exception(
                            "Default data of block " + eepElement.Name + " is expected to be: "
                            + eepElement.BlockSize + ", real value is: " + block.Data.Count);
                    }

                    eepElement.DefaultData = block.Data;
                }
                else if (eepElement.NvmRef.CrcUsage == "true")
                {
                    // Crc available for the block
                    var block = eepElement.NvmRef.InitCallback == "true"
                                    ? EepDefaultData.Defaults.SingleOrDefault(
                                        x => x.Name == eepElement.NvmRef.InitCallbackValue)
                                    : EepDefaultData.Defaults.SingleOrDefault(
                                        x => x.Name == eepElement.NvmRef.RomBlock);

                    if (block == null)
                    {
                        throw new Exception("No default data found for block " + eepElement.Name);
                    }

                    if ((block.Data.Count + (2 * eepElement.DataSets)) != eepElement.BlockSize * eepElement.DataSets)
                    {
                        throw new Exception(
                            "Default data of block " + eepElement.Name + " is expected to be: "
                            + eepElement.BlockSize + ", real value is: " + block.Data.Count);
                    }

                    var crc = Crc16.CalculateCrc16(block.Data.ToArray());
                    var dataWithCrc = new List<byte>();
                    dataWithCrc.AddRange(block.Data);
                    dataWithCrc.AddRange(crc);

                    eepElement.DefaultData = dataWithCrc;
                }
            }

            if (this.eepData.Any(x => x.DefaultData == null))
            {
                throw new Exception("Default data not correct!");
            }
        }
    }
}