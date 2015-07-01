// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EepDiagScriptGenerator.cs" company="Visteon">
//   
// </copyright>
// <summary>
//   The diag script generator.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace DataParser.EEP
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    ///     The diag script generator.
    /// </summary>
    internal class EepDiagScriptGenerator
    {
        /// <summary>
        ///     The step.
        /// </summary>
        private const int Step = 0x40;

        /// <summary>
        ///     The enter eol.
        /// </summary>
        private const string EnterEol =
            "DIAG [Manufacturing Session] SEND [10,70] EXPECT [50,70,*] TIMEOUT [5000] \nWAIT[5000]\n";

        /// <summary>
        ///     The write prefix.
        /// </summary>
        private const string WritePrefix = "DIAG [Write data] SEND [31,01,F0,FA,05,00,";

        /// <summary>
        ///     The write suffix.
        /// </summary>
        private const string WriteSuffix = "] EXPECT [71,01,F0,FA] TIMEOUT [2000]";

        /// <summary>
        /// Initializes a new instance of the <see cref="EepDiagScriptGenerator"/> class.
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
        public EepDiagScriptGenerator(string filename, List<byte> image)
        {
            if (File.Exists(filename))
            {
                File.SetAttributes(filename, FileAttributes.Normal);
                File.Delete(filename);
            }

            using (var writer = new StreamWriter(filename))
            {
                writer.WriteLine(EnterEol);

                for (var i = 0; i < image.Count; i += Step)
                {
                    var rangeToWrite = image.GetRange(i, Step).ConvertAll(x => x.ToString("X2"));
                    var line = WritePrefix + ((i >> 8) & 0xFF).ToString("X2") + "," + (i & 0xFF).ToString("X2") + ","
                               + ((Step >> 8) & 0xFF).ToString("X2") + "," + (Step & 0xFF).ToString("X2") + ","
                               + string.Join(",", rangeToWrite) + WriteSuffix;
                    writer.WriteLine(line);
                }
            }
        }
    }
}