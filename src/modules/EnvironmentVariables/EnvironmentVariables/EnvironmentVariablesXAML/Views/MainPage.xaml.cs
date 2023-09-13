// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Labs.WinUI;
using CommunityToolkit.Mvvm.Input;
using EnvironmentVariables.Models;
using EnvironmentVariables.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace EnvironmentVariables.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; private set; }

        public ICommand EditCommand => new RelayCommand<Variable>(EditVariable);

        public ICommand NewProfileCommand => new AsyncRelayCommand(AddProfileAsync);

        public ICommand AddProfileCommand => new RelayCommand(AddProfile);

        public ICommand AddVariableCommand => new RelayCommand(AddVariable);

        public MainPage()
        {
            this.InitializeComponent();
            ViewModel = App.GetService<MainViewModel>();
            DataContext = ViewModel;
        }

        private async Task ShowEditDialogAsync(Variable variable)
        {
            var resourceLoader = Helpers.ResourceLoaderInstance.ResourceLoader;

            EditVariableDialog.Title = resourceLoader.GetString("EditVariableDialog_Title");
            EditVariableDialog.PrimaryButtonText = resourceLoader.GetString("SaveBtn");
            EditVariableDialog.PrimaryButtonCommand = EditCommand;
            EditVariableDialog.PrimaryButtonCommandParameter = variable;

            var clone = variable.Clone();
            EditVariableDialog.DataContext = clone;

            await EditVariableDialog.ShowAsync();
        }

        private async void EditVariable_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            SettingsCard card = sender as SettingsCard;
            if (card != null)
            {
                await ShowEditDialogAsync(card.CommandParameter as Variable);
            }
        }

        private void EditVariable(Variable original)
        {
            var edited = EditVariableDialog.DataContext as Variable;
            ViewModel.EditVariable(original, edited);
        }

        private async Task AddProfileAsync()
        {
            SwitchViewsSegmentedView.SelectedIndex = 0;

            var resourceLoader = Helpers.ResourceLoaderInstance.ResourceLoader;
            AddProfileDialog.Title = resourceLoader.GetString("AddNewProfileDialog_Title");
            AddProfileDialog.PrimaryButtonText = resourceLoader.GetString("AddBtn");
            AddProfileDialog.PrimaryButtonCommand = AddProfileCommand;
            AddProfileDialog.DataContext = new ProfileVariablesSet(Guid.NewGuid(), string.Empty);
            await AddProfileDialog.ShowAsync();
        }

        private void AddProfile()
        {
            var profile = AddProfileDialog.DataContext as ProfileVariablesSet;
            ViewModel.AddProfile(profile);
        }

        private void AddVariable()
        {
            var profile = AddProfileDialog.DataContext as ProfileVariablesSet;
            if (profile != null)
            {
                if (AddVariableSwitchPresenter.Value as string == "NewVariable")
                {
                    profile.Variables.Add(new Variable(AddNewVariableName.Text, AddNewVariableValue.Text, VariablesSetType.Profile));
                }
                else
                {
                    foreach (Variable variable in ExistingVariablesListView.SelectedItems)
                    {
                        var clone = variable.Clone(true);
                        profile.Variables.Add(clone);
                    }
                }
            }

            AddNewVariableName.Text = string.Empty;
            AddNewVariableValue.Text = string.Empty;
            ExistingVariablesListView.SelectedItems.Clear();
            AddVariableFlyout.Hide();
        }
    }
}
