namespace NetDaemon.Common.Reactive.States
{
    public sealed class MediaPlayerState : OnOffState
    {
        public MediaPlayerState(string? state) : base(state)
        {
        }

        public bool Playing => State is "playing";
        public bool Idle => State is "idle";

        public bool Paused => State is "paused";

        public bool On => State is "on" || Playing;
        public bool Off => State is "off" || Idle || Paused;
    }
}