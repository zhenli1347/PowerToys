// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using ArchivePreviewHandler.Helpers;
using ArchivePreviewHandler.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using ManagedCommon;
using SharpCompress.Archives;
using Windows.ApplicationModel.Resources;

namespace ArchivePreviewHandler
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IIconManager _iconManager;
        private int _directoryCount;
        private int _fileCount;
        private long _size;
        private long _extractedSize;

        [ObservableProperty]
        private bool _isFailed;

        [ObservableProperty]
        private string _directoryText;

        [ObservableProperty]
        private string _fileText;

        [ObservableProperty]
        private string _sizeText;

        public ObservableCollection<ArchiveItem> Tree { get; }

        public MainViewModel(IIconManager iconManager)
        {
            _iconManager = iconManager;

            Tree = new ObservableCollection<ArchiveItem>();
        }

        public void DoPreview(string filePath)
        {
            try
            {
                using var stream = File.OpenRead(filePath);
                using var archive = ArchiveFactory.Open(stream);

                _size = new FileInfo(filePath).Length; // archive.TotalSize isn't accurate
                _extractedSize = archive.TotalUncompressSize;

                foreach (var entry in archive.Entries)
                {
                    var levels = entry.Key.Split('/', '\\').Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();

                    ArchiveItem parent = null;
                    for (var i = 0; i < levels.Length; i++)
                    {
                        var type = (!entry.IsDirectory && i == levels.Length - 1) ? ArchiveItemType.File : ArchiveItemType.Directory;
                        var icon = type == ArchiveItemType.Directory ? _iconManager.GetDirectoryIcon() : _iconManager.GetFileIcon(entry.Key);
                        var item = new ArchiveItem(levels[i], type, icon);

                        if (type == ArchiveItemType.Directory)
                        {
                            item.IsExpanded = parent == null; // Only the root level is expanded
                        }
                        else if (type == ArchiveItemType.File)
                        {
                            item.Size = entry.Size;
                        }

                        if (parent == null)
                        {
                            var existing = Tree.FirstOrDefault(e => e.Name == item.Name);
                            if (existing == null)
                            {
                                var index = GetIndex(Tree, item);
                                Tree.Insert(index, item);
                                CountItem(item);
                            }

                            parent = existing ?? Tree.First(e => e.Name == item.Name);
                        }
                        else
                        {
                            var existing = parent.Children.FirstOrDefault(e => e.Name == item.Name);
                            if (existing == null)
                            {
                                var index = GetIndex(parent.Children, item);
                                parent.Children.Insert(index, item);
                                CountItem(item);
                            }

                            parent = existing ?? parent.Children.First(e => e.Name == item.Name);
                        }
                    }
                }

                DirectoryText = string.Format(CultureInfo.CurrentCulture, ResourceLoader.GetForViewIndependentUse().GetString("Archive_Directory_Count"), _directoryCount);
                FileText = string.Format(CultureInfo.CurrentCulture, ResourceLoader.GetForViewIndependentUse().GetString("Archive_File_Count"), _fileCount);
                SizeText = string.Format(CultureInfo.CurrentCulture, ResourceLoader.GetForViewIndependentUse().GetString("Archive_Size"), SizeHelper.GetHumanSize(_size), SizeHelper.GetHumanSize(_extractedSize));
            }
            catch (Exception ex)
            {
                Logger.LogError("Preview failed", ex);
                IsFailed = true;
            }
        }

        private int GetIndex(ObservableCollection<ArchiveItem> collection, ArchiveItem item)
        {
            for (var i = 0; i < collection.Count; i++)
            {
                if (item.Type == collection[i].Type && string.Compare(collection[i].Name, item.Name, StringComparison.Ordinal) > 0)
                {
                    return i;
                }
            }

            return item.Type switch
            {
                ArchiveItemType.Directory => collection.Count(e => e.Type == ArchiveItemType.Directory),
                ArchiveItemType.File => collection.Count,
                _ => 0,
            };
        }

        private void CountItem(ArchiveItem item)
        {
            if (item.Type == ArchiveItemType.Directory)
            {
                _directoryCount++;
            }
            else if (item.Type == ArchiveItemType.File)
            {
                _fileCount++;
            }
        }
    }
}
