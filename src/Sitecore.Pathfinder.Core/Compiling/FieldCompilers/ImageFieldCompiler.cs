﻿// © 2015 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Pathfinder.Extensions;
using Sitecore.Pathfinder.Projects.Items;

namespace Sitecore.Pathfinder.Compiling.FieldCompilers
{
    public class ImageFieldCompiler : FieldCompilerBase
    {
        public ImageFieldCompiler() : base(Constants.FieldCompilers.Normal)
        {
        }

        public override bool CanCompile(IFieldCompileContext context, Field field)
        {
            return string.Equals(field.TemplateField.Type, "image", StringComparison.OrdinalIgnoreCase);
        }

        public override string Compile(IFieldCompileContext context, Field field)
        {
            var qualifiedName = field.Value.Trim();
            if (string.IsNullOrEmpty(qualifiedName))
            {
                return string.Empty;
            }

            var item = field.Item.Project.FindQualifiedItem(qualifiedName);
            if (item == null)
            {
                context.Trace.TraceError(Texts.Image_reference_not_found, qualifiedName);
                return string.Empty;
            }

            return $"<image mediapath=\"\" alt=\"\" width=\"\" height=\"\" hspace=\"\" vspace=\"\" showineditor=\"\" usethumbnail=\"\" src=\"\" mediaid=\"{item.Uri.Guid.Format()}\" />";
        }
    }
}