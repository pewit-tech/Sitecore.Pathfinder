﻿// © 2015 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Pathfinder.Diagnostics;
using Sitecore.Pathfinder.Projects;
using Sitecore.Pathfinder.Projects.References;
using Sitecore.Pathfinder.Snapshots;
using Sitecore.Pathfinder.Text;

namespace Sitecore.Pathfinder.Parsing.Items.TreeNodeParsers
{
    public abstract class TextNodeParserBase : ITextNodeParser
    {
        protected TextNodeParserBase(double priority)
        {
            Priority = priority;
        }

        public double Priority { get; }

        public abstract bool CanParse(ItemParseContext context, ITextNode textNode);

        public abstract void Parse(ItemParseContext context, ITextNode textNode);

        [NotNull]
        protected virtual ITextNode GetItemNameTextNode([NotNull] IParseContext context, [NotNull] ITextNode textNode, [NotNull] string attributeName = "Name")
        {
            return textNode.GetAttributeTextNode(attributeName) ?? new FileNameTextNode(context.ItemName, textNode.Snapshot);
        }

        [CanBeNull]
        protected virtual IReference ParseReference([NotNull] ItemParseContext context, [NotNull] IProjectItem projectItem, [NotNull] ITextNode source, [NotNull] string text)
        {
            if (text.StartsWith("/sitecore/", StringComparison.OrdinalIgnoreCase))
            {
                var sourceProperty = new SourceProperty<string>(source.Name, string.Empty, SourcePropertyFlags.IsQualified);
                sourceProperty.SetValue(source);
                return context.ParseContext.Factory.Reference(projectItem, sourceProperty);
            }

            Guid guid;
            if (Guid.TryParse(text, out guid))
            {
                var sourceProperty = new SourceProperty<string>(source.Name, string.Empty, SourcePropertyFlags.IsGuid);
                sourceProperty.SetValue(source);
                return context.ParseContext.Factory.Reference(projectItem, sourceProperty);
            }

            if (text.StartsWith("{") && text.EndsWith("}"))
            {
                var sourceProperty = new SourceProperty<string>(source.Name, string.Empty, SourcePropertyFlags.IsSoftGuid);
                sourceProperty.SetValue(source);
                return context.ParseContext.Factory.Reference(projectItem, sourceProperty);
            }

            return null;
        }

        [NotNull]
        [ItemNotNull]
        protected virtual IEnumerable<IReference> ParseReferences<T>([NotNull] ItemParseContext context, [NotNull] IProjectItem projectItem, [NotNull] SourceProperty<T> sourceProperty)
        {
            var sourceTextNode = sourceProperty.SourceTextNode;
            if (sourceTextNode == null)
            {
                return Enumerable.Empty<IReference>();
            }

            return ParseReferences(context, projectItem, sourceTextNode);
        }

        [NotNull]
        [ItemNotNull]
        protected virtual IEnumerable<IReference> ParseReferences([NotNull] ItemParseContext context, [NotNull] IProjectItem projectItem, [NotNull] ITextNode textNode)
        {
            var text = textNode.Value;

            var reference = ParseReference(context, projectItem, textNode, text);
            if (reference != null)
            {
                yield return reference;
                yield break;
            }

            if (text.IndexOf('|') >= 0)
            {
                var parts = text.Split(Constants.Pipe, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    reference = ParseReference(context, projectItem, textNode, part);
                    if (reference != null)
                    {
                        yield return reference;
                    }
                }

                yield break;
            }

            if (text.IndexOf('&') < 0 && text.IndexOf('=') < 0)
            {
                yield break;
            }

            var urlString = new UrlString(text);

            foreach (string key in urlString.Parameters)
            {
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                var parameterValue = urlString.Parameters[key];

                reference = ParseReference(context, projectItem, textNode, parameterValue);
                if (reference != null)
                {
                    yield return reference;
                }
            }
        }
    }
}