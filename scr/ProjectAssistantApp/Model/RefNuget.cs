namespace ProjectAssistant.App.Model
{
    using Platform.Model;

    public class RefNuget : Nuget
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RefNuget"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public RefNuget(NugetInfo<RefNugetInfo> data) : base(data)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Project" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="data">The data.</param>
        public RefNuget(Nuget parent, NugetInfo<RefNugetInfo> data) : base(data)
        {
            this.Parent = parent;
        }

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
        public new Nuget Parent { get; set; }

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
