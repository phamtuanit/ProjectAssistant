namespace ProjectAssistant.App.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Linq;
    using Caliburn.Micro;
    using Model;
    using ProjectAssistant.Contract.Model;
    using ProjectAssistant.Platform.Controller;
    using ProjectAssistant.Platform.Model;
    using PropertyChanged;
    using Validation;
    using ILog = log4net.ILog;

    /// <summary>
    /// Class NugetMgrViewModel.
    /// </summary>
    [Export]
    [ImplementPropertyChanged]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class NugetMgrViewModel : ContentBase<NugetMgrViewModel, Nuget>
    {
        #region Variables

        /// <summary>
        /// The project controller
        /// </summary>
        private readonly NugetController nugetController = IoC.Get<NugetController>();

        /// <summary>
        /// The logger
        /// </summary>
        private static readonly ILog Logger = log4net.LogManager.GetLogger(typeof(NugetMgrViewModel));

        /// <summary>
        /// Gets or sets the root dir.
        /// </summary>
        /// <value>The root dir.</value>
        [StringRequireValidator("Root directory should be not empty")]
        public string RootDir
        {
            get { return this.SearchSetting.RootDir; }
            set
            {
                this.SearchSetting.RootDir = value;
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
            get { return this.SearchSetting.NuspecFilter; }
            set { this.SearchSetting.NuspecFilter = value; }
        }

        /// <summary>
        /// Gets or sets the assembly version.
        /// </summary>
        /// <value>The assembly version.</value>
        [StringRequireValidator("Version should be not empty")]
        [RegularValidator(@"^([\d]+[.][\d]+[.][\d]+([.][\d]+)*)$", "Version input incorrect format")]
        public string NugetVersion
        {
            get { return this.SearchSetting.NugetVersion; }
            set { this.SearchSetting.NugetVersion = value; }
        }

        #endregion

        #region Overrides of ContentBase<Nuget>

        /// <summary>
        /// Scans this instance.
        /// </summary>
        public override IList<Nuget> HandleScanning()
        {
            Logger.Debug("HandleScanning...");
            // Scanning nuget
            var results = this.nugetController.GetItems(this.SearchSetting).Select(i => new Nuget(i)).ToList();

            Logger.Debug($"HandleScanning...DONE - Items count = [{results.Count}]");
            return results;
        }

        /// <summary>
        /// Increases the version.
        /// </summary>
        public override bool HandleIncreaseVersion(IList<Nuget> selectedItems)
        {
            Logger.Debug("HandleIncreaseVersion...");
            try
            {
                var nuspecInfos = selectedItems.Select(n => (NugetInfo<RefNugetInfo>)n).ToList();
                var versionInfo = new VersionInfo(this.NugetVersion);
                var increaseResult = this.nugetController.IncreaseVersion(nuspecInfos, versionInfo);
                foreach (var result in increaseResult)
                {
                    var nugetSpec = selectedItems.FirstOrDefault(n => n.Name.Equals(result.Data.Name));
                    nugetSpec?.Refresh();
                }
                Logger.Debug($"HandleIncreaseVersion...DONE - Items increase success = [{increaseResult.Count(n => n.HasError == false)}]");
                return increaseResult.All(rl => rl.HasError);
            }
            catch (Exception ex)
            {
                Logger.Error($"HandleIncreaseVersion - Exception: {ex.Message}", ex);
                throw;
            }
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

        #endregion
    }
}
