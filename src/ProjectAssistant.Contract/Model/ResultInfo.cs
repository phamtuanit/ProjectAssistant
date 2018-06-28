namespace ProjectAssistant.Contract.Model
{
    /// <summary>
    /// Class ResultInfo.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResultInfo<T>
    {
        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <value>The data.</value>
        public T Data { get;}

        /// <summary>
        /// Gets a value indicating whether this instance has error.
        /// </summary>
        /// <value><c>true</c> if this instance has error; otherwise, <c>false</c>.</value>
        public bool HasError => !string.IsNullOrWhiteSpace(this.Error);

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>The error.</value>
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets the name of the source project.
        /// </summary>
        /// <value>The name of the source project.</value>
        public string SourceProjectName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultInfo{T}"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public ResultInfo(T data)
        {
            this.Data = data;
        }
    }
}
