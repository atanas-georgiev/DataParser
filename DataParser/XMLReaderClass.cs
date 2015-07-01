// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XMLReader.cs" company="Visteon">
//   
// </copyright>
// <summary>
//   The xml reader.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace DataParser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;

    using DataParser.EEP;

    /// <summary>
    ///     The xml reader.
    /// </summary>
    internal class XmlReaderClass
    {
        /// <summary>
        ///     The tem p_ filename.
        /// </summary>
        private const string TempFilename = "conf.xml";

        /// <summary>
        ///     The parameter version.
        /// </summary>
        private readonly int[] feparameterversion = new int[2];

        /// <summary>
        ///     The fevariants.
        /// </summary>
        private readonly List<string> fevariants = new List<string>();

        /// <summary>
        ///     The eep conf file.
        /// </summary>
        private string eepConfFile;

        /// <summary>
        ///     The eep zip file.
        /// </summary>
        private string eepZipFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlReaderClass"/> class.
        ///     Initializes a new instance of the <see cref="XmlReader"/> class.
        /// </summary>
        /// <param Name="xmlConfiguration">
        /// The xml configuration.
        /// </param>
        /// <param name="xmlConfiguration">
        /// The xml Configuration.
        /// </param>
        public XmlReaderClass(string xmlConfiguration)
        {
            var xmlDoc = XDocument.Load(xmlConfiguration);

            this.ReadXmlParameters(xmlDoc);
            this.ReadConfigZipData();
        }

        /// <summary>
        ///     Gets the variants.
        /// </summary>
        public List<string> FeVariants
        {
            get
            {
                return this.fevariants;
            }
        }

        /// <summary>
        ///     Gets the input parameter folder.
        /// </summary>
        public string FeInputParameterFolder { get; private set; }

        /// <summary>
        ///     Gets the input sequence folder.
        /// </summary>
        public string FeInputSequenceFolder { get; private set; }

        /// <summary>
        ///     Gets the parameter version.
        /// </summary>
        public int[] FeParameterVersion
        {
            get
            {
                return this.feparameterversion;
            }
        }

        /// <summary>
        ///     Gets the output folder.
        /// </summary>
        public string FeOutputFolder { get; private set; }

        /// <summary>
        ///     Gets or sets the eep data.
        /// </summary>
        public List<EepEaDataStructure> EepData { get; set; }

        /// <summary>
        ///     Gets or sets the nv data.
        /// </summary>
        public List<EepNvmDataStructure> NvData { get; set; }

        /// <summary>
        ///     Gets the eep cfg man defaults file.
        /// </summary>
        public string EepCfgManDefaultsFile { get; private set; }

        /// <summary>
        ///     Gets the rte defaults file.
        /// </summary>
        public string RteDefaultsFile { get; private set; }

        /// <summary>
        ///     Gets the coding defaults file.
        /// </summary>
        public string CodingDefaultsFile { get; private set; }

        /// <summary>
        ///     Gets the dlt defaults file.
        /// </summary>
        public string DltDefaultsFile { get; private set; }

        /// <summary>
        ///     Gets the diag script file.
        /// </summary>
        public string DiagScriptFile { get; private set; }

        /// <summary>
        ///     Gets the bin file.
        /// </summary>
        public string BinFile { get; private set; }

        /// <summary>
        ///     Gets the defaults.
        /// </summary>
        public List<EepDefaultBlockStruct> Defaults { get; private set; }

        /// <summary>
        /// The read ea data.
        /// </summary>
        /// <param Name="xmlDoc">
        /// The xml doc.
        /// </param>
        /// <param name="xmlDoc">
        /// The xml Doc.
        /// </param>
        private void ReadEaData(XmlDocument xmlDoc)
        {
            this.EepData = new List<EepEaDataStructure>();
            var xmlNodeList = xmlDoc.GetElementsByTagName("ECUC-MODULE-CONFIGURATION-VALUES");

            foreach (
                var xmlNode in xmlNodeList.Cast<XmlNode>().Where(xmlNode => xmlNode.ChildNodes[0].InnerText == "Ea"))
            {
                var xmlEa = xmlNode;

                foreach (XmlNode node in xmlEa.ChildNodes[4].ChildNodes)
                {
                    if (node["PARAMETER-VALUES"] != null && node["SHORT-NAME"] != null)
                    {
                        if (node["PARAMETER-VALUES"].FirstChild.FirstChild.InnerText
                            != "/MICROSAR/Ea/EaBlockConfiguration/EaBlockNumber")
                        {
                            continue;
                        }

                        var el = new EepEaDataStructure { Name = node["SHORT-NAME"].InnerText };

                        foreach (XmlNode node1 in node["PARAMETER-VALUES"].ChildNodes)
                        {
                            if (node1.ChildNodes[0].InnerText == "/MICROSAR/Ea/EaBlockConfiguration/EaBlockId")
                            {
                                el.BlockId = int.Parse(node1.ChildNodes[1].InnerText);
                            }

                            if (node1.ChildNodes[0].InnerText == "/MICROSAR/Ea/EaBlockConfiguration/EaBlockNumber")
                            {
                                el.BlockNumber = int.Parse(node1.ChildNodes[1].InnerText);
                            }

                            if (node1.ChildNodes[0].InnerText == "/MICROSAR/Ea/EaBlockConfiguration/EaBlockSize")
                            {
                                el.BlockSize = int.Parse(node1.ChildNodes[1].InnerText);
                            }

                            if (node1.ChildNodes[0].InnerText == "/MICROSAR/Ea/EaBlockConfiguration/EaNumberOfDatasets")
                            {
                                el.DataSets = int.Parse(node1.ChildNodes[1].InnerText);
                            }
                        }

                        this.EepData.Add(el);
                    }
                }
            }
        }

        /// <summary>
        /// The read nvm data.
        /// </summary>
        /// <param Name="xmlDoc">
        /// The xml doc.
        /// </param>
        /// <param name="xmlDoc">
        /// The xml Doc.
        /// </param>
        private void ReadNvmData(XmlDocument xmlDoc)
        {
            this.NvData = new List<EepNvmDataStructure>();
            var xmlNodeList = xmlDoc.GetElementsByTagName("ECUC-MODULE-CONFIGURATION-VALUES");

            foreach (var xmlNode in
                xmlNodeList.Cast<XmlNode>().Where(xmlNode => xmlNode.ChildNodes[0].InnerText == "NvM"))
            {
                var xmlNvm = xmlNode;

                foreach (XmlNode node in xmlNvm.ChildNodes[4].ChildNodes)
                {
                    if (node != null && node["SHORT-NAME"] != null && node["PARAMETER-VALUES"] != null
                        && node["SUB-CONTAINERS"] != null)
                    {
                        var el = new EepNvmDataStructure();
                        el.Descriptor = node["SHORT-NAME"].InnerText;

                        foreach (XmlNode node1 in node["PARAMETER-VALUES"].ChildNodes)
                        {
                            if (node1.ChildNodes[0].InnerText == "/MICROSAR/NvM/NvMBlockDescriptor/NvMBlockCrcType")
                            {
                                el.Crc = node1.ChildNodes[1].InnerText;
                            }

                            if (node1.ChildNodes[0].InnerText
                                == "/MICROSAR/NvM/NvMBlockDescriptor/NvMBlockManagementType")
                            {
                                el.Type = node1.ChildNodes[1].InnerText;
                            }

                            if (node1.ChildNodes[0].InnerText == "/MICROSAR/NvM/NvMBlockDescriptor/NvMNvBlockNum")
                            {
                                el.BlockId = int.Parse(node1.ChildNodes[1].InnerText);
                            }

                            if (node1.ChildNodes[0].InnerText == "/MICROSAR/NvM/NvMBlockDescriptor/NvMBlockUseCrc")
                            {
                                el.CrcUsage = node1.ChildNodes[1].InnerText;
                            }

                            if (node1.ChildNodes[0].InnerText == "/MICROSAR/NvM/NvMBlockDescriptor/NvMNvBlockLength")
                            {
                                el.Length = int.Parse(node1.ChildNodes[1].InnerText);
                            }

                            if (node1.ChildNodes[0].InnerText
                                == "/MICROSAR/NvM/NvMBlockDescriptor/NvMRomBlockDataAddress")
                            {
                                el.RomBlock = node1.ChildNodes[1].InnerText;
                            }

                            if (node1.ChildNodes[0].InnerText == "/MICROSAR/NvM/NvMBlockDescriptor/NvMUseInitCallback")
                            {
                                el.InitCallback = node1.ChildNodes[1].InnerText;
                            }

                            if (node1.ChildNodes[0].InnerText == "/MICROSAR/NvM/NvMBlockDescriptor/NvMInitBlockCallback")
                            {
                                el.InitCallbackValue = node1.ChildNodes[1].InnerText;
                            }
                        }

                        if (node["SUB-CONTAINERS"].FirstChild.ChildNodes[2].FirstChild.ChildNodes[2] != null)
                        {
                            var node2 =
                                node["SUB-CONTAINERS"].FirstChild.ChildNodes[2].FirstChild.ChildNodes[2].FirstChild
                                    .ChildNodes[1];
                            var vals = node2.InnerText.Split('/');
                            el.EaRef = vals[vals.Length - 1];
                        }

                        this.NvData.Add(el);
                    }
                }
            }
        }

        /// <summary>
        /// The read xml parameters.
        /// </summary>
        /// <param Name="xmlDoc">
        /// The xml doc.
        /// </param>
        /// <param name="xmlDoc">
        /// The xml Doc.
        /// </param>
        private void ReadXmlParameters(XDocument xmlDoc)
        {
            if (xmlDoc.Root == null)
            {
                throw new Exception("Problem opening xml configuration!");
            }

            this.FeInputParameterFolder = xmlDoc.Root.Elements("FE").Elements("InputParameterFolder").First().Value;
            this.FeInputSequenceFolder = xmlDoc.Root.Elements("FE").Elements("InputSequenceFolder").First().Value;
            this.FeOutputFolder = xmlDoc.Root.Elements("FE").Elements("OutputFolder").First().Value;
            this.feparameterversion[0] =
                int.Parse(xmlDoc.Root.Elements("FE").Elements("ParameterVersionMajor").First().Value);
            this.feparameterversion[1] =
                int.Parse(xmlDoc.Root.Elements("FE").Elements("ParameterVersionMinor").First().Value);
            foreach (var item in xmlDoc.Root.Elements("FE").Elements("Variants").Elements())
            {
                this.fevariants.Add(item.Value);
            }

            this.eepZipFile = xmlDoc.Root.Elements("EEP").Elements("ZipConfiguration").First().Value;
            this.eepConfFile = xmlDoc.Root.Elements("EEP").Elements("ConfigurationFile").First().Value;
            this.EepCfgManDefaultsFile = xmlDoc.Root.Elements("EEP").Elements("CfgManDefaultsFile").First().Value;
            this.RteDefaultsFile = xmlDoc.Root.Elements("EEP").Elements("RteDefaultsFile").First().Value;
            this.CodingDefaultsFile = xmlDoc.Root.Elements("EEP").Elements("CodingDefaultsFile").First().Value;
            this.DltDefaultsFile = xmlDoc.Root.Elements("EEP").Elements("DltDefaultsFile").First().Value;
            this.DiagScriptFile = xmlDoc.Root.Elements("EEP").Elements("DiagScriptFileOutput").First().Value;
            this.BinFile = xmlDoc.Root.Elements("EEP").Elements("BinFileOutput").First().Value;

            this.Defaults = new List<EepDefaultBlockStruct>();
            foreach (var item in xmlDoc.Root.Elements("EEP").Elements("DefaultData").Elements())
            {
                var block = new EepDefaultBlockStruct { Name = item.Attribute("name").Value, Data = new List<byte>() };

                foreach (var val in item.Attribute("data").Value.Split(','))
                {
                    block.Data.Add(byte.Parse(val));
                }

                this.Defaults.Add(block);
            }
        }

        /// <summary>
        ///     The read config zip data.
        /// </summary>
        private void ReadConfigZipData()
        {
            // Clear temp config file
            if (File.Exists(TempFilename))
            {
                File.SetAttributes(TempFilename, FileAttributes.Normal);
                File.Delete(TempFilename);
            }

            // Extract configurator configuration from config.zip
            using (var zip = ZipFile.Open(this.eepZipFile, ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries.Where(entry => entry.Name == this.eepConfFile))
                {
                    entry.ExtractToFile(TempFilename);
                }
            }

            // Read configuration xml
            var xmlDoc1 = new XmlDocument();
            xmlDoc1.Load(TempFilename);

            // Extract Ea and Nvm data
            this.ReadEaData(xmlDoc1);
            this.ReadNvmData(xmlDoc1);

            // Remove temp file
            File.SetAttributes(TempFilename, FileAttributes.Normal);
            File.Delete(TempFilename);

            // Add nvm pointers
            foreach (var e in this.EepData)
            {
                e.NvmRef = this.NvData.FirstOrDefault(x => x.EaRef == e.Name);
            }
        }
    }
}