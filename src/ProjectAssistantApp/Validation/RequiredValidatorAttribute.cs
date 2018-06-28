namespace ProjectAssistant.App.Validation
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// A Required validation attribute sub-classed to allow a validation controller constrain when and how testing is done
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredValidatorAttribute : RequiredAttribute, IValidationControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredValidatorAttribute" /> class.
        /// </summary>
        /// <param name="errorMessage">The error code.</param>
        /// <param name="args">The args.</param>
        public RequiredValidatorAttribute(string errorMessage, params object[] args)
        {
            this.ErrorMessage = errorMessage;
        }

        /// <summary>
        /// The order of sequence rules
        /// </summary>
        public ushort Order { get; set; }

        /// <summary>
        /// Check to need continue validation other rules when failed
        /// </summary>
        public bool IsIgnoreWhenHasError { get; set; }

        #region IValidationControl

        /// <summary>
        /// When true a validation controller will alway validate
        /// </summary>
        public bool ValidateWhileDisabled { get; set; }

        /// <summary>
        /// If not defined the guard property to check for disabled state is Can[PropertyName]
        /// However it may be necessary to test another guard property and this is the place 
        /// to specify the alternative property to query.
        /// </summary>
        public string GuardProperty { get; set; }

        #endregion
    }
}
