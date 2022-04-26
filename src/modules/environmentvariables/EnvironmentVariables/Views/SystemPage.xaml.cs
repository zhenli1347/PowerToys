using EnvironmentVariables.Helpers;
using EnvironmentVariables.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EnvironmentVariables.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SystemPage : Page
    {
        public ObservableCollection<Person> Items { get; set; }
        public SystemPage()
        {
            this.InitializeComponent();

            var uservars = EnvironmentVariablesHelper.GetVariables(EnvironmentVariableTarget.User);
            Items = new ObservableCollection<Person>();
            foreach (EnvVariable var in uservars)
            {
                Items.Add(new Person() {  Key = var.Key, Value = var.Value });
           //     Items.Add(var);
            }
        }

        private void EmployeeGrid_BeginningEdit(object sender, CommunityToolkit.WinUI.UI.Controls.DataGridBeginningEditEventArgs e)
        {
            //Person p = new Person
            //{
            //    PersonId = "[Add New Row]",
            //    FirstName = nul
            //};
            //int x = EmployeeGrid.SelectedIndex;
            //int y = Items.Count;
            //if (x + 1 == y)
            //{
            //    Items.Insert(Items.Count, p);
            //}
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Person p = button.DataContext as Person;
            var messageDialog = new ContentDialog()
            {
                Title = "Warning",
                Content = "Are you sure you want to delete?",
                PrimaryButtonText = "Ok",
                CloseButtonText = "Cancel"
            };
            var result = await messageDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                Items.Remove(p);
            }
        }

        private async void EmployeeGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EditDialog.DataContext = EmployeeGrid.SelectedItem as Person;
           
            await EditDialog.ShowAsync();
        }
    }

    public class Person : INotifyPropertyChanged
    {
        private string key;
        private string valuestring;

        public string Key
        {
            get
            {
                return key;
            }
            set
            {
                key = value;
                RaisePropertyChanged(nameof(Key));
            }
        }
        public string Value
        {
            get
            {
                return valuestring;
            }
            set
            {
                valuestring = value;
                RaisePropertyChanged(nameof(Value));
            }
        }
       

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string propertyname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
    }
}
