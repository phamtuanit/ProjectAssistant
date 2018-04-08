namespace ProjectAssistant.Platform.Controller
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Linq;
    using Helper;
    using Model;
    using ProjectAssistant.Contract;
    using ProjectAssistant.Contract.Controller;
    using ProjectAssistant.Contract.Model;
    using log4net;

    /// <summary>
    /// Class ReferenceAssemblyController.
    /// </summary>
    /// <seealso />
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ReferenceNugetController : IController<NugetInfo<RefNugetInfo>>
    {
        /// <summary>
        /// The logger
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ReferenceNugetController));

        /// <summary>
        /// The caching filter setting
        /// </summary>
        private FilterSetting cachingFilterSetting;

        #region Implementation of IController<ProjectInfo>

        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <param name="filterSetting">The filter setting.</param>
        /// <returns>IList<ProjectInfo/>.</returns>
        public IList<NugetInfo<RefNugetInfo>> GetItems(FilterSetting filterSetting)
        {
            Logger.Debug("GetItems <- Enter");

            this.cachingFilterSetting = filterSetting;
            var result = new List<NugetInfo<RefNugetInfo>>();
            if (string.IsNullOrEmpty(filterSetting.ReferenceNugetFilter))
            {
                return result;
            }

            var nugetNames = filterSetting.ReferenceNugetFilter.Split(',').Select(n => n.Trim()).ToList();

            nugetNames.ForEach(nugetName =>
            {
                var absoluteFilePath = Path.Combine(filterSetting.RootDir, nugetName);

                if (Directory.Exists(filterSetting.RootDir))
                {
                    var rootDir = new DirectoryInfo(filterSetting.RootDir);
                    try
                    {
                        var usedFiles = new List<FileInfo>();
                        var directories = rootDir.GetDirectories("*", SearchOption.AllDirectories);
                        foreach (var dir in directories)
                        {
                            FileInfo nugetConfig;
                            var rs = this.IsReferenceNuget(dir.FullName, nugetName, filterSetting.NugetConfigFilter, out nugetConfig);
                            if (rs)
                            {
                                usedFiles.Add(nugetConfig);
                            }
                        }

                        if (usedFiles.Any())
                        {
                            result.Add(this.BuildReferInfo(nugetName, absoluteFilePath, usedFiles));
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"GetItems - Exception: {ex.Message}", ex);
                        throw;
                    }
                }
            });

            Logger.Debug("GetItems -> Leave");
            return result;
        }

        /// <summary>
        /// Increases the version.
        /// </summary>
        /// <param name="selectedItems">The selected items.</param>
        /// <param name="versionInfo">The version information.</param>
        /// <returns>IList<ResultInfo /><ProjectInfo /><ReferAssemblyInfo />&gt;&gt;.</returns>
        public IList<ResultInfo<NugetInfo<RefNugetInfo>>> IncreaseVersion(IList<NugetInfo<RefNugetInfo>> selectedItems, VersionInfo versionInfo)
        {
            Logger.Debug("IncreaseVersion <- Enter");
            this.ChangeDependentNuget(selectedItems, versionInfo);
            return this.ChangeRefNuget(selectedItems, versionInfo);
        }

        /// <summary>
        /// Changes the dependent nuget.
        /// </summary>
        /// <param name="selectedItems">The selected items.</param>
        /// <param name="versionInfo">The version information.</param>
        /// <returns></returns>
        private IList<ResultInfo<NugetInfo<RefNugetInfo>>> ChangeDependentNuget(IList<NugetInfo<RefNugetInfo>> selectedItems, VersionInfo versionInfo)
        {
            Logger.Info($"Change dependent nuget. new version = {versionInfo.Version}");
            var resultInfo = new List<ResultInfo<NugetInfo<RefNugetInfo>>>();
            var rootPath = this.cachingFilterSetting.RootDir;

            if (!Directory.Exists(rootPath))
            {
                Logger.Error($"Path not found {rootPath}.");
            }

            foreach (var sourcePrj in selectedItems)
            {
                var refNugetName = sourcePrj.Name;
                var nuspecFiles = Directory.GetFiles(rootPath, "*.nuspec", SearchOption.AllDirectories);
                foreach (var nuspec in nuspecFiles)
                {
                    try
                    {
                        Logger.Debug($"Change dependent nuget. File = {nuspec}");
                        VSProjectHelper.ChangeDependentNugetVersion(refNugetName, nuspec, versionInfo);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn($"Cannot change dependent nuget for [{refNugetName}]. {ex.Message}");
                    }
                }
            }

            return resultInfo;
        }

        /// <summary>
        /// Changes the reference nuget.
        /// </summary>
        /// <param name="selectedItems">The selected items.</param>
        /// <param name="versionInfo">The version information.</param>
        /// <returns></returns>
        private IList<ResultInfo<NugetInfo<RefNugetInfo>>> ChangeRefNuget(IList<NugetInfo<RefNugetInfo>> selectedItems, VersionInfo versionInfo)
        {
            try
            {
                var resultInfo = new List<ResultInfo<NugetInfo<RefNugetInfo>>>();
                foreach (var item in selectedItems)
                {
                    var refNugetName = item.Name;
                    foreach (var childItem in item.Items)
                    {
                        var assemblyResult = new ResultInfo<NugetInfo<RefNugetInfo>>(childItem)
                        {
                            SourceProjectName = refNugetName
                        };

                        try
                        {
                            var changResult = VSProjectHelper.ChangeRefNugetVersion(childItem.Path, refNugetName, versionInfo.Version);
                            if (!changResult)
                            {
                                assemblyResult.Error = $"Can not change reference version for assembly {refNugetName} with version=[{versionInfo.Version}]";
                            }
                        }
                        catch (Exception ex)
                        {
                            assemblyResult.Error = ex.Message;
                        }

                        resultInfo.Add(assemblyResult);
                    }
                }

                Logger.Debug("IncreaseVersion -> Leave");
                return resultInfo;
            }
            catch (Exception ex)
            {
                Logger.Error($"IncreaseVersion - Exception: {ex.Message}", ex);
                throw;
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Builds the nuget information.
        /// </summary>
        /// <param name="assemblyName">Name of the file.</param>
        /// <param name="path">The path.</param>
        /// <param name="listItems">The list items.</param>
        /// <returns>IList<NuspecInfo></NuspecInfo>.</returns>
        private NugetInfo<RefNugetInfo> BuildReferInfo(string assemblyName, string path, List<FileInfo> listItems)
        {
            var referenceAssemblys = this.BuildItemInfo(listItems, assemblyName);
            var asemblyInfo = new NugetInfo<RefNugetInfo>
            {
                Name = assemblyName,
                Path = path,
                Items = referenceAssemblys
            };

            return asemblyInfo;
        }

        /// <summary>
        /// Determines whether [is reference project] [the specified file path].
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="nugetName">The search pattern.</param>
        /// <param name="configPattern">The configuration pattern.</param>
        /// <param name="nugetConfig">The nuget configuration.</param>
        /// <returns>
        ///   <c>true</c> if [is reference project] [the specified file path]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsReferenceNuget(string filePath, string nugetName, string configPattern, out FileInfo nugetConfig)
        {
            try
            {
                nugetConfig = null;
                Logger.Debug($"IsReferenceProject - Executing for : {filePath}");
                var files = Directory.GetFiles(filePath, configPattern, SearchOption.TopDirectoryOnly);

                if (files.Any())
                {
                    nugetConfig = new FileInfo(files[0]);
                    var refNugets = VSProjectHelper.GetRefNuget(files[0]);
                    return refNugets.Contains(nugetName);
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"GetItems - Exception: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Builds the item information.
        /// </summary>
        /// <param name="sourceItem">The source item.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns>IList<ItemInfo></ItemInfo>.</returns>
        public List<RefNugetInfo> BuildItemInfo(IList<FileInfo> sourceItem, string assemblyName)
        {
            var result = new List<RefNugetInfo>();
            if (sourceItem.Any())
            {
                var data = sourceItem.Select(i => this.BuildRefNugetInfo(i, assemblyName)).ToList();
                result.AddRange(data);
            }
            return result;
        }

        /// <summary>
        /// Builds the item information.
        /// </summary>
        /// <param name="sourceItem">The source item.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns>ItemInfo.</returns>
        private RefNugetInfo BuildRefNugetInfo(FileInfo sourceItem, string assemblyName)
        {
            var item = new RefNugetInfo(sourceItem)
            {
                RefVersion = VSProjectHelper.GetRefNugetVersion(sourceItem.FullName, assemblyName)
            };
            return item;
        }

        #endregion
    }
}
