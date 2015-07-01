// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EepEaDataStructure.cs" company="Visteon">
//   
// </copyright>
// <summary>
//   The ea data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace DataParser.EEP
{
    using System.Collections.Generic;

    /// <summary>
    ///     The ea data.
    /// </summary>
    internal class EepEaDataStructure
    {
        /// <summary>
        ///     The nvm ref.
        /// </summary>
        public EepNvmDataStructure NvmRef { get; set; }

        /// <summary>
        ///     Gets or sets the Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the block id.
        /// </summary>
        public int BlockId { get; set; }

        /// <summary>
        ///     Gets or sets the block number.
        /// </summary>
        public int BlockNumber { get; set; }

        /// <summary>
        ///     Gets or sets the block size.
        /// </summary>
        public int BlockSize { get; set; }

        /// <summary>
        ///     Gets or sets the data sets.
        /// </summary>
        public int DataSets { get; set; }

        /// <summary>
        ///     Gets or sets the default data.
        /// </summary>
        public List<byte> DefaultData { get; set; }
    }
}