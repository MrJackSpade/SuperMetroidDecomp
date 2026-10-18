namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>What a scripted room PLM does between draws.</summary>
internal enum ScriptedRoomPlmCueKind
{
    /// <summary>Queues a library-2 sound, at most three pending.</summary>
    QueueSoundLibrary2Maximum3,
    /// <summary>Queues a library-2 sound, at most six pending.</summary>
    QueueSoundLibrary2Maximum6,
    /// <summary>Installs a pre-instruction the slot runs before its timer on later passes.</summary>
    InstallPreInstruction,
    /// <summary>Restores the inert pre-instruction.</summary>
    ClearPreInstruction,
    /// <summary>Sets the animals-escaped event.</summary>
    SetAnimalsEscapedEvent,
    /// <summary>Moves the PLM four blocks right.</summary>
    MoveRightFourBlocks,
    /// <summary>Continues at the lowered-acid hand program once the Lower Norfair acid is lowered.</summary>
    ContinueIfLowerNorfairAcidLowered,
    /// <summary>Moves the Lower Norfair acid to its lowered height.</summary>
    SetLoweredAcidHeight,
    /// <summary>Turns the Wrecked Ship Chozo spikes into slopes.</summary>
    TransformSpikesToSlopes,
    /// <summary>Turns the Wrecked Ship Chozo slopes back into spikes.</summary>
    RevertSlopesToSpikes,
}
