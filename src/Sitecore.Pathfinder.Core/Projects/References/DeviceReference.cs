﻿// © 2015 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Pathfinder.Diagnostics;

namespace Sitecore.Pathfinder.Projects.References
{
    public class DeviceReference : Reference
    {
        public DeviceReference([NotNull] IProjectItem owner, [NotNull] SourceProperty<string> sourceProperty, [NotNull] string referenceText, [NotNull] string databaseName) : base(owner, sourceProperty, referenceText, databaseName)
        {
        }

        public override IProjectItem Resolve()
        {
            return Owner;
        }
    }
}
