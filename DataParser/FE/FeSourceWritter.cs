// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SourceWritter.cs" company="Visteon">
//   
// </copyright>
// <summary>
//   The source writter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace DataParser.FE
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    ///     The source writter.
    /// </summary>
    public class SourceWritter
    {
        /// <summary>
        ///     The block 0 address.
        /// </summary>
        private const int Block0Address = 0x0;

        /// <summary>
        ///     The block 1 address.
        /// </summary>
        private const int Block1Address = 0x2000;

        /// <summary>
        ///     The block 3 address.
        /// </summary>
        private const int Block3Address = 0x6000;

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceWritter"/> class.
        /// </summary>
        /// <param Name="filename">
        /// The filename.
        /// </param>
        /// <param Name="data">
        /// The data.
        /// </param>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <param name="data">
        /// The data.
        /// </param>
        public SourceWritter(string filename, List<byte> data)
        {
            using (var b = new StreamWriter(File.Open(filename, FileMode.Create)))
            {
                int i;

                for (i = Block1Address - 1; i >= Block0Address; i--)
                {
                    if (data[i] != 0xFF)
                    {
                        break;
                    }
                }

                b.Write("#define cBlock0Size {0}\n", i + 1);
                b.Write("const uint8 u8FeBoot_Block0[cBlock0Size] =\n{");

                for (var j = 0; j <= i; j++)
                {
                    if (j % 16 == 0)
                    {
                        b.Write("\n    ");
                    }

                    b.Write("0x{0:X2}", data[j]);
                    if (j != i)
                    {
                        b.Write(", ");
                    }
                }

                b.WriteLine("\n};\n");

                b.Write("#define cBlock3Size {0}\n", data.Count - Block3Address);
                b.Write("const uint8 u8FeBoot_Block3[cBlock3Size] =\n{");

                for (var j = Block3Address; j < data.Count; j++)
                {
                    if (j % 16 == 0)
                    {
                        b.Write("\n    ");
                    }

                    b.Write("0x{0:X2}", data[j]);
                    if (j != data.Count - 1)
                    {
                        b.Write(", ");
                    }
                }

                b.WriteLine("\n};\n");
            }
        }
    }
}