using NetDaemon.Common.Reactive;
namespace NetDaemon.Common
{
    public interface INetDaemonApp : INetDaemonRxApp, IApplicationMetadata, INetDaemonPersistantApp
    {
    }
}