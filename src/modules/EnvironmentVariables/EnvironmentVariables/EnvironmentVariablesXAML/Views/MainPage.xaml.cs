// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Controls;
using EnvironmentVariables.Models;
using EnvironmentVariables.ViewModels;
using Microsoft.UI.Xaml.Controls;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EnvironmentVariables.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; private set; }

        public ICommand EditCommand => new RelayCommand<SettingsCard>(EditVariable);

        public ICommand NewProfileCommand => new AsyncRelayCommand(AddProfileAsync);

        public ICommand AddProfileCommand => new RelayCommand(AddProfile);

        public ICommand UpdateProfileCommand => new RelayCommand(UpdateProfile);

        public ICommand AddVariableCommand => new RelayCommand(AddVariable);

        public ICommand AddDefaultVariableCommand => new RelayCommand<DefaultVariablesSet>(AddDefaultVariable);

        public MainPage()
        {
            this.InitializeComponent();
            ViewModel = App.GetService<MainViewModel>();
            DataContext = ViewModel;
        }

        private async Task ShowEditDialogAsync(SettingsCard card)
        {
            var resourceLoader = Helpers.ResourceLoaderInstance.ResourceLoader;

            EditVariableDialog.Title = resourceLoader.GetString("EditVariableDialog_Title");
            EditVariableDialog.PrimaryButtonText = resourceLoader.GetString("SaveBtn");
            EditVariableDialog.SecondaryButtonText = resourceLoader.GetString("CancelBtn");
            EditVariableDialog.PrimaryButtonCommand = EditCommand;
            EditVariableDialog.PrimaryButtonCommandParameter = card;

            var variable = card.CommandParameter as Variable;
            var clone = variable.Clone();
            EditVariableDialog.DataContext = clone;

            await EditVariableDialog.ShowAsync();
        }

        private async void EditVariable_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            SettingsCard card = sender as SettingsCard;
            if (card != null)
            {
                await ShowEditDialogAsync(card);
            }
        }

        private void EditVariable(SettingsCard card)
        {
            var variableSet = card.DataContext as ProfileVariablesSet;
            var original = card.CommandParameter as Variable;
            var edited = EditVariableDialog.DataContext as Variable;
            ViewModel.EditVariable(original, edited, variableSet);
        }

        private async Task AddProfileAsync()
        {
            SwitchViewsSegmentedView.SelectedIndex = 0;

            var resourceLoader = Helpers.ResourceLoaderInstance.ResourceLoader;
            AddProfileDialog.Title = resourceLoader.GetString("AddNewProfileDialog_Title");
            AddProfileDialog.PrimaryButtonText = resourceLoader.GetString("AddBtn");
            AddProfileDialog.SecondaryButtonText = resourceLoader.GetString("CancelBtn");
            AddProfileDialog.PrimaryButtonCommand = AddProfileCommand;
            AddProfileDialog.DataContext = new ProfileVariablesSet(Guid.NewGuid(), string.Empty);
            await AddProfileDialog.ShowAsync();
        }

        private void AddProfile()
        {
            var profile = AddProfileDialog.DataContext as ProfileVariablesSet;
            ViewModel.AddProfile(profile);
        }

        private void UpdateProfile()
        {
            var updatedProfile = AddProfileDialog.DataContext as ProfileVariablesSet;
            ViewModel.UpdateProfile(updatedProfile);
        }

        private async void RemoveProfileBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var button = sender as Button;
            var profile = button.CommandParameter as ProfileVariablesSet;

            if (profile != null)
            {
                var resourceLoader = Helpers.ResourceLoaderInstance.ResourceLoader;
                ContentDialog dialog = new ContentDialog();
                dialog.XamlRoot = RootPage.XamlRoot;
                dialog.Title = profile.Name;
                dialog.PrimaryButtonText = resourceLoader.GetString("Yes");
                dialog.CloseButtonText = resourceLoader.GetString("No");
                dialog.DefaultButton = ContentDialogButton.Primary;
                dialog.Content = new TextBlock() { Text = resourceLoader.GetString("Delete_Dialog_Description") };
                dialog.PrimaryButtonClick += (s, args) =>
                {
                    ViewModel.RemoveProfile(profile);
                };

                var result = await dialog.ShowAsync();
            }
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
                        if (!profile.Variables.Where(x => x.Name == variable.Name).Any())
                        {
                            var clone = variable.Clone(true);
                            profile.Variables.Add(clone);
                        }
                    }
                }
            }

            AddNewVariableName.Text = string.Empty;
            AddNewVariableValue.Text = string.Empty;
            ExistingVariablesListView.SelectionChanged -= ExistingVariablesListView_SelectionChanged;
            ExistingVariablesListView.SelectedItems.Clear();
            ExistingVariablesListView.SelectionChanged += ExistingVariablesListView_SelectionChanged;
            AddVariableFlyout.Hide();
        }

        private void AddDefaultVariable(DefaultVariablesSet set)
        {
            var variable = AddDefaultVariableDialog.DataContext as Variable;
            var type = set.Type;

            ViewModel.AddDefaultVariable(variable, type);
        }

        private async void Delete_Variable_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            MenuFlyoutItem menuItem = sender as MenuFlyoutItem;
            var variableSet = menuItem.DataContext as ProfileVariablesSet;
            var variable = menuItem.CommandParameter as Variable;

            if (variable != null)
            {
                var resourceLoader = Helpers.ResourceLoaderInstance.ResourceLoader;
                ContentDialog dialog = new ContentDialog();
                dialog.XamlRoot = RootPage.XamlRoot;
                dialog.Title = variable.Name;
                dialog.PrimaryButtonText = resourceLoader.GetString("Yes");
                dialog.CloseButtonText = resourceLoader.GetString("No");
                dialog.DefaultButton = ContentDialogButton.Primary;
                dialog.Content = new TextBlock() { Text = resourceLoader.GetString("Delete_Dialog_Description") };
                dialog.PrimaryButtonClick += (s, args) =>
                {
                    ViewModel.DeleteVariable(variable, variableSet);
                };
                var result = await dialog.ShowAsync();
            }
        }

        private void AddNewVariableName_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox nameTxtBox = sender as TextBox;
            var profile = AddProfileDialog.DataContext as ProfileVariablesSet;

            if (nameTxtBox != null)
            {
                if (nameTxtBox.Text.Length == 0 || profile.Variables.Where(x => x.Name == nameTxtBox.Text).Any())
                {
                    ConfirmAddVariableBtn.IsEnabled = false;
                }
                else
                {
                    ConfirmAddVariableBtn.IsEnabled = true;
                }
            }
        }

        private void ReloadButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.LoadEnvironmentVariables();
            ViewModel.IsStateModified = EnvironmentState.Unchanged;
        }

        private void ExistingVariablesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ExistingVariablesListView.SelectedItems.Count == 0)
            {
                ConfirmAddVariableBtn.IsEnabled = false;
            }
            else
            {
                ConfirmAddVariableBtn.IsEnabled = true;
            }

            var profile = AddProfileDialog.DataContext as ProfileVariablesSet;

            int toRemove = -1;

            if (e.RemovedItems.Count > 0)
            {
                Variable removedVariable = e.RemovedItems[0] as Variable;
                for (int i = 0; i < profile.Variables.Count; i++)
                {
                    if (profile.Variables[i].Name == removedVariable.Name)
                    {
                        toRemove = i;
                        break;
                    }
                }

                if (toRemove != -1)
                {
                    profile.Variables.RemoveAt(toRemove);
                }
            }
        }

        private async void EditProfileBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            SwitchViewsSegmentedView.SelectedIndex = 0;

            var button = sender as Button;
            var profile = button.CommandParameter as ProfileVariablesSet;

            if (profile != null)
            {
                var resourceLoader = Helpers.ResourceLoaderInstance.ResourceLoader;
                AddProfileDialog.Title = resourceLoader.GetString("AddNewProfileDialog_Title");
                AddProfileDialog.PrimaryButtonText = resourceLoader.GetString("SaveBtn");
                AddProfileDialog.SecondaryButtonText = resourceLoader.GetString("CancelBtn");
                AddProfileDialog.PrimaryButtonCommand = UpdateProfileCommand;
                AddProfileDialog.DataContext = profile.Clone();
                await AddProfileDialog.ShowAsync();
            }
        }

        private void ExistingVariablesListView_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var profile = AddProfileDialog.DataContext as ProfileVariablesSet;

            foreach (Variable item in ExistingVariablesListView.Items)
            {
                if (item != null)
                {
                    foreach (var profileItem in profile.Variables)
                    {
                        if (profileItem.Name == item.Name)
                        {
                            ExistingVariablesListView.SelectedItems.Add(item);
                        }
                    }
                }
            }
        }

        private async Task ShowAddDefaultVariableDialogAsync(DefaultVariablesSet set)
        {
            var resourceLoader = Helpers.ResourceLoaderInstance.ResourceLoader;

            AddDefaultVariableDialog.Title = resourceLoader.GetString("AddVariable_Title");
            AddDefaultVariableDialog.PrimaryButtonText = resourceLoader.GetString("SaveBtn");
            AddDefaultVariableDialog.SecondaryButtonText = resourceLoader.GetString("CancelBtn");
            AddDefaultVariableDialog.PrimaryButtonCommand = AddDefaultVariableCommand;
            AddDefaultVariableDialog.PrimaryButtonCommandParameter = set;

            var variableType = set.Id == VariablesSet.SystemGuid ? VariablesSetType.System : VariablesSetType.User;
            AddDefaultVariableDialog.DataContext = new Variable(string.Empty, string.Empty, variableType);

            await AddDefaultVariableDialog.ShowAsync();
        }

        private async void AddDefaultVariableBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var button = sender as Button;
            var defaultVariableSet = button.CommandParameter as DefaultVariablesSet;

            if (defaultVariableSet != null)
            {
                await ShowAddDefaultVariableDialogAsync(defaultVariableSet);
            }
        }

        private void EditVariableDialogNameTxtBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var variable = EditVariableDialog.DataContext as Variable;
            var settingsCard = EditVariableDialog.PrimaryButtonCommandParameter as SettingsCard;
            var variableSet = settingsCard.DataContext as VariablesSet;

            if (variableSet == null)
            {
                // default set
                variableSet = variable.ParentType == VariablesSetType.User ? ViewModel.UserDefaultSet : ViewModel.SystemDefaultSet;
            }

            if (variableSet != null)
            {
                if (variableSet.Variables.Where(x => x.Name == EditVariableDialogNameTxtBox.Text).Any() || !variable.Valid)
                {
                    EditVariableDialog.IsPrimaryButtonEnabled = false;
                }
                else
                {
                    EditVariableDialog.IsPrimaryButtonEnabled = true;
                }
            }
        }
    }
}
