using System;
using System.IO;

namespace Microsoft.Build.MSBuildLocator
{
    /// <summary>
    /// Represents an installed .NET SDK instance (on any OS platform)
    /// </summary>
    public class DotNetSdkInstance : MSBuildInstance
    {
        public DotNetSdkInstance(Version version, string path, DiscoveryType discoveryType)
        {
            Name = ".NET Core SDK";
            Version = version;
            DiscoveryType = discoveryType;
            MSBuildPath = path;  // the DLLs, etc, are right in the root of the .NET SDK path
        }
    }
}
