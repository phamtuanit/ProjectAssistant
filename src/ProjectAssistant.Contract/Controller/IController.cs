namespace ProjectAssistant.Contract.Controller
{
    using System.Collections.Generic;
    using Model;

    /// <summary>
    /// Interface IController
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IController<T>
    {
        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <param name="filterSetting">The filter setting.</param>
        /// <returns>IList&lt;T&gt;.</returns>
        IList<T> GetItems(FilterSetting filterSetting);

        /// <summary>
        /// Increases the version.
        /// </summary>
        /// <param name="selectedItems">The selected items.</param>
        /// <param name="versionInfo">The version information.</param>
        /// <returns>IList&lt;ResultInfo&lt;T&gt;&gt;.</returns>
        IList<ResultInfo<T>> IncreaseVersion(IList<T> selectedItems, VersionInfo versionInfo);
    }
}