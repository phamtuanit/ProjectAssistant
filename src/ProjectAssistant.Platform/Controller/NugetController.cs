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
    /// Class NugetController.
    /// </summary>
    /// <seealso cref="IController{T}.Model.NuspecInfo}" />
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class NugetController : IController<NugetInfo<RefNugetInfo>>
    {
        /// <summary>
        /// The logger
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(NugetController));

        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <param name="filterSetting">The filter setting.</param>
        /// <returns></returns>
        public IList<NugetInfo<RefNugetInfo>> GetItems(FilterSetting filterSetting)
        {
            Logger.Info("GetItems <- Enter");
            try
            {
                var result = new List<NugetInfo<RefNugetInfo>>();
                if (Directory.Exists(filterSetting.RootDir))
                {
                    var folders = Directory.GetDirectories(filterSetting.RootDir, "*", SearchOption.AllDirectories);

                    foreach (var folder in folders)
                    {
                        var files = Directory.GetFiles(folder, filterSetting.NuspecFilter, SearchOption.AllDirectories);
                        if (files.Any())
                        {
                            result.AddRange(this.BuildNugetInfo(files));
                        }
                    }
                    return result;
                }
                Logger.Info("GetItems -> Leave");
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
        public IList<ResultInfo<NugetInfo<RefNugetInfo>>> IncreaseVersion(IList<NugetInfo<RefNugetInfo>> selectedItems, VersionInfo versionInfo)
        {
            Logger.Info("IncreaseVersion <- Enter");
            try
            {
                var result = new List<ResultInfo<NugetInfo<RefNugetInfo>>>();
                if (!selectedItems.Any())
                {
                    return result;
                }

                foreach (var nuspecInfo in selectedItems)
                {
                    var nuspecResult = new ResultInfo<NugetInfo<RefNugetInfo>>(nuspecInfo);
                    try
                    {
                        nuspecInfo.IncreaseNugetVersion(versionInfo.Version);
                        result.Add(nuspecResult);
                    }
                    catch (Exception ex)
                    {
                        nuspecResult.Error = ex.Message;
                    }

                }
                Logger.Info("IncreaseVersion -> Leave");
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error($"IncreaseVersion - Exception: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Builds the project information.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <returns></returns>
        private IList<NugetInfo<RefNugetInfo>> BuildNugetInfo(string[] files)
        {
            return files.Select(f => new NugetInfo<RefNugetInfo>(new FileInfo(f))).ToList();
        }
    }
}
