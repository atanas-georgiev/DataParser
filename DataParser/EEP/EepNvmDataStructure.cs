// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EepNvmDataStructure.cs" company="Visteon">
//   
// </copyright>
// <summary>
//   The eep nvm data structure.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace DataParser.EEP
{
    /// <summary>
    ///     The eep nvm data structure.
    /// </summary>
    internal class EepNvmDataStructure
    {
        /// <summary>
        ///     Gets or sets the Descriptor.
        /// </summary>
        public string Descriptor { get; set; }

        /// <summary>
        ///     Gets or sets the Length.
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        ///     Gets or sets the block id.
        /// </summary>
        public int BlockId { get; set; }

        /// <summary>
        ///     Gets or sets the Crc.
        /// </summary>
        public string Crc { get; set; }

        /// <summary>
        ///     Gets or sets the CrcUsage.
        /// </summary>
        public string CrcUsage { get; set; }

        /// <summary>
        ///     Gets or sets the Type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        ///     Gets or sets the InitCallback.
        /// </summary>
        public string InitCallback { get; set; }

        /// <summary>
        ///     Gets or sets the InitCallbackValue.
        /// </summary>
        public string InitCallbackValue { get; set; }

        /// <summary>
        ///     Gets or sets the rom block.
        /// </summary>
        public string RomBlock { get; set; }

        /// <summary>
        ///     Gets or sets the ea ref.
        /// </summary>
        public string EaRef { get; set; }
    }
}