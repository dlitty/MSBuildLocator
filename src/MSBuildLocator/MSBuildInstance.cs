using System;

namespace Microsoft.Build.MSBuildLocator
{
    /// <summary>
    ///     Represents an installed instance of MSBuild.
    /// </summary>
    public class MSBuildInstance
    {
        /// <summary>
        ///     Full name of the instance; for example, Visual Studio instance with SKU name
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        ///     Path to the MSBuild associated with this instance.
        /// </summary>
        public string MSBuildPath { get; protected set; }

        /// <summary>
        ///     Version of this Instance
        /// </summary>
        public Version Version { get; protected set; }

        /// <summary>
        ///     Indicates how this instance was discovered.
        /// </summary>
        public DiscoveryType DiscoveryType { get; protected set; }
    }
}
