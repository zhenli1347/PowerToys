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
            EditDialog.Title = "Delete environment variable";
            EditDialog.PrimaryButtonText = "Delete";
            EditDialog.PrimaryButtonCommand = DeleteCommand;

            EditDialog.DataContext = ViewModel.Current;
            await EditDialog.ShowAsync();
        }

        private void Update()
        {
            ViewModel.UpdateItem(EditDialog.DataContext as EnvVariable, ViewModel.Current);
        }

        private void Delete()
        {
            ViewModel.DeleteItem(EditDialog.DataContext as EnvVariable);
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
    }
}
