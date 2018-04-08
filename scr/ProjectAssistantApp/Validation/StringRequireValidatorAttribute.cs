namespace ProjectAssistant.App.Validation
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// String Require Validator Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = false)]
    public class StringRequireValidatorAttribute : RequiredAttribute, IValidationControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringRequireValidatorAttribute"/> class.
        /// </summary>
        /// <param name="errorMessages">
        /// The require error code.
        /// </param>
        public StringRequireValidatorAttribute(string errorMessages)
        {
            this.RequiredValidator = new RequiredValidatorAttribute(errorMessages, null);
        }

        /// <summary>
        /// The order of sequence rules
        /// </summary>
        public ushort Order { get; set; }

        /// <summary>
        /// Check to need continue validation other rules when failed
        /// </summary>
        public bool IsIgnoreWhenHasError { get; set; }

        /// <summary>
        /// The RequiredExAttribute.
        /// </summary>
        private RequiredValidatorAttribute RequiredValidator { get; set; }

        /// <summary>
        /// Check Valid Data
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// </returns>
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                this.ErrorMessage = this.RequiredValidator.ErrorMessage;
                return false;
            }

            var str = value as string;
            if (string.IsNullOrEmpty(str))
            {
                this.ErrorMessage = this.RequiredValidator.ErrorMessage;
                return false;
            }

            if (this.IsValidateWhiteSpace && string.IsNullOrWhiteSpace(value.ToString()))
            {
                this.ErrorMessage = this.RequiredValidator.ErrorMessage;
                return false;
            }

            return true;
        }

        #region Implementation of IValidationControl

        /// <summary>
        /// Gets or sets a value indicating whether ValidateWhileDisabled.
        /// </summary>
        public bool ValidateWhileDisabled { get; set; }

        /// <summary>
        /// Gets or sets GuardProperty.
        /// </summary>
        public string GuardProperty { get; set; }

        /// <summary>
        /// Support validate space
        /// </summary>
        public bool IsValidateWhiteSpace { get; set; } = false;

        #endregion
    }
}
