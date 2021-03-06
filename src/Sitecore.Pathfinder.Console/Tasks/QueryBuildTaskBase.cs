// � 2015 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Pathfinder.Diagnostics;
using Sitecore.Pathfinder.Projects.References;
using Sitecore.Pathfinder.Snapshots;
using Sitecore.Pathfinder.Tasks.Building;

namespace Sitecore.Pathfinder.Tasks
{
    public abstract class QueryBuildTaskBase : BuildTaskBase
    {
        protected QueryBuildTaskBase([NotNull] string taskName) : base(taskName)
        {
        }

        protected virtual void Display([NotNull] IBuildContext context, [NotNull, ItemNotNull] IEnumerable<IReference> references)
        {
            foreach (var reference in references)
            {
                string line = $"{reference.Owner.Snapshot.SourceFile.ProjectFileName}";

                var textNode = reference.TextNode;
                line += $"({textNode.TextSpan.LineNumber},{textNode.TextSpan.LineNumber})";

                context.Trace.WriteLine(line);
            }
        }
    }
}
