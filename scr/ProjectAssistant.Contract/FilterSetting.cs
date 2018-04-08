namespace ProjectAssistant.Contract
{
    /// <summary>
    /// Class FilterSetting.
    /// </summary>
    public class FilterSetting
    {
        /// <summary>
        /// Gets or sets the root dir.
        /// </summary>
        /// <value>The root dir.</value>
        public string RootDir { get; set; }

        /// <summary>
        /// Gets or sets the project filter.
        /// </summary>
        /// <value>The project filter.</value>
        public string ProjectFilter { get; set; }

        /// <summary>
        /// Gets or sets the nuget filter.
        /// </summary>
        /// <value>The nuget filter.</value>
        public string NuspecFilter { get; set; }

        /// <summary>
        /// Gets or sets the nuget configuration filter.
        /// </summary>
        /// <value>
        /// The nuget configuration filter.
        /// </value>
        public string NugetConfigFilter { get; set; }

        /// <summary>
        /// Gets or sets the reference assembly filter.
        /// </summary>
        /// <value>The reference assembly filter.</value>
        public string ReferenceAssemblyFilter { get; set; }

        /// <summary>
        /// Gets or sets the reference nuget filter.
        /// </summary>
        /// <value>
        /// The reference nuget filter.
        /// </value>
        public string ReferenceNugetFilter { get; set; }

        /// <summary>
        /// Gets or sets the assembly version.
        /// </summary>
        /// <value>The assembly version.</value>
        public string AssemblyVersion { get; set; }

        /// <summary>
        /// Gets or sets the nuget version.
        /// </summary>
        /// <value>The nuget version.</value>
        public string NugetVersion { get; set; }

        /// <summary>
        /// The nuget package output
        /// </summary>
        public string NugetOutput { get; set; }

        /// <summary>
        /// The nuget tool path
        /// </summary>
        public string NugetToolDir { get; set; }
    }
}
