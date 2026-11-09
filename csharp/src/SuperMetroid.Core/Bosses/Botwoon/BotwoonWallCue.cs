namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Gameplay cues of Botwoon's wall programs.</summary>
internal enum BotwoonWallCue
{
    /// <summary>Sets the room's first two scroll cells blue.</summary>
    SetScrollsBlue,
    /// <summary>Queues the row's crumble sound, at most six queued.</summary>
    QueueCrumbleSound,
    /// <summary>Moves the PLM's origin down one block, to the next row.</summary>
    MoveDownOneBlock,
}
