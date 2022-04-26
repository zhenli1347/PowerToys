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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EnvironmentVariables.Views
{
    public sealed partial class MainPageControl : UserControl
    {
        public MainPageControl()
        {
            this.InitializeComponent();
        }

        public ICommand NewCommand => new AsyncRelayCommand(OpenNewDialog);

        public ICommand EditCommand => new AsyncRelayCommand(OpenEditDialog);

        private ICommand UpdateCommand => new RelayCommand(Update);

        private ICommand InsertCommand => new RelayCommand(Insert);

        private async Task OpenNewDialog()
        {
            EditDialog.Title = "New Character";
            EditDialog.PrimaryButtonText = "Insert";
            EditDialog.PrimaryButtonCommand = InsertCommand;
            EditDialog.DataContext = new EnvVariable();
            await EditDialog.ShowAsync();
        }

        private async Task OpenEditDialog()
        {
            EditDialog.Title = "Edit Character";
            EditDialog.PrimaryButtonText = "Update";
            EditDialog.PrimaryButtonCommand = UpdateCommand;
            var clone = ViewModel.Current.Clone();
            clone.Name = ViewModel.Current.Name;
            EditDialog.DataContext = clone;
            await EditDialog.ShowAsync();
        }

        private void Update()
        {
            ViewModel.UpdateItem(EditDialog.DataContext as EnvVariable, ViewModel.Current);
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
