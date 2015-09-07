﻿// © 2015 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using NUnit.Framework;
using Sitecore.Pathfinder.Projects.Items;
using Sitecore.Pathfinder.Projects.Templates;

namespace Sitecore.Pathfinder.Projects
{
    [TestFixture]
    public partial class ProjectTests
    {
        [Test]
        public void JsonTemplateTest()
        {
            var projectItem = Project.Items.FirstOrDefault(i => i.QualifiedName == "/sitecore/templates/Json-Template");
            Assert.IsNotNull(projectItem);

            var template = (Template)projectItem;

            Assert.AreEqual("Json-Template", template.ShortName);
            Assert.AreEqual("/sitecore/templates/System/Templates/Standard Template", template.BaseTemplates);
            Assert.AreEqual("Applications/16x16/about.png", template.Icon);
            Assert.AreEqual("Short Help.", template.ShortHelp);
            Assert.AreEqual("Long Help.", template.LongHelp);

            var standardValuesItem = Project.Items.FirstOrDefault(i => i.QualifiedName == "/sitecore/templates/Json-Template/__Standard Values") as Item;
            Assert.IsNotNull(standardValuesItem);
            Assert.AreEqual(template.StandardValuesItem, standardValuesItem);

            var templateSection = template.Sections.FirstOrDefault(s => s.SectionName == "Fields");
            Assert.IsNotNull(templateSection);
            Assert.IsNotNull("Fields", templateSection.SectionName);

            var templateField = templateSection.Fields.FirstOrDefault(f => f.FieldName == "Title");
            Assert.IsNotNull(templateField);
            Assert.IsNotNull("Title", templateField.FieldName);
            Assert.IsNotNull("Single-Line Text", templateField.Type);
            Assert.IsNotNull("Short Help.", templateField.ShortHelp);
            Assert.IsNotNull("Long Help.", templateField.LongHelp);
            Assert.IsTrue(templateField.Shared);
            Assert.IsFalse(templateField.Unversioned);
            Assert.AreEqual(100, templateField.SortOrder);
            Assert.AreEqual("/sitecore/content", templateField.Source);

            var field = standardValuesItem.Fields.FirstOrDefault(f => f.FieldName == "Text");
            Assert.IsNotNull(field);
            Assert.AreEqual("Hello World", field.Value);
                                         
            var renderings = standardValuesItem.Fields.FirstOrDefault(f => f.FieldName == "__Renderings");
            Assert.IsNotNull(renderings);
            Assert.AreEqual("~/layout/renderings/HelloWorld.html", renderings.Value);
            Assert.AreEqual("HtmlTemplate", renderings.ValueHint);
        }
    }
}