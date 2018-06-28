namespace ProjectAssistant.App.Validation
{
    public interface IValidationControl
    {
        /// <summary>
        /// When true a validation controller will 
        /// </summary>
        bool ValidateWhileDisabled { get; set; }

        /// <summary>
        /// If not defined the guard property to check for disabled state is Can[PropertyName]
        /// However it may be necessary to test another guard property and this is the place 
        /// to specify the alternative property to query.
        /// </summary>
        string GuardProperty { get; set; }

        /// <summary>
        /// The order of rule validation
        /// </summary>
        ushort Order { get; set; }

        /// <summary>
        /// The flag to check has continue to validate other rules when failed
        /// </summary>
        bool IsIgnoreWhenHasError { get; set; }
    }
}
