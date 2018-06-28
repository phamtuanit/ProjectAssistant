namespace ProjectAssistant.App.Model
{
    using Platform.Model;

    public class RefProject : Project
    {
        /// <summary>
        /// Gets or sets the reference version.
        /// </summary>
        /// <value>
        /// The reference version.
        /// </value>
        public string RefVersion { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public Project Parent { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RefProject"/> class.
        /// </summary>
        public RefProject() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Project" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="data">The data.</param>
        public RefProject(Project parent, ProjectInfo<ReferAssemblyInfo> data) : base(data)
        {
            this.Parent = parent;
        }

        #region Overrides of Project

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            this.OnPropertyChanged("RefVersion");
        }

        #endregion

        /// <summary>
        /// References the checked.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        public void RefChecked(object dataContext)
        {
            this.Parent?.Refresh();
        }
    }
}
