namespace ProjectAssistant.App.ViewModels
{
    using System.ComponentModel.Composition;
    using Caliburn.Micro;
    using Interface;
    using PropertyChanged;

    [Export]
    [Export(typeof(IContentManagement))]
    [ImplementPropertyChanged]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class BodyViewModel : Conductor<Screen> , IContentManagement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BodyViewModel"/> class.
        /// </summary>
        public BodyViewModel()
        {
        }

        /// <summary>
        /// Shows the content.
        /// </summary>
        /// <param name="content">The content.</param>
        public void ShowContent(Screen content)
        {
            if ( ((Screen) this.ActiveItem) != content )
            {
                this.ActivateItem(content);
            }
        }
    }
}
