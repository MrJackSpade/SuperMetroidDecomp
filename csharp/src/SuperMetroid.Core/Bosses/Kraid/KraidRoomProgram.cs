namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The Kraid room's ceiling, platform and spike PLM programs (#1296), in authored order.</summary>
internal enum KraidRoomProgram
{
    /// <summary>Crumbles a ceiling block, then runs on into setting it to background one.</summary>
    CrumbleCeilingIntoBackground1,
    /// <summary>Sets a ceiling block to background one (an unused header).</summary>
    SetCeilingToBackground1,
    /// <summary>Crumbles a ceiling block, then runs on into setting it to background two.</summary>
    CrumbleCeilingIntoBackground2,
    /// <summary>Sets a ceiling block to background two: the first platform variant.</summary>
    SetCeilingToBackground2,
    /// <summary>Crumbles a ceiling block, then runs on into setting it to background three.</summary>
    CrumbleCeilingIntoBackground3,
    /// <summary>Sets a ceiling block to background three: the second platform variant.</summary>
    SetCeilingToBackground3,
    /// <summary>Clears the defeated room's ceiling.</summary>
    ClearCeiling,
    /// <summary>Loads eleven spike passes, then runs on into the first pass.</summary>
    CrumbleSpikes,
    /// <summary>One pass: crumbles two spike columns, moving right after each, and repeats while passes remain.</summary>
    CrumbleSpikesPass,
    /// <summary>Clears the defeated room's spikes.</summary>
    ClearSpikes,
}
