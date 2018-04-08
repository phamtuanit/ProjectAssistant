namespace ProjectAssistant.App.Interface
{
    using System;
    using Contract;

    public interface IHeaderBase
    {
        /// <summary>
        /// Occurs when [search text changed event].
        /// </summary>
        event EventHandler<string> SearchTextChangedEvent;

        /// <summary>
        /// Gets or sets the search setting.
        /// </summary>
        /// <value>
        /// The search setting.
        /// </value>
        FilterSetting SearchSetting { get; }

        /// <summary>
        /// Gets the filter text.
        /// </summary>
        /// <value>The filter text.</value>
        string FilterText { get; }
    }
}
