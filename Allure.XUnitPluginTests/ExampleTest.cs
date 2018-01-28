using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.DependencyModel;
using Xunit;

namespace Allure.XUnitPluginTests
{
    public class ExampleTest
    {
        static readonly HashSet<string> platformAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "microsoft.visualstudio.testplatform.unittestframework.dll",
            "microsoft.visualstudio.testplatform.core.dll",
            "microsoft.visualstudio.testplatform.testexecutor.core.dll",
            "microsoft.visualstudio.testplatform.extensions.msappcontaineradapter.dll",
            "microsoft.visualstudio.testplatform.objectmodel.dll",
            "microsoft.visualstudio.testplatform.utilities.dll",
            "vstest.executionengine.appcontainer.exe",
            "vstest.executionengine.appcontainer.x86.exe",
            "xunit.execution.desktop.dll",
            "xunit.execution.dotnet.dll",
            "xunit.execution.win8.dll",
            "xunit.execution.universal.dll",
            "xunit.runner.utility.desktop.dll",
            "xunit.runner.utility.dotnet.dll",
            "xunit.runner.visualstudio.testadapter.dll",
            "xunit.runner.visualstudio.uwp.dll",
            "xunit.runner.visualstudio.win81.dll",
            "xunit.runner.visualstudio.wpa81.dll",
            "xunit.core.dll",
            "xunit.assert.dll",
            "xunit.dll"
        };

        [Fact]
        public void Test1()
        {

            var sourcePath = Directory.GetCurrentDirectory();
            var sources = Directory.GetFiles(sourcePath, "*.dll")
                .Where(file => !platformAssemblies.Contains(Path.GetFileName(file)))
                .ToList();
            // Combine all input libs and merge their contexts to find the potential reporters
            var result = new List<IRunnerReporter>();
            var dcjr = new DependencyContextJsonReader();
            var deps = sources.Select(Path.GetFullPath)
                .Select(s => s.Replace(".dll", ".deps.json"))
                .Where(File.Exists)
                .Select(f => new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText(f))))
                .Select(dcjr.Read);
            var ctx = deps.Aggregate(DependencyContext.Default,
                (context, dependencyContext) => context.Merge(dependencyContext));
            dcjr.Dispose();

            var depsAssms = ctx.GetRuntimeAssemblyNames(RuntimeEnvironment.GetRuntimeIdentifier())
                .ToList();

            // Make sure to also check assemblies within the directory of the sources
            var dllsInSources = sources.Select(Path.GetFullPath)
                .Select(Path.GetDirectoryName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .SelectMany(p => Directory.GetFiles(p, "*.dll").Select(f => Path.Combine(p, f)))
                .Select(f => new AssemblyName {Name = Path.GetFileNameWithoutExtension(f)})
                .ToList();

            foreach (var assemblyName in depsAssms.Concat(dllsInSources))
            {
                try
                {
                    var assembly = Assembly.Load(assemblyName);
                    foreach (var type in assembly.DefinedTypes)
                    {
#pragma warning disable CS0618
                        if (type == null || type.IsAbstract || type == typeof(DefaultRunnerReporter).GetTypeInfo() ||
                            type == typeof(DefaultRunnerReporterWithTypes).GetTypeInfo() ||
                            type.ImplementedInterfaces.All(i => i != typeof(IRunnerReporter)))
                            continue;
#pragma warning restore CS0618

                        var ctor = type.DeclaredConstructors.FirstOrDefault(c => c.GetParameters().Length == 0);
                        if (ctor == null)
                        {
                            Console.WriteLine(
                                $"Type {type.FullName} in assembly {assembly} appears to be a runner reporter, but does not have an empty constructor.");
                            continue;
                        }

                        result.Add((IRunnerReporter) ctor.Invoke(new object[0]));
                    }
                }
                catch (Exception exception)
                {
                    exception.ToString();
                    continue;
                }
            }


            Assert.True(false, "");
        }
    }
}