namespace ProjectAssistant.App.Model
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// Interface ICheckedNode
    /// </summary>
    public interface ICheckedNode
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is checked.
        /// </summary>
        /// <value><c>null</c> if [is checked] contains no value, <c>true</c> if [is checked]; otherwise, <c>false</c>.</value>
        bool? IsChecked { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        ICheckedNode Parent { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>The items.</value>
        ObservableCollection<ICheckedNode> Items { get; set; }
    }
}
