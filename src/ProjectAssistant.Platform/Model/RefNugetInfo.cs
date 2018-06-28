namespace ProjectAssistant.Platform.Model
{
    using System.IO;

    /// <summary>
    /// Class ReferAssemblyInfo.
    /// </summary>
    /// <seealso />
    public class RefNugetInfo : NugetInfo<RefNugetInfo>
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
        public RefNugetInfo(FileInfo dataInfo) : base(dataInfo)
        {
            var dir = new DirectoryInfo(dataInfo.DirectoryName);
            this.Name = dir.Name;
        }
    }
}
