﻿// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Framework.ConfigurationModel;
using Sitecore.Pathfinder.Diagnostics;
using Sitecore.Pathfinder.Extensions;
using Sitecore.Pathfinder.IO;

namespace Sitecore.Pathfinder.Extensibility
{
    public static class StartupExtensions
    {
        [Flags]
        public enum CompositionOptions
        {
            None = 0x0,

            AddWebsiteAssemblyResolver = 0x01,

            DisableExtensions = 0x2
        }

        [CanBeNull]
        public static CompositionContainer RegisterCompositionService([NotNull] this Startup startup, [NotNull] IConfiguration configuration, [NotNull] string projectDirectory, [NotNull] Assembly callingAssembly, [NotNull, ItemNotNull] IEnumerable<string> additionalAssemblyFileNames, CompositionOptions options)
        {
            var toolsDirectory = configuration.GetString(Constants.Configuration.ToolsDirectory);

            if (options.HasFlag(CompositionOptions.AddWebsiteAssemblyResolver))
            {
                // add an assembly resolver that points to the website/bin directory - this will load files like Sitecore.Kernel.dll
                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => ResolveAssembly(args, configuration);
            }

            // add application assemblies
            var coreAssembly = typeof(Constants).Assembly;

            var catalogs = new List<ComposablePartCatalog>
            {
                new AssemblyCatalog(coreAssembly),
                new AssemblyCatalog(callingAssembly)
            };

            var disableExtensions = configuration.GetBool("disable-extensions");
            if (!disableExtensions && !options.HasFlag(CompositionOptions.DisableExtensions))
            {
                // add assemblies from the tools directory
                AddFeatureAssemblies(catalogs, toolsDirectory);

                // add additional assemblies - this is used in Sitecore.Pathfinder.Server to load assemblies from the /bin folder
                AddAdditionalAssemblies(catalogs, additionalAssemblyFileNames);

                // add core extensions
                var coreExtensionsDirectory = Path.Combine(toolsDirectory, "files\\extensions");
                var coreAssemblyFileName = Path.Combine(coreExtensionsDirectory, "Sitecore.Pathfinder.Core.Extensions.dll");
                AddDynamicAssembly(catalogs, toolsDirectory, coreAssemblyFileName, coreExtensionsDirectory);
                AddAssembliesFromDirectory(catalogs, coreExtensionsDirectory);

                // add extension from [Project]/node_modules directory
                AddNodeModules(catalogs, coreAssemblyFileName, projectDirectory);

                // add projects extensions
                var projectExtensionsDirectory = PathHelper.Combine(projectDirectory, configuration.GetString(Constants.Configuration.ProjectExtensionsDirectory));
                var projectAssemblyFileName = Path.Combine(projectExtensionsDirectory, configuration.GetString(Constants.Configuration.ProjectExtensionsAssemblyFileName));
                AddDynamicAssembly(catalogs, toolsDirectory, projectAssemblyFileName, projectExtensionsDirectory);
                AddAssembliesFromDirectory(catalogs, projectExtensionsDirectory);
            }

            // build composition graph
            var exportProvider = new CatalogExportProvider(new AggregateCatalog(catalogs));
            var compositionContainer = new CompositionContainer(exportProvider);
            exportProvider.SourceProvider = compositionContainer;

            // register the composition service itself for DI
            compositionContainer.ComposeExportedValue<ICompositionService>(compositionContainer);
            compositionContainer.ComposeExportedValue(configuration);

            return compositionContainer;
        }

        private static void AddAdditionalAssemblies([NotNull, ItemNotNull] ICollection<ComposablePartCatalog> catalogs, [NotNull, ItemNotNull] IEnumerable<string> additionalAssemblyFileNames)
        {
            foreach (var additionalAssemblyFileName in additionalAssemblyFileNames)
            {
                AddAssembly(catalogs, additionalAssemblyFileName);
            }
        }

        private static void AddAssembliesFromDirectory([NotNull, ItemNotNull] ICollection<ComposablePartCatalog> catalogs, [NotNull] string directory)
        {
            if (!Directory.Exists(directory))
            {
                return;
            }

            // only load Sitecore.Pathfinder.*.dll assemblies for performance
            foreach (var assemblyFileName in Directory.GetFiles(directory, "Sitecore.Pathfinder.*.dll", SearchOption.AllDirectories))
            {
                AddAssembly(catalogs, assemblyFileName);
            }
        }

        private static void AddAssembly([NotNull, ItemNotNull] ICollection<ComposablePartCatalog> catalogs, [NotNull] string assemblyFileName)
        {
            var fileName = Path.GetFileName(assemblyFileName);

            if (catalogs.OfType<AssemblyCatalog>().Any(c => string.Equals(Path.GetFileName(c.Assembly.Location), fileName, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            try
            {
                catalogs.Add(new AssemblyCatalog(assemblyFileName));
            }
            catch (FileLoadException ex)
            {
                Console.WriteLine(Texts.Failed_to_load_assembly___0____1_, ex.Message, assemblyFileName);
            }
        }

        private static void AddDynamicAssembly([NotNull, ItemNotNull] ICollection<ComposablePartCatalog> catalogs, [NotNull] string toolsDirectory, [NotNull] string assemblyFileName, [NotNull] string directory)
        {
            if (!Directory.Exists(directory))
            {
                return;
            }

            var compilerService = new CsharpCompilerService();

            var fileNames = Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories);
            if (!fileNames.Any())
            {
                return;
            }

            var assembly = compilerService.Compile(toolsDirectory, assemblyFileName, fileNames);
            if (assembly != null)
            {
                AddAssembly(catalogs, assemblyFileName);
            }
        }

        private static void AddFeatureAssemblies([NotNull, ItemNotNull] ICollection<ComposablePartCatalog> catalogs, [NotNull] string toolsDirectory)
        {
            // only load Sitecore.Pathfinder.*.dll assemblies for performance
            foreach (var assemblyFileName in Directory.GetFiles(toolsDirectory, "Sitecore.Pathfinder.*.dll", SearchOption.TopDirectoryOnly))
            {
                AddAssembly(catalogs, assemblyFileName);
            }
        }

        private static void AddNodeModules([NotNull, ItemNotNull] ICollection<ComposablePartCatalog> catalogs, [NotNull] string coreAssemblyFileName, [NotNull] string projectDirectory)
        {
            var nodeModules = Path.Combine(projectDirectory, "node_modules");
            if (!Directory.Exists(nodeModules))
            {
                return;
            }

            foreach (var directory in Directory.GetDirectories(nodeModules))
            {
                var manifest = Path.Combine(directory, "pathfinder.json");
                if (!File.Exists(manifest))
                {
                    continue;
                }

                // todo: exclude nested node_modules directories

                AddDynamicAssembly(catalogs, directory, coreAssemblyFileName, directory);
                AddAssembliesFromDirectory(catalogs, directory);
            }
        }

        [CanBeNull]
        private static Assembly ResolveAssembly([NotNull] ResolveEventArgs args, [NotNull] IConfiguration configuration)
        {
            var websiteDirectory = configuration.GetString(Constants.Configuration.WebsiteDirectory);

            var fileName = args.Name;
            var n = fileName.IndexOf(',');
            if (n >= 0)
            {
                fileName = fileName.Left(n).Trim();
            }

            if (!fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".dll";
            }

            fileName = Path.Combine(websiteDirectory, "bin\\" + fileName);
            return File.Exists(fileName) ? Assembly.LoadFrom(fileName) : null;
        }
    }
}
