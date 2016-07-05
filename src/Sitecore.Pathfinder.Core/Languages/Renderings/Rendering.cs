// � 2015 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Pathfinder.Diagnostics;
using Sitecore.Pathfinder.Languages.Content;
using Sitecore.Pathfinder.Projects;
using Sitecore.Pathfinder.Snapshots;

namespace Sitecore.Pathfinder.Languages.Renderings
{
    public class Rendering : ContentFile
    {
        public Rendering([NotNull] IProjectBase project, [NotNull] ISnapshot snapshot, [NotNull] string databaseName, [NotNull] string itemPath, [NotNull] string itemName, [NotNull] string filePath, [NotNull] string templateIdOrPath) : base(project, snapshot, filePath)
        {
            DatabaseName = databaseName;
            ItemPath = itemPath;
            ItemName = itemName;
            TemplateIdOrPath = templateIdOrPath;

            Placeholders = new LockableList<string>(this);
        }

        [NotNull]
        public string DatabaseName { get; }

        [NotNull]
        public string ItemName { get; }

        [NotNull]
        public string ItemPath { get; }

        [NotNull, ItemNotNull]
        public ICollection<string> Placeholders { get; }

        [NotNull]
        public ProjectItemUri RenderingItemUri { get; set; } = ProjectItemUri.Empty;

        [NotNull]
        public string TemplateIdOrPath { get; }
    }
}
