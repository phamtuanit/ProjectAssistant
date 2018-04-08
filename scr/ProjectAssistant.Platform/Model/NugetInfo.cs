namespace ProjectAssistant.Platform.Model
{
    using System.Collections.Generic;
    using System.IO;
    using Helper;
    using ProjectAssistant.Contract.Model;

    public class NugetInfo<U> : ItemInfo
    {
        /// <summary>
        /// Gets or sets the nuget version.
        /// </summary>
        /// <value>The nuget version.</value>
        public string NugetVersion { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>The items.</value>
        public IList<U> Items { get; set; } = new List<U>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NugetInfo{U}" /> class.
        /// </summary>
        public NugetInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NugetInfo{U}" /> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public NugetInfo(FileInfo data) : this()
        {
            if (data == null)
            {
                return;
            }

            this.Name = System.IO.Path.GetFileName(data.Name);
            this.Path = data.FullName;

            this.BuildNugetVersion();
        }
    }
}
