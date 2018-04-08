namespace ProjectAssistant.App.ViewModels
{
    using System.ComponentModel.Composition;
    using Caliburn.Micro;
    using PropertyChanged;
    using ILog = log4net.ILog;

    [Export]
    [ImplementPropertyChanged]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ShellViewModel : Conductor<Screen>
    {
        /// <summary>
        /// The logger
        /// </summary>
        private static readonly ILog Logger = log4net.LogManager.GetLogger(typeof(ShellViewModel));

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string FooterLog { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellViewModel"/> class.
        /// </summary>
        public ShellViewModel()
        {
        }

        #region Overrides of ViewAware

        /// <summary>
        /// Called when an attached view's Loaded event fires.
        /// </summary>
        /// <param name="view"/>
        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            this.ActiveMainPage();
        }

        #endregion

        /// <summary>
        /// Actives the main page.
        /// </summary>
        private void ActiveMainPage()
        {
            Logger.Debug("ActiveMainPage...");
            var mainPage = IoC.Get<MainPageViewModel>();
            this.ActivateItem(mainPage);
            Logger.Debug("ActiveMainPage...Done");
        }
    }
}