namespace ProjectAssistant.Platform.Controller
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Constant;
    using Helper;
    using Model;
    using ProjectAssistant.Contract;
    using ProjectAssistant.Contract.Controller;
    using ProjectAssistant.Contract.Model;
    using ILog = log4net.ILog;
    using log4net;

    /// <summary>
    /// Class ProjectController.
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ProjectController : IController<ProjectInfo<ReferAssemblyInfo>>
    {
        /// <summary>
        /// The logger
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ProjectController));

        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <param name="filterSetting">The filter setting.</param>
        /// <returns></returns>
        public IList<ProjectInfo<ReferAssemblyInfo>> GetItems(FilterSetting filterSetting)
        {
            Logger.Debug("GetItems <- Enter");
            try
            {
                var folders = Directory.GetDirectories(filterSetting.RootDir, "*", SearchOption.AllDirectories);
                var result = new List<ProjectInfo<ReferAssemblyInfo>>();

                foreach (var folder in folders)
                {
                    var files = Directory.GetFiles(folder, filterSetting.ProjectFilter, SearchOption.TopDirectoryOnly);
                    if (files.Any())
                    {
                        result.AddRange(this.BuildProjectInfo(files));
                    }
                }

                Logger.Debug("GetItems -> Leave");
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error($"GetItems - Exception: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Increases the version.
        /// </summary>
        /// <param name="selectedItems">The selected items.</param>
        /// <param name="versionInfo">The version information.</param>
        /// <returns>IList&lt;ResultInfo&lt;T&gt;&gt;.</returns>
        public IList<ResultInfo<ProjectInfo<ReferAssemblyInfo>>> IncreaseVersion(IList<ProjectInfo<ReferAssemblyInfo>> selectedItems, VersionInfo versionInfo)
        {
            try
            {
                Logger.Debug("IncreaseVersion <- Enter");
                var result = new List<ResultInfo<ProjectInfo<ReferAssemblyInfo>>>();
                foreach (var pr in selectedItems)
                {
                    var prResult = new ResultInfo<ProjectInfo<ReferAssemblyInfo>>(pr);
                    try
                    {
                        UpdateAssemblyVersion(pr, versionInfo);
                        pr.GetAssemblyInfo();
                        pr.Refresh();
                    }
                    catch (Exception ex)
                    {
                        prResult.Error = ex.Message;
                    }
                    result.Add(prResult);
                }
                Logger.Debug("IncreaseVersion -> Leave");
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error($"GetItems - Exception: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Builds the project information.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <returns></returns>
        private IList<ProjectInfo<ReferAssemblyInfo>> BuildProjectInfo(string[] files)
        {
            var result = new List<ProjectInfo<ReferAssemblyInfo>>();
            foreach (var f in files)
            {
                var pr = new ProjectInfo<ReferAssemblyInfo>(new FileInfo(f));
                pr.GetAssemblyInfo();
                result.Add(pr);
            }
            return result;
        }

        /// <summary>
        /// Updates the assembly version.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="versionInfo">The version information.</param>
        /// <exception cref="Exception">UpdateAssemblyVersion - No assembly file.</exception>
        private static void UpdateAssemblyVersion(ProjectInfo<ReferAssemblyInfo> project, VersionInfo versionInfo)
        {
            Logger.Debug("UpdateAssemblyVersion <- Enter");
            try
            {
                // Get AssemblyInfo.cs file
                if (!File.Exists(project.Path))
                {
                    return;
                }

                var dirName = Path.Combine(Path.GetDirectoryName(project.Path), FileVersionInfoConst.PropertiesFolderName);
                var files = Directory.GetFiles(dirName, "AssemblyInfo.cs", SearchOption.TopDirectoryOnly);
                if (files.Any())
                {
                    foreach (var file in files)
                    {
                        //var newVersion = VersionHelper.GetIncreasionVersion(project.AssemblyVersion);
                        if (!string.IsNullOrEmpty(versionInfo.Version))
                        {
                            UpVersion(file, versionInfo.Version);
                        }
                    }
                }
                else
                {
                    throw new Exception("UpdateAssemblyVersion - No assembly file.");
                }
                Logger.Debug("UpdateAssemblyVersion -> Leave");
            }
            catch (Exception ex)
            {
                Logger.Error($"UpdateAssemblyVersion - Exception: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Build nugget package
        /// </summary>
        /// <param name="searchSetting"></param>
        /// <param name="selectedProject"></param>
        public IList<string> BuildNuget(FilterSetting searchSetting, List<ProjectInfo<ReferAssemblyInfo>> selectedProject)
        {
            var errors = new List<string>();
            foreach (var project in selectedProject)
            {
                try
                {
                    MSBuildHelper.BuildNuget(searchSetting.NugetToolDir, searchSetting.NugetOutput, project);
                }
                catch (Exception ex)
                {
                    Logger.Error("[BuildNuget] Got an error when build nuget.", ex);
                    errors.Add($"{project.Name}: {ex.Message}");
                }
            }
            return errors;
        }

        /// <summary>
        /// Updates the version.
        /// </summary>
        /// <param name="filePath">Content of the file.</param>
        /// <param name="newVersion">The new version.</param>
        private static void UpVersion(string filePath, string newVersion)
        {
            Logger.Debug("UpVersion <- Enter");

            try
            {
                var allLines = File.ReadAllLines(filePath);
                var listIndex = new List<int>();
                var index = 0;
                foreach (var line in allLines)
                {
                    if (line.StartsWith("//"))
                    {
                        index++;
                        continue;
                    }

                    if (line.Contains(FileVersionInfoConst.AssemblyInformationalVersionConst)
                        || line.Contains(FileVersionInfoConst.AssemblyVersionConst)
                        || line.Contains(FileVersionInfoConst.AssemblyFileVersionConst)
                    )
                    {
                        listIndex.Add(index);
                    }
                    index++;
                }

                foreach (var curIndex in listIndex)
                {
                    var lineStr = allLines[curIndex];
                    allLines[curIndex] = ChangeVersionInLine(lineStr, newVersion);
                }

                File.WriteAllLines(filePath, allLines, Encoding.Unicode);
            }
            catch (Exception ex)
            {
                Logger.Error($"UpVersion - Exception: {ex.Message}", ex);
                throw;
            }

            Logger.Debug("UpVersion -> Leave");
        }

        /// <summary>
        /// Changes the version in line.
        /// </summary>
        /// <param name="lineStr">The value.</param>
        /// <param name="newVersion">The new version.</param>
        /// <returns>System.String.</returns>
        private static string ChangeVersionInLine(string lineStr, string newVersion)
        {
            var startIndex = lineStr.IndexOf("\"", StringComparison.Ordinal);
            var endIndex = lineStr.IndexOf("\"", startIndex + 1, StringComparison.Ordinal);
            var oldVersion = lineStr.Substring(startIndex + 1, endIndex - startIndex - 1);
            return lineStr.Replace(oldVersion, newVersion);
        }
    }
}
