namespace ProjectAssistant.App.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Caliburn.Micro;
    using Contract;
    using Interface;
    using MaterialDesignThemes.Wpf;
    using Model;
    using Platform.Helper;
    using PropertyChanged;
    using Views;

    /// <summary>
    /// Class ContentBase.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the t view model.</typeparam>
    /// <typeparam name="TDataContext">The type of the t data context.</typeparam>
    /// <seealso cref="ValidatingScreen{TViewModel}" />
    /// <seealso cref="Caliburn.Micro.Screen" />
    [ImplementPropertyChanged]
    public abstract class ContentBase<TViewModel, TDataContext> : ValidatingScreen<TViewModel> 
        where TDataContext : ICheckedNode 
        where TViewModel : ValidatingScreen<TViewModel>
    {
        /// <summary>
        /// The is selected all
        /// </summary>
        private bool? isSelectedAll;

        /// <summary>
        /// The header base
        /// </summary>
        private readonly IHeaderBase headerBase = IoC.Get<IHeaderBase>();

        /// <summary>
        /// Gets or sets the origin items.
        /// </summary>
        /// <value>The origin items.</value>
        public IList<TDataContext> OriginItems { get; set; }

        /// <summary>
        /// Gets or sets the search setting.
        /// </summary>
        /// <value>
        /// The search setting.
        /// </value>
        public FilterSetting SearchSetting { get; private set; }

        /// <summary>
        /// Gets or sets the folders.
        /// </summary>
        /// <value>The folders.</value>
        public ObservableCollection<TDataContext> Items { get; set; } = new ObservableCollection<TDataContext>();

        /// <summary>
        /// Sets a value indicating whether this instance is all items selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is all items selected; otherwise, <c>false</c>.
        /// </value>
        public bool? IsAllItemsSelected
        {
            set
            {
                this.isSelectedAll = value;
                this.CheckedAll(value);
            }

            get { return this.isSelectedAll; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentBase{T}"/> class.
        /// </summary>
        protected ContentBase()
        {
            this.IsAllItemsSelected = false;
            this.SearchSetting = this.headerBase.SearchSetting;
            this.headerBase.SearchTextChangedEvent += this.OnSearchTextChangedEvent;
            this.OriginItems = new List<TDataContext>();
        }

        /// <summary>
        /// Called when deactivating.
        /// </summary>
        /// <param name="close">Indicates whether this instance will be closed.</param>
        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            this.headerBase.SearchTextChangedEvent -= this.OnSearchTextChangedEvent;
        }

        /// <summary>
        /// Called when [search text changed event].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="text">The text.</param>
        private void OnSearchTextChangedEvent(object sender, string text)
        {
            this.HandleSearchChanged(text);
        }

        /// <summary>
        /// Handles the search changed.
        /// </summary>
        /// <param name="text">The text.</param>
        protected virtual void HandleSearchChanged(string text)
        {
            bool needReloadData;
            var filteredItems = this.FilterItemsByText(text, out needReloadData);

            // Determine should be reload data on GUI or NOT
            if (needReloadData)
            {
                Execute.OnUIThreadAsync(() =>
                {
                    this.ClearDataSource();
                    foreach (var item in filteredItems)
                    {
                        this.AddItem(item);
                    }
                });
            }
        }

        /// <summary>
        /// Scans this instance.
        /// </summary>
        public async void Scan()
        {
            if (string.IsNullOrEmpty(this.headerBase.SearchSetting.RootDir) || this.ValidateInput())
            {
                await this.ShowDialog("RootDialog", "Please input correspond search criteria(s)");
                return;
            }

            Exception exception = null;

            // Show BusyIndicator
            var busyView = new BusyIndicator();
            await DialogHost.Show(busyView, "RootDialog", (sender, args) =>
            {
                Task.Run<IList<TDataContext>>(() =>
                {
                    try
                    {
                        if (!Directory.Exists(this.SearchSetting.RootDir))
                        {
                            throw new Exception($"The folder with path \"{this.SearchSetting.RootDir} \" was not found.");
                        }

                        // Clear source
                        this.ClearDataSource();
                        this.IsAllItemsSelected = false; // Uncheck selected all
                        var items = this.HandleScanning();
                        this.OriginItems = items;
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }

                    // Filter items
                    bool isChangeData;
                    return this.FilterItemsByText(this.headerBase.FilterText, out isChangeData);
                }).ContinueWith(tks =>
                {
                    Execute.OnUIThreadAsync(() =>
                    {
                        var result = tks.Result;
                        if (result != null)
                        {
                            foreach (var itemInfo in result)
                            {
                                this.AddItem(itemInfo);
                            }
                        }

                        DialogHost.CloseDialogCommand.Execute(true, busyView);
                    });
                });
            }, null);

            if (exception != null)
            {
                await this.ShowDialog("RootDialog", $"Some thing went wrong execute action. Error(s): {exception.Message}");
            }
        }

        /// <summary>
        /// Increases the version.
        /// </summary>
        public async void Run()
        {
            var selectedItems = this.GetSelectedItems();
            if (!selectedItems.Any())
            {
                await this.ShowDialog("RootDialog", "Please selected item(s)");
                return;
            }

            if (this.Validate())
            {
                return;
            }

            // Show BusyIndicator
            var busyView = new BusyIndicator();
            Exception exception = null;

            await DialogHost.Show(busyView, "RootDialog", (sender, args) =>
            {
                Task.Run<bool>(() =>
                {
                    var result = false;
                    try
                    {
                        result = this.HandleIncreaseVersion(selectedItems);
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }
                    return result;
                }).ContinueWith(tks =>
                {
                    Execute.OnUIThreadAsync(() =>
                    {
                        DialogHost.CloseDialogCommand.Execute(true, busyView);
                    });
                });
            }, null);

            if (exception != null)
            {
                await this.ShowDialog("RootDialog", $"Some thing went wrong execute action. Error(s): {exception.Message}");
            }
        }

        /// <summary>
        /// Directories the browser.
        /// </summary>
        public void DirectoryBrowser()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = @"Direct to Implementation folder of Project";
                var result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    this.SearchSetting.RootDir = dialog.SelectedPath;
                    this.NotifyOfPropertyChange("RootDir");
                }
            }
        }

        /// <summary>
        /// Scans this instance.
        /// </summary>
        public abstract IList<TDataContext> HandleScanning();

        /// <summary>
        /// Increases the version.
        /// </summary>
        public abstract bool HandleIncreaseVersion(IList<TDataContext> selectedItems);

        /// <summary>
        /// Filters the items by text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="isChangedData">if set to <c>true</c> [is changed data].</param>
        /// <returns>IList&lt;TDataContext&gt;.</returns>
        protected abstract IList<TDataContext> FilterItemsByText(string text, out bool isChangedData);

        /// <summary>
        /// Validates the input.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public abstract bool ValidateInput();

        /// <summary>
        /// Checked all.
        /// </summary>
        /// <param name="isChecked">if set to <c>true</c> [is checked].</param>
        public void CheckedAll(bool? isChecked)
        {
            foreach (var it in this.Items)
            {
                it.IsChecked = isChecked == true;
            }
        }

        /// <summary>
        /// Gets the selected projects.
        /// </summary>
        /// <returns></returns>
        protected IList<TDataContext> GetSelectedItems()
        {
            return this.Items.Where(pr => pr.IsChecked != false).ToList();
        }

        /// <summary>
        /// Clears the data source.
        /// </summary>
        protected void ClearDataSource()
        {
            if (this.Items.Count > 0)
            {
                Execute.OnUIThread(() =>
                {
                    this.Items.Clear();
                });
            }
        }

        /// <summary>
        /// Adds the project.
        /// </summary>
        /// <param name="item">The project.</param>
        protected void AddItem(TDataContext item)
        {
            this.Items.Add(item);
        }

        /// <summary>
        /// Needs the reload data.
        /// </summary>
        protected virtual bool IsChangeData(IList<TDataContext> filteringItems)
        {
            // Diff number of element
            if (filteringItems.Count != this.Items.Count)
            {
                return true;
            }

            // Equal number element but diff inner data
            var listFilteringItemName = filteringItems.Select(n => n.Name).ToList();
            var listCurItemName = this.Items.Select(n => n.Name).ToList();
            if (VSProjectHelper.Equals(listCurItemName, listFilteringItemName))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="mainScreen">The main screen.</param>
        /// <param name="message">The message.</param>
        /// <returns>Task.</returns>
        public async Task ShowDialog(string mainScreen, string message)
        {
            var dialogView = new SimpleDialog
            {
                Message = {Text = message}
            };
            await DialogHost.Show(dialogView, mainScreen);
        }

        /// <summary>
        /// show error dialog
        /// </summary>
        /// <param name="message"></param>
        /// <param name="mainScreen"></param>
        /// <returns></returns>
        public async Task ShowErrorDialog(string message, string mainScreen = "RootDialog")
        {
            var dialogView = new SimpleDialog
            {
                Message = { Text = message }
            };
            await DialogHost.Show(dialogView, mainScreen);
        }
    }
}
