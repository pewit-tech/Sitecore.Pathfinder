﻿// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sitecore.Pathfinder.Diagnostics;
using Sitecore.Pathfinder.Extensions;
using Sitecore.Pathfinder.Snapshots;

namespace Sitecore.Pathfinder.Projects
{
    [DebuggerDisplay("{GetType().Name,nq}: {ItemIdOrPath}")]
    public abstract class DatabaseProjectItem : ProjectItem, IHasSourceTextNodes
    {
        [NotNull, ItemNotNull]
        private readonly IList<ITextNode> _sourceTextNodes;

        [CanBeNull]
        private ID _id;

        protected DatabaseProjectItem([NotNull] IProjectBase project, Guid guid, [NotNull] string databaseName, [NotNull] string itemName, [NotNull] string itemIdOrPath) : base(project, new ProjectItemUri(databaseName, guid))
        {
            ItemNameProperty = NewSourceProperty("ItemName", string.Empty, SourcePropertyFlags.IsShort);
            IconProperty = NewSourceProperty("Icon", string.Empty);

            _sourceTextNodes = new LockableList<ITextNode>(this);

            DatabaseName = databaseName;
            ItemName = itemName;
            ItemIdOrPath = itemIdOrPath;
        }

        public override IEnumerable<ISnapshot> AdditionalSnapshots => AdditionalSourceTextNodes.Select(s => s.Snapshot);

        public IEnumerable<ITextNode> AdditionalSourceTextNodes => _sourceTextNodes.Skip(1);

        [NotNull]
        public Database Database => Project.GetDatabase(DatabaseName);

        [NotNull]
        public string DatabaseName { get; private set; }

        [NotNull]
        public string Icon
        {
            get { return IconProperty.GetValue(); }
            set { IconProperty.SetValue(value); }
        }

        [NotNull]
        public SourceProperty<string> IconProperty { get; }

        [NotNull, Obsolete("Use Uri.Guid instead", false)]
        public ID ID => _id ?? (_id = new ID(Uri.Guid));

        /// <summary>Indicates if the item or template will saved to the database during installation.</summary>
        public bool IsEmittable { get; set; } = true;

        /// <summary>Indicates if the item or template is imported from a packages. It will not be emitted.</summary>
        public bool IsImport { get; set; }

        [NotNull]
        public string ItemIdOrPath { get; private set; }

        /// <summary>The name of the item or template. Same as ShortName.</summary>
        [NotNull]
        public string ItemName
        {
            get { return ItemNameProperty.GetValue(); }
            set { ItemNameProperty.SetValue(value); }
        }

        [NotNull]
        public SourceProperty<string> ItemNameProperty { get; }

        /// <summary>The name of the item or template. Same as ItemName and ShortName.</summary>
        [NotNull, Obsolete("Use ItemName instead", false)]
        public string Name => ItemName;

        public override string QualifiedName => ItemIdOrPath;

        public override string ShortName => ItemName;

        public override ISnapshot Snapshot => SourceTextNode.Snapshot;

        public ITextNode SourceTextNode => _sourceTextNodes.FirstOrDefault() ?? TextNode.Empty;

        protected override void Merge(IProjectItem newProjectItem, bool overwrite)
        {
            base.Merge(newProjectItem, overwrite);

            var databaseProjectItem = newProjectItem as DatabaseProjectItem;
            Assert.Cast(databaseProjectItem, nameof(databaseProjectItem));

            _sourceTextNodes.Merge(SourceTextNode, databaseProjectItem.AdditionalSourceTextNodes, databaseProjectItem.SourceTextNode, overwrite);

            if (overwrite)
            {
                ItemNameProperty.SetValue(databaseProjectItem.ItemNameProperty);

                ItemIdOrPath = databaseProjectItem.ItemIdOrPath;
                DatabaseName = databaseProjectItem.DatabaseName;
            }

            if (!string.IsNullOrEmpty(databaseProjectItem.DatabaseName))
            {
                DatabaseName = databaseProjectItem.DatabaseName;
            }

            if (!string.IsNullOrEmpty(databaseProjectItem.Icon))
            {
                IconProperty.SetValue(databaseProjectItem.IconProperty);
            }

            IsEmittable = IsEmittable || databaseProjectItem.IsEmittable;
            IsImport = IsImport || databaseProjectItem.IsImport;

            References.AddRange(databaseProjectItem.References);
        }

        [NotNull]
        protected DatabaseProjectItem AddSourceTextNode([NotNull] ITextNode textNode)
        {
            _sourceTextNodes.Remove(textNode);
            _sourceTextNodes.Insert(0, textNode);

            return this;
        }

        [NotNull]
        protected DatabaseProjectItem AddAdditionalSourceTextNode([NotNull] ITextNode textNode)
        {
            if (!_sourceTextNodes.Contains(textNode))
            {
                _sourceTextNodes.Add(textNode);
            }

            return this;
        }
    }
}
