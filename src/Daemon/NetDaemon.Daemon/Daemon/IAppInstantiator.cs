using System;
using System.Collections.Generic;
using NetDaemon.Common;

namespace NetDaemon.Daemon
{
    public interface IAppInstantiator
    {
        ApplicationContext Instantiate(Type netDaemonAppType, string appId, IEnumerable<Type>? dependencies = null);
    }
}