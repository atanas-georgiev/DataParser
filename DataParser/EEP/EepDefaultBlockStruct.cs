// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EepDefaultBlockStruct.cs" company="Visteon">
//   
// </copyright>
// <summary>
//   The default block.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace DataParser.EEP
{
    using System.Collections.Generic;

    /// <summary>
    ///     The default block.
    /// </summary>
    internal class EepDefaultBlockStruct
    {
        /// <summary>
        ///     Gets or sets the Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the data.
        /// </summary>
        public List<byte> Data { get; set; }
    }
}