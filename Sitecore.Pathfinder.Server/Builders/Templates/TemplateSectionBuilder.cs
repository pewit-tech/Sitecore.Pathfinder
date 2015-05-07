namespace Sitecore.Pathfinder.Builders.Templates
{
  using System.Collections.Generic;
  using System.Linq;
  using Sitecore.Data.Items;
  using Sitecore.Pathfinder.Emitters;
  using Sitecore.Pathfinder.Projects.Templates;

  public class TemplateSectionBuilder
  {
    [CanBeNull]
    private IEnumerable<TemplateFieldBuilder> fieldBuilders;

    public TemplateSectionBuilder([NotNull] TemplateSectionModel templateSectionModel)
    {
      this.TemplateSectionModel = templateSectionModel;
    }

    [NotNull]
    public IEnumerable<TemplateFieldBuilder> Fields
    {
      get
      {
        return this.fieldBuilders ?? (this.fieldBuilders = this.TemplateSectionModel.Fields.Select(f => new TemplateFieldBuilder(f)).ToList());
      }
    }

    [CanBeNull]
    public Item Item { get; set; }

    [NotNull]
    public TemplateSectionModel TemplateSectionModel { get; }

    public void ResolveItem([NotNull] IEmitContext context, [CanBeNull] Item templateItem)
    {
      if (this.Item == null && templateItem != null)
      {
        this.Item = templateItem.Children[this.TemplateSectionModel.Name];
      }

      if (this.Item == null)
      {
        return;
      }

      foreach (var field in this.Fields)
      {
        field.ResolveItem(context, this.Item);
      }
    }
  }
}
