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
    public class ReferenceNugetMgrViewModel : ContentBase<ReferenceNugetMgrViewModel, Nuget>
    {
        #region Variables
        /// <summary>
        /// The logger
        /// </summary>
        private static readonly ILog Logger = log4net.LogManager.GetLogger(typeof(ReferenceNugetMgrViewModel));

        #endregion

        #region Properties

        /// <summary>
        /// The project controller
        /// </summary>
        private readonly ReferenceNugetController referenceNugetController = IoC.Get<ReferenceNugetController>();

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
            get { return this.SearchSetting.NugetConfigFilter; }
            set { this.SearchSetting.NugetConfigFilter = value; }
        }

        /// <summary>
        /// Gets or sets the reference ass.
        /// </summary>
        /// <value>The reference ass.</value>
        //[StringRequireValidatorAttribute("Assembly name should be not empty")]
        public string ReferenceNuget
        {
            get { return this.SearchSetting.ReferenceNugetFilter; }
            set
            {
                this.SearchSetting.ReferenceNugetFilter = value;
            }
        }

        /// <summary>
        /// Gets or sets the ass version.
        /// </summary>
        /// <value>The ass version.</value>
        [StringRequireValidator("Version should be not empty")]
        [RegularValidator(@"^([\d]+[.][\d]+[.][\d]+([.][\d]+)*)$", "Version input incorrect format")]
        public string NugetVersion { get; set; }
       
        #endregion

        #region Overrides of ContentBase<Project>

        /// <summary>
        /// Scans this instance.
        /// </summary>
        /// <returns>IList<Project>.</Project></returns>
        public override IList<Nuget> HandleScanning()
        {
            Logger.Debug("HandleScanning...");
            try
            {
                this.SearchSetting.ReferenceAssemblyFilter = this.ReferenceNuget;
                var referenceProject = this.referenceNugetController.GetItems(this.SearchSetting);
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
        public override bool HandleIncreaseVersion(IList<Nuget> selectedItems)
        {
            Logger.Debug("HandleIncreaseVersion...");
            if (string.IsNullOrEmpty(this.NugetVersion))
            {
                return false;
            }
            // Build data to increase version
            var selectingItem = selectedItems.Select(n => n as NugetInfo<RefNugetInfo>).ToList();
            var processingProjects = new List<NugetInfo<RefNugetInfo>>();

            // Filter before execute increase
            foreach (var prjData in selectingItem)
            {
                var prjItem = selectedItems.FirstOrDefault(n => n.Name.Equals(prjData.Name)); // Binding model
                var checkingItems = prjItem?.Items.Where(n => n.IsChecked == true).ToList(); // Get checked child node
                // Data model
                var needExecutePrj = prjData.Items.Where(n => n.Name.Equals(checkingItems?.FirstOrDefault(i => i.Name.Equals(n.Name))?.Name)).ToList();
                
                // Create new project data and list project items
                var nugetPrjData = new NugetInfo<RefNugetInfo>()
                {
                    Name = prjData.Name,
                    Path = prjData.Path,
                    NugetVersion = prjData.NugetVersion,
                    Items = needExecutePrj,
                };
                processingProjects.Add(nugetPrjData);
            }

            var versionInfo = new VersionInfo(this.NugetVersion);
            var results = this.referenceNugetController.IncreaseVersion(processingProjects, versionInfo);

            // Update result on GUI
            foreach (var resultItem in results)
            {
                if (resultItem.HasError)
                {
                    continue;
                }

                // Get source items
                var sourceBindingProjects = selectedItems.FirstOrDefault(itm => itm.Name.Equals(resultItem.SourceProjectName));
                var sourceDataProjects = selectingItem.FirstOrDefault(itm => itm.Name.Equals(resultItem.SourceProjectName)); ;

                // Update on GUI
                var refPrjBindingItem = sourceBindingProjects?.Items?.FirstOrDefault(n => resultItem.Data.Name.Equals(n.Name));
                var refItem = refPrjBindingItem as RefNuget;
                if (refItem != null)
                {
                    refItem.RefVersion = this.NugetVersion;
                }

                // Update for data item
                var refPrjDataItem = sourceDataProjects?.Items?.FirstOrDefault(n => resultItem.Data.Name.Equals(n.Name));
                if (refPrjDataItem != null)
                {
                    refPrjDataItem.RefVersion = this.NugetVersion;
                }
            }
            Logger.Debug($"HandleIncreaseVersion...DONE - Update version success for [{results.Count(n => n.HasError == false)}] projects");
            return results.All(rs => rs.HasError);
        }

        /// <summary>
        /// Filters the items by text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="isChangedData">if set to <c>true</c> [is changed data].</param>
        /// <returns>ObservableCollection&lt;T&gt;.</returns>
        protected override IList<Nuget> FilterItemsByText(string text, out bool isChangedData)
        {
            Logger.Debug("FilterItemsByText...");

            if (string.IsNullOrEmpty(text))
            {
                isChangedData = true;
                this.ClearDataSource();
                this.UpdateParentForRefProject(this.OriginItems);
                return this.OriginItems;
            }

            var projectDataList = new List<Nuget>();

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
                Logger.Debug("FilterItemsByText - Update new parent for each child node");
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
            return string.IsNullOrEmpty(this.ReferenceNuget);
        }

        /// <summary>
        /// Needs the reload data.
        /// </summary>
        /// <param name="filteringItems">The filtering items.</param>
        /// <returns><c>true</c> if [is change data] [the specified filtering items]; otherwise, <c>false</c>.</returns>
        protected override bool IsChangeData(IList<Nuget> filteringItems)
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
        private List<Nuget> BuildReferenceInfos(IList<NugetInfo<RefNugetInfo>> referAssemblyInfos)
        {
            Logger.Debug("BuildReferenceInfos...");

            var results = new List<Nuget>();
            foreach (var assemblyInfo in referAssemblyInfos)
            {
                var parentProject = new Nuget(assemblyInfo);
                var lstProjects = assemblyInfo.Items.Select(i => this.BuildNugetItem(i, parentProject)).ToList();

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
        private RefNuget BuildNugetItem(RefNugetInfo projectInfo, Nuget parentNode)
        {
            var refPrj = new RefNuget(parentNode, projectInfo)
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
        private Nuget BuildProjectForFilter(Nuget sourceProject, List<ICheckedNode> refProjects)
        {
            // build data
            var prjData = sourceProject as NugetInfo<RefNugetInfo>;
           
            // build model binding
            var project = new Nuget(prjData);
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
        private void UpdateParentForRefProject(IList<Nuget> originItems)
        {
            // Re-Update parent for RefProject item
            foreach (var project in originItems)
            {
                foreach (var refItem in project.Items)
                {
                    var nugetRefItem = refItem as RefNuget;
                    if (nugetRefItem != null)
                    {
                        refItem.IsChecked = false;
                        nugetRefItem.Parent = project;
                    }
                }
                project.IsChecked = false;
            }
        }

        #endregion
    }
}
