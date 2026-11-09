namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>What a block-reaction PLM does between draws.</summary>
internal enum BlockReactionObjectCueKind
{
    /// <summary>Queues the cue's sound at <see cref="SoundRequestPriority.WhenIdle"/>.</summary>
    QueueSoundWhenIdle,
    /// <summary>Queues the cue's sound at <see cref="SoundRequestPriority.Low"/>.</summary>
    QueueSoundAtLowPriority,
    /// <summary>Queues the cue's sound at <see cref="SoundRequestPriority.Normal"/>.</summary>
    QueueSoundAtNormalPriority,
    /// <summary>Restores the respawning breakable-grapple behaviour, as a broken grapple block leaves it.</summary>
    RestoreRespawningGrappleBehavior,
}
