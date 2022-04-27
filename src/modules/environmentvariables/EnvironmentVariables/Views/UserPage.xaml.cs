using EnvironmentVariables.Models;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EnvironmentVariables.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UserPage : Page
    {
        public UserPage()
        {
            this.InitializeComponent();
        }


        public ICommand NewDialogCommand => new AsyncRelayCommand(OpenNewDialog);

        public ICommand EditDialogCommand => new AsyncRelayCommand(OpenEditDialog);

        public ICommand RemoveDialogCommand => new AsyncRelayCommand(OpenDeleteDialog);

        private ICommand UpdateCommand => new RelayCommand(Update);

        private ICommand InsertCommand => new RelayCommand(Insert);

        private ICommand DeleteCommand => new RelayCommand(Delete);

        private async Task OpenNewDialog()
        {
            EditDialog.Title = "New environment variable";
            EditDialog.PrimaryButtonText = "Add";
            EditDialog.PrimaryButtonCommand = InsertCommand;
            EditDialog.DataContext = new EnvVariable();
            await EditDialog.ShowAsync();
        }

        private async Task OpenEditDialog()
        {
            EditDialog.Title = "Edit environment variable";
            EditDialog.PrimaryButtonText = "Update";
            EditDialog.PrimaryButtonCommand = UpdateCommand;
            var clone = ViewModel.Current.Clone();
            clone.Key = ViewModel.Current.Key;
            EditDialog.DataContext = clone;
            await EditDialog.ShowAsync();
        }

        private async Task OpenDeleteDialog()
        {
            DeleteDialog.PrimaryButtonCommand = DeleteCommand;
            DeleteDialog.DataContext = ViewModel.Current;
            await DeleteDialog.ShowAsync();
        }

        private void Update()
        {
            ViewModel.UpdateItem(EditDialog.DataContext as EnvVariable, ViewModel.Current);
        }

        private void Delete()
        {
            ViewModel.DeleteItem(DeleteDialog.DataContext as EnvVariable);
        }

        private void Insert()
        {
            // Does not work when filter is active:
            //  ViewModel.Items.Add(EditDialog.DataContext as EnvVariable);

            var character = ViewModel.AddItem(EditDialog.DataContext as EnvVariable);
            if (ViewModel.Items.Contains(character))
            {
                ViewModel.Current = character;
            }
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            // If you need the clicked element:
         
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuFlyoutItem)
            {
                MenuFlyoutItem selectedItem = sender as MenuFlyoutItem;
                ViewModel.Current = (EnvVariable)selectedItem.DataContext;
                await OpenDeleteDialog();
            }
        }

            private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            EnvVariable selectedItem = e.ClickedItem as EnvVariable;
            ViewModel.Current = selectedItem;
            EditDialog.Title = "Edit environment variable";
            EditDialog.PrimaryButtonText = "Update";
            EditDialog.PrimaryButtonCommand = UpdateCommand;
            var clone = ViewModel.Current.Clone();
            clone.Key = ViewModel.Current.Key;
            EditDialog.DataContext = clone;
            await EditDialog.ShowAsync();
        }
    }
}
