namespace ProjectAssistant.Platform.Model
{
    using System.Collections.Generic;
    using System.IO;
    using ProjectAssistant.Contract.Model;

    /// <summary>
    /// Class ProjectInfo.
    /// </summary>
    /// <seealso cref="ItemInfo" />
    public class ProjectInfo<U> : ItemInfo
    {
        /// <summary>
        /// Gets or sets the assembly version.
        /// </summary>
        /// <value>The assembly version.</value>
        public string AssemblyVersion { get; set; }

        /// <summary>
        /// Gets or sets the assembly file version.
        /// </summary>
        /// <value>The assembly file version.</value>
        public string FileVersion { get; set; }

        /// <summary>
        /// Gets or sets the assembly informational version.
        /// </summary>
        /// <value>The assembly informational version.</value>
        public string InformationalVersion { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>The items.</value>
        public IList<U> Items { get; set; } = new List<U>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectInfo{U}"/> class.
        /// </summary>
        public ProjectInfo(){
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectInfo{U}"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public ProjectInfo(FileInfo data)
        {
            if (data == null)
            {
                return;
            }

            this.Name = System.IO.Path.GetFileNameWithoutExtension(data.Name);
            this.Path = data.FullName;
        }
    }
}
