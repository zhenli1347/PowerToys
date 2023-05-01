// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;

namespace ArchivePreviewHandler.Models
{
    public partial class ArchiveItem : ObservableObject
    {
        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private ArchiveItemType _type;

        [ObservableProperty]
        private ImageSource _icon;

        [ObservableProperty]
        private long _size;

        [ObservableProperty]
        private bool _isExpanded;

        public ObservableCollection<ArchiveItem> Childrens { get; }

        public ArchiveItem(string name, ArchiveItemType type, ImageSource icon)
        {
            Name = name;
            Type = type;
            Icon = icon;
            Childrens = new ObservableCollection<ArchiveItem>();
        }
    }
}
