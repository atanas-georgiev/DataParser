// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BinFileGenerator.cs" company="Visteon">
//   
// </copyright>
// <summary>
//   The bin file generator.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace DataParser.EEP
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    ///     The bin file generator.
    /// </summary>
    internal class EepBinFileGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EepBinFileGenerator"/> class.
        /// </summary>
        /// <param Name="filename">
        /// The filename.
        /// </param>
        /// <param Name="image">
        /// The image.
        /// </param>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <param name="image">
        /// The image.
        /// </param>
        public EepBinFileGenerator(string filename, IEnumerable<byte> image)
        {
            if (File.Exists(filename))
            {
                File.SetAttributes(filename, FileAttributes.Normal);
                File.Delete(filename);
            }

            using (var writer = new BinaryWriter(File.Open(filename, FileMode.Create)))
            {
                foreach (var b in image)
                {
                    writer.Write(b);
                }
            }
        }
    }
}