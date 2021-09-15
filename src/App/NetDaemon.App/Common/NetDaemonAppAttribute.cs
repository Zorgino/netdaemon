using System;
using System.Collections.Generic;

namespace NetDaemon.Common
{
    /// <summary>
    /// Marks a class as a NetDaemonApp
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class NetDaemonAppAttribute : Attribute
    {
        public NetDaemonAppAttribute()
        {
        }

        public NetDaemonAppAttribute(string? id = null, params Type[]? dependencies)
        {
            Id = id;
            Dependencies = dependencies;
        }

        /// <summary>
        /// Id of an app
        /// </summary>
        public string? Id { get; init; }

        public Type[]? Dependencies { get; init; }
    }
}