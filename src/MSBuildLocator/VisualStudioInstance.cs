// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

namespace Microsoft.Build.MSBuildLocator
{
    /// <summary>
    ///     Represents an installed instance of Visual Studio.
    /// </summary>
    public class VisualStudioInstance : MSBuildInstance
    {
        internal VisualStudioInstance(string name, string path, Version version, DiscoveryType discoveryType)
        {
            Name = name;
            VisualStudioRootPath = path;
            Version = version;
            DiscoveryType = discoveryType;
            MSBuildPath = Path.Combine(VisualStudioRootPath, "MSBuild", "15.0", "Bin");
        }

        /// <summary>
        ///     Path to the Visual Studio installation
        /// </summary>
        public string VisualStudioRootPath { get; }
    }
}