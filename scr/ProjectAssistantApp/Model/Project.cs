namespace ProjectAssistant.App.Model
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Platform.Model;

    public class Project : ProjectInfo<ReferAssemblyInfo>, ICheckedNode, INotifyPropertyChanged
    {
        /// <summary>
        /// The is checked
        /// </summary>
        private bool? isChecked = false;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is checked.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is checked; otherwise, <c>false</c>.
        /// </value>
        public bool? IsChecked
        {
            get { return this.isChecked; }
            set
            {
                this.isChecked = value;
                this.UpdateChild(value);
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the assembly version.
        /// </summary>
        /// <value>
        /// The assembly version.
        /// </value>
        public new string AssemblyVersion => base.AssemblyVersion;

        /// <summary>
        /// Gets or sets the assembly file version.
        /// </summary>
        /// <value>
        /// The assembly file version.
        /// </value>
        public new string FileVersion => base.FileVersion;

        /// <summary>
        /// Gets or sets the assembly informational version.
        /// </summary>
        /// <value>
        /// The assembly informational version.
        /// </value>
        public new string InformationalVersion => base.InformationalVersion;

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public ICheckedNode Parent { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        public ObservableCollection<ICheckedNode> Items { get; set; } = new ObservableCollection<ICheckedNode>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Project"/> class.
        /// </summary>
        public Project()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Project"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public Project(ProjectInfo<ReferAssemblyInfo> data)
        {
            this.Path = data.Path;
            this.Name = data.Name;
            base.FileVersion = data.FileVersion;
            base.AssemblyVersion = data.AssemblyVersion;
            base.InformationalVersion = data.InformationalVersion;
            base.Items = data.Items;
        }

        #region Overrides of ProjectInfo

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            this.OnPropertyChanged("AssemblyVersion");
            this.OnPropertyChanged("FileVersion");
            this.OnPropertyChanged("InformationalVersion");

            this.UpdateCheckedAll();
        }

        /// <summary>
        /// Updates the checked all.
        /// </summary>
        private void UpdateCheckedAll()
        {
            if (this.Items != null && this.Items.Any())
            {
                var checkedChildCount = this.Items.Count(i => i.IsChecked == true);
                if (checkedChildCount == this.Items.Count)
                {
                    this.IsChecked = true;
                }
                else if (checkedChildCount == 0)
                {
                    this.IsChecked = false;
                }
                else
                {
                    this.IsChecked = null;
                }
            }
        }

        #endregion

        /// <summary>
        /// Updates the child.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        private void UpdateChild(bool? value)
        {
            if (value == null)
            {
                return;
            }

            if (this.Items != null && this.Items.Any())
            {
                foreach (var i in this.Items)
                {
                    i.IsChecked = value == true;
                }
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
