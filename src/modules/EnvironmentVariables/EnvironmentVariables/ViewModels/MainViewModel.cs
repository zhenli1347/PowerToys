﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnvironmentVariables.Helpers;
using EnvironmentVariables.Models;
using ManagedCommon;
using Microsoft.UI.Dispatching;

namespace EnvironmentVariables.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IEnvironmentVariablesService _environmentVariablesService;

        private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        public DefaultVariablesSet UserDefaultSet { get; private set; } = new DefaultVariablesSet(VariablesSet.UserGuid, ResourceLoaderInstance.ResourceLoader.GetString("User"), VariablesSetType.User);

        public DefaultVariablesSet SystemDefaultSet { get; private set; } = new DefaultVariablesSet(VariablesSet.SystemGuid, ResourceLoaderInstance.ResourceLoader.GetString("System"), VariablesSetType.System);

        public VariablesSet DefaultVariables { get; private set; } = new DefaultVariablesSet(Guid.NewGuid(), ResourceLoaderInstance.ResourceLoader.GetString("DefaultVariables"), VariablesSetType.User);

        [ObservableProperty]
        private ObservableCollection<ProfileVariablesSet> _profiles;

        [ObservableProperty]
        private ObservableCollection<Variable> _appliedVariables = new ObservableCollection<Variable>();

        [ObservableProperty]
        private bool _isElevated;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsInfoBarButtonVisible))]
        private EnvironmentState _isStateModified;

        public bool IsInfoBarButtonVisible => IsStateModified == EnvironmentState.EnvironmentMessageReceived;

        public ProfileVariablesSet AppliedProfile { get; set; }

        public MainViewModel(IEnvironmentVariablesService environmentVariablesService)
        {
            _environmentVariablesService = environmentVariablesService;
            var isElevated = App.GetService<IElevationHelper>().IsElevated;
            IsElevated = isElevated;
        }

        private void LoadDefaultVariables()
        {
            UserDefaultSet.Variables.Clear();
            SystemDefaultSet.Variables.Clear();
            DefaultVariables.Variables.Clear();

            EnvironmentVariablesHelper.GetVariables(EnvironmentVariableTarget.Machine, SystemDefaultSet);
            EnvironmentVariablesHelper.GetVariables(EnvironmentVariableTarget.User, UserDefaultSet);

            foreach (var variable in UserDefaultSet.Variables)
            {
                DefaultVariables.Variables.Add(variable);
            }

            foreach (var variable in SystemDefaultSet.Variables)
            {
                DefaultVariables.Variables.Add(variable);
            }
        }

        [RelayCommand]
        public void LoadEnvironmentVariables()
        {
            LoadDefaultVariables();
            LoadProfiles();
            PopulateAppliedVariables();
        }

        private void LoadProfiles()
        {
            try
            {
                var profiles = _environmentVariablesService.ReadProfiles();
                foreach (var profile in profiles)
                {
                    profile.PropertyChanged += Profile_PropertyChanged;

                    foreach (var variable in profile.Variables)
                    {
                        variable.ParentType = VariablesSetType.Profile;
                    }
                }

                var appliedProfiles = profiles.Where(x => x.IsEnabled).ToList();
                if (appliedProfiles.Count > 0)
                {
                    var appliedProfile = appliedProfiles.First();
                    if (appliedProfile.IsCorrectlyApplied())
                    {
                        AppliedProfile = appliedProfile;
                        IsStateModified = EnvironmentState.Unchanged;
                    }
                    else
                    {
                        IsStateModified = EnvironmentState.ChangedOnStartup;
                        appliedProfile.IsEnabled = false;
                    }
                }

                Profiles = new ObservableCollection<ProfileVariablesSet>(profiles);
            }
            catch (Exception ex)
            {
                // Show some error
                Logger.LogError("Failed to load profiles.json file", ex);
            }
        }

        private void PopulateAppliedVariables()
        {
            LoadDefaultVariables();

            var variables = new List<Variable>();
            if (AppliedProfile != null)
            {
                variables = variables.Concat(AppliedProfile.Variables.OrderBy(x => x.Name)).ToList();
            }

            variables = variables.Concat(UserDefaultSet.Variables.OrderBy(x => x.Name)).Concat(SystemDefaultSet.Variables.OrderBy(x => x.Name)).ToList();

            // Handle PATH variable - add USER value to the end of the SYSTEM value
            var userPath = variables.Where(x => x.Name.Equals("PATH", StringComparison.OrdinalIgnoreCase) && x.ParentType == VariablesSetType.User).FirstOrDefault();
            var systemPath = variables.Where(x => x.Name.Equals("PATH", StringComparison.OrdinalIgnoreCase) && x.ParentType == VariablesSetType.System).FirstOrDefault();

            if (!string.IsNullOrEmpty(userPath.Name) && !string.IsNullOrEmpty(systemPath.Name))
            {
                var clone = systemPath.Clone();
                clone.Values += ";" + userPath.Values;
                variables.Remove(userPath);
                variables.Insert(variables.IndexOf(systemPath), clone);
                variables.Remove(systemPath);
            }

            // Handle variables with the same name but different casing in USER and SYSTEM variables
            // TODO: (stefan)
            variables = variables.GroupBy(x => x.Name).Select(y => y.First()).ToList();

            AppliedVariables = new ObservableCollection<Variable>(variables);
        }

        internal void AddDefaultVariable(Variable variable, VariablesSetType type)
        {
            if (type == VariablesSetType.User)
            {
                UserDefaultSet.Variables.Add(variable);
                UserDefaultSet.Variables = new ObservableCollection<Variable>(UserDefaultSet.Variables.OrderBy(x => x.Name).ToList());
            }
            else if (type == VariablesSetType.System)
            {
                SystemDefaultSet.Variables.Add(variable);
                SystemDefaultSet.Variables = new ObservableCollection<Variable>(SystemDefaultSet.Variables.OrderBy(x => x.Name).ToList());
            }

            EnvironmentVariablesHelper.SetVariable(variable);
        }

        internal void EditVariable(Variable original, Variable edited, ProfileVariablesSet variablesSet)
        {
            bool propagateChange = variablesSet == null /* not a profile */ || variablesSet.Id.Equals(AppliedProfile?.Id);
            bool changed = original.Name != edited.Name || original.Values != edited.Values;
            if (changed)
            {
                var task = original.Update(edited, propagateChange);
                task.ContinueWith(x =>
                {
                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        PopulateAppliedVariables();
                    });
                });

                _ = Task.Run(SaveAsync);
            }
        }

        internal void AddProfile(ProfileVariablesSet profile)
        {
            profile.PropertyChanged += Profile_PropertyChanged;
            if (profile.IsEnabled)
            {
                UnsetAppliedProfile();
                SetAppliedProfile(profile);
            }

            Profiles.Add(profile);

            _ = Task.Run(SaveAsync);
        }

        internal void UpdateProfile(ProfileVariablesSet updatedProfile)
        {
            var existingProfile = Profiles.Where(x => x.Id == updatedProfile.Id).FirstOrDefault();
            existingProfile.Name = updatedProfile.Name;
            existingProfile.IsEnabled = updatedProfile.IsEnabled;
            existingProfile.Variables = updatedProfile.Variables;
        }

        private async Task SaveAsync()
        {
            try
            {
                await _environmentVariablesService.WriteAsync(Profiles);
            }
            catch (Exception ex)
            {
                // Show some error
                Logger.LogError("Failed to save to profiles.json file", ex);
            }
        }

        private void Profile_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var profile = sender as ProfileVariablesSet;

            if (profile != null)
            {
                if (e.PropertyName == nameof(ProfileVariablesSet.IsEnabled))
                {
                    if (profile.IsEnabled)
                    {
                        UnsetAppliedProfile();
                        SetAppliedProfile(profile);
                    }
                    else
                    {
                        UnsetAppliedProfile();
                    }
                }
            }

            _ = Task.Run(SaveAsync);
        }

        private void SetAppliedProfile(ProfileVariablesSet profile)
        {
            var task = profile.Apply();
            task.ContinueWith((a) =>
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    PopulateAppliedVariables();
                });
            });
            AppliedProfile = profile;
        }

        private void UnsetAppliedProfile()
        {
            if (AppliedProfile != null)
            {
                var appliedProfile = AppliedProfile;
                appliedProfile.PropertyChanged -= Profile_PropertyChanged;
                var task = AppliedProfile.UnApply();
                task.ContinueWith((a) =>
                {
                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        PopulateAppliedVariables();
                    });
                });
                AppliedProfile.IsEnabled = false;
                AppliedProfile = null;
                appliedProfile.PropertyChanged += Profile_PropertyChanged;
            }
        }

        internal void RemoveProfile(ProfileVariablesSet profile)
        {
            if (profile.IsEnabled)
            {
                UnsetAppliedProfile();
            }

            Profiles.Remove(profile);

            _ = Task.Run(SaveAsync);
        }

        internal void DeleteVariable(Variable variable, ProfileVariablesSet profile)
        {
            bool propagateChange = true;

            if (profile != null)
            {
                // Profile variable
                profile.Variables.Remove(variable);

                if (!profile.IsEnabled)
                {
                    propagateChange = false;
                }
            }
            else
            {
                if (variable.ParentType == VariablesSetType.User)
                {
                    UserDefaultSet.Variables.Remove(variable);
                }
                else if (variable.ParentType == VariablesSetType.System)
                {
                    SystemDefaultSet.Variables.Remove(variable);
                }
            }

            if (propagateChange)
            {
                var task = Task.Run(() =>
                {
                    EnvironmentVariablesHelper.UnsetVariable(variable);
                });
                task.ContinueWith((a) =>
                {
                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        PopulateAppliedVariables();
                    });
                });
            }
        }
    }
}