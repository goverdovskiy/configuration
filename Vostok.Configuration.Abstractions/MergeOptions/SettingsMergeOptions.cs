﻿namespace Vostok.Configuration.Abstractions.MergeOptions
{
    public class SettingsMergeOptions
    {
        public TreeMergeStyle TreeMergeStyle { get; set; } = TreeMergeStyle.Shallow;
        public ListMergeStyle ListMergeStyle { get; set; } = ListMergeStyle.Concat;
    }
}