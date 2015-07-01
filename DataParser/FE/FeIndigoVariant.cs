// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndigoVariant.cs" company="Visteon">
//   
// </copyright>
// <summary>
//   The indigo variant.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace DataParser.FE
{
    using System;
    using System.IO;
    using System.Linq;

    /// <summary>
    ///     The indigo variant.
    /// </summary>
    public class IndigoVariant
    {
        /// <summary>
        ///     The max size merged file.
        /// </summary>
        private const ulong MaxSizeMergedFile = 32 * 1024;

        /// <summary>
        ///     The par files.
        /// </summary>
        private readonly string[] parFiles;

        /// <summary>
        ///     The seq files.
        /// </summary>
        private readonly string[] seqFiles;

        /// <summary>
        ///     The version.
        /// </summary>
        private readonly int[] version;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndigoVariant"/> class.
        /// </summary>
        /// <param Name="strSeqFolder">
        /// The str seq folder.
        /// </param>
        /// <param Name="strParFolder">
        /// The str par folder.
        /// </param>
        /// <param name="strSeqFolder">
        /// The str Seq Folder.
        /// </param>
        /// <param name="strParFolder">
        /// The str Par Folder.
        /// </param>
        /// <param name="versionF">
        /// The version f.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        public IndigoVariant(string strSeqFolder, string strParFolder, int[] versionF)
        {
            if (!Directory.Exists(strSeqFolder))
            {
                throw new ArgumentException("Folder " + strSeqFolder + " do not exist!");
            }

            if (!Directory.Exists(strParFolder))
            {
                throw new ArgumentException("Folder " + strParFolder + " do not exist!");
            }

            this.version = versionF;
            this.seqFiles = Directory.GetFiles(strSeqFolder);
            this.parFiles = Directory.GetFiles(strParFolder);
        }

        /// <summary>
        /// The copy files.
        /// </summary>
        /// <param Name="outputFolder">
        /// The output folder.
        /// </param>
        /// <param name="outputFolder">
        /// The output Folder.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        public void CopyFiles(string outputFolder)
        {
            var frontEndParameterData = "FrontEndParameterData_" + this.version[0] + "_" + this.version[1] + ".gdc32dat";
            var frontEndProductionData = "FrontEndProductionData_" + this.version[0] + "_" + this.version[1]
                                         + ".gdc32dat";
            var frontEndSupplierSecuredData = "FrontEndSupplierSecuredData_" + this.version[0] + "_" + this.version[1]
                                              + ".gdc32dat";

            var fileFrontEndParameterData = this.parFiles.Where(x => x.EndsWith(frontEndParameterData)).ToArray();
            if (fileFrontEndParameterData.Length == 0)
            {
                throw new ArgumentException("File " + frontEndParameterData + " do not exist!");
            }

            File.Copy(fileFrontEndParameterData[0], outputFolder + "\\0x017F4000_FrontEndParameterData.gdc32dat");

            var fileFrontEndProductionData = this.parFiles.Where(x => x.EndsWith(frontEndProductionData)).ToArray();
            if (fileFrontEndProductionData.Length == 0)
            {
                throw new ArgumentException("File " + frontEndProductionData + " do not exist!");
            }

            File.Copy(fileFrontEndProductionData[0], outputFolder + "\\0x017F6000_FrontEndProductionData.gdc32dat");

            var fileFrontEndSupplierSecuredData =
                this.parFiles.Where(x => x.EndsWith(frontEndSupplierSecuredData)).ToArray();
            if (fileFrontEndSupplierSecuredData.Length == 0)
            {
                throw new ArgumentException("File " + frontEndSupplierSecuredData + " do not exist!");
            }

            File.Copy(
                fileFrontEndSupplierSecuredData[0], 
                outputFolder + "\\0x017F7000_FrontEndSupplierSecuredData.gdc32dat");

            foreach (var item in this.seqFiles)
            {
                var file = Path.GetFileName(item);
                if (file != "0x017F4000_FrontEndParameterData.gdc32dat"
                    && file != "0x017F6000_FrontEndProductionData.gdc32dat"
                    && file != "0x017F7000_FrontEndSupplierSecuredData.gdc32dat"
                    && file != Path.GetFileName(outputFolder) + ".gdc32dat"
                    && file != "default.gdc32dat")
                {
                    File.Copy(item, outputFolder + "\\" + file);
                }
            }
        }

        /// <summary>
        /// The merge files.
        /// </summary>
        /// <param Name="outputFolder">
        /// The output folder.
        /// </param>
        /// <param Name="name">
        /// The Name.
        /// </param>
        /// <param name="outputFolder">
        /// The output Folder.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        public void MergeFiles(string outputFolder, string name)
        {
            var filesToMerge = Directory.GetFiles(outputFolder, "0x*.gdc32dat").OrderBy(x => x).ToArray();
            var offset = this.GetAddress(filesToMerge[0]);
            var lastFile = new FileInfo(filesToMerge[filesToMerge.Length - 1]);
            var end = this.GetAddress(lastFile.FullName) + (ulong)lastFile.Length;
            if (end - offset > MaxSizeMergedFile)
            {
                throw new ArgumentException("Merged file more than 32K!");
            }

            var arrayData = Enumerable.Repeat<byte>(0xFF, (int)(end - offset)).ToList<byte>();

            foreach (var file in filesToMerge)
            {
                using (var b = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    var address = this.GetAddress(file);
                    var pos = 0;
                    var length = (int)b.BaseStream.Length;

                    while (pos < length)
                    {
                        if (arrayData[(int)address + pos - (int)offset] != 0xFF)
                        {
                            throw new ArgumentException(
                                string.Format("Address 0x{0:X} overlapps!", address + (ulong)pos));
                        }

                        arrayData[(int)address + pos - (int)offset] = b.ReadByte();
                        pos += sizeof(byte);
                    }
                }

                using (var b = new BinaryWriter(File.Open(outputFolder + "\\" + name + ".gdc32dat", FileMode.Create)))
                {
                    foreach (var data in arrayData)
                    {
                        b.Write(data);
                    }
                }

                if (name == "PRODUCTION")
                {
                    var mhx = new MhxWriter(outputFolder + "\\PRODUCTION.mhx", arrayData, 0x2000);
                    var src = new SourceWritter(outputFolder + "\\..\\source.c", arrayData);
                }
            }
        }

        /// <summary>
        /// The get address.
        /// </summary>
        /// <param Name="filename">
        /// The filename.
        /// </param>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <returns>
        /// The <see cref="ulong"/>.
        /// </returns>
        private ulong GetAddress(string filename)
        {
            return filename != null ? Convert.ToUInt32(Path.GetFileName(filename).Substring(2, 8), 16) : 0;
        }
    }
}