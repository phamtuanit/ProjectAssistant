namespace ProjectAssistant.Platform.Model
{
    using System.IO;

    /// <summary>
    /// Class ReferAssemblyInfo.
    /// </summary>
    /// <seealso />
    public class ReferAssemblyInfo : ProjectInfo<ReferAssemblyInfo>
    {
        /// <summary>
        /// Gets or sets the reference version.
        /// </summary>
        /// <value>The reference version.</value>
        public string RefVersion { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferAssemblyInfo" /> class.
        /// </summary>
        /// <param name="dataInfo">The data information.</param>
        public ReferAssemblyInfo(FileInfo dataInfo) : base(dataInfo)
        {
        }
    }
}
