namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>What a block-reaction PLM does between draws.</summary>
internal enum BlockReactionPlmCueKind
{
    /// <summary>Queues a library-2 sound, at most one pending.</summary>
    QueueSoundLibrary2Maximum1,
    /// <summary>Queues a library-2 sound, at most three pending.</summary>
    QueueSoundLibrary2Maximum3,
    /// <summary>Queues a library-2 sound, at most six pending.</summary>
    QueueSoundLibrary2Maximum6,
    /// <summary>Sets the block's BTS to one, as a broken grapple block leaves it.</summary>
    SetBtsToOne,
}
