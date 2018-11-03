// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.Build.MSBuildLocator
{
    public static class MSBuildLocator
    {
        private static readonly string[] s_msBuildAssemblies =
        {
            "Microsoft.Build", "Microsoft.Build.Framework", "Microsoft.Build.Tasks.Core",
            "Microsoft.Build.Utilities.Core"
        };

        /// <summary>
        ///     Query for all Visual Studio instances.
        /// </summary>
        /// <remarks>
        ///     Only includes Visual Studio 2017 (v15.0) and higher.
        /// </remarks>
        /// <returns>Enumeration of all Visual Studio instances detected on the machine.</returns>
        public static IEnumerable<VisualStudioInstance> QueryVisualStudioInstances()
        {
            return QueryVisualStudioInstances(VisualStudioInstanceQueryOptions.Default);
        }

        /// <summary>
        ///     Query for Visual Studio instances matching the given options.
        /// </summary>
        /// <remarks>
        ///     Only includes Visual Studio 2017 (v15.0) and higher.
        /// </remarks>
        /// <param name="options">Query options for Visual Studio instances.</param>
        /// <returns>Enumeration of Visual Studio instances detected on the machine.</returns>
        public static IEnumerable<VisualStudioInstance> QueryVisualStudioInstances(
            VisualStudioInstanceQueryOptions options)
        {
            return GetVSInstances().Where(i => i.DiscoveryType.HasFlag(options.DiscoveryTypes));
        }

        /// <summary>
        ///     Discover instances of Visual Studio and register the first one. See <see cref="RegisterInstance" />.
        /// </summary>
        /// <returns>Instance of Visual Studio found and registered.</returns>
        public static MSBuildInstance RegisterDefaults()
        {
            MSBuildInstance instance = GetVSInstances().FirstOrDefault() as MSBuildInstance;
            if (instance == null)
            {
                instance = GetSdkInstances().FirstOrDefault();
            }
            RegisterInstance(instance);

            return instance;
        }

        /// <summary>
        ///    Finds all installed .NET Core SDK Instances
        /// </summary>
        /// <returns>The dot net sdk instances.</returns>
        public static IEnumerable<DotNetSdkInstance> GetDotNetSdkInstances()
        {
            return GetSdkInstances();
        }

        /// <summary>
        ///     Finds and returns all MSBuild instances - both Visual Studio (Windows only) and .NET Core SDK instances.
        /// </summary>
        /// <returns>The all MSBuild instances.</returns>
        public static IEnumerable<MSBuildInstance> GetAllMSBuildInstances()
        {
            var allInstances = new List<MSBuildInstance>();
            allInstances.AddRange(GetVSInstances());
            allInstances.AddRange(GetSdkInstances());
            return allInstances;
        }

        /// <summary>
        ///     Add assembly resolution for Microsoft.Build core dlls in the current AppDomain from the specified
        ///     instance of a .NET SDK version.
        /// </summary>
        /// <param name="instance"></param>
        public static void RegisterInstance(MSBuildInstance instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            AppDomain.CurrentDomain.AssemblyResolve += (_, eventArgs) =>
            {
                var assemblyName = new AssemblyName(eventArgs.Name);
                if (s_msBuildAssemblies.Contains(assemblyName.Name, StringComparer.OrdinalIgnoreCase))
                {
                    var targetAssembly = Path.Combine(instance.MSBuildPath, assemblyName.Name + ".dll");
                    return File.Exists(targetAssembly) ? Assembly.LoadFrom(targetAssembly) : null;
                }

                return null;
            };
        }

        private static IEnumerable<VisualStudioInstance> GetVSInstances()
        {
            var devConsole = GetDevConsoleInstance();
            if (devConsole != null)
                yield return devConsole;

            foreach (var instance in VisualStudioLocationHelper.GetInstances())
                yield return instance;
        }

        private static VisualStudioInstance GetDevConsoleInstance()
        {
            var path = Environment.GetEnvironmentVariable("VSINSTALLDIR");
            if (!string.IsNullOrEmpty(path))
            {
                var versionString = Environment.GetEnvironmentVariable("VSCMD_VER");
                Version version;
                Version.TryParse(versionString, out version);

                if (version == null)
                {
                    versionString = Environment.GetEnvironmentVariable("VisualStudioVersion");
                    Version.TryParse(versionString, out version);
                }

                return new VisualStudioInstance("DEVCONSOLE", path, version, DiscoveryType.DeveloperConsole);
            }

            return null;
        }

        private static IEnumerable<DotNetSdkInstance> GetSdkInstances()
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var paths = Environment.GetEnvironmentVariable("PATH");
            var sep = (isWindows ? ';' : ':');
            var cliExe = (isWindows ? "dotnet.exe" : "dotnet");

            string basePath = null;
            foreach (var path in paths.Split(sep))
            {
                if (File.Exists(Path.Combine(path, cliExe)))
                {
                    basePath = path;
                    break;
                }
            }

            if (String.IsNullOrEmpty(basePath))
            {
                throw new ApplicationException("Unable to determine base directory for .NET Core SDKs");
            }

            var sdksDir = Path.Combine(basePath, "sdk");

            var instances = new List<DotNetSdkInstance>();

            foreach (var versionDir in Directory.GetDirectories(sdksDir))
            {
                if (File.Exists(Path.Combine(versionDir, "MSBuild.dll")))
                {
                    var versionString = Path.GetFileName(versionDir);
                    Version version;
                    if (Version.TryParse(versionString, out version))
                    {
                        instances.Add(new DotNetSdkInstance(version, versionDir, DiscoveryType.Other));
                    }
                }
            }

            instances.Sort((lhs, rhs) => rhs.Version.CompareTo(lhs.Version));

            return instances;
        }
    }
}