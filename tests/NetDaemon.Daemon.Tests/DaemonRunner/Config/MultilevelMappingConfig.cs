namespace NetDaemon.Daemon.Tests.DaemonRunner.Config
{
    public class MultilevelMappingConfig : NetDaemon.Common.Reactive.NetDaemonRxApp
    {
        public Node? Root { get; set; }
    }

    public class Node
    {
        public string? Data { get; set; }

        public Node? Child { get; set; }
    }
}