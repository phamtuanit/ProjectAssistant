namespace ProjectAssistant.App.Model
{
    using System.Collections.ObjectModel;

    public class CheckedNodeBase : ICheckedNode
    {
        #region Implementation of ICheckedNode

        /// <summary>
        /// Gets or sets a value indicating whether this instance is checked.
        /// </summary>
        /// <value><c>null</c> if [is checked] contains no value, <c>true</c> if [is checked]; otherwise, <c>false</c>.</value>
        public bool? IsChecked { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public ICheckedNode Parent { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>The items.</value>
        public ObservableCollection<ICheckedNode> Items { get; set; }

        #endregion
    }
}
