﻿// © 2015 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.IO;
using Sitecore.Pathfinder.Emitting;
using Sitecore.Pathfinder.Languages.Content;
using Sitecore.Pathfinder.Projects;

namespace Sitecore.Pathfinder.Emitters.Files
{
    public class ContentFileEmitter : EmitterBase
    {
        public ContentFileEmitter() : base(Constants.Emitters.BinFiles)
        {
        }

        public override bool CanEmit(IEmitContext context, IProjectItem projectItem)
        {
            return projectItem is ContentFile;
        }

        public override void Emit(IEmitContext context, IProjectItem projectItem)
        {
            var contentFile = (ContentFile)projectItem;

            var destinationFileName = FileUtil.MapPath(contentFile.FilePath);

            context.FileSystem.CreateDirectoryFromFileName(destinationFileName);
            context.FileSystem.Copy(projectItem.Snapshot.SourceFile.AbsoluteFileName, destinationFileName, context.ForceUpdate);
        }
    }
}
