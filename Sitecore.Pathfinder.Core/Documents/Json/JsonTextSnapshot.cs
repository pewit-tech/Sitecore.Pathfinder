﻿namespace Sitecore.Pathfinder.Documents.Json
{
  using System.Linq;
  using Newtonsoft.Json.Linq;
  using Sitecore.Pathfinder.Diagnostics;

  public class JsonTextSnapshot : TextSnapshot
  {
    private ITextNode root;

    public JsonTextSnapshot([NotNull] ISourceFile sourceFile, [NotNull] string contents) : base(sourceFile, contents)
    {
    }

    public override ITextNode Root
    {
      get
      {
        if (this.root == null)
        {
          JToken token;
          try
          {
            token = JToken.Parse(this.Contents);
          }
          catch
          {
            return TextNode.Empty;
          }

          var jobject = token as JObject;
          if (jobject != null)
          {
            var r = jobject.Properties().FirstOrDefault(p => p.Name != "$schema");
            if (r == null)
            {
              return TextNode.Empty;
            }

            var value = r.Value as JObject;
            if (value == null)
            {
              return TextNode.Empty;
            }

            this.root = this.Parse(r.Name, value, null);
          }

          var jarray = token as JArray;
          if (jarray != null)
          {
            this.root = new JsonTextNode(this, string.Empty, jarray);

            foreach (var o in jarray.OfType<JObject>())
            {
              this.Parse(string.Empty, o, this.root);
            }
          }
        }

        return this.root ?? TextNode.Empty;
      }
    }

    public override ITextNode GetNestedTextNode(ITextNode textNode, string name)
    {
      return textNode.ChildNodes.FirstOrDefault(n => n.Name == name);
    }

    [NotNull]
    protected virtual ITextNode Parse([NotNull] string name, [NotNull] JObject jobject, [CanBeNull] ITextNode parent)
    {
      var treeNode = new JsonTextNode(this, name, jobject, parent);
      parent?.ChildNodes.Add(treeNode);

      foreach (var property in jobject.Properties())
      {
        switch (property.Value.Type)
        {
          case JTokenType.Object:
            this.Parse(property.Name, property.Value.Value<JObject>(), treeNode);
            break;

          case JTokenType.Array:
            var array = property.Value.Value<JArray>();
            var arrayTreeNode = new JsonTextNode(this, property.Name, array, parent);

            foreach (var o in array.OfType<JObject>())
            {
              this.Parse(string.Empty, o, arrayTreeNode);
            }

            treeNode.ChildNodes.Add(arrayTreeNode);
            break;

          case JTokenType.Boolean:
          case JTokenType.Date:
          case JTokenType.Float:
          case JTokenType.Integer:
          case JTokenType.String:
            var propertyTreeNode = new JsonTextNode(this, property.Name, property, treeNode);
            treeNode.Attributes.Add(propertyTreeNode);
            break;
        }
      }

      return treeNode;
    }
  }
}
