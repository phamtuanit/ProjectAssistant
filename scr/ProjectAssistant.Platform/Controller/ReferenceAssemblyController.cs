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
    public class ReferenceAssemblyController : IController<ProjectInfo<ReferAssemblyInfo>>
    {
        /// <summary>
        /// The logger
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ReferenceAssemblyController));

        #region Implementation of IController<ProjectInfo>

        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <param name="filterSetting">The filter setting.</param>
        /// <returns>IList<ProjectInfo/>.</returns>
        public IList<ProjectInfo<ReferAssemblyInfo>> GetItems(FilterSetting filterSetting)
        {
            Logger.Debug("GetItems <- Enter");

            var result = new List<ProjectInfo<ReferAssemblyInfo>>();
            if (string.IsNullOrEmpty(filterSetting.ReferenceAssemblyFilter))
            {
                return result;
            }

            var assemblyNames = filterSetting.ReferenceAssemblyFilter.Split(',').Select(n => n.Trim()).ToList();

            assemblyNames.ForEach(assemblyName =>
            {
                var absoluteFilePath = Path.Combine(filterSetting.RootDir, assemblyName);

                if (Directory.Exists(filterSetting.RootDir))
                {
                    var rootDir = new DirectoryInfo(filterSetting.RootDir);
                    try
                    {
                        var usedFiles = new List<FileInfo>();
                        var files = rootDir.GetFiles("*.csproj", SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            var rs = this.IsReferenceProject(file.FullName, assemblyName);
                            if (rs)
                            {
                                usedFiles.Add(file);
                            }
                        }

                        if (usedFiles.Any())
                        {
                            result.Add(this.BuildReferAssemblyInfo(assemblyName, absoluteFilePath, usedFiles));
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
        public IList<ResultInfo<ProjectInfo<ReferAssemblyInfo>>> IncreaseVersion(IList<ProjectInfo<ReferAssemblyInfo>> selectedItems, VersionInfo versionInfo)
        {
            Logger.Debug("IncreaseVersion <- Enter");
            try
            {
                var resultInfo = new List<ResultInfo<ProjectInfo<ReferAssemblyInfo>>>();
                foreach (var item in selectedItems)
                {
                    var referAssemblyName = item.Name;
                    foreach (var childItem in item.Items)
                    {
                        var assemblyResult = new ResultInfo<ProjectInfo<ReferAssemblyInfo>>(childItem)
                        {
                            SourceProjectName = referAssemblyName
                        };

                        try
                        {
                            var changResult = VSProjectHelper.ChangeReferenceVersion(childItem.Path, referAssemblyName, versionInfo.Version);
                            if (!changResult)
                            {
                                assemblyResult.Error = $"Can not change reference version for assembly {referAssemblyName} with version=[{versionInfo.Version}]";
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
        private ProjectInfo<ReferAssemblyInfo> BuildReferAssemblyInfo(string assemblyName, string path, List<FileInfo> listItems)
        {
            var referenceAssemblys = this.BuildItemInfo(listItems, assemblyName);
            var asemblyInfo = new ProjectInfo<ReferAssemblyInfo>
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
        /// <param name="assemblyName">The search pattern.</param>
        /// <returns><c>true</c> if [is reference project] [the specified file path]; otherwise, <c>false</c>.</returns>
        private bool IsReferenceProject(string filePath, string assemblyName)
        {
            try
            {
                Logger.Debug($"IsReferenceProject - Executing for : {filePath}");
                var result = !string.IsNullOrEmpty(VSProjectHelper.GetReferenceVersion(filePath, assemblyName));
                return result;
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
        public List<ReferAssemblyInfo> BuildItemInfo(IList<FileInfo> sourceItem, string assemblyName)
        {
            var result = new List<ReferAssemblyInfo>();
            if (sourceItem.Any())
            {
                var data = sourceItem.Select(i => this.BuildReferAssemblyInfo(i, assemblyName)).ToList();
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
        private ReferAssemblyInfo BuildReferAssemblyInfo(FileInfo sourceItem, string assemblyName)
        {
            var item = new ReferAssemblyInfo(sourceItem)
            {
                RefVersion = VSProjectHelper.GetReferenceVersion(sourceItem.FullName, assemblyName)
            };
            return item;
        }

        #endregion
    }
}
