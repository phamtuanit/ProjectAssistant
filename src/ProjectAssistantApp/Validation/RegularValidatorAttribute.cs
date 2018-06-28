namespace ProjectAssistant.App.Validation
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Class RegularValidatorAttribute.
    /// </summary>
    /// <seealso cref="System.ComponentModel.DataAnnotations.RegularExpressionAttribute" />
    /// <seealso cref="IValidationControl" />
    class RegularValidatorAttribute : RegularExpressionAttribute, IValidationControl
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RegularValidatorAttribute"/> class.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        public RegularValidatorAttribute(string pattern) : base(pattern)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegularValidatorAttribute"/> class.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="errorMsg">The error MSG.</param>
        public RegularValidatorAttribute(string pattern, string errorMsg) : base(pattern)
        {
            this.ErrorMessage = errorMsg;
        }

        #endregion

        #region Implementation of IValidationControl

        /// <summary>
        /// When true a validation controller will
        /// </summary>
        /// <value><c>true</c> if [validate while disabled]; otherwise, <c>false</c>.</value>
        public bool ValidateWhileDisabled { get; set; }

        /// <summary>
        /// If not defined the guard property to check for disabled state is Can[PropertyName]
        /// However it may be necessary to test another guard property and this is the place
        /// to specify the alternative property to query.
        /// </summary>
        /// <value>The guard property.</value>
        public string GuardProperty { get; set; }

        /// <summary>
        /// The order of rule validation
        /// </summary>
        /// <value>The order.</value>
        public ushort Order { get; set; }

        /// <summary>
        /// The flag to check has continue to validate other rules when failed
        /// </summary>
        /// <value><c>true</c> if this instance is ignore when has error; otherwise, <c>false</c>.</value>
        public bool IsIgnoreWhenHasError { get; set; }

        #endregion
    }
}
