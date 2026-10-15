namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What a coloured cap's programs do besides drawing (#1296).</summary>
internal enum ColoredDoorCue
{
    /// <summary>Becomes a blue cap when this door was opened before.</summary>
    BranchIfOpened,
    /// <summary>Links <see cref="ColoredDoorProgram.Hit"/> for the shot wake.</summary>
    LinkHit,
    /// <summary>Installs the colour's shot-wake pre-instruction.</summary>
    ArmWake,
    /// <summary>Counts a hit; at the colour's threshold the door opens.</summary>
    CountHit,
    /// <summary>Queues the red cap's hit sound.</summary>
    QueueHitSound,
    /// <summary>Queues the door-opening sound.</summary>
    QueueOpenSound,
    /// <summary>Queues the door-closing sound.</summary>
    QueueCloseSound,
}
