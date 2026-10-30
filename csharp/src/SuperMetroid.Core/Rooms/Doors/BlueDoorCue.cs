namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What the blue caps' programs do besides drawing (#1296).</summary>
internal enum BlueDoorCue
{
    /// <summary>Queues the door-opening sound.</summary>
    QueueOpenSound,
    /// <summary>Queues the door-closing sound.</summary>
    QueueCloseSound,
    /// <summary>Restores the cap's shootable blue-door behaviour for its facing.</summary>
    RestoreBlueBehavior,
}
