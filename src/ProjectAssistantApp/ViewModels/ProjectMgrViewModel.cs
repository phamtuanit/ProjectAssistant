namespace ProjectAssistant.App.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Caliburn.Micro;
    using Contract.Model;
    using MaterialDesignThemes.Wpf;
    using Model;
    using Platform.Controller;
    using Platform.Model;
    using PropertyChanged;
    using Validation;
    using Views;
    using ILog = log4net.ILog;
    using LogManager = log4net.LogManager;

    /// <summary>
    /// Class ProjectMgrViewModel.
    /// </summary>
    [Export]
    [ImplementPropertyChanged]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ProjectMgrViewModel : ContentBase<ProjectMgrViewModel, Project>
    {
        #region Variables

        /// <summary>
        /// The project controller
        /// </summary>
        private readonly ProjectController projectController = IoC.Get<ProjectController>();

        /// <summary>
        /// The logger
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ProjectMgrViewModel));

        /// <summary>
        /// Gets or sets the root dir.
        /// </summary>
        /// <value>The root dir.</value>
        [StringRequireValidatorAttribute("Root directory should be not empty")]
        public string RootDir
        {
            get { return this.SearchSetting.RootDir; }
            set
            {
                this.SearchSetting.RootDir = value;
                if (string.IsNullOrWhiteSpace(this.SearchSetting.NugetOutput))
                {
                    this.NugetOutput = Path.Combine(value, "NugetOutput");
                }
            }
        }

        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        /// <value>
        /// The filter.
        /// </value>
        [StringRequireValidator("Filter pattern should be not empty")]
        public string Filter
        {
            get { return this.SearchSetting.ProjectFilter; }
            set { this.SearchSetting.ProjectFilter = value;}
        }

        /// <summary>
        /// The nuget package output
        /// </summary>
        public string NugetOutput
        {
            get { return this.SearchSetting.NugetOutput; }
            set { this.SearchSetting.NugetOutput = value; }
        }

        /// <summary>
        /// The nuget tool path
        /// </summary>
        public string NugetToolDir
        {
            get { return this.SearchSetting.NugetToolDir; }
            set { this.SearchSetting.NugetToolDir = value; }
        }

        /// <summary>
        /// Gets or sets the assembly version.
        /// </summary>
        /// <value>The assembly version.</value>
        [StringRequireValidator("Version should be not empty")]
        [RegularValidator(@"^([\d]+[.][\d]+[.][\d]+([.][\d]+)*)$", "Version input incorrect format")]
        public string AssemblyVersion
        {
            get { return this.SearchSetting.AssemblyVersion; }
            set { this.SearchSetting.AssemblyVersion = value; }
        }

        #endregion

        #region Override ContentBase<Project> methods

        /// <summary>
        /// Scans this instance.
        /// </summary>
        /// <returns>IList<Project/>.</returns>
        public override IList<Project> HandleScanning()
        {
            Logger.Debug("HandleScanning...");
            // Scanning project
            var result = this.projectController.GetItems(this.SearchSetting).Select(i => new Project(i)).ToList();
            Logger.Debug($"HandleScanning...DONE - Items scanned = [{result.Count}]");
            return result;
        }

        /// <summary>
        /// Increases the version.
        /// </summary>
        /// <param name="selectedItems">The selected items.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public override bool HandleIncreaseVersion(IList<Project> selectedItems)
        {
            Logger.Debug("HandleIncreaseVersion...");

            var ptInfoList = selectedItems.Select(pr => (ProjectInfo<ReferAssemblyInfo>)pr).ToList();
            var versionInfo = new VersionInfo(this.AssemblyVersion);
            var result = this.projectController.IncreaseVersion(ptInfoList, versionInfo);

            Logger.Debug($"HandleIncreaseVersion...DONE - Items increase success = [{result.Count(n => n.HasError == false)}]");
            return result.All(rs => rs.HasError);
        }

        /// <summary>
        /// Filters the items by text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="isChangedData">if set to <c>true</c> [is changed data].</param>
        /// <returns>ObservableCollection&lt;T&gt;.</returns>
        protected override IList<Project> FilterItemsByText(string text, out bool isChangedData)
        {
            Logger.Debug("FilterItemsByText...");
            isChangedData = true;

            if (string.IsNullOrEmpty(text))
            {
                Logger.Debug($"FilterItemsByText...DONE - Return Origin list items, count =[{this.OriginItems.Count}]");
                return this.OriginItems;
            }

            var results = this.OriginItems.Where(n => n.Name.ToLower().Contains(text.ToLower())).ToList();
            Logger.Debug($"FilterItemsByText...DONE - Items filtered = [{results.Count}]");
            return results;
        }

        /// <summary>
        /// Validates the input.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public override bool ValidateInput()
        {
            return false;
        }

        /// <summary>
        /// Build nuget package
        /// </summary>
        public async void BuildNuget()
        {
            if (string.IsNullOrWhiteSpace(this.NugetOutput))
            {
                await this.ShowErrorDialog("Nuget output directory must not be null.");
                return;
            }

            var selectedProject = this.GetSelectedItems().Select(i => (ProjectInfo<ReferAssemblyInfo>) i).ToList();
            if (!selectedProject.Any())
            {
                await this.ShowErrorDialog("Please select at least one project to build.");
                return;
            }

            if (!Directory.Exists(this.SearchSetting.NugetToolDir))
            {
                await this.ShowErrorDialog("Nuget tool dir could not be found.");
                return;
            }

            var busyView = new BusyIndicator();
            await DialogHost.Show(busyView,
                "RootDialog",
                (sender, args) =>
                {
                    Task.Run<IList<string>>(() => this.projectController.BuildNuget(this.SearchSetting, selectedProject))
                    .ContinueWith(tsk =>
                    {
                        Execute.OnUIThreadAsync(() =>
                        {
                            DialogHost.CloseDialogCommand.Execute(true, busyView);
                        });

                        var errors = tsk.Result;
                        if (errors.Any())
                        {
                            var errorStr = new StringBuilder();
                            foreach (var error in errors)
                            {
                                errorStr.AppendLine(error);
                            }

                            Execute.OnUIThreadAsync(() =>
                            {
                                this.ShowErrorDialog(errorStr.ToString());
                            });
                        }
                    });
                }, null);
        }

        #endregion
    }
}
