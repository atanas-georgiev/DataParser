// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MHXWriter.cs" company="Visteon">
//   
// </copyright>
// <summary>
//   The mhx writer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace DataParser.FE
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    ///     The mhx writer.
    /// </summary>
    public class MhxWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MhxWriter"/> class.
        /// </summary>
        /// <param Name="filename">
        /// The filename.
        /// </param>
        /// <param Name="data">
        /// The data.
        /// </param>
        /// <param Name="offset">
        /// The offset.
        /// </param>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <param name="offset">
        /// The offset.
        /// </param>
        public MhxWriter(string filename, List<byte> data, int offset)
        {
            using (var b = new StreamWriter(File.Open(filename, FileMode.Create)))
            {
                b.WriteLine("S009000047414C4550335A");

                for (var i = 0; i < data.Count; i += 16)
                {
                    if (data.Count - i >= 16)
                    {
                        b.Write("S113");
                        b.Write("{0:X4}", i + offset);

                        foreach (var item in data.GetRange(i, 16))
                        {
                            b.Write("{0:X2}", item);
                        }

                        b.Write("{0:X2}", this.CalculateCheckSum(i + offset, data.GetRange(i, 16)));

                        b.Write("\n");
                    }
                    else
                    {
                        b.Write("S1");
                        b.Write("{0:X2}", data.Count - i + 3);
                        b.Write("{0:X4}", i + offset);

                        foreach (var item in data.GetRange(i, data.Count - i))
                        {
                            b.Write("{0:X2}", item);
                        }

                        b.Write("{0:X2}", this.CalculateCheckSum(i + offset, data.GetRange(i, data.Count - i)));

                        b.Write("\n");
                    }
                }

                b.WriteLine("S9030000FC");
            }
        }

        /// <summary>
        /// The calculate check sum.
        /// </summary>
        /// <param Name="address">
        /// The address.
        /// </param>
        /// <param Name="data">
        /// The data.
        /// </param>
        /// <param name="address">
        /// The address.
        /// </param>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The <see cref="byte"/>.
        /// </returns>
        private byte CalculateCheckSum(int address, List<byte> data)
        {
            var result = (byte)(data.Sum(x => x) + (address & 0xFF) + ((address >> 8) & 0xFF) + (data.Count + 3));
            return (byte)(0xFF - result);
        }
    }
}