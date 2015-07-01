// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EepDefaultData.cs" company="Visteon">
//   
// </copyright>
// <summary>
//   The eep default data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace DataParser.EEP
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    ///     The eep default data.
    /// </summary>
    internal class EepDefaultData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EepDefaultData"/> class.
        /// </summary>
        /// <param Name="list">
        /// The list.
        /// </param>
        /// <param Name="cfgManFile">
        /// The cfg man file.
        /// </param>
        /// <param Name="rteFile">
        /// The rte file.
        /// </param>
        /// <param Name="codingFile">
        /// The coding file.
        /// </param>
        /// <param Name="dltFile">
        /// The dlt file.
        /// </param>
        /// <param name="list">
        /// The list.
        /// </param>
        /// <param name="cfgManFile">
        /// The cfg Man File.
        /// </param>
        /// <param name="rteFile">
        /// The rte File.
        /// </param>
        /// <param name="codingFile">
        /// The coding File.
        /// </param>
        /// <param name="dltFile">
        /// The dlt File.
        /// </param>
        public EepDefaultData(
            IEnumerable<EepDefaultBlockStruct> list, 
            string cfgManFile, 
            string rteFile, 
            string codingFile, 
            string dltFile)
        {
            const string String1 = @"    {
        0xFF, 0xFF
    }, // MANUFACTURING_MODE_CONDITION_1,  Type: U16
    0xFF, // DFE_PRESENCE,  Type: U8
    {
        0xFF, 0xFF
    }, // MANUFACTURING_MODE_CONDITION_2,  Type: U16";
            const string String2 = @"    {
        0xA5, 0xCA
    }, // MANUFACTURING_MODE_CONDITION_1,  Type: U16
    0xA5, // DFE_PRESENCE,  Type: U8
    {
        0x5A, 0x35
    }, // MANUFACTURING_MODE_CONDITION_2,  Type: U16";

            Defaults = new List<EepDefaultBlockStruct>();

            foreach (var d in list)
            {
                Defaults.Add(d);
            }

            var text = File.ReadAllText(cfgManFile);
            var first = text.IndexOf("KSS_START_SEC_CONST_NVDATA", StringComparison.Ordinal);
            var last = text.LastIndexOf("KSS_STOP_SEC_CONST_NVDATA", StringComparison.Ordinal);

            text = text.Substring(first, last - first);

            var data = text.Split(new[] { "const" }, StringSplitOptions.None);

            var block = new EepDefaultBlockStruct();

            for (var i = 1; i < data.Length; i++)
            {
                block = new EepDefaultBlockStruct { Name = data[i].Split(' ')[3], Data = new List<byte>() };

                if (block.Name == "NvmAppl_RomB6006_KSS")
                {
                    data[i] = data[i].Replace(String1, String2);
                }

                foreach (var el in data[i].Split())
                {
                    if (el.StartsWith("0x"))
                    {
                        block.Data.Add(Convert.ToByte(el.Substring(2, 2), 16));
                    }
                }

                Defaults.Add(block);
            }

            ///////////////////////////////////////////////////
            text = File.ReadAllText(rteFile);
            first = text.IndexOf("RTE_START_SEC_CONST_DEFAULT_RTE_CDATA_GROUP_UNSPECIFIED", StringComparison.Ordinal);
            last = text.LastIndexOf("RTE_STOP_SEC_CONST_DEFAULT_RTE_CDATA_GROUP_UNSPECIFIED", StringComparison.Ordinal);

            text = text.Substring(first, last - first);

            data = text.Split(new[] { "CONST" }, StringSplitOptions.None);

            block = new EepDefaultBlockStruct();

            for (var i = 0; i < data.Length; i++)
            {
                if (data[i].Contains("="))
                {
                    block = new EepDefaultBlockStruct { Name = data[i].Split(' ')[1], Data = new List<byte>() };

                    first = data[i].IndexOf("{", StringComparison.Ordinal);
                    last = data[i].LastIndexOf("}", StringComparison.Ordinal);

                    data[i] = data[i].Substring(first, last - first);
                    data[i] = data[i].Replace("U", ",").Replace(" ", ",").Replace("}", ",").Replace("{", ",");

                    foreach (var d in data[i].Split(','))
                    {
                        if (Regex.IsMatch(d, @"\d") && d != "3408")
                        {
                            if (block.Name != "Rte_SwcBc_Cal_BcConsCharFactor")
                            {
                                block.Data.Add(byte.Parse(d));
                            }
                            else
                            {
                                var num = int.Parse(d);
                                block.Data.Add((byte)(num & 0xFF));
                                block.Data.Add((byte)((num >> 8) & 0xFF));
                            }
                        }
                    }

                    Defaults.Add(block);
                }
            }

            ///////////////////////////////////////////////////
            text = File.ReadAllText(dltFile);
            first = text.IndexOf("uint8 ", StringComparison.Ordinal);
            last = text.IndexOf("[", StringComparison.Ordinal);

            text = text.Substring(first, last - first);

            block = new EepDefaultBlockStruct { Name = text.Split(' ')[1] };

            text = File.ReadAllText(dltFile);
            first = text.IndexOf("{", StringComparison.Ordinal);
            last = text.IndexOf("}", StringComparison.Ordinal);
            text = text.Substring(first, last - first);
            data = text.Replace(' ', ',').Split(',');

            block.Data = new List<byte>();

            foreach (var t in data)
            {
                if (Regex.IsMatch(t, @"\d"))
                {
                    block.Data.Add(byte.Parse(t));
                }
            }

            Defaults.Add(block);

            ///////////////////////////////////////////////////
            text = File.ReadAllText(codingFile);
            first = text.IndexOf("Coding_START_SEC_CONST_8", StringComparison.Ordinal);
            last = text.IndexOf("Coding_STOP_SEC_CONST_8", StringComparison.Ordinal);
            text = text.Substring(first, last - first);

            data = text.Split(new[] { "Coding_CONST" }, StringSplitOptions.None);

            block = new EepDefaultBlockStruct();

            for (var i = 0; i < data.Length; i++)
            {
                if (data[i].Contains("="))
                {
                    block = new EepDefaultBlockStruct();
                    block.Name = data[i].Split(')')[1].Split('[')[0].Trim();
                    block.Data = new List<byte>();

                    first = data[i].IndexOf("{", StringComparison.Ordinal);
                    last = data[i].LastIndexOf("}", StringComparison.Ordinal);

                    data[i] = data[i].Substring(first, last - first);
                    data[i] = this.RemoveBetween(data[i], "/*", "*/");
                    block.Data = new List<byte>();

                    foreach (var d in data[i].Split(','))
                    {
                        byte val;

                        // try
                        // {
                        // byte.Parse(d.Trim(), NumberStyles.AllowHexSpecifier);
                        // }
                        // catch (Exception)
                        // {

                        // }
                        if (byte.TryParse(
                            d.Replace("0x", string.Empty).Trim(), 
                            NumberStyles.AllowHexSpecifier, 
                            CultureInfo.InvariantCulture, 
                            out val))
                        {
                            block.Data.Add(val);
                        }
                    }

                    Defaults.Add(block);
                }
            }

            ///////////////////////////////////////////////////
        }

        /// <summary>
        ///     The defaults.
        /// </summary>
        public static List<EepDefaultBlockStruct> Defaults { get; private set; }

        /// <summary>
        /// The remove between.
        /// </summary>
        /// <param Name="s">
        /// The s.
        /// </param>
        /// <param Name="begin">
        /// The begin.
        /// </param>
        /// <param Name="end">
        /// The end.
        /// </param>
        /// <param name="s">
        /// The s.
        /// </param>
        /// <param name="begin">
        /// The begin.
        /// </param>
        /// <param name="end">
        /// The end.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string RemoveBetween(string s, string begin, string end)
        {
            var regex = new Regex(string.Format("\\{0}.*?\\{1}", begin, end));
            return regex.Replace(s, string.Empty);
        }
    }
}