namespace ProjectAssistant.Contract.Model
{
    public class ItemInfo
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>The path.</value>
        public string Path { get; set; }

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        public virtual void Refresh() { }
    }
}
