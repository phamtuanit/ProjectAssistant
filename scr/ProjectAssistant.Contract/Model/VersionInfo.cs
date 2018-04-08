namespace ProjectAssistant.Contract.Model
{
    /// <summary>
    /// Class VersionInfo.
    /// </summary>
    public class VersionInfo
    {
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        public string Version { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionInfo"/> class.
        /// </summary>
        /// <param name="version">The version.</param>
        public VersionInfo(string version)
        {
            this.Version = version;
        }
    }
}
