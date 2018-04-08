namespace ProjectAssistant.App.ViewModels
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reactive.Linq;
    using System.Reflection;
    using Caliburn.Micro;
    using Platform.Extension;
    using PropertyChanged;
    using Validation;
    using ILog = log4net.ILog;

    public class ValidatingScreen<TViewModel> : Screen, INotifyDataErrorInfo where TViewModel : ValidatingScreen<TViewModel>
    {
        #region Variables

        /// <summary>
        /// The error tracking
        /// </summary>
        public readonly ConcurrentDictionary<string, List<string>> errorDic;

        /// <summary>
        /// Gets or sets the validation context.
        /// </summary>
        /// <value>
        /// The validation context.
        /// </value>
        private ValidationContext @ValidationContext { get; }

        /// <summary>
        /// The validator set;
        /// </summary>
        protected readonly HashSet<string> validatorList = new HashSet<string>();

        /// <summary>
        /// The logger
        /// </summary>
        private static readonly ILog Logger = log4net.LogManager.GetLogger(typeof(ValidatingScreen<TViewModel>));

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatingScreen{TViewModel}"/> class.
        /// </summary>
        public ValidatingScreen()
        {
            this.ValidationContext = new ValidationContext(this, null, null);
            this.errorDic = new ConcurrentDictionary<string, List<string>>();
        }

        #endregion

        #region Override of Screen

        /// <summary>
        /// Called when [view loaded].
        /// </summary>
        /// <param name="view">The view.</param>
        protected override void OnViewLoaded(object view)
        {
            Logger.Debug("OnViewLoaded...");
            base.OnViewLoaded(view);

            Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                h => this.PropertyChanged += h,
                h => this.PropertyChanged -= h).Where(
                x =>
                {
                    if (PropertyGetters.ContainsKey(x.EventArgs.PropertyName))
                    {
                        this.validatorList.Add(x.EventArgs.PropertyName);
                        return true;
                    }

                    return false;
                }).Throttle(TimeSpan.FromSeconds(0.75)).Subscribe(x => this.ValidateLazy());
            Logger.Debug("OnViewLoaded...");
        }

        #endregion

        #region Implementation of INotifyDataErrorInfo

        /// <summary>
        /// Returns True if any of the property values generate a validation error
        /// </summary>
        [DoNotNotify]
        public virtual bool HasErrors => !this.errorDic.IsEmpty;

        /// <summary>
        /// Occurs when the validation errors have changed for a property or for the entire entity.
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        /// <summary>
        /// Get error message of property field
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public IEnumerable GetErrors(string propertyName)
        {
            if (!string.IsNullOrWhiteSpace(propertyName))
            {
                if (PropertyGetters.ContainsKey(propertyName))
                {
                    List<string> errors;
                    if (this.errorDic.TryGetValue(propertyName, out errors) && !errors.IsNullOrEmpty())
                    {
                        // Realize a list and send to the OnColumnrror() method
                        this.OnProperyErrors(propertyName, errors);

                        foreach (var error in errors)
                        {
                            yield return error;
                        }
                    }
                }
            }

            yield break;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Validates this instance.
        /// </summary>
        public virtual bool Validate()
        {
            var keys = Validators.Keys;
            foreach (var columnName in keys)
            {
                this.Validate(columnName);
            }

            return this.HasErrors;
        }

        /// <summary>
        /// Validates the lazy.
        /// </summary>
        public void ValidateLazy()
        {
            while (this.validatorList.Any())
            {
                var columnName = this.validatorList.FirstOrDefault();
                if (!string.IsNullOrEmpty(columnName))
                {
                    this.Validate(columnName);
                    this.validatorList.Remove(columnName);
                }
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// This protected method is called when WPF calls the Error method and give the class a chance to extend the list of reported errorDic
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="errors">The list of errors generated by validator</param>
        protected virtual void OnProperyErrors(string propertyName, IEnumerable<string> errors)
        {
            // Does nothing
        }

        /// <summary>
        /// Called when [property errors changed].
        /// </summary>
        /// <param name="property">The p.</param>
        public void OnPropertyErrorsChanged(string property)
        {
            this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(property));
        }

        /// <summary>
        /// Validate error at column name
        /// </summary>
        /// <param name="columnName"></param>
        protected void Validate(string columnName)
        {
            if (!Validators.ContainsKey(columnName))
            {
                return;
            }

            var value = PropertyGetters[columnName]((TViewModel)this);
            this.ValidateProperty(columnName, value);
        }

        /// <summary>
        /// Valid property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        protected bool ValidateProperty(string propertyName, object value)
        {
            if (!Validators.ContainsKey(propertyName))
            {
                return true;
            }

            bool isValid = true;
            var validators = Validators[propertyName];

            // This assumes a specific type of validator will be declared only once
            var validateErrors = new List<string>();
            foreach (var validator in validators)
            {
                try
                {
                   if (validator.GetValidationResult(value, this.ValidationContext) != ValidationResult.Success)
                    {
                        validateErrors.Add(validator.FormatErrorMessage(string.Empty));
                        var validationControl = validator as IValidationControl;
                        if (validationControl != null && !validationControl.IsIgnoreWhenHasError)
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.WarnFormat("Skip to validation because this rule[{0}] does not valid. Exception: {1}", validator, ex);
                }
            }

            isValid = validateErrors.IsNullOrEmpty();
            if (isValid)
            {
                this.RemoveError(propertyName);
            }
            else
            {
                this.RemoveError(propertyName);
                this.AddError(propertyName, validateErrors);
            }

            //this.ValidateResult = isValid;
            //this.ValidateSuccess.OnUIThreadAsync();
            return isValid;
        }

        /// <summary>
        /// Adds the error.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="error">The error.</param>
        public void AddError<TProperty>(Expression<Func<TProperty>> property, string error)
        {
            var propertyName = property.GetMemberInfo().Name;
            this.AddError(propertyName, error);
        }

        /// <summary>
        /// Adds the error.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="errors">The error.</param>
        public void AddError<TProperty>(Expression<Func<TProperty>> property, IList<string> errors)
        {
            var propertyName = property.GetMemberInfo().Name;
            this.AddError(propertyName, errors);
        }

        /// <summary>
        /// Adds the error.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="error">The error.</param>
        public void AddError(string propertyName, string error)
        {
            Logger.DebugFormat("Add error message [{0}] to property [{1}]...", error, propertyName);
            if (!PropertyGetters.ContainsKey(propertyName))
            {
                //Logger.DebugFormat($"[{propertyName}] is not existed.");
                return;
            }

            List<string> errors;
            if (!this.errorDic.TryGetValue(propertyName, out errors))
            {
                errors = new List<string>() { error };
                this.errorDic.TryAdd(propertyName, errors);
            }
            else
            {
                errors.Add(error);
            }

            // Notify error change
            this.OnNotifyErrorChanged(propertyName);
            Logger.Debug($"Add error for [{propertyName}] with [{errors}] successfully.");
        }

        /// <summary>
        /// Add error list to field
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="errors"></param>
        public void AddError(string propertyName, IList<string> errors)
        {
            Logger.DebugFormat("Add error messages to property [{0}]...", propertyName);

            if (!PropertyGetters.ContainsKey(propertyName))
            {
                //Logger.DebugFormat($"[{propertyName}] is not existed.");
                return;
            }

            List<string> errorsTmp;
            if (!this.errorDic.TryGetValue(propertyName, out errorsTmp))
            {
                errorsTmp = new List<string>(errors);
                this.errorDic.TryAdd(propertyName, errorsTmp);
            }
            else
            {
                errorsTmp.AddRange(errors);
            }

            // Notify error change
            this.OnNotifyErrorChanged(propertyName);
            Logger.Debug($"Add error for [{propertyName}] with [{errors}] successfully.");
        }

        /// <summary>
        /// Removes the error.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void RemoveError(string propertyName)
        {
            Logger.DebugFormat("Remove error for property [{0}]...", propertyName);

            List<string> errors;
            if (!this.errorDic.TryRemove(propertyName, out errors))
            {
                //Logger.Warn($"[{propertyName}] is not existed.");
                return;

            }

            this.OnNotifyErrorChanged(propertyName);
            Logger.DebugFormat("Remove error for property [{0}] successfully.", propertyName);
        }

        /// <summary>
        /// Raise error changed at property
        /// </summary>
        /// <param name="propertyName"></param>
        public void OnNotifyErrorChanged(string propertyName)
        {
            var handler = this.ErrorsChanged;
            handler?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Grab all the property getter MethodInfo instances which have validations declared and hold them in a static class
        /// </summary>
        protected static readonly Dictionary<string, Func<TViewModel, object>> PropertyGetters =
        (from p in typeof(TViewModel).GetProperties()
            where p.GetAttributes<ValidationAttribute>(true).OfType<IValidationControl>().ToArray().Length != 0
            select p).ToDictionary(p => p.Name, GetValueGetter);

        /// <summary>
        /// Create a dictionary of the validators (if any) associated with each class property
        /// </summary>
        protected static readonly Dictionary<string, ValidationAttribute[]> Validators =
        (from p in typeof(TViewModel).GetProperties()
            let attrs = p.GetAttributes<ValidationAttribute>(true)
                .OfType<IValidationControl>().OrderBy(x => x.Order)
                .OfType<ValidationAttribute>().ToArray()
            where attrs.Length != 0
            select new KeyValuePair<string, ValidationAttribute[]>(p.Name, attrs)).ToDictionary(
            p => p.Key, p => p.Value);

        /// <summary>
        /// Uses Linq to create a list of MethodInfo.GetGetProperty methods.
        /// </summary>
        /// <param name="property">The property for which to get the getter</param>
        /// <returns>The Getter</returns>
        protected static Func<TViewModel, object> GetValueGetter(PropertyInfo property)
        {
            var instance = Expression.Parameter(typeof(TViewModel), "i");
            var cast = Expression.TypeAs(Expression.Property(instance, property), typeof(object));
            return (Func<TViewModel, object>)Expression.Lambda(cast, instance).Compile();
        }

        #endregion
    }
}
