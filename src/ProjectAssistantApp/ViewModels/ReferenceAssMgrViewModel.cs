namespace ProjectAssistant.App.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using Caliburn.Micro;
    using Contract.Model;
    using Model;
    using Platform.Controller;
    using Platform.Helper;
    using Platform.Model;
    using PropertyChanged;
    using Validation;
    using ILog = log4net.ILog;

    /// <summary>
    /// Class ReferenceAssMgrViewModel.
    /// </summary>
    /// <seealso cref="Project" />
    [Export]
    [ImplementPropertyChanged]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ReferenceAssMgrViewModel : ContentBase<ReferenceAssMgrViewModel, Project>
    {
        #region Variables
        /// <summary>
        /// The logger
        /// </summary>
        private static readonly ILog Logger = log4net.LogManager.GetLogger(typeof(ReferenceAssMgrViewModel));

        #endregion

        #region Properties

        /// <summary>
        /// The project controller
        /// </summary>
        private readonly ReferenceAssemblyController referenceAssemblyController = IoC.Get<ReferenceAssemblyController>();

        /// <summary>
        /// Gets or sets the root dir.
        /// </summary>
        /// <value>The root dir.</value>
        //[StringRequireValidatorAttribute("Root directory should be not empty")]
        public string RootDir {
            get { return this.SearchSetting.RootDir; }
            set { this.SearchSetting.RootDir = value; }
        }

        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        /// <value>The filter.</value>
        //[StringRequireValidatorAttribute("Filter pattern should be not empty")]
        public string Filter
        {
            get { return this.SearchSetting.ProjectFilter; }
            set { this.SearchSetting.ProjectFilter = value; }
        }

        /// <summary>
        /// Gets or sets the reference ass.
        /// </summary>
        /// <value>The reference ass.</value>
        //[StringRequireValidatorAttribute("Assembly name should be not empty")]
        public string ReferenceAss
        {
            get { return this.SearchSetting.ReferenceAssemblyFilter; }
            set
            {
                this.SearchSetting.ReferenceAssemblyFilter = value;
            }
        }

        /// <summary>
        /// Gets or sets the ass version.
        /// </summary>
        /// <value>The ass version.</value>
        [StringRequireValidator("Version should be not empty")]
        [RegularValidator(@"^([\d]+[.][\d]+[.][\d]+([.][\d]+)*)$", "Version input incorrect format")]
        public string AssemblyVersion { get; set; }
       
        #endregion

        #region Overrides of ContentBase<Project>

        /// <summary>
        /// Scans this instance.
        /// </summary>
        /// <returns>IList<Project>.</Project></returns>
        public override IList<Project> HandleScanning()
        {
            Logger.Debug("HandleScanning...");
            try
            {
                this.SearchSetting.ReferenceAssemblyFilter = this.ReferenceAss;
                var referenceProject = this.referenceAssemblyController.GetItems(this.SearchSetting);
                Logger.Debug("HandleScanning...DONE");
                return this.BuildReferenceInfos(referenceProject);
            }
            catch (Exception ex)
            {
                Logger.Error($"HandleScanning - Exception: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Increases the version.
        /// </summary>
        /// <param name="selectedItems">The selected items.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public override bool HandleIncreaseVersion(IList<Project> selectedItems)
        {
            Logger.Debug("HandleIncreaseVersion...");
            if (string.IsNullOrEmpty(this.AssemblyVersion))
            {
                return false;
            }

            var selectingItem = selectedItems.Select(n => n as ProjectInfo<ReferAssemblyInfo>).ToList();
            var processingProject = new List<ProjectInfo<ReferAssemblyInfo>>();

            // Filter before execute increase
            foreach (var prjData in selectingItem)
            {
                var prjItem = selectedItems.FirstOrDefault(n => n.Name.Equals(prjData.Name)); // Binding model
                var checkingItems = prjItem?.Items.Where(n => n.IsChecked == true).ToList(); // Get checked child node
                var needExecutePrj = prjData.Items.Where(n => n.Name.Equals(checkingItems?.FirstOrDefault(i => i.Name.Equals(n.Name))?.Name)).ToList();// Data model
                var prjInfo = new ProjectInfo<ReferAssemblyInfo>()
                {
                    Name = prjData.Name,
                    Path = prjData.Path,
                    AssemblyVersion = prjData.AssemblyVersion,
                    FileVersion = prjData.FileVersion,
                    InformationalVersion = prjData.InformationalVersion,
                    Items = needExecutePrj
                };
                processingProject.Add(prjInfo);
            }

            // Do increase version for selected project
            var versionInfo = new VersionInfo(this.AssemblyVersion);
            var results = this.referenceAssemblyController.IncreaseVersion(processingProject, versionInfo);

            // Update result on GUI
            foreach (var resultItem in results)
            {
                if (!resultItem.HasError)
                {
                    var sourceBindingProjects = selectedItems.FirstOrDefault(itm => itm.Name.Equals(resultItem.SourceProjectName));
                    var sourceDataProjects = selectingItem.FirstOrDefault(itm => itm.Name.Equals(resultItem.SourceProjectName)); ;

                    // Update on GUI
                    var refPrjBindingItem = sourceBindingProjects?.Items?.FirstOrDefault(n => resultItem.Data.Name.Equals(n.Name));
                    var refItem = refPrjBindingItem as RefProject;
                    if (refItem != null)
                    {
                        refItem.RefVersion = this.AssemblyVersion;
                    }

                    // Update for data item
                    var refPrjDataItem = sourceDataProjects?.Items?.FirstOrDefault(n => resultItem.Data.Name.Equals(n.Name));
                    if (refPrjDataItem != null)
                    {
                        refPrjDataItem.RefVersion = this.AssemblyVersion;
                    }
                }
            }

            // Update version on Project data object


            Logger.Debug($"HandleIncreaseVersion...DONE - Update version success for [{results.Count(n => n.HasError == false)}] projects");
            return results.All(rs => rs.HasError);
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

            if (string.IsNullOrEmpty(text))
            {
                this.ClearDataSource();
                this.UpdateParentForRefProject(this.OriginItems);
                isChangedData = true;
                return this.OriginItems;
            }

            var projectDataList = new List<Project>();
            foreach (var project in this.OriginItems)
            {
                var matchingItems = project.Items.Where(i => i.Name.ToLower().Contains(text.ToLower())).ToList();
                if (!matchingItems.Any())
                {
                    continue;
                }

                var projectFilter = this.BuildProjectForFilter(project, matchingItems);
                projectDataList.Add(projectFilter);
            }
            
            // Should be update new parent node for each items
            isChangedData = this.IsChangeData(projectDataList);
            if (isChangedData) // Update new parent node
            {
                this.UpdateParentForRefProject(projectDataList);
            }

            Logger.Debug($"FilterItemsByText...Done - Item count = [{projectDataList.Count}]");

            return projectDataList;
        }

        /// <summary>
        /// Validates the input.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public override bool ValidateInput()
        {
            return string.IsNullOrEmpty(this.ReferenceAss);
        }

        /// <summary>
        /// Needs the reload data.
        /// </summary>
        /// <param name="filteringItems">The filtering items.</param>
        /// <returns><c>true</c> if [is change data] [the specified filtering items]; otherwise, <c>false</c>.</returns>
        protected override bool IsChangeData(IList<Project> filteringItems)
        {
            Logger.Debug("IsChangeData...");

            // Diff number of element
            if (filteringItems.Count != this.Items.Count)
            {
                Logger.Debug($"IsChangeData...DONE - Result=[{true}]");
                return true;
            }

            // Equal number element but diff inner data
            var listFilteringItemName = filteringItems.Select(n => n.Name).ToList();
            var listCurItemName = this.Items.Select(n => n.Name).ToList();
            if (VSProjectHelper.Equals(listCurItemName, listFilteringItemName))
            {
                // Verify child items
                for (var i = 0; i < this.Items.Count; i++)
                {
                    var filterItems = filteringItems[i].Items.Select(n => n.Name).ToList();
                    var sourceItems = this.Items[i].Items.Select(n => n.Name).ToList();
                    var compareResult = VSProjectHelper.Equals(sourceItems, filterItems);
                    if (!compareResult)
                    {
                        Logger.Debug($"IsChangeData...DONE - Result=[{true}]");
                        return true;
                    }
                }
            }
            else // Not equals
            {
                Logger.Debug($"IsChangeData...DONE - Result=[{true}]");
                return true;
            }
            Logger.Debug($"IsChangeData...DONE - Result=[{false}]");
            return false;
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Builds the reference infos.
        /// </summary>
        /// <param name="referAssemblyInfos">The refer assembly infos.</param>
        /// <returns>IList<Project/>.</returns>
        private List<Project> BuildReferenceInfos(IList<ProjectInfo<ReferAssemblyInfo>> referAssemblyInfos)
        {
            Logger.Debug("BuildReferenceInfos...");

            var results = new List<Project>();
            foreach (var assemblyInfo in referAssemblyInfos)
            {
                var parentProject = new Project(assemblyInfo);
                var lstProjects = assemblyInfo.Items.Select(i => this.BuildProjectItem(i, parentProject)).ToList();

                foreach (var project in lstProjects)
                {
                    parentProject.Items.Add(project);
                }
               
                results.Add(parentProject);
            }
            Logger.Debug($"BuildReferenceInfos...DONE - Number project build success = [{results.Count}]");
            return results;
        }

        /// <summary>
        /// Builds the project item.
        /// </summary>
        /// <param name="projectInfo">The project information.</param>
        /// <param name="parentNode">The parent node.</param>
        /// <returns>Project.</returns>
        private RefProject BuildProjectItem(ReferAssemblyInfo projectInfo, Project parentNode)
        {
            var refPrj = new RefProject(parentNode, projectInfo)
            {
                RefVersion = projectInfo.RefVersion
            };
            return refPrj;
        }

        /// <summary>
        /// Builds the project.
        /// </summary>
        /// <param name="sourceProject">The source project.</param>
        /// <param name="refProjects">The reference projects.</param>
        /// <returns>Project.</returns>
        private Project BuildProjectForFilter(Project sourceProject, List<ICheckedNode> refProjects)
        {
            // build data
            var prjData = sourceProject as ProjectInfo<ReferAssemblyInfo>;
           
            // build model binding
            var project = new Project(prjData);
            refProjects.ForEach(refPrj =>
            {
                refPrj.IsChecked = false;
            });

            foreach (var refProject in refProjects)
            {
                project.Items.Add(refProject);
            }

            return project;
        }

        /// <summary>
        /// Updates the parent for reference project.
        /// </summary>
        private void UpdateParentForRefProject(IList<Project> originItems)
        {
            // Re-Update parent for RefProject item
            foreach (var project in originItems)
            {
                foreach (var refItem in project.Items)
                {
                    var refProjectItem = refItem as RefProject;
                    if (refProjectItem != null)
                    {
                        refProjectItem.IsChecked = false;
                        refProjectItem.Parent = project;
                    }
                }
                project.IsChecked = false;
            }
        }

        #endregion
    }
}
