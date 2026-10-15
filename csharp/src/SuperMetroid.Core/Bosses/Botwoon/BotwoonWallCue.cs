namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Gameplay cues of Botwoon's wall programs.</summary>
internal enum BotwoonWallCue
{
    /// <summary><c>$84:AB51</c>: sets the room's first two scroll cells blue.</summary>
    SetScrollsBlue,
    /// <summary>Queues the row's crumble sound, library two sound ten, at most six queued.</summary>
    QueueCrumbleSound,
    /// <summary><c>$84:AB59</c>: moves the PLM's origin down one block, to the next row.</summary>
    MoveDownOneBlock,
}
