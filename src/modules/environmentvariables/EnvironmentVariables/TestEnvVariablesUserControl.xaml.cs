using EnvironmentVariables.Models;
using EnvironmentVariables.ViewModels;
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
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EnvironmentVariables
{
    public partial class TestEnvVariablesUserControl : UserControl
    {
        public TestEnvVariablesUserControl()
        {
            this.InitializeComponent();
        }

        public TestEnvVariablesUserControl(EnvironmentVariableTarget target)
        {
            this.InitializeComponent();
            ViewModel = new EnvVariables(target);
            DataContext = ViewModel;
        }
    
        private EnvVariables ViewModel { get; set; }

        private void AddVariable(object sender, RoutedEventArgs e)
        {
            ViewModel.AddVariable("testvar", "testvalue");
        }

        private void EditVariable(object sender, RoutedEventArgs e)
        {
            var selectedEnvVariable = EnvVariablesList.SelectedItem as EnvVariable;
            if (selectedEnvVariable != null)
            {
                new Thread(() =>
                {
                    ViewModel.EditVariable("testvar1", "testvalue");
                }).Start();
            }
        }
        private void DeleteVariable(object sender, RoutedEventArgs e)
        {
            var selectedEnvVariable = EnvVariablesList.SelectedItem as EnvVariable;
            if (selectedEnvVariable != null)
            {
                ViewModel.DeleteVariable(selectedEnvVariable.Label);
            }
        }
    }
}
