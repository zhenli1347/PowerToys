// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArchivePreviewHandler.Models
{
    public class ArchiveItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DirectoryTemplate { get; set; }

        public DataTemplate FileTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            var archiveItem = item as ArchiveItem;
            return archiveItem.Type == ArchiveItemType.Directory ? DirectoryTemplate : FileTemplate;
        }
    }
}
